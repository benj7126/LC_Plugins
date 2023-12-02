using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;

namespace TTerminal
{
    [BepInPlugin(Plugin.PLUGIN_GUID, Plugin.PLUGIN_NAME, Plugin.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        const string PLUGIN_GUID = "catNull.LC.TerminalTranscends";
        const string PLUGIN_NAME = "TerminalTranscends";
        const string PLUGIN_VERSION = "1.0.0";

        private static ManualLogSource ThisLogger;

        public static void Log(object data)
        {
            ThisLogger.LogInfo(data);
        }

        private void Awake()
        {
            ThisLogger = Logger;

            Harmony.CreateAndPatchAll(typeof(Patch.TerminalPach));

            Log($"Plugin {Plugin.PLUGIN_GUID} is loaded!");
        }
    }
}