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
    partial class Program : MyGridProgram
    {
        DebugAPI debugAPI;
        // This file contains your actual script.
        //
        // You can either keep all your code here, or you can create separate
        // code files to make your program easier to navigate while coding.
        //
        // In order to add a new utility class, right-click on your project, 
        // select 'New' then 'Add Item...'. Now find the 'Space Engineers'
        // category under 'Visual C# Items' on the left hand side, and select
        // 'Utility Class' in the main area. Name it in the box below, and
        // press OK. This utility class will be merged in with your code when
        // deploying your final script.
        //
        // You can also simply create a new utility class manually, you don't
        // have to use the template if you don't want to. Just do so the first
        // time to see what a utility class looks like.
        // 
        // Go to:
        // https://github.com/malware-dev/MDK-SE/wiki/Quick-Introduction-to-Space-Engineers-Ingame-Scripts
        //
        // to learn more about ingame scripts.

        public Program()
        {
            debugAPI = new DebugAPI(this);
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
        }

        public void Save()
        {
            // Called when the program needs to save its state. Use
            // this method to save your state to the Storage field
            // or some other means. 
            // 
            // This method is optional and can be removed if not
            // needed.
        }

        public void Main(string argument, UpdateType updateSource)
        {
            // This is really, REALLY bad, but this is just a basic debug script, I don't intend to use it on servers
            List<IMyArtificialMassBlock> massBlocks = new List<IMyArtificialMassBlock>();
            GridTerminalSystem.GetBlocksOfType(massBlocks);
            Echo(massBlocks.Count.ToString());
            IMyShipController myShipController = GridTerminalSystem.GetBlockWithName("Cockpit") as IMyShipController;

            Vector3D averagePosFilterNonFunctional = Vector3D.Zero;

            Vector3D averagePosNoFilter = Vector3D.Zero;
            int count = 0;
            int count2 = 0;
            foreach (IMyArtificialMassBlock massBlock in massBlocks)
            {
                if (massBlock.Enabled == false)
                {
                    continue;
                }
                averagePosNoFilter += massBlock.GetPosition();
                count2++;
                if (massBlock.Enabled == false || massBlock.IsFunctional == false)
                {
                    continue;
                }
                count++;
                averagePosFilterNonFunctional += massBlock.GetPosition();
            }
            averagePosFilterNonFunctional /= count;
            averagePosNoFilter /= count2;
            debugAPI.DrawPoint(averagePosNoFilter, Color.Red, 0.5f, 1.5f, true);
            debugAPI.DrawPoint(averagePosFilterNonFunctional, Color.Green, 0.5f, 1.5f, true);
            debugAPI.DrawPoint(myShipController.CenterOfMass, Color.Blue, 0.5f, 1.5f, true);

            // BLUE IS ACTUAL CENTER OF MASS
            // Red is average position of all mass blocks
            // Green is average position of all functional 
        }
    }
}
