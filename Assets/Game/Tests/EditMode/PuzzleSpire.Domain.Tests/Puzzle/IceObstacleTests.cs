using NUnit.Framework;
using PuzzleSpire.Domain.Puzzle;

namespace PuzzleSpire.Domain.Tests.Puzzle
{
    public sealed class IceObstacleTests
    {
        [Test]
        public void Constructor_SetsOverlayLayerAndDurability()
        {
            var ice = new IceObstacle("normal_ice", 2);

            Assert.That(ice.Type, Is.EqualTo(CellObstacleType.Ice));
            Assert.That(ice.Layer, Is.EqualTo(ObstacleLayer.Overlay));
            Assert.That(ice.CurrentDurability, Is.EqualTo(2));
            Assert.That(
                ice.AcceptedImpacts.HasFlag(
                    ObstacleImpactKind.SameCellBlockRemoved),
                Is.True);
        }

        [Test]
        public void RemovingBlockOnIce_AppliesOneDamage()
        {
            var board = new Board(3, 3);
            var position = new GridPosition(1, 1);
            var ice = new IceObstacle("normal_ice", 2);

            board.PlaceObstacle(position, ice);
            board.PlaceBlock(
                position,
                new Block(BlockColor.Red, BlockType.Normal));

            board.RemoveBlock(position);

            var system = new ObstacleImpactSystem();
            system.ApplyImpacts(
                board,
                new ObstacleImpactEvent[0],
                new[] { position });

            Assert.That(ice.CurrentDurability, Is.EqualTo(1));
        }

        [Test]
        public void RemovingAdjacentBlock_DoesNotDamageIce()
        {
            var board = new Board(3, 3);
            var icePosition = new GridPosition(1, 1);
            var removedPosition = new GridPosition(1, 0);
            var ice = new IceObstacle("normal_ice", 2);

            board.PlaceObstacle(icePosition, ice);

            var system = new ObstacleImpactSystem();
            system.ApplyImpacts(
                board,
                new ObstacleImpactEvent[0],
                new[] { removedPosition });

            Assert.That(ice.CurrentDurability, Is.EqualTo(2));
        }

        [Test]
        public void DirectHitAndSameCellRemoval_DoNotDoubleCount()
        {
            var board = new Board(3, 3);
            var position = new GridPosition(1, 1);
            var ice = new IceObstacle("normal_ice", 3);

            board.PlaceObstacle(position, ice);

            var directEvents = new[]
            {
                new ObstacleImpactEvent(
                    ObstacleImpactKind.Direct,
                    new[] { position })
            };

            var system = new ObstacleImpactSystem();
            system.ApplyImpacts(
                board,
                directEvents,
                new[] { position });

            Assert.That(ice.CurrentDurability, Is.EqualTo(2));
        }

        [Test]
        public void TwoDirectEvents_ApplyTwoDamage()
        {
            var board = new Board(3, 3);
            var position = new GridPosition(1, 1);
            var ice = new IceObstacle("normal_ice", 3);

            board.PlaceObstacle(position, ice);

            var directEvents = new[]
            {
                new ObstacleImpactEvent(
                    ObstacleImpactKind.Direct,
                    new[] { position }),
                new ObstacleImpactEvent(
                    ObstacleImpactKind.Direct,
                    new[] { position })
            };

            var system = new ObstacleImpactSystem();
            system.ApplyImpacts(
                board,
                directEvents,
                new GridPosition[0]);

            Assert.That(ice.CurrentDurability, Is.EqualTo(1));
        }
    }
}