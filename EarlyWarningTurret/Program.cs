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
        private string _blockGroup = "Enemy Alert";
        
        private List<IMySoundBlock> _alertSounders;
        private List<IMyInteriorLight> _alertLights;
        private List<IMyTurretControlBlock> _alertCtCs;
        
        public Program()
        {
            _alertSounders = new List<IMySoundBlock>();
            _alertLights = new List<IMyInteriorLight>();
            _alertCtCs = new List<IMyTurretControlBlock>();

            var blockGroup = GridTerminalSystem.GetBlockGroupWithName(_blockGroup);
            
            blockGroup.GetBlocksOfType(_alertSounders);
            blockGroup.GetBlocksOfType(_alertLights);
            blockGroup.GetBlocksOfType(_alertCtCs);

            Runtime.UpdateFrequency = UpdateFrequency.Update100;
        }
        
        
        private bool _hadTarget = false;
        public void Main(string argument, UpdateType updateSource)
        {
            bool hasTarget = false;

            foreach (var turret in _alertCtCs)
            {
                if (turret.HasTarget) hasTarget = true;
            }


            if (!_hadTarget && hasTarget)
            {
                // Enable group

                foreach (var light in _alertLights)
                {
                    light.Enabled = true;
                }


            }

            if (hasTarget)
            {
                foreach (var sound in _alertSounders)
                {
                    sound.Play();
                }
            }

            if (_hadTarget && !hasTarget)
            {
                // Disable group
                foreach (var light in _alertLights)
                {
                    light.Enabled = false;
                }
                foreach (var sound in _alertSounders)
                {
                    sound.Stop();
                }
                
            };

            _hadTarget = hasTarget;

        }
    }
}