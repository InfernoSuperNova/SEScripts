using System;
using System.Collections.Generic;
using IngameScript.Helper.Log;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;

namespace IngameScript.Ship.Components.Missiles.LaunchMechanisms
{

    public enum MissileLauncherState
    {
        Waiting,
        Launching,
        Done,
        Failed
    }
    
    /// <summary>
    /// Represents a missile launcher with its launch mechanism and associated missile
    /// </summary>
    /// <summary>
    /// Item class.
    /// </summary>
    /// <summary>
    /// Item class.
    /// </summary>
    public class MissileLauncher
    {
        private MissileLauncherState _state = MissileLauncherState.Waiting;
        private LaunchMechanism _launchMechanism;
        private readonly Missile _missile;

        private readonly IMyThrust _launchPulseThruster; // Only applicable if LaunchMechanism is PulsedThruster
        private readonly List<IMyUserControllableGun> _launchWeapons; // Only applicable if LaunchMechanism is Weapon
        private readonly MissileFinder _missileFinder;
        private Action<MissileLauncher, MissileCommand> _onMissileLaunchSuccess;
        private readonly MissileCommandContext _missileCommandContext;

        public MissileLauncher(LaunchMechanism launchMechanism, Missile missile, IMyThrust launchPulseThruster, List<IMyUserControllableGun> launchWeapons, MissileCommandContext missileCommandContext, MissileFinder missileFinder)
        {
            _launchMechanism = launchMechanism;
            _missile = missile;
            _launchPulseThruster = launchPulseThruster;
            _launchWeapons = launchWeapons;
            _missileCommandContext = missileCommandContext;
            _missileFinder = missileFinder;
            
            

            
        }

        /// <summary>
        /// Gets the launch mechanism type for this launcher
        /// </summary>
        internal LaunchMechanism LaunchMechanism => _launchMechanism;

        /// <summary>
        /// Gets the missile associated with this launcher
        /// </summary>
        public Missile Missile => _missile;
        
        /// <summary>
        /// Gets the launch control mode for this launcher
        /// </summary>
        public MissileCommandContext MissileCommandContext => _missileCommandContext;

        /// <summary>
        /// Gets the launch pulse thruster (only applicable if LaunchMechanism is PulsedThruster)
        /// </summary>
        public IMyThrust LaunchPulseThruster => _launchPulseThruster;

        /// <summary>
        /// Gets the launch weapons (only applicable if LaunchMechanism is Weapon)
        /// </summary>
        public List<IMyUserControllableGun> LaunchWeapons => _launchWeapons;

        public bool HasSuccessfullyDetached => _missile.ReferenceGrid != _missileFinder.ReferenceGrid;
        public MissileLauncherState State => _state;

        public void Launch(Action<MissileLauncher, MissileCommand> onMissileLaunchSuccess, MissileCommand missileCommand)
        {
            if (_state == MissileLauncherState.Waiting)
            {
                _state = MissileLauncherState.Launching;
                _onMissileLaunchSuccess = (launcher, request) => onMissileLaunchSuccess(launcher, missileCommand);
        
                // Execute launch mechanism immediately instead of waiting for LateUpdate
                EvaluateLaunchMechanism();


                switch (_launchMechanism)
                {
                    case LaunchMechanism.PulsedThruster:
                        _launchPulseThruster.ThrustOverridePercentage = 1.0f;
                        break;
                }
                
            }
        }
        public void EarlyUpdate(int frame)
        {
            if (_state == MissileLauncherState.Launching)
            {
                EvaluateLaunchMechanism();
                if (HasSuccessfullyDetached)
                {
                    _state = MissileLauncherState.Done;
                    OnSuccessfulDetach();
                    return;
                }
                if (_launchMechanism == LaunchMechanism.None)
                {
                    _state = MissileLauncherState.Failed;
                    Program.LogLine($"(MissileLauncher) '{_missileFinder.FriendlyName}' - Launch mechanism failed (no launch mechanism set)", LogLevel.Error);
                    // We don't clean up here and instead let the launcher hang on to the missile for analysis later
                }
            }
        }
        
        public void LateUpdate(int frame)
        {
            
        }

        private void EvaluateLaunchMechanism()
        {
            // Check flag values on launch mechanism
            if (_launchMechanism.HasFlag(LaunchMechanism.Mechanical))
            {
                MechanicalLaunchMechanism();
            }
            if (_launchMechanism.HasFlag(LaunchMechanism.Connector))
            {
                ConnectorLaunchMechanism();
            }
            if (_launchMechanism.HasFlag(LaunchMechanism.MergeBlock))
            {
                MergeBlockLaunchMechanism();
            }
            if (_launchMechanism.HasFlag(LaunchMechanism.PulsedThruster))
            {
                PulsedThrusterLaunchMechanism();
            }
            if (_launchMechanism.HasFlag(LaunchMechanism.Weapon))
            {
                WeaponLaunchMechanism();
            }
        }

        #region Launch Mechanisms
        /// <summary>
        /// MechanicalLaunchMechanism method.
        /// </summary>
        /// <returns>The result of the operation.</returns>
        /// <summary>
        /// MechanicalLaunchMechanism method.
        /// </summary>
        /// <returns>The result of the operation.</returns>
        private void MechanicalLaunchMechanism()
        {
            foreach (var block in _missile.Blocks)
            {
                var connector = block as IMyMechanicalConnectionBlock;
                if (connector != null && connector.IsAttached)
                {
                    connector.Detach();
                }
            }

            foreach (var block in _missile.Blocks)
            {
                var top = block as IMyAttachableTopBlock;
                if (top != null && top.IsAttached)
                {
                    top.Base.Detach();
                }
            }
            _launchMechanism &= ~LaunchMechanism.Mechanical;
        }
        private void ConnectorLaunchMechanism()
        {
            foreach (var block in _missile.Blocks)
            {
                var connector = block as IMyMechanicalConnectionBlock;
                if (connector != null && connector.IsAttached)
                {
                    connector.Detach();
                }
            }

            _launchMechanism &= ~LaunchMechanism.Connector;
        }
        private void MergeBlockLaunchMechanism()
        {
            foreach (var block in _missile.Blocks)
            {
                var mergeBlock = block as IMyShipMergeBlock;
                if (mergeBlock != null && mergeBlock.IsConnected)
                {
                    mergeBlock.Enabled = false;
                }
            }

            _launchMechanism &= ~LaunchMechanism.MergeBlock;
        }

        
        private void PulsedThrusterLaunchMechanism()
        {
            if (Program.RNG.NextDouble() < 0.5) // Can't explain it but it works
            _launchPulseThruster.Enabled = !_launchPulseThruster.Enabled;
            
        }

        private void WeaponLaunchMechanism()
        {
            foreach (var weapon in _launchWeapons)
            {
                weapon.ShootOnce();
            }

            _launchMechanism &= ~LaunchMechanism.Weapon;
        }

        #endregion
        /// <summary>
        /// OnSuccessfulDetach method.
        /// </summary>
        /// <returns>The result of the operation.</returns>
        /// <summary>
        /// OnSuccessfulDetach method.
        /// </summary>
        /// <returns>The result of the operation.</returns>
        private void OnSuccessfulDetach()
        {
            _missileFinder.MarkFired();
            _onMissileLaunchSuccess?.Invoke(this, new MissileCommand());
            
            
            _launchPulseThruster.ThrustOverridePercentage = 0.0f;
            _launchPulseThruster.Enabled = false;
        }
        
    }
}