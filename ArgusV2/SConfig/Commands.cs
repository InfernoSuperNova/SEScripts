
using System;
using System.Collections.Generic;
using IngameScript.Helper;
using IngameScript.Helper.Log;

namespace IngameScript.SConfig
{
    public static class Commands
    {
        private static Dictionary<string, Action> _commands;
        
        
        public static void Setup()
        {
            _commands = new Dictionary<string, Action>
            {
                { Config.String.ArgumentUnTarget, ShipManager.PrimaryShip.Target },
                { Config.String.ArgumentTarget, ShipManager.PrimaryShip.UnTarget },
            };

            if (LogLevel.Trace <= Config.General.LogLevel)
            {
                foreach (var command in _commands)
                {
                    Program.LogLine($"Command: {command.Key}", LogLevel.Trace);
                }
            }
            
        }


        public static bool TryGetValue(string argument, out Action action)
        {
            return _commands.TryGetValue(argument, out action);
        }
    }
}