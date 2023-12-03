using GameNetcodeStuff;
using System;
using System.Collections;
using System.Text;
using TTerminal.Commands;
using TTerminal.DataSets;

namespace TTerminal.TerminalCode
{
    public class BaseCommands
    {
        public class BackCommand : TTCommand
        {
            public override string Name => "Back";
            public override string Description => "Moves to previous state.";
            public void run()
            {
                TTerminal.Back();
            }
        }
        public class OneWordEcho : TTCommand
        {
            public override string Name => "Echo";
            public override string Description => "Repeats the first argument.";
            public void run(string ToEcho)
            {
                TTerminal.WriteLine(ToEcho);
            }
        }
        public class ConvoTestStuff : TTMultiCommand
        {
            public override string Name => "Converse";
            public override string Description => "Short conversation.";
            public IEnumerator run()
            {
                TTerminal.WriteLine("Wanna talk? [CONFIRM/DENY]");
                yield return null;
                if (!Confirmed)
                {
                    TTerminal.WriteLine("Well, fuck you too :middle_finger:");
                    yield break;
                }

                TTerminal.WriteLine("How nice, sadly i dont know what to talk about...");
                TTerminal.WriteLine("Do you????");
                yield return null;
                if (Response.ToLower().Contains("yes"))
                {
                    TTerminal.WriteLine("There was some sort of yes in there, so ig you can just talk and i will listen.");
                    yield break;
                }

                TTerminal.WriteLine("Didnt see a \"yes\", guess we just wont talk then...\n");
                yield break;
            }
        }
        public class CommandInfo : TTCommand
        {
            public override string Name => "Enquire";
            public override string Description => "Enquires about a command.";
            public override Type State => typeof(Main);

            [UsingDS(typeof(ActiveCommandsDS))]
            public void run(Command ACommand)
            {
                TTerminal.WriteLine("Enquirering about " + ACommand.Name + ": ");
                TTerminal.WriteLine("To run: \"" + ACommand.GetNameAndParams() + "\"");
                TTerminal.WriteLine(ACommand.Description);

                /*
                foreach (CParam p in ACommand.args)
                {
                    TTerminal.WriteLine(" -" + p.name + ": " + p.);
                }
                */
            }
        }
        public class PlayerInfo : TTCommand
        {
            public override string Name => "GetHP";
            public override string Description => "Gets hp of target player.";
            public override Type State => typeof(Main);

            [UsingDS(typeof(PlayerDS))]
            public void run(PlayerControllerB player)
            {
                TTerminal.WriteLine(player.playerUsername + " has " + player.health + " hp.");
            }
        }
    }
}
