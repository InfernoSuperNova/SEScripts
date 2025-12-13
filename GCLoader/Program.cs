using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    public partial class Program : MyGridProgram
    {
        // Space Engineers Programmable Block GC stress "toy"
// Note: very low CPU cost per tick, spreads GC load over time

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
        }

        List<object> survivors = new List<object>();
        Random rnd = new Random();
        int tick = 0;

        public void Main()
        {
            tick++;

            for (int i = 0; i < 2000; i++) // larger batch
            {
                int size = rnd.Next(128, 150000); // 128 bytes → ~150 KB
                object temp = new byte[size];

            // Keep some objects alive for promotion
                if (rnd.NextDouble() < 0.3)
                    survivors.Add(temp);
            }

            // Slowly trim survivors
            if (tick % 100 == 0 && survivors.Count > 2000)
            {
                int removeCount = survivors.Count / 50; // ~2%
                survivors.RemoveRange(0, removeCount);
            }

            Echo("Tick: " + tick + ", Survivors: " + survivors.Count);
            Echo(Runtime.LastRunTimeMs.ToString());
        }


    }
}