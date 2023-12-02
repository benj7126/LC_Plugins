using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using TTerminal.Commands;

namespace TTerminal
{
    public abstract class TTState
    {
        public static Dictionary<Type, List<Command>> AllCMDList = new Dictionary<Type, List<Command>>(); // commands are the same for all states

        public List<Command> CMDList
        {
            get
            {
                if (!AllCMDList.ContainsKey(GetType()))
                {
                    AllCMDList.Add(GetType(), new List<Command>());
                }

                return AllCMDList[GetType()];
            }
        }

        public abstract string Name { get; }
        public abstract Type ParentState { get; }

        public void EnterState()
        {
            if (ES3.Load<bool>("HasEntered" + Name, "TTSave", false))
            {
                Enter();
            }
            else
            {
                ES3.Save<bool>("HasEntered" + Name, true, "TTSave");

                Enter(true);
            }
        }

        protected virtual void Enter(bool first = false) { }
    }
}
