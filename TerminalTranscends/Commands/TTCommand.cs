﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TTerminal.DataSets;

namespace TTerminal.Commands
{
    public abstract class TTCommand : Command
    {
        private MethodInfo MI;

        public override bool ExecuteCommand(object[] paramArr)
        {
            MI.Invoke(this, paramArr);
            return false;
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

            if (MI == null)
            {
                Plugin.Warn($"The type {GetType()} dosent contain a 'public void run(...) {{ ... }}' function.");
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

            if (Setup != null)
                Setup();
        }
    }
}
