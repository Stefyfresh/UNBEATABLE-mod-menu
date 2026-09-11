using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;

namespace ModMenu
{
    public static class ConfigFinder
    {
        public static readonly string[] disallowedPluginGUIDs = ["com.sinai.unityexplorer"];
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

                    // Skip plugins that I am manually ignoring
                    if (disallowedPluginGUIDs.Contains(plugin.Info.Metadata.GUID)) continue;

                    // Add all plugin information
                    pluginGUIDs.Add(plugin.Info.Metadata.GUID);
                    pluginConfigFiles.Add(plugin.Info.Metadata.GUID, plugin.Config);
                    pluginMetadata.Add(plugin.Info.Metadata.GUID, plugin.Info.Metadata);
                }

                ModMenu.Logger.LogInfo("Gathered all configs for plugins!");
            }
            else
            {
                ModMenu.Logger.LogError("Could not find any plugins to gather config files for.");
            }
        }
    }
}