using System.Collections;
using System.Collections.Generic;
using IngameScript.Ship.Components.Missiles.LaunchMechanisms;
using Sandbox.ModAPI.Ingame;

namespace IngameScript.Ship.Components.Missiles
{
    public class MissileManager
    {

        private List<MissileFinder> _finders;
        private List<MissileLauncher> _launchers;
        private List<Missile> _missiles;

        
        
        private List<MissileLauncher> _reusedMissileCandidateList = new List<MissileLauncher>();
        private List<Missile> _reusedLauncherCandidateList = new List<Missile>();
        public MissileManager(List<IMyTerminalBlock> blocks, ControllableShip ship)
        {
            _finders = new List<MissileFinder>();
            _launchers = new List<MissileLauncher>();
            _missiles = new List<Missile>();
            foreach (var block in blocks)
            {
                if (block.CustomName.StartsWith(Config.String.MissileFinderPrefix)) _finders.Add(new MissileFinder(block, ship));
            }
        }

        public void EarlyUpdate(int frame)
        {
            foreach (var finder in _finders) finder.EarlyUpdate(frame);
            foreach (var launcher in _launchers) launcher.EarlyUpdate(frame);
            foreach (var missile in _missiles) missile.EarlyUpdate(frame);
        }

        public void LateUpdate(int frame)
        {
            foreach (var finder in _finders) finder.LateUpdate(frame);
            foreach (var launcher in _launchers) launcher.LateUpdate(frame);
            foreach (var missile in _missiles) missile.LateUpdate(frame);
        }


        public void RequestCiws(TrackableShip target)
        {
            var _launcherCandidates = _reusedMissileCandidateList;
            _launcherCandidates.Clear(); 
            var _missileCandidates = _reusedLauncherCandidateList;
            _missileCandidates.Clear();
            
            
            var launchRequest = new LaunchRequest(
                launchControl: LaunchControl.Ciws,
                target: target,
                targetingMode: TargetingMode.AABBCenter,
                behavior: Behavior.DirectAttack
            );

            foreach (var launcher in _launchers)
            {
                if (launcher.LaunchControl.HasFlag(LaunchControl.Ciws)) 
                    _launcherCandidates.Add(launcher);
            }
            
            foreach (var missile in _missiles)
            {
                if (missile.LaunchCapability.HasFlag(LaunchControl.Ciws) // If the missile is capable of CIWS...
                    && !missile.LaunchContext.HasFlag(LaunchControl.Ciws) // Not currently in flight as CIWS...
                    && (missile.Position - target.Position).Dot(target.Velocity) > 0) // And has ballistic advantage
                    _missileCandidates.Add(missile); // Then it's a candidate for redirection
            }
            
            var closestLauncher = _launcherCandidates[0];
            var closestMissile = _missileCandidates[0];
            
            var launcherDistance = (closestLauncher.Missile.Position - target.Position).LengthSquared();
            var missileDistance = (closestMissile.Position - target.Position).LengthSquared();

            foreach (var launcher in _launcherCandidates)
            {
                var distance = (launcher.Missile.Position - target.Position).LengthSquared();
                if (distance < launcherDistance)
                {
                    closestLauncher = launcher;
                    launcherDistance = distance;
                }
            }
            foreach (var missile in _missileCandidates)
            {
                var distance = (missile.Position - target.Position).LengthSquared();
                if (distance < missileDistance)
                {
                    closestMissile = missile;
                    missileDistance = distance;
                }
            }
            
            if (launcherDistance < missileDistance)
            {
                closestLauncher.Launch(OnMissileLaunchSuccess, launchRequest);
            }
            else
            {
                closestMissile.Redirect(target, launchRequest);
            }
        }

        public void RequestMissileFireManual()
        {
            var launchRequest = new LaunchRequest(
                launchControl: LaunchControl.Manual,
                target: null,
                targetingMode: TargetingMode.AABBCenter,
                behavior: Behavior.DirectAttack
            );

            foreach (var launcher in _launchers)
            {
                launcher.Launch(OnMissileLaunchSuccess, launchRequest);
            }
        }

        private void OnMissileLaunchSuccess(MissileLauncher launcher, LaunchRequest launchRequest)
        {
            var missile = launcher.Missile;
            _launchers.Remove(launcher);
            _missiles.Add(missile);
            missile.SetLaunchParameters(launchRequest);
        }

        private void Collect()
        {
            foreach (var finder in _finders)
            {
                MissileLauncher launcher;
                if (finder.TryCollectMissile(out launcher))
                {
                    _launchers.Add(launcher);
                }
            }
        }
    }
}