using System.Collections;
using System.Collections.Generic;
using IngameScript.Ship.Components.Missiles.LaunchMechanisms;
using Sandbox.ModAPI.Ingame;

namespace IngameScript.Ship.Components.Missiles
{
    public class MissileManager
    {

        private List<MissileFinder> _missileFinders;
        private List<MissileLauncher> _launchers;


        public MissileManager(List<IMyTerminalBlock> blocks, ControllableShip ship)
        {
            
            _missileFinders = new List<MissileFinder>();
            foreach (var block in blocks)
            {
                if (block.CustomName.StartsWith(Config.String.MissileFinderPrefix)) _missileFinders.Add(new MissileFinder(block, ship));
            }
        }

        public void EarlyUpdate(int frame)
        {
            foreach (var finder in _missileFinders) finder.EarlyUpdate(frame);
            foreach (var launcher in _launchers) launcher.EarlyUpdate(frame);
        }

        public void LateUpdate(int frame)
        {
            foreach (var finder in _missileFinders) finder.LateUpdate(frame);
            foreach (var launcher in _launchers) launcher.LateUpdate(frame);
        }

        public void RequestCiws(TrackableShip target)
        {
            var launchRequest = new LaunchRequest(
                launchControl: LaunchControl.ManualCiwsOverride,
                target: target,
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
            // TODO: Add missile to active tracking list and configure it with launch parameters
        }

        private void Collect()
        {
            foreach (var finder in _missileFinders)
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