using System;
using System.Linq;
using BepInEx.Configuration;
using UnityEngine;

namespace ModMenu
{
    public static class ConfigExtension
    {
        // Creates an option provider for an AcceptableValueList type
        public static void CreateListOptionProvider(this AcceptableValueBase instance, ConfigEntryBase config, ConfigDefinition definition, int providerIndex)
        {
            CreateListOptionProvider((dynamic)instance, config, definition, providerIndex);
        }
        private static void CreateListOptionProvider<T>(AcceptableValueList<T> acceptableValues, ConfigEntryBase config, ConfigDefinition definition, int providerIndex) where T : IEquatable<T>
        {
            try
            {
                OptionsProvider.OptionProviders.TryAdd((OptionsProvider.Option)providerIndex,
                    new OptionsProvider.TextOptionProvider(
                        MenuBuilder.BeautifyString(MenuBuilder.UnCamelCase(definition.Key)),
                        acceptableValues.AcceptableValues.Select((i) => TomlTypeConverter.ConvertToString(i, config.SettingType)).ToArray(),
                        false,
                        () => acceptableValues.AcceptableValues.ToList().IndexOf((T)config.BoxedValue),
                        (i) =>
                        {
                            // Set to default if index is wrong
                            if (i < 0 || i >= acceptableValues.AcceptableValues.Count())
                            {
                                config.BoxedValue = config.DefaultValue;
                                return;
                            }

                            // Set the index
                            config.BoxedValue = acceptableValues.AcceptableValues[i];
                        }
                    )
                );
            }
            catch (Exception ex)
            {
                ModMenu.Logger.LogWarning($"Error creating option provider for \"{MenuBuilder.UnCamelCase(definition.Key)}\": {ex}");
            }
        }


        // Creates an option provider for an AcceptableValueRange type
        public static void CreateRangeOptionProvider(this AcceptableValueBase instance, ConfigEntryBase config, ConfigDefinition definition, int providerIndex)
        {
            CreateRangeOptionProvider((dynamic)instance, config, definition, providerIndex);
        }
        private static void CreateRangeOptionProvider<T>(AcceptableValueRange<T> acceptableValues, ConfigEntryBase config, ConfigDefinition definition, int providerIndex) where T : IComparable
        {
            OptionsProvider.OptionProviders.TryAdd((OptionsProvider.Option)providerIndex, new OptionsProvider.TextOptionProvider(
                MenuBuilder.BeautifyString(MenuBuilder.UnCamelCase(definition.Key)),
                [config.GetSerializedValue(), "1", "2"],
                false,
                () => 0,
                (i) =>
                {
                    try
                    {
                        // Increase value
                        if (i == 1)
                        {
                            config.BoxedValue = Convert.ChangeType(Convert.ToDouble(config.BoxedValue) + 1, typeof(T));
                            if (((T)config.BoxedValue).CompareTo(acceptableValues.MaxValue) > 0) config.BoxedValue = acceptableValues.MaxValue;
                        }

                        // Decrease value
                        if (i == 2)
                        {
                            config.BoxedValue = Convert.ChangeType(Convert.ToDouble(config.BoxedValue) - 1, typeof(T));
                            if (((T)config.BoxedValue).CompareTo(acceptableValues.MinValue) < 0) config.BoxedValue = acceptableValues.MinValue;
                        }

                        // Change text
                        if (OptionsProvider.OptionProviders.TryGetValue((OptionsProvider.Option)providerIndex, out OptionsProvider.OptionProvider provider))
                        {
                            if (provider is OptionsProvider.TextOptionProvider textProvider)
                            {
                                textProvider._baseOptions = [config.GetSerializedValue(), "1", "2"];
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ModMenu.Logger.LogWarning($"Error in updating value for config \"{MenuBuilder.UnCamelCase(definition.Key)}\": {ex}");
                    }
                }
                )
            );
        }
    }
}