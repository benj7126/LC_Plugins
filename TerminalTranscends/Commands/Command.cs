using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UI;

namespace TTerminal.Commands
{
    public abstract class Command
    {
        public abstract string Name { get; }
        public virtual Type State => null;
        public virtual int Priority => 0;

        public CParam[] args;

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
            object converted = Convert.ChangeType(toConvert, type);

            if (converted != null)
                return converted;

            return null;
        }
        public string[] GetPossibleValues()
        {
            return new string[] { "test" };
        }
    }
}
