using System;
using System.Collections.Generic;
using DirectShowLib;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRageMath;

namespace IngameScript
{
    internal class GravityDriveManager
    {
        // This is calculated out in relation to how many mass blocks there are
        // The threshold will be higher if there are more mass blocks
        // The threshold will be lower if there are fewer mass blocks


        private bool Dampeners;
        private double forwardBackwardForce;
        private readonly List<LinearGravityGenerator> forwardBackwardGravity;
        private readonly List<SphericalGravityGenerator> forwardBackwardSphericalGravity;

        private int frame;
        private int framesIdle;

        private bool gravityDriveDisabled = true;
        private readonly int gravityDriveDisableDelay = 600;

        private readonly List<IMyGravityGenerator> gravityGeneratorReferenceList;
        private readonly List<IMyGravityGeneratorSphere> gravityGeneratorSphereReferenceList;

        private readonly IMyGridTerminalSystem GridTerminalSystem;
        private int interruptedFor;
        private double leftRightForce;
        private readonly List<LinearGravityGenerator> leftRightGravity;

        private readonly int massBlockBalanceDelay = 301; // 5 seconds


        private readonly double
            massBlockMinimumMomentPercentageToDisable =
                1d / 1000; // Change must be greater than this percentage to disable a mass block

        private readonly List<MassBlock> massBlocks;
        private readonly List<IMyArtificialMassBlock> massBlocksReferenceList;

        private readonly float passiveSphericalRadius;

        private bool Precision;
        private bool PreviousDampeners;


        private Vector3D PreviousMoveIndicator = Vector3D.Zero;
        private bool PreviousPrecision;

        private GravityDriveManagerState
            previousState =
                GravityDriveManagerState.ActiveMove; // Just so that it gets to a deterministic state on the first run

        private Vector3D PreviousTransformedVelocity = Vector3D.Zero;

        private readonly MyGridProgram program;
        private readonly float repulseSphericalRadius;


        private IMyShipController ShipController;
        private double shipMass;
        private readonly int spaceBallBalanceDelay = 1201; // 20 seconds
        private readonly int spaceBallBalanceOffset = 2000000;
        private readonly List<IMySpaceBall> spaceBallReferenceList;

        private readonly List<SpaceBall> spaceBalls;
        private GravityDriveManagerState state = GravityDriveManagerState.Idle;
        private double upDownForce;

        private readonly List<LinearGravityGenerator> upDownGravity;

        public GravityDriveManager(List<IMyArtificialMassBlock> massBlocks, List<IMySpaceBall> spaceBalls,
            IMyGridTerminalSystem GridTerminalSystem, MyGridProgram program,
            List<IMyGravityGenerator> gravityGenerators, List<IMyGravityGeneratorSphere> sphericalGravityGenerators,
            IMyShipController shipController, float passiveSphericalRadius, float repulseSphericalRadius)
        {
            ShipController = shipController;
            this.massBlocks = new List<MassBlock>();
            massBlocksReferenceList = massBlocks;
            foreach (var block in massBlocks) this.massBlocks.Add(new MassBlock(block));
            this.spaceBalls = new List<SpaceBall>();
            spaceBallReferenceList = spaceBalls;
            foreach (var ball in spaceBalls) this.spaceBalls.Add(new SpaceBall(ball));
            this.GridTerminalSystem = GridTerminalSystem;
            this.program = program;
            gravityGeneratorReferenceList = gravityGenerators;
            gravityGeneratorSphereReferenceList = sphericalGravityGenerators;
            this.passiveSphericalRadius = passiveSphericalRadius;
            this.repulseSphericalRadius = repulseSphericalRadius;

            forwardBackwardSphericalGravity = new List<SphericalGravityGenerator>();
            var forwardReferenceForward = shipController.WorldMatrix.Forward;
            var centerOfMass = shipController.CenterOfMass;
            foreach (var gravityGenerator in sphericalGravityGenerators)
            {
                var pos = gravityGenerator.WorldMatrix.Translation;
                var centerOfMassToBlock = centerOfMass - pos;
                if (Vector3D.Dot(centerOfMassToBlock, forwardReferenceForward) > 0)
                    forwardBackwardSphericalGravity.Add(new SphericalGravityGenerator(gravityGenerator, -1, "Rear"));
                else
                    forwardBackwardSphericalGravity.Add(new SphericalGravityGenerator(gravityGenerator, 1, "Forward"));
            }

            upDownGravity = new List<LinearGravityGenerator>();
            leftRightGravity = new List<LinearGravityGenerator>();
            forwardBackwardGravity = new List<LinearGravityGenerator>();
            foreach (var generator in gravityGenerators)
                if (generator.WorldMatrix.Up == -shipController.WorldMatrix.Forward)
                    forwardBackwardGravity.Add(new LinearGravityGenerator(generator, 1, "Forward"));
                else if (generator.WorldMatrix.Up == shipController.WorldMatrix.Forward)
                    forwardBackwardGravity.Add(new LinearGravityGenerator(generator, -1, "Backward"));
                else if (generator.WorldMatrix.Up == -shipController.WorldMatrix.Left)
                    leftRightGravity.Add(new LinearGravityGenerator(generator, 1, "Left"));
                else if (generator.WorldMatrix.Up == shipController.WorldMatrix.Left)
                    leftRightGravity.Add(new LinearGravityGenerator(generator, -1, "Right"));
                else if (generator.WorldMatrix.Up == -shipController.WorldMatrix.Up)
                    upDownGravity.Add(new LinearGravityGenerator(generator, 1, "Up"));
                else if (generator.WorldMatrix.Up == shipController.WorldMatrix.Up)
                    upDownGravity.Add(new LinearGravityGenerator(generator, -1, "Down"));
            InitializeMassValues();
            forwardBackwardForce = CalculateForceFromGravityGenerators(forwardBackwardGravity,
                forwardBackwardSphericalGravity, shipController.WorldMatrix.Forward);
            leftRightForce = CalculateForceFromGravityGenerators(leftRightGravity);
            upDownForce = CalculateForceFromGravityGenerators(upDownGravity);
            var shipMass = shipController.CalculateShipMass();
            this.shipMass = shipMass.PhysicalMass;

            CalculateMoments(centerOfMass, this.massBlocks);
            CalculateMoments(centerOfMass, this.spaceBalls);
        }

        public void Interrupt(int frames)
        {
            if (state == GravityDriveManagerState.Repulse) return;
            SetGravityForce(upDownGravity, 0);
            SetGravityForce(leftRightGravity, 0);
            SetGravityForce(forwardBackwardGravity, 0);
            SetGravityForce(forwardBackwardSphericalGravity, 0);
            interruptedFor = frames;
        }

        public void Update(IMyShipController ShipController, bool precision, bool doRepulse, bool doBalance, Vector3D autoDodgeVal)
        {
            
            this.ShipController = ShipController;
            Precision = precision;
            Dampeners = ShipController.DampenersOverride;

            if (Dampeners != PreviousDampeners) FlightDataRecorder.GravityDriveDampenersChangedEvent(Dampeners);
            if (Precision != PreviousPrecision) FlightDataRecorder.GravityDrivePrecisionChangedEvent(Precision);

            
            if (interruptedFor > 0)
            {
                interruptedFor--;
                return;
            }

            frame++;

            if (!gravityDriveDisabled && framesIdle > gravityDriveDisableDelay)
            {
                gravityDriveDisabled = true;
                EnableMassBlocks(false);
                EnableGenerators(false);
                FlightDataRecorder.GravityDriveDisabledEvent();
            }
            
            float baselineGravity = 9.81f;
            var naturalGravity = ShipController.GetNaturalGravity();
            var naturalGravityLength = naturalGravity.Length();
            
            
            var driveEffectiveness = MathHelper.Clamp(baselineGravity - (naturalGravityLength * 2), 0, baselineGravity) / baselineGravity;

            float driveMultiplier = 1;
            if (driveEffectiveness != 0)
            {
                driveMultiplier = (float)(1 / driveEffectiveness);
            }
            
            var velocity = ShipController.GetShipVelocities().LinearVelocity + (naturalGravity / 10);
            
            
            if (frame % 901 == 0) _shipMass = ShipController.CalculateShipMass();
            
            this.shipMass = _shipMass.PhysicalMass;
            velocity *= driveMultiplier;
            
            Vector3 transformedVelocity =
                Vector3D.TransformNormal(velocity, MatrixD.Transpose(ShipController.WorldMatrix));

            
            
            var transformedVelocityNormalized = transformedVelocity;

            if (transformedVelocityNormalized.LengthSquared() > 3)
            {
                transformedVelocityNormalized.X =
                    MathHelper.Clamp(MathHelper.RoundOn2(transformedVelocityNormalized.X), -1, 1);
                transformedVelocityNormalized.Y =
                    MathHelper.Clamp(MathHelper.RoundOn2(transformedVelocityNormalized.Y), -1, 1);
                transformedVelocityNormalized.Z =
                    MathHelper.Clamp(MathHelper.RoundOn2(transformedVelocityNormalized.Z), -1, 1);
            }

            var velocityLengthSquared = velocity.ALengthSquared();

            Vector3D moveIndicator = ShipController.MoveIndicator;
            moveIndicator.X *= -1;
            moveIndicator.Z *= -1;
            

            
            if (autoDodgeVal.LengthSquared() != 0)
            {
                var transformedAutoDodgeVal = Vector3D.TransformNormal(autoDodgeVal, MatrixD.Transpose(ShipController.WorldMatrix));
                transformedAutoDodgeVal.X *= -1;
                moveIndicator += transformedAutoDodgeVal;
            }
            
            var moveIndicatorLengthSquared = moveIndicator.ALengthSquared();

            moveIndicator *= driveMultiplier;   
            
            
            
            previousState = state;
            state = GravityDriveManagerState.Idle;

            // if (we are trying to move) or (we are moving and Dampeners are on)
            if (moveIndicatorLengthSquared > 0 || (velocityLengthSquared > 0.000001 && Dampeners))
                state = GravityDriveManagerState.ActiveMove;
            // if (we are repulsing)
            if (doRepulse) state = GravityDriveManagerState.Repulse;
            

            switch (state)
            {
                // If the gravity drive should be idle

                #region GravityDriveIdle

                case GravityDriveManagerState.Idle:

                    if (previousState != GravityDriveManagerState.Idle)
                    {
                        //EnableGenerators(false);
                        //EnableMassBlocks(false);
                        // I'll do something better here later, turning stuff on and off is expensive
                        SetGravityNoDampeners(Vector3D.Zero, 0);
                        PreviousMoveIndicator = Vector3D.Zero;
                        PreviousTransformedVelocity = Vector3D.Zero;
                    }

                    framesIdle++;
                    break;

                #endregion

                // If the gravity drive should be active

                #region GravityDriveActive

                case GravityDriveManagerState.ActiveMove:

                    if (previousState != GravityDriveManagerState.ActiveMove)
                    {
                        if (!gravityDriveDisabled) break;
                        gravityDriveDisabled = false;
                        framesIdle = 0;
                        EnableMassBlocks(true);
                        EnableGenerators(true);
                        FlightDataRecorder.GravityDriveEnabledEvent();
                    }


                    var moveType = MoveType.NoDampenersNoPrecision;
                    // Incorperate Dampeners and precision mode
                    moveType = moveType + (Precision ? 2 : 0);
                    moveType = moveType + (Dampeners ? 1 : 0);
                    Move(moveType, moveIndicator, transformedVelocity, transformedVelocityNormalized);
                    break;

                #endregion

                // If the mass blocks should be off and the spherical gravity generators should be on at full force

                #region GravityDriveRepulse

                case GravityDriveManagerState.Repulse:
                    if (previousState != GravityDriveManagerState.Repulse)
                    {
                        FlightDataRecorder.GravityDriveRepulseEnabledEvent();
                        Repulse();
                    }

                    break;

                #endregion
            }

            
            // obnoxious edge case
            if (previousState == GravityDriveManagerState.Repulse && state != GravityDriveManagerState.Repulse)
            {
                FlightDataRecorder.GravityDriveRepulseDisabledEvent();
                for (var i = 0; i < forwardBackwardSphericalGravity.Count; i++)
                {
                    var spherical = forwardBackwardSphericalGravity[i];
                    spherical.SetRadius(passiveSphericalRadius);
                    EnableMassBlocks(true); // This should be fine
                }
            }
            
            if (!doBalance) return;
            if (frame % massBlockBalanceDelay == 0)
            {
                
                var centerOfMass = ShipController.CenterOfMass;
                CalculateMoments(centerOfMass, massBlocks);
                
                EnableMassBlockPairs();
                BalanceMassBlocks(massBlocks, spaceBalls, centerOfMass);

                //Temp
                UpdateMassState(state, massBlocks);
                forwardBackwardForce = CalculateForceFromGravityGenerators(forwardBackwardGravity,
                    forwardBackwardSphericalGravity, ShipController.WorldMatrix.Forward);
                leftRightForce = CalculateForceFromGravityGenerators(leftRightGravity);
                upDownForce = CalculateForceFromGravityGenerators(upDownGravity);
                double previousMassError;
                
                
                double massError;

                var previousMassErrorVector = Vector3D.Zero;
                var massErrorVector = Vector3D.Zero;
                foreach (var massBlock in massBlocks)
                {
                    previousMassErrorVector += massBlock.previousMoment;
                    massErrorVector += massBlock.moment;
                }

                foreach (var spaceBall in spaceBalls)
                {
                    previousMassErrorVector += spaceBall.previousMoment;
                    massErrorVector += spaceBall.moment;
                }

                previousMassErrorVector /= massBlocks.Count + spaceBalls.Count;
                massErrorVector /= massBlocks.Count + spaceBalls.Count;
                previousMassError = previousMassErrorVector.ALength();
                massError = massErrorVector.ALength();

                FlightDataRecorder.GravityDriveMassBalanceEvent(MathHelper.RoundOn2((float)previousMassError / 1000),
                    MathHelper.RoundOn2((float)massError / 1000));
                
            }
            
            if ((frame + spaceBallBalanceOffset) % spaceBallBalanceDelay == 0)
            {
                var centerOfMass = ShipController.CenterOfMass;
                CalculateMoments(centerOfMass, spaceBalls);
                BalanceSpaceBalls(massBlocks, spaceBalls, centerOfMass);
                UpdateMassState(state, spaceBalls);
                forwardBackwardForce = CalculateForceFromGravityGenerators(forwardBackwardGravity,
                    forwardBackwardSphericalGravity, ShipController.WorldMatrix.Forward);
                leftRightForce = CalculateForceFromGravityGenerators(leftRightGravity);
                upDownForce = CalculateForceFromGravityGenerators(upDownGravity);
                double previousMassError;
                double massError;

                var previousMassErrorVector = Vector3D.Zero;
                var massErrorVector = Vector3D.Zero;
                foreach (var massBlock in massBlocks)
                {
                    previousMassErrorVector += massBlock.previousMoment;
                    massErrorVector += massBlock.moment;
                }

                foreach (var spaceBall in spaceBalls)
                {
                    previousMassErrorVector += spaceBall.previousMoment;
                    massErrorVector += spaceBall.moment;
                }

                previousMassErrorVector /= massBlocks.Count + spaceBalls.Count;
                massErrorVector /= massBlocks.Count + spaceBalls.Count;
                previousMassError = previousMassErrorVector.ALength();
                massError = massErrorVector.ALength();

                FlightDataRecorder.GravityDriveBallBalanceEvent(MathHelper.RoundOn2((float)previousMassError / 1000),
                    MathHelper.RoundOn2((float)massError / 1000));
            }

            PreviousDampeners = Dampeners;
            PreviousPrecision = Precision;
        }

        private void Move(MoveType moveType, Vector3D moveIndicator, Vector3 transformedVelocity,
            Vector3 transformedVelocityNormalized)
        {
            switch (moveType)
            {
                case MoveType.NoDampenersNoPrecision:
                    if (moveIndicator != PreviousMoveIndicator) SetGravityNoDampeners(moveIndicator, 1);
                    break;
                case MoveType.DampenersNoPrecision:
                    if (moveIndicator != PreviousMoveIndicator ||
                        transformedVelocityNormalized != PreviousTransformedVelocity)
                        SetGravityDampeners(moveIndicator, transformedVelocity, 1f);
                    break;
                case MoveType.NoDampenersPrecision:
                    if (moveIndicator != PreviousMoveIndicator) SetGravityNoDampeners(moveIndicator, 0.1f);
                    break;
                case MoveType.DampenersPrecision:
                    if (moveIndicator != PreviousMoveIndicator ||
                        transformedVelocityNormalized != PreviousTransformedVelocity)
                        SetGravityDampeners(moveIndicator, transformedVelocity, 0.1f);
                    break;
            }

            PreviousMoveIndicator = moveIndicator;
            PreviousTransformedVelocity = transformedVelocityNormalized;
        }

        private void SetGravityNoDampeners(Vector3D moveIndicator, float multiplier)
        {
            moveIndicator *= multiplier;
            SetGravityForce(upDownGravity, (float)moveIndicator.Y);
            SetGravityForce(leftRightGravity, (float)moveIndicator.X);
            SetGravityForce(forwardBackwardGravity, (float)moveIndicator.Z);
            SetGravityForce(forwardBackwardSphericalGravity, (float)moveIndicator.Z);
        }

        private void SetGravityDampeners(Vector3D moveIndicator, Vector3D transformedVelocity, float multiplier)
        {
            moveIndicator *= multiplier;
            if (moveIndicator.Y == 0)
            {
                var acceleration = shipMass / upDownForce;
                var dampenerForce = transformedVelocity.Y == 0 ? 0 : (float)(transformedVelocity.Y * 10 * acceleration);
                SetGravityForce(upDownGravity, -dampenerForce);
            }
            else
            {
                SetGravityForce(upDownGravity, (float)moveIndicator.Y);
            }

            if (moveIndicator.X == 0)
            {
                var acceleration = shipMass / leftRightForce;
                var dampenerForce = transformedVelocity.X == 0 ? 0 : (float)(transformedVelocity.X * 10 * acceleration);
                SetGravityForce(leftRightGravity, dampenerForce);
            }
            else
            {
                SetGravityForce(leftRightGravity, (float)moveIndicator.X);
            }

            if (moveIndicator.Z == 0)
            {
                var acceleration = shipMass / forwardBackwardForce;
                var dampenerForce = transformedVelocity.Z == 0 ? 0 : (float)(transformedVelocity.Z * 10 * acceleration);
                SetGravityForce(forwardBackwardGravity, dampenerForce);
                SetGravityForce(forwardBackwardSphericalGravity, dampenerForce);
            }
            else
            {
                SetGravityForce(forwardBackwardGravity, (float)moveIndicator.Z);
                SetGravityForce(forwardBackwardSphericalGravity,(float)moveIndicator.Z);
            }
        }

        private void Repulse()
        {
            EnableMassBlocks(false);
            foreach (var spherical in forwardBackwardSphericalGravity)
            {
                spherical.actualGravityGenerator.Enabled = true;
                spherical.SetRadius(repulseSphericalRadius);
                spherical.SetGravityOverride(-9.81f);
            }

            SetGravityForce(upDownGravity, 0);
            SetGravityForce(leftRightGravity, 0);
            SetGravityForce(forwardBackwardGravity, 0);
        }


        private void InitializeMassValues()
        {
            foreach (var ball in spaceBalls) ball.block.VirtualMass = 20000;
        }


        private void EnableGenerators(bool state)
        {
            SetGravityGeneratorsEnabled(upDownGravity, state);
            SetGravityGeneratorsEnabled(leftRightGravity, state);
            SetGravityGeneratorsEnabled(forwardBackwardGravity, state);
            SetGravityGeneratorsEnabled(forwardBackwardSphericalGravity, state);
        }


        private void CalculateMoments(Vector3D centerOfMass, List<MassBlock> massBlocks, List<SpaceBall> spaceBalls)
        {
            foreach (var massBlock in massBlocks) massBlock.CalculateMoment(centerOfMass);
            foreach (var spaceBall in spaceBalls) spaceBall.CalculateMoment(centerOfMass);
        }

        private void CalculateMoments(Vector3D centerOfMass, List<SpaceBall> spaceBalls)
        {
            foreach (var spaceBall in spaceBalls) spaceBall.CalculateMoment(centerOfMass);
        }

        private void CalculateMoments(Vector3D centerOfMass, List<MassBlock> massBlocks)
        {
            for (var i = massBlocks.Count - 1; i >= 0; i--)
            {
                var massBlock = massBlocks[i];
                if (massBlock.block.Closed || !GridTerminalSystem.CanAccess(massBlock.block))
                {
                    massBlocks.RemoveAt(i);
                    massBlocksReferenceList.Remove(massBlock.block);
                }

                massBlock.CalculateMoment(centerOfMass);
            }
        }

        private void UpdateMassState(GravityDriveManagerState state, List<SpaceBall> spaceBalls)
        {
            if (state != GravityDriveManagerState.ActiveMove) return;
            foreach (var spaceBall in spaceBalls) spaceBall.block.Enabled = spaceBall.Enabled;
        }

        private void UpdateMassState(GravityDriveManagerState state, List<MassBlock> massBlocks)
        {
            if (state != GravityDriveManagerState.ActiveMove) return;
            foreach (var massBlock in massBlocks) massBlock.block.Enabled = massBlock.Enabled;
        }

        
        int i = 0, j = 1;
        private MyShipMass _shipMass;

        private void EnableMassBlockPairs()
        {
           
            var disabled = new List<MassBlock>();
            var disabledCount = disabled.Count;
                
            foreach (var mass in massBlocks)
                if (!mass.Enabled) disabled.Add(mass);
            if (disabledCount <= 1) return;
            for (int step = 0; step < 50000; step++)
            {
                if (j >= disabledCount)
                {
                    i++;
                    if (i >= disabledCount - 1)
                    {
                        break;
                    }
                    j = i+1;
                }
                
                var a = disabled[i];
                var b = disabled[j];

                var added = a.moment + b.moment;
                var lengthSquared = added.X * added.X + added.Y * added.Y + added.Z * added.Z;
                if (lengthSquared < 1) {
                    a.Enabled = b.Enabled = true;
                }
                j++;
            }

            if (i >= disabledCount || i + 1 >= disabledCount)
            {
                i = 0;
                j = 1;
            }
        }

        private void BalanceMassBlocks(List<MassBlock> blocks, List<SpaceBall> balls, Vector3D centerOfMass)
        {
            var currentMoment = Vector3D.Zero;

            var maximumPossibleMoment = Vector3D.Zero;
            
            // Calculate current total moment
            foreach (var block in blocks)
            {
                currentMoment += block.Enabled ? block.moment : Vector3D.Zero;
                maximumPossibleMoment += Vector3D.Abs(block.moment);
            }
            
            foreach (var ball in balls)
            {
                currentMoment += ball.moment;
                maximumPossibleMoment += Vector3D.Abs(ball.moment);
            }
            

            var maximumPossibleMomentLengthSquared = maximumPossibleMoment.ALengthSquared();
            // Balance the moments by toggling blocks with a preference for turning them on

            //Vector3D largestMomentChange = GetLargestChangingMoment(blocks);
            int count = 0;
            foreach (var block in blocks)
            {
                count++;
                
                if (currentMoment.ALengthSquared() > (long)50000 * 50000)
                {
                    var isOn = block.Enabled;

                    var momentNoChange = isOn ? block.moment : Vector3D.Zero;
                    var momentWithChange = isOn ? Vector3D.Zero : block.moment;


                    // Check if the difference between the current moment and the moment with the block toggled is greater than a certain percentage of the total moment
                    // var impact = (block.moment / maximumPossibleMoment).ALengthSquared();
                    // if (isOn && impact < massBlockMinimumMomentPercentageToDisable *
                    //     massBlockMinimumMomentPercentageToDisable) continue;

                    currentMoment = currentMoment - momentNoChange;


                    // If changing reduces the moment, toggle (multiplied for bias to turning on blocks)
                    if ((currentMoment + momentWithChange).ALengthSquared() * (isOn ? 1.03 : 0.97) <
                        (currentMoment + momentNoChange).ALengthSquared())
                    {
                        block.Enabled = !isOn;
                        currentMoment = currentMoment + momentWithChange;
                    }
                    else
                    {
                        currentMoment = currentMoment + momentNoChange;
                    }
                }
                else
                {
                    break; // If the total moment is balanced, exit the loop
                }
                
            }
                
        }

        //private Vector3D GetLargestChangingMoment(List<MassBlock> blocks)
        //{
        //    Vector3D largest = Vector3D.Zero;
        //    foreach (var block in blocks)
        //    {
        //        Vector3D momentChange = block.moment - block.previousMoment;
        //        if (momentChange.ALengthSquared() > largest.ALengthSquared())
        //        {
        //            largest = momentChange;
        //        }
        //    }
        //}

        private void BalanceSpaceBalls(List<MassBlock> blocks, List<SpaceBall> balls, Vector3D centerOfMass)
        {
            var totalMoment = Vector3D.Zero;

            // Calculate current total moment
            foreach (var block in blocks) totalMoment += block.Enabled ? block.moment : Vector3D.Zero;
            foreach (var ball in balls) totalMoment += ball.moment;
            foreach (var ball in spaceBalls)
                if (totalMoment.ALengthSquared() > 10 * 10)
                {
                    var currentMass = ball.block.VirtualMass;
                    var newMass = currentMass + 500;


                    float step = 5000;
                    // Adjust the mass to try to balance the moment
                    // Calculate the current and new moments
                    var currentMoment = ball.moment;
                    var downMomentChange = ball.momentDown;
                    var upMomentChange = ball.momentUp;


                    var noAdjustedMoment = totalMoment;
                    var downAdjustedMoment = totalMoment - currentMoment + downMomentChange;
                    var upAdjustedMoment = totalMoment - currentMoment + upMomentChange;

                    var downMomentLength = downAdjustedMoment.ALengthSquared() + 2 * 2;
                    var upMomentLength =
                        upAdjustedMoment.ALengthSquared() - 2 * 2; // slight bias towards increasing mass
                    var noAdjustedMomentLength = noAdjustedMoment.ALengthSquared();

                    var finalMoment = totalMoment;

                    var moveUpMul = upMomentChange.ALengthSquared() / currentMoment.ALengthSquared() * step;
                    var moveDownMul = downMomentChange.ALengthSquared() / currentMoment.ALengthSquared() * step;

                    ball.previousMoveUpMul = moveUpMul;
                    ball.previousMoveDownMul = moveDownMul;


                    if (downMomentLength < noAdjustedMomentLength && downMomentLength < upMomentLength)
                    {
                        newMass = Math.Max(currentMass - (float)moveDownMul * 0.99f, 0);
                        finalMoment = downAdjustedMoment;
                    }
                    else if (upMomentLength < noAdjustedMomentLength && upMomentLength < downMomentLength)
                    {
                        newMass = Math.Min(currentMass + (float)moveUpMul * 1.01f, 20000);
                        finalMoment = upAdjustedMoment;
                    }
                    // Assuming you want to keep the mass unchanged if momentNoChange is the smallest
                    else
                    {
                        newMass = currentMass;
                    }

                    ball.SetMass(newMass);
                    totalMoment = finalMoment;
                }
        }

        private void SetGravityGeneratorsEnabled(List<LinearGravityGenerator> gravityGeneratorList, bool enabled)
        {
            for (var i = 0; i < gravityGeneratorList.Count; i++)
            {
                var gravityGenerator = gravityGeneratorList[i];
                gravityGenerator.actualGravityGenerator.Enabled = enabled;
            }
        }

        private void SetGravityGeneratorsEnabled(List<SphericalGravityGenerator> gravityGeneratorList, bool enabled)
        {
            for (var i = 0; i < gravityGeneratorList.Count; i++)
            {
                var gravityGenerator = gravityGeneratorList[i];
                gravityGenerator.actualGravityGenerator.Enabled = enabled;
            }
        }

        private void SetGravityForce(List<LinearGravityGenerator> gravityGeneratorList, float gravity)
        {
            for (var i = 0; i < gravityGeneratorList.Count; i++)
            {
                var gravityGenerator = gravityGeneratorList[i];
                gravityGenerator.SetGravity(gravity);
            }
        }

        // I love overloading
        private void SetGravityForce(List<SphericalGravityGenerator> gravityGeneratorList, float gravity)
        {
            for (var i = 0; i < gravityGeneratorList.Count; i++)
            {
                var gravityGenerator = gravityGeneratorList[i];
                gravityGenerator.SetGravity(gravity);
            }
        }

        public void UpdateBlocksList(List<IMyArtificialMassBlock> mass, List<IMySpaceBall> balls,
            List<IMyGravityGenerator> linearGrav, List<IMyGravityGeneratorSphere> sphericalGrav,
            IMyShipController forwardReference)
        {
            foreach (var block in mass)
                if (!massBlocksReferenceList.Contains(block))
                {
                    massBlocksReferenceList.Add(block);
                    massBlocks.Add(new MassBlock(block));
                    block.Enabled = state == GravityDriveManagerState.ActiveMove ? true : false;
                }

            foreach (var block in balls)
                if (!spaceBallReferenceList.Contains(block))
                {
                    spaceBallReferenceList.Add(block);
                    spaceBalls.Add(new SpaceBall(block));
                    block.Enabled = state == GravityDriveManagerState.ActiveMove ? true : false;
                }

            foreach (var block in linearGrav)
                if (!gravityGeneratorReferenceList.Contains(block))
                {
                    gravityGeneratorReferenceList.Add(block);
                    if (block.WorldMatrix.Up == -ShipController.WorldMatrix.Forward)
                        forwardBackwardGravity.Add(new LinearGravityGenerator(block, 1, "Forward"));
                    else if (block.WorldMatrix.Up == ShipController.WorldMatrix.Forward)
                        forwardBackwardGravity.Add(new LinearGravityGenerator(block, -1, "Backward"));
                    else if (block.WorldMatrix.Up == -ShipController.WorldMatrix.Left)
                        leftRightGravity.Add(new LinearGravityGenerator(block, -1, "Left"));
                    else if (block.WorldMatrix.Up == ShipController.WorldMatrix.Left)
                        leftRightGravity.Add(new LinearGravityGenerator(block, 1, "Right"));
                    else if (block.WorldMatrix.Up == -ShipController.WorldMatrix.Up)
                        upDownGravity.Add(new LinearGravityGenerator(block, 1, "Up"));
                    else if (block.WorldMatrix.Up == ShipController.WorldMatrix.Up)
                        upDownGravity.Add(new LinearGravityGenerator(block, -1, "Down"));
                }

            // Clear out any gravity generators that are no longer in the list
            for (var i = forwardBackwardGravity.Count - 1; i >= 0; i--)
            {
                var gravityGenerator = forwardBackwardGravity[i];
                if (gravityGenerator.actualGravityGenerator.Closed ||
                    !program.GridTerminalSystem.CanAccess(gravityGenerator.actualGravityGenerator))
                {
                    forwardBackwardGravity.RemoveAt(i);
                    gravityGeneratorReferenceList.Remove(gravityGenerator.actualGravityGenerator);
                }
            }

            for (var i = leftRightGravity.Count - 1; i >= 0; i--)
            {
                var gravityGenerator = leftRightGravity[i];
                if (gravityGenerator.actualGravityGenerator.Closed ||
                    !program.GridTerminalSystem.CanAccess(gravityGenerator.actualGravityGenerator))
                {
                    leftRightGravity.RemoveAt(i);
                    gravityGeneratorReferenceList.Remove(gravityGenerator.actualGravityGenerator);
                }
            }

            for (var i = upDownGravity.Count - 1; i >= 0; i--)
            {
                var gravityGenerator = upDownGravity[i];
                if (gravityGenerator.actualGravityGenerator.Closed ||
                    !program.GridTerminalSystem.CanAccess(gravityGenerator.actualGravityGenerator))
                {
                    upDownGravity.RemoveAt(i);
                    gravityGeneratorReferenceList.Remove(gravityGenerator.actualGravityGenerator);
                }
            }

            for (var i = forwardBackwardSphericalGravity.Count - 1; i >= 0; i--)
            {
                var gravityGenerator = forwardBackwardSphericalGravity[i];
                if (gravityGenerator.actualGravityGenerator.Closed ||
                    !program.GridTerminalSystem.CanAccess(gravityGenerator.actualGravityGenerator))
                {
                    forwardBackwardSphericalGravity.RemoveAt(i);
                    gravityGeneratorSphereReferenceList.Remove(gravityGenerator.actualGravityGenerator);
                }
            }


            var forwardReferenceForward = forwardReference.WorldMatrix.Forward;
            var centerOfMass = forwardReference.CenterOfMass;
            foreach (var block in sphericalGrav)
                if (!gravityGeneratorSphereReferenceList.Contains(block))
                {
                    gravityGeneratorSphereReferenceList.Add(block);
                    var pos = block.WorldMatrix.Translation;
                    var centerOfMassToBlock = centerOfMass - pos;
                    if (Vector3D.Dot(centerOfMassToBlock, forwardReferenceForward) > 0)
                        forwardBackwardSphericalGravity.Add(new SphericalGravityGenerator(block, -1, "Rear"));
                    else
                        forwardBackwardSphericalGravity.Add(new SphericalGravityGenerator(block, 1, "Forward"));
                }
        }


        public void EnableMassBlocks(bool state)
        {
            for (var i = massBlocks.Count - 1; i >= 0; i--)
            {
                var massBlock = massBlocks[i];
                if (massBlock.block.Closed || !GridTerminalSystem.CanAccess(massBlock.block))
                {
                    massBlocks.RemoveAt(i);
                    massBlocksReferenceList.Remove(massBlock.block);
                    continue;
                }

                massBlock.block.Enabled = state ? massBlock.Enabled : false;
            }

            for (var i = spaceBalls.Count - 1; i >= 0; i--)
            {
                var spaceBall = spaceBalls[i];
                if (spaceBall.block.Closed || !GridTerminalSystem.CanAccess(spaceBall.block))
                {
                    spaceBalls.RemoveAt(i);
                    spaceBallReferenceList.Remove(spaceBall.block);
                    continue;
                }

                spaceBall.block.Enabled = state;
            }
        }


        private double CalculateForceFromGravityGenerators(List<LinearGravityGenerator> gravityGenerators)
        {
            // I love how elegant and simple this is
            var acceleration = 9.81 * gravityGenerators.Count;

            double mass = 0;
            foreach (var block in massBlocks) mass += block.Enabled ? block.block.VirtualMass : 0;
            foreach (var ball in spaceBalls) mass += ball.Enabled ? ball.block.VirtualMass : 0;
            return mass * acceleration;
        }

        private double CalculateForceFromGravityGenerators(List<LinearGravityGenerator> gravityGenerators,
            List<SphericalGravityGenerator> sphericals, Vector3D forward)
        {
            // I hate how complicated and ugly this is
            var force = CalculateForceFromGravityGenerators(gravityGenerators);


            var allSphericalNetForce = Vector3D.Zero;
            // Because sphericals have a centralized gravity field instead of everything within the radius going in one direction, we need to calculate the force between each spherical and each mass block
            foreach (var spherical in sphericals)
            {
                var sphericalPosition = spherical.actualGravityGenerator.GetPosition();
                var thisSphericalNetForce = Vector3D.Zero;
                foreach (var block in massBlocks)
                {
                    if (!block.Enabled) continue;
                    var direction = (sphericalPosition - block.block.GetPosition()).Normalized();
                    thisSphericalNetForce += direction * 50000 * 9.81 * spherical.sign;
                }

                foreach (var ball in spaceBalls)
                {
                    var direction = (sphericalPosition - ball.block.GetPosition()).Normalized();
                    thisSphericalNetForce += direction * ball.block.VirtualMass * 9.81 * spherical.sign;
                }

                allSphericalNetForce += thisSphericalNetForce;
            }

            var directionalForce = Vector3D.Dot(forward, allSphericalNetForce);

            return force + directionalForce;
        }


        private enum GravityDriveManagerState
        {
            Idle,
            ActiveMove,
            Repulse
        }

        private enum MoveType
        {
            NoDampenersNoPrecision,
            DampenersNoPrecision,
            NoDampenersPrecision,
            DampenersPrecision
        }
    }
}