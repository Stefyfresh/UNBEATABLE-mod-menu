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
        public const string PLUGIN_VERSION = "1.1.0";
        internal static new ManualLogSource Logger;

        // Internal mod options
        public static ConfigEntry<bool> showOptionDescriptions;
        public static ConfigEntry<bool> sortOptions;
        public static ConfigEntry<bool> showVersionText;
        public static ConfigEntry<MenuSpacing> menuSpacing;
        public static ConfigEntry<bool> doUpdateCheck;


        public static ModMenu Instance { get; private set; }
        public static int NumLoadedMods => Instance.transform.GetComponents<BaseUnityPlugin>().Length;


        private void Awake()
        {
            Logger = base.Logger;
            Logger.LogInfo($"Plugin {PLUGIN_GUID} is loaded!");

            Instance = this;

            showOptionDescriptions = Config.Bind(
                "General",
                "ShowOptionDescriptions",
                true,
                "Shows a detailed description for any options that have a description."
            );

            sortOptions = Config.Bind(
                "General",
                "SortMenuOptions",
                true,
                "Sorts all the configuration options for each mod's section in alphabetical order."
            );

            showVersionText = Config.Bind(
                "General",
                "ShowVersionAndMods",
                true,
                "Shows the current game version along with the number of loaded mods on the main and arcade menu screens."
            );

            menuSpacing = Config.Bind(
                "General",
                "MenuSpacing",
                MenuSpacing.Normal,
                "Changes the spacing of the mod menu options."
            );
            menuSpacing.SettingChanged += (sender, args) =>
            {
                MenuBuilder.currentDefinition = menuSpacing.Definition;
                MenuBuilder.TryRebuildMenu(true);
            };

            doUpdateCheck = Config.Bind(
                "General",
                "DoUpdateCheck",
                true,
                "Checks GitHub for the most recent version of any mods that contain a GitHub URL in their attributes."
            );

            var harmony = new Harmony(PLUGIN_GUID);
            harmony.PatchAll();
        }

        private void Start()
        {
            ConfigManager.Init();
        }
    }
}