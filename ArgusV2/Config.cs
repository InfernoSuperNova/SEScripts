namespace IngameScript
{
    public static class Config
    {
        public const string ArgumentUnTarget = "Untarget";
        public const string ArgumentTarget = "Target";


        public static double ProportialGain = 500;
        public static double IntegralGain = 0;
        public static double DerivativeGain = 30;
        
        public static double IntegralLowerLimit = -0.05;
        public static double IntegralUpperLimit = 0.05;
        
        
        public static double MaxAngularVelocityRpm = 30;
        public static string GroupName = "ArgusV2";

        public static double MaxWeaponRange = 2000;
    }
}