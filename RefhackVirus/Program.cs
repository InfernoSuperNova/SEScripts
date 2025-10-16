
using System;
using System.Text;
using EmptyKeys.UserInterface.Controls;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.Entities.Blocks;
using SpaceEngineers.Game.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        public TimedLog _Log = new TimedLog(10);
            
        public static void Print(object echo)
        {
            I.Echo(echo.ToString());
        }
        
        public static void Log(object echo)
        {
            I._Log.Add(echo.ToString());
        }
        
        public static Program I;
        
        
        public Program()
        {
            I = this;
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
            InfectedGridManager.TryInfectGrid(Me.CubeGrid, null, InfectionType.Initial, Me.GetPosition());
        }

        private int _frame = 0;
        public void Main(string argument, UpdateType updateSource)
        {
            InfectedGridManager.Update(_frame++);
            _Log.Update();
            Print(InfectedGridManager.InfectedCount);
            Print(_Log);
            IMyMedicalRoom medbay;
        }
        
        
        // TODO:
        // Grid select up/down
        // Apply effects to grid
        // Ideas:
        // (All ideas where possible should have an option to trigger once, periodically, or trigger constantly)
        // unlock all connectors
        // depower
        // turn off medbays
        // reset terminal names
        // dump ingots
        // fuck up custom data
        // break turrets
        // explode warheads
        // open/close random door
        // Turn on all gravity generators and mass blocks (max value)
        // Round all terminal values
        // Set every single property of every single block to a random value
        // Get position
        // set block share with all (is this possible?)
        // Get a map of every block and able to set property actions on specific blocks
        // Random gyro override
        // Specific gyro override
        // Empty ammo and reactors
        // Make all guns fire
        // Make weapons shoot friendlies
        // Garble block names
        // Extend pistons to max
        // Set beacons to 200km
        // Detach mechanical connections
        // NaNify everything
    }
}