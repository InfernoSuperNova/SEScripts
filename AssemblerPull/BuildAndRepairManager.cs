using System.Collections.Generic;
using System.Linq;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript
{
    public static class BuildAndRepairManager
    {
        private static int _currentBarIndex = 0;


        private static IMyCubeGrid _thisGrid;
        private static List<IMyTerminalBlock> _bars = new List<IMyTerminalBlock>();
        private static Dictionary<string, int> _costs = new Dictionary<string, int>();
        private static HashSet<IMySlimBlock> _alreadyWeldingBlocks = new HashSet<IMySlimBlock>();
        
        public static void Initialize(IMyGridTerminalSystem gridTerminalSystem, IMyCubeGrid thisGrid)
        {
            _thisGrid = thisGrid;
            _bars.Clear();
            List<IMyTerminalBlock> bars = new List<IMyTerminalBlock>();
            gridTerminalSystem.GetBlocksOfType(bars);
            foreach (var bar in bars)
            {
                try 
                {
                    var _ = bar.GetValue<long>("BuildAndRepair.Mode");
                    _bars.Add(bar);
                }
                catch { }
            }
        }
        public static void Update()
        {
            if (_bars.Count == 0) return;
            if (_currentBarIndex >= _bars.Count)
            {
                _currentBarIndex = 0;
                _alreadyWeldingBlocks.Clear();
            }

            IMyTerminalBlock bar = _bars[_currentBarIndex++];
            ProcessBar(bar);
        }

        private static void ProcessBar(IMyTerminalBlock bar)
        {
            Program.LogLine($"Iterating bar: {bar.CustomName}");
            bool grindingProcessed = ProcessGrinding(bar);
            bool weldingProcessed = false;
            if (!grindingProcessed) weldingProcessed = ProcessWelding(bar);
            
            if (!weldingProcessed) InventoryManager.DumpExcessItems(bar.GetInventory(), new List<string>());
        }

        private static bool ProcessWelding(IMyTerminalBlock bar)
        {
            //Program.LogLine($"Iterating bar: {bar.CustomName}");
            var targets = bar.GetValue<List<IMySlimBlock>>("BuildAndRepair.PossibleTargets");
            if (targets == null || targets.Count == 0) return false;
            var block = GetTargetBlock(targets);
            bar.SetValue("BuildAndRepair.CurrentPickedTarget", block);
            
            //var block = bar.GetValue<IMySlimBlock>("BuildAndRepair.CurrentPickedTarget");
            if (block == null) return false;
            _costs.Clear();
            block.GetMissingComponents(_costs);

            var inv = bar.GetInventory();
            if (_costs.Count == 0) // This is a projected block so we want to grab one of every starting component (construction comp, steel plate, interior plate, girder etc)
            {
                InventoryManager.TopUpInventory(inv, new MyItemType("MyObjectBuilder_Component", "Construction"), 200);
                InventoryManager.TopUpInventory(inv, new MyItemType("MyObjectBuilder_Component", "SteelPlate"), 200);
                InventoryManager.TopUpInventory(inv, new MyItemType("MyObjectBuilder_Component", "InteriorPlate"), 200);
                InventoryManager.TopUpInventory(inv, new MyItemType("MyObjectBuilder_Component", "Girder"), 200);
                InventoryManager.TopUpInventory(inv, new MyItemType("MyObjectBuilder_Component", "MetalGrid"), 200);
            }
            foreach (var item in _costs)
            {
                Program.LogLine($"Missing {item.Key}: {item.Value}");
                InventoryManager.TopUpInventory(inv, new MyItemType("MyObjectBuilder_Component", item.Key), item.Value);
            }

            return true;
        }
        
        private static bool ProcessGrinding(IMyTerminalBlock bar)
        {
            var targets = bar.GetValue<List<IMySlimBlock>>("BuildAndRepair.PossibleGrindTargets");
            if (targets == null || targets.Count == 0) return false;
            Program.LogLine($"Iterating grinding targets for bar: {bar.CustomName}");
            var block = GetTargetBlock(targets);
            bar.SetValue("BuildAndRepair.CurrentPickedGrindTarget", block);
            
            
            return true;
        }

        private static IMySlimBlock GetTargetBlock(List<IMySlimBlock> targets)
        {
            foreach (IMySlimBlock block in targets)
            {
                if (!_alreadyWeldingBlocks.Contains(block) && (_thisGrid.GetCubeBlock(block.Position) == null || !_thisGrid.GetCubeBlock(block.Position).IsFullIntegrity ))
                {
                    _alreadyWeldingBlocks.Add(block);
                    Program.LogLine($"Selected block: {block}");
                    return block;
                }
            }
            return targets.FirstOrDefault();
        }
    }
}