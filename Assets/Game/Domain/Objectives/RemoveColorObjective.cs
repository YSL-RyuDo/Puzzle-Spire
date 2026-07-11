using System;

namespace PuzzleSpire.Domain.Puzzle
{
    public sealed class RemoveColorObjective :
        IStageObjective
    {
        public string ObjectiveId { get; }

        public BlockColor TargetColor { get; }

        public int CurrentAmount { get; private set; }

        public int TargetAmount { get; }

        public int RemainingAmount =>
            Math.Max(
                0,
                TargetAmount - CurrentAmount);

        public bool IsCompleted =>
            CurrentAmount >= TargetAmount;

        public RemoveColorObjective(
            string objectiveId,
            BlockColor targetColor,
            int targetAmount)
        {
            if (string.IsNullOrWhiteSpace(objectiveId))
            {
                throw new ArgumentException(
                    "목표 ID가 필요합니다.",
                    nameof(objectiveId));
            }

            if (!Enum.IsDefined(
                    typeof(BlockColor),
                    targetColor))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(targetColor),
                    targetColor,
                    "정의되지 않은 블록 색상입니다.");
            }

            if (targetAmount <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(targetAmount),
                    targetAmount,
                    "목표 개수는 1 이상이어야 합니다.");
            }

            ObjectiveId = objectiveId;
            TargetColor = targetColor;
            TargetAmount = targetAmount;
        }

        public bool Apply(
            PuzzleTurnResult turnResult)
        {
            if (turnResult == null)
            {
                throw new ArgumentNullException(
                    nameof(turnResult));
            }

            if (!turnResult.WasResolved
                || IsCompleted)
            {
                return false;
            }

            int removedTargetCount = 0;

            for (int i = 0;
                 i < turnResult.RemovedBlocks.Count;
                 i++)
            {
                RemovedBlockInfo removedBlock =
                    turnResult.RemovedBlocks[i];

                if (removedBlock.Color == TargetColor)
                {
                    removedTargetCount++;
                }
            }

            if (removedTargetCount == 0)
            {
                return false;
            }

            int previousAmount =
                CurrentAmount;

            CurrentAmount =
                Math.Min(
                    TargetAmount,
                    CurrentAmount
                    + removedTargetCount);

            return CurrentAmount != previousAmount;
        }
    }
}