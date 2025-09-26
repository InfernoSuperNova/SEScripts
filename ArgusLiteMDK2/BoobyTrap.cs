using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EmptyKeys.UserInterface.Controls;
using Sandbox.Game.World;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRage.Game;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRageMath;

namespace IngameScript
{
    public static class BoobyTrap
    {
        public static void DisablePower(IMyGridTerminalSystem gridTerminalSystem)
        {
            var blocks = new List<IMyFunctionalBlock>();
            gridTerminalSystem.GetBlocksOfType(blocks);
            foreach (var block in blocks)
            {
                block.Enabled = false;
            }
        }

        public static void DetachSubgrids(IMyGridTerminalSystem gridTerminalSystem)
        {
            var rotors = new List<IMyMotorStator>();
            var pistons = new List<IMyPistonBase>();
            gridTerminalSystem.GetBlocksOfType(rotors);
            gridTerminalSystem.GetBlocksOfType(pistons);
            foreach (var rotor in rotors)
            {
                rotor.Detach();
            }

            foreach (var piston in pistons)
            {
                piston.Detach();
            }
        }

        public static void PurgeAllTheItems(IMyGridTerminalSystem gridTerminalSystem)
        {
            var blocks = new List<IMyShipConnector>();
            gridTerminalSystem.GetBlocksOfType(blocks);
            foreach (var connector in blocks)
            {
                connector.CollectAll = true;
                connector.ThrowOut = true;
            }
        }

        public static void DetonateAllTheWarheads(IMyGridTerminalSystem gridTerminalSystem)
        {
            var blocks = new List<IMyWarhead>();
            gridTerminalSystem.GetBlocksOfType(blocks);
            foreach (var warhead in blocks)
            {
                warhead.IsArmed = true;
                warhead.Detonate();
            }
        }

        public static void BullshittifyTheNames(IMyGridTerminalSystem gridTerminalSystem)
        {
            var blocks = new List<IMyTerminalBlock>();
            gridTerminalSystem.GetBlocksOfType(blocks);
            foreach (var block in blocks)
            {
                block.CustomName = "#ERROR#";
                block.CustomData = "#ERROR#";
            }
        }
        
    }
}