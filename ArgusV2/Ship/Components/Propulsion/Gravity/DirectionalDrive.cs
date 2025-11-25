using System.Collections.Generic;
using IngameScript.Helper;
using IngameScript.Ship.Components.Propulsion.Gravity.Wrapper;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRageMath;

namespace IngameScript.Ship.Components.Propulsion.Gravity
{
    internal class DirectionalDrive
    {
        private List<GravityGenerator> _generators;
        private bool _previousEnabled;
        private float _acceleration;
        private int _framesOff;

        public DirectionalDrive(List<GravityGenerator> generators, Direction direction)
        {
            _generators = generators;
            TotalAcceleration = 0;
            Direction = direction;
            foreach (var generator in generators)
            {
                var linear = generator as GravityGeneratorLinear;

                if (linear != null) TotalAcceleration += Config.GravityAcceleration; // No better way to get acceleration?
            }
        }

        public void EarlyUpdate(int frame)
        {
            if (_acceleration == 0) _framesOff++;
            else _framesOff = 0;

            if (_framesOff > Config.GdriveTimeoutFrames) Enabled = false;
        }

        public void LateUpdate(int frame)
        {
            if (_previousEnabled != Enabled) 
                foreach (var generator in _generators) generator.Enabled = Enabled;
            _previousEnabled = Enabled;
        }
        
        public Direction Direction { get; private set; }
        public bool Enabled { get; set; }
        public double TotalAcceleration { get; private set; } // Probably not spherical inclusive for now

        public void SetAcceleration(float acceleration)
        {
            Enabled = true;
            if (acceleration == _acceleration) return; // Could possibly round this to maybe 2 dp to reduce polling rate
            _acceleration = acceleration;
            foreach (var generator in _generators)
            {
                generator.Acceleration = acceleration * (float)Config.GravityAcceleration;
            }
        }
    }
}