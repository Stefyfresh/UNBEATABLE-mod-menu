using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using Newtonsoft.Json;

namespace ModMenu
{
    public static class ConfigFinder
    {
        public static readonly string pluginShowOverridePath = Path.Combine(Paths.ConfigPath, $"{ModMenu.PLUGIN_GUID}.pluginOverrides.json");
        public static Dictionary<string, bool> pluginShowOverrides;


        public static List<string> pluginGUIDs = [];
        public static Dictionary<string, ConfigFile> pluginConfigFiles = [];
        public static Dictionary<string, BepInPlugin> pluginMetadata = [];
        public static void Init()
        {
            BaseUnityPlugin[] plugins = ModMenu.Instance?.gameObject?.GetComponents<BaseUnityPlugin>();

            if (plugins != null && plugins.Count() > 0)
            {
                List<BaseUnityPlugin> sortedPlugins = plugins.ToList();
                sortedPlugins.Sort((a, b) => a.Info.Metadata.Name.CompareTo(b.Info.Metadata.Name));

                foreach (BaseUnityPlugin plugin in sortedPlugins)
                {
                    // Skip the current plugin as we set the configs manually
                    if (plugin == ModMenu.Instance) continue;

                    // Add all plugin information
                    pluginGUIDs.Add(plugin.Info.Metadata.GUID);
                    pluginConfigFiles.Add(plugin.Info.Metadata.GUID, plugin.Config);
                    pluginMetadata.Add(plugin.Info.Metadata.GUID, plugin.Info.Metadata);
                }

                LoadPluginShowOverrides();

                ModMenu.Logger.LogInfo("Gathered all configs for plugins!");
            }
            else
            {
                ModMenu.Logger.LogWarning("Could not find any plugins to gather config files for.");
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