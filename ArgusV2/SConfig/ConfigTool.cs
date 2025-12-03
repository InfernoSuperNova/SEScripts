using System;
using System.Collections.Generic;
using IngameScript.Helper;
using VRage.Game.ModAPI.Ingame.Utilities;

namespace IngameScript.SConfig
{
    public class ConfigTool
    {
        private static readonly Dictionary<string, ConfigTool> Configs = new Dictionary<string, ConfigTool>();

        public delegate void ConfigSync(Dictionary<string, object> dict);
        public ConfigSync Sync { get; set; }

        public ConfigTool(string name, string comment)
        {
            Name = name;
            Comment = comment;
            Configs.Add(name, this);
        }

        public string Name { get; }
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
                    dict[kv.Key] = new Dictionary<string, object>();
                kv.Value.Sync?.Invoke((Dictionary<string, object>)dict[kv.Key]); // sync fields < - > dictionary
            }

            return Dwon.Serialize(parsed);
        }
    }
}