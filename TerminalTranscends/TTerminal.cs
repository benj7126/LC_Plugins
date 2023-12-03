using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using TTerminal.Engine;
using TTerminal.Patch;

namespace TTerminal
{
    public class TTerminal
    {
        public static Terminal Terminal;

        internal static string input = null;
        public static TTEngine Engine = null;
        
        public static string GetInput()
        {
            return input == null ? Terminal.currentText.Substring(Terminal.currentText.Length - Terminal.textAdded) : input;
        }
        public static string GetAllText()
        {
            return Terminal.currentText;
        }
        public static void Paste(string textToAdd)
        {
            Terminal.currentText = textToAdd;
        }
        public static void Clear()
        {
            Terminal.currentText = "";
        }
        public static void Write(string textToAdd)
        {
            Terminal.currentText += textToAdd;
        }
        public static void WriteLine(string textToAdd)
        {
            Terminal.currentText += textToAdd + "\n";
        }
        public static int CharsWritten()
        {
            return Terminal.textAdded;
        }
        public static void GoToState(Type State, bool clean = false) // if clean remove history
        {
            Engine.EnterState(State, clean);
        }
        public static void Back()
        {
            Engine.LastState();
        }
        public static void Display(int textAdded = 0)
        {
            TerminalPach.WritingText = true;

            Terminal.screenText.text = Terminal.currentText;
            Terminal.textAdded = textAdded;

            TerminalPach.WritingText = false;
        }
    }
}
