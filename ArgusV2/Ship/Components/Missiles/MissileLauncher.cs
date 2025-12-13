using System;
using System.Collections.Generic;
using IngameScript.Helper.Log;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;

namespace IngameScript.Ship.Components.Missiles.LaunchMechanisms
{

    enum MissileLauncherState
    {
        Waiting,
        Launching,
        Done,
        Failed
    }
    
    /// <summary>
    /// Represents a missile launcher with its launch mechanism and associated missile
    /// </summary>
    public class MissileLauncher
    {
        private MissileLauncherState _state = MissileLauncherState.Waiting;
        private LaunchMechanism _launchMechanism;
        private readonly Missile _missile;

        private readonly List<IMyThrust> _launchPulseThrusters; // Only applicable if LaunchMechanism is PulsedThruster
        private readonly List<IMyUserControllableGun> _launchWeapons; // Only applicable if LaunchMechanism is Weapon
        private readonly MissileFinder _missileFinder;
        private Action<MissileLauncher, LaunchRequest> _onMissileLaunchSuccess;

        public MissileLauncher(LaunchMechanism launchMechanism, Missile missile, List<IMyThrust> launchPulseThrusters, List<IMyUserControllableGun> launchWeapons, MissileFinder missileFinder)
        {
            _launchMechanism = launchMechanism;
            _missile = missile;
            _launchPulseThrusters = launchPulseThrusters;
            _launchWeapons = launchWeapons;
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
        /// Gets the launch pulse thrusters (only applicable if LaunchMechanism is PulsedThruster)
        /// </summary>
        public List<IMyThrust> LaunchPulseThrusters => _launchPulseThrusters;

        /// <summary>
        /// Gets the launch weapons (only applicable if LaunchMechanism is Weapon)
        /// </summary>
        public List<IMyUserControllableGun> LaunchWeapons => _launchWeapons;

        public bool HasSuccessfullyDetached => _missile.ReferenceGrid != _missileFinder.ReferenceGrid;

        public void Launch(Action<MissileLauncher, LaunchRequest> onMissileLaunchSuccess, LaunchRequest launchRequest)
        {
            if (_state == MissileLauncherState.Waiting)
            {
                _state = MissileLauncherState.Launching;
                _onMissileLaunchSuccess = (launcher, request) => onMissileLaunchSuccess(launcher, launchRequest);
            }
        }
        public void EarlyUpdate(int frame)
        {
            
        }
        
        public void LateUpdate(int frame)
        {
            if (_state == MissileLauncherState.Launching)
            {
                if (HasSuccessfullyDetached)
                {
                    _state = MissileLauncherState.Done;
                    OnSuccessfulDetach();
                    return;
                }
                EvaluateLaunchMechanism();
                if (_launchMechanism == LaunchMechanism.None)
                {
                    _state = MissileLauncherState.Failed;
                    Program.LogLine($"(MissileLauncher) '{_missileFinder.FriendlyName}' - Launch mechanism failed", LogLevel.Error);
                    // We don't clean up here and instead let the launcher hang on to the missile for analysis later
                }
            }
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
            // This is a special case and requires continuous attention
            foreach (var thruster in _launchPulseThrusters)
            {
                thruster.Enabled = !thruster.Enabled;
            }
            // We don't remove this flag because this condition needs to continuously fire until true
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
        private void OnSuccessfulDetach()
        {
            _missileFinder.MarkFired();
            _onMissileLaunchSuccess?.Invoke(this, new LaunchRequest());
        }
        
    }
}