using BepInEx;
using DunGen;
using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TTerminal.Engine;
using TTerminal.TerminalCode;

namespace TTerminal.Patch
{
    internal class TerminalPach
    {
        public static TerminalNode InfLoopNode;
        public static TTEngine Engine;

        [HarmonyPatch(typeof(Terminal), "Awake")]
        [HarmonyPostfix]
        static void TAwakePostfix(Terminal __instance)
        {
            InfLoopNode = ScriptableObject.CreateInstance<TerminalNode>();
            InfLoopNode.acceptAnything = true;
            InfLoopNode.maxCharactersToType = 100;
            InfLoopNode.terminalOptions = new CompatibleNoun[] { new CompatibleNoun() };
            InfLoopNode.terminalOptions[0].result = InfLoopNode;

            TTerminal.Terminal = __instance;

            Engine = new TTEngine();
            Engine.Initialize();

            TTerminal.Engine = Engine;
            // __instance.screenText.textComponent.fontSize /= 2; // half font size - you cant see shit...
        }

        [HarmonyPatch(typeof(Terminal), "Update")]
        [HarmonyPostfix]
        static void TUpdatePostfix(Terminal __instance)
        {
            if (!__instance.terminalInUse)
                return;

            History.Update();
        }

        public static bool WritingText = false;
        [HarmonyPatch(typeof(Terminal), "TextChanged")]
        [HarmonyPrefix]
        static bool TTextChangedPrefix(Terminal __instance)
        {
            if (WritingText)
            {
                return false;
            }

            if (__instance.currentNode == null)
            {
                return true;
            }
            if (Traverse.Create(__instance).Field("modifyingText").GetValue<bool>())
            {
                return true;
            }

            Autocomplete.Reset(); // if the user ever writes anything

            return true;
        }

        [HarmonyPatch(typeof(Terminal), "PressESC")]
        [HarmonyPrefix]
        static bool TPressESCPrefix(Terminal __instance, ref InputAction.CallbackContext context)
        {
            if (!context.performed)
            {
                return false;
            }

            if (context.control.name == "tab") // make it do tab and not exit when tab is pressed
            {
                Autocomplete.TabPressed();
                return false;
            }

            return true;
        }

        [HarmonyPatch(typeof(Terminal), "BeginUsingTerminal")]
        [HarmonyPostfix]
        static void TBeginUsingTerminalPostfix(Terminal __instance)
        {
            // this shit dosent work at all...
            HUDManager.Instance.controlTipLines[0].text = "Quit terminal : [ESC]";
            HUDManager.Instance.controlTipLines[1].text = "Auto complete : [TAB]";
            HUDManager.Instance.controlTipLines[2].text = "Cycle commands : [UP/DOWN]";
        }

        [HarmonyPatch(typeof(Terminal), "LoadNewNode")]
        [HarmonyPrefix]
        static bool TLoadNewNodePrefix(Terminal __instance, ref TerminalNode node)
        {
            // if enter is ever pressed
            Autocomplete.Reset();
            History.Reset(); // dosent clear history, just cur idx

            if (node == InfLoopNode)
            {
                if (TTerminal.CharsWritten() == 0)
                    return false;

                History.AddCommand(); // in this loop skips first setup, so dosent add blank to history

                // TTerminalInterpreter.instance.TakeInput(__instance);
                TTerminal.input = TTerminal.GetInput();
                TTerminal.Write("\n");
                Engine.TakeInput();
                TTerminal.input = null;

                TTerminal.Display();

                return false;
            }

            // tell it on startup to use our loop note...
            __instance.currentNode = InfLoopNode;
            TTerminal.GoToState(typeof(Main), true);
            TTerminal.Display();

            return false;
        }
    }

    internal class Autocomplete
    {
        private static int charsAutocompleted = 0;
        private static int cycleIndex = 0;

        public static void Reset()
        {
            charsAutocompleted = 0;
            cycleIndex = 0;
        }

        public static void TabPressed()
        {
            string NonCompletedText = TTerminal.GetAllText();
            string input = TTerminal.GetInput();

            if (charsAutocompleted != 0)
            {
                NonCompletedText = NonCompletedText.Substring(0, NonCompletedText.Length - charsAutocompleted);
                input = input.Substring(0, input.Length - charsAutocompleted);
            } 

            string addString = TTerminal.Engine.Autocomplete(input, cycleIndex);
            
            // string addString = cycleIndex % 2 == 0 ? "TextTestLOL" : "Other..."; // TTerminalInterpreter.instance.TryCompleteCommandSegment(__instance, textAddedACIdx);
            
            cycleIndex++;

            TTerminal.Paste(NonCompletedText + addString);
            TTerminal.Display(TTerminal.CharsWritten() + addString.Length - charsAutocompleted);

            charsAutocompleted = addString.Length;

        }
    }

    internal class History
    {
        private static List<string> history = new List<string>();
        private static int historyIndex = -1;
        private static string origString = null;
        public static void Reset()
        {
            origString = null;
            historyIndex = -1;
        }
        public static void AddCommand()
        {
            history.Add(TTerminal.GetInput());
        }

        public static void Update()
        {
            int lastTextHistoryIdx = historyIndex;

            if (Keyboard.current.upArrowKey.wasPressedThisFrame)
            {
                historyIndex++;
                if (historyIndex > history.Count - 1)
                    historyIndex = history.Count - 1;
            }

            if (Keyboard.current.downArrowKey.wasPressedThisFrame)
            {
                historyIndex--;
                if (historyIndex < -1)
                    historyIndex = -1;
            }

            if (lastTextHistoryIdx != historyIndex)
            {
                string removed = TTerminal.GetInput();
                string baseText = TTerminal.GetAllText().Substring(0, TTerminal.GetAllText().Length - removed.Length);
                string textToAdd;

                if (lastTextHistoryIdx == -1)
                {
                    origString = removed;

                    textToAdd = history[history.Count - historyIndex - 1];
                }
                else if (historyIndex == -1)
                {
                    textToAdd = origString;
                }
                else
                {
                    textToAdd = history[history.Count - historyIndex - 1];
                }

                TTerminal.Paste(baseText + textToAdd);
                TTerminal.Display(textToAdd.Length);
            }
        }
    }
}