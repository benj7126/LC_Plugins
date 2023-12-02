using System;
using System.Collections.Generic;
using System.Text;
using TTerminal.Patch;

namespace TTerminal
{
    public class TTerminal
    {
        public static Terminal terminal;
        public static string GetInput()
        {
            return terminal.currentText.Substring(terminal.currentText.Length - terminal.textAdded);
        }
        public static string GetAllText()
        {
            return terminal.currentText;
        }
        public static void Paste(string textToAdd)
        {
            terminal.currentText = textToAdd;
        }
        public static void Write(string textToAdd)
        {
            terminal.currentText += textToAdd;
        }
        public static void WriteLine(string textToAdd)
        {
            terminal.currentText += textToAdd + "\n";
        }
        public static int CharsWritten()
        {
            return terminal.textAdded;
        }
        public static void Display(int textAdded = 0)
        {
            TerminalPach.WritingText = true;

            terminal.screenText.text = terminal.currentText;
            terminal.textAdded = textAdded;

            TerminalPach.WritingText = false;
        }
    }
}
