namespace PuzzleSpire.Domain.Puzzle
{
    public sealed class BoxObstacle :
        DamageableCellObstacle
    {
        public override CellObstacleType Type =>
            CellObstacleType.Box;

        public override ObstacleLayer Layer =>
            ObstacleLayer.Blocking;

        public override ObstacleImpactKind
            AcceptedImpacts =>
                ObstacleImpactKind.Direct
                | ObstacleImpactKind.Adjacent;

        public BoxObstacle(
            string definitionId,
            int maxDurability)
            : base(
                definitionId,
                maxDurability)
        {
        }
    }
}