using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using TTerminal.Commands;

namespace TTerminal.Commands
{
    internal class TransitionCommand : InternalCommand
    {
        Type targetState;
        public override string Description
        {
            get
            {
                return $"Move to the " + Name + " state.";
            }
        }
        public TransitionCommand(string name, Type targetState) : base(name)
        {
            this.targetState = targetState;
        }

        public override bool ExecuteCommand(object[] paramArr)
        {
            TTerminal.GoToState(targetState);
            return false;
        }
    }
}
