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

    internal enum SetupState
    {
        AwaitingSetup,
        SetupComplete,
        SetupFailed
    }

    public class MissileFinder
    {
        #region config
        private AT_Vector3D _localCornerA;
        private AT_Vector3D _localCornerB;
        
        private PayloadType _payloadType;
        private PropulsionType _propulsionType;
        private LaunchMechanism _launchMechanism;
        
        private MissileCommandContext _missileCommandContext = MissileCommandContext.Manual | MissileCommandContext.Ciws; // Unsure if I want this to be a global setting instead
        private RefuelPriority _refuelPriority; // Ditto
        
        private AT_Vector3D _launchPulseThrusterLocation;
        
        #endregion
        
        private IMyTerminalBlock _finderBlock;

        
        private ControllableShip _thisShip;
        private MissileFinderState _state;
        private MissilePattern _pattern;
        
        private List<IMyCubeBlock> _blocks;
        private IMyThrust _launchPulseThruster; // Only applicable if LaunchMechanism is PulsedThruster
        private List<IMyUserControllableGun> _launchWeapons; // Only applicable if LaunchMechanism is Weapon
        private FriendlyCustomName _friendlyCustomName;
        
        
        private SetupState _setupState = SetupState.AwaitingSetup;


        public MissileFinder(IMyTerminalBlock finder, ControllableShip ship)
        {
            _finderBlock = finder;
            _thisShip = ship;
        }

        public void Setup()
        {
            _setupState = SetupState.SetupComplete;
            _friendlyCustomName = new FriendlyCustomName(_finderBlock, Config.String.MissileFinderPrefix);
            _finderBlock.CustomData = SyncConfig(_finderBlock.CustomData);
            if (_launchMechanism == 0)
            {
                SetupError("No launch mechanism specified");
                return;
            }
            _blocks = new List<IMyCubeBlock>();
            _launchWeapons = new List<IMyUserControllableGun>();
            
            if (_launchMechanism.HasFlag(LaunchMechanism.PulsedThruster))
            {
                var slimBlock = _finderBlock.CubeGrid.GetCubeBlock(_finderBlock.Position + LocalTranslate(_launchPulseThrusterLocation));
                if (slimBlock != null)
                {
                    var fatBLock = slimBlock.FatBlock;
                    var launchPulseThruster = fatBLock as IMyThrust;
                    if (launchPulseThruster != null)
                    {
                        _launchPulseThruster = launchPulseThruster;
                    }
                    else
                    {
                        SetupError($"Block at pulse thruster position is not a thruster");
                        return;
                    }
                }
                else
                {
                    SetupError($"Block at pulse thruster position does not exist");
                    return;
                }
                
            }
            _state = MissileFinderState.Refreshing;
            _pattern = new MissilePattern();
            
            Program.LogLine($"MissileFinder Initialized with friendly name '{_friendlyCustomName.FriendlyName}'", LogLevel.Info);
            RefreshBlocks();
        }

        private void SetupError(string reason)
        {
            Program.LogLine($"MissileFinder setup failed for '{_friendlyCustomName.FriendlyName}': {reason}", LogLevel.Error);
            _setupState = SetupState.SetupFailed;
        }

 

        private Vector3I FirstCorner => _finderBlock.Position + LocalTranslate(_localCornerA);

        private Vector3I SecondCorner => _finderBlock.Position + LocalTranslate(_localCornerB);

        public string FriendlyName => _friendlyCustomName.FriendlyName;

        public IMyCubeGrid ReferenceGrid => _finderBlock.CubeGrid;

        // MissileFinder gets the final say on whether it returns a launcher; this is if the missile is fully constructed and ready to launch
        public bool TryCollectMissile(out MissileLauncher launcher)
        {
            
            launcher = null;
            
            if (_setupState != SetupState.SetupComplete) return false;
            
            if (_state == MissileFinderState.Collected) return false;
            
            if (_blocks.Count == 0) return false;
            
            if (!_pattern.IsValid) return false;
            
            if (!_pattern.MatchesPattern(_blocks)) return false;
            
            // Verify all blocks are working
            foreach (var block in _blocks) if (!block.IsWorking) return false;
            

            // Create the missile and launcher
            
            if (_launchMechanism.HasFlag(LaunchMechanism.PulsedThruster) && _launchPulseThruster == null)
            {
                Program.LogLine($"ERROR: MissileFinder '{_friendlyCustomName.FriendlyName}' specifies pulse thruster launch type but no pulse thruster found", LogLevel.Error);
                return false;
 
            }
            
            var missile = new Missile(_blocks, _missileCommandContext);
            launcher = new MissileLauncher(_launchMechanism, missile, _launchPulseThruster, _launchWeapons, _missileCommandContext, this);
            _state = MissileFinderState.Collected;
            Program.LogLine($"(MissileFinder) '{_finderBlock.CustomName}' - Successfully collected {_blocks.Count} blocks", LogLevel.Debug);
            return true;
        }

        #region API
        public void MarkFired()
        {
            Program.LogLine($"(MissileFinder) '{_finderBlock.CustomName}' - Missile marked as fired", LogLevel.Debug);
            _state = MissileFinderState.Fired;
        }

        public void EarlyUpdate(int frame)
        {
            
            // AT_Vector3D ca = (Vector3D.Min(FirstCorner, SecondCorner) - 0.5) * _finderBlock.CubeGrid.GridSize;
            // AT_Vector3D cb = (Vector3D.Max(FirstCorner, SecondCorner) + 0.5) * _finderBlock.CubeGrid.GridSize;
            //
            // var aabb = new BoundingBoxD(ca, cb);
            // var obb = new MyOrientedBoundingBoxD(aabb, _finderBlock.CubeGrid.WorldMatrix);
            // Program.Debug.DrawOBB(obb, Color.Red, DebugAPI.Style.Wireframe, 0.02f, 0.016f, true);
            //
            //
            // var startOffset = _finderBlock.Position + LocalTranslate(_launchPulseThrusterLocation);
            //
            //
            // var ca2 = ((Vector3D)startOffset - 0.5) * _finderBlock.CubeGrid.GridSize;
            // var cb2 = ((Vector3D)startOffset + 0.5) * _finderBlock.CubeGrid.GridSize;
            //
            // var aabb2 = new BoundingBoxD(ca2, cb2);
            // var obb2 = new MyOrientedBoundingBoxD(aabb2, _finderBlock.CubeGrid.WorldMatrix);
            // Program.Debug.DrawOBB(obb2, Color.Blue, DebugAPI.Style.Wireframe, 0.02f, 0.016f, true);
            
            
            
            if (_setupState != SetupState.SetupComplete) return;
            
            if (frame % Config.Missile.FinderRefreshFrequencyFrames != 0) return;
            RefreshBlocks();
        }
        public void LateUpdate(int frame)
        {
            
        }
        
        #endregion
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
            
            cat.Sync("CornerA", ref _localCornerA, "X/Y/Z, Forward/Up/Left. Negative values move in opposite direction");
            cat.Sync("CornerB", ref _localCornerB, "Use these corners to create a bounding box around the missile. Orientation is block relative." );
            
            
            
            cat.SyncEnum("PayloadType", ref _payloadType);
            cat.SyncEnum("PropulsionType", ref _propulsionType);
            cat.SyncEnum("LaunchMechanism", ref _launchMechanism);
            cat.SyncEnum("LaunchControl", ref _missileCommandContext, inlineComment: "Recommended to keep CIWS enabled");
            cat.SyncEnum("RefuelPriority", ref _refuelPriority);
            
            cat.Sync("LaunchPulseThrusterLocation", ref _launchPulseThrusterLocation, "Optional");
            _localCornerA = _localCornerA.Floor();
            _localCornerB = _localCornerB.Floor();
            
            return Dwon.Serialize(parsed);
        }

        private Vector3I LocalTranslate(AT_Vector3D translation)
        {
            var orientation = _finderBlock.Orientation;
            
            var forward = Base6Directions.Directions[(int)orientation.Forward] * (float)translation.X;
            var up = Base6Directions.Directions[(int)orientation.Up] * (float)translation.Y;
            var left = Base6Directions.Directions[(int)orientation.Left] * (float)translation.Z;


            return Vector3I.Round(forward + up + left);
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
                    //Program.LogLine($"(MissileFinder) '{_finderBlock.CustomName}' - Resetting after fire", LogLevel.Debug);
                    _blocks = new List<IMyCubeBlock>();
                    _pattern.ClearPattern();
                    _state = MissileFinderState.Refreshing;
                    break;
            }

            Vector3I ca = Vector3I.Min(FirstCorner, SecondCorner);
            Vector3I cb = Vector3I.Max(FirstCorner, SecondCorner) + Vector3I.One;
            
            foreach (var pos in Vector3I.EnumerateRange(ca, cb))
            {
                var slimBlock = _finderBlock.CubeGrid.GetCubeBlock(pos);
                if (slimBlock == null) continue;
                var fullBlock = slimBlock.FatBlock;
                _blocks.Add(fullBlock);
            }
            
            // Save pattern on first refresh (initial construction detection)
            if (isFirstRefresh && _blocks.Count > 0)
            {
                _pattern.SavePattern(_blocks);
            }
        }

        public void RefreshPattern()
        {
            _pattern.ClearPattern();
        }

    }
}