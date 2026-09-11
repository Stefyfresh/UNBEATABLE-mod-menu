using System;
using HarmonyLib;

namespace ModMenu
{
    [HarmonyPatch(typeof(OptionsProvider))]
    [HarmonyPatch("FillOptions")]
    internal class AddMenuOptions
    {
        static void Postfix()
        {
            // *Format: name (string), options (string[]), current value getter (delegate), set value (Action)
            // Audio device
            // OptionsProvider._optionProviders.Add((OptionsProvider.Option)200,
            //     new OptionsProvider.TextOptionProvider(
            //         "Audio Device",
            //         FMODCustomController.GetAudioDeviceNames(),
            //         false,
            //         FMODCustomController.GetCurrentDeviceIndex,
            //         FMODCustomController.SetCurrentDeviceIndex
            //     )
            // );
        }
    }
}