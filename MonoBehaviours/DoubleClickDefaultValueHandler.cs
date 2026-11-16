using Arcade.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ModMenu.MonoBehaviours
{
    public class DoubleClickDefaultValueHandler : MonoBehaviour, IPointerDownHandler
    {
        public static readonly float doubleClickThreshold = 0.20f;
        private float doubleClickTimer = 0;

        private void Update()
        {
            if (doubleClickTimer > 0) doubleClickTimer -= Time.deltaTime;
            else doubleClickTimer = 0;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (doubleClickTimer < Time.fixedDeltaTime) doubleClickTimer += doubleClickThreshold;
            else HandleDoubleClick();
        }

        private void HandleDoubleClick()
        {
            ArcadeSettingSlider sliderManager = GetComponent<ArcadeSettingSlider>();
            if (sliderManager && sliderManager._optionProvider != null && sliderManager._slider != null)
            {
                // Set to an invalid value so it resets
                sliderManager._optionProvider._setSliderValue(float.PositiveInfinity);
                float defaultValue = sliderManager._optionProvider.GetValue;

                // Reset drag
                sliderManager._slider.interactable = false;
                sliderManager._slider.interactable = true;

                // Update slider
                sliderManager._slider.value = defaultValue;
            }
        }
    }
}