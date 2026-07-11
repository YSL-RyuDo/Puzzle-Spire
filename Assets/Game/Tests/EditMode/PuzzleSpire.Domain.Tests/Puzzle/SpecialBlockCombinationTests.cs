using NUnit.Framework;
using PuzzleSpire.Domain.Puzzle;

namespace PuzzleSpire.Domain.Tests.Puzzle
{
    public sealed class SpecialBlockCombinationTests
    {
        [Test]
        public void Resolver_FindsAdjacentRocketCombination()
        {
            Board board =
                CreateFilledBoard(3, 3);

            ReplaceBlock(
                board,
                new GridPosition(1, 1),
                new Block(
                    BlockColor.Red,
                    BlockType.HorizontalRocket));

            ReplaceBlock(
                board,
                new GridPosition(2, 1),
                new Block(
                    BlockColor.Blue,
                    BlockType.VerticalRocket));

            SpecialBlockCombinationResolver resolver =
                new SpecialBlockCombinationResolver();

            bool resolved =
                resolver.TryResolve(
                    board,
                    new GridPosition(1, 1),
                    out SpecialBlockCombination combination);

            Assert.That(resolved, Is.True);

            Assert.That(
                combination.Type,
                Is.EqualTo(
                    SpecialBlockCombinationType
                        .RocketRocket));
        }

        [Test]
        public void Resolver_DoesNotUseDiagonalBlock()
        {
            Board board =
                CreateFilledBoard(3, 3);

            ReplaceBlock(
                board,
                new GridPosition(0, 0),
                new Block(
                    BlockColor.Red,
                    BlockType.Bomb));

            ReplaceBlock(
                board,
                new GridPosition(1, 1),
                new Block(
                    BlockColor.Blue,
                    BlockType.Bomb));

            SpecialBlockCombinationResolver resolver =
                new SpecialBlockCombinationResolver();

            bool resolved =
                resolver.TryResolve(
                    board,
                    new GridPosition(0, 0),
                    out _);

            Assert.That(resolved, Is.False);
        }

        [Test]
        public void RocketRocket_RemovesCross()
        {
            Board board =
                CreateFilledBoard(5, 5);

            GridPosition firstPosition =
                new GridPosition(2, 2);

            GridPosition secondPosition =
                new GridPosition(3, 2);

            Block firstBlock =
                new Block(
                    BlockColor.Red,
                    BlockType.HorizontalRocket);

            Block secondBlock =
                new Block(
                    BlockColor.Blue,
                    BlockType.VerticalRocket);

            ReplaceBlock(
                board,
                firstPosition,
                firstBlock);

            ReplaceBlock(
                board,
                secondPosition,
                secondBlock);

            SpecialBlockCombination combination =
                new SpecialBlockCombination(
                    SpecialBlockCombinationType
                        .RocketRocket,
                    firstPosition,
                    firstBlock,
                    secondPosition,
                    secondBlock);

            SpecialBlockEffectResolver chainResolver =
                new SpecialBlockEffectResolver();

            SpecialBlockCombinationEffectResolver resolver =
                new SpecialBlockCombinationEffectResolver(
                    chainResolver);

            SpecialBlockEffectResult result =
                resolver.Resolve(
                    board,
                    combination);

            Assert.That(
                result.AffectedPositions.Count,
                Is.EqualTo(9));
        }

        [Test]
        public void RocketBomb_RemovesThreeRowsAndColumns()
        {
            Board board =
                CreateFilledBoard(7, 7);

            GridPosition firstPosition =
                new GridPosition(3, 3);

            GridPosition secondPosition =
                new GridPosition(4, 3);

            Block rocket =
                new Block(
                    BlockColor.Red,
                    BlockType.HorizontalRocket);

            Block bomb =
                new Block(
                    BlockColor.Blue,
                    BlockType.Bomb);

            ReplaceBlock(
                board,
                firstPosition,
                rocket);

            ReplaceBlock(
                board,
                secondPosition,
                bomb);

            SpecialBlockCombination combination =
                new SpecialBlockCombination(
                    SpecialBlockCombinationType
                        .RocketBomb,
                    firstPosition,
                    rocket,
                    secondPosition,
                    bomb);

            SpecialBlockCombinationEffectResolver resolver =
                new SpecialBlockCombinationEffectResolver(
                    new SpecialBlockEffectResolver());

            SpecialBlockEffectResult result =
                resolver.Resolve(
                    board,
                    combination);

            Assert.That(
                result.AffectedPositions.Count,
                Is.EqualTo(33));
        }

        [Test]
        public void BombBomb_RemovesFiveByFiveArea()
        {
            Board board =
                CreateFilledBoard(7, 7);

            GridPosition firstPosition =
                new GridPosition(3, 3);

            GridPosition secondPosition =
                new GridPosition(4, 3);

            Block firstBomb =
                new Block(
                    BlockColor.Red,
                    BlockType.Bomb);

            Block secondBomb =
                new Block(
                    BlockColor.Blue,
                    BlockType.Bomb);

            ReplaceBlock(
                board,
                firstPosition,
                firstBomb);

            ReplaceBlock(
                board,
                secondPosition,
                secondBomb);

            SpecialBlockCombination combination =
                new SpecialBlockCombination(
                    SpecialBlockCombinationType
                        .BombBomb,
                    firstPosition,
                    firstBomb,
                    secondPosition,
                    secondBomb);

            SpecialBlockCombinationEffectResolver resolver =
                new SpecialBlockCombinationEffectResolver(
                    new SpecialBlockEffectResolver());

            SpecialBlockEffectResult result =
                resolver.Resolve(
                    board,
                    combination);

            Assert.That(
                result.AffectedPositions.Count,
                Is.EqualTo(49));
        }

        [Test]
        public void ColorBombColorBomb_RemovesEntireBoard()
        {
            Board board =
                CreateFilledBoard(5, 5);

            GridPosition firstPosition =
                new GridPosition(2, 2);

            GridPosition secondPosition =
                new GridPosition(3, 2);

            Block firstColorBomb =
                new Block(
                    BlockColor.Red,
                    BlockType.ColorBomb);

            Block secondColorBomb =
                new Block(
                    BlockColor.Blue,
                    BlockType.ColorBomb);

            ReplaceBlock(
                board,
                firstPosition,
                firstColorBomb);

            ReplaceBlock(
                board,
                secondPosition,
                secondColorBomb);

            SpecialBlockCombination combination =
                new SpecialBlockCombination(
                    SpecialBlockCombinationType
                        .ColorBombColorBomb,
                    firstPosition,
                    firstColorBomb,
                    secondPosition,
                    secondColorBomb);

            SpecialBlockCombinationEffectResolver resolver =
                new SpecialBlockCombinationEffectResolver(
                    new SpecialBlockEffectResolver());

            SpecialBlockEffectResult result =
                resolver.Resolve(
                    board,
                    combination);

            Assert.That(
                result.AffectedPositions.Count,
                Is.EqualTo(25));
        }

        private static Board CreateFilledBoard(
            int width,
            int height)
        {
            Board board =
                new Board(width, height);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    board.PlaceBlock(
                        new GridPosition(x, y),
                        new Block(BlockColor.Red));
                }
            }

            return board;
        }

        private static void ReplaceBlock(
            Board board,
            GridPosition position,
            Block replacement)
        {
            board.RemoveBlock(position);

            board.PlaceBlock(
                position,
                replacement);
        }
    }
}