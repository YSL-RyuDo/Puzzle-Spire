using System;
using System.Collections.Generic;
using PuzzleSpire.Domain.Puzzle;
using UnityEngine;

namespace PuzzleSpire.Presentation.Puzzle
{
    public sealed class BoardView : MonoBehaviour
    {
        [Header("References")]

        [SerializeField]
        private BlockView _blockPrefab;

        [SerializeField]
        private Transform _blockRoot;

        [Header("Board Layout")]

        [SerializeField]
        [Min(0.1f)]
        private float _cellSize = 1f;

        [SerializeField]
        [Range(0f, 0.45f)]
        private float _blockGap = 0.08f;

        private readonly Dictionary<GridPosition, BlockView>
            _blockViews =
                new Dictionary<GridPosition, BlockView>();

        public int BlockViewCount => _blockViews.Count;

        public void Rebuild(Board board)
        {
            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            if (_blockPrefab == null)
            {
                throw new InvalidOperationException(
                    "BoardView에 Block Prefab이 연결되지 않았습니다.");
            }

            Clear();

            Transform root =
                _blockRoot != null
                    ? _blockRoot
                    : transform;

            float startX =
                -((board.Width - 1) * _cellSize) * 0.5f;

            float startY =
                -((board.Height - 1) * _cellSize) * 0.5f;

            float blockScale =
                Mathf.Max(
                    0.05f,
                    _cellSize - _blockGap);

            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    GridPosition position =
                        new GridPosition(x, y);

                    Block block =
                        board.GetBlock(position);

                    if (block == null)
                    {
                        continue;
                    }

                    BlockView blockView =
                        Instantiate(
                            _blockPrefab,
                            root);

                    blockView.transform.localPosition =
                        new Vector3(
                            startX + x * _cellSize,
                            startY + y * _cellSize,
                            0f);

                    blockView.transform.localRotation =
                        Quaternion.identity;

                    blockView.transform.localScale =
                        new Vector3(
                            blockScale,
                            blockScale,
                            1f);

                    blockView.Initialize(
                        position,
                        block);

                    _blockViews.Add(
                        position,
                        blockView);
                }
            }
        }

        public bool TryGetBlockView(
            GridPosition position,
            out BlockView blockView)
        {
            return _blockViews.TryGetValue(
                position,
                out blockView);
        }

        public void Clear()
        {
            foreach (BlockView blockView in _blockViews.Values)
            {
                if (blockView == null)
                {
                    continue;
                }

                if (Application.isPlaying)
                {
                    Destroy(blockView.gameObject);
                }
                else
                {
                    DestroyImmediate(blockView.gameObject);
                }
            }

            _blockViews.Clear();
        }
    }
}