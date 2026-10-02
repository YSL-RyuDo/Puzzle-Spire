namespace PuzzleSpire.Domain.Puzzle
{
    public interface IDamageableObstacle :
        ICellObstacle
    {
        int MaxDurability { get; }

        int CurrentDurability { get; }

        bool IsDestroyed { get; }

        ObstacleImpactKind AcceptedImpacts { get; }

        ObstacleDamageResult TakeDamage(
            int amount);
    }
}