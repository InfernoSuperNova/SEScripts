using System;

namespace IngameScript.Helper
{
    /// <summary>
    /// Item class.
    /// </summary>
    /// <summary>
    /// Item class.
    /// </summary>
    public static class ArgusMath
    {
        /// <summary>
        /// SnapToMultiple method.
        /// </summary>
        /// <param name="value">The value parameter.</param>
        /// <param name="multiple">The multiple parameter.</param>
        /// <returns>The result of the operation.</returns>
        /// <summary>
        /// SnapToMultiple method.
        /// </summary>
        /// <param name="value">The value parameter.</param>
        /// <param name="multiple">The multiple parameter.</param>
        /// <returns>The result of the operation.</returns>
        public static double SnapToMultiple(double value, double multiple)
        {
            return (Math.Round(value / multiple) * multiple);
        }
    }
}