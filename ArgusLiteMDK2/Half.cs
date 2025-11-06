using System;

namespace IngameScript
{
    public struct Half
    {
        private readonly ushort value;

        
        public Half(float floatValue)
        {
            value = FloatToHalf(floatValue);
        }

        public static explicit operator Half(float floatValue)
        {
            return new Half(floatValue);
        }

        public static explicit operator Half(double doubleValue)
        {
            return new Half((float)doubleValue);
        }

        public static explicit operator char(Half halfValue)
        {
            return (char)halfValue.value;
        }

        private static ushort FloatToHalf(float floatValue)
        {
            // Simple conversion (for illustration purposes)
            var floatBits = BitConverter.ToInt32(BitConverter.GetBytes(floatValue), 0);
            var sign = (floatBits >> 16) & 0x8000;
            var exponent = ((floatBits >> 23) & 0xFF) - 127 + 15;
            var mantissa = floatBits & 0x007FFFFF;

            if (exponent <= 0)
            {
                mantissa = (mantissa | 0x00800000) >> (1 - exponent);
                return (ushort)(sign | (mantissa >> 13));
            }

            if (exponent == 0xFF - (127 - 15))
                return (ushort)(sign | 0x7C00); // Infinity or NaN
            if (exponent > 30)
                return (ushort)(sign | 0x7C00); // Overflow to Infinity
            return (ushort)(sign | (exponent << 10) | (mantissa >> 13));
        }
    }
}