using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using BepInEx;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace ModMenu
{
    public static class PluginUpdateManager
    {
        private static List<PluginUpdateInfo> pluginsWithUpdates = [];

        public static void CheckForUpdates(BaseUnityPlugin plugin)
        {
            if (plugin != null && ModMenu.doUpdateCheck.Value)
            {
                try
                {
                    string filePath = plugin.Info.Location;
                    if (File.Exists(filePath))
                    {
                        // Check assembly version info for a URL
                        FileVersionInfo pluginFileInfo = FileVersionInfo.GetVersionInfo(filePath);
                        string[] candidateURLs = [pluginFileInfo.CompanyName, pluginFileInfo.FileDescription, pluginFileInfo.Comments, pluginFileInfo.LegalCopyright, pluginFileInfo.LegalTrademarks];
                        string foundURL = candidateURLs.FirstOrDefault(s => Uri.IsWellFormedUriString(s, UriKind.Absolute));

                        if (!foundURL.IsNullOrWhiteSpace())
                        {
                            // Check that it is a GitHub link and extract the parameters if so
                            Uri uri = new UriBuilder(foundURL).Uri;
                            if (Regex.IsMatch(foundURL, @"github\.com\/(.+?)\/(.+?)") && uri.Segments.Count() == 3)
                            {
                                // Extract author and repo from URI
                                string author = uri.Segments[1].TrimEnd('/').ToString();
                                string repo = uri.Segments[2].TrimEnd('/').ToString();
                                string requestURL = $"https://api.github.com/repos/{author}/{repo}/releases/latest";

                                ModMenu.Instance.StartCoroutine(PluginUpdateFetch(requestURL, plugin, foundURL));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ModMenu.Logger.LogWarning($"Failed to check for updates for plugin {plugin.Info.Metadata.GUID}! {ex.Message}");
                }
            }
        }


        public static void NotifyPluginUpdates()
        {
            if (ModMenu.doUpdateCheck.Value) ModMenu.Instance.StartCoroutine(NotifyPluginUpdatesCoroutine());
        }


        private static IEnumerator NotifyPluginUpdatesCoroutine()
        {
            yield return new WaitForSeconds(2f);

            foreach (PluginUpdateInfo updateInfo in pluginsWithUpdates)
            {
                ArcadeNotification.Show<ArcadeChoiceNotification>("Choice", null).Fill(
                    "<cspace=0.05em>Mod update available!",
                    $"<cspace=0.05em>{updateInfo.plugin.Info.Metadata.Name} ({updateInfo.plugin.Info.Metadata.GUID})\n New version:  <space=3>v{updateInfo.newVersion}\n Your version:  v{updateInfo.oldVersion}",
                    "<cspace=0.05em>Dismiss",
                    "<cspace=0.05em>Open download page",
                    null,
                    () => OpenPluginPage(updateInfo.URL)
                    );
            }

            pluginsWithUpdates.Clear();
        }


        private static IEnumerator PluginUpdateFetch(string URL, BaseUnityPlugin plugin, string originalURL)
        {
            // Send request
            UnityWebRequest request = UnityWebRequest.Get(URL);
            yield return request.SendWebRequest();

            // Handle response
            if (request.result == UnityWebRequest.Result.Success)
            {
                // Get JSON data
                string text = request.downloadHandler.text;
                JObject jsonData = JObject.Parse(text);
                if (jsonData.TryGetValue("tag_name", out JToken tagToken))
                {
                    string versionString = (string)tagToken;
                    Version version = Version.Parse(versionString.TrimStart('v', 'V'));

                    // Check if version is newer
                    if (version.CompareTo(plugin.Info.Metadata.Version) == 1)
                    {
                        // Add update info
                        string updateURL = jsonData.TryGetValue("html_url", out JToken urlToken) ? (string)urlToken : $"{originalURL}/releases/latest";
                        ModMenu.Logger.LogMessage($"New version of mod {plugin.Info.Metadata.GUID} available! {tagToken}");
                        pluginsWithUpdates.Add(new PluginUpdateInfo
                        {
                            plugin = plugin,
                            oldVersion = plugin.Info.Metadata.Version,
                            newVersion = version,
                            URL = updateURL
                        });
                    }
                }
            }
            else
            {
                ModMenu.Logger.LogWarning($"Check for updates failed for {plugin.Info.Metadata.GUID}! {request.result} {request.responseCode}");
            }
            yield break;
        }


        private static void OpenPluginPage(string URL)
        {
            try
            {
                Process.Start(new ProcessStartInfo(URL) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                ModMenu.Logger.LogWarning($"Failed to open page at {URL}! {ex.Message}");
            }
        }


        private class PluginUpdateInfo
        {
            public BaseUnityPlugin plugin;
            public Version oldVersion;
            public Version newVersion;
            public string URL;
        }
    }
}