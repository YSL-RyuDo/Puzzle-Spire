using System;
using System.Linq;
using NUnit.Framework;
using PuzzleSpire.Domain.Puzzle;

namespace PuzzleSpire.Domain.Tests.Puzzle
{
    public sealed class ConnectedBlockFinderTests
    {
        [Test]
        public void Find_ReturnsConnectedBlocksWithSameColor()
        {
            Board board = new Board(3, 3);

            board.PlaceBlock(
                new GridPosition(0, 0),
                new Block(BlockColor.Red));

            board.PlaceBlock(
                new GridPosition(1, 0),
                new Block(BlockColor.Red));

            board.PlaceBlock(
                new GridPosition(1, 1),
                new Block(BlockColor.Red));

            board.PlaceBlock(
                new GridPosition(2, 1),
                new Block(BlockColor.Blue));

            ConnectedBlockFinder finder =
                new ConnectedBlockFinder();

            var result = finder.Find(
                board,
                new GridPosition(0, 0));

            Assert.That(result, Has.Count.EqualTo(3));

            Assert.That(
                result.Contains(new GridPosition(0, 0)),
                Is.True);

            Assert.That(
                result.Contains(new GridPosition(1, 0)),
                Is.True);

            Assert.That(
                result.Contains(new GridPosition(1, 1)),
                Is.True);
        }

        [Test]
        public void Find_DoesNotIncludeDifferentColorBlocks()
        {
            Board board = new Board(3, 3);

            board.PlaceBlock(
                new GridPosition(0, 0),
                new Block(BlockColor.Red));

            board.PlaceBlock(
                new GridPosition(1, 0),
                new Block(BlockColor.Blue));

            ConnectedBlockFinder finder =
                new ConnectedBlockFinder();

            var result = finder.Find(
                board,
                new GridPosition(0, 0));

            Assert.That(result, Has.Count.EqualTo(1));

            Assert.That(
                result.Contains(new GridPosition(0, 0)),
                Is.True);

            Assert.That(
                result.Contains(new GridPosition(1, 0)),
                Is.False);
        }

        [Test]
        public void Find_DoesNotIncludeDiagonalBlocks()
        {
            Board board = new Board(3, 3);

            board.PlaceBlock(
                new GridPosition(0, 0),
                new Block(BlockColor.Green));

            board.PlaceBlock(
                new GridPosition(1, 1),
                new Block(BlockColor.Green));

            ConnectedBlockFinder finder =
                new ConnectedBlockFinder();

            var result = finder.Find(
                board,
                new GridPosition(0, 0));

            Assert.That(result, Has.Count.EqualTo(1));

            Assert.That(
                result.Contains(new GridPosition(0, 0)),
                Is.True);

            Assert.That(
                result.Contains(new GridPosition(1, 1)),
                Is.False);
        }

        [Test]
        public void Find_ReturnsSingleBlockWhenNoSameColorNeighborExists()
        {
            Board board = new Board(3, 3);

            board.PlaceBlock(
                new GridPosition(1, 1),
                new Block(BlockColor.Yellow));

            ConnectedBlockFinder finder =
                new ConnectedBlockFinder();

            var result = finder.Find(
                board,
                new GridPosition(1, 1));

            Assert.That(result, Has.Count.EqualTo(1));

            Assert.That(
                result[0],
                Is.EqualTo(new GridPosition(1, 1)));
        }

        [Test]
        public void Find_ReturnsEmptyResultWhenStartCellIsEmpty()
        {
            Board board = new Board(3, 3);

            ConnectedBlockFinder finder =
                new ConnectedBlockFinder();

            var result = finder.Find(
                board,
                new GridPosition(1, 1));

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void Find_ReturnsEmptyResultWhenStartPositionIsOutsideBoard()
        {
            Board board = new Board(3, 3);

            ConnectedBlockFinder finder =
                new ConnectedBlockFinder();

            var result = finder.Find(
                board,
                new GridPosition(3, 0));

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void Find_WithNullBoard_ThrowsException()
        {
            ConnectedBlockFinder finder =
                new ConnectedBlockFinder();

            Assert.Throws<ArgumentNullException>(
                () => finder.Find(
                    null,
                    new GridPosition(0, 0)));
        }

        [Test]
        public void Find_FindsConnectedBlocksAroundCorner()
        {
            Board board = new Board(4, 4);

            board.PlaceBlock(
                new GridPosition(0, 0),
                new Block(BlockColor.Purple));

            board.PlaceBlock(
                new GridPosition(0, 1),
                new Block(BlockColor.Purple));

            board.PlaceBlock(
                new GridPosition(0, 2),
                new Block(BlockColor.Purple));

            board.PlaceBlock(
                new GridPosition(1, 2),
                new Block(BlockColor.Purple));

            board.PlaceBlock(
                new GridPosition(2, 2),
                new Block(BlockColor.Purple));

            ConnectedBlockFinder finder =
                new ConnectedBlockFinder();

            var result = finder.Find(
                board,
                new GridPosition(0, 0));

            Assert.That(result, Has.Count.EqualTo(5));

            Assert.That(
                result.Contains(new GridPosition(2, 2)),
                Is.True);
        }
    }
}