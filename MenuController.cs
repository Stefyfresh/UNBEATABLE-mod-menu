using Arcade.UI.AnimationSystem;
using Arcade.UI.MenuStates;
using Arcade.UI.SongSelect;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace ModMenu
{
    public static class MenuController
    {
        public static bool showMenu;
        public static bool hasLoaded;
        public static Transform transitionsTransform;
        public static Transform optionsTransitionsTransform;
        private static bool hasMadeFaster;

        public static void SetFasterMenuTransitions(bool faster)
        {
            float speed = 1;
            if (faster && !hasMadeFaster)
            {
                speed = 1.5f;
                ModMenu.Logger.LogInfo("Set increased menu transition speed.");
                hasMadeFaster = true;
            }
            else if (!faster && hasMadeFaster)
            {
                speed = 1 / 1.5f;
                ModMenu.Logger.LogInfo("Set decreased menu transition speed.");
                hasMadeFaster = false;
            }

            foreach (Transform child in transitionsTransform.transform.GetChild(2))
            {
                UITransition trans = child.gameObject.GetComponent<UITransition>();
                if (trans) trans.speed *= speed;
            }
            ModMenu.fasterMenuTransitions.Value = faster;
        }


        public static void RegisterModMenu()
        {
            if (optionsTransitionsTransform)
            {
                UITransition enterTransition = optionsTransitionsTransform.Find("None-Options")?.GetComponent<UITransition>();
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


    [HarmonyPatch(typeof(ArcadeSongList))]
    [HarmonyPatch("Update")]
    internal class MenuUpdatePatch
    {
        static void Postfix()
        {
            // Code to run when menu is active
            if (MenuController.showMenu && MenuBuilder.modMenuGO && MenuBuilder.modMenuGO.transform.parent.gameObject.activeInHierarchy)
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

                if (!MenuController.hasLoaded && wantedState == true)
                {
                    // Move the scrollbar to the top
                    MenuBuilder.modMenuGO.SetActive(true);
                    LayoutRebuilder.ForceRebuildLayoutImmediate(MenuBuilder.modMenuContent as RectTransform);
                    if (MenuBuilder.scroll) MenuBuilder.scroll.verticalNormalizedPosition = 1;

                    MenuController.hasLoaded = true;
                }

                if (MenuBuilder.modMenuGO.activeSelf != wantedState) MenuBuilder.modMenuGO.SetActive(wantedState);
            }
        }
    }
}