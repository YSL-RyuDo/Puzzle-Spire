using System.Collections.Generic;
using NUnit.Framework;
using PuzzleSpire.Domain.Puzzle;

namespace PuzzleSpire.Domain.Tests.Puzzle
{
    public sealed class ObstacleImpactSystemTests
    {
        [Test]
        public void TwoSeparateBombEvents_ApplyTwoDamageToSameBox()
        {
            var board = new Board(3, 3);
            var boxPosition = new GridPosition(1, 1);
            var box = new BoxObstacle("wood_box", 3);

            board.PlaceObstacle(boxPosition, box);

            var events = new List<ObstacleImpactEvent>
            {
                new ObstacleImpactEvent(
                    ObstacleImpactKind.Direct,
                    new[] { boxPosition }),
                new ObstacleImpactEvent(
                    ObstacleImpactKind.Direct,
                    new[] { boxPosition })
            };

            var system = new ObstacleImpactSystem();
            IReadOnlyList<ObstacleStateChange> changes = system.ApplyImpacts(
                board,
                events,
                new GridPosition[0]);

            Assert.That(changes.Count, Is.EqualTo(1));
            Assert.That(changes[0].AppliedDamage, Is.EqualTo(2));
            Assert.That(box.CurrentDurability, Is.EqualTo(1));
        }

        [Test]
        public void DuplicatePositionInsideOneEvent_AppliesOnlyOneDamage()
        {
            var board = new Board(3, 3);
            var boxPosition = new GridPosition(1, 1);
            var box = new BoxObstacle("wood_box", 3);

            board.PlaceObstacle(boxPosition, box);

            var events = new List<ObstacleImpactEvent>
            {
                new ObstacleImpactEvent(
                    ObstacleImpactKind.Direct,
                    new[] { boxPosition, boxPosition })
            };

            var system = new ObstacleImpactSystem();
            system.ApplyImpacts(
                board,
                events,
                new GridPosition[0]);

            Assert.That(box.CurrentDurability, Is.EqualTo(2));
        }

        [Test]
        public void HorizontalAndVerticalEvents_ApplyTwoDamageAtIntersection()
        {
            var board = new Board(3, 3);
            var intersection = new GridPosition(1, 1);
            var box = new BoxObstacle("wood_box", 3);

            board.PlaceObstacle(intersection, box);

            var events = new List<ObstacleImpactEvent>
            {
                new ObstacleImpactEvent(
                    ObstacleImpactKind.Direct,
                    new[]
                    {
                        new GridPosition(0, 1),
                        intersection,
                        new GridPosition(2, 1)
                    }),
                new ObstacleImpactEvent(
                    ObstacleImpactKind.Direct,
                    new[]
                    {
                        new GridPosition(1, 0),
                        intersection,
                        new GridPosition(1, 2)
                    })
            };

            var system = new ObstacleImpactSystem();
            IReadOnlyList<ObstacleStateChange> changes = system.ApplyImpacts(
                board,
                events,
                new GridPosition[0]);

            Assert.That(changes.Count, Is.EqualTo(1));
            Assert.That(changes[0].AppliedDamage, Is.EqualTo(2));
            Assert.That(box.CurrentDurability, Is.EqualTo(1));
        }

        [Test]
        public void AdjacentImpact_DoesNotAddExtraDamageWhenDirectHitExists()
        {
            var board = new Board(3, 3);
            var boxPosition = new GridPosition(1, 1);
            var box = new BoxObstacle("wood_box", 3);

            board.PlaceObstacle(boxPosition, box);

            var events = new List<ObstacleImpactEvent>
            {
                new ObstacleImpactEvent(
                    ObstacleImpactKind.Direct,
                    new[] { boxPosition })
            };

            var removedPositions = new[]
            {
                new GridPosition(1, 0)
            };

            var system = new ObstacleImpactSystem();
            system.ApplyImpacts(
                board,
                events,
                removedPositions);

            Assert.That(box.CurrentDurability, Is.EqualTo(2));
        }
    }
}