using System;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using VRageMath;

namespace IngameScript
{
    /// <summary>
    /// Item class.
    /// </summary>
    /// <summary>
    /// Item class.
    /// </summary>
    public class GyroManager
    {
        private readonly List<IMyGyro> _gyros;
        
        
        public GyroManager(List<IMyTerminalBlock> blocks)
        {
            _gyros = new List<IMyGyro>();
            foreach (var b in blocks)
            {
                var gyro = b as IMyGyro;
                if (gyro != null)
                {
                    _gyros.Add(gyro);
                }
            }
            
        }
        

        public void ApplyGyroOverride(double pitchSpeed, double yawSpeed, double rollSpeed, MatrixD worldMatrix)
        {
            var rotationVec = new Vector3D(pitchSpeed, yawSpeed, rollSpeed);
            var relativeRotationVec = Vector3D.TransformNormal(rotationVec, worldMatrix);
            foreach (var gyro in _gyros)
                if (gyro.IsFunctional && gyro.IsWorking && gyro.Enabled && !gyro.Closed)
                {
                    var transformedRotationVec =
                        Vector3D.TransformNormal(relativeRotationVec, MatrixD.Transpose(gyro.WorldMatrix));
                    gyro.Pitch = (float)transformedRotationVec.X;
                    gyro.Yaw = (float)transformedRotationVec.Y;
                    gyro.Roll = (float)transformedRotationVec.Z;
                    gyro.GyroOverride = true;
                    return;
                }
        }

        public void ResetGyroOverrides()
        {
            foreach (var gyro in _gyros)
                if (gyro.IsFunctional && gyro.IsWorking && gyro.Enabled && !gyro.Closed)
                {
                    gyro.GyroOverride = false;
                    return;
                }
        }
        
        
    }
}