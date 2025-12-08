
using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using IngameScript.Helper;
using IngameScript.SConfig;

namespace IngameScript
{
    
    partial class Program : MyGridProgram
    {
        
        public static DebugAPI Debug;
        public static Random RNG;
        public TimedLog _Log = new TimedLog(10);

        private static Queue<double> updateTimes = new Queue<double>();
        
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
                Program.LogLine("Creating commands", LogLevel.Info);
                Commands.Setup();
                //Runtime.UpdateFrequency = UpdateFrequency.Update1;
                
                var elapsed = DateTime.UtcNow - startTime;
                LogLine($"Setup completed in {elapsed.TotalMilliseconds:F1} ms", LogLevel.Highlight);
            }
            catch (Exception ex)
            {
                Echo("Crashed: " + ex);
            }
            finally
            {
                I = null;
            }
            Echo(_Log.ToString());
        }
        
        public static Program I { get; private set; }
        
        public void Main(string argument, UpdateType updateSource)
        {

            try
            {
                I = this;
                if ((updateSource & UpdateType.Update1) != 0) RunUpdate();
                if ((updateSource & (UpdateType.Trigger | UpdateType.Terminal)) != 0) RunCommand(argument);
            }
            catch (Exception ex)
            {
                Echo(ex.ToString());
                Runtime.UpdateFrequency = UpdateFrequency.None;
            }
            finally
            {
                I = null;
            }
        }


        // TODO to be feature parity with arguslite:
        // Handle block removal in all systems
        // UI, switches (hud LCD probably)
        // Fix aimbot
        // Auto dodge
        // Turret override
        // No group early exit
        
        private void RunUpdate()
        {
            using (Debug.Measure(duration => { updateTimes.Enqueue(duration.TotalMilliseconds); }))
            {
                TimerManager.TickAll();
                ShipManager.EarlyUpdate(_frame);
                ShipManager.LateUpdate(_frame++);
            }

            if (updateTimes.Count > 600) updateTimes.Dequeue();
            Log(updateTimes.Average());
            _Log.Update();
            Log(_Log);
        }
        private void RunCommand(string argument)
        {
            Action action;
            if (Commands.TryGetValue(argument, out action))
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