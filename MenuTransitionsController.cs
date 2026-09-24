using Arcade.UI.AnimationSystem;
using Arcade.UI.MenuStates;
using Arcade.UI.SongSelect;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace ModMenu
{
    public static class MenuTransitionsController
    {
        public static bool showMenu;
        public static bool hasLoaded;
        // public static Transform transitionsTransform;
        public static Transform optionsTransitionsTransform;
        // private static bool hasMadeFaster;

        // public static void SetFasterMenuTransitions(bool faster)
        // {
        //     float speed = 1;
        //     if (faster && !hasMadeFaster)
        //     {
        //         speed = 1.5f;
        //         ModMenu.Logger.LogInfo("Set increased menu transition speed.");
        //         hasMadeFaster = true;
        //     }
        //     else if (!faster && hasMadeFaster)
        //     {
        //         speed = 1 / 1.5f;
        //         ModMenu.Logger.LogInfo("Set decreased menu transition speed.");
        //         hasMadeFaster = false;
        //     }

        //     foreach (Transform child in transitionsTransform.transform.GetChild(2))
        //     {
        //         UITransition trans = child.gameObject.GetComponent<UITransition>();
        //         if (trans) trans.speed *= speed;
        //     }
        //     ModMenu.fasterMenuTransitions.Value = faster;
        // }


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
}