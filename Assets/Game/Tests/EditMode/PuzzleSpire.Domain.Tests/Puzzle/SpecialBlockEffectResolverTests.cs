using NUnit.Framework;
using PuzzleSpire.Domain.Puzzle;

namespace PuzzleSpire.Domain.Tests.Puzzle
{
    public sealed class SpecialBlockEffectResolverTests
    {
        [Test]
        public void HorizontalRocket_AffectsEntireRow()
        {
            Board board = CreateFilledBoard(5, 3);

            ReplaceBlock(
                board,
                new GridPosition(2, 1),
                new Block(
                    BlockColor.Red,
                    BlockType.HorizontalRocket));

            SpecialBlockEffectResolver resolver =
                new SpecialBlockEffectResolver();

            SpecialBlockEffectResult result =
                resolver.Resolve(
                    board,
                    new[]
                    {
                        new GridPosition(2, 1)
                    });

            Assert.That(
                result.AffectedPositions.Count,
                Is.EqualTo(5));

            Assert.That(
                result.ActivatedSpecialBlocks.Count,
                Is.EqualTo(1));
        }

        [Test]
        public void VerticalRocket_AffectsEntireColumn()
        {
            Board board = CreateFilledBoard(3, 5);

            ReplaceBlock(
                board,
                new GridPosition(1, 2),
                new Block(
                    BlockColor.Red,
                    BlockType.VerticalRocket));

            SpecialBlockEffectResolver resolver =
                new SpecialBlockEffectResolver();

            SpecialBlockEffectResult result =
                resolver.Resolve(
                    board,
                    new[]
                    {
                        new GridPosition(1, 2)
                    });

            Assert.That(
                result.AffectedPositions.Count,
                Is.EqualTo(5));
        }

        [Test]
        public void Bomb_AffectsThreeByThreeArea()
        {
            Board board = CreateFilledBoard(5, 5);

            ReplaceBlock(
                board,
                new GridPosition(2, 2),
                new Block(
                    BlockColor.Red,
                    BlockType.Bomb));

            SpecialBlockEffectResolver resolver =
                new SpecialBlockEffectResolver();

            SpecialBlockEffectResult result =
                resolver.Resolve(
                    board,
                    new[]
                    {
                        new GridPosition(2, 2)
                    });

            Assert.That(
                result.AffectedPositions.Count,
                Is.EqualTo(9));
        }

        [Test]
        public void Rocket_HittingAnotherRocket_CreatesChainReaction()
        {
            Board board = CreateFilledBoard(5, 5);

            ReplaceBlock(
                board,
                new GridPosition(1, 2),
                new Block(
                    BlockColor.Red,
                    BlockType.HorizontalRocket));

            ReplaceBlock(
                board,
                new GridPosition(3, 2),
                new Block(
                    BlockColor.Blue,
                    BlockType.VerticalRocket));

            SpecialBlockEffectResolver resolver =
                new SpecialBlockEffectResolver();

            SpecialBlockEffectResult result =
                resolver.Resolve(
                    board,
                    new[]
                    {
                        new GridPosition(1, 2)
                    });

            Assert.That(
                result.ActivatedSpecialBlocks.Count,
                Is.EqualTo(2));

            Assert.That(
                result.AffectedPositions.Count,
                Is.EqualTo(9));
        }

        [Test]
        public void ColorBomb_AffectsAllMatchingColors()
        {
            Board board = new Board(3, 3);

            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    BlockColor color =
                        (x + y) % 2 == 0
                            ? BlockColor.Red
                            : BlockColor.Blue;

                    board.PlaceBlock(
                        new GridPosition(x, y),
                        new Block(color));
                }
            }

            ReplaceBlock(
                board,
                new GridPosition(1, 1),
                new Block(
                    BlockColor.Red,
                    BlockType.ColorBomb));

            SpecialBlockEffectResolver resolver =
                new SpecialBlockEffectResolver();

            SpecialBlockEffectResult result =
                resolver.Resolve(
                    board,
                    new[]
                    {
                        new GridPosition(1, 1)
                    });

            Assert.That(
                result.AffectedPositions.Count,
                Is.EqualTo(5));
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
            Block block)
        {
            board.RemoveBlock(position);
            board.PlaceBlock(position, block);
        }
    }
}