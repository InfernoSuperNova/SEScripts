
using IngameScript.Helper;
using IngameScript.Helper.Log;
using IngameScript.SConfig.Database;
using IngameScript.TruncationWrappers;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using VRage.Game;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript.Ship.Components
{
    public enum GunReloadType
    {
        Normal,
        NeedsCharging
    }
    
    public enum GunFireType
    {
        Normal,
        Delay
    }

    public enum GunState
    {
        Firing,
        Cancelling,
        ReadyToFire,
        Reloading,
        Recharging,
        NonFunctional
    }
    
    
    public class Gun
    {
        private static readonly MyDefinitionId ElectricityId =
            new MyDefinitionId(typeof(MyObjectBuilder_GasProperties), "Electricity");
        
        
        private IMyUserControllableGun _gun;
        private GunReloadType _reloadType;
        private GunFireType _fireType;
        private MyResourceSinkComponent _gunSinkComponent;
        private int _reloadTimerId;
        private int _firingTimerId;
        private CachedValue<GunState> _state;
        
        
        
        
        private bool _cancelled;
        private GunData _gunData;
        private GunManager _manager;

        public Gun(IMyUserControllableGun gun, GunManager manager)
        {
            
            var blockDefinition = gun.BlockDefinition;
            Program.LogLine($"Set up new gun {blockDefinition}", LogLevel.Trace);
            var gunData = GunData.Get(blockDefinition.SubtypeIdAttribute);
            if (gunData == GunData.DefaultGun) gunData = GunData.Get(blockDefinition.TypeIdString);
            _gunData = gunData;
            _manager = manager;
            
            _gun = gun;
            _gunSinkComponent = gun.Components.Get<MyResourceSinkComponent>();
            _reloadType = _gunData.ReloadType;
            _fireType = _gunData.FireType;

            _state = new CachedValue<GunState>(EvaluateState);
        }
        
        public AT_Vector3D GridPosition => (Vector3)(_gun.Min + _gun.Max) / 2 * _manager.ThisShip.GridSize;
        /// <summary>
        /// GetPosition method.
        /// </summary>
        /// <returns>The result of the operation.</returns>
        /// <summary>
        /// GetPosition method.
        /// </summary>
        /// <returns>The result of the operation.</returns>
        public AT_Vector3D WorldPosition => _gun.GetPosition();
        /// <summary>
        /// Gets or sets the Direction.
        /// </summary>
        /// <summary>
        /// Gets or sets the Direction.
        /// </summary>
        public AT_Vector3D Direction { get; set; }
        public float Velocity => _gunData.ProjectileData.ProjectileVelocity;
        public float Acceleration => _gunData.ProjectileData.Acceleration;
        public float MaxVelocity => _gunData.ProjectileData.MaxVelocity;
        public float MaxRange => _gunData.ProjectileData.MaxRange;

        public GunData GunData => _gunData;

        public GunState State => _state.Value;

        public AT_Vector3D Forward => _gun.WorldMatrix.Forward;

        public void EarlyUpdate(int frame)
        {
            _state.Invalidate();
        }

        public void LateUpdate(int frame)
        {
            
        }

        public bool Fire()
        {
            if (State != GunState.ReadyToFire) return false;
            
            _gun.ShootOnce();
            _reloadTimerId = TimerManager.Start(_gunData.ReloadTimeFrames, OnReloadComplete);
            if (_fireType == GunFireType.Delay)
            {
                _firingTimerId = TimerManager.Start(_gunData.FireTimeFrames, OnFireComplete);
            }
            else
            {
                _firingTimerId = TimerManager.Start(0, OnFireComplete);
            }
            return true;
        }


        public bool ForceCancel()
        {
            if (State != GunState.Firing) return false;
            _gun.Enabled = false; // The only way to force a gun to not fire is to turn it off
            TimerManager.Start(0, ReenableGun);
            _cancelled = true;
            return true;
        }
        
        public bool CancelIfAboutToFire()
        {

            if (State != GunState.Firing) return false;


            if (TimerManager.GetRemaining(_firingTimerId) > 1) return false;
            _gun.Enabled = false; // The only way to force a gun to not fire is to turn it off
            TimerManager.Start(0, ReenableGun);
            _cancelled = true;
            return true;

        }
        
        
        
        
        public AT_Vector3D GetFireVelocity(AT_Vector3D shipVelocity)
        {
            AT_Vector3D combinedVelocity = shipVelocity + Direction * Velocity;
            if (combinedVelocity.LengthSquared() > Velocity * Velocity)
                combinedVelocity = combinedVelocity.Normalized() * Velocity;
            return combinedVelocity;
        }
        
        
        
        private GunState EvaluateState()
        {
            bool functional = _gun.IsFunctional;
            if (!functional) return GunState.NonFunctional;
                
            if (_fireType == GunFireType.Delay && TimerManager.IsActive(_firingTimerId)) return _cancelled ? GunState.Cancelling : GunState.Firing;
                
            switch (_reloadType)
            {
                case GunReloadType.Normal:
                    if (TimerManager.IsActive(_reloadTimerId)) return GunState.Reloading;
                    break;
                case GunReloadType.NeedsCharging:
                    if (TimerManager.IsActive(_reloadTimerId)) return GunState.Reloading; // Railguns for example have a 4 second refractory period after attempting to fire so we start this timer too anyway
                    if (_gunSinkComponent.CurrentInputByType(ElectricityId) > 0.02f) return GunState.Recharging;
                    break;
            }
            return GunState.ReadyToFire;
        }

        private void OnFireComplete()
        {
            if (_cancelled)
            {
                _cancelled = false;
                return;
            }
        }

        private void OnReloadComplete()
        {
            
        }

        private void ReenableGun()
        {
            _gun.Enabled = true;
        }
    }
}