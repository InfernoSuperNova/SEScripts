using System;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRageMath;

namespace IngameScript
{
    internal class SpaceBall
    {
        public IMySpaceBall block;
        public bool Enabled = true;
        public Vector3D moment;
        public Vector3D momentDown;
        public Vector3D momentUp;
        public Vector3D previousMoment;
        public double previousMoveDownMul = 1000;
        public double previousMoveUpMul = 1000;

        public SpaceBall(IMySpaceBall spaceBall)
        {
            block = spaceBall;
            moment = new Vector3D(0, 0, 0);
        }

        public bool Functional => block.IsFunctional;

        public void CalculateMoment(Vector3D centerOfMass)
        {
            previousMoment = moment;
            var blockPosition = block.GetPosition();
            var distanceVector = blockPosition - centerOfMass;

            double mass = block.VirtualMass;
            moment = distanceVector * mass;
            momentDown = distanceVector * Math.Max(mass - previousMoveDownMul, 0);
            momentUp = distanceVector * Math.Min(mass + previousMoveUpMul, 20000);
        }

        public Vector3D CalculateMoment(Vector3D centerOfMass, float mass)
        {
            var blockPosition = block.GetPosition();
            var distanceVector = blockPosition - centerOfMass;
            return distanceVector * mass;
        }

        public void SetMass(float mass)
        {
            block.VirtualMass = mass;
        }
    }
}