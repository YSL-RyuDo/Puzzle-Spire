using System;
using NUnit.Framework;
using PuzzleSpire.Domain.Puzzle;

namespace PuzzleSpire.Domain.Tests.Puzzle
{
    public sealed class RemoveColorObjectiveTests
    {
        [Test]
        public void Apply_IncreasesMatchingColorProgress()
        {
            RemoveColorObjective objective =
                new RemoveColorObjective(
                    "remove_red",
                    BlockColor.Red,
                    5);

            PuzzleTurnResult result =
                CreateResult(
                    BlockColor.Red,
                    BlockColor.Red,
                    BlockColor.Blue);

            bool changed =
                objective.Apply(result);

            Assert.That(changed, Is.True);
            Assert.That(
                objective.CurrentAmount,
                Is.EqualTo(2));

            Assert.That(
                objective.RemainingAmount,
                Is.EqualTo(3));
        }

        [Test]
        public void Apply_IgnoresDifferentColor()
        {
            RemoveColorObjective objective =
                new RemoveColorObjective(
                    "remove_red",
                    BlockColor.Red,
                    5);

            PuzzleTurnResult result =
                CreateResult(
                    BlockColor.Blue,
                    BlockColor.Green);

            bool changed =
                objective.Apply(result);

            Assert.That(changed, Is.False);
            Assert.That(
                objective.CurrentAmount,
                Is.EqualTo(0));
        }

        [Test]
        public void Apply_CapsProgressAtTargetAmount()
        {
            RemoveColorObjective objective =
                new RemoveColorObjective(
                    "remove_red",
                    BlockColor.Red,
                    2);

            PuzzleTurnResult result =
                CreateResult(
                    BlockColor.Red,
                    BlockColor.Red,
                    BlockColor.Red);

            objective.Apply(result);

            Assert.That(
                objective.CurrentAmount,
                Is.EqualTo(2));

            Assert.That(
                objective.IsCompleted,
                Is.True);
        }

        [Test]
        public void ObjectiveSystem_AllCompletedReturnsTrue()
        {
            RemoveColorObjective objective =
                new RemoveColorObjective(
                    "remove_red",
                    BlockColor.Red,
                    1);

            ObjectiveSystem system =
                new ObjectiveSystem(
                    new IStageObjective[]
                    {
                        objective
                    });

            system.Apply(
                CreateResult(BlockColor.Red));

            Assert.That(
                system.AllCompleted,
                Is.True);
        }

        private static PuzzleTurnResult CreateResult(
            params BlockColor[] colors)
        {
            GridPosition[] positions =
                new GridPosition[colors.Length];

            RemovedBlockInfo[] removedBlocks =
                new RemovedBlockInfo[colors.Length];

            for (int i = 0; i < colors.Length; i++)
            {
                GridPosition position =
                    new GridPosition(i, 0);

                positions[i] = position;

                removedBlocks[i] =
                    new RemovedBlockInfo(
                        position,
                        colors[i],
                        BlockType.Normal);
            }

            return new PuzzleTurnResult(
                true,
                positions,
                removedBlocks,
                Array.Empty<GravityMove>(),
                Array.Empty<BlockSpawn>());
        }
    }
}