using IngameScript.Helper;
using IngameScript.TruncationWrappers;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRageMath;

namespace IngameScript.Ship.Components.Propulsion.Gravity.Wrapper
{
    public abstract class GravityGenerator
    {
        protected bool IsInverted;
        
        public IMyGravityGeneratorBase Generator { get; protected set; }
        public Direction Direction { get; protected set; }

        
        public bool Enabled
        {
            get { return Generator.Enabled; }
            set  { Generator.Enabled = value; }
        }

        // The managing classes should not have to concern themselves with what direction the gravity generator identifies as
        public float Acceleration
        {
            get
            {
                return Generator.GravityAcceleration * InvertedSign;
            }
            set
            {
                Generator.GravityAcceleration = value * InvertedSign;
            }
        }

        public AT_Vector3D Position => Generator.GetPosition();
        public bool Closed => Generator.Closed;

        public int InvertedSign => IsInverted ? -1 : 1;
        public Vector3I GridPosition => Generator.Position;
    }
    
    
}