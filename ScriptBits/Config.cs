using System;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    public static class Config
    {
        private static ConfigTool _configTool = new ConfigTool("General Config", "")
        {
            Sync = SyncConfig
        };

        public static readonly GeneralConfig General = new GeneralConfig();
        public static readonly StringConfig String = new StringConfig();
        
        public static void Setup(IMyProgrammableBlock me)
        {
            me.CustomData = ConfigTool.SyncConfig(me.CustomData);
        }

        private static void SyncConfig(Dictionary<string, object> obj)
        {
            General.Sync(obj);
            String.Sync(obj);
        }

        public class GeneralConfig
        {
            public LogLevel LogLevel = LogLevel.Trace;
            public double MaxWeaponRange = 2000;
            public double GridSpeedLimit = 104;
            public double MaxAngularVelocityRpm = 30;
            
            internal void Sync(Dictionary<string, object> obj)
            {
                var config = ConfigCategory.From(obj, "General Config");
                
                config.SyncEnum("LogLevel", ref LogLevel);
                config.Sync("MaxWeaponRange", ref MaxWeaponRange);
                config.Sync("GridSpeedLimit", ref GridSpeedLimit);
                config.Sync("MaxAngularVelocityRPM", ref MaxAngularVelocityRpm);
            }
        }
        public class StringConfig
        {
            public string ArgumentTarget = "Target";
            public string ArgumentUnTarget = "Untarget";
            public string GroupName = "ArgusV2";
            
            internal void Sync(Dictionary<string, object> obj)
            {
                var config = ConfigCategory.From(obj, "String Config");
                
                config.Sync("ArgumentTarget", ref ArgumentTarget);
                config.Sync("ArgumentUnTarget", ref ArgumentUnTarget);
                config.Sync("GroupName", ref GroupName);
            }
        }
    }
}