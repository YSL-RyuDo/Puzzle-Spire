using System.Collections.Generic;
using PuzzleSpire.Domain.Puzzle;
using UnityEngine;

namespace PuzzleSpire.Presentation.Puzzle
{
    public sealed class SpecialBlockHintController :
        MonoBehaviour
    {
        [Header("References")]

        [SerializeField]
        private PuzzleController _puzzleController;

        [SerializeField]
        private BoardView _boardView;

        private readonly ConnectedBlockFinder
            _connectedBlockFinder =
                new ConnectedBlockFinder();

        private bool _previousCanShowHints;
        private bool _hasPreviousState;

        private void Awake()
        {
            if (_puzzleController == null)
            {
                _puzzleController =
                    GetComponent<PuzzleController>();
            }

            if (_boardView == null)
            {
                _boardView =
                    FindFirstObjectByType<BoardView>();
            }

            if (_puzzleController == null
                || _boardView == null)
            {
                Debug.LogError(
                    "SpecialBlockHintController references are missing.");

                enabled = false;
            }
        }

        private void Update()
        {
            if (!_puzzleController.IsInitialized)
            {
                return;
            }

            bool canShowHints =
                _puzzleController.CanAcceptInput;

            if (!_hasPreviousState)
            {
                _hasPreviousState = true;
                _previousCanShowHints = canShowHints;

                if (canShowHints)
                {
                    RefreshHints();
                }

                return;
            }

            if (_previousCanShowHints
                == canShowHints)
            {
                return;
            }

            _previousCanShowHints =
                canShowHints;

            if (canShowHints)
            {
                RefreshHints();
            }
            else
            {
                ClearHints();
            }
        }

        public void RefreshHints()
        {
            Board board =
                _puzzleController.Board;

            if (board == null)
            {
                return;
            }

            BlockView[] blockViews =
                _boardView.GetComponentsInChildren<
                    BlockView>(true);

            Dictionary<GridPosition, BlockView>
                viewsByPosition =
                    new Dictionary<
                        GridPosition,
                        BlockView>();

            for (int i = 0;
                 i < blockViews.Length;
                 i++)
            {
                BlockView blockView =
                    blockViews[i];

                viewsByPosition[
                    blockView.Position] =
                        blockView;

                GetHintView(blockView).Hide();
            }

            HashSet<GridPosition> visited =
                new HashSet<GridPosition>();

            for (int x = 0;
                 x < board.Width;
                 x++)
            {
                for (int y = 0;
                     y < board.Height;
                     y++)
                {
                    GridPosition position =
                        new GridPosition(x, y);

                    if (!board.IsPlayable(position)
                        || visited.Contains(position))
                    {
                        continue;
                    }

                    Block block =
                        board.GetBlock(position);

                    if (block == null
                        || block.Type != BlockType.Normal)
                    {
                        visited.Add(position);
                        continue;
                    }

                    IReadOnlyList<GridPosition>
                        connectedPositions =
                            _connectedBlockFinder.Find(
                                board,
                                position);

                    for (int i = 0;
                         i < connectedPositions.Count;
                         i++)
                    {
                        visited.Add(
                            connectedPositions[i]);
                    }

                    BlockType hintType =
                        GetHintType(
                            connectedPositions.Count);

                    if (hintType == BlockType.Normal)
                    {
                        continue;
                    }

                    ShowHintOnEntireGroup(
                        connectedPositions,
                        viewsByPosition,
                        hintType);
                }
            }
        }

        public void ClearHints()
        {
            BlockCreationHintView[] hintViews =
                _boardView.GetComponentsInChildren<
                    BlockCreationHintView>(true);

            for (int i = 0;
                 i < hintViews.Length;
                 i++)
            {
                hintViews[i].Hide();
            }
        }

        private static BlockType GetHintType(
            int connectedCount)
        {
            if (connectedCount >= 9)
            {
                return BlockType.ColorBomb;
            }

            if (connectedCount >= 7)
            {
                return BlockType.Bomb;
            }

            if (connectedCount >= 5)
            {
                // Preview is a generic rocket symbol.
                // Actual direction is selected when created.
                return BlockType.HorizontalRocket;
            }

            return BlockType.Normal;
        }

        private static void ShowHintOnEntireGroup(
            IReadOnlyList<GridPosition>
                connectedPositions,
            IReadOnlyDictionary<
                GridPosition,
                BlockView> viewsByPosition,
            BlockType hintType)
        {
            for (int i = 0;
                 i < connectedPositions.Count;
                 i++)
            {
                GridPosition position =
                    connectedPositions[i];

                if (!viewsByPosition.TryGetValue(
                        position,
                        out BlockView blockView))
                {
                    continue;
                }

                GetHintView(blockView).Show(
                    hintType);
            }
        }

        private static BlockCreationHintView
            GetHintView(BlockView blockView)
        {
            BlockCreationHintView hintView =
                blockView.GetComponent<
                    BlockCreationHintView>();

            if (hintView == null)
            {
                hintView =
                    blockView.gameObject.AddComponent<
                        BlockCreationHintView>();
            }

            return hintView;
        }

        private void OnDisable()
        {
            if (_boardView != null)
            {
                ClearHints();
            }

            _hasPreviousState = false;
            _previousCanShowHints = false;
        }
    }
}