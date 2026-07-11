using System;
using System.Collections.Generic;

namespace PuzzleSpire.Domain.Puzzle
{
    public sealed class RandomBlockFactory : IBlockFactory
    {
        private readonly BlockColor[] _availableColors;
        private readonly Random _random;

        public RandomBlockFactory(
            IReadOnlyList<BlockColor> availableColors,
            int randomSeed)
        {
            if (availableColors == null)
            {
                throw new ArgumentNullException(
                    nameof(availableColors));
            }

            if (availableColors.Count == 0)
            {
                throw new ArgumentException(
                    "사용 가능한 블록 색상이 하나 이상 필요합니다.",
                    nameof(availableColors));
            }

            _availableColors =
                new BlockColor[availableColors.Count];

            for (int i = 0; i < availableColors.Count; i++)
            {
                BlockColor color = availableColors[i];

                if (!Enum.IsDefined(typeof(BlockColor), color))
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(availableColors),
                        color,
                        "정의되지 않은 블록 색상이 포함되어 있습니다.");
                }

                _availableColors[i] = color;
            }

            _random = new Random(randomSeed);
        }

        public Block Create()
        {
            int randomIndex =
                _random.Next(0, _availableColors.Length);

            BlockColor selectedColor =
                _availableColors[randomIndex];

            return new Block(
                selectedColor,
                BlockType.Normal);
        }
    }
}