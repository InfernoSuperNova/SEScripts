using System;
using System.Collections.Generic;
using IngameScript.Ship.Components.Missiles.GuidanceObjective;
using IngameScript.Ship.Components.Missiles.LaunchMechanisms;
using Sandbox.ModAPI.Ingame;

namespace IngameScript.Ship.Components.Missiles.RequestHandlers
{
    /// <summary>
    /// Item class.
    /// </summary>
    /// <summary>
    /// Item class.
    /// </summary>
    public class CiwsRequestHandler
    {
        /// <summary>
        /// List method.
        /// </summary>
        /// <returns>The result of the operation.</returns>
        /// <summary>
        /// List method.
        /// </summary>
        /// <returns>The result of the operation.</returns>
        private List<MissileLauncher> _launcherCandidates = new List<MissileLauncher>();
        /// <summary>
        /// List method.
        /// </summary>
        /// <returns>The result of the operation.</returns>
        /// <summary>
        /// List method.
        /// </summary>
        /// <returns>The result of the operation.</returns>
        private List<Missile> _missileCandidates = new List<Missile>();

        public void HandleRequest(
            List<MissileLauncher> launchers,
            List<Missile> missiles,
            TrackableShip target,
            Action<MissileLauncher, MissileCommand> onLaunchSuccess)
        {
            var launchRequest = CreateCiwsLaunchRequest(target);

            FilterCandidates(launchers, missiles, target);

            if (_launcherCandidates.Count == 0 && _missileCandidates.Count == 0)
                return; // No candidates available

            var closestLauncher = FindClosestLauncher(target);
            var closestMissile = FindClosestMissile(target);

            var launcherDist = closestLauncher != null
                ? (closestLauncher.Missile.Position - target.Position).LengthSquared()
                : float.MaxValue;
            var missileDist = closestMissile != null
                ? (closestMissile.Position - target.Position).LengthSquared()
                : float.MaxValue;

            if (launcherDist < missileDist && closestLauncher != null)
            {
                closestLauncher.Launch(onLaunchSuccess, launchRequest);
            }
            else
            {
                closestMissile?.Command(launchRequest);
            }
        }

        private void FilterCandidates(List<MissileLauncher> launchers,
            List<Missile> missiles, TrackableShip target)
        {
            _launcherCandidates.Clear();
            _missileCandidates.Clear();

            // Filter launchers capable of CIWS
            foreach (var launcher in launchers)
            {
                if (launcher.MissileCommandContext.HasFlag(MissileCommandContext.Ciws))
                    _launcherCandidates.Add(launcher);
            }

            // Filter missiles capable of CIWS with ballistic advantage
            foreach (var missile in missiles)
            {
                if (missile.LaunchCapability.HasFlag(MissileCommandContext.Ciws) // If the missile is capable of CIWS...
                    && !missile.LaunchContext.HasFlag(MissileCommandContext.Ciws) // Not currently in flight as CIWS...
                    && (missile.Position - target.Position).Dot(target.Velocity) > 0) // And has ballistic advantage
                    _missileCandidates.Add(missile); // Then it's a candidate for redirection
            }
        }

        private MissileLauncher FindClosestLauncher(TrackableShip target)
        {
            if (_launcherCandidates.Count == 0)
                return null;

            var closestLauncher = _launcherCandidates[0];
            var closestDistance = (closestLauncher.Missile.Position - target.Position).LengthSquared();

            foreach (var launcher in _launcherCandidates)
            {
                var distance = (launcher.Missile.Position - target.Position).LengthSquared();
                if (distance < closestDistance)
                {
                    closestLauncher = launcher;
                    closestDistance = distance;
                }
            }

            return closestLauncher;
        }

        private Missile FindClosestMissile(TrackableShip target)
        {
            if (_missileCandidates.Count == 0)
                return null;

            var closestMissile = _missileCandidates[0];
            var closestDistance = (closestMissile.Position - target.Position).LengthSquared();

            foreach (var missile in _missileCandidates)
            {
                var distance = (missile.Position - target.Position).LengthSquared();
                if (distance < closestDistance)
                {
                    closestMissile = missile;
                    closestDistance = distance;
                }
            }

            return closestMissile;
        }

        private MissileCommand CreateCiwsLaunchRequest(TrackableShip target)
        {
            
            var missionObjective = new DirectAttackBehavior(target);
            
            return new MissileCommand(
                MissileCommandContext.Ciws,
                missionObjective
            );
        }
    }


}
