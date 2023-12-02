using System;
using System.Collections.Generic;
using System.Text;
using TTerminal.Commands;

namespace TTerminal.TerminalCode
{
    public class Main : TTState
    {
        public override string Name => "Main";
        public override Type ParentState => null;

        protected override void Enter(bool first = false)
        {
            TTerminal.Clear();
            TTerminal.WriteLine("\n\nMain state...");
            TTerminal.WriteLine("DDDDDDD");
        }
    }
    public class TMP : TTState
    {
        public override string Name => "TMP";
        public override Type ParentState => typeof(Main);

        protected override void Enter(bool first = false)
        {
            TTerminal.Clear();
            TTerminal.WriteLine("\n\ntmp...");
        }
    }
    public class OneWordEcho : TTCommand
    {
        public override string Name => "Echo";
        public override Type State => typeof(Main);
    }
    public class PlayerInfo : TTCommand
    {
        public override string Name => "Enquire";
        public override Type State => typeof(Main);
    }
}
