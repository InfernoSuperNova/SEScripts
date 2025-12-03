using System;
using VRageMath;

namespace IngameScript.TruncationWrappers
{
  
    public struct AT_Vector3D
    {
        private Vector3D _value;  
        


        public AT_Vector3D(double x, double y, double z)
        {
          _value = new Vector3D(x, y, z);
        }
        
        AT_Vector3D(Vector3D value)
        {
          _value = value;
        }
        
        public static implicit operator AT_Vector3D(Vector3D value) => new AT_Vector3D(value);
        public static implicit operator AT_Vector3D(Vector3 value) => (Vector3D)value;
        public static implicit operator Vector3D(AT_Vector3D value) => value._value;
        public static explicit operator Vector3I(AT_Vector3D value) => (Vector3I)value._value;
        public static explicit operator AT_Vector3D(Vector3I value) => (Vector3D)value;


        public static AT_Vector3D operator -(AT_Vector3D value) => -value._value;

        public static bool operator ==(AT_Vector3D value1, AT_Vector3D value2) => value1._value == value2._value;
        
        public static bool operator !=(AT_Vector3D value1, AT_Vector3D value2) => value1._value != value2._value;
        
        // public static AT_Vector3D operator %(AT_Vector3D value1, double value2) => value1._value % value2;
        //
        // public static AT_Vector3D operator %(AT_Vector3D value1, AT_Vector3D value2) => value1._value % value2._value;

        public static AT_Vector3D operator +(AT_Vector3D value1, AT_Vector3D value2) => value1._value + value2._value;
        
        //public static AT_Vector3D operator +(AT_Vector3D value1, double value2) => value1._value + value2;

        //public static AT_Vector3D operator +(AT_Vector3D value1, float value2) => value1._value + value2;
        
        public static AT_Vector3D operator -(AT_Vector3D value1, AT_Vector3D value2) => value1._value - value2._value;
        
        //public static AT_Vector3D operator -(AT_Vector3D value1, double value2) => value1._value - value2;
        
        public static AT_Vector3D operator *(AT_Vector3D value1, AT_Vector3D value2) => value1._value * value2._value;
        
        public static AT_Vector3D operator *(AT_Vector3D value, double scaleFactor) => value._value * scaleFactor;
        
        public static AT_Vector3D operator *(double scaleFactor, AT_Vector3D value) => value._value * scaleFactor;
        
        public static AT_Vector3D operator /(AT_Vector3D value1, AT_Vector3D value2) => value1._value / value2._value;
        
        public static AT_Vector3D operator /(AT_Vector3D value, double divider) => value._value / divider;

        //public static AT_Vector3D operator /(double value, AT_Vector3D divider) =>value / divider._value;

        public double X => _value.X;
        public double Y => _value.Y;
        public double Z => _value.Z;
        


        public static AT_Vector3D Zero => Vector3D.Zero;
        public static Vector3 Forward => Vector3D.Forward;
        public static Vector3 Left => Vector3D.Left;
        public static AT_Vector3D Up => Vector3D.Up;
        public double Length() => _value.Length();

        public double Dot(AT_Vector3D other) => _value.Dot(other._value);

        public double LengthSquared() => _value.LengthSquared();

        public static AT_Vector3D Transform(AT_Vector3D vec, MatrixD mat) => Vector3D.Transform(vec, mat);

        public static AT_Vector3D TransformNormal(AT_Vector3D vec, MatrixD mat) => Vector3D.TransformNormal(vec, mat);

        public static AT_Vector3D Abs(AT_Vector3D u1) => Vector3D.Abs(u1);

        public static AT_Vector3D Normalize(AT_Vector3D val) => Vector3D.Normalize(val._value);

        public static AT_Vector3D ClampToSphere(AT_Vector3D vec, double radius) =>
          Vector3D.ClampToSphere(vec._value, radius);

        public static AT_Vector3D ProjectOnVector(ref AT_Vector3D valA, ref AT_Vector3D valB) =>
          Vector3D.ProjectOnVector(ref valA._value, ref valB._value);

        public static AT_Vector3D Cross(AT_Vector3D vecA, AT_Vector3D vecB) => Vector3D.Cross(vecA, vecB);

        public AT_Vector3D Normalized() => _value.Normalized();

        public bool Equals(AT_Vector3D other) => _value == other._value;
        
    }
    
}