namespace PuzzleSpire.Domain.Puzzle
{
    public readonly struct ObstacleDamageResult
    {
        public int RequestedDamage { get; }

        public int AppliedDamage { get; }

        public int PreviousDurability { get; }

        public int CurrentDurability { get; }

        public bool WasDamaged =>
            AppliedDamage > 0;

        public bool WasDestroyedThisHit =>
            PreviousDurability > 0
            && CurrentDurability <= 0;

        public ObstacleDamageResult(
            int requestedDamage,
            int appliedDamage,
            int previousDurability,
            int currentDurability)
        {
            RequestedDamage = requestedDamage;
            AppliedDamage = appliedDamage;
            PreviousDurability = previousDurability;
            CurrentDurability = currentDurability;
        }
    }
}