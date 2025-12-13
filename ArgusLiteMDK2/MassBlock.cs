using SpaceEngineers.Game.ModAPI.Ingame;
using VRageMath;

namespace IngameScript
{
    internal class MassBlock
    {
        public IMyArtificialMassBlock block;
        public double distanceFromCenterSquared;
        public bool Enabled = true;
        public Vector3D moment;
        public Vector3D previousMoment = new Vector3D(0, 0, 0);

        public MassBlock(IMyArtificialMassBlock massBlock)
        {
            block = massBlock;
            moment = new Vector3D(0, 0, 0);
        }

        public bool Functional => block.IsFunctional;

        public void CalculateMoment(Vector3D centerOfMass)
        {
            previousMoment = moment;
            var blockPosition = block.GetPosition();
            var distanceVector = blockPosition - centerOfMass;
            double mass = Functional ? 50000 : 0; // 50 tonnes in kg
            moment = distanceVector * mass;
            distanceFromCenterSquared = distanceVector.ALengthSquared();
        }
    }
}