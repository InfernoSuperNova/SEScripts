using System;
using System.Collections.Generic;
using IngameScript.Helper;
using IngameScript.Helper.Log;
using IngameScript.TruncationWrappers;
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
            Program.LogLine("Setting up gyro manager", LogLevel.Info);
            _gyros = new List<IMyGyro>();
            foreach (var b in blocks)
            {
                var gyro = b as IMyGyro;
                if (gyro != null)
                {
                    _gyros.Add(gyro);
                }
            }
            
            if (_gyros.Count <= 0) Program.LogLine($"No gyroscopes found in group: {Config.String.GroupName}", LogLevel.Warning);
            
            
            _pitch = new PIDController(Config.Pid.ProportionalGain, Config.Pid.IntegralGain, Config.Pid.DerivativeGain, 
                Config.Pid.IntegralUpperLimit, Config.Pid.IntegralLowerLimit);
            _yaw = new PIDController(Config.Pid.ProportionalGain, Config.Pid.IntegralGain, Config.Pid.DerivativeGain,
                Config.Pid.IntegralUpperLimit, Config.Pid.IntegralLowerLimit);
        }
        
        public void Rotate(ref FiringSolution solution, double roll = 0)
        {
            int roundValue = 7;
            double rotationalGain = 1.0;
            if (solution.Dot > 0.9999)
            {
                rotationalGain *= 0.8;
                roundValue = 4;
            }

            if (solution.Dot > 0.99999)
            {
                rotationalGain *= 0.8;
                roundValue = 3;
            }
            if (solution.Dot > 0.999999)
            {
                rotationalGain *= 0.8;
                roundValue = 2;
            }
            if (solution.Dot > 0.9999999)
            {
                rotationalGain *= 0.8;
                roundValue = 1;
            }
            
            double gp;
            double gy;
            var gr = roll;

            //Rotate Toward forward

            var waxis = AT_Vector3D.Cross(solution.CurrentForward, solution.DesiredForward);
            var axis = AT_Vector3D.TransformNormal(waxis, MatrixD.Transpose(solution.WorldMatrix));
            var x = _pitch.Filter(-axis.X, roundValue);
            var y = _yaw.Filter(-axis.Y, roundValue);

            gp = MathHelper.Clamp(x, -Config.General.MaxAngularVelocityRpm, Config.General.MaxAngularVelocityRpm);
            gy = MathHelper.Clamp(y, -Config.General.MaxAngularVelocityRpm, Config.General.MaxAngularVelocityRpm);

            if (Math.Abs(gy) + Math.Abs(gp) > Config.General.MaxAngularVelocityRpm)
            {
                var adjust = Config.General.MaxAngularVelocityRpm / (Math.Abs(gy) + Math.Abs(gp));
                gy *= adjust;
                gp *= adjust;
            }
            gp *= rotationalGain;
            gy *= rotationalGain;
            ApplyGyroOverride(gp, gy, gr, solution.WorldMatrix);
        }


        private void ApplyGyroOverride(double pitchSpeed, double yawSpeed, double rollSpeed, MatrixD worldMatrix)
        {
            var rotationVec = new AT_Vector3D(pitchSpeed, yawSpeed, rollSpeed);
            var relativeRotationVec = AT_Vector3D.TransformNormal(rotationVec, worldMatrix);
            foreach (var gyro in _gyros)
                if (gyro.IsFunctional && gyro.IsWorking && gyro.Enabled && !gyro.Closed)
                {
                    var transformedRotationVec =
                        AT_Vector3D.TransformNormal(relativeRotationVec, MatrixD.Transpose(gyro.WorldMatrix));
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