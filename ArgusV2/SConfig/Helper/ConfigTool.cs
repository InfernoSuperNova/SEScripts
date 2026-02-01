using System;
using System.Collections.Generic;
using IngameScript.Helper;
using IngameScript.Helper.Log;
using IngameScript.SConfig.Helper;
using VRage.Game.ModAPI.Ingame.Utilities;

namespace IngameScript.SConfig
{
    /// <summary>
    /// Item class.
    /// </summary>
    /// <summary>
    /// Item class.
    /// </summary>
    public class ConfigTool
    {
        /// <summary>
        /// Dictionary method.
        /// </summary>
        /// <returns>The result of the operation.</returns>
        /// <summary>
        /// Dictionary method.
        /// </summary>
        /// <returns>The result of the operation.</returns>
        private static readonly Dictionary<string, ConfigTool> Configs = new Dictionary<string, ConfigTool>();

        public delegate void ConfigSync(Dictionary<string, object> dict);
        /// <summary>
        /// Gets or sets the Sync.
        /// </summary>
        /// <summary>
        /// Gets or sets the Sync.
        /// </summary>
        public ConfigSync Sync { get; set; }

        public ConfigTool(string name, string comment)
        {
            Name = name;
            Comment = comment;
            Configs.Add(name, this);
        }

        public string Name { get; }
        /// <summary>
        /// Gets or sets the Comment.
        /// </summary>
        /// <summary>
        /// Gets or sets the Comment.
        /// </summary>
        public string Comment { get; }

        public static string SyncConfig(string input)
        {
            var parsed = Dwon.Parse(input);
            
            var dict = parsed as Dictionary<string, object>;
            if (dict == null)
            {
                Program.LogLine("Config malformed", LogLevel.Critical);
                throw new Exception();
            }

            foreach (var kv in Configs)
            {
                if (!dict.ContainsKey(kv.Key))
                {
                    // Create new Field with empty dictionary
                    dict[kv.Key] = new Dwon.Field(new Dictionary<string, object>());
                }

                // Unwrap Field to get the dictionary
                var field = dict[kv.Key] as Dwon.Field;
                var configDict = field != null ? field.Obj as Dictionary<string, object> : dict[kv.Key] as Dictionary<string, object>;

                if (configDict != null)
                {
                    kv.Value.Sync?.Invoke(configDict); // sync fields < - > dictionary
                }
            }

            return Dwon.Serialize(parsed);
        }
    }
}