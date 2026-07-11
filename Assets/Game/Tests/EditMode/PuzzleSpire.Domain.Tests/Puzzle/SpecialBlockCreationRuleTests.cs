using System;
using NUnit.Framework;
using PuzzleSpire.Domain.Puzzle;

namespace PuzzleSpire.Domain.Tests.Puzzle
{
    public sealed class SpecialBlockCreationRuleTests
    {
        [Test]
        public void FourBlocks_DoNotCreateSpecialBlock()
        {
            SpecialBlockCreationRule rule =
                new SpecialBlockCreationRule();

            GridPosition[] positions =
            {
                new GridPosition(0, 0),
                new GridPosition(1, 0),
                new GridPosition(2, 0),
                new GridPosition(3, 0)
            };

            bool result =
                rule.TryGetSpecialBlockType(
                    positions,
                    new GridPosition(1, 0),
                    out BlockType type);

            Assert.That(result, Is.False);
            Assert.That(type, Is.EqualTo(BlockType.Normal));
        }

        [Test]
        public void SevenBlocks_CreateBomb()
        {
            SpecialBlockCreationRule rule =
                new SpecialBlockCreationRule();

            GridPosition[] positions =
            {
                new GridPosition(0, 0),
                new GridPosition(1, 0),
                new GridPosition(2, 0),
                new GridPosition(0, 1),
                new GridPosition(1, 1),
                new GridPosition(2, 1),
                new GridPosition(1, 2)
            };

            bool result =
                rule.TryGetSpecialBlockType(
                    positions,
                    new GridPosition(1, 1),
                    out BlockType type);

            Assert.That(result, Is.True);
            Assert.That(type, Is.EqualTo(BlockType.Bomb));
        }

        [Test]
        public void NineBlocks_CreateColorBomb()
        {
            SpecialBlockCreationRule rule =
                new SpecialBlockCreationRule();

            GridPosition[] positions =
            {
                new GridPosition(0, 0),
                new GridPosition(1, 0),
                new GridPosition(2, 0),
                new GridPosition(0, 1),
                new GridPosition(1, 1),
                new GridPosition(2, 1),
                new GridPosition(0, 2),
                new GridPosition(1, 2),
                new GridPosition(2, 2)
            };

            bool result =
                rule.TryGetSpecialBlockType(
                    positions,
                    new GridPosition(1, 1),
                    out BlockType type);

            Assert.That(result, Is.True);

            Assert.That(
                type,
                Is.EqualTo(
                    BlockType.ColorBomb));
        }

        [Test]
        public void Resolver_PreservesSelectedCellAndCreatesRocket()
        {
            Board board =
                new Board(5, 1);

            for (int x = 0; x < 5; x++)
            {
                board.PlaceBlock(
                    new GridPosition(x, 0),
                    new Block(BlockColor.Red));
            }

            ConnectedBlockFinder finder =
                new ConnectedBlockFinder();

            BlockRemovalSystem removalSystem =
                new BlockRemovalSystem(
                    finder,
                    minimumGroupSize: 2);

            GravitySystem gravitySystem =
                new GravitySystem();

            BlockSpawnSystem spawnSystem =
                new BlockSpawnSystem(
                    new FixedBlockFactory(
                        BlockColor.Blue));

            PuzzleStateMachine stateMachine =
                new PuzzleStateMachine(
                    PuzzleState.WaitingForInput);

            PuzzleTurnResolver resolver =
                new PuzzleTurnResolver(
                    finder,
                    removalSystem,
                    gravitySystem,
                    spawnSystem,
                    stateMachine);

            GridPosition selectedPosition =
                new GridPosition(2, 0);

            PuzzleTurnResult result =
                resolver.Resolve(
                    board,
                    selectedPosition);

            Assert.That(
                result.HasCreatedSpecialBlock,
                Is.True);

            Assert.That(
                result.RemovedCount,
                Is.EqualTo(4));

            Assert.That(
                result.CreatedSpecialBlock.Position,
                Is.EqualTo(selectedPosition));

            Assert.That(
                result.CreatedSpecialBlock.Type,
                Is.EqualTo(
                    BlockType.HorizontalRocket));

            Assert.That(
                board.GetBlock(selectedPosition).Type,
                Is.EqualTo(
                    BlockType.HorizontalRocket));
        }

        private sealed class FixedBlockFactory :
            IBlockFactory
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
        public void FiveBlocks_CreateRandomRocket()
        {
            SpecialBlockCreationRule rule =
                new SpecialBlockCreationRule(12345);

            GridPosition[] positions =
            {
        new GridPosition(0, 0),
        new GridPosition(1, 0),
        new GridPosition(2, 0),
        new GridPosition(3, 0),
        new GridPosition(4, 0)
    };

            bool result =
                rule.TryGetSpecialBlockType(
                    positions,
                    new GridPosition(2, 0),
                    out BlockType type);

            bool isRocket =
                type == BlockType.HorizontalRocket
                || type == BlockType.VerticalRocket;

            Assert.That(result, Is.True);
            Assert.That(isRocket, Is.True);
        }
    }


}