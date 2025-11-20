using System;
using System.Collections.Generic;
using EmptyKeys.UserInterface.Controls;
using IngameScript.Database;
using IngameScript.Helper;
using Sandbox.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame;
using VRageMath;

namespace IngameScript.Ship.Components
{

    public enum PositionValidity
    {
        Fallback,
        Ready,
        Firing
    }

    public enum FireType
    {
        FireWhenReady,
        WaitForAll,
        Volley
    }
    
    public class GunManager
    {
        private List<Gun> _guns = new List<Gun>();
        
        private List<Gun> _currentFiringGroup = new List<Gun>();

        private Vector3D _firingReferencePosition;
        private PositionValidity _fireGroupValidity;
        private int _fireGroupCount;
        private GunData _gunData;
        
        public GunManager(List<IMyTerminalBlock> blocks, ControllableShip thisShip)
        {
            foreach (var block in blocks)
            {
                var gun = block as IMyUserControllableGun;
                if (gun != null) _guns.Add(new Gun(gun, this));
            }

            ThisShip = thisShip;
        }

        public ControllableShip ThisShip { get; }
        public IMyCubeGrid Grid => ThisShip.Controller.CubeGrid;
        public int GunCount => _guns.Count;
        
        public Vector3D FireRefPos => _firingReferencePosition;
        public PositionValidity FireGroupValidity => _fireGroupValidity;
        public int FireGroupCount => _fireGroupCount;

        public void EarlyUpdate(int frame)
        {
            foreach (var gun in _guns) gun.EarlyUpdate(frame);
        }

        public void SelectFiringGroup()
        {
            var validity = PositionValidity.Fallback;
            
            var fireGroupsReady = new Dictionary<GunData, List<Gun>>();
            var fireGroupsFiring = new Dictionary<GunData, List<Gun>>();
            var readyCount = 0;
            var firingCount = 0;
            
            foreach (var gun in _guns)
            {
                switch (gun.State)
                {
                    case GunState.ReadyToFire:
                        if (!fireGroupsReady.ContainsKey(gun.GunData)) fireGroupsReady.Add(gun.GunData, new List<Gun>());
                        fireGroupsReady[gun.GunData].Add(gun);
                        readyCount++;
                        break;
                    case GunState.Firing:
                        if (!fireGroupsFiring.ContainsKey(gun.GunData)) fireGroupsFiring.Add(gun.GunData, new List<Gun>());
                        fireGroupsFiring[gun.GunData].Add(gun);
                        firingCount++;
                        break;
                }
            }
            Program.Log(readyCount);
            
            var groupDict = fireGroupsFiring; // TODO: Make sure there's always a fallback
            if (readyCount > 0)
            {
                //_fireGroupCount = readyCount;
                _fireGroupValidity = PositionValidity.Ready;
                groupDict = fireGroupsReady;
                //_firingReferencePosition = Vector3D.Transform(readyPos / readyCount, ThisShip.WorldMatrix);
            }
            
            
            
            
            
            if (firingCount > 0) // We do firing last so it is always prioritized
            {
                //_fireGroupCount = firingCount;
                _fireGroupValidity = PositionValidity.Firing;
                groupDict = fireGroupsFiring;
                //_firingReferencePosition = Vector3D.Transform(firingPos / firingCount, ThisShip.WorldMatrix);
            }
            
            
            

            var target = ThisShip.GetTargetPosition();
            var ourPosApprox = ThisShip.Position;
            var rangeSquared = (target - ourPosApprox).LengthSquared();
            
            foreach (var group in groupDict)
            {
                var data = group.Key;
                var guns = group.Value;
                var maxRange = data.ProjectileData.MaxRange;
                if (maxRange * maxRange < rangeSquared) continue; // Completely ignore gun cases that are out of range
                _currentFiringGroup = guns;
                _gunData = data;
                break;
            }

            _fireGroupCount = _currentFiringGroup.Count;

            if (_fireGroupCount == 0)
            {
                _firingReferencePosition = ThisShip.Position;
                return;
            }
            
            Vector3D average = Vector3D.Zero;

            foreach (var gun in _currentFiringGroup)
            {
                average += gun.GridPosition;
            }

            average /= _fireGroupCount;

            _firingReferencePosition = Vector3D.Transform(average, ThisShip.WorldMatrix);

        }
        

        public void LateUpdate(int frame)
        {
            foreach (var gun in _guns) gun.LateUpdate(frame);
        }

        
        
        // API
        public void FireAll()
        {
            foreach (var gun in _guns) gun.Fire();
        }

        public void CancelAll()
        {
            foreach (var gun in _guns) gun.ForceCancel();
        }
        
        public void TryFire()
        {
            foreach (var gun in _guns) gun.Fire();
        }

        public void TryCancel()
        {
            foreach (var gun in _guns) gun.CancelIfAboutToFire();
        }


        public Vector3D GetBallisticSolution()
        {

            if (_gunData == null) return Vector3D.Zero;
            var maxSpeed = _gunData.ProjectileData.MaxVelocity;
            var target = ThisShip.GetTargetPosition();
            var displacement = target - _firingReferencePosition;

            var refGun = _currentFiringGroup[0];

            if (refGun == null) return Vector3D.Zero;

            var gravity = Vector3D.Zero; // TODO: This
            bool hasGravity = false;

            return Solver.BallisticSolver(maxSpeed, refGun.GetFireVelocity(ThisShip.Velocity),
                _gunData.ProjectileData.Acceleration * refGun.Forward,
                displacement,
                ThisShip.CurrentTarget.Position, ThisShip.CurrentTarget.Velocity, ThisShip.CurrentTarget.Acceleration,
                Vector3D.Zero, false, gravity, hasGravity);
        }
    }
}