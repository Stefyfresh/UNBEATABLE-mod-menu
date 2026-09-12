using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using UnityEngine;

namespace ModMenu
{
    // this is the place of pain and suffering and dumb stupid code that somehow works
    public static class ConfigExtension
    {
        // Creates an option provider for an AcceptableValueList type
        public static void CreateListOptionProvider(this AcceptableValueBase instance, ConfigEntryBase config, ConfigDefinition definition, int providerIndex)
        {
            DoCreateListOptionProvider((dynamic)instance, config, definition, providerIndex);
        }
        private static void DoCreateListOptionProvider<T>(AcceptableValueList<T> acceptableValues, ConfigEntryBase config, ConfigDefinition definition, int providerIndex) where T : IEquatable<T>
        {
            try
            {
                OptionsProvider.OptionProviders[(OptionsProvider.Option)providerIndex] =
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
            DoCreateRangeOptionProvider((dynamic)instance, config, definition, providerIndex);
        }
        private static void DoCreateRangeOptionProvider<T>(AcceptableValueRange<T> acceptableValues, ConfigEntryBase config, ConfigDefinition definition, int providerIndex) where T : IComparable
        {
            OptionsProvider.OptionProviders[(OptionsProvider.Option)providerIndex] = new OptionsProvider.TextOptionProvider(
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
            );
        }


        // Creates an option provider for an enum type
        public static void CreateEnumOptionProvider(this ConfigEntryBase config, ConfigDefinition definition, int providerIndex)
        {
            DoCreateEnumOptionProvider((dynamic)config, definition, providerIndex);
        }

        private static void DoCreateEnumOptionProvider<T>(ConfigEntry<T> config, ConfigDefinition definition, int providerIndex)
        {
            try
            {
                List<T> values = Enum.GetValues(typeof(T)).OfType<T>().ToList();
                List<int> numbers = Enum.GetValues(typeof(T)).Cast<int>().ToList();
                List<string> names = Enum.GetNames(config.SettingType).ToList();

                // Bitflag things
                if (config.SettingType.GetCustomAttributes(typeof(FlagsAttribute), inherit: true).Any())
                {
                    // Find last power of 2 and get the next 2^n - 1
                    int power = numbers.FindLast((n) => n % 2 == 0) * 2 - 1;
                    int start = numbers.First();
                    values = [];
                    names = [];

                    for (int i = start; i <= power; i++)
                    {
                        T val = (T)Enum.Parse(typeof(T), $"{i}");
                        values.Add(val);
                        names.Add(val.ToString());
                    }
                }

                OptionsProvider.OptionProviders[(OptionsProvider.Option)providerIndex] =
                    new OptionsProvider.TextOptionProvider(
                        MenuBuilder.BeautifyString(MenuBuilder.UnCamelCase(definition.Key)),
                        names.ToArray(),
                        false,
                        () => values.IndexOf(config.Value),
                        (i) =>
                        {
                            try
                            {
                                // Set to default if index is wrong
                                if (i < 0 || i >= values.Count())
                                {
                                    config.BoxedValue = config.DefaultValue;
                                    return;
                                }

                                // Set the index
                                config.BoxedValue = values[i];
                            }
                            catch (Exception ex)
                            {
                                ModMenu.Logger.LogWarning($"Error in updating value for config \"{MenuBuilder.UnCamelCase(definition.Key)}\": {ex}");
                            }
                        }
                    );
            }
            catch (Exception ex)
            {
                ModMenu.Logger.LogWarning($"Error creating option provider for \"{MenuBuilder.UnCamelCase(definition.Key)}\": {ex}");
            }
        }
    }
}