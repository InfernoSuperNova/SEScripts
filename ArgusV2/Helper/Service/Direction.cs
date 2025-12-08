using VRageMath;

namespace IngameScript.Helper
{
    public enum Direction : byte
    {
        /// <summary>
        /// 
        /// </summary>
        Forward,
        /// <summary>
        /// 
        /// </summary>
        Backward,
        /// <summary>
        /// 
        /// </summary>
        Left,
        /// <summary>
        /// 
        /// </summary>
        Right,
        /// <summary>
        /// 
        /// </summary>
        Up,
        /// <summary>
        /// 
        /// </summary>
        Down,
    }

    public class Base6Directions
    {
        public static readonly Vector3[] Directions = new Vector3[6]
        {
            Vector3.Forward,
            Vector3.Backward,
            Vector3.Left,
            Vector3.Right,
            Vector3.Up,
            Vector3.Down
        };
    }
}