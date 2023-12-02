using HarmonyLib;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using TTerminal.Commands;
using TTerminal.DataSets;
using TTerminal.TerminalCode;
using UnityEngine.Analytics;

namespace TTerminal.Engine
{
    internal class TTEngine
    {
        CommandKeeper keeper = new CommandKeeper();
        Dictionary<Type, TTDataSet> dataSets = new Dictionary<Type, TTDataSet>();
        
        public void Initialize()
        {
            /*
            Assembly[] loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly assembly in loadedAssemblies)
            {
                if (assembly == Assembly.GetExecutingAssembly())
                    Plugin.Log("Equals: " + assembly);
                else
                    Plugin.Log("NEquals: " + assembly);
            }
            */

            Assembly assembly = Assembly.GetExecutingAssembly();

            Type[] types = assembly.GetTypes();

            // Loop through each type and filter for classes
            foreach (Type type in types)
            {
                if (typeof(TTCommand).IsAssignableFrom(type) && type != typeof(TTCommand))
                {
                    Command command = (Command)Activator.CreateInstance(type);
                    keeper.AllCommands.Add(command);

                    if (command.State == null)
                    {
                        keeper.GlobalCommands.Add(command);
                    }
                    else
                    {
                        TTState tmpParentState = (TTState)Activator.CreateInstance(command.State);
                        tmpParentState.CMDList.Add(command);
                    }
                }
                else if (typeof(TTState).IsAssignableFrom(type) && type != typeof(TTState))
                {
                    TTState tmpState = (TTState)Activator.CreateInstance(type);

                    if (tmpState.ParentState == null)
                        continue;

                    TransitionCommand command = new TransitionCommand(RemovePunctuation(tmpState.Name), tmpState.ParentState);
                    keeper.AllCommands.Add(command);

                    TTState tmpParentState = (TTState)Activator.CreateInstance(tmpState.ParentState);
                    tmpParentState.CMDList.Add(command);
                }
                else if (typeof(TTDataSet).IsAssignableFrom(type) && type != typeof(TTDataSet))
                {
                    dataSets.Add(type, (TTDataSet)Activator.CreateInstance(type));
                }
            }

            foreach (Command command in keeper.AllCommands)
                command.HandleCommandSetup();
        }
        public string Autocomplete(string input, int cycleIndex)
        {
            string text = RemovePunctuation(input);
            string[] array = text.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            array = array.Length == 0 ? new string[] { "" } : array;

            string lastArg = array.Length == 0 ? "" : array[array.Length - 1];

            Command[] commands = keeper.GetActiveCommands();

            Array.Sort(commands, delegate (Command c1, Command c2) {
                return c1.Priority.CompareTo(c2.Priority);
            });

            List<string> outputs = new List<string>();

            Plugin.Log("1");

            foreach (Command command in commands)
            {
                Plugin.Log("2");
                if (array.Length == 1)
                {
                    Plugin.Log("add " + command.Name);
                    outputs.Add(command.Name);
                    continue;
                }

                if (RemovePunctuation(command.Name) != array[0])
                    continue;

                Plugin.Log("3");
                if (array.Length > command.args.Length)
                    continue;

                int i = 1;
                for (i = 1; i < array.Length; i++)
                {
                    if (command.args[i].ConvertToType(array[i]) == null && command.args[i].defaultValue == null)
                        continue; // if theres a default value, it dosent have to match
                }

                Plugin.Log("4");
                outputs.AddRange(command.args[i].GetPossibleValues());
            }

            List<string> semiMatch = new List<string>();

            Plugin.Log("5");
            if (lastArg.Length == 0)
            {
                semiMatch = outputs;
            }
            else
            {
                for (int i = 0; i < outputs.Count; i++)
                {
                    Plugin.Log("5.5 - " + i);
                    for (int argI = 0; argI < lastArg.Length; argI++)
                    {
                        string compare = RemovePunctuation(outputs[i]);

                        Plugin.Log("6");
                        if (compare.Length < lastArg.Length)
                            break;

                        Plugin.Log(argI);
                        Plugin.Log(lastArg[argI]);
                        Plugin.Log(compare[argI]);
                        if (lastArg[argI] != compare[argI])
                            break;

                        Plugin.Log("7");
                        if (argI == lastArg.Length - 1)
                        {
                            semiMatch.Add(outputs[i].Substring(argI + 1));
                        }
                    }
                }
            }

            if (semiMatch.Count == 0)
                return "";

            int cycleIndexP1 = cycleIndex % (semiMatch.Count + 1);

            return cycleIndexP1 == semiMatch.Count ? "" : semiMatch[cycleIndexP1];
        }

        public void TakeInput()
        {
            string text = RemovePunctuation(TTerminal.GetInput());
            string[] array = text.Split(" ", StringSplitOptions.RemoveEmptyEntries);

            Command[] commands = keeper.GetActiveCommands();

            Array.Sort(commands, delegate (Command c1, Command c2) {
                return c1.Priority.CompareTo(c2.Priority);
            });

            Command passedCommand = null;

            foreach (Command command in commands)
            {
                if (RemovePunctuation(command.Name) == array[0])
                {
                    passedCommand = command;
                    break;
                }
            }

            if (passedCommand == null)
            {
                TTerminal.WriteLine($"Command {TTerminal.GetInput()} is unknown.");
                return;
            }

            object[] convertedParams = new object[passedCommand.args.Length];

            for (int i = 0; i < passedCommand.args.Length; i++)
            {
                CParam param = passedCommand.args[i];
                string strParam = array[i];

                object converted = param.ConvertToType(strParam);
                if (converted == null)
                {
                    if (param.defaultValue == null)
                    {
                        TTerminal.WriteLine($"Missing/un-convertable argument [{param.name}]");
                        return;
                    }

                    convertedParams[i] = param.defaultValue;
                    continue;
                }
                convertedParams[i] = converted;
            }

            passedCommand.ExecuteCommand(convertedParams);
        }

        private string RemovePunctuation(string s)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (char c in s)
            {
                if (!char.IsPunctuation(c))
                {
                    stringBuilder.Append(c);
                }
            }
            return stringBuilder.ToString().ToLower();
        }

        private void SetState(TTState state)
        {
            keeper.curState = state;
            state.EnterState();
        }

        public void EnterState(Type stateType, bool clean = false)
        {
            SetState((TTState)Activator.CreateInstance(stateType));

            StateHistory.AddState(keeper.curState, clean);
        }

        public void LastState()
        {
            SetState(StateHistory.Back());
        }
    }

    public class CommandKeeper
    {
        public TTState curState;
        public List<Command> AllCommands = new List<Command>();

        public List<Command> GlobalCommands = new List<Command>();
        public List<Command> fakeGlovalCommands = new List<Command>();

        public Command[] GetLocalCommands()
        {
            List<Command> commands = curState.CMDList;
            return commands.ToArray();
        }
        public Command[] GetActiveCommands()
        {
            Command[] retCMD = GetLocalCommands();
            
            retCMD.AddRangeToArray(GlobalCommands.ToArray());
            retCMD.AddRangeToArray(fakeGlovalCommands.ToArray());
            
            return retCMD;
        }
        public Command[] GetAllCommands()
        {
            return AllCommands.ToArray();
        }
    }

    internal class StateHistory
    {
        private static List<Type> history = new List<Type>();

        public static void AddState(TTState state, bool clean = false)
        {
            if (clean)
                history.Clear();

            history.Add(state.GetType());
        }

        public static TTState Back()
        {
            Type lastType = history.Last();
            if (history.Count != 1)
            {
                history.RemoveAt(history.Count - 1);
            }

            return (TTState)Activator.CreateInstance(lastType);
        }
    }
}
