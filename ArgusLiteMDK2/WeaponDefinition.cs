namespace IngameScript
{
    public class WeaponDefinition
    {
        public double MaxSpeed;
        public double InitialSpeed;
        public bool IsCappedSpeed;
        public double Acceleration;
        public bool HasGravity;


        public WeaponDefinition(double MaxSpeed, double InitialSpeed, bool isCappedSpeed, double Acceleration,
            bool HasGravity)
        {
            this.MaxSpeed = MaxSpeed;
            this.InitialSpeed = InitialSpeed;
            this.IsCappedSpeed = isCappedSpeed;
            this.Acceleration = Acceleration;
            this.HasGravity = HasGravity;
        }
    }
}