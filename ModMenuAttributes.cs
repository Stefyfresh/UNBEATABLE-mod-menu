/// <summary>
/// A class that specifies how Mod Menu should interpret and display a BepInEx config entry.
/// 
/// Usage:
/// Just like ConfigurationManagerAttributes, this class template has to be copied inside the plugin's project and referenced by its code directly.
/// Make a new instance, assign any fields that you want to override, and pass it into the tags in the ConfigDescription.
/// 
/// </summary>
internal sealed class ModMenuAttributes
{
    /// <summary>
    /// If set, changes the format specifier used for displaying and rounding values in Mod Menu.
    /// Only designed to work with AcceptableValueRange config options.
    /// </summary>
    public string FormatSpecifier;

    /// <summary>
    /// If set, adds a unit suffix text to the end of a numeric value in Mod Menu. 
    /// This does not automatically include a space, so if you would like a value to show up as "1 m" the suffix should be " m".
    /// Only designed to work with AcceptableValueRange config options.
    /// </summary>
    public string UnitSuffix;
}