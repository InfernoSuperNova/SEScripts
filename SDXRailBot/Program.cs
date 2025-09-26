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
        private const string DesignatorName = "Block_2";
        private const string RailGroupName = "Block_3";
        private const string GyroGroupName = "Block_4";
        private const float ProjectileSpeed = 2500; // m/s
        private const double FireSensitivity = 0.999998; // Basically a measure of how close to "on target" the script has to be to fire (1 being mathematically perfect, -1 being exactly pointing away)
        // 1 is almost never appropriate
        // 0.9999998 is good for fixed rails
        // 0.999998 is good for kess cannons
        private const int MinWeaponDelay = 7; // frames
        
        public static WcPbApi Api;

        private double weaponOffsetUp = 0.0;
        private double weaponOffsetRight = 0.0;
        private double weaponOffsetForward = 0.0;
        
        
        private readonly PIDController pitch;
        private readonly PIDController yaw;
        private readonly PIDController roll;
        private readonly double kP = 100; // Proportional gain
        private readonly double kI = 0.01; // Integral gain
        private readonly double kD = 30; // Derivative gain
        private double integralClamp = 0.2; // Maximum windup for the integral if you are using it
        private static double MaxAngularVelocityRPM = 30;
        
        // Need:
        // Railgun list
        // Gyro list
        private IMyTerminalBlock _designator;

        private List<IMyTerminalBlock> _rails = new List<IMyTerminalBlock>();
        private int _currentRailIndex;
        private IMyTerminalBlock _referenceRail;
        private IMyTerminalBlock _lastRail;
        private List<IMyGyro> _gyros = new List<IMyGyro>();
        private IMyShipController _mainCockpit;
        private bool _doAim = true;
        private bool _wasAiming = false;

        private int _weaponDelayFrame = 0;
        
        private Vector3 lastTargetVel = Vector3.Zero;

        private DebugAPI debug;
        public Program()
        {
            debug = new DebugAPI(this);
            Api = new WcPbApi();
            try
            {
                Api.Activate(Me);
            }
            catch (Exception e)
            {
                Echo("API fail"); 
                Echo(e.Message);
                Echo(e.StackTrace);
                return;
            }

            pitch = new PIDController(kP, kI, kD, -integralClamp, integralClamp);
            yaw = new PIDController(kP, kI, kD, -integralClamp, integralClamp);
            roll = new PIDController(kP, kI, kD, -integralClamp, integralClamp);

            
            _designator = GridTerminalSystem.GetBlockWithName(DesignatorName);
            _mainCockpit = (IMyShipController)GridTerminalSystem.GetBlockWithName("block_1");
            //TODO: Null checks
            var gyroGroup = GridTerminalSystem.GetBlockGroupWithName(GyroGroupName);
            var railGroup = GridTerminalSystem.GetBlockGroupWithName(RailGroupName);
            
            gyroGroup.GetBlocksOfType(_gyros);
            railGroup.GetBlocksOfType(_rails);
            
            _rails.Sort((a, b) =>
            {
                int valueA = 0, valueB = 0;
                int.TryParse(a.CustomData, out valueA);
                int.TryParse(b.CustomData, out valueB);
                return valueA.CompareTo(valueB);
            });
            
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
            if (command == "command_1") _doAim = !_doAim;
        }
        
        private void Update()
        {
            var threats = new Dictionary<long, MyDetectedEntityInfo>();
            Api.GetSortedThreatsByID(Me, threats);
            foreach (var threat in threats)
            {
                if (threat.Value.HitPosition != null)
                    debug.DrawLine(Me.CubeGrid.GetPosition(), (Vector3D)threat.Value.HitPosition, Color.Red, 0.25f,
                        0.016f, true);  
            }
            
            
            
            
            if (!_doAim)
            {
                if (_wasAiming)
                {
                    ResetGyroOverrides(_gyros);
                    _wasAiming = false;
                }
                

                return;
            }
            var roll = _mainCockpit.RollIndicator;
            
            _wasAiming = true;
            
            //Echo(Runtime.LastRunTimeMs.ToString());
            
            // var target = Api.GetAiFocus(Me.EntityId);
            // var thing = target is MyDetectedEntityInfo ? (MyDetectedEntityInfo)target : new MyDetectedEntityInfo();
            // Echo(thing.Name);
            
            
            var thing2 = Api.GetWeaponTarget(_designator) ?? new MyDetectedEntityInfo();
            
            var targetVelocity = thing2.Velocity;
            
            var entityId = thing2.EntityId;
            GetNewReferenceRail();
            if (entityId == 0)
            {
                ResetGyroOverrides(_gyros);
                return;
            }
            if (_referenceRail == null)
            {
                if (_lastRail == null) return;
                Vector3D targetPos2 = Api.GetPredictedTargetPosition(_lastRail, entityId, 0) ?? new Vector3D();
                if (targetPos2 == Vector3.Zero)
                    if (thing2.HitPosition != null)
                    {
                        targetPos2 = (Vector3D)thing2.HitPosition;

                    }
                    else
                    {
                        ResetGyroOverrides(_gyros);
                        return;
                    }
                var weaponPos2 = GetOffsetWeaponPosition(_lastRail);
                debug.DrawLine(targetPos2, weaponPos2, Color.Red, 0.25f, 0.016f, true);
                var weaponToTarget2 = targetPos2 - weaponPos2;
                var dirNormal2 = weaponToTarget2.Normalized();
            
                double onTarget2 = _referenceRail.WorldMatrix.Forward.Dot(dirNormal2);
                Rotate(dirNormal2, roll * 10, _referenceRail, onTarget2);
                
            }
            Vector3D targetAcc = (targetVelocity - lastTargetVel) / 60;
            Vector3D targetPos = Api.GetPredictedTargetPosition(_referenceRail, entityId, 0) ?? new Vector3D();
            var weaponPos = GetOffsetWeaponPosition(_lastRail);
            //Vector3D targetPos = InterceptSolver.Compute((Vector3D)thing2.HitPosition, thing2.Velocity - _referenceRail.CubeGrid.LinearVelocity, targetAcc, weaponPos, ProjectileSpeed, 0, ProjectileSpeed);
            
            if (targetPos == Vector3.Zero)
                if (thing2.HitPosition != null)
                {
                    targetPos = (Vector3D)thing2.HitPosition;

                }
                else
                {
                    ResetGyroOverrides(_gyros);
                    return;
                }
            
            debug.DrawLine(targetPos, weaponPos, Color.White, 0.25f, 0.016f, true);
            var weaponToTarget = targetPos - weaponPos;
            var dirNormal = weaponToTarget.Normalized();
            
            double onTarget = _referenceRail.WorldMatrix.Forward.Dot(dirNormal);
            Rotate(dirNormal, roll * 10, _referenceRail, onTarget);
            
            bool inRange = targetPos != thing2.HitPosition;
            
            //Echo(onTarget.ToString());
            
            if (++_weaponDelayFrame >= MinWeaponDelay && inRange && onTarget >= FireSensitivity) // Can't get any better than this!!
            {
                _weaponDelayFrame = 0;
                Api.FireWeaponOnce(_referenceRail);
            }
            lastTargetVel = targetVelocity;
        }
        private Vector3D GetOffsetWeaponPosition(IMyTerminalBlock weapon)
        {
            var weaponMatrix = weapon.WorldMatrix;
            return weapon.GetPosition() + weaponMatrix.Up * weaponOffsetUp + weaponMatrix.Forward * weaponOffsetForward + weaponMatrix.Right * weaponOffsetRight;
        }
        
        private void GetNewReferenceRail()
        {

            if (_referenceRail == null || !Api.IsWeaponReadyToFire(_referenceRail))
            {
                _currentRailIndex = (_currentRailIndex + 1) % _rails.Count;
                if (Api.IsWeaponReadyToFire(_rails[_currentRailIndex]))
                {
                    _referenceRail = _rails[_currentRailIndex];
                    _lastRail = _referenceRail;
                }
                
            }
        }
        
        
        
        private void Rotate(Vector3D desiredGlobalFwdNormalized, double roll, IMyTerminalBlock reference, double onTarget)
        {
            int roundValue = 7;
            double rotationalGain = 1.0;
            if (onTarget > 0.999)
            {
                rotationalGain *= 0.8;
                roundValue = 4;
            }

            if (onTarget > 0.9999)
            {
                rotationalGain *= 0.8;
                roundValue = 3;
            }
            if (onTarget > 0.99999)
            {
                rotationalGain *= 0.8;
                roundValue = 2;
            }
            if (onTarget > 0.999999)
            {
                rotationalGain *= 0.8;
                roundValue = 1;
            }
            
            double gp;
            double gy;
            var gr = roll;

            //Rotate Toward forward

            var waxis = Vector3D.Cross(reference.WorldMatrix.Forward, desiredGlobalFwdNormalized);
            var axis = Vector3D.TransformNormal(waxis, MatrixD.Transpose(reference.WorldMatrix));
            var x = pitch.Filter(-axis.X, roundValue);
            var y = yaw.Filter(-axis.Y, roundValue);

            gp = MathHelper.Clamp(x, -MaxAngularVelocityRPM, MaxAngularVelocityRPM);
            gy = MathHelper.Clamp(y, -MaxAngularVelocityRPM, MaxAngularVelocityRPM);

            if (Math.Abs(gy) + Math.Abs(gp) > MaxAngularVelocityRPM)
            {
                var adjust = MaxAngularVelocityRPM / (Math.Abs(gy) + Math.Abs(gp));
                gy *= adjust;
                gp *= adjust;
                // No you're right, this sucks
            }

            gp *= rotationalGain;
            gy *= rotationalGain;
            
            ApplyGyroOverride(gp, gy, gr, _gyros, reference.WorldMatrix);
        }
        
        
        private void ApplyGyroOverride(double pitchSpeed, double yawSpeed, double rollSpeed, List<IMyGyro> gyroList,
            MatrixD worldMatrix)
        {
            var rotationVec = new Vector3D(pitchSpeed, yawSpeed, rollSpeed);
            var relativeRotationVec = Vector3D.TransformNormal(rotationVec, worldMatrix);

            foreach (var gyro in gyroList)
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

        private void ResetGyroOverrides(List<IMyGyro> gyroList)
        {
            foreach (var gyro in gyroList)
                if (gyro.IsFunctional && gyro.IsWorking && gyro.Enabled && !gyro.Closed)
                {
                    gyro.GyroOverride = false;
                    return;
                }
        }
    }
}