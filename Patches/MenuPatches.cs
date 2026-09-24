using HarmonyLib;
using UnityEngine;
using System;
using UnityEngine.UI;
using Arcade.UI.SongSelect;
using UI;
using System.Linq;
using TMPro;
using UnityEngine.Localization.PropertyVariants;
using Arcade.UI;
using Arcade.UI.MenuStates;
using Arcade.UI.AnimationSystem;
using BepInEx;

namespace ModMenu.Patches
{
    [HarmonyPatch(typeof(UIFocusOnButton))]
    [HarmonyPatch("Awake")]
    internal class FixMenuSelectables
    {
        static void Postfix(ref UIFocusOnButton __instance)
        {
            if (validObjectsToPatch.Contains(__instance.name)) __instance.selectIfChild = false;
        }
        private static readonly string[] validObjectsToPatch = ["OptionsButton"];
    }



    [HarmonyPatch(typeof(ArcadeSongList))]
    [HarmonyPatch("Awake")]
    internal class ArcadeSongListAwakePatch
    {
        static void Postfix(ref ArcadeSongList __instance)
        {
            try
            {
                // Get existing GameObjects
                GameObject keybindsButtonGO = __instance.transform.Find("ScreenArea/OptionsCorner/FullOptionsMenu/Tabs/Keybinds")?.gameObject;
                Transform interfaceScreen = __instance.transform.Find("ScreenArea/OptionsCorner/FullOptionsMenu/Categories/Interface");
                MenuBuilder.labelPrefab = __instance.transform.Find("ScreenArea/OptionsCorner/FullOptionsMenu/Categories/Keybinds/Viewport/Content/LabelStandard")?.gameObject;
                MenuBuilder.selectorPrefab = __instance.transform.Find("ScreenArea/OptionsCorner/FullOptionsMenu/Categories/Graphics/Viewport/Content/OptionSelector")?.gameObject;
                MenuBuilder.togglePrefab = __instance.transform.Find("ScreenArea/OptionsCorner/FullOptionsMenu/Categories/Graphics/Viewport/Content/OptionToggle")?.gameObject;
                MenuBuilder.originalKeybindsPrefab = __instance.transform.Find("ScreenArea/OptionsCorner/FullOptionsMenu/Categories/Keybinds/Viewport/Content/MoveUpKey")?.gameObject;
                MenuBuilder.buttonPrefab = __instance.transform.Find("ScreenArea/OptionsCorner/FullOptionsMenu/Categories/Keybinds/Viewport/Content/ResetKeybinds")?.gameObject;


                // Make button
                MenuBuilder.modButtonGO = UnityEngine.Object.Instantiate(keybindsButtonGO, keybindsButtonGO.transform.parent);
                MenuBuilder.modButtonGO.name = MenuBuilder.modButtonName;
                MenuBuilder.modButtonGO.transform.SetSiblingIndex(0);
                MenuBuilder.modButtonGO.transform.parent.GetComponent<VerticalLayoutGroup>().spacing = MenuConstants.buttonLineSpacing;
                MenuBuilder.modButtonGO.GetComponent<UnityEngine.EventSystems.EventTrigger>().triggers[0].callback.m_PersistentCalls.m_Calls[4].arguments.boolArgument = false; // silly code to make it not enable the keybinds menu on click
                foreach (TextMeshProUGUI tmp in MenuBuilder.modButtonGO.GetComponentsInChildren<TextMeshProUGUI>(true))
                {
                    tmp.text = "<mspace=11>//<mspace=17> </mspace><cspace=0.35em>mods.";
                    if (tmp.gameObject.GetComponent<GameObjectLocalizer>() is GameObjectLocalizer localizer) localizer.enabled = false;
                }
                LayoutRebuilder.ForceRebuildLayoutImmediate(MenuBuilder.modButtonGO.transform.parent as RectTransform);
                Canvas.ForceUpdateCanvases();

                // Make menu
                MenuBuilder.modMenuGO = UnityEngine.Object.Instantiate(interfaceScreen.gameObject, interfaceScreen.parent);
                MenuBuilder.modMenuGO.name = MenuBuilder.modMenuName;
                MenuBuilder.modMenuGO.GetComponent<ScrollRect>().scrollSensitivity = MenuConstants.modMenuScrollSensitivity;
                MenuBuilder.modMenuContent = MenuBuilder.modMenuGO.transform.Find("Viewport/Content");

                // Faster menu transitions
                // MenuController.transitionsTransform = __instance.transform.Find("Transitions");
                // MenuController.SetFasterMenuTransitions(ModMenu.fasterMenuTransitions.Value);

                // Register the menu transitions
                MenuTransitionsController.optionsTransitionsTransform = __instance.transform.Find("ScreenArea/OptionsCorner/Transitions");
                MenuTransitionsController.RegisterModMenu();


                // Get scroll
                MenuBuilder.scroll = MenuBuilder.modMenuGO.GetComponent<ScrollRect>();

                // Get UI Material
                MenuBuilder.customUIMaterial = __instance.GetComponentInChildren<RawImage>().material;

                // Set spacing
                MenuBuilder.modMenuContent.GetComponent<VerticalLayoutGroup>().spacing = MenuConstants.menuLineSpacing;

                // Set selectable for going back
                MenuBuilder.modMenuGO.GetComponent<UIFocusOnButton>().selectables = [MenuBuilder.modButtonGO];

                ModMenu.Logger.LogInfo("Set relevant parameters for menu GameObjects.");

                // Build menu
                MenuBuilder.BuildMenu(true);
            }
            catch (Exception ex)
            {
                ModMenu.Logger.LogWarning($"Could not create mod menu! {ex}");
            }

        }
    }


    [HarmonyPatch(typeof(OptionsProvider.OptionProvider))]
    [HarmonyPatch("Name", MethodType.Getter)]
    internal class OptionsProviderNamePatch
    {
        static void Postfix(ref OptionsProvider.OptionProvider __instance, ref string __result)
        {
            if (__result.EndsWith("needs an entry in Options!", StringComparison.Ordinal))
            {
                __result = __instance._name;
            }
        }
    }



    [HarmonyPatch(typeof(FMODButton))]
    [HarmonyPatch("OnPointerClick")]
    internal class SelectCorrectSelectablePointer
    {
        static void Postfix(ref FMODButton __instance)
        {
            if (__instance.gameObject.name == MenuBuilder.modButtonGO.name) MenuBuilder.firstSelectable.Select();
        }
    }



    [HarmonyPatch(typeof(FMODButton))]
    [HarmonyPatch("OnSubmit")]
    internal class SelectCorrectSelectable
    {
        static void Postfix(ref FMODButton __instance)
        {
            if (__instance.gameObject.name == MenuBuilder.modButtonGO.name) MenuBuilder.firstSelectable.Select();
        }
    }



    [HarmonyPatch(typeof(MainMenuController))]
    [HarmonyPatch("Start")]
    internal class MainMenuControllerStartPatch
    {
        static void Postfix(ref MainMenuController __instance)
        {
            int numMods = ModMenu.Instance.transform.GetComponents<BaseUnityPlugin>().Length;
            __instance.VersionNumber.text = $"v{Application.version} ({numMods} {(numMods == 1 ? "MOD" : "MODS")} LOADED)";
        }
    }



    [HarmonyPatch(typeof(ArcadeSongList))]
    [HarmonyPatch("Update")]
    internal class MenuUpdatePatch
    {
        static void Postfix()
        {
            // Code to run when menu is active
            if (MenuTransitionsController.showMenu && MenuBuilder.modMenuGO && MenuBuilder.modMenuGO.transform.parent.gameObject.activeInHierarchy)
            {
                // Enable menu screen when menu is active
                bool wantedState = true;
                foreach (Transform child in MenuBuilder.modMenuGO.transform.parent)
                {
                    if (child.gameObject.activeSelf && child.name != MenuBuilder.modMenuName)
                    {
                        wantedState = false;
                    }
                }

                if (!MenuTransitionsController.hasLoaded && wantedState == true)
                {
                    // Move the scrollbar to the top
                    MenuBuilder.modMenuGO.SetActive(true);
                    LayoutRebuilder.ForceRebuildLayoutImmediate(MenuBuilder.modMenuContent as RectTransform);
                    if (MenuBuilder.scroll) MenuBuilder.scroll.verticalNormalizedPosition = 1;

                    MenuTransitionsController.hasLoaded = true;
                }

                if (MenuBuilder.modMenuGO.activeSelf != wantedState) MenuBuilder.modMenuGO.SetActive(wantedState);
            }
        }
    }
}