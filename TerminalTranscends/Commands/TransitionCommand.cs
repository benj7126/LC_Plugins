using System;
using System.Collections.Generic;
using System.Text;
using TTerminal.Commands;

namespace TTerminal.Commands
{
    internal class TransitionCommand : InternalCommand
    {
        Type targetState;
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
