using System;

namespace IngameScript.Helper
{
    public static class ArgusMath
    {
        public static double SnapToMultiple(double value, double multiple)
        {
            return (Math.Round(value / multiple) * multiple);
        }
    }
}