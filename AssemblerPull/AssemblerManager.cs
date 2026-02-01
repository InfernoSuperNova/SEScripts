using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using VRage;
using VRage.Game;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript
{
    public static class AssemblerManager
    {
        private static List<IMyAssembler> _assemblers = new List<IMyAssembler>();
        private static int _currentAssemblerIndex = 0;
        private static int _redistributionTimer = 0;
        private const int REDISTRIBUTION_INTERVAL = 900; // Redistribute every 600 ticks
        private const int OUTPUT_INVENTORY_THRESHOLD = 10; // Push items out when output inventory exceeds this amount

        // Recipe lookup: Component/Ammo SubtypeId -> List of required ingot SubtypeIds
        private static Dictionary<string, List<string>> _recipeRequirements = new Dictionary<string, List<string>>()
        {
            // Components
            { "BulletproofGlass", new List<string> { "Silicon" } },
            { "ComputerComponent", new List<string> { "Iron", "Silicon" } },
            { "ConstructionComponent", new List<string> { "Iron" } },
            { "DetectorComponent", new List<string> { "Iron", "Nickel" } },
            { "Display", new List<string> { "Iron", "Silicon" } },
            { "Explosives", new List<string> { "Silicon", "Magnesium" } },
            { "Girder", new List<string> { "Iron" } },
            { "GravityGeneratorComponent", new List<string> { "Iron", "Cobalt", "Silver", "Gold" } },
            { "InteriorPlate", new List<string> { "Iron" } },
            { "LargeTube", new List<string> { "Iron" } },
            { "Medical", new List<string> { "Iron", "Nickel", "Silver" } },
            { "MetalGrid", new List<string> { "Iron", "Nickel", "Cobalt" } },
            { "MotorComponent", new List<string> { "Iron", "Nickel" } },
            { "PowerCell", new List<string> { "Iron", "Silicon", "Nickel" } },
            { "RadioCommunication", new List<string> { "Iron", "Silicon" } },
            { "ReactorComponent", new List<string> { "Stone", "Iron", "Silver" } },
            { "SmallTube", new List<string> { "Iron" } },
            { "SolarCell", new List<string> { "Nickel", "Silicon" } },
            { "SteelPlate", new List<string> { "Iron" } },
            { "Superconductor", new List<string> { "Iron", "Gold" } },
            { "Thrust", new List<string> { "Iron", "Cobalt", "Gold", "Platinum" } },

            // Ammunition
            { "Position0140_LargeRailgunAmmo", new List<string> { "Iron", "Nickel", "Silicon", "Uranium" } },
            { "SmallRailgunAmmo", new List<string> { "Iron", "Nickel", "Silicon", "Uranium" } },
            { "Position0120_LargeCalibreAmmo", new List<string> { "Iron", "Nickel", "Magnesium", "Uranium" } },
            { "MediumCalibreAmmo", new List<string> { "Iron", "Nickel", "Magnesium" } },
            { "Missile200mm", new List<string> { "Iron", "Nickel", "Silicon", "Uranium", "Platinum", "Magnesium" } },
            { "AutocannonClip", new List<string> { "Iron", "Nickel", "Magnesium" } },
            { "Position0080_NATO_25x184mmMagazine", new List<string> { "Iron", "Nickel", "Magnesium" } }
        };

        public static List<IMyAssembler> Assemblers => _assemblers;

        public static void Initialize(IMyGridTerminalSystem gts)
        {
            _assemblers.Clear();
            gts.GetBlocksOfType(_assemblers);
            Program.LogLine($"Initialized AssemblerManager with {_assemblers.Count} assemblers", LogLevel.Info);
        }

        public static void Update()
        {
            if (_assemblers.Count == 0)
                return;

            // Periodic queue redistribution
            _redistributionTimer++;
            if (_redistributionTimer >= REDISTRIBUTION_INTERVAL)
            {
                _redistributionTimer = 0;
                RedistributeQueues();
            }

            // Process one assembler per frame
            if (_currentAssemblerIndex >= _assemblers.Count)
                _currentAssemblerIndex = 0;

            IMyAssembler assembler = _assemblers[_currentAssemblerIndex++];
            ProcessAssembler(assembler);
        }

        public static void ProcessAssembler(IMyAssembler assembler)
        {
            if (assembler.Mode != MyAssemblerMode.Assembly)
                return;

            var queue = new List<MyProductionItem>();
            assembler.GetQueue(queue);

            if (queue.Count == 0)
                return;

            // Get required ingots for the first item in the queue
            List<string> requiredIngots = GetRequiredIngots(queue.First());

            // Get current inventory
            List<MyInventoryItem> items = new List<MyInventoryItem>();
            assembler.GetInventory().GetItems(items);
            
            // Create a dictionary of current inventory amounts by ingot type
            Dictionary<string, int> currentInventory = new Dictionary<string, int>();
            foreach (MyInventoryItem item in items)
            {
                string subtypeId = item.Type.SubtypeId;
                int amount = (int)item.Amount;

                if (currentInventory.ContainsKey(subtypeId))
                    currentInventory[subtypeId] += amount;
                else
                    currentInventory[subtypeId] = amount;
            }

            // Push excess assembled items out of output inventory
            PushExcessAssembledItems(assembler, currentInventory, requiredIngots);

            // Refill required ingots
            foreach (string requiredIngot in requiredIngots)
            {
                int current = 0;
                currentInventory.TryGetValue(requiredIngot, out current);

                // Get per-ingot configuration
                IngotConfig config;
                if (!Program.IngotConfigs.TryGetValue(requiredIngot, out config))
                {
                    // Use defaults if ingot not configured
                    config = new IngotConfig(2000, 1200);
                }

                // Hysteresis: ignore healthy inventories
                if (current >= config.RefillThreshold)
                    continue;

                int needed = config.DesiredAmount - current;
                if (needed <= 0)
                    continue;

                MyItemType ingestType = new MyItemType("MyObjectBuilder_Ingot", requiredIngot);
                InventoryManager.PullItems(ingestType, needed, assembler.GetInventory());
            }
        }

        private static void PushExcessAssembledItems(IMyAssembler assembler, Dictionary<string, int> currentInventory, List<string> requiredIngots)
        {
            // Get the output inventory
            IMyInventory outputInventory = assembler.OutputInventory;
            if (outputInventory == null)
                return;

            // Get items in output inventory
            List<MyInventoryItem> outputItems = new List<MyInventoryItem>();
            outputInventory.GetItems(outputItems);

            // Convert required ingots to a HashSet for O(1) lookup
            HashSet<string> requiredIngotsSet = new HashSet<string>(requiredIngots);

            // Check each item in the output inventory
            foreach (var item in outputItems)
            {
                string itemSubtypeId = item.Type.SubtypeId;
                int itemAmount = (int)item.Amount;

                // Skip items that are required ingots for the current recipe
                if (requiredIngotsSet.Contains(itemSubtypeId))
                    continue;

                // Check if this item exceeds the threshold
                if (itemAmount > OUTPUT_INVENTORY_THRESHOLD)
                {
                    Program.LogLine($"Dumping excess {itemSubtypeId} from {assembler.DisplayName} output inventory (current: {itemAmount}, threshold: {OUTPUT_INVENTORY_THRESHOLD})", LogLevel.Info);

                    // Try component containers first, then cargo containers
                    if (!InventoryManager.DumpItemToContainers(outputInventory, item, InventoryManager.ComponentContainers))
                    {
                        InventoryManager.DumpItemToContainers(outputInventory, item, InventoryManager.CargoContainers);
                    }
                }
            }
        }

        private static List<IMyAssembler> _reusedAssemblerList = new List<IMyAssembler>();
        private static void RedistributeQueues()
        {
            // Accumulate all queues from all assemblers
            Dictionary<MyDefinitionId, MyFixedPoint> accumulatedQueue = AccumulateAllQueues();

            if (accumulatedQueue.Count == 0)
                return;

            
            foreach (var item in accumulatedQueue)
            {
                _reusedAssemblerList.Clear();
                foreach (var assembler in _assemblers)
                {
                    if (assembler.Mode != MyAssemblerMode.Assembly || !assembler.CanUseBlueprint(item.Key))
                        continue;

                    _reusedAssemblerList.Add(assembler);
                }

                if (_reusedAssemblerList.Count == 0)
                    continue;

                MyDefinitionId blueprintId = item.Key;
                double amount = (double)item.Value;
                double perAssembler = amount / _reusedAssemblerList.Count;
                double remainder = amount % _reusedAssemblerList.Count;
                
                for (int i = 0; i < _reusedAssemblerList.Count; i++)
                {
                    IMyAssembler assembler = _reusedAssemblerList[i];
                    assembler.AddQueueItem(blueprintId, perAssembler);
                    if (i < remainder)
                        assembler.AddQueueItem(blueprintId, (double)1);
                }
            }
        }

        private static Dictionary<MyDefinitionId, MyFixedPoint> AccumulateAllQueues()
        {
            Dictionary<MyDefinitionId, MyFixedPoint> accumulatedQueue = new Dictionary<MyDefinitionId, MyFixedPoint>();

            foreach (IMyAssembler assembler in _assemblers)
            {
                if (assembler.Mode != MyAssemblerMode.Assembly)
                    continue;

                var queue = new List<MyProductionItem>();
                assembler.GetQueue(queue);

                foreach (MyProductionItem item in queue)
                {
                    MyDefinitionId blueprintId = item.BlueprintId;
                    if (!accumulatedQueue.ContainsKey(blueprintId))
                        accumulatedQueue[blueprintId] = 0;
                    accumulatedQueue[blueprintId] += item.Amount;

                }
                assembler.ClearQueue();
            }

            return accumulatedQueue;
        }

        private static List<string> GetRequiredIngots(MyProductionItem productionItem)
        {
            string itemSubtypeId = productionItem.BlueprintId.ToString().Split('/')[1];

            List<string> requiredIngots;
            if (_recipeRequirements.TryGetValue(itemSubtypeId, out requiredIngots))
            {
                return requiredIngots;
            }
            Program.LogLine($"No recipe found for {itemSubtypeId}", LogLevel.Warning);
            // Return empty list if item not found in recipes
            return new List<string>();
        }

        public static bool HasQueue(IMyAssembler assembler)
        {
            List<MyProductionItem> queue = new List<MyProductionItem>();
            assembler.GetQueue(queue);
            return queue.Count > 0;
        }
    }
}
