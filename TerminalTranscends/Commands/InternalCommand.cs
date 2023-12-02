using System;
using System.Collections.Generic;
using System.Text;

namespace TTerminal.Commands
{
    internal class InternalCommand : Command
    {
        private string name;
        public override string Name => name;

        public InternalCommand(string name)
        {
            this.name = name;
        }

        internal override void HandleCommandSetup() { }
    }
}
