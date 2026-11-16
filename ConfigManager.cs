using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using Newtonsoft.Json;
using UnityEngine;

namespace ModMenu
{
    public static class ConfigManager
    {
        public static readonly string pluginShowOverridePath = Path.Combine(Paths.ConfigPath, $"{ModMenu.PLUGIN_GUID}.pluginOverrides.json");
        public static Dictionary<string, bool> pluginShowOverrides;

        public static List<string> pluginGUIDs = [];
        public static Dictionary<string, ConfigFile> pluginConfigFiles = [];
        public static Dictionary<string, BepInPlugin> pluginMetadata = [];

        public static int NumConfigurableMods => pluginConfigFiles.Count(kvp => kvp.Value.Count > 0);
        public static int NumConfigOptions
        {
            get
            {
                int configs = 0;
                pluginConfigFiles.Values.ToList().ForEach(configFile => configs += configFile.Count);
                return configs;
            }
        }


        public static void Init()
        {
            try
            {
                if (Chainloader.PluginInfos.Values != null && Chainloader.PluginInfos.Values.Count() > 0)
                {
                    List<PluginInfo> sortedPlugins = Chainloader.PluginInfos.Values.Where(plugin => plugin != null).ToList();
                    sortedPlugins.Sort((a, b) => a.Metadata.Name.CompareTo(b.Metadata.Name));

                    foreach (PluginInfo plugin in sortedPlugins)
                    {
                        // Skip mod menu as we set the configs manually
                        if (plugin.Instance == ModMenu.Instance)
                        {
                            PluginUpdateManager.CheckForUpdates(plugin.Instance);
                            continue;
                        }

                        // Add all plugin information
                        pluginGUIDs.Add(plugin.Metadata.GUID);
                        pluginConfigFiles.Add(plugin.Metadata.GUID, plugin.Instance.Config);
                        pluginMetadata.Add(plugin.Metadata.GUID, plugin.Metadata);

                        // Check for plugin updates
                        PluginUpdateManager.CheckForUpdates(plugin.Instance);
                    }

                    LoadPluginShowOverrides();

                    ModMenu.Logger.LogInfo($"Gathered all config options for {sortedPlugins.Count()} plugins.");
                }
                else
                {
                    ModMenu.Logger.LogWarning("Could not find any plugins to gather config files for!");
                }
            }
            catch (System.Exception ex)
            {
                ModMenu.Logger.LogError($"Failed to gather config options! {ex.Message}");
            }
        }


        private static void LoadPluginShowOverrides()
        {
            try
            {
                // Load file
                if (File.Exists(pluginShowOverridePath))
                {
                    pluginShowOverrides = JsonConvert.DeserializeObject<Dictionary<string, bool>>(File.ReadAllText(pluginShowOverridePath));

                    // Set the active GUIDs
                    SetActiveGUIDs();
                }
                else
                {
                    ResetPluginShowOverrides();
                }

                SavePluginShowOverrides();
            }
            catch (System.Exception ex)
            {
                ModMenu.Logger.LogWarning($"Failed to load plugin override json data! {ex}");
                File.Delete(pluginShowOverridePath);
                ResetPluginShowOverrides();
            }
        }


        public static void SavePluginShowOverrides()
        {
            try
            {
                // Only save disabled plugins, unloaded plugins, or plugins with at least 1 config entry
                Dictionary<string, bool> toSave = pluginShowOverrides
                    .Where(kvp => !kvp.Value || !pluginConfigFiles.TryGetValue(kvp.Key, out ConfigFile file) || file.Count > 0)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                File.WriteAllText(pluginShowOverridePath, JsonConvert.SerializeObject(toSave, Formatting.Indented));
            }
            catch (System.Exception ex)
            {
                ModMenu.Logger.LogWarning($"Failed to save plugin override json data! {ex}");
                ResetPluginShowOverrides();
            }
        }


        public static void SetActiveGUIDs()
        {
            // Set active GUIDs to the sorted list of ones that are currently enabled
            pluginGUIDs = pluginMetadata.Keys.Where(GetPluginShowOverrideState).ToList();
        }


        public static void ResetPluginShowOverrides()
        {
            // Initialize blank save with the currently loaded plugins
            pluginShowOverrides = [];
            foreach (string plugin in pluginMetadata.Keys) pluginShowOverrides.Add(plugin, true);
            SetActiveGUIDs();
        }



        public static void SetPluginShowOverrideState(string pluginGUID, bool enabled)
        {
            if (pluginShowOverrides == null) ResetPluginShowOverrides();

            pluginShowOverrides[pluginGUID] = enabled;
            SetActiveGUIDs();
            SavePluginShowOverrides();
        }



        public static bool GetPluginShowOverrideState(string pluginGUID)
        {
            if (pluginShowOverrides == null) ResetPluginShowOverrides();

            return !pluginShowOverrides.TryGetValue(pluginGUID, out bool enabled) || enabled;
        }
    }
}