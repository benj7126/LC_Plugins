using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TTerminal.DataSets;

namespace TTerminal.Commands
{
    public abstract class TTMultiCommand : Command
    {
        private IEnumerator IE;
        private MethodInfo MI;

        public bool awaitingResult = false;
        private string response;
        public string Response => response;
        public bool Confirmed => RemovePunctuation(response) == "confirm";
        public override bool ExecuteCommand(object[] paramArr)
        {
            if (awaitingResult)
            {
                response = TTerminal.GetInput();
            }
            else
            {
                IE = (IEnumerator)MI.Invoke(this, paramArr);
                awaitingResult = true;
            }

            bool done = IE.MoveNext();

            awaitingResult = done;
            return done;
        }

        public virtual string[] GetAllowedResponses()
        {
            return new string[0];
        }

        public override string GetNameAndParams()
        {
            string NnP = Name;

            foreach (CParam param in args)
            {
                NnP += " [" + param.name + (param.defaultValue == null ? "*" : "") + "]";
            }

            return NnP.Substring(0, NnP.Length-1);
        }
        internal override void HandleCommandSetup()
        {
            MI = GetType().GetMethod("run", BindingFlags.Public | BindingFlags.Instance);
            
            if (MI == null || MI.ReturnType != typeof(IEnumerator))
            {
                Plugin.Warn($"The type {GetType()} dosent contain a 'public IEnumerator run(...) {{ ... yield break; }}' function.");
                return;
            }

            UsingDSAttribute MA = MI.GetCustomAttribute<UsingDSAttribute>();
            int ArgIdx = 0;

            ParameterInfo[] PI = MI.GetParameters();

            args = new CParam[PI.Length];
            for (int i = 0; i < PI.Length; i++)
            {
                ParameterInfo pi = PI[i];
                args[i] = new CParam();
                args[i].type = pi.GetType();
                args[i].name = pi.Name.Replace("_", " ");

                if (pi.HasDefaultValue)
                    args[i].defaultValue = pi.DefaultValue;

                if (MA == null)
                    continue;

                if (MA.DSArr.Length > ArgIdx)
                {
                    if (MA.DSArr[ArgIdx] == null)
                    {
                        ArgIdx++;
                        continue;
                    }

                    if (pi.ParameterType == ((TTDataSet)Activator.CreateInstance(MA.DSArr[ArgIdx])).BaseType)
                    {
                        args[i].TTDataSet = MA.DSArr[ArgIdx];
                        ArgIdx++;
                    }
                }
            }

            Setup();
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
    }
}
