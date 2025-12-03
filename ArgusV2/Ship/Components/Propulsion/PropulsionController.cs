

using System.Collections.Generic;
using IngameScript.Helper;
using IngameScript.Ship.Components.Propulsion.Gravity;
using IngameScript.Ship.Components.Propulsion.Thruster;
using IngameScript.TruncationWrappers;
using Sandbox.ModAPI.Ingame;
using VRageMath;

namespace IngameScript.Ship.Components.Propulsion
{
    public class PropulsionController
    {
        private GDrive _gDrive;
        private TDrive _tDrive;
        private ControllableShip _ship;

        public PropulsionController(List<IMyTerminalBlock> blocks, ControllableShip ship)
        {
            Program.LogLine($"Setting up propulsion controller", LogLevel.Info);
            _ship = ship;
            _gDrive = new GDrive(blocks, ship);
            _tDrive = new TDrive(); // TODO
        }

        public void EarlyUpdate(int frame)
        {
            // Put logic to discern propulsion here?
            
            _gDrive.EarlyUpdate(frame);
            _tDrive.EarlyUpdate(frame);
        }

        public void LateUpdate(int frame)
        {
            var userInput = _ship.Controller.MoveIndicator;

            Matrix matrix; // TODO: Cache cockpit local orientation matrix
            _ship.Controller.Orientation.GetMatrix(out matrix);

            AT_Vector3D desiredMovement = AT_Vector3D.Transform(userInput, matrix);

            if (_ship.Controller.DampenersOverride)
            {
                var velocity = _ship.Velocity;
                var localVelocity = AT_Vector3D.TransformNormal(velocity, MatrixD.Invert(_ship.WorldMatrix)); // TODO: Cache inverted matrix somewhere in ship
                
                var dampenValueForwardBackward = localVelocity * AT_Vector3D.Forward * 10 * GetForwardBackwardAcceleration();
                var dampenValueLeftRight = localVelocity * AT_Vector3D.Left * 10 * GetLeftRightAcceleration();
                var dampenValueUpDown = localVelocity * AT_Vector3D.Up * 10 * GetUpDownAcceleration();

                if (desiredMovement.Dot(AT_Vector3D.Forward) == 0) desiredMovement += dampenValueForwardBackward;
                if (desiredMovement.Dot(AT_Vector3D.Left) == 0) desiredMovement += dampenValueLeftRight;
                if (desiredMovement.Dot(AT_Vector3D.Up) == 0) desiredMovement += dampenValueUpDown;
            }
            
            
            // Put logic to resolve propulsion here?
            _gDrive.ApplyPropulsion(desiredMovement);
            _gDrive.LateUpdate(frame);
            _tDrive.LateUpdate(frame);
        }

        private double GetForwardBackwardAcceleration()
        {
            // TODO: Implement thruster drive acceleration
            return _ship.Mass.TotalMass / _gDrive.GetForwardBackwardForce();
        }

        private double GetLeftRightAcceleration()
        {
            // TODO: Implement thruster drive acceleration
            return _ship.Mass.TotalMass / _gDrive.GetLeftRightForce();
        }

        private double GetUpDownAcceleration()
        {
            // TODO: Implement thruster drive acceleration
            return _ship.Mass.TotalMass / _gDrive.GetUpDownForce();
        }
    }
}