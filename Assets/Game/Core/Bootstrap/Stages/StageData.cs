using System;
using System.Collections.Generic;
using PuzzleSpire.Domain.Puzzle;
using UnityEngine;

namespace PuzzleSpire.Bootstrap
{
    [CreateAssetMenu(
        fileName = "Stage_001",
        menuName = "Puzzle Spire/Stage Data")]
    public sealed class StageData : ScriptableObject
    {
        [Header("Identity")]

        [SerializeField]
        private string _stageId = "Stage_001";

        [Header("Board")]

        [SerializeField]
        [Min(1)]
        private int _width = 9;

        [SerializeField]
        [Min(1)]
        private int _height = 9;

        [SerializeField]
        private int _randomSeed = 12345;

        [SerializeField]
        private BlockColor[] _availableColors =
        {
            BlockColor.Red,
            BlockColor.Blue,
            BlockColor.Green,
            BlockColor.Yellow,
            BlockColor.Purple
        };

        [Header("Stage Rules")]

        [SerializeField]
        [Min(1)]
        private int _moveLimit = 20;

        [SerializeField]
        private BlockColor _targetColor = BlockColor.Red;

        [SerializeField]
        [Min(1)]
        private int _targetAmount = 15;

        [Header("Layout - Top Row To Bottom Row")]

        [SerializeField]
        [TextArea(3, 20)]
        private string[] _layoutRows;

        public string StageId => _stageId;

        public int Width => _width;

        public int Height => _height;

        public int RandomSeed => _randomSeed;

        public int MoveLimit => _moveLimit;

        public BlockColor TargetColor => _targetColor;

        public int TargetAmount => _targetAmount;

        public BlockColor[] CreateAvailableColors()
        {
            ValidateOrThrow();

            BlockColor[] copiedColors =
                new BlockColor[_availableColors.Length];

            Array.Copy(
                _availableColors,
                copiedColors,
                _availableColors.Length);

            return copiedColors;
        }

        public IReadOnlyList<GridPosition>
            CreateVoidPositions()
        {
            ValidateOrThrow();

            if (_layoutRows == null
                || _layoutRows.Length == 0)
            {
                return Array.Empty<GridPosition>();
            }

            List<GridPosition> voidPositions =
                new List<GridPosition>();

            for (int rowIndex = 0;
                 rowIndex < _layoutRows.Length;
                 rowIndex++)
            {
                string row = _layoutRows[rowIndex];

                int boardY =
                    _height - 1 - rowIndex;

                for (int x = 0; x < row.Length; x++)
                {
                    char cellCharacter = row[x];

                    if (cellCharacter == 'X'
                        || cellCharacter == 'x')
                    {
                        voidPositions.Add(
                            new GridPosition(
                                x,
                                boardY));
                    }
                }
            }

            return voidPositions;
        }

        public void ValidateOrThrow()
        {
            if (string.IsNullOrWhiteSpace(_stageId))
            {
                throw new InvalidOperationException(
                    "Stage ID is empty.");
            }

            if (_width <= 0)
            {
                throw new InvalidOperationException(
                    "Stage width must be at least 1.");
            }

            if (_height <= 0)
            {
                throw new InvalidOperationException(
                    "Stage height must be at least 1.");
            }

            if (_moveLimit <= 0)
            {
                throw new InvalidOperationException(
                    "Move limit must be at least 1.");
            }

            if (_targetAmount <= 0)
            {
                throw new InvalidOperationException(
                    "Target amount must be at least 1.");
            }

            if (_availableColors == null
                || _availableColors.Length == 0)
            {
                throw new InvalidOperationException(
                    "At least one block color is required.");
            }

            for (int i = 0;
                 i < _availableColors.Length;
                 i++)
            {
                if (!Enum.IsDefined(
                        typeof(BlockColor),
                        _availableColors[i]))
                {
                    throw new InvalidOperationException(
                        $"Invalid block color: {_availableColors[i]}");
                }
            }

            if (_layoutRows == null
                || _layoutRows.Length == 0)
            {
                return;
            }

            if (_layoutRows.Length != _height)
            {
                throw new InvalidOperationException(
                    $"Layout row count must be {_height}, " +
                    $"but it is {_layoutRows.Length}.");
            }

            for (int rowIndex = 0;
                 rowIndex < _layoutRows.Length;
                 rowIndex++)
            {
                string row = _layoutRows[rowIndex];

                if (row == null)
                {
                    throw new InvalidOperationException(
                        $"Layout row {rowIndex} is null.");
                }

                if (row.Length != _width)
                {
                    throw new InvalidOperationException(
                        $"Layout row {rowIndex} must contain " +
                        $"{_width} characters.");
                }

                for (int x = 0; x < row.Length; x++)
                {
                    char character = row[x];

                    bool isPlayable =
                        character == 'O'
                        || character == 'o'
                        || character == '.';

                    bool isVoid =
                        character == 'X'
                        || character == 'x';

                    if (!isPlayable && !isVoid)
                    {
                        throw new InvalidOperationException(
                            $"Invalid layout character '{character}' " +
                            $"at row {rowIndex}, column {x}. " +
                            "Use O, X, or .");
                    }
                }
            }
        }
    }
}