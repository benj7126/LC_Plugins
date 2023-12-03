using BepInEx; using BepInEx.Logging; using HarmonyLib; using System;  namespace TTerminal {     [BepInPlugin(Plugin.PLUGIN_GUID, Plugin.PLUGIN_NAME, Plugin.PLUGIN_VERSION)]     internal class Plugin : BaseUnityPlugin     {         const string PLUGIN_GUID = "catNull.LC.TerminalTranscends";         const string PLUGIN_NAME = "TerminalTranscends";         const string PLUGIN_VERSION = "1.0.0";          private static ManualLogSource ThisLogger;

        public static void Log(object data)         {             ThisLogger.LogInfo(data);         }         public static void Warn(object data)         {             ThisLogger.LogWarning(data);         }          private void Awake()         {
            if (ES3.Load<string>("Version", "TTSave", "0.0.0") != PLUGIN_VERSION)
            {
                // ES3 clear all saves | new version migth mean new things 

                ES3.DeleteFile("TTSave");

                ES3.Save<string>("Version", PLUGIN_VERSION, "TTSave");
            }              ThisLogger = Logger;              //ES3.Save<bool>("HasUsedTerminal", false, "LCGeneralSaveData");              Harmony.CreateAndPatchAll(typeof(Patch.TerminalPach));              Log($"Plugin {Plugin.PLUGIN_GUID} is loaded!");         }     } }