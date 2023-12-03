using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Text;
using TTerminal.Commands;
using TTerminal.DataSets;

namespace TTerminal.TerminalCode
{
    public class Main : TTState
    {
        public override string Name => "Main";
        public override Type ParentState => null;

        protected override void Enter(bool first = false)
        {
            if (first)
            {
                stage = 0;
                FirstRun();
                return;
            }

            TTerminal.Clear();
            // make some sort of default main menu thingy...
        }

        private void FirstRun()
        {
            TTerminal.Clear();
            TTerminal.WriteLine("\n\n\nBG IG, A System-Act Ally");
            TTerminal.WriteLine("Copyright (C) 2084-2108, Halden Electronics Inc.");
            TTerminal.WriteLine("Courtesy of the Company.");
            TTerminal.WriteLine("\n\n\nBios for FORTUNE-9 87.7/10MHZ System.");
            TTerminal.WriteLine($"\nCurrent date is {DateTime.Now.DayOfWeek.ToString().Substring(0, 3)}  {DateTime.Now.Date.ToString().Substring(0, 10)}");
            TTerminal.WriteLine($"Current time is {DateTime.Now.TimeOfDay}");

            TTerminal.Write("\nPlease enter favorite animal:");
        }

        int stage = -1;

        public override string[] ModifyAutoComplete(string[] strings)
        {
            return stage == -1 ? base.ModifyAutoComplete(strings) : new string[0];
        }
        public override bool CanRun()
        {
            return stage != -1;
        }
        public override void Run()
        {
            stage++;

            switch (stage)
            {
                case 1:
                    TTerminal.Write("\n\nPlease describe your role in a team dynamic:");
                    break;
                case 2:
                    stage = -1;
                    TTerminal.Clear();
                    TTerminal.Write("\n");
                    TTerminal.WriteLine("__      _____ _    ___ ___  __  __ ___");
                    TTerminal.WriteLine("\\ \\    / | __| |  / __/   \\|  \\/  | __|");
                    TTerminal.WriteLine(" \\ \\/\\/ /| _|| |_| (_| (_) | |\\/| | _|");
                    TTerminal.WriteLine("  \\_/\\_/ |___|____\\___\\___/|_|  |_|___|");
                    TTerminal.WriteLine("\n\nWelcome to the FORTUNE-9 OS");
                    TTerminal.WriteLine("          Courtesy of the Company");
                    TTerminal.WriteLine("\nType \"CMD\" for a list of commands.");
                    break;
            }
        }
    }
}
