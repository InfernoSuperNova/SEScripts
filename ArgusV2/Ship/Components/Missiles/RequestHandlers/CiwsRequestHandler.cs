using System;
using System.Collections.Generic;
using IngameScript.Ship.Components.Missiles.LaunchMechanisms;
using Sandbox.ModAPI.Ingame;

namespace IngameScript.Ship.Components.Missiles.RequestHandlers
{
    public class CiwsRequestHandler
    {
        private List<MissileLauncher> _launcherCandidates = new List<MissileLauncher>();
        private List<Missile> _missileCandidates = new List<Missile>();

        public void HandleRequest(
            List<MissileLauncher> launchers,
            List<Missile> missiles,
            TrackableShip target,
            Action<MissileLauncher, LaunchRequest> onLaunchSuccess)
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
                closestMissile?.Redirect(target, launchRequest);
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
                if (launcher.LaunchControl.HasFlag(LaunchControl.Ciws))
                    _launcherCandidates.Add(launcher);
            }

            // Filter missiles capable of CIWS with ballistic advantage
            foreach (var missile in missiles)
            {
                if (missile.LaunchCapability.HasFlag(LaunchControl.Ciws) // If the missile is capable of CIWS...
                    && !missile.LaunchContext.HasFlag(LaunchControl.Ciws) // Not currently in flight as CIWS...
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

        private LaunchRequest CreateCiwsLaunchRequest(TrackableShip target)
        {
            return new LaunchRequest(
                launchControl: LaunchControl.Ciws,
                target: target,
                targetingMode: TargetingMode.AABBCenter,
                behavior: Behavior.DirectAttack
            );
        }
    }
}
