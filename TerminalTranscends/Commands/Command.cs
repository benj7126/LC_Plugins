using DunGen;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;

namespace TTerminal.Commands
{
    public abstract class Command
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        public virtual Type State => null;
        public virtual int Priority => 0;

        public CParam[] args = new CParam[0];

        public virtual void Setup() { }

        internal abstract void HandleCommandSetup();

        public virtual string GetNameAndParams()
        {
            return "[ An error has occured as you should not be able to see this... ]";
        }

        public virtual bool ExecuteCommand(object[] paramArr)
        {
            return false; // is it still on-going?
        }

        /* will probably make it differently...
        // Alias
        internal static List<string> aliases = new List<string>();
        internal void AddAlias(string alias)
        {
            aliases.Add(alias);
        }
        internal string[] GetNames()
        {
            string[] names = new string[aliases.Count + 1];
            names[0] = Name;
            for (int i = 1; i < names.Length; i++)
            {
                names[i] = aliases[i-1];
            }
            return names;
        }
        */
    }
    public class CParam
    {
        public Type type;
        public Type TTDataSet;
        public object defaultValue = null;

        public string name;

        public object ConvertToType(string toConvert)
        {
            if (TTDataSet != null)
            {
                Dictionary<string, object> converter = TTerminal.Engine.dataSets[TTDataSet].GetData();
                Plugin.Log(converter);

                foreach (KeyValuePair<string, object> kvp in converter)
                {
                    Plugin.Log("Compare: " + RemovePunctuation(kvp.Key) + " == " + toConvert + " -"+ (RemovePunctuation(kvp.Key) == toConvert));
                    if (RemovePunctuation(kvp.Key) == toConvert)
                        return kvp.Value;
                }

                return null;
            }
            else
            {
                object converted = null;

                try
                {
                    converted = Convert.ChangeType(toConvert, type);
                }
                catch
                {
                    return null;
                }

                return converted;
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
        public string[] GetPossibleValues()
        {
            if (TTDataSet != null)
                return TTerminal.Engine.dataSets[TTDataSet].GetData().Keys.ToArray();

            return new string[0];
        }
    }
}
