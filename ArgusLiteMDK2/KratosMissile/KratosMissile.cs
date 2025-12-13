using System;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame;
using VRageMath;


//DebugAPI debugAPI = new DebugAPI(ALDebug.program);
//using (debugAPI.Measure((t) => ALDebug.program.Echo($"diff={t.TotalMilliseconds * 1000 - 0.2}"))) A surprise tool that will help us later
namespace IngameScript
{
    public enum MissileState
    {
        StandingByForLaunch,
        MaintainLaunchTrajectory,
        InterceptWaypoint,
        InterceptTarget,
        CancelVelocity,
        HoldPosition,
    }

    public enum MissilePropulsionType
    {
        Ion,
        Hydrogen,
        Atmospheric
    }

    internal class KratosMissile
    {
        public MissileState State = MissileState.StandingByForLaunch;
        
        private readonly double _accel;
        
        private double _serverMaxSpeed = 150; // BAD
        private int _maxDistanceFromLoiterPosition = 50;
        private int _frame;
        private int _minFramesAliveToDetonate = 60;
        private double _hitSurroundPointDist = 7;
        private double _fuelRatio = 0.5;
        private double _minFuelRatio = 0.1;
        
        
        
        
        
       
        public readonly bool ConstructedSuccessfully;
        
        private int _currentFrameMaintainingTrajectory;
        private int _detIndex;
        private Vector3D _distanceToTarget;
    #region Motion
        private Vector3D _position;
        private Vector3D _previousPosition;
        private Vector3D _velocity;
        private Vector3D _previousVelocity;
        private Vector3D _previousWaypointPosition;
    #endregion

    #region Blocks
        public readonly IMyGyro Gyro;
        
    
        public readonly IMyGasTank[] GasTanks;
        
        private IMyTerminalBlock[] _blocks;
        private readonly IMyBatteryBlock[] _batteries;
        private readonly IMyShipConnector[] _connectors;
        private readonly IMyThrust[] _forwardThrusters;
        private readonly IMyWarhead[] _kaboom;
        private readonly IMyShipMergeBlock[] _mergers; 
        private IMyCubeGrid _missileGrid;
    #endregion

        public bool Functional;
        


        private readonly int _framesToMaintainTrajectory;
        
        

        
        


        private double _gyroPreviousPitch;
        private double _gyroPreviousRoll;
        private double _gyroPreviousYaw;
        public bool HasLaunched;
        

        private readonly double _maxAngularVelocityRpm = 60;
        

        
        public readonly MissilePropulsionType MissilePropulsionType = MissilePropulsionType.Ion;
        public readonly string Name = "";
        private readonly PID _pitch;


        // Navigation constant, typically between 3 and 5
        private readonly double _pnGain = 5.0;
        

        private double _previousPitchSpeed;
        private double _previousYawSpeed;

        private readonly IMyThrust _referenceThruster;
        
        public Vector3D SurroundDir = Vector3D.Zero;
        public double SurroundDist = 0;
        
        // Definitions for the target and host ships
        
        public ArgusShip Host; 
        public ArgusTargetableShip Target;

        // TODO: Regions
        public TargetableBlockCategory _targetedBlockCategory = TargetableBlockCategory.Default;
        public Dictionary<ArgusTargetableShip, Dictionary<TargetableBlockCategory, int>> TargetedCategoryBlockIndices = new Dictionary<ArgusTargetableShip, Dictionary<TargetableBlockCategory, int>>();
       
        

        private readonly double _timeStep = 1 / 60.0;


        private readonly List<Vector3D> _waypoints = new List<Vector3D>();
        private readonly PID _yaw;

        public KratosMissile(ArgusShip host, List<IMyTerminalBlock> blocks, KratosMissileBehavior behavior, double pnGain, string name, bool hasDetachmentGroup)
        {
            this.Name = name;
            Host = host;
            this._blocks = blocks.ToArray();
            if (blocks.Count == 0) return;
            var forwardThrustersTempList = new List<IMyThrust>();
            var batteriesTempList = new List<IMyBatteryBlock>();
            var gasTanksTempList = new List<IMyGasTank>();
            var kaboomTempList = new List<IMyWarhead>();
            var mergersTempList = new List<IMyShipMergeBlock>();
            var connectorsTempList = new List<IMyShipConnector>();

            double mass = 0;
            double force = 0;
            foreach (var block in blocks)
            {
                mass += block.Mass;
                if (block is IMyThrust)
                {
                    forwardThrustersTempList.Add(block as IMyThrust);
                    // Check the thruster type

                    switch (block.BlockDefinition.SubtypeName)
                    {
                        case "SmallBlockSmallAtmosphericThrust":
                            MissilePropulsionType = MissilePropulsionType.Atmospheric;
                            break;
                        case "SmallBlockSmallHydrogenThrust":
                            MissilePropulsionType = MissilePropulsionType.Hydrogen;
                            break;
                    }

                    force += (block as IMyThrust).MaxEffectiveThrust;
                }
                else if (block is IMyBatteryBlock)
                {
                    batteriesTempList.Add(block as IMyBatteryBlock);
                }
                else if (block is IMyGyro)
                {
                    Gyro = block as IMyGyro;
                    if (Gyro.CustomData == "KMM") return;
                     _missileGrid = Gyro.CubeGrid;
                }
                else if (block is IMyGasTank)
                {
                    var gasTank = block as IMyGasTank;
                    if (gasTank.FilledRatio > _fuelRatio) gasTank.Stockpile = false;
                    else gasTank.Stockpile = true;
                    gasTanksTempList.Add(gasTank);
                }
                else if (block is IMyWarhead)
                {
                    kaboomTempList.Add(block as IMyWarhead);
                }
                else if (block is IMyShipMergeBlock)
                {
                    mergersTempList.Add(block as IMyShipMergeBlock);
                }
                else if (block is IMyShipConnector)
                {
                    var connector = block as IMyShipConnector;
                    connector.Connect();
                    connectorsTempList.Add(connector);
                }
            }

            _forwardThrusters = forwardThrustersTempList.ToArray();
            _batteries = batteriesTempList.ToArray();
            GasTanks = gasTanksTempList.ToArray();
            _kaboom = kaboomTempList.ToArray();
            _mergers = mergersTempList.ToArray();
            _connectors = connectorsTempList.ToArray();

            _pitch = new PID(behavior.KP, behavior.KI, behavior.KD, Program.TimeStep);
            _yaw = new PID(behavior.KP, behavior.KI, behavior.KD, Program.TimeStep);

            _accel = force / mass;


            foreach (var tank in GasTanks)
            {
                if (tank.FilledRatio > _fuelRatio)
                {
                    tank.Stockpile = false;
                }
            }
            
            ConstructedSuccessfully =
                (MissilePropulsionType == MissilePropulsionType.Ion && _forwardThrusters.Length > 0 && _batteries.Length > 0 &&
                 _kaboom.Length > 0 && _mergers.Length > 0 && Gyro != null)
                || (MissilePropulsionType == MissilePropulsionType.Hydrogen && _forwardThrusters.Length > 0 && _batteries.Length > 0 &&
                    GasTanks.Length > 0 && _kaboom.Length > 0 && (_mergers.Length > 0 || hasDetachmentGroup || _connectors.Length > 0) &&
                    Gyro != null)
                || (MissilePropulsionType == MissilePropulsionType.Atmospheric && _forwardThrusters.Length > 0 && _batteries.Length > 0 &&
                    _kaboom.Length > 0 && _mergers.Length > 0 && Gyro != null);

            

            if (!ConstructedSuccessfully) return;
            _referenceThruster = _forwardThrusters[0];
            this._pnGain = pnGain;
            this._framesToMaintainTrajectory = behavior.MaintainTrajectorAfterLaunchFrames;

            //foreach (var gyro in gyroTempList)
            //{
            //    gyro.CustomData = "KMM";
            //}
        }



        public void Refuel()
        {
            foreach (var tank in GasTanks)
            {
                if (tank.FilledRatio > _fuelRatio)
                {
                    tank.Stockpile = false;
                }
            }
            foreach (var connector in this._connectors)
                if (!connector.IsConnected)
                    connector.Connect();
        }

        public void UpdateMotion(Vector3D targetPosition)
        {
            // _missileGrid = Gyro.CubeGrid;
            // var boundingSphere = _missileGrid.WorldVolume;
            _previousPosition = _position;
            // _position = boundingSphere.Center;
            _previousVelocity = _velocity;
            _velocity = (_position - _previousPosition) / Program.TimeStep;
            _distanceToTarget = targetPosition - _position;
        }
        public void Update(Vector3D gravity, Vector3D gravityNormal)
        {
            ALDebug.Echo("Missile updating");

            
            _frame++;
            
            

            
            var forwardThrusters = this._forwardThrusters;
            var batteries = this._batteries;
            var gasTanks = this.GasTanks;
            var kaboom = this._kaboom;
            var mergers = this._mergers;
            var connectors = this._connectors;
            Functional = CheckBlocks(forwardThrusters, batteries, gasTanks, kaboom, mergers, connectors, Gyro);


            var missileToTargetDistanceSquared = _distanceToTarget.ALengthSquared();

            ALDebug.Echo(State);
            switch (State)
            {
                case MissileState.StandingByForLaunch:
                    break;
                case MissileState.MaintainLaunchTrajectory:
                    if (_currentFrameMaintainingTrajectory < _framesToMaintainTrajectory)
                    {
                        Gyro.GyroOverride = true;
                        _currentFrameMaintainingTrajectory++;
                        if (Gyro.Pitch != 0 || Gyro.Yaw != 0 || Gyro.Roll != 0)
                        {
                            Gyro.Pitch = 0;
                            Gyro.Yaw = 0;
                            Gyro.Roll = 0;
                        }

                        foreach (var thruster in forwardThrusters)
                        {
                            thruster.Enabled = true;
                            thruster.ThrustOverride = 1000000;
                        }
                    }
                    else
                    {
                        State = MissileState.InterceptWaypoint;
                    }

                    break;
                case MissileState.InterceptWaypoint:

                    STD_GUIDANCE(_waypoints[0],(_waypoints[0] - _previousWaypointPosition) / Program.TimeStep,Vector3D.Zero);
                    _previousWaypointPosition = _waypoints[0];
                    TryTargetNextWaypoint(missileToTargetDistanceSquared);
                    break;
                case MissileState.InterceptTarget:
                    
                    STD_GUIDANCE(Target.GetBlockPositionOfCategoryAtIndex(_targetedBlockCategory,
                        TargetedCategoryBlockIndices[Target][_targetedBlockCategory]), Target.Velocity, Target.Acceleration);

                    TryDetonate(missileToTargetDistanceSquared);
                    break;
                case MissileState.CancelVelocity:
                    CancelVelocity(gravity);

                    break;
                case MissileState.HoldPosition:
                    HoldPosition(gravity, gravityNormal);
                    break;
                // case MissileState.LoiterAroundTarget:
                //     InterceptSurroundPos(Target);
                //     break;
                // case MissileState.LoiterAroundHost:
                //     InterceptSurroundPos(Host);
                //     break;
            }
        }

        

        public void ForceDetonate()
        {
            if (!HasLaunched || _frame < _minFramesAliveToDetonate)
            {
                var kaboom2 = this._kaboom;
                for (var i = 0; i < kaboom2.Length; i++)
                {
                    var item = kaboom2[i];
                    item.IsArmed = false;
                }
                return;
            }
            var kaboom = this._kaboom;
            for (var i = 0; i < kaboom.Length; i++)
            {
                var item = kaboom[i];
                item.IsArmed = true;
                item.Detonate();
            }
        }

        // TODO: Use me
        private bool CheckBlocks(IMyTerminalBlock[] blocks)
        {
            var length = blocks.Length;
            for (var i = 0; i < length; i++)
            {
                var block = blocks[i];
                if (block.Closed)
                {
                    ForceDetonate();
                    return false;
                }
            }

            return true;
        }

        private bool CheckBlocks(IMyThrust[] forwardThrusters, IMyBatteryBlock[] batteries, IMyGasTank[] gasTanks,
            IMyWarhead[] kaboom, IMyShipMergeBlock[] mergers, IMyShipConnector[] connectors, IMyGyro gyro)
        {
            var forwardThrusters2 = forwardThrusters; // hehe cache misses be like
            var length = forwardThrusters2.Length;
            for (var i = 0; i < length; i++)
                if (forwardThrusters2[i].Closed)
                {
                    ForceDetonate();
                    return false;
                }

            var batteries2 = batteries;
            length = batteries2.Length;
            for (var i = 0; i < length; i++)
                if (batteries2[i].Closed)
                {
                    ForceDetonate();
                    return false;
                }

            var gasTanks2 = gasTanks;
            length = gasTanks2.Length;
            for (var i = 0; i < length; i++)
                if (gasTanks2[i].Closed || gasTanks2[i].FilledRatio == 0)
                {
                    ForceDetonate();
                    return false;
                }

            var kaboom2 = kaboom;
            length = kaboom2.Length;
            for (var i = 0; i < length; i++)
                if (kaboom2[i].Closed)
                {
                    ForceDetonate();
                    return false;
                }

            var mergers2 = mergers;
            length = mergers2.Length;
            for (var i = 0; i < length; i++)
                if (mergers2[i].Closed)
                {
                    ForceDetonate();
                    return false;
                }

            var connectors2 = connectors;
            length = connectors2.Length;
            for (var i = 0; i < length; i++)
                if (connectors2[i].Closed)
                {
                    ForceDetonate();
                    return false;
                }

            var gyro2 = gyro;
            if (!gyro2.IsFunctional || gyro2.Closed)
            {
                ForceDetonate();
                return false;
            }

            return true;
        }

        private void InterceptSurroundPos(ArgusShip ship)
        {
            Vector3D target = ship.Position + SurroundDir * SurroundDist;
            STD_GUIDANCE(target, ship.Velocity, ship.Acceleration);
            var missileToTargetDistanceSquared = (target - _position).ALengthSquared();
            if (missileToTargetDistanceSquared < _hitSurroundPointDist * _hitSurroundPointDist) State = MissileState.CancelVelocity;
        }

        private void HoldPosition(Vector3D gravity, Vector3D gravityNormal)
        {
            
            Disarm();
            if (gravity == Vector3D.Zero)
            {
                DisableThrusters();
            }
            else
            {
                Gyro.GyroOverride = true;
                Rotate(-gravityNormal, _maxAngularVelocityRpm, false);
            }
            
            
        }

        private void DisableThrusters()
        {
            foreach (var thruster in _forwardThrusters) thruster.ThrustOverride = 0;
            Gyro.GyroOverride = false;
        }

        public int Launch()
        {
            if (MissilePropulsionType == MissilePropulsionType.Hydrogen)
            {

                bool fuelFail = false;
                foreach (var tank in GasTanks)
                {
                    if (tank.FilledRatio < _minFuelRatio)
                    {
                        tank.Stockpile = true;
                        fuelFail = true;
                    }
                    
                }
                if (fuelFail) return 1;
            }
            
            HasLaunched = true;
            foreach (var thruster in _forwardThrusters)
            {
                thruster.Enabled = true;
                thruster.ThrustOverridePercentage = 1;
            }

            foreach (var battery in _batteries) battery.Enabled = true;
            foreach (var merger in _mergers) merger.Enabled = false;
            foreach (var connector in _connectors)
            {
                connector.Disconnect();
                connector.Enabled = false;
            }

            foreach (var tank in GasTanks) tank.Stockpile = false;
            State = MissileState.MaintainLaunchTrajectory;
            return 0;
        }

        private Vector3D _savedTargetVelocity = Vector3D.Zero; // TODO: Assign me

        public void SetTargetVelocity(Vector3D targetVelocity)
        {
            //if (guidanceMode == MissileGuidanceMode.CancelVelocity) return;
            _savedTargetVelocity = targetVelocity;
        }

        public void AddWaypoint(Vector3D waypoint)
        {
            _waypoints.Add(waypoint);
        }

        private void CancelVelocity(Vector3D gravity)
        {
            

            var missileVelocityNormalized = Vector3D.Normalize(_velocity);


            Rotate(-missileVelocityNormalized, _maxAngularVelocityRpm, true);

            double thrustDot = Math.Pow(missileVelocityNormalized.Dot(_forwardThrusters[0].WorldMatrix.Forward), _serverMaxSpeed);
            
            var thrust = thrustDot == 0 ? 0 : (float)(thrustDot * 10 * _accel);
            /*
            var thrust =
                MathHelper.Clamp(Math.Pow(MissileVelocityNormalized.Dot(forwardThrusters[0].WorldMatrix.Forward), _serverMaxSpeed),
                    0, 1) * MissileVelocity.ALength() / 10;
                    */

            var cross = _velocity.Cross(_forwardThrusters[0].WorldMatrix.Forward);
            foreach (var thruster in _forwardThrusters) thruster.ThrustOverridePercentage = thrust;


            if (_velocity.ALengthSquared() < 0.5 * 0.5)
            {
                DisableThrusters();
                State = MissileState.HoldPosition;
            }
        }


        private void STD_GUIDANCE(Vector3D targetPosition, Vector3D targetVelocity, Vector3D targetAcceleration)
        {
            // Sorts CurrentVelocities

            // Uses RdavNav Navigation APN Guidance System
            //-----------------------------------------------

            // Setup LOS rates and PN system
            var losOld = Vector3D.Normalize((targetPosition - targetVelocity) - _previousPosition);
            var losNew = Vector3D.Normalize(targetPosition - _position);

            // And Assigners
            var am = new Vector3D(1, 0, 0);
            double losRate;
            Vector3D losDelta;
            var missileForwards = _forwardThrusters[0].WorldMatrix.Backward;

            // Vector/Rotation Rates
            if (losOld.ALengthSquared() == 0)
            {
                losDelta = new Vector3D(0, 0, 0);
                losRate = 0.0;
            }
            else
            {
                losDelta = losNew - losOld;
                losRate = Math.Sqrt(losDelta.ALengthSquared()) / _timeStep;
            }

            //-----------------------------------------------

            // Closing Velocity
            var vclosing = Math.Sqrt((targetVelocity - _velocity).ALengthSquared());
            var v = _velocity.ALength();
            // If Under Gravity Use Gravitational Accel
            var gravityComp = -Host.Controller.GetNaturalGravity();
            // Calculate the final lateral acceleration
            var lateralDirection = Vector3D.Cross(Vector3D.Cross(targetVelocity - _velocity, losNew),
                targetVelocity - _velocity);
            var lateralDirectionLengthSquared = lateralDirection.ALengthSquared();
            if (lateralDirectionLengthSquared != 0) lateralDirection /= Math.Sqrt(lateralDirectionLengthSquared);

            var lateralAccelerationComponent =
                lateralDirection * _pnGain * losRate * vclosing + losDelta * 9.8 * (0.5 * _pnGain);

            // If Impossible Solution (ie maxes turn rate) Use Drift Cancelling For Minimum T
            var oversteerReqt = Math.Sqrt(lateralAccelerationComponent.ALengthSquared()) / _accel;
            if (oversteerReqt > 0.98)
                lateralAccelerationComponent = _accel * Vector3D.Normalize(lateralAccelerationComponent +
                                                                          oversteerReqt *
                                                                          Vector3D.Normalize(-_velocity) * 40);

            // Calculates And Applies thrust In Correct Direction (Performs own inequality check)
            var thrustPower = Math.Pow(Vector3D.Dot(missileForwards, Vector3D.Normalize(lateralAccelerationComponent)),
                4); // TESTTESTTEST
            thrustPower =
                MathHelper.Clamp(thrustPower, MathHelper.Clamp(_serverMaxSpeed - v, 0, 1),
                    1); // for improved thrust performance on the get-go
            thrustPower = MathHelper.RoundToInt(thrustPower * 10) / 10d;

            for (var i = 0; i < _forwardThrusters.Length; i++)
            {
                var thruster = _forwardThrusters[i];
                if (thruster.ThrustOverridePercentage != (float)thrustPower)
                    thruster.ThrustOverridePercentage = (float)thrustPower;
            }

            // Calculates Remaining Force Component And Adds Along LOS
            var lateralAccelerationComponentLengthSquared = lateralAccelerationComponent.ALengthSquared();
            var rejectedAccel = Math.Sqrt(_accel * _accel - lateralAccelerationComponentLengthSquared);
            if (double.IsNaN(rejectedAccel)) rejectedAccel = 0;
            lateralAccelerationComponent += losNew * rejectedAccel;

            //-----------------------------------------------

            // Guides To Target Using Gyros
            am = Vector3D.Normalize(lateralAccelerationComponent + gravityComp);
            
            Rotate(am, _maxAngularVelocityRpm, false);
        }

        private void TryDetonate(double missileToTargetDistanceSquared)
        {
            if (_frame < _minFramesAliveToDetonate) return;
            if (missileToTargetDistanceSquared < 50 * 50) //Arms
            {
                foreach (var item in _kaboom) item.IsArmed = true;
                if (_kaboom.Length <= _detIndex) return;
                if (missileToTargetDistanceSquared < 15 * 15) //A mighty earth shattering kaboom
                {
                    _kaboom[_detIndex].Detonate();
                    _detIndex++;
                }
            }
        }

        private void TryTargetNextWaypoint(double missileToTargetDistanceSquared)
        {
            if (missileToTargetDistanceSquared < 10 * 10)
            {
                if (_waypoints.Count == 0) return;
                _waypoints.RemoveAt(0);
            }
        }

        private void Disarm()
        {
            for (var i = 0; i < _kaboom.Length; i++)
            {
                var item = _kaboom[i];
                item.IsArmed = false;
            }
        }

        private void Rotate(Vector3D desiredGlobalFwdNormalized, double maxAngularVelocityRpm, bool precision)
        {
            // Check if desiredGlobalFwdNormalized is nan
            if (double.IsNaN(desiredGlobalFwdNormalized.X) || double.IsNaN(desiredGlobalFwdNormalized.Y) ||
                double.IsNaN(desiredGlobalFwdNormalized.Z)) return;

            double gp;
            double gy;

            //Rotate Toward forward
            var referenceThruster = this._referenceThruster;
            var referenceThrusterWorldMatrix = referenceThruster.WorldMatrix;

            var waxis = Vector3D.Cross(referenceThrusterWorldMatrix.Backward, desiredGlobalFwdNormalized);
            var axis = Vector3D.TransformNormal(waxis, MatrixD.Transpose(referenceThrusterWorldMatrix));
            var x = _pitch.Control(-axis.X);
            var y = _yaw.Control(-axis.Y);
            
            gp = MathHelper.Clamp(x, -maxAngularVelocityRpm, maxAngularVelocityRpm);
            gy = MathHelper.Clamp(y, -maxAngularVelocityRpm, maxAngularVelocityRpm);

            if (Math.Abs(gy) + Math.Abs(gp) > maxAngularVelocityRpm)
            {
                var adjust = maxAngularVelocityRpm / (Math.Abs(gy) + Math.Abs(gp));
                gy *= adjust;
                gp *= adjust;
            }


            ApplyGyroOverride(gp, gy, Gyro, referenceThruster.WorldMatrix, precision);
        }

        private void ApplyGyroOverride(double pitchSpeed, double yawSpeed, IMyGyro gyro, MatrixD worldMatrix, bool precision)
        {
            if (!precision)
            {
                pitchSpeed = MathHelper.RoundToInt(pitchSpeed * 10) / 10d;
                yawSpeed = MathHelper.RoundToInt(yawSpeed * 10) / 10d;
            }
            

            if (pitchSpeed == _previousPitchSpeed && yawSpeed == _previousYawSpeed) return;
            _previousPitchSpeed = pitchSpeed;
            _previousYawSpeed = yawSpeed;

            var rotationVec = new Vector3D(pitchSpeed, yawSpeed, 0);
            var relativeRotationVec = Vector3D.TransformNormal(rotationVec, worldMatrix);


            if (gyro.IsFunctional && gyro.IsWorking && gyro.Enabled && !gyro.Closed)
            {
                var transformedRotationVec =
                    Vector3D.TransformNormal(relativeRotationVec, MatrixD.Transpose(gyro.WorldMatrix));
                if (precision)
                {
                    gyro.Pitch = (float)transformedRotationVec.X;
                    _gyroPreviousPitch = transformedRotationVec.X;
                    gyro.Yaw = (float)transformedRotationVec.Y;
                    _gyroPreviousYaw = transformedRotationVec.Y;
                    gyro.Roll = (float)transformedRotationVec.Z;
                    _gyroPreviousRoll = transformedRotationVec.Z;
                    return;
                }
                
                if (Math.Abs(transformedRotationVec.X - _gyroPreviousPitch) > 0.05)
                {
                    gyro.Pitch = (float)transformedRotationVec.X;
                    _gyroPreviousPitch = transformedRotationVec.X;
                    //ALDebug.program.Echo("Pitching");
                }

                if (Math.Abs(transformedRotationVec.Y - _gyroPreviousYaw) > 0.05)
                {
                    gyro.Yaw = (float)transformedRotationVec.Y;
                    _gyroPreviousYaw = transformedRotationVec.Y;
                    //ALDebug.program.Echo("Yawing");
                }

                if (Math.Abs(transformedRotationVec.Z - _gyroPreviousRoll) > 0.05)
                {
                    gyro.Roll = (float)transformedRotationVec.Z;
                    _gyroPreviousRoll = transformedRotationVec.Z;
                    //ALDebug.program.Echo("Rolling");
                }

                gyro.GyroOverride = true;
            }
        }



        public Vector3D GetPosition()
        {
            return Gyro.CubeGrid.WorldVolume.Center;
        }

        public Vector3D GetForward()
        {
            return _forwardThrusters[0].WorldMatrix.Backward;
        }

        public KratosMissileManager.MissileData GetData()
        {
            // *10 because LCD updates at 6hz as opposed to 60hz of literally everything else
            switch (MissilePropulsionType)
            {
                case MissilePropulsionType.Ion:
                    return new KratosMissileManager.MissileData(_position, _previousPosition, _distanceToTarget,
                        _forwardThrusters[0].WorldMatrix.Backward,
                        _batteries[0].CurrentStoredPower / _batteries[0].MaxStoredPower, Gyro.EntityId, HasLaunched,
                        false);
                case MissilePropulsionType.Hydrogen:
                    return new KratosMissileManager.MissileData(_position, _previousPosition, _distanceToTarget,
                        _forwardThrusters[0].WorldMatrix.Backward, Math.Min(1, GasTanks[0].FilledRatio / _fuelRatio), Gyro.EntityId, HasLaunched,
                        true);
                case MissilePropulsionType.Atmospheric:
                    return new KratosMissileManager.MissileData(_position, _previousPosition, _distanceToTarget,
                        _forwardThrusters[0].WorldMatrix.Backward,
                        _batteries[0].CurrentStoredPower / _batteries[0].MaxStoredPower, Gyro.EntityId, HasLaunched,
                        false);
            }

            return new KratosMissileManager.MissileData(_position, _previousPosition, _distanceToTarget,
                _forwardThrusters[0].WorldMatrix.Backward, _batteries[0].CurrentStoredPower / _batteries[0].MaxStoredPower,
                Gyro.EntityId, HasLaunched, false);
        }

        public Vector3D GetGyroPosition()
        {
            return Gyro.GetPosition();
        }

        public Vector3D GetThrusterPosition()
        {
            return _forwardThrusters[0].GetPosition();
        }

        public bool IsComplete()
        {
            for (int i = 0; i < _blocks.Length; i++)
            {
                var block = _blocks[i];
                if (!block.IsFunctional) return false;
            }

            if (!Gyro.IsFunctional) return false;
            return true;
        }

        public void LoiterAroundHost()
        {
            Vector3D targetPosition = Host.Position + SurroundDir * SurroundDist;
            UpdateMotion(targetPosition);
            if 
            (
                State == MissileState.InterceptTarget 
                //|| 
                //State == MissileState.LoiterAroundTarget
                || 
                (
                    (
                        State == MissileState.HoldPosition 
                        || 
                        State == MissileState.CancelVelocity
                    )
                    && 
                    (targetPosition - _position).LengthSquared() > _maxDistanceFromLoiterPosition * _maxDistanceFromLoiterPosition
                )
                || 
                (
                    State == MissileState.InterceptWaypoint 
                    && _waypoints.Count == 0
                )
            )
            {
                //State = MissileState.LoiterAroundHost;
            }

        }
        public void LoiterAroundTarget()
        {
            Vector3D targetPosition = Target.Position + SurroundDir * SurroundDist;
            UpdateMotion(targetPosition);
            if 
            (
                State == MissileState.InterceptTarget 
                || 
                //State == MissileState.LoiterAroundHost
                //|| 
                (
                    (
                        State == MissileState.HoldPosition 
                        || 
                        State == MissileState.CancelVelocity
                    )
                    &&
                    (targetPosition - _position).LengthSquared() > _maxDistanceFromLoiterPosition * _maxDistanceFromLoiterPosition
                )
                || 
                (
                    State == MissileState.InterceptWaypoint 
                    && _waypoints.Count == 0
                )
            )
            {
                //State = MissileState.LoiterAroundTarget;
            }
        }

        public void InterceptTarget()
        {
            Vector3D target = Target.GetBlockPositionOfCategoryAtIndex(_targetedBlockCategory,
                TargetedCategoryBlockIndices[Target][_targetedBlockCategory]);
            UpdateMotion(target);
            if 
            (
                State != MissileState.InterceptTarget 
                && 
                State != MissileState.StandingByForLaunch 
                &&
                State != MissileState.MaintainLaunchTrajectory
            )
            {
                State = MissileState.InterceptTarget;
            } 
        }


    }
}
