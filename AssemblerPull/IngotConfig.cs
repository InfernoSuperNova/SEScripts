namespace IngameScript
{
    public struct IngotConfig
    {
        public int DesiredAmount;
        public int RefillThreshold;

        public IngotConfig(int desired, int threshold)
        {
            DesiredAmount = desired;
            RefillThreshold = threshold;
        }
    }
}
