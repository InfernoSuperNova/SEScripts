
using Sandbox.ModAPI.Ingame;
using System;


namespace IngameScript
{
    public partial class Program : MyGridProgram
    {
        private readonly TimedLog _log = new TimedLog(10);
        public readonly LogLevel LogLevel = LogLevel.Trace;
        public Program()
        {
            I = this;
            Config.Setup(Me);
        }

        public static Program I { get; private set; }

        public void Save()
        {

        }

        public void Main(string argument, UpdateType updateSource)
        {

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
            I._log.Add(echo.ToString(), level);
        }
    }
}