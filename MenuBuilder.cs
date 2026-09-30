using HarmonyLib;
using UnityEngine;
using Arcade.UI.Options;
using TMPro;
using CrossPlatform;
using UI;
using UnityEngine.UI;
using BepInEx.Configuration;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;
using BepInEx;
using System;
using System.Reflection;
using ModMenu.Keybinds;
using FMOD.Studio;
using FMODUnity;
using UnityEngine.Localization.PropertyVariants;
using UnityEngine.Localization.Settings;

namespace ModMenu
{
    public static class MenuBuilder
    {
        public static readonly string modMenuName = "Mod Menu";
        public static readonly string modButtonName = "Mod Menu Button";

        public static Transform modMenuContent;
        public static GameObject modMenuGO;
        public static GameObject modButtonGO;
        public static GameObject labelPrefab;
        public static GameObject selectorPrefab;
        public static GameObject togglePrefab;
        public static GameObject inputPrefab;
        public static GameObject keybindsPrefab;
        public static GameObject originalKeybindsPrefab;
        public static GameObject buttonPrefab;

        public static GameObject bottomTextPrefab;
        public static GameObject bottomTextObject;
        public static TextMeshProUGUI bottomTextTMP;

        public static Selectable firstSelectable;


        public static Material customUIMaterial;
        public static ScrollRect scroll;

        public static bool createdOptionProviders;
        public static int lastOptionProviderIndex;

        public static int numErrors;

        public static void Init(Transform menuRoot)
        {
            try
            {
                // Reset state
                firstSelectable = null;
                numErrors = 0;

                // Get existing GameObjects
                GameObject keybindsButtonGO = menuRoot.Find("ScreenArea/OptionsCorner/FullOptionsMenu/Tabs/Keybinds")?.gameObject;
                Transform interfaceScreen = menuRoot.Find("ScreenArea/OptionsCorner/FullOptionsMenu/Categories/Interface");
                labelPrefab = menuRoot.Find("ScreenArea/OptionsCorner/FullOptionsMenu/Categories/Keybinds/Viewport/Content/LabelStandard")?.gameObject;
                selectorPrefab = menuRoot.Find("ScreenArea/OptionsCorner/FullOptionsMenu/Categories/Graphics/Viewport/Content/OptionSelector")?.gameObject;
                togglePrefab = menuRoot.Find("ScreenArea/OptionsCorner/FullOptionsMenu/Categories/Graphics/Viewport/Content/OptionToggle")?.gameObject;
                originalKeybindsPrefab = menuRoot.Find("ScreenArea/OptionsCorner/FullOptionsMenu/Categories/Keybinds/Viewport/Content/MoveUpKey")?.gameObject;
                buttonPrefab = menuRoot.Find("ScreenArea/OptionsCorner/FullOptionsMenu/Categories/Keybinds/Viewport/Content/ResetKeybinds")?.gameObject;
                bottomTextPrefab = menuRoot.Find("ScreenArea/OptionsCorner/FullOptionsMenu/Categories/Keybinds/LabelSecondary").gameObject;


                // Make button
                modButtonGO = UnityEngine.Object.Instantiate(keybindsButtonGO, keybindsButtonGO.transform.parent);
                modButtonGO.name = modButtonName;
                modButtonGO.transform.SetSiblingIndex(0);
                modButtonGO.transform.parent.GetComponent<VerticalLayoutGroup>().spacing = MenuConstants.buttonLineSpacing;
                modButtonGO.GetComponent<UnityEngine.EventSystems.EventTrigger>().triggers[0].callback.m_PersistentCalls.m_Calls[4].arguments.boolArgument = false; // silly code to make it not enable the keybinds menu on click
                foreach (TextMeshProUGUI tmp in modButtonGO.GetComponentsInChildren<TextMeshProUGUI>(true))
                {
                    tmp.text = "<mspace=11>//<mspace=17> </mspace><cspace=0.35em>mods.";
                    if (tmp.gameObject.GetComponent<GameObjectLocalizer>() is GameObjectLocalizer localizer) localizer.enabled = false;
                }
                LayoutRebuilder.ForceRebuildLayoutImmediate(modButtonGO.transform.parent as RectTransform);
                Canvas.ForceUpdateCanvases();

                // Make menu
                modMenuGO = UnityEngine.Object.Instantiate(interfaceScreen.gameObject, interfaceScreen.parent);
                modMenuGO.name = modMenuName;
                modMenuGO.GetComponent<ScrollRect>().scrollSensitivity = MenuConstants.modMenuScrollSensitivity;
                modMenuContent = modMenuGO.transform.Find("Viewport/Content");

                // Register the menu transitions
                MenuTransitionsController.optionsTransitionsTransform = menuRoot.Find("ScreenArea/OptionsCorner/Transitions");
                MenuTransitionsController.RegisterModMenu();


                // Get scroll
                scroll = modMenuGO.GetComponent<ScrollRect>();

                // Get UI Material
                customUIMaterial = menuRoot.GetComponentInChildren<RawImage>().material;

                // Set spacing
                modMenuContent.GetComponent<VerticalLayoutGroup>().spacing = MenuConstants.menuLineSpacing;

                // Set selectable for going back
                modMenuGO.GetComponent<UIFocusOnButton>().selectables = [modButtonGO];

                // Make bottom text
                if (bottomTextPrefab != null)
                {
                    bottomTextObject = UnityEngine.Object.Instantiate(bottomTextPrefab, modMenuGO.transform);
                    bottomTextTMP = bottomTextObject.GetComponent<TextMeshProUGUI>();
                    RectTransform rect = bottomTextObject.transform as RectTransform;
                    rect.sizeDelta = new Vector2(0, rect.sizeDelta.y);
                    rect.localPosition = new Vector3(0, rect.localPosition.y, rect.localPosition.z);
                    SetLabelText(bottomTextObject, $"{ConfigFinder.NumConfigurableMods} configurable {(ConfigFinder.NumConfigurableMods == 1 ? "mods" : "mods")} with {ConfigFinder.NumConfigOptions} {(ConfigFinder.NumConfigOptions == 1 ? "option" : "options")} loaded.", HorizontalAlignmentOptions.Left);
                }

                ModMenu.Logger.LogInfo("Set relevant parameters for menu GameObjects.");

                // Build menu
                BuildMenu(true);
            }
            catch (Exception ex)
            {
                ModMenu.Logger.LogWarning($"Could not create mod menu! {ex}");
            }
        }

        public static void BuildMenu(bool scrollToTop)
        {
            // Destroy previous items
            while (modMenuContent.childCount > 0)
            {
                UnityEngine.Object.DestroyImmediate(modMenuContent.GetChild(0).gameObject);
            }

            // Try to create the input prefab and set state accordingly
            bool inputEnabled = TryGenerateInputPrefab();

            // Try to generate the keybinds prefab and set state accordingly
            bool keybindsEnabled = TryGenerateKeybindsPrefab();

            CreateModMenuTextAndOptions();

            // Generate fields for existing plugin configs
            int currentConfigIndex = (int)ModMenuOptions.AutoCreatedOptions;
            foreach (string pluginGUID in ConfigFinder.pluginGUIDs)
            {
                if (ConfigFinder.pluginConfigFiles.TryGetValue(pluginGUID, out ConfigFile pluginConfigFile) && pluginConfigFile.Count > 0)
                {
                    // Create texts for the name of the plugin
                    if (TryCreateMenuObject(labelPrefab, out GameObject tempGO))
                    {
                        TrySetLabelOptions(tempGO, MenuConstants.titleLabelTopMargin);
                    }

                    if (TryCreateMenuObject(labelPrefab, out tempGO))
                    {
                        TrySetLabelOptions(tempGO, MenuConstants.titleLabelHeight, PadStringWithLines(ConfigFinder.pluginMetadata[pluginGUID].Name), HorizontalAlignmentOptions.Center);
                    }


                    // Group objects into sections
                    List<ConfigDefinition> sortedDefinitions = pluginConfigFile.Keys.ToList();
                    sortedDefinitions.Sort((a, b) =>
                    {
                        if (a.Section == "General" && b.Section != "General") return -1;
                        if (b.Section == "General" && a.Section != "General") return 1;

                        int sectionComparison = a.Section.CompareTo(b.Section);
                        if (sectionComparison != 0 || !ModMenu.sortOptions.Value) return sectionComparison;

                        return a.Key.CompareTo(b.Key);
                    });
                    string currentSectionName = "General";

                    // Get plugin entries and make options for them
                    foreach (ConfigDefinition definition in sortedDefinitions)
                    {
                        ConfigEntryBase config = pluginConfigFile[definition];

                        // Ignore entries set as not browsable
                        if (config.Description.Tags != null && config.Description.Tags.Count() > 0)
                        {
                            bool skipThisConfig = false;
                            foreach (object tag in config.Description.Tags)
                            {
                                Type type = tag.GetType();
                                if (type.Name == "ConfigurationManagerAttributes")
                                {
                                    FieldInfo field = type.GetField("Browsable");
                                    object browsable = field?.GetValue(tag);
                                    if (browsable is bool Browsable && !Browsable) skipThisConfig = true;
                                }
                            }
                            if (skipThisConfig) continue;
                        }

                        // Create section text if there is a section
                        if (definition.Section != currentSectionName)
                        {
                            if (TryCreateMenuObject(labelPrefab, out tempGO))
                            {
                                TrySetLabelOptions(tempGO, MenuConstants.configSectionTopMargin);
                            }

                            if (TryCreateMenuObject(labelPrefab, out tempGO))
                            {
                                TrySetLabelOptions(tempGO, MenuConstants.configSectionHeight, definition.Section, HorizontalAlignmentOptions.Right);
                            }

                            currentSectionName = definition.Section;
                        }

                        // Bool option type stuff
                        if (config.SettingType == typeof(bool))
                        {
                            if (!createdOptionProviders) OptionsProvider.OptionProviders[(OptionsProvider.Option)currentConfigIndex] =
                                new OptionsProvider.ToggleOptionProvider(
                                    BeautifyString(UnCamelCase(definition.Key)),//Name
                                    () => (bool)config.BoxedValue,      //Getter
                                    (b) => config.BoxedValue = b        //Setter
                                );

                            if (CheckProviderValidityOrShowError(definition, currentConfigIndex) && TryCreateMenuObject(togglePrefab, out tempGO))
                            {
                                TrySetToggleOptions(tempGO, definition.Key, currentConfigIndex, MenuConstants.optionSelectorHeight);
                            }
                        }
                        // String type stuff
                        else if (config.SettingType == typeof(string) && inputEnabled && config.Description.AcceptableValues == null)
                        {
                            // TODO: maybe remove this idk
                            // only cosmetic lol
                            if (!createdOptionProviders) OptionsProvider.OptionProviders[(OptionsProvider.Option)currentConfigIndex] =
                                new OptionsProvider.ToggleOptionProvider(BeautifyString(UnCamelCase(definition.Key)), () => false, (b) => { });

                            if (CheckProviderValidityOrShowError(definition, currentConfigIndex) && TryCreateMenuObject(inputPrefab, out tempGO))
                            {
                                TrySetInputOptions(tempGO, config, definition, MenuConstants.optionSelectorHeight, currentConfigIndex);
                            }
                        }
                        // Enum type stuff
                        else if (config.SettingType.IsEnum)
                        {
                            if (config.SettingType == typeof(KeyCode) && TryCreateMenuObject(keybindsPrefab, out tempGO) && keybindsEnabled)
                            {
                                TrySetKeybindOptions(tempGO, MenuConstants.optionSelectorHeight, config, definition);
                            }
                            else
                            {
                                if (!createdOptionProviders) config.CreateEnumOptionProvider(definition, currentConfigIndex, ConfigFinder.pluginMetadata[pluginGUID]);

                                if (CheckProviderValidityOrShowError(definition, currentConfigIndex) && TryCreateMenuObject(selectorPrefab, out tempGO))
                                {
                                    TrySetSelectorOptions(tempGO, definition, currentConfigIndex, MenuConstants.optionSelectorHeight);
                                }
                            }
                        }
                        // Acceptable value stuff
                        else if (config.Description.AcceptableValues != null)
                        {
                            if (!createdOptionProviders)
                            {
                                AcceptableValueBase acceptable = config.Description.AcceptableValues;

                                // Acceptable value list stuff
                                if (acceptable.GetType().IsGenericType && acceptable.GetType().GetGenericTypeDefinition() == typeof(AcceptableValueList<>) && !createdOptionProviders)
                                {
                                    acceptable.CreateListOptionProvider(config, definition, currentConfigIndex);
                                }

                                // Acceptable value range stuff
                                if (acceptable.GetType().IsGenericType && acceptable.GetType().GetGenericTypeDefinition() == typeof(AcceptableValueRange<>) && !createdOptionProviders)
                                {
                                    acceptable.CreateRangeOptionProvider(config, definition, currentConfigIndex);
                                }
                            }

                            if (CheckProviderValidityOrShowError(definition, currentConfigIndex) && TryCreateMenuObject(selectorPrefab, out tempGO))
                            {
                                TrySetSelectorOptions(tempGO, definition, currentConfigIndex, MenuConstants.optionSelectorHeight);
                            }
                        }
                        // Generic stuff
                        else
                        {
                            // Create GameObject if provider exists
                            if (OptionsProvider.OptionProviders.ContainsKey((OptionsProvider.Option)currentConfigIndex) && TryCreateMenuObject(selectorPrefab, out tempGO))
                            {
                                TrySetSelectorOptions(tempGO, definition, currentConfigIndex, MenuConstants.optionSelectorHeight);
                            }
                            else
                            {
                                // Don't make descriptions for configs that cannot be processed
                                currentConfigIndex++;
                                continue;
                            }
                        }

                        currentConfigIndex++;
                        CreateDescriptionIfNeeded(config);
                    }
                }
            }

            if (numErrors > 0) bottomTextTMP.text += "<br><mspace=11>//<mspace=17> </mspace><b>" + BeautifyString($"<uppercase>ERROR:</uppercase> Failed to create {numErrors} {(numErrors == 1 ? "option" : "options")}!");

            CreatePluginShowOverrides(currentConfigIndex);

            if (!createdOptionProviders)
            {
                ModMenu.Logger.LogInfo("Successfully created option providers for all plugin configs.");
                createdOptionProviders = true;
            }

            // Scroll to top
            if (scrollToTop)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(modMenuContent as RectTransform);
                if (scroll) scroll.verticalNormalizedPosition = 1;
            }

            // Set selectable
            CustomUINavigation nav = modButtonGO.GetComponent<CustomUINavigation>();
            if (nav)
            {
                nav.rightNavigation.explicitSelectables = [firstSelectable];
            }

            ModMenu.Logger.LogInfo("Successfully built mod menu.");
        }


        public static bool TryRebuildMenu(bool scrollToTop)
        {
            try
            {
                BuildMenu(scrollToTop);
                return true;
            }
            catch (Exception ex)
            {
                ModMenu.Logger.LogWarning($"Failed to rebuild menu! {ex}");
                return false;
            }
        }


        private static void CreateModMenuTextAndOptions()
        {
            // Create mod menu label
            if (TryCreateMenuObject(labelPrefab, out GameObject tempGO))
            {
                TrySetLabelOptions(tempGO, MenuConstants.titleLabelHeight, PadStringWithLines("Mod Menu"), HorizontalAlignmentOptions.Center);
            }

            // Create descriptions option
            if (!createdOptionProviders) OptionsProvider.OptionProviders.TryAdd((OptionsProvider.Option)ModMenuOptions.ShowOptionDescriptions,
                new OptionsProvider.ToggleOptionProvider(
                    BeautifyString("Show Option Descriptions"),
                    () => ModMenu.showOptionDescriptions.Value,
                    SetShowOptionDescriptions
                )
            );

            if (TryCreateMenuObject(togglePrefab, out tempGO))
            {
                TrySetToggleOptions(tempGO, ModMenu.showOptionDescriptions.Definition.Key, (int)ModMenuOptions.ShowOptionDescriptions, MenuConstants.optionSelectorHeight);
                CreateDescriptionIfNeeded(ModMenu.showOptionDescriptions);
            }

            // Create sort option
            if (!createdOptionProviders) OptionsProvider.OptionProviders.TryAdd((OptionsProvider.Option)ModMenuOptions.SortMenuOptions,
                new OptionsProvider.ToggleOptionProvider(
                    BeautifyString("Sort Menu Options"),
                    () => ModMenu.sortOptions.Value,
                    SetMenuSort
                )
            );

            if (TryCreateMenuObject(togglePrefab, out tempGO))
            {
                TrySetToggleOptions(tempGO, ModMenu.sortOptions.Definition.Key, (int)ModMenuOptions.SortMenuOptions, MenuConstants.optionSelectorHeight);
                CreateDescriptionIfNeeded(ModMenu.sortOptions);
            }
        }


        private static void CreateDescriptionIfNeeded(ConfigEntryBase config)
        {
            CreateDescriptionIfNeeded(config.Description.Description);
        }
        private static void CreateDescriptionIfNeeded(string description)
        {
            if (ModMenu.showOptionDescriptions.Value)
            {
                if (!description.IsNullOrWhiteSpace())
                {
                    GameObject tempGO;
                    foreach (string line in SplitLineIntoChunks(description, "\n", MenuConstants.configDescriptionLength))
                    {
                        if (!line.IsNullOrWhiteSpace() && TryCreateMenuObject(labelPrefab, out tempGO))
                        {
                            TrySetLabelOptions(tempGO, MenuConstants.configDescriptionHeight, line, HorizontalAlignmentOptions.Left);
                        }
                    }
                    if (TryCreateMenuObject(labelPrefab, out tempGO))
                    {
                        TrySetLabelOptions(tempGO, MenuConstants.configDescriptionBottomMargin);
                    }
                }
            }
        }


        private static void CreatePluginShowOverrides(int currentConfigIndex)
        {
            // Create label
            if (TryCreateMenuObject(labelPrefab, out GameObject tempGO))
            {
                TrySetLabelOptions(tempGO, MenuConstants.titleLabelTopMargin);
            }
            if (TryCreateMenuObject(labelPrefab, out tempGO))
            {
                TrySetLabelOptions(tempGO, MenuConstants.titleLabelHeight, PadStringWithLines("Show in Mod Menu"), HorizontalAlignmentOptions.Center);
            }

            // Create options
            foreach (BepInPlugin plugin in ConfigFinder.pluginMetadata.Values)
            {
                if (ConfigFinder.pluginConfigFiles.TryGetValue(plugin.GUID, out ConfigFile pluginConfigFile) && pluginConfigFile.Count > 0)
                {
                    if (!createdOptionProviders) OptionsProvider.OptionProviders[(OptionsProvider.Option)currentConfigIndex] =
                        new OptionsProvider.ToggleOptionProvider(
                            BeautifyString(UnCamelCase(plugin.Name)),//Name
                            () => ConfigFinder.GetPluginShowOverrideState(plugin.GUID),
                            (b) =>
                            {
                                ConfigFinder.SetPluginShowOverrideState(plugin.GUID, b);
                                ResetOptionProviders();
                                TryRebuildMenu(false);
                            }
                        );

                    if (TryCreateMenuObject(togglePrefab, out tempGO))
                    {
                        TrySetToggleOptions(tempGO, plugin.Name, currentConfigIndex, MenuConstants.optionSelectorHeight);
                    }

                    CreateDescriptionIfNeeded($"Show or hide all configuration for plugin \"{plugin.Name}\"");

                    currentConfigIndex++;
                }
            }

            lastOptionProviderIndex = currentConfigIndex;
        }



        private static void ResetOptionProviders()
        {
            createdOptionProviders = false;
            for (int i = (int)ModMenuOptions.AutoCreatedOptions; i < lastOptionProviderIndex; i++)
            {
                OptionsProvider.OptionProviders.Remove((OptionsProvider.Option)i);
            }
        }

        private static bool CheckProviderValidityOrShowError(ConfigDefinition definition, int providerIndex)
        {
            if (OptionsProvider.OptionProviders.ContainsKey((OptionsProvider.Option)providerIndex))
            {
                return true;
            }
            else
            {
                ModMenu.Logger.LogWarning($"Option provider for config value \"{UnCamelCase(definition.Key)}\" is not present or not valid!");
                ShowErrorAsLabel(MenuConstants.optionSelectorHeight, $"<b>Error creating option \"{UnCamelCase(definition.Key)}\"!", HorizontalAlignmentOptions.Center);
                numErrors++;
                return false;
            }
        }


        private static void ShowErrorAsLabel(int height, string errorText, HorizontalAlignmentOptions alignment)
        {
            if (TryCreateMenuObject(labelPrefab, out GameObject tempGO))
            {
                TrySetLabelOptions(tempGO, height, errorText, alignment);
            }
        }



        public static List<string> SplitLineIntoChunks(string str, string separator, int maxChunkLength)
        {
            List<string> strings = [];
            foreach (string s in str.Split(separator).ToList())
            {
                if (s.Length > maxChunkLength)
                {
                    int lastSpaceIndex = maxChunkLength - 1;
                    for (int i = 0; i < maxChunkLength; i++)
                    {
                        if (s[i] == ' ') lastSpaceIndex = i;
                    }

                    strings.Add(s.Substring(0, lastSpaceIndex));

                    List<string> split = SplitLineIntoChunks(s.Substring(lastSpaceIndex + 1), separator, maxChunkLength);
                    strings.AddRange(split);
                }
                else
                {
                    strings.Add(s);
                }
            }
            return strings;
        }

        public static string UnCamelCase(string text)
        {
            // Case 1: Inserts space between lowercase/digit and an uppercase letter
            // Case 2: Inserts space between an uppercase letter and the start of a new camelCase word
            return Regex.Replace(text, @"([a-z0-9])([A-Z])|([A-Z])([A-Z][a-z])", "$1$3 $2$4");
        }


        public static string BeautifyString(string str)
        {
            return $"<cspace=0.2em>{str.Replace(" ", "<space=0.72em>")}</cspace>";
        }


        public static string PadStringWithLines(string str)
        {
            return PadStringWithLines(str, MenuConstants.titleLabelLength);
        }

        public static string PadStringWithLines(string str, int length)
        {
            int spacesTot = MenuConstants.titleLabelSpacePadding * 2;
            string output = str;
            if (str.Length + MenuConstants.titleLabelSpacePadding < length)
            {
                int numDashesPerSide = (length - str.Length) / 2 - MenuConstants.titleLabelSpacePadding;
                output = new string('-', numDashesPerSide) + new string(' ', spacesTot) + str + new string(' ', spacesTot) + new string('-', numDashesPerSide);
            }
            return output;
        }


        private static bool TryCreateMenuObject(GameObject prefab, out GameObject created)
        {
            if (prefab != null)
            {
                created = UnityEngine.Object.Instantiate(prefab, modMenuContent);
                if (created == null)
                {
                    ModMenu.Logger.LogWarning("Menu object was instantiated as null! Ignoring.");
                    return false;
                }
                else
                {
                    created.SetActive(true);
                    return true;
                }
            }
            else
            {
                ModMenu.Logger.LogWarning("A menu option was created with a null prefab! Ignoring.");
                created = null;
                return false;
            }
        }


        private static bool TrySetLabelOptions(GameObject gameObject, int height, string text = "", HorizontalAlignmentOptions alignment = HorizontalAlignmentOptions.Center)
        {
            try
            {
                SetHeight(gameObject, height);
                SetLabelText(gameObject, text, alignment);
                FixLocalizedFont(gameObject);
                return true;
            }
            catch (Exception)
            {
                ModMenu.Logger.LogWarning($"Failed to set parameters on label object!");
                gameObject?.SetActive(false);
                numErrors++;
                return false;
            }
        }


        private static bool TrySetToggleOptions(GameObject gameObject, string optionName, int currentConfigIndex, int height)
        {
            try
            {
                SetNavigationTransform(gameObject);
                Selectable selectable = SetOptionProviderToggle(gameObject, currentConfigIndex);
                if (firstSelectable == null) firstSelectable = selectable;
                SetHeight(gameObject, height);
                FixLocalizedFont(gameObject);
                return true;
            }
            catch (Exception)
            {
                ModMenu.Logger.LogWarning($"Failed to set parameters on toggle object for option \"{optionName}\"!");
                gameObject?.SetActive(false);
                ShowErrorAsLabel(MenuConstants.optionSelectorHeight, $"<b>Error creating option \"{UnCamelCase(optionName)}\"", HorizontalAlignmentOptions.Left);
                numErrors++;
                return false;
            }
        }


        private static bool TrySetSelectorOptions(GameObject gameObject, ConfigDefinition definition, int currentConfigIndex, int height)
        {
            try
            {
                SetNavigationTransform(gameObject);
                Selectable selectable = SetOptionProviderSelector(gameObject, currentConfigIndex);
                if (firstSelectable == null) firstSelectable = selectable;
                SetHeight(gameObject, height);
                FixLocalizedFont(gameObject);
                return true;
            }
            catch (Exception)
            {
                ModMenu.Logger.LogWarning($"Failed to set parameters on selector object for option \"{definition.Key}\"!");
                gameObject?.SetActive(false);
                ShowErrorAsLabel(MenuConstants.optionSelectorHeight, $"<b>Error creating option \"{UnCamelCase(definition.Key)}\"", HorizontalAlignmentOptions.Left);
                numErrors++;
                return false;
            }
        }


        private static bool TrySetKeybindOptions(GameObject gameObject, int height, ConfigEntryBase config, ConfigDefinition definition)
        {
            try
            {
                SetNavigationTransform(gameObject);
                SetHeight(gameObject, MenuConstants.optionSelectorHeight);
                FixLocalizedFont(gameObject);
                SetKeybindManager(gameObject, config, definition);
                return true;
            }
            catch (Exception)
            {
                ModMenu.Logger.LogWarning($"Failed to set parameters on keybind object for option \"{definition.Key}\"!");
                gameObject?.SetActive(false);
                ShowErrorAsLabel(MenuConstants.optionSelectorHeight, $"<b>Error creating option \"{UnCamelCase(definition.Key)}\"", HorizontalAlignmentOptions.Left);
                numErrors++;
                return false;
            }
        }


        private static bool TrySetInputOptions(GameObject gameObject, ConfigEntryBase config, ConfigDefinition definition, int height, int currentConfigIndex)
        {
            try
            {
                SetNavigationTransform(gameObject);
                SetOptionProviderInput(gameObject, config, currentConfigIndex);
                SetHeight(gameObject, height);
                FixLocalizedFont(gameObject);
                return true;
            }
            catch (Exception)
            {
                ModMenu.Logger.LogWarning($"Failed to set parameters on input object for option \"{definition.Key}\"!");
                gameObject?.SetActive(false);
                ShowErrorAsLabel(MenuConstants.optionSelectorHeight, $"<b>Error creating option \"{UnCamelCase(definition.Key)}\"", HorizontalAlignmentOptions.Left);
                numErrors++;
                return false;
            }
        }


        public static void SetHeight(GameObject gameObject, int height)
        {
            RectTransform rect = gameObject.transform as RectTransform;
            if (rect) rect.sizeDelta = new Vector2(rect.sizeDelta.x, height);
        }


        private static void SetLabelText(GameObject labelGO, string text = "", HorizontalAlignmentOptions alignment = HorizontalAlignmentOptions.Center)
        {
            TextMeshProUGUI tmp = labelGO.GetComponent<TextMeshProUGUI>();
            if (text == "")
            {
                UnityEngine.Object.Destroy(tmp);
            }
            else
            {
                tmp.horizontalAlignment = alignment;
                tmp.text = $"<mspace=11>//<mspace=17> </mspace>" + BeautifyString(text);
            }
        }

        private static void FixLocalizedFont(GameObject gameObject)
        {
            LocalizedFont localizedFont = gameObject.GetComponentInChildren<LocalizedFont>(true);
            if (localizedFont != null)
            {
                UnityEngine.Object.DestroyImmediate(localizedFont);
            }
        }


        private static void SetNavigationTransform(GameObject gameObject)
        {
            CustomUINavigation nav = gameObject.GetComponentInChildren<CustomUINavigation>();
            if (nav != null)
            {
                nav.upNavigation.parentFilter = modMenuContent;
                nav.downNavigation.parentFilter = modMenuContent;
            }
        }


        private static OptionHorizontalToggle SetOptionProviderToggle(GameObject gameObject, int option)
        {
            OptionHorizontalToggle toggle = gameObject.GetComponent<OptionHorizontalToggle>();
            if (toggle != null)
            {
                LocalizationSettings.SelectedLocaleChanged -= toggle.OnSelectedLocaleChanged;
                toggle._optionProvider = null;
                toggle.option = (OptionsProvider.Option)option;

                // Equivalent to OnEnable without localization
                if (Application.IsPlaying(toggle) && OptionsProvider.OptionProviders.TryGetValue((OptionsProvider.Option)option, out var value) && value is OptionsProvider.ToggleOptionProvider optionProvider)
                {
                    toggle._optionProvider = optionProvider;
                    toggle.UpdateTexts();
                }
            }
            return toggle;
        }

        private static OptionHorizontalSelector SetOptionProviderSelector(GameObject gameObject, int option)
        {
            OptionHorizontalSelector selector = gameObject.GetComponent<OptionHorizontalSelector>();
            if (selector != null)
            {
                LocalizationSettings.SelectedLocaleChanged -= selector.OnSelectedLocaleChanged;
                selector._optionProvider = null;
                selector.option = (OptionsProvider.Option)option;

                // Equivalent to OnEnable without localization
                if (Application.IsPlaying(selector))
                {
                    selector.LoadProvider();
                    selector.UpdateTexts();
                }
            }
            return selector;
        }

        private static OptionHorizontalToggle SetOptionProviderInput(GameObject gameObject, ConfigEntryBase config, int option)
        {
            OptionHorizontalToggle toggle = gameObject.GetComponent<OptionHorizontalToggle>();
            if (toggle != null)
            {
                LocalizationSettings.SelectedLocaleChanged -= toggle.OnSelectedLocaleChanged;
                toggle._optionProvider = null;
                toggle.option = (OptionsProvider.Option)option;

                // TODO: this is dumb
                // Equivalent to OnEnable without localization
                if (Application.IsPlaying(toggle) && OptionsProvider.OptionProviders.TryGetValue((OptionsProvider.Option)option, out var value) && value is OptionsProvider.ToggleOptionProvider optionProvider)
                {
                    toggle._optionProvider = optionProvider;
                    toggle.UpdateTexts();
                }

                // Invalidate provider so it does not go left and right
                toggle._optionProvider = null;
                toggle.optionName = null;
                toggle.optionValue = null;
            }

            TMP_InputField input = gameObject.GetComponentInChildren<TMP_InputField>();
            if (input != null)
            {
                input.text = config.GetSerializedValue();
                input.onSubmit.AddListener((text) =>
                {
                    // Set the config value
                    config.SetSerializedValue(text);

                    // Reassign the text in case it was rejected
                    input.text = config.GetSerializedValue();
                });
            }
            return toggle;
        }

        private static void SetKeybindManager(GameObject gameObject, ConfigEntryBase config, ConfigDefinition definition)
        {
            OptionKeybindSelector selector = gameObject.GetComponentInChildren<OptionKeybindSelector>();
            CustomUINavigation nav = gameObject.GetComponentInChildren<CustomUINavigation>();
            if (selector && nav)
            {
                GameObject button = selector.gameObject;

                gameObject.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = "<mspace=11>//<mspace=17> </mspace><cspace=0.35em>" + BeautifyString(UnCamelCase(definition.Key));

                EventReference menuAcceptEvent = selector.menuAcceptEvent;
                ArcadeKeybindMenu menu = selector.menu;
                TextMeshProUGUI setting = selector.setting;

                CustomUINavigation.NavigationDirection upNavigation = nav.upNavigation;
                CustomUINavigation.NavigationDirection downNavigation = nav.downNavigation;
                CustomUINavigation.NavigationDirection leftNavigation = nav.leftNavigation;
                CustomUINavigation.NavigationDirection rightNavigation = nav.rightNavigation;

                UnityEngine.Object.DestroyImmediate(nav);
                UnityEngine.Object.DestroyImmediate(selector);

                ModMenuKeybindManager keybinds = button.AddComponent<ModMenuKeybindManager>();
                keybinds.Init(menuAcceptEvent, menu, setting, config as ConfigEntry<KeyCode>);
                setting.horizontalAlignment = HorizontalAlignmentOptions.Center;

                CustomUINavigation newNav = button.AddComponent<CustomUINavigation>();
                newNav.upNavigation = upNavigation;
                newNav.downNavigation = downNavigation;
                newNav.leftNavigation = leftNavigation;
                newNav.rightNavigation = rightNavigation;
            }
        }


        public static void SetShowOptionDescriptions(bool enabled)
        {
            ModMenu.showOptionDescriptions.Value = enabled;
            if (TryRebuildMenu(true)) firstSelectable?.Select();
        }

        public static void SetMenuSort(bool enabled)
        {
            ModMenu.sortOptions.Value = enabled;
            ResetOptionProviders();
            if (TryRebuildMenu(true)) firstSelectable?.Select();
        }

        // private static void SetTextMode(GameObject gameObject, FontStyles style)
        // {
        //     TextMeshProUGUI text = gameObject.transform.Find("ValueGroup/Value").GetComponent<TextMeshProUGUI>();
        //     if (text != null) text.fontStyle = style;
        // }


        public static bool TryGenerateInputPrefab()
        {
            try
            {
                // Create and get objects
                inputPrefab = UnityEngine.Object.Instantiate(togglePrefab, modMenuContent);
                inputPrefab.name = "OptionInput";
                inputPrefab.transform.Find("ValueGroup/LeftArrow").gameObject.SetActive(false);
                inputPrefab.transform.Find("ValueGroup/RightArrow").gameObject.SetActive(false);
                GameObject oldValue = inputPrefab.transform.Find("ValueGroup/Value").gameObject;
                TextMeshProUGUI oldValueText = oldValue.GetComponent<TextMeshProUGUI>();
                oldValue.SetActive(false);
                GameObject consoleInputPrefab = JeffBezosController.instance?.transform.Find("AllenDulles/Canvas Parent/Canvas/Console/Console Input")?.gameObject;
                GameObject inputGO = UnityEngine.Object.Instantiate(consoleInputPrefab, inputPrefab.transform.Find("ValueGroup"));
                // Set transform values
                if (inputGO.transform is RectTransform rect)
                {
                    rect.localPosition = Vector3.zero;
                    rect.pivot = new Vector2(0.5f, 0.5f);
                    rect.sizeDelta = new Vector2(MenuConstants.inputTextAreaWidth, MenuConstants.inputTextAreaHeight);
                }

                Image bg = inputGO.GetComponent<Image>();
                if (bg)
                {
                    bg.material = customUIMaterial;
                    bg.color = new Color(0.0248f, 0, 0, 1);
                }

                // Set up text area
                TMP_InputField input = inputGO.GetComponent<TMP_InputField>();
                input.text = "";
                input.caretWidth = 2;
                input.lineType = TMP_InputField.LineType.SingleLine;
                input.fontAsset = oldValueText.font;
                if (input.textComponent is TextMeshProUGUI text)
                {
                    text.font = oldValueText.font;
                    text.fontMaterial = oldValueText.fontMaterial;
                    text.color = new Color(0.15f, 0, 0, 1);
                }
                if (input.placeholder is TextMeshProUGUI placeholderText)
                {
                    placeholderText.font = oldValueText.font;
                    placeholderText.text = BeautifyString("...");
                    placeholderText.fontMaterial = oldValueText.fontMaterial;
                    placeholderText.color = new Color(0.15f, 0, 0, 0.5f);
                    placeholderText.fontStyle = FontStyles.Normal;
                }
                if (inputGO.transform.Find("Text Area/Caret") is RectTransform caretRect)
                {
                    caretRect.offsetMax = new Vector2(0, 0);
                    caretRect.offsetMin = new Vector2(0, 0);
                    caretRect.pivot = new Vector2(0.5f, 0.5f);
                }
                if (inputGO.transform.Find("Text Area/Text") is RectTransform textRect)
                {
                    textRect.offsetMax = new Vector2(0, 0);
                    textRect.offsetMin = new Vector2(0, 0);
                    textRect.pivot = new Vector2(0.5f, 0.5f);
                }
                if (inputGO.transform.Find("Text Area/Placeholder") is RectTransform placeholderRect)
                {
                    placeholderRect.offsetMax = new Vector2(0, 0);
                    placeholderRect.offsetMin = new Vector2(0, 0);
                    placeholderRect.pivot = new Vector2(0.5f, 0.5f);
                }

                // inputGO.AddComponent<InputTextController>();

                inputPrefab.SetActive(false);
                return true;
            }
            catch (Exception ex)
            {
                ModMenu.Logger.LogWarning($"Failed to create input prefab! {ex}");
                numErrors++;
                inputPrefab?.SetActive(false);
                return false;
            }
        }


        public static bool TryGenerateKeybindsPrefab()
        {
            try
            {
                // Create and get objects
                keybindsPrefab = UnityEngine.Object.Instantiate(originalKeybindsPrefab, modMenuContent);
                keybindsPrefab.name = "OptionKeybinds";

                keybindsPrefab.transform.Find("Primary").gameObject.SetActive(false);
                // RectTransform rect = keybindsPrefab.transform.Find("Secondary").transform as RectTransform;
                // rect.sizeDelta = new Vector2(300, rect.sizeDelta.y);
                keybindsPrefab.transform.Find("Secondary").GetComponent<LayoutElement>().preferredWidth = MenuConstants.optionSelectorWidth;

                // OptionKeybindSelector selector = keybindsPrefab.GetComponentInChildren<OptionKeybindSelector>();
                // if (selector) selector.enabled = false;

                // HorizontalLayoutGroup horizontalLayout = keybindsPrefab.GetComponent<HorizontalLayoutGroup>();
                // if (horizontalLayout) horizontalLayout.childControlWidth = false;

                keybindsPrefab?.SetActive(false);
                return true;
            }
            catch (Exception ex)
            {
                ModMenu.Logger.LogWarning($"Failed to create keybinds prefab! {ex}");
                numErrors++;
                // keybindsPrefab?.SetActive(false);
                return false;
            }
        }
    }
}