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
    public static class ReceiveKeys
    {
        
    }

    public static class UnicastReceiveKeys
    {
        public const string Direction = "MuD_Direction";
        public const string Distance = "MuD_Distance";
        public const string Owned = "MuD_Owned";
        
        public const string MothershipPosition = "MuD_Position";
        public const string MothershipVelocity = "MuD_Velocity";
        public const string MothershipDirection = "MuD_MDirection";
    }

    public static class SendKeys
    {
        public const string Heartbeat = "DuM_Heartbeat";
        public const string Register = "DuM_Register";
        public const string Deregister = "DuM_Deregister";
    }
    
    
    //TODO: auto docking mode, actively avoid collisions with mothership, squad tags, don't go to world origin if no target
    partial class Program : MyGridProgram
    {
        // config
        private static double MaxAngularVelocityRPM = 30; // 30 for large grid 60 for small grid
        private static string _groupName = "SDXDrone"; // Group everything on the drone with this name
        private static string _mainThrusterName = "MainDrive"; // Name your main drive this - this is merely a forward reference
        private static int _maxHeartbeatMisses = 3600; // In frames, how long we should keep following the previous velocity of the mothership before we give up
        private static float _velocitySecondsFollowAhead = 0.0f; // Velocity to follow ahead by mult
        private static float _minAntennaRange = 5000; // Script will adjust antenna range to keep in contact with mothership, this is the lowest it will go
        
        // Controller gains
        private const double PositionGain = 0.2; 
        private const double VelocityGain = 0.5;
        private const double FaceForwardVelocity = 25;
        private const double FaceForwardDistance = 25;
        private const double FaceAnyVelocity = 50;
        private const double FaceAnyDistance = 50;
        private bool _isFacingForward = false;

        //PID
        private readonly ClampedIntegralPID pitch;
        private readonly ClampedIntegralPID yaw;
        private readonly ClampedIntegralPID roll;
        private readonly double kP = 500; // Proportional gain
        private readonly double kI = 0; // Integral gain
        private readonly double kD = 150; // Derivative gain
        private double integralClamp = 0.05; // Maximum windup for the integral if you are using it
        private static readonly double TimeStep = 1.0 / 60.0;
        
        private Vector3D _mothershipPosition;
        private Vector3 _mothershipVelocity;
        private Vector3 _mothershipDirection;
        private Vector3 _direction = Vector3.Forward; // Need to set to something otherwise there is a distinct possibility that the drone might try to unalive the mothership
        private float _distance;
        private long _owner;
        
        private bool HasOwner => _owner != 0;
        private bool _hadOwner = false;
        private int _heartbeatMisses;
        
        private IMyThrust _mainDrive;
        private IMyRadioAntenna _mainAntenna;

        private List<IMyGyro> _gyros = new List<IMyGyro>();

        public Program()
        {
            debug = new DebugAPI(this, true);

            pitch = new ClampedIntegralPID(kP, kI, kD, TimeStep, -integralClamp, integralClamp);
            yaw = new ClampedIntegralPID(kP, kI, kD, TimeStep, -integralClamp, integralClamp);
            roll = new ClampedIntegralPID(kP, kI, kD, TimeStep, -integralClamp, integralClamp);
            
            _mainDrive = (IMyThrust)GridTerminalSystem.GetBlockWithName(_mainThrusterName);


            var group = GridTerminalSystem.GetBlockGroupWithName(_groupName);
            if (group == null)
            {
                Echo("Group not found! Please create a group called '" + _groupName + "' and recompile!");
                return;
            }
            group.GetBlocksOfType(_gyros);
            group.GetBlocksOfType(Thrusters.thrusters);

            List<IMyRadioAntenna> antennas = new List<IMyRadioAntenna>();
            group.GetBlocksOfType(antennas);
            if (antennas == null || antennas[0] == null)
            {
                Echo("No antennas found! Please add an antenna to the group and recompile");
                return;
            }
            _mainAntenna = antennas[0];
            Runtime.UpdateFrequency = UpdateFrequency.Update1;


            ResetGyroOverrides(_gyros);
            Thrusters.Reset();
        }

        private DebugAPI debug;
        void Main(string argument, UpdateType updateSource)
        {
            ReceiveDataUpdates();
            SendHeartbeat();
            
            if (_heartbeatMisses >= 1)
            {
                _mothershipPosition += _mothershipVelocity * (float)TimeStep;
            }


           
            
            if (!HasOwner)
            {
                if (_hadOwner)
                {
                    Echo("Resetting");
                    _hadOwner = false;
                    ResetGyroOverrides(_gyros);
                    Thrusters.Reset();
                    return;
                }
                Echo("Not connected to mothership, attempting to establish connection...");
                return;
            }

            Echo("Connection established to mothership");

            
            Vector3D targetPos = _mothershipPosition + (_direction * _distance);
            Vector3D currentPos = Me.CubeGrid.WorldVolume.Center;
            Vector3D currentVelocity = Me.CubeGrid.LinearVelocity;
            Vector3D mothershipVelocity = _mothershipVelocity;

            // Error terms
            Vector3D positionalError = currentPos - targetPos;
            var positionLength = positionalError.Length();
           

            // Desired velocity is mothership’s velocity + correction toward target position
            Vector3D targetVelocity = mothershipVelocity ;


            Vector3D relativeVelocity = (currentVelocity - targetVelocity);

            Vector3D velocityComponent = relativeVelocity;
            double velocityComponentLength = velocityComponent.Length();
            Vector3D squaredVelocityComponent = velocityComponent.Normalized() * (velocityComponentLength * velocityComponentLength);
            if (!squaredVelocityComponent.IsValid()) squaredVelocityComponent = Vector3D.Zero;
            Vector3D velocityError = squaredVelocityComponent * VelocityGain + (positionalError * positionLength) * PositionGain;
            
            // Now apply thrust in velocityError direction
            Thrusters.Apply(velocityError);
            
            
            if (relativeVelocity.LengthSquared() < FaceForwardVelocity * FaceForwardVelocity && positionalError.LengthSquared() < FaceForwardDistance * FaceForwardDistance)
            {
                _isFacingForward = true;
                
            }
            else if (relativeVelocity.LengthSquared() > FaceAnyVelocity * FaceAnyVelocity && positionalError.LengthSquared() > FaceAnyDistance * FaceAnyDistance)
            {
                _isFacingForward = false;
            }


            if (_isFacingForward)
            {
                Rotate(_mothershipDirection, 0);
                Echo(_mothershipDirection.ToString());
                Echo("Facing forward");
            }
            else
            {
                Rotate(-velocityError.Normalized(), 0);
                Echo(velocityError.Normalized().ToString());
                Echo("Facing to correct error");
            }

            double dist = (currentPos - _mothershipPosition).Length();
            _mainAntenna.Radius = (float)Math.Max(dist, _minAntennaRange);

            
        }

        private Vector3D SqrVec(Vector3D vec, double i)
        {
            return new Vector3D(
                vec.X * vec.X * Math.Sign(vec.X),
                vec.Y * vec.Y * Math.Sign(vec.Y),
                vec.Z * vec.Z * Math.Sign(vec.Z));
        }
        private Vector3D PowVec(Vector3D vec, double i)
        {
            return new Vector3D(
                Math.Pow(vec.X, i) * Math.Sign(vec.X),
                Math.Pow(vec.Y, i) * Math.Sign(vec.Y),
                Math.Pow(vec.Z, i) * Math.Sign(vec.Z));
        }

        private void ReceiveDataUpdates()
        {
            bool gotUpdateFromMothership = false;
            Echo("Receiving updates");
            while (IGC.UnicastListener.HasPendingMessage)
            {
                var message = IGC.UnicastListener.AcceptMessage();
                switch (message.Tag)
                {
                    case UnicastReceiveKeys.Direction:
                        if (message.Source != _owner) break;
                        _direction = (Vector3)message.Data;
                        Echo("Received new direction");
                        break;
                    case UnicastReceiveKeys.Distance:
                        if (message.Source != _owner) break;
                        _distance = (float)message.Data;
                        Echo("received new distance");
                        break;
                    case UnicastReceiveKeys.Owned:
                        _owner = (long)message.Data;
                        Echo("Got new owner");
                        break;
                    case UnicastReceiveKeys.MothershipPosition:
                        Vector3D? value = message.Data as Vector3D?;
                        if (value != null)
                        {
                            _mothershipPosition = (Vector3D)value;
                            gotUpdateFromMothership = true;
                        }
                        break;
                    case UnicastReceiveKeys.MothershipVelocity:
                        var value2 = message.Data as Vector3?;
                        if (value2 != null)
                        {
                            _mothershipVelocity = (Vector3)value2;
                            gotUpdateFromMothership = true;
                        }
                        break;
                    case UnicastReceiveKeys.MothershipDirection:
                        var value3 = message.Data as Vector3?;
                        if (value3 != null)
                        {
                            _mothershipDirection = (Vector3)value3;
                            gotUpdateFromMothership = true;
                        }
                        break;
                }
                
            }
            
            if (_owner != 0 && !gotUpdateFromMothership)
            {
                if (++_heartbeatMisses >= _maxHeartbeatMisses)
                {
                    _owner = 0;
                    _hadOwner = true;
                    Echo("Lost owner heartbeat");
                }

                Echo("Missed " + _heartbeatMisses + "/" + _maxHeartbeatMisses + " heartbeats");
            }
            else
            {
                _heartbeatMisses = 0;
                
            }
        }

        private void SendHeartbeat()
        {
            if (_owner != 0)
            {
                if (_heartbeatMisses > 0)
                {
                    IGC.SendUnicastMessage(_owner, SendKeys.Register, 0);
                }
                
                Echo("Heartbeat sending");
                IGC.SendUnicastMessage(_owner, SendKeys.Heartbeat, Me.CubeGrid.WorldVolume.Center);
            }
            else // Try and register
            {
                IGC.SendBroadcastMessage(SendKeys.Register, 0);
            }
        }
        
        private void Rotate(Vector3D desiredGlobalFwdNormalized, double roll)
        {
            double gp;
            double gy;
            var gr = roll;

            //Rotate Toward forward

            var waxis = Vector3D.Cross(_mainDrive.WorldMatrix.Backward, desiredGlobalFwdNormalized);
            var axis = Vector3D.TransformNormal(waxis, MatrixD.Transpose(_mainDrive.WorldMatrix));
            var x = pitch.Control(-axis.X);
            var y = yaw.Control(-axis.Y);

            gp = MathHelper.Clamp(x, -MaxAngularVelocityRPM, MaxAngularVelocityRPM);
            gy = MathHelper.Clamp(y, -MaxAngularVelocityRPM, MaxAngularVelocityRPM);

            if (Math.Abs(gy) + Math.Abs(gp) > MaxAngularVelocityRPM)
            {
                var adjust = MaxAngularVelocityRPM / (Math.Abs(gy) + Math.Abs(gp));
                gy *= adjust;
                gp *= adjust;
                // No you're right, this sucks
            }
            
            ApplyGyroOverride(gp, gy, gr, _gyros, _mainDrive.WorldMatrix);
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