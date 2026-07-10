using NUnit.Framework;
using PuzzleSpire.Domain.Puzzle;

namespace PuzzleSpire.Domain.Tests.Puzzle
{
    public sealed class GridPositionTests
    {
        [Test]
        public void Constructor_SetsXAndY()
        {
            GridPosition position = new GridPosition(3, 5);

            Assert.That(position.X, Is.EqualTo(3));
            Assert.That(position.Y, Is.EqualTo(5));
        }

        [Test]
        public void EqualPositions_AreEqual()
        {
            GridPosition first = new GridPosition(2, 4);
            GridPosition second = new GridPosition(2, 4);

            Assert.That(first, Is.EqualTo(second));
            Assert.That(first == second, Is.True);
        }

        [Test]
        public void DifferentPositions_AreNotEqual()
        {
            GridPosition first = new GridPosition(2, 4);
            GridPosition second = new GridPosition(4, 2);

            Assert.That(first, Is.Not.EqualTo(second));
            Assert.That(first != second, Is.True);
        }

        [Test]
        public void ToString_ReturnsReadableCoordinates()
        {
            GridPosition position = new GridPosition(7, 8);

            Assert.That(position.ToString(), Is.EqualTo("(7, 8)"));
        }
    }
}