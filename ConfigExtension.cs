using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
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
            try
            {
                DoCreateListOptionProvider((dynamic)instance, config, definition, providerIndex);
            }
            catch (Exception ex)
            {
                ModMenu.Logger.LogWarning($"Error creating option provider for \"{definition.Key}\": {ex}");
            }
        }
        private static void DoCreateListOptionProvider<T>(AcceptableValueList<T> acceptableValues, ConfigEntryBase config, ConfigDefinition definition, int providerIndex) where T : IEquatable<T>
        {

            OptionsProvider.OptionProviders[(OptionsProvider.Option)providerIndex] =
                new OptionsProvider.TextOptionProvider(
                    TextUtils.BeautifyString(TextUtils.UnCamelCase(definition.Key)),
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


        // Creates an option provider for an AcceptableValueRange type
        public static void CreateRangeOptionProvider(this AcceptableValueBase instance, ConfigEntryBase config, ConfigDefinition definition, int providerIndex)
        {
            try
            {
                DoCreateRangeOptionProvider((dynamic)instance, config, definition, providerIndex);
            }
            catch (Exception ex)
            {
                ModMenu.Logger.LogWarning($"Error creating option provider for \"{definition.Key}\": {ex}");
            }
        }
        private static void DoCreateRangeOptionProvider<T>(AcceptableValueRange<T> acceptableValues, ConfigEntryBase config, ConfigDefinition definition, int providerIndex) where T : IComparable
        {
            bool isInt = config.SettingType == typeof(int);
            float minVal = Convert.ToSingle(acceptableValues.MinValue);
            float maxVal = Convert.ToSingle(acceptableValues.MaxValue);
            float stepSize = Mathf.Abs(maxVal - minVal) / 10f;
            OptionsProvider.OptionProviders[(OptionsProvider.Option)providerIndex] = new OptionsProvider.SliderOptionProvider(
                TextUtils.BeautifyString(TextUtils.UnCamelCase(definition.Key)),
                new Vector2(minVal, maxVal),
                // isInt ? Math.Min(stepSize, 1) : stepSize,
                isInt ? 1 : 0,
                () => Convert.ToSingle(config.BoxedValue),
                (val) =>
                {
                    try
                    {
                        // ModMenu.Logger.LogInfo(val);
                        // ModMenu.Logger.LogInfo(Environment.StackTrace);
                        if (isInt) config.BoxedValue = Convert.ToInt32(val);
                        else config.BoxedValue = val;
                    }
                    catch (Exception ex)
                    {
                        ModMenu.Logger.LogWarning($"Error in updating value for config \"{definition.Key}\": {ex}");
                    }
                }
            );
        }


        // Creates an option provider for an enum type
        public static void CreateEnumOptionProvider(this ConfigEntryBase config, int providerIndex, BepInPlugin plugin = null)
        {
            try
            {
                ConfigDefinition definition = config.Definition;

                // List<T> values = Enum.GetValues(typeof(T)).OfType<T>().ToList();
                List<int> numbers = Enum.GetValues(config.SettingType).Cast<int>().ToList();
                List<string> names = Enum.GetNames(config.SettingType).Select(TextUtils.UnCamelCase).ToList();

                // Bitflag things
                if (config.SettingType.GetCustomAttributes(typeof(FlagsAttribute), inherit: true).Any())
                {
                    // Find last power of 2 and get the next 2^n - 1
                    int power = numbers.FindLast((n) => n % 2 == 0) * 2 - 1;
                    int start = numbers.First();
                    numbers = [];
                    names = [];

                    for (int i = start; i <= power; i++)
                    {
                        // T val = (T)Enum.Parse(typeof(T), $"{i}");
                        numbers.Add(i);
                        names.Add(TextUtils.UnCamelCase(Enum.Format(config.SettingType, i, "F")));
                    }
                }

                OptionsProvider.OptionProviders[(OptionsProvider.Option)providerIndex] =
                    new OptionsProvider.TextOptionProvider(
                        TextUtils.BeautifyString(TextUtils.UnCamelCase(definition.Key)),
                        names.ToArray(),
                        false,
                        () => numbers.IndexOf((int)config.BoxedValue),
                        (i) =>
                        {
                            try
                            {
                                // Set to default if index is wrong
                                if (i < 0 || i >= numbers.Count())
                                {
                                    config.BoxedValue = config.DefaultValue;
                                    return;
                                }

                                // Set the index
                                config.BoxedValue = numbers[i];
                            }
                            catch (Exception ex)
                            {
                                ModMenu.Logger.LogWarning($"Error in updating value for config \"{definition.Key}\": {ex}");
                            }
                        }
                    );
            }
            catch (Exception ex)
            {
                ModMenu.Logger.LogWarning($"Error creating option provider for \"{config.Definition.Key}\": {ex}");
            }
        }
    }
}