using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TTerminal.DataSets;

namespace TTerminal.Commands
{
    public abstract class TTCommand : Command
    {
        public override bool ExecuteCommand(object[] paramArr)
        {
            return base.ExecuteCommand(paramArr);
        }
        internal override void HandleCommandSetup()
        {
            MethodInfo MI = GetType().GetMethod("run", BindingFlags.Public);

            if (MI == null)
            {
                Plugin.Warn($"The type {GetType()} dosent contain a 'public void run(...) {{}}' function.");
                return;
            }

            UsingDSAttribute MA = MI.GetCustomAttribute<UsingDSAttribute>();
            int ArgIdx = 0;

            Plugin.Log("Sigh 2");
            ParameterInfo[] PI = MI.GetParameters();

            Plugin.Log("Sigh 3");
            args = new CParam[PI.Length];
            for (int i = 0; i < PI.Length; i++)
            {
                ParameterInfo pi = PI[i];
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

            if (Setup != null)
                Setup();
        }
    }
}
