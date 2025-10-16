
using System;
using System.Text;
using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        public static void Print(object echo)
        {
            I.Echo(echo.ToString());
        }

        public static void Log(object echo)
        {
            I.EchoSB.Append(echo);
            I.EchoSB.Append("\n");
        }
        
        public static Program I;

        public StringBuilder EchoSB = new StringBuilder();
        
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
            Print(InfectedGridManager.InfectedCount);
            Print(EchoSB.ToString());
        }
    }
}