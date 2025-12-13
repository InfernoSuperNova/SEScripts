using System;
using System.Text;
using VRageMath;

namespace IngameScript
{
    public static class ArgusExtensions
    {
        // Create a metamethod for assigning an int to a Vector3D
        public static Vector3D NormalizeTest(this Vector3D storage, Vector3D value)
        {
            var num = 1.0 / Math.Sqrt(value.X * value.X + value.Y * value.Y + value.Z * value.Z);

            storage.X = value.X * num;
            storage.Y = value.Y * num;
            storage.Z = value.Z * num;
            return storage;
        }


        public static double ALength(this Vector3D storage)
        {
            return storage.Length();
        }

        public static double ALengthSquared(this Vector3D storage)
        {
            return storage.LengthSquared();
        }


        public static StringBuilder TrimTrailingWhitespace(this StringBuilder sb)
        {
            //int num = sb.Length;
            //while (num > 0 && (sb[num - 1] == ' ' || sb[num - 1] == '\r' || sb[num - 1] == '\n'))
            //{
            //    num--;
            //}

            //sb.Length = num;
            //return sb;
            var n = sb.Length - 1;
            while (n >= 0 && char.IsWhiteSpace(sb[n]))
                n--;
            sb.Length = n + 1;
            return sb;
        }

        public static StringBuilder RemoveNonNumberChars(this StringBuilder sb)
        {
            // Compare to NumberCharWhitelist

            for (var i = 0; i < sb.Length; i++)
                if (!char.IsDigit(sb[i]))
                {
                    sb.Remove(i, 1);
                    i--;
                }

            return sb;
        }
    }
}