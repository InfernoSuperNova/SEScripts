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
            Sync = SyncConfig
        };

        public static readonly GeneralConfig General = new GeneralConfig();
        public static readonly StringConfig String = new StringConfig();
        public static readonly BehaviorConfig Behavior = new BehaviorConfig();
        public static readonly TrackerConfig Tracker = new TrackerConfig();
        public static readonly GdriveConfig Gdrive = new GdriveConfig();
        public static readonly PidConfig Pid = new PidConfig();

        public static void Setup(IMyProgrammableBlock me)
        {
            Program.LogLine("Setting up config");
            // We touch the static classes that have config data that needs collecting/setting
            GunData.Init();
            ProjectileData.Init();
            
            me.CustomData = ConfigTool.SyncConfig(me.CustomData);
            
            Program.LogLine("Written config to custom data", LogLevel.Debug);
            Program.LogLine("Commands set up", LogLevel.Debug);
            SetupGlobalState(me);
            Program.LogLine("Config setup done", LogLevel.Info);
            
        }

        private static void SetupGlobalState(IMyProgrammableBlock me)
        {
            Program.LogLine("Setting up global state", LogLevel.Debug);
            GlobalState.PrecisionMode = Gdrive.DefaultPrecisionMode;
            Program.LogLine($"Precision mode state is {GlobalState.PrecisionMode}", LogLevel.Trace);
        }

        private static void SyncConfig(Dictionary<string, object> obj)
        {
            General.Sync(obj);
            String.Sync(obj);
            Behavior.Sync(obj);
            Tracker.Sync(obj);
            Gdrive.Sync(obj);
            Pid.Sync(obj);
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
                
                config.Sync("LogLevel", ref LogLevel);
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
            public string TrackerGroupName = "TrackerGroup";
            
            internal void Sync(Dictionary<string, object> obj)
            {
                var config = ConfigCategory.From(obj, "String Config");
                
                config.Sync("ArgumentTarget", ref ArgumentTarget);
                config.Sync("ArgumentUnTarget", ref ArgumentUnTarget);
                config.Sync("GroupName", ref GroupName);
                config.Sync("TrackerGroupName", ref TrackerGroupName);
            }
        }

        public class BehaviorConfig
        {
            public double LockRange = 3000; // m
            public float LockAngle = 40; // deg
            public double MinFireDot = 0.999999; // Should be updated further
            
            internal void Sync(Dictionary<string, object> obj)
            {
                var config = ConfigCategory.From(obj, "Behavior Config");
                
                config.Sync("LockRange", ref LockRange);
                config.Sync("LockAngle", ref LockAngle);
                config.Sync("MinFireDot", ref MinFireDot);
            }
        }

        public class TrackerConfig
        {
            public string TrackingName = "CTC: Tracking";
            public string SearchingName = "CTC: Searching";
            public string StandbyName = "CTC: Standby";
            public int ScannedBlockMaxValidFrames = 3600; // 1 minutes
            
            internal void Sync(Dictionary<string, object> obj)
            {
                var config = ConfigCategory.From(obj, "Tracker Config");
                
                config.Sync("TrackingName", ref TrackingName);
                config.Sync("SearchingName", ref SearchingName);
                config.Sync("StandbyName", ref StandbyName);
                config.Sync("ScannedBlockMaxValidFrames", ref ScannedBlockMaxValidFrames);
            }
        }

        public class GdriveConfig
        {
            public int TimeoutFrames = 300;
            public double Acceleration = 9.81;
            public bool DefaultPrecisionMode = false;
            public bool DisablePrecisionModeOnEnemyDetected = false;
            public double Step = 0.005;
            public int MassBalanceFrequencyFrames = 300;
            public int BallBalanceFrequencyFrames = 600;
            public int AccelerationRecalcDelay = 600;

            internal void Sync(Dictionary<string, object> obj)
            {
                var config = ConfigCategory.From(obj, "Gravity Config");
                
                config.Sync("TimeoutFrames", ref TimeoutFrames);
                config.Sync("Acceleration", ref Acceleration);
                config.Sync("DefaultPrecisionMode", ref DefaultPrecisionMode);
                config.Sync("DisablePrecisionModeOnEnemyDetected", ref DisablePrecisionModeOnEnemyDetected);
                config.Sync("Step", ref Step);
                config.Sync("MassBalanceFrequencyFrames", ref MassBalanceFrequencyFrames);
                config.Sync("BallBalanceFrequencyFrames", ref BallBalanceFrequencyFrames);
                config.Sync("AccelerationRecalcDelay", ref AccelerationRecalcDelay);
            }
        }

        public class PidConfig
        {
            public double ProportionalGain = 500;
            public double IntegralGain = 0;
            public double DerivativeGain = 30;
            public double IntegralLowerLimit = -0.05;
            public double IntegralUpperLimit = 0.05;
            
            internal void Sync(Dictionary<string, object> obj)
            {
                var config = ConfigCategory.From(obj, "PID Config");
                
                config.Sync("ProportionalGain", ref ProportionalGain);
                config.Sync("IntegralGain", ref IntegralGain);
                config.Sync("DerivativeGain", ref DerivativeGain);
                config.Sync("IntegralLowerLimit", ref IntegralLowerLimit);
                config.Sync("IntegralUpperLimit", ref IntegralUpperLimit);
            }
        }
    }
}