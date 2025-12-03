using System.Collections.Generic;
using IngameScript.Helper;
using IngameScript.Ship.Components.Propulsion.Gravity.Wrapper;
using IngameScript.TruncationWrappers;
using VRageMath;
using Base6Directions = IngameScript.Helper.Base6Directions;

namespace IngameScript.Ship.Components.Propulsion.Gravity
{
    internal class DirectionalDrive
    {
        private BalancedMassSystem _massSystem;
        
        private readonly List<GravityGenerator> _generators;
        private bool _previousEnabled;
        private float _acceleration;
        private float _previousAcceleration; // Stored to detect when the state of the drive hasn't changed between frames
        private int _framesOff;
        
        private CachedValue<double> _linearForce;
        private CachedValue<double> _sphericalForce;
        

        public DirectionalDrive(List<GravityGenerator> generators, Direction direction, BalancedMassSystem massSystem)
        {
            _generators = generators;
            Direction = direction;
            
            _linearForce = new CachedValue<double>(() => CalculateLinearForce(massSystem.TotalMass));
            _sphericalForce = new CachedValue<double>(() => CalculateSphericalForce(massSystem.AllBlocks));

            _massSystem = massSystem;
        }
        
        public Direction Direction { get; private set; }
        public bool Enabled { get; private set; }
        public double MaxLinearForce => _linearForce.Value;
        public double MaxSphericalForce => _sphericalForce.Value;
        public double MaxForce => MaxLinearForce + MaxSphericalForce;
        
        public void EarlyUpdate(int frame)
        {
            if (_acceleration == 0) _framesOff++;
            else _framesOff = 0;
            if (_framesOff > Config.Gdrive.TimeoutFrames) Enabled = false;
            _generators.RemoveAll(g => g.Closed);

            if (frame % Config.Gdrive.AccelerationRecalcDelay == 0 && _massSystem.HasStateChanged())
            {
                _linearForce.Invalidate();
                _sphericalForce.Invalidate();
            }
        }
        
        public void LateUpdate(int frame)
        {
            if (_previousEnabled != Enabled) 
                foreach (var generator in _generators) generator.Enabled = Enabled;
            _previousEnabled = Enabled;
            if (_previousAcceleration != _acceleration)
                foreach (var generator in _generators)
                    generator.Acceleration = (float)(_acceleration * Config.Gdrive.Acceleration);
            _previousAcceleration = _acceleration;
        }

        public void SetAcceleration(float acceleration)
        {
            acceleration = (float)MathHelperD.Clamp(ArgusMath.SnapToMultiple(acceleration, Config.Gdrive.Step), -1, 1);
            if (acceleration == _acceleration && acceleration == 0) return; // If idle
            Enabled = true;
            if (acceleration == _acceleration) return; // If hasn't changed
            _acceleration = acceleration;
        }


        private double CalculateSphericalForce(List<Mass> masses)
        {
            var netForce = AT_Vector3D.Zero;
            foreach (var generator in _generators)
            {
                if (generator is GravityGeneratorSpherical)
                {
                    foreach (var mass in masses)
                    {
                        var dir = ((AT_Vector3D)(mass.GridPosition - generator.GridPosition)).Normalized();
                        var force = dir * mass.BalancerVirtualMass * Config.Gdrive.Acceleration *
                                    generator.InvertedSign;
                        netForce += force;
                    }
                }
            }
            
            return netForce.Dot(Base6Directions.Directions[(int)Direction]);
        }
        private double CalculateLinearForce(double mass)
        {
            double acceleration = 0;
            foreach (var generator in _generators)
            {
                var linear = generator as GravityGeneratorLinear;
                if (linear != null) acceleration += Config.Gdrive.Acceleration;
            }

            return acceleration * mass;
        }
    }
}