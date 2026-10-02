namespace PuzzleSpire.Domain.Puzzle
{
    public sealed class IceObstacle : DamageableCellObstacle
    {
        public override CellObstacleType Type => CellObstacleType.Ice;
        public override ObstacleLayer Layer => ObstacleLayer.Overlay;

        public override ObstacleImpactKind AcceptedImpacts =>
            ObstacleImpactKind.Direct |
            ObstacleImpactKind.SameCellBlockRemoved;

        public IceObstacle(string definitionId, int maxDurability)
            : base(definitionId, maxDurability)
        {
        }
    }
}