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
using CoreSystems.Api;
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
        private const string PdcGroupName = "PDCs";
        public static WcPbApi Api;
        public static Program I;


 
        
        private static List<PDC> Pdcs => PDC.PdcList;
        
        public Program()
        {
            I = this;
            Api = new WcPbApi();
            try
            {
                Api.Activate(Me);
            }
            catch (Exception e)
            {
                Echo("WeaponCore Api is failing! \n Make sure WeaponCore is enabled!"); 
                Echo(e.Message);
                Echo(e.StackTrace);
                return;
            }
            
            
            
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

        
        
        
        
        private uint frame = 0;
        public void Main(string argument, UpdateType updateSource)
        {
            if (frame == 0)
            {
                var termPdcs = new List<IMyFunctionalBlock>();
                var pdcGroup = GridTerminalSystem.GetBlockGroupWithName(PdcGroupName);
                pdcGroup.GetBlocksOfType(termPdcs);
                foreach (var termPdc in termPdcs)
                {
                    Echo("Creating new PDC");
                    var pdc = new PDC(termPdc);
                }
            }
            
            
            foreach (var pdc in Pdcs)
            {
                pdc.Update();
            }
            
            Echo(Runtime.LastRunTimeMs.ToString());
            frame++;
        }
    }
}