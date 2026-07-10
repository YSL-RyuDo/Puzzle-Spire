using System;
using NUnit.Framework;
using PuzzleSpire.Domain.Puzzle;

namespace PuzzleSpire.Domain.Tests.Puzzle
{
    public sealed class BlockRemovalSystemTests
    {
        [Test]
        public void Remove_RemovesTwoOrMoreConnectedBlocks()
        {
            Board board = new Board(3, 3);

            GridPosition firstPosition =
                new GridPosition(0, 0);

            GridPosition secondPosition =
                new GridPosition(1, 0);

            GridPosition thirdPosition =
                new GridPosition(1, 1);

            board.PlaceBlock(
                firstPosition,
                new Block(BlockColor.Red));

            board.PlaceBlock(
                secondPosition,
                new Block(BlockColor.Red));

            board.PlaceBlock(
                thirdPosition,
                new Block(BlockColor.Red));

            BlockRemovalSystem removalSystem =
                CreateRemovalSystem();

            var removedPositions = removalSystem.Remove(
                board,
                firstPosition);

            Assert.That(removedPositions, Has.Count.EqualTo(3));
            Assert.That(board.IsEmpty(firstPosition), Is.True);
            Assert.That(board.IsEmpty(secondPosition), Is.True);
            Assert.That(board.IsEmpty(thirdPosition), Is.True);
        }

        [Test]
        public void Remove_DoesNotRemoveSingleBlock()
        {
            Board board = new Board(3, 3);

            GridPosition position =
                new GridPosition(1, 1);

            Block block =
                new Block(BlockColor.Blue);

            board.PlaceBlock(position, block);

            BlockRemovalSystem removalSystem =
                CreateRemovalSystem();

            var removedPositions = removalSystem.Remove(
                board,
                position);

            Assert.That(removedPositions, Is.Empty);
            Assert.That(board.IsEmpty(position), Is.False);
            Assert.That(board.GetBlock(position), Is.SameAs(block));
        }

        [Test]
        public void Remove_DoesNotRemoveDifferentColorNeighbor()
        {
            Board board = new Board(3, 3);

            GridPosition firstRedPosition =
                new GridPosition(0, 0);

            GridPosition secondRedPosition =
                new GridPosition(1, 0);

            GridPosition bluePosition =
                new GridPosition(2, 0);

            board.PlaceBlock(
                firstRedPosition,
                new Block(BlockColor.Red));

            board.PlaceBlock(
                secondRedPosition,
                new Block(BlockColor.Red));

            Block blueBlock =
                new Block(BlockColor.Blue);

            board.PlaceBlock(
                bluePosition,
                blueBlock);

            BlockRemovalSystem removalSystem =
                CreateRemovalSystem();

            var removedPositions = removalSystem.Remove(
                board,
                firstRedPosition);

            Assert.That(removedPositions, Has.Count.EqualTo(2));

            Assert.That(
                board.IsEmpty(firstRedPosition),
                Is.True);

            Assert.That(
                board.IsEmpty(secondRedPosition),
                Is.True);

            Assert.That(
                board.IsEmpty(bluePosition),
                Is.False);

            Assert.That(
                board.GetBlock(bluePosition),
                Is.SameAs(blueBlock));
        }

        [Test]
        public void Remove_FromEmptyCell_ReturnsEmptyResult()
        {
            Board board = new Board(3, 3);

            BlockRemovalSystem removalSystem =
                CreateRemovalSystem();

            var removedPositions = removalSystem.Remove(
                board,
                new GridPosition(1, 1));

            Assert.That(removedPositions, Is.Empty);
        }

        [Test]
        public void Remove_FromOutsideBoard_ReturnsEmptyResult()
        {
            Board board = new Board(3, 3);

            BlockRemovalSystem removalSystem =
                CreateRemovalSystem();

            var removedPositions = removalSystem.Remove(
                board,
                new GridPosition(3, 0));

            Assert.That(removedPositions, Is.Empty);
        }

        [Test]
        public void Remove_WithNullBoard_ThrowsException()
        {
            BlockRemovalSystem removalSystem =
                CreateRemovalSystem();

            Assert.Throws<ArgumentNullException>(
                () => removalSystem.Remove(
                    null,
                    new GridPosition(0, 0)));
        }

        [Test]
        public void Constructor_WithNullFinder_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(
                () => new BlockRemovalSystem(null));
        }

        [Test]
        public void Constructor_WithInvalidMinimumGroupSize_ThrowsException()
        {
            ConnectedBlockFinder finder =
                new ConnectedBlockFinder();

            Assert.Throws<ArgumentOutOfRangeException>(
                () => new BlockRemovalSystem(
                    finder,
                    1));
        }

        private static BlockRemovalSystem CreateRemovalSystem()
        {
            ConnectedBlockFinder finder =
                new ConnectedBlockFinder();

            return new BlockRemovalSystem(finder);
        }
    }
}