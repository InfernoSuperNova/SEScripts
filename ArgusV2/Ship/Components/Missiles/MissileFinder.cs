using System;
using System.Collections.Generic;
using IngameScript.Helper;
using IngameScript.Helper.Log;
using IngameScript.SConfig.Helper;
using IngameScript.Ship.Components.Missiles.LaunchMechanisms;
using IngameScript.TruncationWrappers;
using Sandbox.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame;
using VRageMath;
using Base6Directions = IngameScript.Helper.Base6Directions;

namespace IngameScript.Ship.Components.Missiles
{
    internal enum MissileFinderState
    {
        Refreshing,      // Currently finding blocks, allowed to refresh
        Collected,       // Missile has been collected by manager, should not refresh
        Fired            // Missile has fired, need to allocate new list on next refresh
    }

    public class MissileFinder
    {
        #region config
        private AT_Vector3D _offset;
        private AT_Vector3D _extents;
        
        private PayloadType _payloadType;
        private PropulsionType _propulsionType;
        private LaunchMechanism _launchMechanism;
        
        private LaunchControl _launchControl; // Unsure if I want this to be a global setting instead
        private RefuelPriority _refuelPriority; // Ditto

        private string _launchPulseThrusterName = "";
        private string _launchWeaponName = "";
        
        #endregion
        
        private IMyTerminalBlock _finderBlock;

        
        private ControllableShip _thisShip;
        private MissileFinderState _state;
        private MissilePattern _pattern;
        
        private List<IMyCubeBlock> _blocks;
        private List<IMyThrust> _launchPulseThrusters; // Only applicable if LaunchMechanism is PulsedThruster
        private List<IMyUserControllableGun> _launchWeapons; // Only applicable if LaunchMechanism is Weapon
        private string _friendlyName;

        public MissileFinder(IMyTerminalBlock finder, ControllableShip ship)
        {
            _finderBlock = finder;
            SyncFinderName();
            _finderBlock.CustomData = SyncConfig(_finderBlock.CustomData);
            _blocks = new List<IMyCubeBlock>();
            _launchPulseThrusters = new List<IMyThrust>();
            _launchWeapons = new List<IMyUserControllableGun>();
            _thisShip = ship;
            _state = MissileFinderState.Refreshing;
            _pattern = new MissilePattern();
            
            Program.LogLine($"(MissileFinder) Initialized with friendly name '{_friendlyName}'", LogLevel.Info);
            RefreshBlocks();
        }

 

        private Vector3I FirstCorner => _finderBlock.Position + LocalTranslate(_offset);
        private Vector3I SecondCorner => _finderBlock.Position + LocalTranslate(_offset + _extents);

        public string FriendlyName => _friendlyName;

        public IMyCubeGrid ReferenceGrid => _finderBlock.CubeGrid;

        // MissileFinder gets the final say on whether it returns a launcher; this is if the missile is fully constructed and ready to launch
        public bool TryCollectMissile(out MissileLauncher launcher)
        {
            launcher = null;

            if (_blocks.Count == 0)
            {
                Program.LogLine($"(MissileFinder) '{_finderBlock.CustomName}' - No blocks found", LogLevel.Debug);
                return false;
            }
            
            // Check if blocks match the saved pattern
            if (!_pattern.MatchesPattern(_blocks))
            {
                Program.LogLine($"(MissileFinder) '{_finderBlock.CustomName}' - Pattern mismatch", LogLevel.Debug);
                return false;
            }
            
            // Verify all blocks are working
            foreach (var block in _blocks)
            {
                if (!block.IsWorking)
                {
                    Program.LogLine($"(MissileFinder) '{_finderBlock.CustomName}' - Block not working: {block.GetType().Name}", LogLevel.Debug);
                    return false;
                }
            }

            // Create the missile and launcher
            var missile = new Missile(_blocks);
            launcher = new MissileLauncher(_launchMechanism, missile, _launchPulseThrusters, _launchWeapons, this);
            _state = MissileFinderState.Collected;
            Program.LogLine($"(MissileFinder) '{_finderBlock.CustomName}' - Successfully collected {_blocks.Count} blocks", LogLevel.Debug);
            return true;
        }

        // Called by the missile manager when the missile has fired and is no longer attached to the ship
        public void MarkFired()
        {
            Program.LogLine($"(MissileFinder) '{_finderBlock.CustomName}' - Missile marked as fired", LogLevel.Debug);
            _state = MissileFinderState.Fired;
        }

        public void EarlyUpdate(int frame)
        {
            if (frame % Config.Missile.FinderRefreshFrequencyFrames != 0) return;
            
            RefreshBlocks();
        }
        public void LateUpdate(int frame)
        {
            
        }
        
        
        private void SyncFinderName()
        {
            if (!string.IsNullOrEmpty(_finderBlock.CustomName) && _finderBlock.CustomName.Contains("["))
            {
                int openBracket = _finderBlock.CustomName.IndexOf('[');
                int closeBracket = _finderBlock.CustomName.LastIndexOf(']');

                if (closeBracket > openBracket)
                {
                    var extracted = _finderBlock.CustomName.Substring(openBracket + 1, closeBracket - openBracket - 1).Trim();
                    if (!string.IsNullOrEmpty(extracted))
                    {
                        _friendlyName = extracted;
                        return;
                    }
                }
            }
            
            _friendlyName = MemorableName.Get();
            _finderBlock.CustomName = $"{Config.String.MissileFinderPrefix} [{_friendlyName}]";
        }
        private string SyncConfig(string input)
        {
            var parsed = Dwon.Parse(input);
            
            var dict = parsed as Dictionary<string, object>;
            if (dict == null)
            {
                Program.LogLine("(MissileFinder) Config malformed", LogLevel.Critical);
                throw new Exception();
            }

            var cat = ConfigCategory.From(dict, "MissileFinder");
            
            cat.Sync("Offset", ref _offset);
            cat.Sync("Extents", ref _extents);
            
            cat.SyncEnum("PayloadType", ref _payloadType);
            cat.SyncEnum("PropulsionType", ref _propulsionType);
            cat.SyncEnum("LaunchMechanism", ref _launchMechanism);
            cat.SyncEnum("LaunchControl", ref _launchControl);
            cat.SyncEnum("RefuelPriority", ref _refuelPriority);
            
            cat.Sync("LaunchPulseThrusterName", ref _launchPulseThrusterName, "Optional");
            cat.Sync("LaunchWeaponName", ref _launchWeaponName, "Optional");
            
            _offset = _offset.Floor();
            _extents = _extents.Floor();
            
            return Dwon.Serialize(parsed);
        }

        private Vector3I LocalTranslate(AT_Vector3D translation)
        {
            var orientation = _finderBlock.Orientation;

            var forward = Base6Directions.Directions[(int)orientation.Forward] * (float)translation.X;
            var up = Base6Directions.Directions[(int)orientation.Up] * (float)translation.Y;
            var left = Base6Directions.Directions[(int)orientation.Left] * (float)translation.Z;


            return (Vector3I)(forward + up + left);
        }

        private void RefreshBlocks()
        {
            bool isFirstRefresh = !_pattern.HasPattern;
            
            switch (_state)
            {
                case MissileFinderState.Refreshing:
                    _blocks.Clear();
                    break;
                case MissileFinderState.Collected:
                    return;
                case MissileFinderState.Fired:
                    Program.LogLine($"(MissileFinder) '{_finderBlock.CustomName}' - Resetting after fire", LogLevel.Debug);
                    _blocks = new List<IMyCubeBlock>();
                    _pattern.ClearPattern();
                    _state = MissileFinderState.Refreshing;
                    break;
            }

            Vector3I ca = Vector3I.Min(FirstCorner, SecondCorner);
            Vector3I cb = Vector3I.Max(FirstCorner, SecondCorner);
            
            
            foreach (var pos in Vector3I.EnumerateRange(ca, cb))
            {
                var slimBlock = _thisShip.Grid.GetCubeBlock(pos);
                if (slimBlock == null) continue;
                var fullBlock = slimBlock.FatBlock;
                _blocks.Add(fullBlock);
            }
            
            // Save pattern on first refresh (initial construction detection)
            if (isFirstRefresh && _blocks.Count > 0)
            {
                _pattern.SavePattern(_blocks);
                Program.LogLine($"(MissileFinder) '{_finderBlock.CustomName}' - Pattern saved with {_blocks.Count} blocks", LogLevel.Debug);
            }
        }
        
        

    }
}