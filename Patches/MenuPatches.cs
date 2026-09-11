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
                GameObject keybindsButtonGO = __instance.transform.Find("ScreenArea/OptionsCorner/FullOptionsMenu/Tabs/Keybinds").gameObject;
                Transform interfaceScreen = __instance.transform.Find("ScreenArea/OptionsCorner/FullOptionsMenu/Categories/Interface");
                // MenuBuilder.chartOffsetGO = __instance.transform.Find("Categories/Gameplay/Viewport/Content/OptionSelector (14)").gameObject;
                MenuBuilder.labelPrefab = __instance.transform.Find("ScreenArea/OptionsCorner/FullOptionsMenu/Categories/Keybinds/Viewport/Content/LabelStandard").gameObject;
                MenuBuilder.selectorPrefab = __instance.transform.Find("ScreenArea/OptionsCorner/FullOptionsMenu/Categories/Graphics/Viewport/Content/OptionSelector").gameObject;
                MenuBuilder.togglePrefab = __instance.transform.Find("ScreenArea/OptionsCorner/FullOptionsMenu/Categories/Graphics/Viewport/Content/OptionToggle").gameObject;



                // Make new GameObjects
                MenuBuilder.modButtonGO = UnityEngine.Object.Instantiate(keybindsButtonGO, keybindsButtonGO.transform.parent);
                MenuBuilder.modButtonGO.name = MenuBuilder.modButtonName;
                MenuBuilder.modButtonGO.transform.SetSiblingIndex(0);

                MenuBuilder.modMenuGO = UnityEngine.Object.Instantiate(interfaceScreen.gameObject, interfaceScreen.parent);
                MenuBuilder.modMenuGO.name = MenuBuilder.modMenuName;
                MenuBuilder.modMenuGO.GetComponent<ScrollRect>().scrollSensitivity = MenuConstants.modMenuScrollSensitivity;
                MenuBuilder.modMenuContent = MenuBuilder.modMenuGO.transform.Find("Viewport/Content");

                // Get scroll
                MenuBuilder.scroll = MenuBuilder.modMenuGO.GetComponent<ScrollRect>();

                // Get UI Material
                MenuBuilder.customUIMaterial = __instance.GetComponentInChildren<RawImage>().material;

                // Set spacing
                MenuBuilder.modMenuContent.GetComponent<VerticalLayoutGroup>().spacing = MenuConstants.menuLineSpacing;

                // Set selectable for going back
                MenuBuilder.modMenuGO.GetComponent<UIFocusOnButton>().selectables = [MenuBuilder.modButtonGO];

                // Build menu
                MenuBuilder.BuildMenu();


                // Set component values
                MenuBuilder.modButtonGO.transform.parent.GetComponent<VerticalLayoutGroup>().spacing = MenuConstants.buttonLineSpacing;
                MenuBuilder.modButtonGO.GetComponent<UnityEngine.EventSystems.EventTrigger>().triggers[0].callback.m_PersistentCalls.m_Calls[4].arguments.boolArgument = false;
                LayoutRebuilder.ForceRebuildLayoutImmediate(MenuBuilder.modButtonGO.transform.parent as RectTransform);
                Canvas.ForceUpdateCanvases();

                // Rename the mods button
                foreach (TextMeshProUGUI tmp in MenuBuilder.modButtonGO.GetComponentsInChildren<TextMeshProUGUI>(true))
                {
                    tmp.text = "<mspace=11>//<mspace=17> </mspace><cspace=0.35em>mods.";
                    if (tmp.gameObject.GetComponent<GameObjectLocalizer>() is GameObjectLocalizer thing) thing.enabled = false;
                }

                // Faster menu transitions
                MenuController.transitionsTransform = __instance.transform.Find("Transitions");
                MenuController.SetFasterMenuTransitions(ModMenu.fasterMenuTransitions.Value);

                // Register the menu transitions
                MenuController.optionsTransitionsTransform = __instance.transform.Find("ScreenArea/OptionsCorner/Transitions");
                MenuController.RegisterModMenu();

                ModMenu.Logger.LogInfo("Set relevant parameters for menu GameObjects.");
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
}