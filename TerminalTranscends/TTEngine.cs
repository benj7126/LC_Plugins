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
    public class TTEngine
    {
        public CommandKeeper keeper = new CommandKeeper();
        public Dictionary<Type, TTDataSet> dataSets = new Dictionary<Type, TTDataSet>();
        public Command executingCommand = null;
        
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
                if (typeof(TTCommand).IsAssignableFrom(type) && type != typeof(TTCommand) ||
                    typeof(TTMultiCommand).IsAssignableFrom(type) && type != typeof(TTMultiCommand))
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

                    TransitionCommand command = new TransitionCommand(RemovePunctuation(tmpState.Name), type);
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

        internal string Autocomplete(string input, int cycleIndex)
        {
            if (executingCommand != null)
            {
                TTMultiCommand MC = executingCommand as TTMultiCommand;
                if (MC != null)
                {
                    string[] responses = MC.GetAllowedResponses();

                    if (responses.Length == 0)
                        return "";

                    int cIdx = cycleIndex % (responses.Length + 1);

                    return cIdx == responses.Length ? "" : responses[cIdx];
                }

                return "";
            }

            string text = RemovePunctuation(input);
            string[] array = text.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            if (text.Length == 0 || (text.Length > 0 && text.Last() == ' '))
            {
                array = array.AddItem("").ToArray();
            }

            string lastArg = array[array.Length - 1];

            Command[] commands = keeper.GetActiveCommands();

            Array.Sort(commands, delegate (Command c1, Command c2) {
                return c1.Priority.CompareTo(c2.Priority);
            });

            List<string> outputs = new List<string>();

            foreach (Command command in commands)
            {
                if (array.Length == 1)
                {
                    outputs.Add(command.Name);
                    continue;
                }

                if (RemovePunctuation(command.Name) != array[0])
                    continue;

                // -1 cuz the 0'th element not an arg
                if (array.Length-1 > command.args.Length)
                    continue;

                int i = 0;
                bool skip = false;
                for (; i < array.Length - 2; i++)
                {
                    if (command.args[i].ConvertToType(array[i + 1]) == null && command.args[i].defaultValue == null)
                        skip = true;
                }

                if (!skip)
                    outputs.AddRange(command.args[i].GetPossibleValues());
            }

            List<string> semiMatch = new List<string>();

            if (lastArg.Length == 0)
            {
                semiMatch = outputs;
            }
            else
            {
                for (int i = 0; i < outputs.Count; i++)
                {
                    for (int argI = 0; argI < lastArg.Length; argI++)
                    {
                        string compare = RemovePunctuation(outputs[i]);

                        if (compare.Length < lastArg.Length)
                            break;

                        if (lastArg[argI] != compare[argI])
                            break;

                        if (argI == lastArg.Length - 1)
                        {
                            semiMatch.Add(outputs[i].Substring(argI + 1));
                        }
                    }
                }
            }

            string[] atocompleteOptions = keeper.curState.ModifyAutoComplete(semiMatch.ToArray());

            if (atocompleteOptions.Length == 0)
                return "";

            int cycleIndexP1 = cycleIndex % (atocompleteOptions.Length + 1);

            return cycleIndexP1 == atocompleteOptions.Length ? "" : atocompleteOptions[cycleIndexP1];
        }

        internal void TakeInput()
        {
            if (executingCommand != null)
            {
                if (!executingCommand.ExecuteCommand(new object[0]))
                    executingCommand = null;
                return;
            }

            if (keeper.curState.CanRun())
            {
                keeper.curState.Run();
                return;
            }

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
                TTerminal.WriteLine($"Command \"{TTerminal.GetInput()}\" is unknown.");
                return;
            }

            object[] convertedParams = new object[passedCommand.args.Length];

            Plugin.Log("c");

            for (int i = 0; i < passedCommand.args.Length; i++)
            {
                CParam param = passedCommand.args[i];
                string strParam = array[i + 1];

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

                Plugin.Log(i + " - " + converted);

                convertedParams[i] = converted;
            }

            Plugin.Log("d");


            if (passedCommand.ExecuteCommand(convertedParams))
            {
                executingCommand = passedCommand;
            }
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

        internal void SetState(TTState state)
        {
            keeper.curState = state;
            state.EnterState();
        }

        internal void EnterState(Type stateType, bool clean = false)
        {
            SetState((TTState)Activator.CreateInstance(stateType));

            StateHistory.AddState(keeper.curState, clean);
        }

        internal void LastState()
        {
            SetState(StateHistory.Back());
        }
    }

    public class CommandKeeper
    {
        internal TTState curState;
        internal List<Command> AllCommands = new List<Command>();

        internal List<Command> GlobalCommands = new List<Command>();
        internal List<Command> fakeGlovalCommands = new List<Command>();

        public Command[] GetLocalCommands()
        {
            List<Command> commands = curState.CMDList;
            return commands.ToArray();
        }
        public Command[] GetActiveCommands()
        {
            Command[] retCMD = GetLocalCommands();

            retCMD = retCMD.AddRangeToArray(GlobalCommands.ToArray());
            retCMD = retCMD.AddRangeToArray(fakeGlovalCommands.ToArray());
            
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
