using System;
using System.Collections.Generic;
using IngameScript.Ship.Components.Propulsion.Gravity.Wrapper;
using IngameScript.TruncationWrappers;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRageMath;

namespace IngameScript.Ship.Components.Propulsion.Gravity
{
    public class BalancedMassSystem
    {
        private readonly List<BlockMass> _blocks;
        private readonly List<BallMass> _balls;
        private readonly List<Mass> _allBlocks;
        private bool _stateChanged;
        private int _updateJitter = 0;
        
        private AT_Vector3D _currentMoment = AT_Vector3D.Zero; // running moment

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
                    _currentMoment += wrap.Moment;
                    continue;
                }

                var mass = block as IMyArtificialMassBlock;
                if (mass != null)
                {
                    var wrap = new BlockMass(mass, this, _ship);
                    _blocks.Add(wrap);
                    _allBlocks.Add(wrap);
                    _currentMoment += wrap.Moment;
                }

            }

            _updateJitter = Program.RNG.Next() % (Math.Max(Config.Gdrive.MassBalanceFrequencyFrames, Config.Gdrive.BallBalanceFrequencyFrames) - 1);
            CalculateTotalMass();

        }
        
        public bool Enabled { get; set; }
        public double TotalMass { get; private set; }

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
        public bool HasStateChanged()
        {
            var state = _stateChanged;
            _stateChanged = false;
            return state;
        }

        private void CalculateTotalMass()
        {
            TotalMass = 0;

            foreach (var block in _blocks)
            {
                TotalMass += block.BalancerVirtualMass;
            }

            foreach (var ball in _balls)
            {
                TotalMass += ball.BalancerVirtualMass;
            }
        }
        
        private void BalanceMassBlocks()
        {
            // _currentMoment persists across frames
            // Each iteration nudges blocks on/off
            AT_Vector3D momentBefore = _currentMoment;
            foreach (var block in _blocks)
            {
                AT_Vector3D momentToggle = block.BalancerAllowed ? AT_Vector3D.Zero : block.Moment;

                if ((_currentMoment - (block.BalancerAllowed ? block.Moment : AT_Vector3D.Zero) + momentToggle).LengthSquared() <
                    _currentMoment.LengthSquared())
                {
                    block.BalancerAllowed = !block.BalancerAllowed;
                    _currentMoment = _currentMoment - (block.BalancerAllowed ? AT_Vector3D.Zero : block.Moment) + momentToggle;
                }
            }
            _stateChanged |= momentBefore != _currentMoment;
        }
        private void BalanceSpaceBalls()
        {
            AT_Vector3D momentBefore = _currentMoment;
                // TODO: Space ball balance logic
            
            
            _stateChanged |= momentBefore != _currentMoment;
        }


    }
}