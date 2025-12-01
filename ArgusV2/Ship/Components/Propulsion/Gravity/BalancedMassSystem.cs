using System;
using System.Collections.Generic;
using IngameScript.Ship.Components.Propulsion.Gravity.Wrapper;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRageMath;

namespace IngameScript.Ship.Components.Propulsion.Gravity
{
    public class BalancedMassSystem
    {
        private readonly List<BlockMass> _blocks;
        private readonly List<BallMass> _balls;
        private int _updateJitter = 0;
        
        private Vector3D _currentMoment = Vector3D.Zero; // running moment

        private ControllableShip _ship;

        public BalancedMassSystem(List<IMyTerminalBlock> blocks, ControllableShip ship)
        {
            
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
                    _currentMoment += wrap.Moment;
                    continue;
                }

                var mass = block as IMyArtificialMassBlock;
                if (mass != null)
                {
                    var wrap = new BlockMass(mass, this, _ship);
                    _blocks.Add(wrap);
                    _currentMoment += wrap.Moment;
                }

            }

            _updateJitter = Program.RNG.Next() % (Math.Max(Config.GdriveArtificialMassBalanceFrequencyFrames, Config.GdriveSpaceBallBalanceFrequencyFrames) - 1);
            CalculateTotalMass();

        }
        
        public bool Enabled { get; set; }
        public double TotalMass { get; private set; }

        public void EarlyUpdate(int frame)
        {
            // TODO: Balancer logic
            if ((frame + _updateJitter) % Config.GdriveArtificialMassBalanceFrequencyFrames == 0)
                BalanceMassBlocks();
            if ((frame + _updateJitter) % Config.GdriveSpaceBallBalanceFrequencyFrames == 0)
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
            foreach (var block in _blocks)
            {
                // Example: check if toggling reduces moment
                Vector3D momentBefore = _currentMoment;
                Vector3D momentToggle = block.BalancerAllowed ? Vector3D.Zero : block.Moment;

                if ((_currentMoment - (block.BalancerAllowed ? block.Moment : Vector3D.Zero) + momentToggle).LengthSquared() <
                    _currentMoment.LengthSquared())
                {
                    block.BalancerAllowed = !block.BalancerAllowed;
                    _currentMoment = _currentMoment - (block.BalancerAllowed ? Vector3D.Zero : block.Moment) + momentToggle;
                }
            }
        }
        private void BalanceSpaceBalls()
        {
            
        }


    }
}