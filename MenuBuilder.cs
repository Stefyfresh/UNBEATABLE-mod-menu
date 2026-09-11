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
        public static Selectable firstSelectable;
        public static Selectable optionDescriptionSelectable;
        public static Material customUIMaterial;
        public static ScrollRect scroll;


        public static bool createdOptionProviders;


        public static void BuildMenu()
        {
            // Destroy previous items
            while (modMenuContent.childCount > 0)
            {
                UnityEngine.Object.DestroyImmediate(modMenuContent.GetChild(0).gameObject);
            }

            // Create the input prefab
            GenerateInputPrefab();

            // Create mod menu label
            GameObject tempGO = UnityEngine.Object.Instantiate(labelPrefab, modMenuContent);
            SetLabelText(tempGO, PadStringWithLines("Mod Menu"), HorizontalAlignmentOptions.Center);
            SetHeight(tempGO, MenuConstants.titleLabelHeight);
            FixLocalizedFont(tempGO);
            tempGO.SetActive(true);

            // Create faster menu transitions option
            if (!createdOptionProviders) OptionsProvider.OptionProviders.TryAdd((OptionsProvider.Option)ModMenuOptions.FasterTransitions,
                new OptionsProvider.ToggleOptionProvider(
                    BeautifyString("Faster Menu Transitions"),
                    () => ModMenu.fasterMenuTransitions.Value,
                    MenuController.SetFasterMenuTransitions
                )
            );
            tempGO = UnityEngine.Object.Instantiate(togglePrefab, modMenuContent);
            SetNavigationTransform(tempGO);
            firstSelectable = SetOptionProviderToggle(tempGO, (int)ModMenuOptions.FasterTransitions);
            FixLocalizedFont(tempGO);
            SetHeight(tempGO, MenuConstants.optionSelectorHeight);
            tempGO.SetActive(true);
            CreateDescriptionsIfNeeded(ModMenu.fasterMenuTransitions);

            // Create descriptions option
            if (!createdOptionProviders) OptionsProvider.OptionProviders.TryAdd((OptionsProvider.Option)ModMenuOptions.ShowOptionDescriptions,
                    new OptionsProvider.ToggleOptionProvider(
                        BeautifyString("Show Option Descriptions"),
                        () => ModMenu.showOptionDescriptions.Value,
                        SetShowOptionDescriptions
                    )
                );

            tempGO = UnityEngine.Object.Instantiate(togglePrefab, modMenuContent);
            SetNavigationTransform(tempGO);
            optionDescriptionSelectable = SetOptionProviderToggle(tempGO, (int)ModMenuOptions.ShowOptionDescriptions);
            FixLocalizedFont(tempGO);
            SetHeight(tempGO, MenuConstants.optionSelectorHeight);
            tempGO.SetActive(true);
            CreateDescriptionsIfNeeded(ModMenu.showOptionDescriptions);



            // Generate fields for existing plugin configs
            int currentConfigIndex = (int)ModMenuOptions.AutoCreatedOptions;
            foreach (string pluginGUID in ConfigFinder.pluginGUIDs)
            {
                if (ConfigFinder.pluginConfigFiles.TryGetValue(pluginGUID, out ConfigFile pluginConfigFile) && pluginConfigFile.Count > 0)
                {
                    // Create texts for the name of the plugin
                    tempGO = UnityEngine.Object.Instantiate(labelPrefab, modMenuContent);
                    SetLabelText(tempGO);
                    FixLocalizedFont(tempGO);
                    SetHeight(tempGO, MenuConstants.titleLabelTopMargin);
                    tempGO.SetActive(true);

                    tempGO = UnityEngine.Object.Instantiate(labelPrefab, modMenuContent);
                    SetLabelText(tempGO, PadStringWithLines(ConfigFinder.pluginMetadata[pluginGUID].Name), HorizontalAlignmentOptions.Center);
                    SetHeight(tempGO, MenuConstants.titleLabelHeight);
                    FixLocalizedFont(tempGO);
                    tempGO.SetActive(true);


                    // GROUP OPTIONS INTO SECTIONS
                    List<ConfigDefinition> sortedDefinitions = pluginConfigFile.Keys.ToList();
                    sortedDefinitions.Sort((a, b) =>
                    {
                        if (a.Section == "General" && b.Section != "General") return -1;
                        if (b.Section == "General" && a.Section != "General") return 1;
                        return a.Section.CompareTo(b.Section);
                    });
                    string currentSectionName = "General";

                    // Get plugin entries and make options for them
                    foreach (ConfigDefinition definition in sortedDefinitions)
                    {
                        // Create section text if there is a section
                        if (definition.Section != currentSectionName)
                        {
                            tempGO = UnityEngine.Object.Instantiate(labelPrefab, modMenuContent);
                            SetLabelText(tempGO);
                            FixLocalizedFont(tempGO);
                            SetHeight(tempGO, MenuConstants.configSectionTopMargin);
                            tempGO.SetActive(true);

                            tempGO = UnityEngine.Object.Instantiate(labelPrefab, modMenuContent);
                            SetLabelText(tempGO, definition.Section, HorizontalAlignmentOptions.Right);
                            SetHeight(tempGO, MenuConstants.configSectionHeight);
                            FixLocalizedFont(tempGO);
                            tempGO.SetActive(true);

                            currentSectionName = definition.Section;
                        }


                        // Process config entry
                        ConfigEntryBase config = pluginConfigFile[definition];

                        // Bool option type stuff
                        if (config.SettingType == typeof(bool))
                        {
                            if (!createdOptionProviders) OptionsProvider.OptionProviders.TryAdd((OptionsProvider.Option)currentConfigIndex,
                                new OptionsProvider.ToggleOptionProvider(
                                    BeautifyString(UnCamelCase(definition.Key)),//Name
                                    () => (bool)config.BoxedValue,      //Getter
                                    (b) => config.BoxedValue = b        //Setter
                                )
                            );

                            tempGO = UnityEngine.Object.Instantiate(togglePrefab, modMenuContent);
                            SetNavigationTransform(tempGO);
                            SetOptionProviderToggle(tempGO, currentConfigIndex);
                            SetHeight(tempGO, MenuConstants.optionSelectorHeight);
                            FixLocalizedFont(tempGO);
                            tempGO.SetActive(true);
                        }
                        else if (config.SettingType == typeof(string))
                        {
                            // only cosmetic lol
                            if (!createdOptionProviders) OptionsProvider.OptionProviders.TryAdd((OptionsProvider.Option)currentConfigIndex,
                                new OptionsProvider.ToggleOptionProvider(BeautifyString(UnCamelCase(definition.Key)), () => false, (b) => { }));

                            tempGO = UnityEngine.Object.Instantiate(inputPrefab, modMenuContent);
                            SetNavigationTransform(tempGO);
                            SetOptionProviderInput(tempGO, config, currentConfigIndex);
                            SetHeight(tempGO, MenuConstants.optionSelectorHeight);
                            FixLocalizedFont(tempGO);
                            tempGO.SetActive(true);
                        }
                        // Generic stuff
                        else
                        {
                            if (!createdOptionProviders)
                            {
                                AcceptableValueBase acceptable = config.Description.AcceptableValues;

                                // Acceptable value list stuff
                                if (acceptable != null && acceptable.GetType().IsGenericType && acceptable.GetType().GetGenericTypeDefinition() == typeof(AcceptableValueList<>) && !createdOptionProviders)
                                {
                                    acceptable.CreateListOptionProvider(config, definition, currentConfigIndex);
                                }

                                // Acceptable value range stuff
                                if (acceptable != null && acceptable.GetType().IsGenericType && acceptable.GetType().GetGenericTypeDefinition() == typeof(AcceptableValueRange<>) && !createdOptionProviders)
                                {
                                    acceptable.CreateRangeOptionProvider(config, definition, currentConfigIndex);
                                }
                            }

                            // Create GameObject
                            if (OptionsProvider.OptionProviders.ContainsKey((OptionsProvider.Option)currentConfigIndex))
                            {
                                tempGO = UnityEngine.Object.Instantiate(selectorPrefab, modMenuContent);
                                SetNavigationTransform(tempGO);
                                SetOptionProviderSelector(tempGO, currentConfigIndex);
                                SetHeight(tempGO, MenuConstants.optionSelectorHeight);
                                FixLocalizedFont(tempGO);
                                tempGO.SetActive(true);
                            }
                            else
                            {
                                // Don't make descriptions for configs that cannot be processed
                                currentConfigIndex++;
                                continue;
                            }
                        }


                        // Increment config index
                        currentConfigIndex++;

                        // Create description lines if enabled
                        CreateDescriptionsIfNeeded(config);
                    }
                }
            }

            if (!createdOptionProviders)
            {
                ModMenu.Logger.LogInfo("Successfully created option providers for all plugin configs.");
                createdOptionProviders = true;
            }

            // Scroll to top
            LayoutRebuilder.ForceRebuildLayoutImmediate(modMenuContent as RectTransform);
            if (scroll) scroll.verticalNormalizedPosition = 1;

            ModMenu.Logger.LogInfo("Successfully built mod menu.");
        }

        // private static void CreateRangedOptionProvider<T>(ConfigEntryBase config, ConfigDefinition definition, int providerIndex, AcceptableValueList<T> acceptableValues) where T : IEquatable<T>
        // {
        //     OptionsProvider.OptionProviders.TryAdd((OptionsProvider.Option)providerIndex,
        //         new OptionsProvider.TextOptionProvider(
        //             BeautifyString(UnCamelCase(definition.Key)),
        //             acceptableValues.AcceptableValues.Select((i) => TomlTypeConverter.ConvertToString(config.BoxedValue, config.SettingType)).ToArray(),
        //             false,
        //             () => acceptableValues.AcceptableValues.ToList().IndexOf((T)config.BoxedValue),
        //             (i) =>
        //             {
        //                 // Set to default if index is wrong
        //                 if (i == -1) config.BoxedValue = config.DefaultValue;

        //                 // Set the index
        //                 config.BoxedValue = acceptableValues.AcceptableValues[i];
        //             }
        //         )
        //     );
        // }


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
            return $"<cspace=0.2em>{str.Replace(" ", "<space=0.72em>")}";
        }


        public static string PadStringWithLines(string str)
        {
            return PadStringWithLines(str, MenuConstants.titleLabelLength);
        }

        public static string PadStringWithLines(string str, int length)
        {
            string output = str;
            if (str.Length + 2 < length)
            {
                int numDashesPerSide = (length - str.Length) / 2 - 1;
                output = new string('-', numDashesPerSide) + " " + str + " " + new string('-', numDashesPerSide);
            }
            return output;
        }


        public static void CreateDescriptionsIfNeeded(ConfigEntryBase config)
        {
            if (ModMenu.showOptionDescriptions.Value)
            {
                GameObject tempGO;
                if (!config.Description.Description.IsNullOrWhiteSpace())
                {
                    foreach (string line in SplitLineIntoChunks(config.Description.Description, "\n", MenuConstants.configDescriptionLength))
                    {
                        if (!line.IsNullOrWhiteSpace())
                        {
                            tempGO = UnityEngine.Object.Instantiate(labelPrefab, modMenuContent);
                            SetLabelText(tempGO, line, HorizontalAlignmentOptions.Left);
                            SetHeight(tempGO, MenuConstants.configDescriptionHeight);
                            FixLocalizedFont(tempGO);
                            tempGO.SetActive(true);
                        }
                    }
                    tempGO = UnityEngine.Object.Instantiate(labelPrefab, modMenuContent);
                    SetLabelText(tempGO);
                    FixLocalizedFont(tempGO);
                    SetHeight(tempGO, MenuConstants.configDescriptionBottomMargin);
                    tempGO.SetActive(true);
                }
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
                // tmp.text = $"<mspace=11>//<mspace=17> </mspace><mspace=0.75em>{text}";
                // text.Replace(" ", "<space=0.3em> </space>");
                tmp.text = $"<mspace=11>//<mspace=17> </mspace>" + BeautifyString(text);
            }
        }

        private static void FixLocalizedFont(GameObject gameObject)
        {
            LocalizedFont localizedFont = gameObject.GetComponent<LocalizedFont>();
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
                toggle.OnDisable();
                toggle._optionProvider = null;
                toggle.option = (OptionsProvider.Option)option;
                // Traverse.Create(toggle).Field("option").SetValue((OptionsProvider.Option)option);
                toggle.OnEnable();
            }
            return toggle;
        }

        private static OptionHorizontalSelector SetOptionProviderSelector(GameObject gameObject, int option)
        {
            OptionHorizontalSelector selector = gameObject.GetComponent<OptionHorizontalSelector>();
            if (selector != null)
            {
                selector.OnDisable();
                selector._optionProvider = null;
                selector.option = (OptionsProvider.Option)option;
                // Traverse.Create(selector).Field("option").SetValue((OptionsProvider.Option)option);
                selector.OnEnable();
            }
            return selector;
        }

        private static OptionHorizontalToggle SetOptionProviderInput(GameObject gameObject, ConfigEntryBase config, int option)
        {
            OptionHorizontalToggle toggle = gameObject.GetComponent<OptionHorizontalToggle>();
            if (toggle != null)
            {
                toggle.OnDisable();
                toggle._optionProvider = null;
                toggle.option = (OptionsProvider.Option)option;
                toggle.OnEnable();
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


        public static void SetShowOptionDescriptions(bool enabled)
        {
            ModMenu.showOptionDescriptions.Value = enabled;
            try
            {
                BuildMenu();
                optionDescriptionSelectable?.Select();
            }
            catch (Exception ex)
            {
                ModMenu.Logger.LogInfo($"Failed to rebuild menu! {ex}");
            }
        }

        // private static void SetTextMode(GameObject gameObject, FontStyles style)
        // {
        //     TextMeshProUGUI text = gameObject.transform.Find("ValueGroup/Value").GetComponent<TextMeshProUGUI>();
        //     if (text != null) text.fontStyle = style;
        // }




        public static void GenerateInputPrefab()
        {
            // Create and get objects
            inputPrefab = UnityEngine.Object.Instantiate(togglePrefab, modMenuContent);
            inputPrefab.name = "OptionInput";
            inputPrefab.transform.Find("ValueGroup/LeftArrow").gameObject.SetActive(false);
            inputPrefab.transform.Find("ValueGroup/RightArrow").gameObject.SetActive(false);
            GameObject oldValue = inputPrefab.transform.Find("ValueGroup/Value").gameObject;
            TextMeshProUGUI oldValueText = oldValue.GetComponent<TextMeshProUGUI>();
            oldValue.SetActive(false);
            GameObject inputGO = UnityEngine.Object.Instantiate(GameObject.Find("/JeffBezos/AllenDulles/Canvas Parent/Canvas/Console/Console Input"), inputPrefab.transform.Find("ValueGroup"));
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
                placeholderText.color = new Color(0.15f, 0, 0, 0.6f);
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
        }
    }
}