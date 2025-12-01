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
        private float _previousAcceleration;
        private int _framesOff;

        public DirectionalDrive(List<GravityGenerator> generators, Direction direction)
        {
            _generators = generators;
            TotalAcceleration = 0;
            Direction = direction;
            foreach (var generator in generators) // TODO: This might need an update when generators are added
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
            if (_previousAcceleration != _acceleration)
                foreach (var generator in _generators)
                    generator.Acceleration = (float)(_acceleration * Config.GravityAcceleration);
            _previousAcceleration = _acceleration;
        }
        
        public Direction Direction { get; private set; }
        public bool Enabled { get; private set; }
        public double TotalAcceleration { get; private set; } // Probably not spherical inclusive for now

        public void SetAcceleration(float acceleration)
        {
            acceleration = (float)MathHelperD.Clamp(ArgusMath.SnapToMultiple(acceleration, Config.GdriveStep), -1, 1);
            if (acceleration == _acceleration && acceleration == 0) return; // If idle
            Enabled = true;
            if (acceleration == _acceleration) return; // If hasn't changed
            _acceleration = acceleration;
        }
    }
}