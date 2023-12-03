using System;
using System.Collections.Generic;
using System.Text;

namespace TTerminal.Commands
{
    internal abstract class InternalCommand : Command
    {
        private string name;
        public override string Name => name;

        public InternalCommand(string name)
        {
            this.name = name;
        }
        public override string GetNameAndParams()
        {
            return name;
        }

        internal override void HandleCommandSetup() { }
    }
}
