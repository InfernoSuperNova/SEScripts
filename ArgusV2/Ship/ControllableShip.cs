using System.Collections.Generic;
using IngameScript.Ship.Components;
using Sandbox.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame;
using VRageMath;

namespace IngameScript.Ship
{
    public class ControllableShip : SupportingShip
    {
        private GyroManager _gyroManager;
        private List<IMyUserControllableGun> _guns; // TODO: Abstract into a guns handler
        private List<IMyLargeTurretBase> _turrets; // TODO: Abstract into a turrets handler
        // TODO: Handler for gravity drive and thrusters (probably just copy and paste from arguslite)

        private TrackableShip _currentTarget;
        
        
        public IMyShipController Controller { get; set; }
        public Vector3D Forward => Controller.WorldMatrix.Forward;
        public Vector3D Up => Controller.WorldMatrix.Up;
        
        public ControllableShip(IMyCubeGrid _grid, List<IMyTerminalBlock> blocks) : base(_grid, blocks)
        {
            _gyroManager = new GyroManager(blocks);
        }

        public override void EarlyUpdate(int frame)
        {
            base.EarlyUpdate(frame);
        }
    }
}