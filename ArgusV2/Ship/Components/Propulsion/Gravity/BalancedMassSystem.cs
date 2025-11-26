using System.Collections.Generic;
using IngameScript.Ship.Components.Propulsion.Gravity.Wrapper;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;

namespace IngameScript.Ship.Components.Propulsion.Gravity
{
    public class BalancedMassSystem
    {
        private readonly List<BlockMass> _blocks;
        private readonly List<BallMass> _balls;



        public BalancedMassSystem(List<IMyTerminalBlock> blocks)
        {
            _blocks = new List<BlockMass>();
            _balls = new List<BallMass>();

            foreach (var block in blocks)
            {
                var ball = block as IMySpaceBall;
                if (ball != null)
                {
                    _balls.Add(new BallMass(ball, this));
                    continue;
                }

                var mass = block as IMyArtificialMassBlock;
                if (mass != null)
                {
                    _blocks.Add(new BlockMass(mass, this));
                }

            }
        }
        
        public bool Enabled { get; set; }

        public void EarlyUpdate(int _frame)
        {
            // TODO: Balancer logic
        }

        public void LateUpdate(int _frame)
        {
            foreach (var block in _blocks)
            {
                block.UpdateState();
            }
            foreach (var ball in _balls)
            {
                ball.UpdateState();
            }
        }
    }
}