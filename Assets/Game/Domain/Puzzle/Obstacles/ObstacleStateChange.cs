namespace PuzzleSpire.Domain.Puzzle
{
    public readonly struct ObstacleStateChange
    {
        public GridPosition Position { get; }

        public string DefinitionId { get; }

        public CellObstacleType Type { get; }

        public ObstacleLayer Layer { get; }

        public ObstacleImpactKind ImpactKind { get; }

        public int PreviousDurability { get; }

        public int CurrentDurability { get; }

        public int AppliedDamage { get; }

        public bool WasDestroyed =>
            PreviousDurability > 0
            && CurrentDurability <= 0;

        public ObstacleStateChange(
            GridPosition position,
            string definitionId,
            CellObstacleType type,
            ObstacleLayer layer,
            ObstacleImpactKind impactKind,
            ObstacleDamageResult damageResult)
        {
            Position = position;
            DefinitionId = definitionId;
            Type = type;
            Layer = layer;
            ImpactKind = impactKind;

            PreviousDurability =
                damageResult.PreviousDurability;

            CurrentDurability =
                damageResult.CurrentDurability;

            AppliedDamage =
                damageResult.AppliedDamage;
        }
    }
}