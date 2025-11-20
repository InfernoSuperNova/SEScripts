using System;
using System.Collections.Generic;
using IngameScript.Helper;
using VRage.Game.ModAPI.Ingame.Utilities;

namespace IngameScript.SConfig
{
    public class ConfigTool
    {
        private static readonly Dictionary<string, ConfigTool> Configs = new Dictionary<string, ConfigTool>();

        public static string SerializeAll()
        {
            Program.LogLine(Configs.Count);
            var config = new Dictionary<string, object>();
            foreach (var kv in Configs)
            {
                Program.LogLine(kv.Key);
                config.Add(kv.Key, kv.Value.Get());
            }

            //return TestDwon();
            return Dwon.Serialize(config);
        }

        public static void DeserializeAll(string config)
        {
            var parsed = Dwon.Parse(config);
            var dict = parsed as Dictionary<string, object>;
            if (dict == null) throw new Exception("Config malformed");
            foreach (var kv in dict)
            {
                ConfigTool config1;
                if (!Configs.TryGetValue(kv.Key, out config1)) continue;
                var dict3 = kv.Value as Dictionary<string, object>;
                if (dict3 != null)
                {
                    config1.Set(dict3);
                }
            }
        }
        
        
        
        public Func<Dictionary<string, object>> Get { get; set; }
        public Action<Dictionary<string, object>> Set { get; set; }

        public ConfigTool(string name, string comment)
        {
            Name = name;
            Comment = comment;
            Configs.Add(name, this);
        }
        
        public string Name { get; }
        public string Comment { get; }
        
        
        public Dictionary<string, object> GetConfig() => Get?.Invoke();
        public void SetConfig( Dictionary<string, object> value) => Set?.Invoke(value);
    }
}