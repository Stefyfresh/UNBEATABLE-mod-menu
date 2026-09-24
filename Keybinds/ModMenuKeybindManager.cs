using System;
using System.Collections;
using System.Linq;
using Arcade.UI.Options;
using BepInEx.Configuration;
using FMODUnity;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ModMenu.Keybinds
{
    public class ModMenuKeybindManager : Selectable, ISubmitHandler, IPointerClickHandler, IEventSystemHandler
    {
        public static readonly KeyCode[] modifierKeys = [
            KeyCode.LeftControl,
            KeyCode.LeftAlt,
            KeyCode.LeftShift,
            KeyCode.LeftWindows,
            KeyCode.LeftCommand,
            KeyCode.RightControl,
            KeyCode.RightAlt,
            KeyCode.RightShift,
            KeyCode.RightWindows,
            KeyCode.RightCommand
        ];
        public static readonly float maxRebindTime = 10;

        public EventReference acceptSound;
        public ArcadeKeybindMenu keybindMenu;
        public TextMeshProUGUI settingText;
        // public KeyCode keySetting;
        public ConfigEntry<KeyCode> config;


        public void Init(EventReference acceptSound, ArcadeKeybindMenu keybindMenu, TextMeshProUGUI settingText, ConfigEntry<KeyCode> config)
        {
            this.acceptSound = acceptSound;
            this.keybindMenu = keybindMenu;
            this.settingText = settingText;
            this.config = config;
            settingText.text = MenuBuilder.BeautifyString(MenuBuilder.UnCamelCase(config.Value.ToString()));
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            StartListening();
        }

        public void OnSubmit(BaseEventData eventData)
        {
            StartListening();
        }

        private void StartListening()
        {
            SelectIfNoneSelected.elementToSelect = gameObject;
            if (EventSystem.current)
            {
                EventSystem.current.firstSelectedGameObject = gameObject;
            }
            JeffBezosController.instance.DisableUIInputs();
            RuntimeManager.PlayOneShot(acceptSound, default(Vector3));
            keybindMenu.rewired.controllers.maps.SetAllMapsEnabled(false);
            StartCoroutine(StartListeningDelayed());
        }

        private IEnumerator StartListeningDelayed()
        {
            yield return new WaitForSeconds(0.1f);
            settingText.text = MenuBuilder.BeautifyString("Rebinding");

            bool foundKey = false;
            float bindTime = 0;
            int prevNumDots = 0;
            while (!foundKey && bindTime < maxRebindTime)
            {
                int numDots = (int)(bindTime * 1.5) % 4;
                if (numDots != prevNumDots)
                {
                    prevNumDots = numDots;
                    settingText.text = MenuBuilder.BeautifyString("Rebinding" + new string('.', numDots));
                }

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    config.Value = KeyCode.None;
                    break;
                }

                if (Input.anyKeyDown)
                {
                    foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
                    {
                        if (Input.GetKeyDown(key) && !modifierKeys.Contains(key))
                        {
                            config.Value = key;
                            foundKey = true;
                            break;
                        }
                    }
                }

                bindTime += Time.deltaTime;
                yield return null;
            }

            JeffBezosController.instance.EnableUIInputs();
            ArcadeRewiredManager.UpdateMaps(true);
            FileStorage.SaveSettings();
            settingText.text = MenuBuilder.BeautifyString(MenuBuilder.UnCamelCase(config.Value.ToString()));
        }
    }
}