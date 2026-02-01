using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using VRage;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript
{
    public class Ballast
    {
        List<IMyShipConnector> Connectors;
        List<IMyCollector> Collectors;
        
        public Ballast(IMyBlockGroup group)
        {
            Connectors = new List<IMyShipConnector>();
            Collectors = new List<IMyCollector>();
            group.GetBlocksOfType(Connectors);
            group.GetBlocksOfType(Collectors);
        }

        public List<IMyCollector> GetCollectors => Collectors;

        public double GetTotalMass => (double)Collectors[0].GetInventory().CurrentMass / Program.I.InventoryMultiplier;

        public void TargetMass(double target)
        {
            double current = GetTotalMass;
            if (current < target) Collectors[0].Enabled = true;
            Program.I.Echo(target.ToString());
            Program.I.Echo(current.ToString());
            if (current > target)
            {
                Collectors[0].Enabled = false;
                
                var collectorInv = Collectors[0].GetInventory();

                var item = collectorInv.GetItemAt(0);
                var itemAmount = collectorInv.GetItemAmount(item.Value.Type);
                
                
                var massPerItem = current / (double)itemAmount;
                
                var itemToTransfer = (MyFixedPoint)((current - target) / massPerItem);

                collectorInv.TransferItemTo(Connectors[0].GetInventory(), 0, 0, true, itemToTransfer);
            }

        }
    }
}