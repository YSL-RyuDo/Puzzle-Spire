using System;

namespace PuzzleSpire.Domain.Puzzle
{
    public sealed class RemoveIceObjective : IStageObjective
    {
        public string ObjectiveId { get; }
        public int TargetAmount { get; }
        public int CurrentAmount { get; private set; }
        public int RemainingAmount => Math.Max(0, TargetAmount - CurrentAmount);
        public bool IsCompleted => CurrentAmount >= TargetAmount;

        public RemoveIceObjective(string objectiveId, int targetAmount)
        {
            if (string.IsNullOrWhiteSpace(objectiveId))
            {
                throw new ArgumentException(
                    "목표 ID가 필요합니다.",
                    nameof(objectiveId));
            }

            if (targetAmount <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(targetAmount),
                    targetAmount,
                    "얼음 목표 개수는 1 이상이어야 합니다.");
            }

            ObjectiveId = objectiveId;
            TargetAmount = targetAmount;
        }

        public bool Apply(PuzzleTurnResult turnResult)
        {
            if (turnResult == null)
            {
                throw new ArgumentNullException(nameof(turnResult));
            }

            if (!turnResult.WasResolved || IsCompleted)
            {
                return false;
            }

            int destroyedIceCount = 0;

            for (int i = 0; i < turnResult.ObstacleChanges.Count; i++)
            {
                ObstacleStateChange change =
                    turnResult.ObstacleChanges[i];

                if (change.Type == CellObstacleType.Ice &&
                    change.WasDestroyed)
                {
                    destroyedIceCount++;
                }
            }

            if (destroyedIceCount == 0)
            {
                return false;
            }

            int previousAmount = CurrentAmount;

            CurrentAmount = Math.Min(
                TargetAmount,
                CurrentAmount + destroyedIceCount);

            return CurrentAmount != previousAmount;
        }

        public void Reset()
        {
            CurrentAmount = 0;
        }
    }
}