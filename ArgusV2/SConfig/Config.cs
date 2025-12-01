using System;
using System.Collections.Generic;
using IngameScript.Helper;
using IngameScript.SConfig;
using IngameScript.SConfig.Database;
using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    public static class Config
    {
        


        private static ConfigTool _configTool = new ConfigTool("General Config", "")
        {
            Get = GetConfig,
            Set = SetConfig
        };

        
        public static Dictionary<string, Action> Commands;
        
        public static LogLevel LogLevel = LogLevel.Trace; // TODO: Hook me up
        
        public static string ArgumentTarget = "Target";
        public static string ArgumentUnTarget = "Untarget";
        public static string GroupName = "ArgusV2";
        public static string TrackerGroupName = "TrackerGroup";

        
        public static double MaxWeaponRange = 2000;
        public static double LockRange = 3000; // m
        public static float LockAngle = 40; // deg
        public static double MinFireDot = 0.999999; // Should be updated further
        
        
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


        public static int GdriveTimeoutFrames = 300; // TODO: Hook me up
        public static double GravityAcceleration = 9.81; // TODO: Hook me up
        public static bool DefaultPrecisionMode = false; // TODO: Hook me up
        public static bool DisablePrecisionModeOnEnemyDetected = false; // TODO: Hook me up TODO: Set me up
        public static double GdriveStep = 0.005;
        public static int GdriveArtificialMassBalanceFrequencyFrames = 300;
        public static int GdriveSpaceBallBalanceFrequencyFrames = 600;
        
        
        public static void Setup(IMyProgrammableBlock me)
        {
            Program.LogLine("Setting up config");
            // We touch the static classes that have config data that needs collecting/setting
            GunData.Init();
            ProjectileData.Init();
            
            if (me.CustomData.Length > 0)
                ConfigTool.DeserializeAll(me.CustomData);
            me.CustomData = ConfigTool.SerializeAll();
            Program.LogLine("Written config to custom data", LogLevel.Debug);
            Commands = new Dictionary<string, Action>
            {
                { ArgumentTarget,     () => ShipManager.PrimaryShip.Target() },
                { ArgumentUnTarget,   () => ShipManager.PrimaryShip.UnTarget() },
                { "FireAllTest",             () => ShipManager.PrimaryShip.Guns.FireAll() },
                { "CancelAllTest",           () => ShipManager.PrimaryShip.Guns.CancelAll() }
            };
            if (LogLevel.Trace <= LogLevel)
            {
                foreach (var command in Commands)
                {
                    Program.LogLine($"Command: {command.Key}", LogLevel.Trace);
                }
            }
            Program.LogLine("Commands set up", LogLevel.Debug);
            SetupGlobalState(me);
            Program.LogLine("Config setup done", LogLevel.Info);
            
        }

        public static void SetupGlobalState(IMyProgrammableBlock me)
        {
            Program.LogLine("Setting up global state", LogLevel.Debug);
            GlobalState.PrecisionMode = DefaultPrecisionMode;
            Program.LogLine($"Precision mode state is {GlobalState.PrecisionMode}", LogLevel.Trace);
        }

        static Config()
        {
            
        }


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
                    ["LockAngle"] = LockAngle,
                    ["MinFireDot"] = MinFireDot,
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
        private static void SetConfig(Dictionary<string, object> obj)
        {
            
            // Unwrap any Dwon.Comment objects recursively first
            Dwon.UnwrapAllComments(obj);

            
            // String Config
            var stringConfig = obj.ContainsKey("String Config") ? obj["String Config"] as Dictionary<string, object> : null;
            if (stringConfig != null)
            {
                ArgumentTarget       = Dwon.GetValue(stringConfig, "ArgumentTarget", ArgumentTarget);
                ArgumentUnTarget     = Dwon.GetValue(stringConfig, "ArgumentUnTarget", ArgumentUnTarget);
                GroupName            = Dwon.GetValue(stringConfig, "GroupName", GroupName);
                TrackerGroupName     = Dwon.GetValue(stringConfig, "TrackerGroupName", TrackerGroupName);
            }

            // Behavior Config
            var behaviorConfig = obj.ContainsKey("Behavior Config") ? obj["Behavior Config"] as Dictionary<string, object> : null;
            if (behaviorConfig != null)
            {
                MaxWeaponRange = Dwon.GetValue(behaviorConfig, "MaxWeaponRange", MaxWeaponRange);
                LockRange      = Dwon.GetValue(behaviorConfig, "LockRange", LockRange);
                LockAngle      = Dwon.GetValue(behaviorConfig, "LockAngle", LockAngle);
                MinFireDot     = Dwon.GetValue(behaviorConfig, "MinFireDot", MinFireDot);
            }

            // Tracker Config
            var trackerConfig = obj.ContainsKey("Tracker Config") ? obj["Tracker Config"] as Dictionary<string, object> : null;
            if (trackerConfig != null)
            {
                TrackingName                 = Dwon.GetValue(trackerConfig, "TrackingName", TrackingName);
                SearchingName                = Dwon.GetValue(trackerConfig, "SearchingName", SearchingName);
                StandbyName                  = Dwon.GetValue(trackerConfig, "StandbyName", StandbyName);
                ScannedBlockMaximumValidTimeFrames = Dwon.GetValue(trackerConfig, "ScannedBlockMaxValidFrames", ScannedBlockMaximumValidTimeFrames);
            }

            // PID Config
            var pidConfig = obj.ContainsKey("PID Config") ? obj["PID Config"] as Dictionary<string, object> : null;
            if (pidConfig != null)
            {
                ProportialGain        = Dwon.GetValue(pidConfig, "ProportionalGain", ProportialGain);
                IntegralGain          = Dwon.GetValue(pidConfig, "IntegralGain", IntegralGain);
                DerivativeGain        = Dwon.GetValue(pidConfig, "DerivativeGain", DerivativeGain);
                IntegralLowerLimit    = Dwon.GetValue(pidConfig, "IntegralLowerLimit", IntegralLowerLimit);
                IntegralUpperLimit    = Dwon.GetValue(pidConfig, "IntegralUpperLimit", IntegralUpperLimit);
                MaxAngularVelocityRpm = Dwon.GetValue(pidConfig, "MaxAngularVelocityRPM", MaxAngularVelocityRpm);
            }
        }
    }
}