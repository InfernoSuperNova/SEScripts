using System;
using System.Collections;
using System.Collections.Generic;
using IngameScript.Ship.Components.Missiles.GuidanceObjective;
using IngameScript.Ship.Components.Missiles.LaunchMechanisms;
using IngameScript.Ship.Components.Missiles.RequestHandlers;
using IngameScript.TruncationWrappers;
using Sandbox.ModAPI.Ingame;

namespace IngameScript.Ship.Components.Missiles
{
    /// <summary>
    /// Item class.
    /// </summary>
    /// <summary>
    /// Item class.
    /// </summary>
    public class MissileManager
    {
        private Queue<MissileFinder> _unprocessedFinders;
        private List<MissileFinder> _finders;
        private List<MissileLauncher> _launchers;
        private List<MissileLauncher> _toRemoveLaunchers = new List<MissileLauncher>();
        private List<Missile> _missiles;

        private CiwsRequestHandler _ciwsHandler;


        private bool _allSetUp = false;

        private List<MissileLauncher> _reusedMissileCandidateList = new List<MissileLauncher>();
        /// <summary>
        /// List method.
        /// </summary>
        /// <returns>The result of the operation.</returns>
        /// <summary>
        /// List method.
        /// </summary>
        /// <returns>The result of the operation.</returns>
        private List<Missile> _reusedLauncherCandidateList = new List<Missile>();
        /// <summary>
        /// MissileManager method.
        /// </summary>
        /// <param name="blocks">The blocks parameter.</param>
        /// <param name="ship">The ship parameter.</param>
        /// <returns>The result of the operation.</returns>
        /// <summary>
        /// MissileManager method.
        /// </summary>
        /// <param name="blocks">The blocks parameter.</param>
        /// <param name="ship">The ship parameter.</param>
        /// <returns>The result of the operation.</returns>
        public MissileManager(List<IMyTerminalBlock> blocks, ControllableShip ship)
        {
            _unprocessedFinders = new Queue<MissileFinder>();
            _finders = new List<MissileFinder>();
            _launchers = new List<MissileLauncher>();
            _missiles = new List<Missile>();
            _ciwsHandler = new CiwsRequestHandler();
            foreach (var block in blocks)
            {
                if (block.CustomName.StartsWith(Config.String.MissileFinderPrefix)) _unprocessedFinders.Enqueue(new MissileFinder(block, ship));
            }
        }

        public void EarlyUpdate(int frame)
        {
            if (_unprocessedFinders.Count > 0) // Should be easy enough to add new finders mid runtime
            {
                var finder = _unprocessedFinders.Dequeue();
                finder.Setup();
                _finders.Add(finder);
                //return;
            }
            
            try { foreach (var finder in _finders) finder.EarlyUpdate(frame); }
            catch (Exception ex) { throw new Exception("Failed in _finders.EarlyUpdate", ex); }

            try { Collect(); } 
            catch (Exception ex) { throw new Exception("Failed in Collect()", ex); }

            try { foreach (var launcher in _launchers) launcher.EarlyUpdate(frame); } 
            catch (Exception ex) { throw new Exception("Failed in _launchers.EarlyUpdate", ex); }

            try { foreach (var missile in _missiles) missile.EarlyUpdate(frame); } 
            catch (Exception ex) { throw new Exception("Failed in _missiles.EarlyUpdate", ex); }
            
            
            foreach (var launcher in _toRemoveLaunchers)
            {
                _launchers.Remove(launcher);
            }
            _toRemoveLaunchers.Clear();
        }

        public void LateUpdate(int frame)
        {
            foreach (var finder in _finders) finder.LateUpdate(frame);
            foreach (var launcher in _launchers) launcher.LateUpdate(frame);
            foreach (var missile in _missiles) missile.LateUpdate(frame);
        }


        public void RequestCiws(TrackableShip target)
        {
            _ciwsHandler.HandleRequest(_launchers, _missiles, target, OnMissileLaunchSuccess);
        }

        public void RequestMissileFireManual()
        {
            var launchRequest = new MissileCommand(
                MissileCommandContext.Manual,
                new TempBehavior()
            );

            foreach (var launcher in _launchers)
            {
                if (launcher.State == MissileLauncherState.Waiting)
                {
                    launcher.Launch(OnMissileLaunchSuccess, launchRequest);
                    return;
                }
                
            }
        }
        
        public void RefreshMissilePatterns()
        {
            foreach (var finder in _finders)
            {
                finder.RefreshPattern();
            }
        }

        private void OnMissileLaunchSuccess(MissileLauncher launcher, MissileCommand missileCommand)
        {
            var missile = launcher.Missile;
            _toRemoveLaunchers.Add(launcher);
            _missiles.Add(missile);
            missile.Command(missileCommand);
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

    public class TempBehavior : IMissionBehavior
    {
        public GuidanceCommand Evaluate()
        {
            return new GuidanceCommand();
        }

        public bool IsComplete()
        {
            return true;
        }

        public void OnComplete()
        {
            
        }

        public void BindToMissile(Missile missile)
        {
            
        }
    }
}