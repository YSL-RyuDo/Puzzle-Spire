using System;
using NUnit.Framework;
using PuzzleSpire.Domain.Puzzle;

namespace PuzzleSpire.Domain.Tests.Puzzle
{
    public sealed class BoardTests
    {
        [Test]
        public void Constructor_SetsWidthAndHeight()
        {
            Board board = new Board(9, 9);

            Assert.That(board.Width, Is.EqualTo(9));
            Assert.That(board.Height, Is.EqualTo(9));
        }

        [Test]
        public void Constructor_CreatesEmptyCells()
        {
            Board board = new Board(3, 3);
            GridPosition position = new GridPosition(2, 1);

            Cell cell = board.GetCell(position);

            Assert.That(cell.Position, Is.EqualTo(position));
            Assert.That(cell.IsEmpty, Is.True);
        }

        [Test]
        public void Constructor_WithInvalidWidth_ThrowsException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new Board(0, 9));
        }

        [Test]
        public void Constructor_WithInvalidHeight_ThrowsException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new Board(9, 0));
        }

        [Test]
        public void IsInside_ReturnsTrueForValidPosition()
        {
            Board board = new Board(3, 3);

            bool result = board.IsInside(
                new GridPosition(2, 2));

            Assert.That(result, Is.True);
        }

        [Test]
        public void IsInside_ReturnsFalseForInvalidPosition()
        {
            Board board = new Board(3, 3);

            Assert.That(
                board.IsInside(new GridPosition(-1, 0)),
                Is.False);

            Assert.That(
                board.IsInside(new GridPosition(3, 0)),
                Is.False);

            Assert.That(
                board.IsInside(new GridPosition(0, 3)),
                Is.False);
        }

        [Test]
        public void PlaceBlock_PlacesBlockAtPosition()
        {
            Board board = new Board(3, 3);
            GridPosition position = new GridPosition(1, 2);
            Block block = new Block(BlockColor.Red);

            board.PlaceBlock(position, block);

            Assert.That(board.IsEmpty(position), Is.False);
            Assert.That(
                board.GetBlock(position),
                Is.SameAs(block));
        }

        [Test]
        public void RemoveBlock_RemovesAndReturnsBlock()
        {
            Board board = new Board(3, 3);
            GridPosition position = new GridPosition(1, 1);
            Block block = new Block(BlockColor.Blue);

            board.PlaceBlock(position, block);

            Block removedBlock = board.RemoveBlock(position);

            Assert.That(removedBlock, Is.SameAs(block));
            Assert.That(board.IsEmpty(position), Is.True);
        }

        [Test]
        public void GetCell_WithOutOfRangePosition_ThrowsException()
        {
            Board board = new Board(3, 3);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => board.GetCell(
                    new GridPosition(3, 0)));
        }
    }
}