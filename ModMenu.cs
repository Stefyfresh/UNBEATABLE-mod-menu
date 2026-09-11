using BepInEx;
using BepInEx.Logging;
using System.Collections.Generic;
using HarmonyLib;
using Rhythm;
using DG.Tweening;
using UnityEngine;
using Arcade.UI.SongSelect;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Arcade.UI.Options;
using TMPro;
using UI;
using Arcade.UI.MenuStates;
using Arcade.UI.AnimationSystem;
using UnityEngine.UIElements;
using Arcade.UI;
using BepInEx.Configuration;

namespace ModMenu
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    [BepInProcess("UNBEATABLE.exe")]
    public class ModMenu : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "com.stefyfresh.ModMenu";
        public const string PLUGIN_NAME = "Mod Menu";
        public const string PLUGIN_VERSION = "0.3.0";
        internal static new ManualLogSource Logger;

        public static ModMenu Instance { get; private set; }

        // Internal mod options
        public static ConfigEntry<bool> fasterMenuTransitions;
        public static ConfigEntry<bool> showOptionDescriptions;


        private void Awake()
        {
            Logger = base.Logger;
            Logger.LogInfo($"Plugin {PLUGIN_GUID} is loaded!");

            Instance = this;

            fasterMenuTransitions = Config.Bind(
                "General",
                "FasterMenuTransitions",
                false,
                "Enables faster transitions in the arcade mode menu"
            );
            showOptionDescriptions = Config.Bind(
                "General",
                "ShowOptionDescriptions",
                true,
                "Shows a detailed description for all mod menu options, if available"
            );

            var harmony = new Harmony(PLUGIN_GUID);
            harmony.PatchAll();
        }

        private void Start()
        {
            ConfigFinder.Init();
        }
    }
}