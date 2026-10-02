using NUnit.Framework;
using PuzzleSpire.Domain.Puzzle;

namespace PuzzleSpire.Domain.Tests.Puzzle
{
    public sealed class BoxObstacleTests
    {
        [Test]
        public void Constructor_SetsDurability()
        {
            BoxObstacle box =
                new BoxObstacle(
                    "wood_box",
                    2);

            Assert.That(
                box.MaxDurability,
                Is.EqualTo(2));

            Assert.That(
                box.CurrentDurability,
                Is.EqualTo(2));

            Assert.That(
                box.IsDestroyed,
                Is.False);
        }

        [Test]
        public void TakeDamage_DecreasesDurability()
        {
            BoxObstacle box =
                new BoxObstacle(
                    "wood_box",
                    2);

            ObstacleDamageResult result =
                box.TakeDamage(1);

            Assert.That(
                result.AppliedDamage,
                Is.EqualTo(1));

            Assert.That(
                box.CurrentDurability,
                Is.EqualTo(1));

            Assert.That(
                box.IsDestroyed,
                Is.False);
        }

        [Test]
        public void TakeDamage_WhenDurabilityReachesZero_DestroysBox()
        {
            BoxObstacle box =
                new BoxObstacle(
                    "wood_box",
                    1);

            ObstacleDamageResult result =
                box.TakeDamage(1);

            Assert.That(
                result.WasDestroyedThisHit,
                Is.True);

            Assert.That(
                box.IsDestroyed,
                Is.True);
        }

        [Test]
        public void TakeDamage_AfterDestroyed_DoesNotApplyAgain()
        {
            BoxObstacle box =
                new BoxObstacle(
                    "wood_box",
                    1);

            box.TakeDamage(1);

            ObstacleDamageResult secondResult =
                box.TakeDamage(1);

            Assert.That(
                secondResult.AppliedDamage,
                Is.EqualTo(0));
        }
    }
}