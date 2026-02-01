using System;
using System.Collections.Generic;
using IngameScript.Helper;
using IngameScript.Helper.Log;
using IngameScript.Ship.Components.Propulsion.Gravity.Wrapper;
using IngameScript.TruncationWrappers;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRageMath;
using Base6Directions = IngameScript.Helper.Base6Directions;


namespace IngameScript.Ship.Components.Propulsion.Gravity
{
    /// <summary>
    /// Item class.
    /// </summary>
    /// <summary>
    /// Item class.
    /// </summary>
    public class GDrive
    {
        private readonly DirectionalDrive _forwardBackward;
        private readonly DirectionalDrive _leftRight;
        private readonly DirectionalDrive _upDown;

        private BalancedMassSystem _massSystem;
        private ControllableShip _ship;


        private bool _previousMassEnabled;
        

        public GDrive(List<IMyTerminalBlock> blocks, ControllableShip ship)
        {
            Program.LogLine($"Setting up gravity drive", LogLevel.Info);
            _ship = ship;
            
            var forwardBackward = new List<GravityGenerator>();
            var leftRight = new List<GravityGenerator>();
            var upDown = new List<GravityGenerator>();

            var genCastArray = new Dictionary<Direction, List<GravityGenerator>>
            {
                { Direction.Up, upDown },
                { Direction.Down, upDown },
                { Direction.Left, leftRight },
                { Direction.Right, leftRight },
                { Direction.Forward, forwardBackward },
                { Direction.Backward, forwardBackward }
            };
            
            foreach (var block in blocks)
            {
                var linearGen = block as IMyGravityGenerator;
                if (linearGen != null)
                {
                    var dir = (Direction)linearGen.Orientation.Up;
                    var list = genCastArray[dir];
                    bool inverted = (int)dir % 2 == 0;
                    list.Add(new GravityGeneratorLinear(linearGen, dir, inverted));
                }

                var sphericalGen = block as IMyGravityGeneratorSphere;
                if (sphericalGen != null)
                {
                    var forward = _ship.LocalDirectionForward;
                    var forwardDir = Base6Directions.Directions[(int)forward];
                    var inverted = forwardDir.Dot((Vector3D)_ship.LocalCenterOfMass - sphericalGen.Position * _ship.GridSize) > 0;
                    var list = genCastArray[forward];
                    list.Add(new GravityGeneratorSpherical(sphericalGen, forward, inverted));
                    
                }
            }

            if (forwardBackward.Count == 0) Program.LogLine($"No Forward/backward gravity generators", LogLevel.Warning);
            if (leftRight.Count == 0) Program.LogLine($"No Left/Right gravity generators", LogLevel.Warning);
            if (upDown.Count == 0) Program.LogLine($"No Up/Down gravity generators", LogLevel.Warning);
            
            
            
            _massSystem = new BalancedMassSystem(blocks, ship);
            
            _forwardBackward = new DirectionalDrive(forwardBackward, Direction.Forward, _massSystem);
            _leftRight = new DirectionalDrive(leftRight, Direction.Left, _massSystem);
            _upDown = new DirectionalDrive(upDown, Direction.Up, _massSystem);
        }

        private bool MassEnabled => _forwardBackward.Enabled || _leftRight.Enabled || _upDown.Enabled;

        public double TotalMass => _massSystem.TotalMass;

        public void EarlyUpdate(int frame)
        {
            _forwardBackward.EarlyUpdate(frame);
            _leftRight.EarlyUpdate(frame);
            _upDown.EarlyUpdate(frame);

            _massSystem.EarlyUpdate(frame);
        }

        public void LateUpdate(int frame)
        {
            _forwardBackward.LateUpdate(frame);
            _leftRight.LateUpdate(frame);
            _upDown.LateUpdate(frame);


            if (MassEnabled != _previousMassEnabled) _massSystem.Enabled = MassEnabled;
            Program.Log(_massSystem.Enabled);
            _previousMassEnabled = MassEnabled;
            _massSystem.LateUpdate(frame);
        }


        public void ApplyPropulsion(AT_Vector3D propLocal)
        {
            _forwardBackward.SetAcceleration((float)propLocal.Dot(AT_Vector3D.Forward));
            _leftRight.SetAcceleration((float)propLocal.Dot(AT_Vector3D.Left));
            _upDown.SetAcceleration((float)propLocal.Dot(AT_Vector3D.Up));
        }

        public double GetForwardBackwardForce()
        {
            var force = _forwardBackward.MaxForce;
            if (force == 0) force = 1;
            return force;
        }

        public double GetLeftRightForce()
        {
            var force = _leftRight.MaxForce;
            if (force == 0) force = 1;
            return force;
        }

        public double GetUpDownForce()
        {
            var force = _upDown.MaxForce;
            if (force == 0) force = 1;
            return force;
        }
    }
}