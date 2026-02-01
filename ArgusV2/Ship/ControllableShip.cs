using System.Collections.Generic;
using IngameScript.Helper;
using IngameScript.Helper.Log;
using IngameScript.Ship.Components;
using IngameScript.Ship.Components.Missiles;
using IngameScript.Ship.Components.Propulsion;
using IngameScript.TruncationWrappers;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame;
using VRageMath;
using Base6Directions = VRageMath.Base6Directions;

namespace IngameScript.Ship
{
    /// <summary>
    /// ControllableShip class.
    /// </summary>
    /// <summary>
    /// ControllableShip class.
    /// </summary>
    public class ControllableShip : SupportingShip
    {
        
        #region Fields

        private readonly GunManager _guns;
        private readonly GyroManager _gyroManager;
        private readonly FireController _fireController;
        private readonly PropulsionController _propulsionController;
        private readonly MissileManager _missileManager;
        private readonly ControllerFinder _controllerFinder;
        private List<IMyLargeTurretBase> _turrets; // TODO: Abstract into a turrets handler, assign
        
        private CachedValue<AT_Vector3D> _gravity;
        private CachedValue<MyShipMass> _mass;
        private CachedValue<AT_Vector3D> _localCenterOfMass;
        private CachedValue<IMyShipController> _controller;
        #endregion
        
        public ControllableShip(IMyCubeGrid grid, List<IMyTerminalBlock> blocks, List<IMyTerminalBlock> trackerBlocks) : base(grid, trackerBlocks)
        {
            Program.LogLine("New ControllableShip : SupportingShip : ArgusShip", LogLevel.Debug);
            _controllerFinder = new ControllerFinder(blocks);
            _controller = new CachedValue<IMyShipController>(() => _controllerFinder.Get());
            // Without a controller, the ship is virtually useless
            if (Controller == null)
            {
                Program.LogLine($"WARNING: Controller not present in group: {Config.String.GroupName}", LogLevel.Warning);
                return;
            }
            _gyroManager = new GyroManager(blocks);
            _guns = new GunManager(blocks, this);
            _fireController = new FireController(this, _guns);
            
            _gravity = new CachedValue<AT_Vector3D>(() => Controller.GetNaturalGravity());
            _mass = new CachedValue<MyShipMass>(() => Controller.CalculateShipMass());
            _localCenterOfMass = new CachedValue<AT_Vector3D>(() =>
                AT_Vector3D.Transform(Controller.CenterOfMass, MatrixD.Invert(WorldMatrix)));

                
            _propulsionController = new PropulsionController(blocks, this);
            _missileManager = new MissileManager(blocks, this);
        }
        
        #region Properties

        /// <summary>
        /// Gets the current controller of the grid.
        /// This is subject to change on a per-frame basis.
        /// </summary>
        /// <summary>
        /// Gets or sets the Controller.
        /// </summary>
        /// <summary>
        /// Gets or sets the Controller.
        /// </summary>
        public IMyShipController Controller => _controller.Value;
        
        /// <summary>
        /// Gets the world matrix of the grid.
        /// Should only be used for transforms of directions and positions in/out of grid local space.
        /// </summary>
        public MatrixD WorldMatrix => _grid.WorldMatrix;
        /// <summary>
        /// Gets the controller centric forward of the grid.
        /// This is more useful and player centric as the controller can be arbitrarily orientated in any of 24 possible options.
        /// </summary>
        public AT_Vector3D WorldForward => Controller.WorldMatrix.Forward; 
        /// <summary>
        /// Gets the controller centric up of the grid.
        /// This is more useful and player centric as the controller can be arbitrarily orientated in any of 24 possible options.
        /// </summary>
        public AT_Vector3D WorldUp => Controller.WorldMatrix.Up;
        /// <summary>
        /// Gets the forward of the controller relative to its own internal orientation in the grid.
        /// </summary>
        /// <summary>
        /// LocalForward method.
        /// </summary>
        /// <param name="int">The int parameter.</param>
        /// <returns>The result of the operation.</returns>
        /// <summary>
        /// LocalForward method.
        /// </summary>
        /// <param name="int">The int parameter.</param>
        /// <returns>The result of the operation.</returns>
        public Vector3 LocalForward => Base6Directions.Directions[(int)Controller.Orientation.Forward];
        /// <summary>
        /// Gets the directional enum of the forward of the controller relative to its own internal orientation in the grid.
        /// </summary>
        /// <summary>
        /// LocalDirectionForward method.
        /// </summary>
        /// <param name="Direction">The Direction parameter.</param>
        /// <returns>The result of the operation.</returns>
        /// <summary>
        /// LocalDirectionForward method.
        /// </summary>
        /// <param name="Direction">The Direction parameter.</param>
        /// <returns>The result of the operation.</returns>
        public Direction LocalDirectionForward => (Direction)Controller.Orientation.Forward;
        /// <summary>
        /// Gets the currently targeted grid, if applicable.
        /// </summary>
        /// <summary>
        /// Gets or sets the CurrentTarget.
        /// </summary>
        /// <summary>
        /// Gets or sets the CurrentTarget.
        /// </summary>
        public TrackableShip CurrentTarget { get; private set; }

        /// <summary>
        /// Gets if the controller currently has a target.
        /// </summary>
        public bool HasTarget => CurrentTarget != null;

        /// <summary>
        /// Gets the current natural gravity at the position of the controller.
        /// </summary>
        public AT_Vector3D Gravity => _gravity.Value;

        /// <summary>
        /// Gets the mass details of the grid. Subject to inaccuracy if being changed rapidly.
        /// </summary>
        public MyShipMass Mass => _mass.Value;

        public AT_Vector3D LocalCenterOfMass => _localCenterOfMass.Value;

        public IMyCubeGrid Grid => _grid;

        #endregion
        
        // IMPORTANT: All components must have their EarlyUpdate/LateUpdate called here
        // // in the correct order. Do not forget to add new components!
        #region Updates
        // Early Update is called in a deterministic, but undefined order. Designed for gathering data.
        // All Early Updates are guaranteed to be called before any Late Updates.
        /// <summary>
        /// EarlyUpdate method.
        /// </summary>
        /// <param name="frame">The frame parameter.</param>
        /// <returns>The result of the operation.</returns>
        /// <summary>
        /// EarlyUpdate method.
        /// </summary>
        /// <param name="frame">The frame parameter.</param>
        /// <returns>The result of the operation.</returns>
        public override void EarlyUpdate(int frame)
        {
            _controller.Invalidate();
            base.EarlyUpdate(frame);
            _guns.EarlyUpdate(frame);
            _propulsionController.EarlyUpdate(frame);
            _missileManager.EarlyUpdate(frame);
            
            
            if ((frame + RandomUpdateJitter) % Polling.GetFramesBetweenPolls(PollFrequency) == 0)
            {
                _gravity.Invalidate();
                _mass.Invalidate();
                _localCenterOfMass.Invalidate();
            }
        }
        // Late Update is called in a deterministic, but undefined order. Designed for acting on data.
        // All Late Updates are guaranteed to be called before any Early Updates.
        /// <summary>
        /// LateUpdate method.
        /// </summary>
        /// <param name="frame">The frame parameter.</param>
        /// <returns>The result of the operation.</returns>
        /// <summary>
        /// LateUpdate method.
        /// </summary>
        /// <param name="frame">The frame parameter.</param>
        /// <returns>The result of the operation.</returns>
        public override void LateUpdate(int frame)
        {
            base.LateUpdate(frame);
            if (HasTarget)
            {
                
                var solution = _fireController.ArbitrateFiringSolution();
                if (solution.TargetPosition == AT_Vector3D.Zero) _gyroManager.ResetGyroOverrides(); // TODO: Don't spam this
                else _gyroManager.Rotate(ref solution);
            }
            _guns.LateUpdate(frame); 
            _propulsionController.LateUpdate(frame);
            _missileManager.LateUpdate(frame);
        }
        #endregion
        
        #region Targeting
        
        /// <summary>
        /// Gets the ship to search for a new target off it's bow, and set it as the current target if valid. 
        /// </summary>
        /// <summary>
        /// Target method.
        /// </summary>
        /// <returns>The result of the operation.</returns>
        /// <summary>
        /// Target method.
        /// </summary>
        /// <returns>The result of the operation.</returns>
        public void Target()
        {
            
            CurrentTarget = ShipManager.GetForwardTarget(this, Config.Behavior.LockRange, Config.Behavior.LockAngle);
            if (CurrentTarget == null)
            {
                _gyroManager.ResetGyroOverrides();
                Program.LogLine("Couldn't find new target", LogLevel.Warning);
            }
            else
            {
                Program.LogLine("Got new target", LogLevel.Info);
            }
        }
        
        /// <summary>
        /// Unsets the current target on a ship and allows it to resume normal control.
        /// </summary>
        /// <summary>
        /// UnTarget method.
        /// </summary>
        /// <returns>The result of the operation.</returns>
        /// <summary>
        /// UnTarget method.
        /// </summary>
        /// <returns>The result of the operation.</returns>
        public void UnTarget()
        {
            CurrentTarget = null;
            _gyroManager.ResetGyroOverrides();
        }

        /// <summary>
        /// Gets the current position of the target.
        /// </summary>
        /// <returns>The targets position, or 0 if invalid. </returns>
        /// <summary>
        /// GetTargetPosition method.
        /// </summary>
        /// <returns>The result of the operation.</returns>
        /// <summary>
        /// GetTargetPosition method.
        /// </summary>
        /// <returns>The result of the operation.</returns>
        public AT_Vector3D GetTargetPosition()
        {
            return CurrentTarget?.Position ?? AT_Vector3D.Zero;
        }
        #endregion

        public void RequestMissileFireManual() => _missileManager.RequestMissileFireManual();

        public void RefreshMissilePatterns()
        {
            _missileManager.RefreshMissilePatterns();
        }
    }
}