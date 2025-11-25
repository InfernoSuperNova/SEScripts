using System.Collections.Generic;
using IngameScript.Helper;
using IngameScript.Ship.Components.Propulsion.Gravity.Wrapper;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRageMath;
using Base6Directions = IngameScript.Helper.Base6Directions;


namespace IngameScript.Ship.Components.Propulsion.Gravity
{
    public class GDrive
    {
        private readonly DirectionalDrive _forwardBackward;
        private readonly DirectionalDrive _leftRight;
        private readonly DirectionalDrive _upDown;

        private BalancedMass Mass;
        private ControllableShip _ship;

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
                    var forward = _ship.LocalOrientationForward;
                    var forwardDir = Base6Directions.Directions[(int)forward];
                    var inverted = forwardDir.Dot(_ship.Position - sphericalGen.GetPosition()) < 0; // TODO: ensure sign
                    var list = genCastArray[forward];
                    list.Add(new GravityGeneratorSpherical(sphericalGen, forward, inverted));
                    
                }
            }

            if (forwardBackward.Count == 0) Program.LogLine($"No Forward/backward gravity generators", LogLevel.Warning);
            if (leftRight.Count == 0) Program.LogLine($"No Left/Right gravity generators", LogLevel.Warning);
            if (upDown.Count == 0) Program.LogLine($"No Up/Down gravity generators", LogLevel.Warning);
            
            _forwardBackward = new DirectionalDrive(forwardBackward, Direction.Forward);
            _leftRight = new DirectionalDrive(leftRight, Direction.Left);
            _upDown = new DirectionalDrive(upDown, Direction.Up);
        }


        public void EarlyUpdate(int frame)
        {
            _forwardBackward.EarlyUpdate(frame);
            _leftRight.EarlyUpdate(frame);
            _upDown.EarlyUpdate(frame);
        }

        public void LateUpdate(int frame)
        {
            _forwardBackward.LateUpdate(frame);
            _leftRight.LateUpdate(frame);
            _upDown.LateUpdate(frame);
        }


        public void ApplyPropulsion(Vector3 propLocal)
        {
            _forwardBackward.SetAcceleration(propLocal.Dot(Vector3D.Forward));
            _leftRight.SetAcceleration(propLocal.Dot(Vector3D.Left));
            _upDown.SetAcceleration(propLocal.Dot(Vector3D.Up));
        }
    }
}