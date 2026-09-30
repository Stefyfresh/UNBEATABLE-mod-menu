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
    [HarmonyPatch(nameof(UIFocusOnButton.Awake))]
    internal class FixMenuSelectables
    {
        static void Postfix(ref UIFocusOnButton __instance)
        {
            if (validObjectsToPatch.Contains(__instance.name)) __instance.selectIfChild = false;
        }
        private static readonly string[] validObjectsToPatch = ["OptionsButton"];
    }



    [HarmonyPatch(typeof(ArcadeSongList))]
    [HarmonyPatch(nameof(ArcadeSongList.Awake))]
    internal class ArcadeSongListAwakePatch
    {
        static void Postfix(ref ArcadeSongList __instance)
        {
            // Initialize and create mod menu
            MenuBuilder.Init(__instance.transform);

            // Create mods text on arcade mode screen
            // TODO: add text to arcade menu "<cspace=0.5em><voffset=-14em><align="left"><space=-41em><size=200%>UNBEATABLE v2.3.1.1 (40 mods loaded)"
            Transform logoText = __instance.transform.Find("ScreenArea/RecurentElements/Logo/Subtitle");
            Transform bpm = __instance.transform.Find("ScreenArea/RecurentElements/BpmCounter");
            if (logoText && bpm)
            {
                TextMeshProUGUI tmp = logoText.GetComponent<TextMeshProUGUI>();
                if (tmp)
                {
                    bpm.localPosition = new Vector3(bpm.localPosition.x, bpm.localPosition.y + 30, bpm.localPosition.z);
                    tmp.maxVisibleCharacters = 9999;
                    tmp.text += $"<br><cspace=0.25em><voffset=-27.5em><align=\"center\"><space=-41em><size=160%>// v{Application.version} ({ModMenu.NumLoadedMods} {(ModMenu.NumLoadedMods == 1 ? "MOD" : "MODS")} LOADED)";
                }
            }
        }
    }


    [HarmonyPatch(typeof(OptionsProvider.OptionProvider))]
    [HarmonyPatch(nameof(OptionsProvider.OptionProvider.Name), MethodType.Getter)]
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
    [HarmonyPatch(nameof(FMODButton.OnPointerClick))]
    internal class SelectCorrectSelectablePointer
    {
        static void Postfix(ref FMODButton __instance)
        {
            if (__instance.gameObject.name == MenuBuilder.modButtonGO.name) MenuBuilder.firstSelectable.Select();
        }
    }



    [HarmonyPatch(typeof(FMODButton))]
    [HarmonyPatch(nameof(FMODButton.OnSubmit))]
    internal class SelectCorrectSelectable
    {
        static void Postfix(ref FMODButton __instance)
        {
            if (__instance.gameObject.name == MenuBuilder.modButtonGO.name) MenuBuilder.firstSelectable.Select();
        }
    }



    [HarmonyPatch(typeof(MainMenuController))]
    [HarmonyPatch(nameof(MainMenuController.Start))]
    internal class MainMenuControllerStartPatch
    {
        static void Postfix(ref MainMenuController __instance)
        {
            __instance.VersionNumber.text = $"v{Application.version} ({ModMenu.NumLoadedMods} {(ModMenu.NumLoadedMods == 1 ? "MOD" : "MODS")} LOADED)";
        }
    }




    [HarmonyPatch(typeof(ArcadeSongList))]
    [HarmonyPatch(nameof(ArcadeSongList.Update))]
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