using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sandbox.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame;
using VRageMath;

namespace IngameScript
{

    #region enums
    public enum InfectionType
    {
        Initial,
        Connector,
        Mechanical,
        BuildAndRepair
    }

    public enum InfectionState
    {
        Infecting,
        Infected
    }
    
    #endregion
    
    #region manager
    public static class InfectedGridManager
    {
        
        #region Fields
        public static Dictionary<IMyCubeGrid, InfectedGrid> InfectedGrids = new Dictionary<IMyCubeGrid, InfectedGrid>();
        #endregion
        #region Properties
        public static int InfectedCount => InfectedGrids.Count;
        #endregion
        
        #region Functions
        public static void TryInfectGrid(IMyCubeGrid grid, IMyCubeGrid infector, InfectionType infectionType, Vector3D infectionPosition)
        {
            if (!InfectedGrids.ContainsKey(grid))
            {
                if (infector != null) Program.Log($"Grid {infector.CustomName} infected grid {grid.CustomName}. Infection type: {infectionType}");
                InfectedGrids.Add(grid, new InfectedGrid(grid, infector, infectionType, infectionPosition));
            }
        }

        public static void Update(int frame)
        {
            var list = InfectedGrids.Values.ToList(); // copy to list once
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var grid = list[i];
                if (grid.ActualGrid.Closed)
                {
                    Program.Log($"Grid {grid.ActualGrid.CustomName} no longer exists.");
                    InfectedGrids.Remove(grid.ActualGrid);
                    continue;
                }
                grid.Update(frame);
            }
                
        }
        #endregion

    }
    #endregion
    
    #region instance
    public class InfectedGrid
    {
        
        #region collections
        private List<IMyCubeBlock> _blocks = new List<IMyCubeBlock>();
        private List<IConnectableBlock> _connectors = new List<IConnectableBlock>();
        #endregion
        
        #region intraInfection
        private InfectionState _infectionState = InfectionState.Infecting;
        private IEnumerator _infectionEnumerator;
        #endregion
        
        public IMyCubeGrid ActualGrid;
        private int _frame = 0;
        private int _lastInfectionFinishFrame = 0;
        
        #region infectionRoute
        public IMyCubeGrid Infector;
        public InfectionType InfectionType;
        public double InfectionTime;
        public Vector3D InfectionPosition;
        #endregion
        public InfectedGrid(IMyCubeGrid grid, IMyCubeGrid infector, InfectionType infectionType, Vector3D infectionPosition)
        {
            ActualGrid = grid;
            Infector = infector;
            InfectionType = infectionType;
            InfectionTime = (System.DateTime.UtcNow - new System.DateTime(1970, 1, 1)).TotalSeconds;
            InfectionPosition = infectionPosition;


            InfectThisGrid(grid);
        }
        
        #region update
        public void Update(int frame)
        {
            _frame = frame;
            switch (_infectionState)
            {
                case InfectionState.Infecting:
                    UpdateInfecting();
                    break;
                case InfectionState.Infected:
                    UpdateInfected();
                    break;
            }
        }

        private void UpdateInfecting()
        {
            _infectionEnumerator.MoveNext();
        }

        private void UpdateInfected()
        {
            if (_frame - _lastInfectionFinishFrame >= Config.FramesToRescan)
            {
                Program.Log($"Rescanning grid {ActualGrid.CustomName}");
                InfectThisGrid(ActualGrid);
                return;
            }
            
            int count = _connectors.Count;
            for (var index = count - 1; index >= 0; index--)
            {
                var connector = _connectors[index];
                if (!connector.HasNewTarget()) continue;
                //Program.Log($"Connector {connector.CustomName} new potential infection target: {connector.GetConnectedGrid().CustomName}");
                InfectedGridManager.TryInfectGrid(connector.GetConnectedGrid(), ActualGrid,
                    connector.BaseInfectionType, connector.Position);
            }
        }
        #endregion
        
        #region intraInfection
        private void InfectThisGrid(IMyCubeGrid grid)
        {
            _infectionState = InfectionState.Infecting;
            _connectors.Clear();
            _blocks.Clear();
            _infectionEnumerator = ScanGridCoroutine(grid);
            
            ProfileRunIEnumerator(_infectionEnumerator);
        }
        
        private IEnumerator ScanGridCoroutine(IMyCubeGrid grid)
        {
            var min = grid.Min;
            var max = grid.Max;

            int cxPerTick = Config.BlocksScannedPerTick;
            int complexity = 0;

            for (int x = min.X; x <= max.X; x++)
            {
                for (int y = min.Y; y <= max.Y; y++)
                {
                    for (int z = min.Z; z <= max.Z; z++)
                    {
                        InfectBlock(grid, x, y, z);
                        complexity++;

                        if (complexity % cxPerTick == 0)
                        {
                            Program.Log($"Scanning grid {grid.CustomName}: {complexity} blocks scanned");
                            yield return null; // pause until next tick
                        }
                    }
                }
            }

            // Finished scanning
            Program.Log($"Block scan finished: {complexity} blocks scanned");
            _lastInfectionFinishFrame = _frame;
            _infectionState = InfectionState.Infected;
        }
        

        private void InfectBlock(IMyCubeGrid grid, int x, int y, int z)
        {
            var pos = new Vector3I(x, y, z);
            var slim = grid.GetCubeBlock(pos);
            
            var fat = slim?.FatBlock;
            if (fat == null) return;

            _blocks.Add(fat);
            HandleConnectors(fat);
        }
        #endregion
        
        #region interInfection
        private void HandleConnectors(IMyCubeBlock fat)
        {
            var connector = fat as IMyShipConnector;
            if (connector != null)
            {
                _connectors.Add(new ConnectorBlock(connector));
                return;
            }

            var bottom = fat as IMyMechanicalConnectionBlock;
            if (bottom != null)
            {
                _connectors.Add(new MechanicalBottom(bottom));
                return;
            }

            var top = fat as IMyAttachableTopBlock;
            if (top != null)
            {
                _connectors.Add(new MechanicalTop(top));
            }
        }
        
        #endregion
        
        #region helpers

        private void ProfileRunIEnumerator(IEnumerator enumerator)
        {
            var startTime = System.DateTime.UtcNow;
            enumerator.MoveNext();
            var elapsed = System.DateTime.UtcNow - startTime;
            Program.Log($"Block scan took {elapsed.TotalMilliseconds:0.00} ms");
        }
        #endregion
    }
    
    #endregion
}