using System.Collections.Generic;
using IngameScript.Database;
using IngameScript.SConfig;
using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    public static class Config
    {
        private static ConfigTool _configTool = new ConfigTool("General Config", "")
        {
            Get = GetConfig,
        };
        public static void Setup(IMyProgrammableBlock me)
        {
            // We touch the static classes that have config data that needs collecting/setting
            GunData.Init();
            ProjectileData.Init();
            me.CustomData = ConfigTool.Collect();
        }

        static Config()
        {
            
        }
        
        public const string ArgumentTarget = "Target";
        public const string ArgumentUnTarget = "Untarget";
        public static string GroupName = "ArgusV2";
        public static string TrackerGroupName = "TrackerGroup";

        
        public static double MaxWeaponRange = 2000;
        public static double LockRange = 3000; // m
        public static float LockAngle = 40; // deg
        
        
        public static string TrackingName = "CTC: Tracking";
        public static string SearchingName = "CTC: Searching";
        public static string StandbyName = "CTC: Standby";
        public static int ScannedBlockMaximumValidTimeFrames = 3600; // 1 minutes

        public static double ProportialGain = 500;
        public static double IntegralGain = 0;
        public static double DerivativeGain = 30;
        public static double IntegralLowerLimit = -0.05;
        public static double IntegralUpperLimit = 0.05;
        public static double MaxAngularVelocityRpm = 30;

        private static Dictionary<string, object> GetConfig()
        {
            return new Dictionary<string, object>
            {
                ["String Config"] = new Dictionary<string, object>
                {
                    ["ArgumentTarget"] = ArgumentTarget,
                    ["ArgumentUnTarget"] = ArgumentUnTarget,
                    ["GroupName"] = GroupName,
                    ["TrackerGroupName"] = TrackerGroupName
                },
                ["Behavior Config"] = new Dictionary<string, object>
                {
                    ["MaxWeaponRange"] = MaxWeaponRange,
                    ["LockRange"] = LockRange,
                    ["LockAngle"] = LockAngle
                },
                ["Tracker Config"] = new Dictionary<string, object>
                {
                    ["TrackingName"] = TrackingName,
                    ["SearchingName"] = SearchingName,
                    ["StandbyName"] = StandbyName,
                    ["ScannedBlockMaxValidFrames"] = ScannedBlockMaximumValidTimeFrames
                },
                ["PID Config"] = new Dictionary<string, object>
                {
                    ["ProportionalGain"] = ProportialGain,
                    ["IntegralGain"] = IntegralGain,
                    ["DerivativeGain"] = DerivativeGain,
                    ["IntegralLowerLimit"] = IntegralLowerLimit,
                    ["IntegralUpperLimit"] = IntegralUpperLimit,
                    ["MaxAngularVelocityRPM"] = MaxAngularVelocityRpm
                }
            };
        }
    }
}