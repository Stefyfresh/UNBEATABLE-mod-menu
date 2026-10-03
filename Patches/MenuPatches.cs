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
            if (ModMenu.showVersionText.Value)
            {
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
            if (ModMenu.showVersionText.Value) __instance.VersionNumber.text = $"v{Application.version} ({ModMenu.NumLoadedMods} {(ModMenu.NumLoadedMods == 1 ? "MOD" : "MODS")} LOADED)";
        }
    }



    [HarmonyPatch(typeof(ArcadeSongList))]
    [HarmonyPatch(nameof(ArcadeSongList.Update))]
    internal class MenuUpdatePatch
    {
        static void Postfix()
        {
            // Code to run when menu is active
            if (CustomMenuController.showMenu && MenuBuilder.modMenuGO && MenuBuilder.modMenuGO.transform.parent.gameObject.activeInHierarchy)
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

                if (!CustomMenuController.hasLoaded && wantedState == true)
                {
                    // Move the scrollbar to the top
                    MenuBuilder.modMenuGO.SetActive(true);
                    LayoutRebuilder.ForceRebuildLayoutImmediate(MenuBuilder.modMenuContent as RectTransform);
                    if (MenuBuilder.scroll) MenuBuilder.scroll.verticalNormalizedPosition = 1;

                    CustomMenuController.hasLoaded = true;
                }

                if (MenuBuilder.modMenuGO.activeSelf != wantedState) MenuBuilder.modMenuGO.SetActive(wantedState);
            }
        }
    }



#pragma warning disable Harmony003
    [HarmonyPatch(typeof(CustomUINavigation))]
    [HarmonyPatch(nameof(CustomUINavigation.FindSelectableAutomatic))]
    internal class CustomUIOnMovePostfix
    {
        static bool Prefix(ref CustomUINavigation __instance, ref Selectable __result, Vector3 dir, bool wrapAround, Selectable[] selectableList, Transform childsOf)
        {
            if (CustomMenuController.showMenu && MenuBuilder.modMenuGO && MenuBuilder.modMenuGO.transform.parent.gameObject.activeInHierarchy)
            {
                float lowestDistance = float.NegativeInfinity;
                float lowestDistanceWrap = float.NegativeInfinity;
                Selectable closestSelectable = null;
                Selectable wrapAroundSelectable = null;
                foreach (Selectable checkSelectable in selectableList ?? Selectable.allSelectablesArray)
                {
                    if (checkSelectable && !(checkSelectable == __instance._owner) && checkSelectable.IsInteractable())
                    {
                        if (checkSelectable.navigation.mode == Navigation.Mode.None)
                        {
                            CustomUINavigation component = checkSelectable.GetComponent<CustomUINavigation>();
                            if (!component || (component.upNavigation.navigationMode == CustomUINavigation.NavigationMode.None && component.downNavigation.navigationMode == CustomUINavigation.NavigationMode.None && component.leftNavigation.navigationMode == CustomUINavigation.NavigationMode.None && component.rightNavigation.navigationMode == CustomUINavigation.NavigationMode.None))
                            {
                                continue;
                            }
                        }
                        if (!childsOf || (checkSelectable.transform.IsChildOf(childsOf) && !(checkSelectable.transform == childsOf)))
                        {
                            RectTransform rect = checkSelectable.transform as RectTransform;
                            Vector3 distanceVector = checkSelectable.transform.TransformPoint(Vector3.zero) - __instance.transform.TransformPoint(CustomUINavigation.GetPointOnRectEdge(__instance.transform as RectTransform, Quaternion.Inverse(__instance.transform.rotation) * dir.normalized));
                            distanceVector.x = 0;
                            distanceVector.z = 0;
                            float distanceInSearchDir = Vector3.Dot(dir, distanceVector);
                            if (wrapAround && distanceInSearchDir < 0f)
                            {
                                float distance = -distanceInSearchDir * distanceVector.sqrMagnitude;
                                if (distance > lowestDistanceWrap)
                                {
                                    lowestDistanceWrap = distance;
                                    wrapAroundSelectable = checkSelectable;
                                }
                            }
                            else if (distanceInSearchDir > 0f)
                            {
                                float distance = distanceInSearchDir / distanceVector.sqrMagnitude;
                                if (distance > lowestDistance)
                                {
                                    lowestDistance = distance;
                                    closestSelectable = checkSelectable;
                                }
                            }
                        }
                    }
                }
                if (wrapAround && closestSelectable == null) __result = wrapAroundSelectable;
                __result = closestSelectable;
                return false;
            }
            return true;
        }
    }
}