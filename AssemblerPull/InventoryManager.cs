using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using VRage;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript
{
    public static class InventoryManager
    {
        private static List<IMyCargoContainer> _cargoContainers = new List<IMyCargoContainer>();
        private static List<IMyCargoContainer> _ingotContainers = new List<IMyCargoContainer>();
        private static List<IMyCargoContainer> _componentContainers = new List<IMyCargoContainer>();

        public static List<IMyCargoContainer> CargoContainers => _cargoContainers;
        public static List<IMyCargoContainer> IngotContainers => _ingotContainers;
        public static List<IMyCargoContainer> ComponentContainers => _componentContainers;

        public static void Initialize(IMyGridTerminalSystem gts)
        {
            _cargoContainers.Clear();
            _ingotContainers.Clear();

            List<IMyCargoContainer> allContainers = new List<IMyCargoContainer>();
            gts.GetBlocksOfType(allContainers);

            // Categorize containers: ingots containers vs regular cargo containers
            foreach (IMyCargoContainer container in allContainers)
            {
                if (container.CustomName.ToLower().Contains("ingot"))
                {
                    _ingotContainers.Add(container);
                }
                else if (container.CustomName.ToLower().Contains("component"))
                {
                    _componentContainers.Add(container);
                }
                else
                {
                    
                }
                _cargoContainers.Add(container);
            }

            Program.LogLine($"Initialized InventoryManager with {_ingotContainers.Count} ingot containers, {_cargoContainers.Count} cargo containers", LogLevel.Info);
        }

        public static void PullItems(MyItemType type, int needed, IMyInventory blockInv)
        {
            if (PullItemsFromContainers(_ingotContainers, type, blockInv, ref needed))
                return;
            if (PullItemsFromContainers(_componentContainers, type, blockInv, ref needed))
                return;
            
            switch (type.SubtypeId)
            {
                case "Ingot":
                    
                    break;
                case "Component":
                    
                    break;
            }
            // First, prioritize ingots containers
            if (PullItemsFromContainers(_ingotContainers, type, blockInv, ref needed))
                return;

            // Then, pull from regular cargo containers
            PullItemsFromContainers(_cargoContainers, type, blockInv, ref needed);
        }

        private static bool PullItemsFromContainers(List<IMyCargoContainer> containers, MyItemType type, IMyInventory blockInv, ref int remainingNeeded)
        {
            Program.LogLine($"Pulling {remainingNeeded} of {type.SubtypeId} into {blockInv.Owner.DisplayName}");
            foreach (IMyCargoContainer container in containers)
            {
                IMyInventory inv = container.GetInventory();
                List<MyInventoryItem> items = new List<MyInventoryItem>();

                inv.GetItems(items);

                foreach (MyInventoryItem item in items)
                {
                    if (item.Type != type)
                        continue;

                    int available = (int)item.Amount;
                    int toMove = Math.Min(remainingNeeded, available);

                    if (toMove <= 0)
                        continue;

                    if (inv.TransferItemTo(
                            blockInv,
                        item,
                        (MyFixedPoint)toMove))
                    {
                        remainingNeeded -= toMove;
                    }

                    if (remainingNeeded <= 0)
                        return true;
                }
            }
            return false;
        }

        public static void DumpExcessItems(IMyInventory assemblerInv, List<string> requiredIngots)
        {
            List<MyInventoryItem> items = new List<MyInventoryItem>();
            assemblerInv.GetItems(items);

            // Convert to HashSet for O(1) lookup performance
            HashSet<string> requiredIngotsSet = new HashSet<string>(requiredIngots);

            for (var index = items.Count - 1; index >= 0; index--)
            {
                var item = items[index];
                string subtypeId = item.Type.SubtypeId;

                // Skip items that are required for the recipe
                if (requiredIngotsSet.Contains(subtypeId))
                    continue;

                // First, try to move excess item to an ingots container
                if (DumpItemToContainers(assemblerInv, item, _ingotContainers))
                    return; // Stop after successfully dumping one item to avoid excessive transfers
                if (DumpItemToContainers(assemblerInv, item, _componentContainers))
                    return;

                // Then, try to move to regular cargo containers
                if (DumpItemToContainers(assemblerInv, item, _cargoContainers))
                    return; // Stop after successfully dumping one item to avoid excessive transfers
            }
        }

        public static void TopUpInventory(IMyInventory blockInv, MyItemType itemType, int desiredAmount)
        {
            // Check current amount in inventory
            List<MyInventoryItem> items = new List<MyInventoryItem>();
            blockInv.GetItems(items);

            int currentAmount = 0;
            foreach (MyInventoryItem item in items)
            {
                if (item.Type == itemType)
                {
                    currentAmount += (int)item.Amount;
                }
            }

            // Only pull if we need more
            int needed = desiredAmount - currentAmount;
            if (needed > 0)
            {
                PullItems(itemType, needed, blockInv);
            }
        }

        public static bool DumpItemToContainers(IMyInventory sourceInv, MyInventoryItem item, List<IMyCargoContainer> containers)
        {
            foreach (IMyCargoContainer container in containers)
            {
                IMyInventory containerInv = container.GetInventory();

                if (!containerInv.IsConnectedTo(sourceInv))
                    continue;
                if (containerInv.CanItemsBeAdded(item.Amount, item.Type) && sourceInv.TransferItemTo(containerInv, item))
                {
                    return true; // Item successfully moved
                }
            }
            return false;
        }
    }
}
