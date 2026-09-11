# UNBEATABLE Mod Menu

A mod to add a mod menu to the hit rhythm game UNBEATABLE!

This mod is currently in development, check back soon for more information!

## Features

- Gathers configuration automatically from all BepInEx plugins that are loaded
- Creates an additional category in the Arcade Mode options menu labeled "mods"
- Uses the same UI as the existing menu screens with some slight tweaks for readbility
- Verified to support boolean, number, and string config types, along with acceptable value ranges and lists

## Configuration

The mod menu is accessed from Arcade Mode; simply press "options" on the bottom right (or the J key by default) to enter the Arcade Mode options. Once inside the menu, the "mods" button will open the mod menu!

All entries for different plugins are labelled with a large header defining the plugin, with configuration options below each header.
The section for each BepInEx config entry is also shown, although the [General] section is omitted for clarity and is always shown first.

There are currently two configuration options for the mod menu itself:

1) Faster Menu Transitions - This increases the speed of all in-game transitions by 1.5x (note: I may remove this later and integrate it into a different mod)
2) Show Option Descriptions - This shows more detailed descriptions for every config option, using the descriptions present in the BepInEx config file

## Mod Installation Instructions

- Download the latest release of the mod from the releases page, and extract the DLL file from inside the zip
- Download BepInEx from [here](https://github.com/BepInEx/BepInEx/releases) and extract the BepInEx folder from the zip into the main UNBEATABLE game code folder (the one that contains UNBEATABLE.exe). You must extract ALL the files from that zip into the main UNBEATABLE folder (do NOT make a new folder!)
- Run the game once and close it
- Put the mod DLL into the `BepInEx/plugins` folder

The structure should then be:

<pre>
UNBEATABLE
├─── UNBEATABLE.exe
├─── UNBEATABLE_Data
├─── {some other folders and files}
├─── .doorstop_version
├─── changelog.txt
├─── doorstop_config.ini
├─── winhttp.dll
└─── BepInEx
    ├─── cache
    ├─── config
    ├─── core
    ├─── patchers
    └─── plugins
        ├─── SomeMod.dll
        └─── SomeOtherMod.dll
</pre>

Once the mod is in the folder, restart the game and it should load.
