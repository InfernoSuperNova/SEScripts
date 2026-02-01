using System;
using System.Collections.Generic;
using System.Linq;
using IngameScript.Helper.Log;
using IngameScript.Ship.Components;

namespace IngameScript.SConfig.Database
{
    /// <summary>
    /// Item class.
    /// </summary>
    /// <summary>
    /// Item class.
    /// </summary>
    public static class EnumLookup
{
    
    private static readonly List<Type> isFlags = new List<Type>
    {
        typeof(PropulsionType),
        typeof(LaunchMechanism)
    };
    private static readonly Dictionary<Type, Dictionary<int, string>> enumToString =
        new Dictionary<Type, Dictionary<int, string>>
        {
            {
                typeof(LogLevel),
                new Dictionary<int, string>
                {
                    { (int)LogLevel.Trace, "Trace" },
                    { (int)LogLevel.Debug, "Debug" },
                    { (int)LogLevel.Info, "Info" },
                    { (int)LogLevel.Warning, "Warning" },
                    { (int)LogLevel.Error, "Error" },
                    { (int)LogLevel.Critical, "Critical" },
                    { (int)LogLevel.Highlight, "Highlight" } 
                }
            },
            {
                typeof(GunReloadType),
                new Dictionary<int, string>
                {
                    { (int)GunReloadType.Normal, "None" },
                    { (int)GunReloadType.NeedsCharging, "NeedsCharging" },
                }
            },
            {
                typeof(GunFireType), 
                new Dictionary<int, string>
                {
                    { (int)GunFireType.Normal, "Normal" },
                    { (int)GunFireType.Delay, "Delay" },
                }
            },
            {
                typeof(PayloadType),
                new Dictionary<int, string>
                {
                    { (int)PayloadType.Kinetic, "Kinetic" },
                    { (int)PayloadType.Warhead, "Warhead" },
                    { (int)PayloadType.Nuke, "Nuke" }
                }
            },
            {
                typeof(LaunchType),
                new Dictionary<int, string>
                {
                    { (int)LaunchType.OneAtATime, "OneAtATime" },
                    { (int)LaunchType.Simultaneous, "Simultaneous" },
                    { (int)LaunchType.Staggered, "Staggered" }
                }
            },
            {
                typeof(MissileCommandContext),
                new Dictionary<int, string>
                {
                    { (int)MissileCommandContext.Automatic, "Automatic" },
                    { (int)MissileCommandContext.Manual, "Manual" },
                    { (int)MissileCommandContext.Ciws, "ManualCiwsOverride" }
                }
            },
            {
                typeof(RefuelPriority),
                new Dictionary<int, string>
                {
                    { (int)RefuelPriority.Low, "Low" },
                    { (int)RefuelPriority.Medium, "Medium" },
                    { (int)RefuelPriority.High, "High" }
                }
            },
            {
                typeof(PropulsionType),
                new Dictionary<int, string>
                {
                    { (int)PropulsionType.None, "None" },
                    { (int)PropulsionType.Hydrogen, "Hydrogen" },
                    { (int)PropulsionType.Ion, "Ion" },
                    { (int)PropulsionType.Atmospheric, "Atmospheric" }
                }
            },
            {
                typeof(LaunchMechanism),
                new Dictionary<int, string>
                {
                    { (int)LaunchMechanism.None, "None" },
                    { (int)LaunchMechanism.Mechanical, "Mechanical" },
                    { (int)LaunchMechanism.Connector, "Connector" },
                    { (int)LaunchMechanism.MergeBlock, "MergeBlock" },
                    { (int)LaunchMechanism.PulsedThruster, "PulsedThruster" },
                    { (int)LaunchMechanism.Weapon, "Weapon" }
                }
            },
            {
                typeof(Behavior),
                new Dictionary<int, string>
                {
                    { (int)Behavior.Interdict, "Interdict" },
                    { (int)Behavior.DirectAttack, "DirectAttack" },
                    { (int)Behavior.SurroundAndClose, "SurroundAndClose" },
                    { (int)Behavior.Defend, "Defend" }
                }
            }
        };

    // Built automatically
    private static readonly Dictionary<Type, Dictionary<string, int>> stringToEnum =
        new Dictionary<Type, Dictionary<string, int>>();

    // Build reverse tables on startup
    static EnumLookup()
    {
        /// <summary>
        /// foreach method.
        /// </summary>
        /// <param name="enumToString">The enumToString parameter.</param>
        /// <returns>The result of the operation.</returns>
        /// <summary>
        /// foreach method.
        /// </summary>
        /// <param name="enumToString">The enumToString parameter.</param>
        /// <returns>The result of the operation.</returns>
        foreach (var pair in enumToString)
        {
            Type type = pair.Key;
            Dictionary<int, string> forward = pair.Value;

            var reverse = new Dictionary<string, int>(StringComparer.Ordinal);

            foreach (var kvp in forward)
            {
                // kvp.Key   = int value
                // kvp.Value = name
                reverse[kvp.Value] = kvp.Key;
            }

            stringToEnum[type] = reverse;
        }
    }

    // Forward: enum → string
        /// <summary>
        /// GetName method.
        /// </summary>
        /// <param name="value">The value parameter.</param>
        /// <returns>The result of the operation.</returns>
        /// <summary>
        /// GetName method.
        /// </summary>
        /// <param name="value">The value parameter.</param>
        /// <returns>The result of the operation.</returns>
    public static string GetName<T>(T value) where T : struct
    {
        Type type = typeof(T);
        int intVal = Convert.ToInt32(value);

        Dictionary<int, string> map;
        /// <summary>
        /// if method.
        /// </summary>
        /// <param name="enumToString.TryGetValue(type">The enumToString.TryGetValue(type parameter.</param>
        /// <param name="map">The map parameter.</param>
        /// <returns>The result of the operation.</returns>
        /// <summary>
        /// if method.
        /// </summary>
        /// <param name="enumToString.TryGetValue(type">The enumToString.TryGetValue(type parameter.</param>
        /// <param name="map">The map parameter.</param>
        /// <returns>The result of the operation.</returns>
        if (enumToString.TryGetValue(type, out map))
        {
            // Check if this is a flags enum
            if (IsFlags(type))
            {
                // Handle flags enum - combine multiple values with " | "
                List<string> flagNames = new List<string>();

                foreach (var kvp in map)
                {
                    int flagValue = kvp.Key;
                    // Skip "None" value (0) if other flags are set
                    if (flagValue == 0 && intVal != 0)
                        continue;

                    // Check if this flag is set in the value
                    if (flagValue != 0 && (intVal & flagValue) == flagValue)
                    {
                        flagNames.Add(kvp.Value);
                    }
                    else if (flagValue == 0 && intVal == 0)
                    {
                        // Only return "None" if the value is exactly 0
                        return kvp.Value;
                    }
                }

                if (flagNames.Count > 0)
                    return string.Join(" | ", flagNames);
            }
            else
            {
                // Regular enum - direct lookup
                string s;
                if (map.TryGetValue(intVal, out s))
                    return s;
            }
        }

        return intVal.ToString();
    }

    // Reverse: string → int → enum
        /// <summary>
        /// TryGetValue method.
        /// </summary>
        /// <param name="name">The name parameter.</param>
        /// <param name="value">The value parameter.</param>
        /// <returns>The result of the operation.</returns>
        /// <summary>
        /// TryGetValue method.
        /// </summary>
        /// <param name="name">The name parameter.</param>
        /// <param name="value">The value parameter.</param>
        /// <returns>The result of the operation.</returns>
    public static bool TryGetValue<T>(string name, out T value) where T : struct
    {
        Type type = typeof(T);

        Dictionary<string, int> map;
        if (stringToEnum.TryGetValue(type, out map))
        {
            // Check if this is a flags enum and the string contains " | "
            if (IsFlags(type) && name.Contains(" | "))
            {
                // Parse multiple flags separated by " | "
                string[] parts = name.Split(new[] { " | " }, StringSplitOptions.None);
                int combinedValue = 0;
                bool allValid = true;

                foreach (string part in parts)
                {
                    string trimmedPart = part.Trim();
                    int flagValue;
                    if (map.TryGetValue(trimmedPart, out flagValue))
                    {
                        combinedValue |= flagValue;
                    }
                    else
                    {
                        allValid = false;
                        break;
                    }
                }

                if (allValid)
                {
                    value = (T)Enum.ToObject(type, combinedValue);
                    return true;
                }
                // If parsing fails, then instead return the default value
                value = default(T);
                return false; 
            }
            else
            {
                // Regular enum or single flag value - direct lookup
                int raw;
                if (map.TryGetValue(name, out raw))
                {
                    value = (T)Enum.ToObject(type, raw);
                    return true;
                }
                value = default(T);
                return false;
            }
        }

        value = default(T);
        return false;
    }

    public static bool IsFlags(Type type)
    {
        return isFlags.Contains(type);
    }

    public static string[] GetNames(Type type)
    {
        Dictionary<int, string> map;
        /// <summary>
        /// TryGetValue method.
        /// </summary>
        /// <param name="type">The type parameter.</param>
        /// <param name="map">The map parameter.</param>
        /// <returns>The result of the operation.</returns>
        /// <summary>
        /// TryGetValue method.
        /// </summary>
        /// <param name="type">The type parameter.</param>
        /// <param name="map">The map parameter.</param>
        /// <returns>The result of the operation.</returns>
        return enumToString.TryGetValue(type, out map) ? map.Values.ToArray() : Array.Empty<string>();
    }
}


}