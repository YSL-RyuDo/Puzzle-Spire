using System;
using NUnit.Framework;
using PuzzleSpire.Domain.Puzzle;

namespace PuzzleSpire.Domain.Tests.Puzzle
{
    public sealed class VoidCellTests
    {
        [Test]
        public void Board_CreatesVoidCellAtSpecifiedPosition()
        {
            GridPosition voidPosition =
                new GridPosition(1, 1);

            Board board =
                new Board(
                    3,
                    3,
                    new[]
                    {
                        voidPosition
                    });

            Assert.That(
                board.IsPlayable(voidPosition),
                Is.False);

            Assert.That(
                board.GetCellType(voidPosition),
                Is.EqualTo(CellType.Void));

            Assert.That(
                board.GetBlock(voidPosition),
                Is.Null);
        }

        [Test]
        public void PlaceBlock_OnVoidCell_ThrowsException()
        {
            GridPosition voidPosition =
                new GridPosition(1, 1);

            Board board =
                new Board(
                    3,
                    3,
                    new[]
                    {
                        voidPosition
                    });

            Assert.Throws<InvalidOperationException>(
                () => board.PlaceBlock(
                    voidPosition,
                    new Block(BlockColor.Red)));
        }

        [Test]
        public void SpawnSystem_DoesNotFillVoidCell()
        {
            GridPosition voidPosition =
                new GridPosition(1, 1);

            Board board =
                new Board(
                    3,
                    3,
                    new[]
                    {
                        voidPosition
                    });

            IBlockFactory factory =
                new FixedBlockFactory(
                    BlockColor.Red);

            BlockSpawnSystem spawnSystem =
                new BlockSpawnSystem(factory);

            var result =
                spawnSystem.FillEmptyCells(board);

            Assert.That(
                result.Count,
                Is.EqualTo(8));

            Assert.That(
                board.GetBlock(voidPosition),
                Is.Null);
        }

        [Test]
        public void Gravity_SkipsVoidAndMovesToLowestPlayableCell()
        {
            GridPosition voidPosition =
                new GridPosition(0, 1);

            Board board =
                new Board(
                    1,
                    4,
                    new[]
                    {
                        voidPosition
                    });

            Block block =
                new Block(BlockColor.Blue);

            board.PlaceBlock(
                new GridPosition(0, 3),
                block);

            GravitySystem gravitySystem =
                new GravitySystem();

            var moves =
                gravitySystem.Apply(board);

            Assert.That(
                board.GetBlock(
                    new GridPosition(0, 0)),
                Is.SameAs(block));

            Assert.That(
                board.GetBlock(voidPosition),
                Is.Null);

            Assert.That(
                moves.Count,
                Is.EqualTo(1));

            Assert.That(
                moves[0].From,
                Is.EqualTo(
                    new GridPosition(0, 3)));

            Assert.That(
                moves[0].To,
                Is.EqualTo(
                    new GridPosition(0, 0)));
        }

        [Test]
        public void Finder_DoesNotCrossVoidCell()
        {
            GridPosition voidPosition =
                new GridPosition(1, 0);

            Board board =
                new Board(
                    3,
                    1,
                    new[]
                    {
                        voidPosition
                    });

            board.PlaceBlock(
                new GridPosition(0, 0),
                new Block(BlockColor.Red));

            board.PlaceBlock(
                new GridPosition(2, 0),
                new Block(BlockColor.Red));

            ConnectedBlockFinder finder =
                new ConnectedBlockFinder();

            var result =
                finder.Find(
                    board,
                    new GridPosition(0, 0));

            Assert.That(
                result.Count,
                Is.EqualTo(1));
        }

        private sealed class FixedBlockFactory
            : IBlockFactory
        {
            private readonly BlockColor _color;

            public FixedBlockFactory(
                BlockColor color)
            {
                _color = color;
            }

            public Block Create()
            {
                return new Block(_color);
            }
        }

        [Test]
        public void Gravity_WhenBottomCellIsVoid_MovesToLowestPlayableCell()
        {
            GridPosition bottomVoidPosition =
                new GridPosition(0, 0);

            Board board =
                new Board(
                    1,
                    4,
                    new[]
                    {
                bottomVoidPosition
                    });

            Block block =
                new Block(BlockColor.Red);

            board.PlaceBlock(
                new GridPosition(0, 3),
                block);

            GravitySystem gravitySystem =
                new GravitySystem();

            var moves =
                gravitySystem.Apply(board);

            Assert.That(
                board.GetBlock(
                    new GridPosition(0, 0)),
                Is.Null);

            Assert.That(
                board.GetBlock(
                    new GridPosition(0, 1)),
                Is.SameAs(block));

            Assert.That(
                moves.Count,
                Is.EqualTo(1));

            Assert.That(
                moves[0].From,
                Is.EqualTo(
                    new GridPosition(0, 3)));

            Assert.That(
                moves[0].To,
                Is.EqualTo(
                    new GridPosition(0, 1)));
        }
    }
}