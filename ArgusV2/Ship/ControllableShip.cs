using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript.Ship
{
    public class ControllableShip : SupportingShip
    {
        private List<IMyGyro> _gyroscopes; // TODO: Abstract into a gyro handler
        private List<IMyUserControllableGun> _guns; // TODO: Abstract into a guns handler
        private List<IMyLargeTurretBase> _turrets; // TODO: Abstract into a turrets handler
        // TODO: Handler for gravity drive and thrusters (probably just copy and paste from arguslite)

        private TrackableShip _currentTarget;
        
        public ControllableShip(IMyCubeGrid _grid) : base(_grid)
        {
            
        }

        public override void EarlyUpdate(int frame)
        {
            base.EarlyUpdate(frame);
        }
    }
}