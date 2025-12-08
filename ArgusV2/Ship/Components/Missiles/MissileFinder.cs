using System;
using System.Collections.Generic;
using IngameScript.Helper;
using IngameScript.SConfig.Helper;
using IngameScript.TruncationWrappers;
using Sandbox.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame;
using VRageMath;
using Base6Directions = IngameScript.Helper.Base6Directions;

namespace IngameScript.Ship.Components.Missiles
{
    internal class MissileFinder
    {
        private IMyTerminalBlock _finderBlock;
        private AT_Vector3D _offset;
        private AT_Vector3D _extents;
        private List<IMyCubeBlock> _blocks;
        private ControllableShip _thisShip;

        public MissileFinder(IMyTerminalBlock finder, ControllableShip ship)
        {
            _finderBlock = finder;
            _finderBlock.CustomData = SyncConfig(_finderBlock.CustomData);
            _blocks = new List<IMyCubeBlock>();
            _thisShip = ship;
            RefreshBlocks();
        }

        private Vector3I FirstCorner => _finderBlock.Position + LocalTranslate(_offset);
        private Vector3I SecondCorner => _finderBlock.Position + LocalTranslate(_offset + _extents);
        
        public string SyncConfig(string input)
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
            _blocks.Clear();

            Vector3I ca = Vector3I.Min(FirstCorner, SecondCorner);
            Vector3I cb = Vector3I.Max(FirstCorner, SecondCorner);
            
            
            foreach (var pos in Vector3I.EnumerateRange(ca, cb))
            {
                var slimBlock = _thisShip.Grid.GetCubeBlock(pos);
                if (slimBlock == null) continue;
                var fullBlock = slimBlock.FatBlock;
                _blocks.Add(fullBlock);
            }
        }
        
    }
}