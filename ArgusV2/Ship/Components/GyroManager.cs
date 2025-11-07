using System;
using System.Collections.Generic;
using IngameScript.Helper;
using Sandbox.ModAPI.Ingame;
using VRageMath;

namespace IngameScript.Ship.Components
{
    public class GyroManager
    {
        private readonly List<IMyGyro> _gyros;
        private readonly PIDController _pitch;
        private readonly PIDController _yaw;
        
        
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
            
            
            _pitch = new PIDController(Config.ProportialGain, Config.IntegralGain, Config.DerivativeGain, 
                Config.IntegralUpperLimit, Config.IntegralLowerLimit);
            _yaw = new PIDController(Config.ProportialGain, Config.IntegralGain, Config.DerivativeGain,
                Config.IntegralUpperLimit, Config.IntegralLowerLimit);
        }
        
        public void Rotate(Vector3D desiredGlobalFwdNormalized, double onTarget, IMyShipController controller, double roll = 0)
        {
            int roundValue = 7;
            double rotationalGain = 1.0;
            if (onTarget > 0.9999)
            {
                rotationalGain *= 0.8;
                roundValue = 4;
            }

            if (onTarget > 0.99999)
            {
                rotationalGain *= 0.8;
                roundValue = 3;
            }
            if (onTarget > 0.999999)
            {
                rotationalGain *= 0.8;
                roundValue = 2;
            }
            if (onTarget > 0.9999999)
            {
                rotationalGain *= 0.8;
                roundValue = 1;
            }
            
            double gp;
            double gy;
            var gr = roll;

            //Rotate Toward forward

            var waxis = Vector3D.Cross(controller.WorldMatrix.Forward, desiredGlobalFwdNormalized);
            var axis = Vector3D.TransformNormal(waxis, MatrixD.Transpose(controller.WorldMatrix));
            var x = _pitch.Filter(-axis.X, roundValue);
            var y = _yaw.Filter(-axis.Y, roundValue);

            gp = MathHelper.Clamp(x, -Config.MaxAngularVelocityRpm, Config.MaxAngularVelocityRpm);
            gy = MathHelper.Clamp(y, -Config.MaxAngularVelocityRpm, Config.MaxAngularVelocityRpm);

            if (Math.Abs(gy) + Math.Abs(gp) > Config.MaxAngularVelocityRpm)
            {
                var adjust = Config.MaxAngularVelocityRpm / (Math.Abs(gy) + Math.Abs(gp));
                gy *= adjust;
                gp *= adjust;
            }
            gp *= rotationalGain;
            gy *= rotationalGain;
            ApplyGyroOverride(gp, gy, gr, controller.WorldMatrix);
        }


        private void ApplyGyroOverride(double pitchSpeed, double yawSpeed, double rollSpeed, MatrixD worldMatrix)
        {
            var rotationVec = new Vector3D(pitchSpeed, yawSpeed, rollSpeed);
            var relativeRotationVec = Vector3D.TransformNormal(rotationVec, worldMatrix);
            Program.Log(_gyros.Count);
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