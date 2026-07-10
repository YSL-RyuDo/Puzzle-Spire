using System;
using NUnit.Framework;
using PuzzleSpire.Domain.Puzzle;

namespace PuzzleSpire.Domain.Tests.Puzzle
{
    public sealed class GravitySystemTests
    {
        [Test]
        public void Apply_MovesBlockDownIntoEmptySpace()
        {
            Board board = new Board(3, 4);

            Block block = new Block(BlockColor.Red);

            board.PlaceBlock(
                new GridPosition(1, 3),
                block);

            GravitySystem gravitySystem =
                new GravitySystem();

            var moves = gravitySystem.Apply(board);

            Assert.That(moves, Has.Count.EqualTo(1));

            Assert.That(
                board.IsEmpty(new GridPosition(1, 3)),
                Is.True);

            Assert.That(
                board.GetBlock(new GridPosition(1, 0)),
                Is.SameAs(block));

            Assert.That(
                moves[0].From,
                Is.EqualTo(new GridPosition(1, 3)));

            Assert.That(
                moves[0].To,
                Is.EqualTo(new GridPosition(1, 0)));

            Assert.That(
                moves[0].Block,
                Is.SameAs(block));
        }

        [Test]
        public void Apply_StacksBlocksFromBottomWithoutChangingOrder()
        {
            Board board = new Board(1, 5);

            Block bottomBlock =
                new Block(BlockColor.Red);

            Block middleBlock =
                new Block(BlockColor.Blue);

            Block topBlock =
                new Block(BlockColor.Green);

            board.PlaceBlock(
                new GridPosition(0, 0),
                bottomBlock);

            board.PlaceBlock(
                new GridPosition(0, 2),
                middleBlock);

            board.PlaceBlock(
                new GridPosition(0, 4),
                topBlock);

            GravitySystem gravitySystem =
                new GravitySystem();

            var moves = gravitySystem.Apply(board);

            Assert.That(moves, Has.Count.EqualTo(2));

            Assert.That(
                board.GetBlock(new GridPosition(0, 0)),
                Is.SameAs(bottomBlock));

            Assert.That(
                board.GetBlock(new GridPosition(0, 1)),
                Is.SameAs(middleBlock));

            Assert.That(
                board.GetBlock(new GridPosition(0, 2)),
                Is.SameAs(topBlock));

            Assert.That(
                board.IsEmpty(new GridPosition(0, 3)),
                Is.True);

            Assert.That(
                board.IsEmpty(new GridPosition(0, 4)),
                Is.True);
        }

        [Test]
        public void Apply_HandlesEachColumnIndependently()
        {
            Board board = new Board(2, 4);

            Block firstColumnBlock =
                new Block(BlockColor.Yellow);

            Block secondColumnBlock =
                new Block(BlockColor.Purple);

            board.PlaceBlock(
                new GridPosition(0, 3),
                firstColumnBlock);

            board.PlaceBlock(
                new GridPosition(1, 2),
                secondColumnBlock);

            GravitySystem gravitySystem =
                new GravitySystem();

            var moves = gravitySystem.Apply(board);

            Assert.That(moves, Has.Count.EqualTo(2));

            Assert.That(
                board.GetBlock(new GridPosition(0, 0)),
                Is.SameAs(firstColumnBlock));

            Assert.That(
                board.GetBlock(new GridPosition(1, 0)),
                Is.SameAs(secondColumnBlock));
        }

        [Test]
        public void Apply_DoesNotMoveBlocksInFullColumn()
        {
            Board board = new Board(1, 3);

            Block firstBlock =
                new Block(BlockColor.Red);

            Block secondBlock =
                new Block(BlockColor.Blue);

            Block thirdBlock =
                new Block(BlockColor.Green);

            board.PlaceBlock(
                new GridPosition(0, 0),
                firstBlock);

            board.PlaceBlock(
                new GridPosition(0, 1),
                secondBlock);

            board.PlaceBlock(
                new GridPosition(0, 2),
                thirdBlock);

            GravitySystem gravitySystem =
                new GravitySystem();

            var moves = gravitySystem.Apply(board);

            Assert.That(moves, Is.Empty);

            Assert.That(
                board.GetBlock(new GridPosition(0, 0)),
                Is.SameAs(firstBlock));

            Assert.That(
                board.GetBlock(new GridPosition(0, 1)),
                Is.SameAs(secondBlock));

            Assert.That(
                board.GetBlock(new GridPosition(0, 2)),
                Is.SameAs(thirdBlock));
        }

        [Test]
        public void Apply_OnEmptyBoard_ReturnsEmptyResult()
        {
            Board board = new Board(3, 3);

            GravitySystem gravitySystem =
                new GravitySystem();

            var moves = gravitySystem.Apply(board);

            Assert.That(moves, Is.Empty);
        }

        [Test]
        public void Apply_WithNullBoard_ThrowsException()
        {
            GravitySystem gravitySystem =
                new GravitySystem();

            Assert.Throws<ArgumentNullException>(
                () => gravitySystem.Apply(null));
        }
    }
}