using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Text;
using TTerminal.Commands;
using UnityEngine;

namespace TTerminal.DataSets
{
    public class PlayerDS : TTDataSet
    {
        public override Type BaseType => typeof(PlayerControllerB);

        protected override Dictionary<string, object> GenerateData()
        {
            Dictionary<string, object> retVal = new Dictionary<string, object>();

            foreach (GameObject gameObject in StartOfRound.Instance.allPlayerObjects)
            {
                PlayerControllerB player = gameObject.GetComponent<PlayerControllerB>();

                retVal.Add(player.playerUsername, player);
            }

            return retVal;
        }
    }
    public class ActiveCommandsDS : TTDataSet
    {
        public override Type BaseType => typeof(Command);

        protected override Dictionary<string, object> GenerateData()
        {
            Dictionary<string, object> retVal = new Dictionary<string, object>();

            foreach (Command c in TTerminal.Engine.keeper.GetActiveCommands())
            {
                retVal.Add(c.Name, c);
            }

            return retVal;
        }
    }
}
