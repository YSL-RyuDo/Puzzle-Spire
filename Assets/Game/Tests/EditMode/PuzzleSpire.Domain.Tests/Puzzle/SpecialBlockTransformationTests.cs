using System.Collections.Generic;
using NUnit.Framework;
using PuzzleSpire.Domain.Puzzle;

namespace PuzzleSpire.Domain.Tests.Puzzle
{
    public sealed class SpecialBlockTransformationTests
    {
        [Test]
        public void BlueColorBombAndBomb_TransformsAllBlueBlocksIntoBombs()
        {
            Board board =
                CreateMixedBoard(6, 4);

            GridPosition colorBombPosition =
                new GridPosition(1, 1);

            GridPosition bombPosition =
                new GridPosition(2, 1);

            ReplaceBlock(
                board,
                colorBombPosition,
                new Block(
                    BlockColor.Blue,
                    BlockType.ColorBomb));

            ReplaceBlock(
                board,
                bombPosition,
                new Block(
                    BlockColor.Red,
                    BlockType.Bomb));

            ColorBombSpecialGroupEffectResolver resolver =
                new ColorBombSpecialGroupEffectResolver(
                    new SpecialBlockEffectResolver());

            bool resolved =
                resolver.TryResolve(
                    board,
                    colorBombPosition,
                    out SpecialBlockEffectResult result);

            Assert.That(resolved, Is.True);

            Assert.That(
                result.Transformations.Count,
                Is.GreaterThan(0));

            for (int i = 0;
                 i < result.Transformations.Count;
                 i++)
            {
                SpecialBlockTransformation transformation =
                    result.Transformations[i];

                Assert.That(
                    transformation.Color,
                    Is.EqualTo(BlockColor.Blue));

                Assert.That(
                    transformation.TransformedType,
                    Is.EqualTo(BlockType.Bomb));
            }
        }

        [Test]
        public void ColorBombBombRocket_MixesBombAndRocketTransformations()
        {
            Board board =
                CreateMixedBoard(7, 5);

            GridPosition colorBombPosition =
                new GridPosition(2, 2);

            GridPosition bombPosition =
                new GridPosition(3, 2);

            GridPosition rocketPosition =
                new GridPosition(4, 2);

            ReplaceBlock(
                board,
                colorBombPosition,
                new Block(
                    BlockColor.Blue,
                    BlockType.ColorBomb));

            ReplaceBlock(
                board,
                bombPosition,
                new Block(
                    BlockColor.Red,
                    BlockType.Bomb));

            ReplaceBlock(
                board,
                rocketPosition,
                new Block(
                    BlockColor.Green,
                    BlockType.HorizontalRocket));

            ColorBombSpecialGroupEffectResolver resolver =
                new ColorBombSpecialGroupEffectResolver(
                    new SpecialBlockEffectResolver());

            bool resolved =
                resolver.TryResolve(
                    board,
                    colorBombPosition,
                    out SpecialBlockEffectResult result);

            Assert.That(resolved, Is.True);

            Assert.That(
                result.Transformations.Count,
                Is.GreaterThanOrEqualTo(2));

            bool foundBomb = false;
            bool foundRocket = false;

            for (int i = 0;
                 i < result.Transformations.Count;
                 i++)
            {
                SpecialBlockTransformation transformation =
                    result.Transformations[i];

                Assert.That(
                    transformation.Color,
                    Is.EqualTo(BlockColor.Blue));

                if (transformation.TransformedType
                    == BlockType.Bomb)
                {
                    foundBomb = true;
                }

                if (transformation.TransformedType
                    == BlockType.HorizontalRocket
                    || transformation.TransformedType == BlockType.VerticalRocket)
                {
                    foundRocket = true;
                }
            }

            Assert.That(foundBomb, Is.True);
            Assert.That(foundRocket, Is.True);
        }

        [Test]
        public void TwoBombsAndOneRocket_UsesTwoToOneTransformationRatio()
        {
            Board board =
                CreateBlueBoard(8, 5);

            GridPosition colorBombPosition =
                new GridPosition(2, 2);

            GridPosition firstBombPosition =
                new GridPosition(3, 2);

            GridPosition secondBombPosition =
                new GridPosition(4, 2);

            GridPosition rocketPosition =
                new GridPosition(5, 2);

            ReplaceBlock(
                board,
                colorBombPosition,
                new Block(
                    BlockColor.Blue,
                    BlockType.ColorBomb));

            ReplaceBlock(
                board,
                firstBombPosition,
                new Block(
                    BlockColor.Red,
                    BlockType.Bomb));

            ReplaceBlock(
                board,
                secondBombPosition,
                new Block(
                    BlockColor.Green,
                    BlockType.Bomb));

            ReplaceBlock(
                board,
                rocketPosition,
                new Block(
                    BlockColor.Yellow,
                    BlockType.VerticalRocket));

            ColorBombSpecialGroupEffectResolver resolver =
                new ColorBombSpecialGroupEffectResolver(
                    new SpecialBlockEffectResolver());

            bool resolved =
                resolver.TryResolve(
                    board,
                    colorBombPosition,
                    out SpecialBlockEffectResult result);

            Assert.That(resolved, Is.True);

            List<BlockType> firstTypes =
                new List<BlockType>();

            int count =
                System.Math.Min(
                    6,
                    result.Transformations.Count);

            for (int i = 0; i < count; i++)
            {
                firstTypes.Add(
                    result.Transformations[i]
                        .TransformedType);
            }

            Assert.That(
                firstTypes[0],
                Is.EqualTo(BlockType.Bomb));

            Assert.That(
                firstTypes[1],
                Is.EqualTo(BlockType.Bomb));

            Assert.That(
                firstTypes[2],
                Is.EqualTo(BlockType.VerticalRocket).Or.EqualTo(BlockType.HorizontalRocket));

            Assert.That(
                firstTypes[3],
                Is.EqualTo(BlockType.Bomb));

            Assert.That(
                firstTypes[4],
                Is.EqualTo(BlockType.Bomb));

            Assert.That(
                firstTypes[5],
                Is.EqualTo(BlockType.VerticalRocket).Or.EqualTo(BlockType.HorizontalRocket));
        }

        private static Board CreateMixedBoard(
            int width,
            int height)
        {
            Board board =
                new Board(width, height);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    BlockColor color =
                        (x + y) % 2 == 0
                            ? BlockColor.Blue
                            : BlockColor.Red;

                    board.PlaceBlock(
                        new GridPosition(x, y),
                        new Block(color));
                }
            }

            return board;
        }

        private static Board CreateBlueBoard(
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
                        new Block(BlockColor.Blue));
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