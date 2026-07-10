using System;
using PuzzleSpire.Domain.Puzzle;
using PuzzleSpire.Presentation.Puzzle;
using UnityEngine;

namespace PuzzleSpire.Bootstrap
{
    public sealed class PuzzleBootstrap : MonoBehaviour
    {
        [Header("References")]

        [SerializeField]
        private BoardView _boardView;

        [Header("Board Settings")]

        [SerializeField]
        [Min(1)]
        private int _boardWidth = 9;

        [SerializeField]
        [Min(1)]
        private int _boardHeight = 9;

        [SerializeField]
        private int _randomSeed = 12345;

        private Board _board;

        public Board Board => _board;

        private void Start()
        {
            InitializePuzzle();
        }

        private void InitializePuzzle()
        {
            if (_boardView == null)
            {
                throw new InvalidOperationException(
                    "PuzzleBootstrap에 BoardView가 연결되지 않았습니다.");
            }

            _board =
                new Board(
                    _boardWidth,
                    _boardHeight);

            BlockColor[] availableColors =
            {
                BlockColor.Red,
                BlockColor.Blue,
                BlockColor.Green,
                BlockColor.Yellow,
                BlockColor.Purple
            };

            IBlockFactory blockFactory =
                new RandomBlockFactory(
                    availableColors,
                    _randomSeed);

            BlockSpawnSystem spawnSystem =
                new BlockSpawnSystem(blockFactory);

            spawnSystem.FillEmptyCells(_board);

            _boardView.Rebuild(_board);
        }
    }
}