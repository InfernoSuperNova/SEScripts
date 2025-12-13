using System.Collections;
using System.Collections.Generic;
using IngameScript.Ship.Components.Missiles.LaunchMechanisms;
using IngameScript.Ship.Components.Missiles.RequestHandlers;
using Sandbox.ModAPI.Ingame;

namespace IngameScript.Ship.Components.Missiles
{
    public class MissileManager
    {

        private List<MissileFinder> _finders;
        private List<MissileLauncher> _launchers;
        private List<Missile> _missiles;

        private CiwsRequestHandler _ciwsHandler;

        private List<MissileLauncher> _reusedMissileCandidateList = new List<MissileLauncher>();
        private List<Missile> _reusedLauncherCandidateList = new List<Missile>();
        public MissileManager(List<IMyTerminalBlock> blocks, ControllableShip ship)
        {
            _finders = new List<MissileFinder>();
            _launchers = new List<MissileLauncher>();
            _missiles = new List<Missile>();
            _ciwsHandler = new CiwsRequestHandler();
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
            _ciwsHandler.HandleCiwsRequest(_launchers, _missiles, target, OnMissileLaunchSuccess);
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