using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using CoreSystems.Api;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        private const string GyroGroupName = "Gyros";
        private List<IMyGyro> _gyros = new List<IMyGyro>();

        private static WcPbApi _wc;
        private DebugAPI debug;

        private bool _doAim = true;
        public Program()
        { 
            var gyroGroup = GridTerminalSystem.GetBlockGroupWithName(GyroGroupName);
            gyroGroup.GetBlocksOfType(_gyros);
            
            _wc = new WcPbApi();
            try
            {
                _wc.Activate(Me);
            }
            catch (Exception e)
            {
                Echo("WeaponCore Api is failing! \n Make sure WeaponCore is enabled!"); 
                Echo(e.Message);
                Echo(e.StackTrace);
                return;
            }
            debug = new DebugAPI(this);
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
        }

        public void Save()
        {
            // Called when the program needs to save its state. Use
            // this method to save your state to the Storage field
            // or some other means. 
            // 
            // This method is optional and can be removed if not
            // needed.
        }
        
        public void Main(string argument, UpdateType updateSource)
        {
            if ((updateSource & UpdateType.Update1) != 0)
            {
                Update();
            }
            if ((updateSource & (UpdateType.Trigger | UpdateType.Terminal)) != 0) Command(argument);
            
        }

        private void Command(string command)
        {
            if (command == "toggle ship aim")
            {
                _doAim = !_doAim;
                if (_doAim == false)
                {
                    foreach (var gyro in _gyros)
                    {
                        if (gyro.GyroOverride) gyro.GyroOverride = false;
                    }
                    
                }
            }
        }
        
        public void Update()
        {
            if (!_doAim) return;
            var target = _wc.GetAiFocus(Me.EntityId);
            var targetInfo = target ?? new MyDetectedEntityInfo();
            Echo(targetInfo.IsEmpty().ToString());
            if (!targetInfo.IsEmpty())
            {
                Vector3D losRate = GetLineOfSightRate(targetInfo.Position, targetInfo.Velocity, Me.CubeGrid.GetPosition(), Me.CubeGrid.LinearVelocity, Me.CubeGrid.WorldMatrix);
                
                
                
                Vector3D LocalAngularVelocity = Vector3D.TransformNormal(losRate, MatrixD.Transpose(Me.CubeGrid.WorldMatrix));
                ApplyGyroOverride(LocalAngularVelocity.Y, -LocalAngularVelocity.X, LocalAngularVelocity.Z, _gyros,
                    Me.CubeGrid.WorldMatrix);
            }
        }

        
        
        void ApplyGyroOverride(double pitchSpeed, double yawSpeed, double rollSpeed, List<IMyGyro> gyroList, MatrixD worldMatrix)
        {
            var rotationVec = new Vector3D(pitchSpeed, yawSpeed, rollSpeed);
            var relativeRotationVec = Vector3D.TransformNormal(rotationVec, worldMatrix);
            foreach (var thisGyro in gyroList)
            {
                var transformedRotationVec = Vector3D.TransformNormal(
                    relativeRotationVec,
                    Matrix.Transpose(thisGyro.WorldMatrix)
                );
                thisGyro.Pitch = (float)transformedRotationVec.X;
                thisGyro.Yaw = (float)transformedRotationVec.Y;
                thisGyro.Roll = (float)transformedRotationVec.Z;
                thisGyro.GyroOverride = true;
                return;
            }
        }
        
        
        Vector3D GetLineOfSightRate(Vector3D targetPos, Vector3D targetVel, Vector3D mePos, Vector3D meVel, MatrixD worldMatrix)
        {
            // Calculate relative target position and velocity
            Vector3D relativePos = mePos - targetPos;
            Vector3D relativeVel = meVel - targetVel;

            // Transform relative position into the ship's local coordinate system
            Vector3D targetDir = mePos - targetPos;
    
            // Calculate LOS rate (example calculation, your calculation might vary)
            Vector3D losRate = Vector3D.Cross(relativeVel, relativePos) / relativePos.LengthSquared();

            // Transform LOS rate from world space to ship-local space
            Vector3D localLosRate = Vector3D.TransformNormal(losRate, MatrixD.Transpose(worldMatrix));
    
            // if (targetDir.Dot(worldMatrix.Up) < 0) // Negative dot: target is below
            // {
            //     localLosRate.Z = -localL osRate.Z;
            //     Echo("Up");
            // }
            //
            // // Yaw (local Y): Invert based on "left" or "right" relative position
            // if (targetDir.Dot(worldMatrix.Right) > 0) // Positive dot: target is on the right
            // {
            //     localLosRate.Z = -localLosRate.Z;
            //     Echo("right");
            // }

            // Roll (local Z): Invert based on "forward" or "backward" relative position
            if (targetDir.Dot(worldMatrix.Forward) < 0) // Negative dot: target is behind
            {
                localLosRate.X = -localLosRate.X;
                localLosRate.Y = -localLosRate.Y;
                localLosRate.Z = -localLosRate.Z;
                
                Echo("Forward");
            }


            return localLosRate;
        }
    }
}