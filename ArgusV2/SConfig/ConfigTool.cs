using System;
using System.Collections.Generic;
using IngameScript.Helper;
using IngameScript.Helper.IngameScript.Helper;
using VRage.Game.ModAPI.Ingame.Utilities;

namespace IngameScript.SConfig
{
    public class ConfigTool
    {
        private static readonly Dictionary<string, ConfigTool> Configs = new Dictionary<string, ConfigTool>();

        public static string Collect()
        {
            Program.LogLine(Configs.Count);
            var config = new Dictionary<string, object>();
            foreach (var kv in Configs)
            {
                Program.LogLine(kv.Key);
                config.Add(kv.Key, kv.Value.Get());
            }

            return TestDwon();
            //return Dwon.Serialize(config);
        }

        
        
        public static string TestDwon()
        {
            // Build test config
            var config = new Dictionary<string, object>
            {
                { "GeneralConfig", new Dictionary<string, object> {
                    { "GroupName", new Dwon.Comment("ArgusV2", "name of group") },
                    { "MaxWeaponRange", 2000 },
                    { "LockAngle", 40 }
                }},
                { "PID", new Dictionary<string, object> {
                    { "P", 500 },
                    { "I", 0 },
                    { "D", new Dwon.Comment(30, "derivative gain") }
                }},
                { "Weapons", new List<object> { "Railgun", "Laser", "PlasmaCannon" } },
                { "NestedObjects", new Dictionary<string, object> {
                    { "SomeNestedObject", new Dwon.Comment("foobar", "inline comment") },
                    { "AnotherNested", new Dictionary<string, object> {
                        { "X", 1 },
                        { "Y", 2 }
                    }}
                }}
            };

            // Serialize
            string serialized = Dwon.Serialize(config);
            // Parse
            object parsed = Dwon.Parse(serialized);

            // Serialize again for round-trip check
            string serializedAgain = Dwon.Serialize(parsed);
            return serializedAgain;
        }
        
        
        
        
        
        
        
        
        
        public static void Save(string config)
        {
            var parsed = MiniToml.Parse(config);

            foreach (var kv in parsed)
            {
                var dict = kv.Value as Dictionary<string, object>;
                if (dict != null) 
                    Configs[kv.Key].Set(dict);
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