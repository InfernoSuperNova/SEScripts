
using Sandbox.ModAPI.Ingame;
using System;
using IngameScript.Helper;
using IngameScript.SConfig;
using VRageMath;

namespace IngameScript
{
    
    partial class Program : MyGridProgram
    {
        public static Program I;
        public static DebugAPI Debug;
        public static Random RNG;
        public TimedLog _Log = new TimedLog(10);
        
        private int _frame;
        public Program()
        {
            try
            {
                var startTime = DateTime.UtcNow;
                _Log.Add("Beginning script setup", LogLevel.Info);
                I = this;
                Debug = new DebugAPI(this);
                RNG = new Random();
                LogLine("Setup debug and RNG", LogLevel.Debug);
                Config.Setup(Me);
                Program.LogLine("Creating this ship as controllable ship", LogLevel.Info);
                ShipManager.CreateControllableShip(Me.CubeGrid, GridTerminalSystem);
                Runtime.UpdateFrequency = UpdateFrequency.Update1;
                
                var elapsed = DateTime.UtcNow - startTime;
                LogLine($"Setup completed in {elapsed.TotalMilliseconds:F1} ms", LogLevel.Highlight);
            }
            catch (Exception ex)
            {
                Echo("Crashed: " + ex);
            }
            Echo(_Log.ToString());
        }
        
        public void Main(string argument, UpdateType updateSource)
        {
            
            if ((updateSource & UpdateType.Update1) != 0) RunUpdate();
            if ((updateSource & (UpdateType.Trigger | UpdateType.Terminal)) != 0) RunCommand(argument);
        }



        private void RunUpdate()
        {
            
            TimerManager.TickAll();
            ShipManager.EarlyUpdate(_frame);
            ShipManager.LateUpdate(_frame++);
            
            _Log.Update();
            Log(_Log);
        }
        private void RunCommand(string argument)
        {
            Action action;
            if (Config.Commands.TryGetValue(argument, out action))
            {
                action();
            }
            else
            {
                // Optional: handle unknown command
                // e.g., Log("Unknown command: " + argument);
            }
        }
        public static void Log(object obj)
        {
            I.Echo(obj.ToString());
        }

        public static void Log(TimeSpan elapsed, string hint)
        {
            double microseconds = elapsed.Ticks / 10.0;
            I.Echo($"{hint}: {microseconds} µs");
        }
        
        public static void LogLine(object echo, LogLevel level = LogLevel.Info)
        {
            I._Log.Add(echo.ToString(), level);
        }
    }
}