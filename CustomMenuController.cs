using Arcade.UI.AnimationSystem;
using Arcade.UI.MenuStates;
using Arcade.UI.SongSelect;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace ModMenu
{
    public static class CustomMenuController
    {
        public static bool showMenu;
        public static bool hasLoaded;
        public static Transform optionsTransitionsTransform;


        public static void RegisterModMenu()
        {
            if (optionsTransitionsTransform)
            {
                UITransition enterTransition = optionsTransitionsTransform.Find("None-Options")?.GetComponent<UITransition>();
                enterTransition.elementToSelect = MenuBuilder.modButtonGO;
                enterTransition.OnTransitionFinished += () =>
                {
                    showMenu = true;
                };

                UITransition exitTransition = optionsTransitionsTransform.Find("Options-None")?.GetComponent<UITransition>();
                exitTransition.OnTransitionFinished += () =>
                {
                    showMenu = false;
                    hasLoaded = false;
                    MenuBuilder.modMenuGO.SetActive(false);
                };
            }

            MenuBuilder.modMenuGO.SetActive(false);
        }
    }
}