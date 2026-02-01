using System;
using System.Collections.Generic;
using IngameScript.Ship.Components.Propulsion.Gravity.Wrapper;
using IngameScript.TruncationWrappers;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRageMath;

namespace IngameScript.Ship.Components.Propulsion.Gravity
{
    /// <summary>
    /// Item class.
    /// </summary>
    /// <summary>
    /// Item class.
    /// </summary>
    public class BalancedMassSystem
    {
        private readonly List<BlockMass> _blocks;
        private readonly List<BallMass> _balls;
        private readonly List<Mass> _allBlocks;
        private bool _stateChanged;
        private int _updateJitter = 0;
        
        private AT_Vector3D _currentMassMoment = AT_Vector3D.Zero; // running moment
        private AT_Vector3D _currentBallMoment = AT_Vector3D.Zero;

        private ControllableShip _ship;

        public BalancedMassSystem(List<IMyTerminalBlock> blocks, ControllableShip ship)
        {
            _allBlocks = new List<Mass>();
            _blocks = new List<BlockMass>();
            _balls = new List<BallMass>();
            _ship = ship;

            foreach (var block in blocks)
            {
                var ball = block as IMySpaceBall;
                if (ball != null)
                {
                    var wrap = new BallMass(ball, this, _ship);
                    _balls.Add(wrap);
                    _allBlocks.Add(wrap);
                    _currentBallMoment += wrap.Moment;
                    continue;
                }

                var mass = block as IMyArtificialMassBlock;
                if (mass != null)
                {
                    var wrap = new BlockMass(mass, this, _ship);
                    _blocks.Add(wrap);
                    _allBlocks.Add(wrap);
                    _currentMassMoment += wrap.Moment;
                }

            }

            _updateJitter = Program.RNG.Next() % (Math.Max(Config.Gdrive.MassBalanceFrequencyFrames, Config.Gdrive.BallBalanceFrequencyFrames) - 1);
            CalculateTotalMass();

        }
        
        public bool Enabled { get; set; }
        /// <summary>
        /// Gets or sets the TotalMass.
        /// </summary>
        /// <summary>
        /// Gets or sets the TotalMass.
        /// </summary>
        public double TotalMass { get; private set; }
        /// <summary>
        /// Gets or sets the TotalMassBlocks.
        /// </summary>
        /// <summary>
        /// Gets or sets the TotalMassBlocks.
        /// </summary>
        public double TotalMassBlocks { get; private set; }

        public List<Mass> AllBlocks => _allBlocks;

        public void EarlyUpdate(int frame)
        {
            if ((frame + _updateJitter) % Config.Gdrive.MassBalanceFrequencyFrames == 0)
                BalanceMassBlocks();
            if ((frame + _updateJitter) % Config.Gdrive.BallBalanceFrequencyFrames == 0)
                BalanceSpaceBalls();
        }



        public void LateUpdate(int frame)
        {

            bool stateChanged = false;
            foreach (var block in _blocks)
            {
                stateChanged |= block.UpdateState();
            }
            foreach (var ball in _balls)
            {
                stateChanged |= ball.UpdateState();
            }

            if (stateChanged)
            {
                CalculateTotalMass();
            }


            
            
        }
        /// <summary>
        /// Returns true if the mass state of the generator has changed since this function was last called.
        /// </summary>
        /// <returns></returns>
        /// <summary>
        /// HasStateChanged method.
        /// </summary>
        /// <returns>The result of the operation.</returns>
        /// <summary>
        /// HasStateChanged method.
        /// </summary>
        /// <returns>The result of the operation.</returns>
        public bool HasStateChanged()
        {
            var state = _stateChanged;
            _stateChanged = false;
            return state;
        }

        private void CalculateTotalMass()
        {
            TotalMass = 0;
            TotalMassBlocks = 0;

            foreach (var block in _blocks)
            {
                TotalMass += block.BalancerVirtualMass;
                TotalMassBlocks += block.BalancerVirtualMass;
            }

            foreach (var ball in _balls)
            {
                TotalMass += ball.BalancerVirtualMass;
            }
        }
        
        private void BalanceMassBlocks()
        {
            AT_Vector3D beforeMoment = _currentMassMoment;
            double beforeMass = TotalMass; // however you track total

            foreach (var block in _blocks)
            {
                double oldMass = beforeMass;
                double newMass = beforeMass +
                                 (block.BalancerAllowed ? -block.AbsoluteVirtualMass : block.AbsoluteVirtualMass);

                AT_Vector3D candidate =
                    _currentMassMoment
                    - (block.BalancerAllowed ? block.Moment : AT_Vector3D.Zero)
                    + (block.BalancerAllowed ? AT_Vector3D.Zero : block.Moment);

                if (AcceptChange(_currentMassMoment, candidate, oldMass, newMass, SignificantTorqueGainMass))
                {
                    block.BalancerAllowed = !block.BalancerAllowed;
                    _currentMassMoment = candidate;
                    beforeMass = newMass; // update after change
                }
            }

            _stateChanged |= beforeMoment != _currentMassMoment;
        }
        private void BalanceSpaceBalls()
        {
            _currentBallMoment = AT_Vector3D.Zero;
            foreach (var ball in _balls)
            {
                _currentBallMoment += ball.Moment;
            }
            
            
            
            // Total moment before any changes
            AT_Vector3D totalBefore = _currentMassMoment + _currentBallMoment;
            double totalMass = TotalMass; // total current mass (blocks + balls)
    
            // We want to reduce moment toward zero
            AT_Vector3D deficit = -totalBefore;

            foreach (var ball in _balls)
            {
                // Unit direction of this ball's moment contribution
                var dir = ball.Moment.Normalized();
        
                // Project deficit along this ball's direction (tuning constant, if too slow then decrease)
                double projection = deficit.Dot(dir) / 250;

                // Ignore tiny adjustments (0.001 prevents oscillation in theory)
                if (Math.Abs(projection) < 0.001)
                    continue;

                float oldMass = ball.BalancerVirtualMass;
                float newMass = (float)MathHelper.Clamp(oldMass + projection, 0f, 20000f);
                if (newMass > 0) newMass += (newMass - oldMass) * 0.1f;
                float applied = newMass - oldMass;
                // Ignore very tiny changes
                if (Math.Abs(applied) < 0.5)
                    continue;

                // Candidate total moment if this change were applied
                AT_Vector3D candidateMoment = _currentMassMoment + _currentBallMoment + dir * applied;

                // Accept change according to “prefer adding mass” logic
                if (AcceptChange(totalBefore, candidateMoment,
                        totalMass, totalMass + applied, SignificantTorqueGainBall))
                {
                    // Apply change
                    ball.BalancerVirtualMass = newMass;
                    _currentBallMoment += dir * applied;

                    // Update running total mass
                    totalMass += applied;

                    // Flag if any state changed
                    _stateChanged |= totalBefore != (_currentMassMoment + _currentBallMoment);
                }
            }
        }
        private const double SignificantTorqueGainMass = 1;
        private const double SignificantTorqueGainBall = 0;
        private bool AcceptChange(AT_Vector3D currentMoment, AT_Vector3D candidateMoment,
            double oldMass, double newMass, double significantTorqueGain)
        {
            double before = currentMoment.LengthSquared();
            double after = candidateMoment.LengthSquared();
            double improvement = before - after;
            double massDelta = newMass - oldMass;

            if (massDelta > 0)
                return improvement > 0; // adding mass? tiny win is enough
            else
                return improvement > significantTorqueGain; // losing mass? must be worth it
        }


    }
}