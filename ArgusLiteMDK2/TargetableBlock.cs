using VRageMath;

namespace IngameScript
{
    internal class TargetableBlock
    {
        public TargetableBlockCategory category;
        public Vector3I positionInGrid;

        public TargetableBlock(TargetableBlockCategory category, Vector3I positionInGrid)
        {
            this.category = category;
            this.positionInGrid = positionInGrid;
        }
    }
}