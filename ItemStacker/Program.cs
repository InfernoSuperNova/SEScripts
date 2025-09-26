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
        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10; // run every ~1.6s
            
            var containers = new List<IMyCargoContainer>();
            GridTerminalSystem.GetBlocksOfType(containers);

            // Get containers named "Leto Container ###"
             _containers = containers.Where(c =>
                {
                    string name = c.CustomName.Trim();
                    if (!name.StartsWith(PREFIX)) return false;
                    string numberPart = name.Substring(PREFIX.Length);
                    int num;
                    return int.TryParse(numberPart, out num) && num >= 1 && num <= 100;
                })
                .OrderBy(c => int.Parse(c.CustomName.Substring(PREFIX.Length)))
                .ToList();
            
        }

        const string PREFIX = "Leto Container ";
        
        private List<IMyCargoContainer> _containers;

        public void Main(string argument, UpdateType updateSource)
        {
            

            // Track how much Ice is in each container
            var containerData = _containers.Select((c, index) =>
                {
                    var inv = c.GetInventory();
                    var items = new List<MyInventoryItem>();
                    inv.GetItems(items, i => i.Type.SubtypeId == "Ice");
                    
                    
                    double amount = items.Count > 0 ? (double)items[0].Amount : 0.0;
                    return new { container = c, inventory = inv, iceAmount = amount, items = items, index = index, fillFactor = amount / (double)inv.MaxVolume };
                })
                .ToList();

            // Redistribution loop: take Ice from overfilled and give to underfilled containers
            for (int i = containerData.Count - 1; i >= 0; i--)
            {
                var donor = containerData[i];
                double surplus = donor.iceAmount;
            
                
                // Move ice
                foreach (var receiver in containerData.Where(r => r.fillFactor < 1 && r.index < donor.index))
                {
                    if (donor.iceAmount <= 0) break;
                    if (receiver.container == donor.container) continue;
            
                    double transferAmount = donor.iceAmount;
                    if (transferAmount <= 0) continue;
            
                    // Perform transfer
                    var iceItem = donor.items.FirstOrDefault(i2 => i2.Type.SubtypeId == "Ice");
                    if (iceItem.Type.TypeId != null)
                    {
                        donor.inventory.TransferItemTo(receiver.inventory, iceItem, (VRage.MyFixedPoint)iceItem.Amount);
                        Echo("Moving item from " + donor.index + " to " + receiver.index);
                    }
                    surplus -= transferAmount;
                    if (surplus <= 0.01) break;
                }
            }
            
            // And push other items back up the chain

            foreach (var donor in containerData)
            {
                if (donor.fillFactor <= 0) continue;

                for (int i = containerData.Count - 1; i > donor.index; i--)
                {
                    
                    
                    if (donor.fillFactor <= 0) break;

                    var receiver = containerData[i];
                    
                    if (receiver.fillFactor >= 1) continue;

                    foreach (var item in donor.items)
                    {
                        if (item.Type.SubtypeId == "Ice") continue;
                        if (item.Amount <= 0) continue;
                        receiver.inventory.TransferItemTo(donor.inventory, item, (VRage.MyFixedPoint)item.Amount);
                        Echo("Moving item from " + donor.index + " to " + receiver.index);
                        break; // ( only move one item per loop)
                    }
                    
                }
            }
        }
    }
}