using System;
using System.Collections.Generic;
using DG.Tweening;
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

        [Header("Special Animation")]

        [SerializeField]
        [Min(0f)]
        private float _chainStepDelay = 0.05f;

        [Header("Removal Animation")]

        [SerializeField]
        [Min(0.01f)]
        private float _removalDuration = 0.16f;

        [Header("Gravity Animation")]

        [SerializeField]
        [Min(0.01f)]
        private float _gravityBaseDuration = 0.10f;

        [SerializeField]
        [Min(0f)]
        private float _gravityDurationPerCell = 0.035f;

        [Header("Spawn Animation")]

        [SerializeField]
        [Min(0.01f)]
        private float _spawnBaseDuration = 0.18f;

        [SerializeField]
        [Min(0f)]
        private float _spawnDurationPerCell = 0.025f;

        [SerializeField]
        [Min(0f)]
        private float _spawnStagger = 0.025f;

        private readonly Dictionary<GridPosition, BlockView>
            _viewsByPosition =
                new Dictionary<GridPosition, BlockView>();

        private readonly Dictionary<Block, BlockView>
            _viewsByBlock =
                new Dictionary<Block, BlockView>();

        private Board _board;
        private Sequence _activeSequence;

        private Transform LayoutRoot =>
            _blockRoot != null
                ? _blockRoot
                : transform;

        public void Rebuild(Board board)
        {
            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            ValidatePrefab();
            KillActiveSequence();
            ClearViews();

            _board = board;

            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    GridPosition position =
                        new GridPosition(x, y);

                    if (!board.IsPlayable(position))
                    {
                        continue;
                    }

                    Block block =
                        board.GetBlock(position);

                    if (block == null)
                    {
                        continue;
                    }

                    CreateBlockView(
                        block,
                        position,
                        CalculateLocalPosition(position));
                }
            }
        }

        public void PlayTurnAnimation(
            PuzzleTurnResult result,
            Action onCompleted)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if (_board == null)
            {
                throw new InvalidOperationException(
                    "BoardView is not initialized.");
            }

            if (_activeSequence != null
                && _activeSequence.IsActive())
            {
                throw new InvalidOperationException(
                    "A turn animation is already running.");
            }

            List<ActivationViewEntry> activationEntries =
                PrepareActivationViews(
                    result.ActivatedSpecialBlocks);

            List<BlockView> removedViews =
                PrepareRemovedViews(
                    result.RemovedPositions);

            SpecialCreationViewEntry specialCreationEntry =
                PrepareSpecialBlockCreation(
                    result.CreatedSpecialBlock);

            PrepareGravityMappings(
                result.GravityMoves);

            Sequence turnSequence =
                DOTween.Sequence();

            AppendSpecialActivationAnimation(
                turnSequence,
                activationEntries);

            AppendRemovalAnimation(
                turnSequence,
                removedViews);

            AppendSpecialBlockCreationAnimation(
                turnSequence,
                specialCreationEntry);

            AppendGravityAnimation(
                turnSequence,
                result.GravityMoves);

            _activeSequence = turnSequence;

            turnSequence.OnComplete(
                () =>
                {
                    _activeSequence = null;

                    PlaySpawnAnimation(
                        result.SpawnedBlocks,
                        onCompleted);
                });
        }

        public bool TryWorldToGridPosition(
            Vector3 worldPosition,
            out GridPosition gridPosition)
        {
            gridPosition = default;

            if (_board == null)
            {
                return false;
            }

            Vector3 localPosition =
                LayoutRoot.InverseTransformPoint(
                    worldPosition);

            float startX =
                CalculateStartX(_board.Width);

            float startY =
                CalculateStartY(_board.Height);

            float boardMinX =
                startX - (_cellSize * 0.5f);

            float boardMinY =
                startY - (_cellSize * 0.5f);

            int x =
                Mathf.FloorToInt(
                    (localPosition.x - boardMinX)
                    / _cellSize);

            int y =
                Mathf.FloorToInt(
                    (localPosition.y - boardMinY)
                    / _cellSize);

            GridPosition candidate =
                new GridPosition(x, y);

            if (!_board.IsPlayable(candidate))
            {
                return false;
            }

            gridPosition = candidate;
            return true;
        }

        private List<ActivationViewEntry>
            PrepareActivationViews(
                IReadOnlyList<SpecialBlockActivation> activations)
        {
            List<ActivationViewEntry> entries =
                new List<ActivationViewEntry>(
                    activations.Count);

            for (int i = 0;
                 i < activations.Count;
                 i++)
            {
                SpecialBlockActivation activation =
                    activations[i];

                if (!_viewsByPosition.TryGetValue(
                        activation.Position,
                        out BlockView blockView))
                {
                    throw new InvalidOperationException(
                        $"Activated BlockView was not found: " +
                        $"{activation.Position}");
                }

                entries.Add(
                    new ActivationViewEntry(
                        activation,
                        blockView));
            }

            return entries;
        }

        private List<BlockView> PrepareRemovedViews(
            IReadOnlyList<GridPosition> removedPositions)
        {
            List<BlockView> removedViews =
                new List<BlockView>(
                    removedPositions.Count);

            for (int i = 0;
                 i < removedPositions.Count;
                 i++)
            {
                GridPosition position =
                    removedPositions[i];

                if (!_viewsByPosition.TryGetValue(
                        position,
                        out BlockView blockView))
                {
                    throw new InvalidOperationException(
                        $"Removed BlockView was not found: " +
                        $"{position}");
                }

                _viewsByPosition.Remove(position);
                _viewsByBlock.Remove(blockView.Block);

                removedViews.Add(blockView);
            }

            return removedViews;
        }

        private SpecialCreationViewEntry
            PrepareSpecialBlockCreation(
                SpecialBlockCreation creation)
        {
            if (creation == null)
            {
                return null;
            }

            if (!_viewsByPosition.TryGetValue(
                    creation.Position,
                    out BlockView blockView))
            {
                throw new InvalidOperationException(
                    $"Special creation BlockView was not found: " +
                    $"{creation.Position}");
            }

            Block previousBlock =
                blockView.Block;

            _viewsByBlock.Remove(previousBlock);

            _viewsByBlock.Add(
                creation.Block,
                blockView);

            return new SpecialCreationViewEntry(
                blockView,
                creation.Block);
        }

        private void PrepareGravityMappings(
            IReadOnlyList<GravityMove> gravityMoves)
        {
            for (int i = 0;
                 i < gravityMoves.Count;
                 i++)
            {
                GravityMove move =
                    gravityMoves[i];

                if (!_viewsByBlock.ContainsKey(move.Block))
                {
                    throw new InvalidOperationException(
                        $"Gravity BlockView was not found: " +
                        $"{move.From} -> {move.To}");
                }

                _viewsByPosition.Remove(move.From);
            }

            for (int i = 0;
                 i < gravityMoves.Count;
                 i++)
            {
                GravityMove move =
                    gravityMoves[i];

                BlockView blockView =
                    _viewsByBlock[move.Block];

                blockView.SetGridPosition(move.To);

                _viewsByPosition.Add(
                    move.To,
                    blockView);
            }
        }

        private void AppendSpecialActivationAnimation(
    Sequence turnSequence,
    IReadOnlyList<ActivationViewEntry> entries)
        {
            if (entries.Count == 0)
            {
                return;
            }

            Sequence activationSequence =
                DOTween.Sequence();

            const float maximumTotalStagger = 0.06f;

            float staggerStep =
                entries.Count <= 1
                    ? 0f
                    : maximumTotalStagger
                      / (entries.Count - 1);

            for (int i = 0;
                 i < entries.Count;
                 i++)
            {
                ActivationViewEntry entry =
                    entries[i];

                Tween activationTween =
                    entry.View.CreateSpecialActivationTween(
                        entry.Activation.Type);

                activationSequence.Insert(
                    i * staggerStep,
                    activationTween);
            }

            turnSequence.Append(activationSequence);
        }

        private void AppendRemovalAnimation(
            Sequence turnSequence,
            IReadOnlyList<BlockView> removedViews)
        {
            if (removedViews.Count == 0)
            {
                return;
            }

            Sequence removalSequence =
                DOTween.Sequence();

            for (int i = 0;
                 i < removedViews.Count;
                 i++)
            {
                removalSequence.Join(
                    removedViews[i]
                        .transform
                        .DOScale(
                            Vector3.zero,
                            _removalDuration)
                        .SetEase(Ease.InBack));
            }

            turnSequence.Append(removalSequence);

            turnSequence.AppendCallback(
                () =>
                    DestroyRemovedViews(
                        removedViews));
        }

        private void AppendSpecialBlockCreationAnimation(
            Sequence turnSequence,
            SpecialCreationViewEntry entry)
        {
            if (entry == null)
            {
                return;
            }

            turnSequence.AppendCallback(
                () =>
                    entry.View.SetBlock(
                        entry.Block));

            turnSequence.Append(
                entry.View.CreateSpecialCreationTween(
                    entry.Block.Type));
        }

        private void AppendGravityAnimation(
            Sequence turnSequence,
            IReadOnlyList<GravityMove> gravityMoves)
        {
            if (gravityMoves.Count == 0)
            {
                return;
            }

            Sequence gravitySequence =
                DOTween.Sequence();

            for (int i = 0;
                 i < gravityMoves.Count;
                 i++)
            {
                GravityMove move =
                    gravityMoves[i];

                BlockView blockView =
                    _viewsByBlock[move.Block];

                int distance =
                    Mathf.Max(
                        1,
                        Mathf.Abs(
                            move.From.Y - move.To.Y));

                float duration =
                    _gravityBaseDuration
                    + distance
                    * _gravityDurationPerCell;

                gravitySequence.Join(
                    blockView.transform
                        .DOLocalMove(
                            CalculateLocalPosition(
                                move.To),
                            duration)
                        .SetEase(Ease.InQuad));
            }

            turnSequence.Append(gravitySequence);
        }

        private void PlaySpawnAnimation(
            IReadOnlyList<BlockSpawn> spawnedBlocks,
            Action onCompleted)
        {
            if (spawnedBlocks.Count == 0)
            {
                onCompleted?.Invoke();
                return;
            }

            Dictionary<int, int> spawnOrderByColumn =
                new Dictionary<int, int>();

            Sequence spawnSequence =
                DOTween.Sequence();

            for (int i = 0;
                 i < spawnedBlocks.Count;
                 i++)
            {
                BlockSpawn spawn =
                    spawnedBlocks[i];

                int column =
                    spawn.Position.X;

                if (!spawnOrderByColumn.TryGetValue(
                        column,
                        out int spawnOrder))
                {
                    spawnOrder = 0;
                }

                spawnOrderByColumn[column] =
                    spawnOrder + 1;

                Vector3 targetPosition =
                    CalculateLocalPosition(
                        spawn.Position);

                Vector3 startPosition =
                    CalculateSpawnStartPosition(
                        spawn.Position,
                        spawnOrder);

                BlockView blockView =
                    CreateBlockView(
                        spawn.Block,
                        spawn.Position,
                        startPosition);

                float dropDistance =
                    Mathf.Abs(
                        startPosition.y
                        - targetPosition.y)
                    / _cellSize;

                float duration =
                    _spawnBaseDuration
                    + dropDistance
                    * _spawnDurationPerCell;

                spawnSequence.Insert(
                    spawnOrder * _spawnStagger,
                    blockView.transform
                        .DOLocalMove(
                            targetPosition,
                            duration)
                        .SetEase(Ease.OutCubic));
            }

            _activeSequence = spawnSequence;

            spawnSequence.OnComplete(
                () =>
                {
                    _activeSequence = null;
                    onCompleted?.Invoke();
                });
        }

        private BlockView CreateBlockView(
            Block block,
            GridPosition position,
            Vector3 localPosition)
        {
            ValidatePrefab();

            BlockView blockView =
                Instantiate(
                    _blockPrefab,
                    LayoutRoot);

            blockView.transform.localPosition =
                localPosition;

            blockView.transform.localRotation =
                Quaternion.identity;

            float blockScale =
                Mathf.Max(
                    0.05f,
                    _cellSize - _blockGap);

            blockView.transform.localScale =
                new Vector3(
                    blockScale,
                    blockScale,
                    1f);

            blockView.Initialize(
                position,
                block);

            _viewsByPosition.Add(
                position,
                blockView);

            _viewsByBlock.Add(
                block,
                blockView);

            return blockView;
        }

        private Vector3 CalculateLocalPosition(
            GridPosition position)
        {
            return new Vector3(
                CalculateStartX(_board.Width)
                + position.X * _cellSize,

                CalculateStartY(_board.Height)
                + position.Y * _cellSize,

                0f);
        }

        private Vector3 CalculateSpawnStartPosition(
            GridPosition targetPosition,
            int spawnOrder)
        {
            Vector3 targetPositionLocal =
                CalculateLocalPosition(
                    targetPosition);

            float spawnY =
                CalculateStartY(_board.Height)
                + _board.Height * _cellSize
                + spawnOrder * _cellSize;

            return new Vector3(
                targetPositionLocal.x,
                spawnY,
                0f);
        }

        private void DestroyRemovedViews(
            IReadOnlyList<BlockView> removedViews)
        {
            for (int i = 0;
                 i < removedViews.Count;
                 i++)
            {
                BlockView blockView =
                    removedViews[i];

                if (blockView == null)
                {
                    continue;
                }

                blockView.gameObject.SetActive(false);
                Destroy(blockView.gameObject);
            }
        }

        private float CalculateStartX(int width)
        {
            return -((width - 1) * _cellSize) * 0.5f;
        }

        private float CalculateStartY(int height)
        {
            return -((height - 1) * _cellSize) * 0.5f;
        }

        private void ValidatePrefab()
        {
            if (_blockPrefab == null)
            {
                throw new InvalidOperationException(
                    "Block prefab is not assigned.");
            }
        }

        private void ClearViews()
        {
            foreach (BlockView blockView
                     in _viewsByBlock.Values)
            {
                if (blockView == null)
                {
                    continue;
                }

                if (UnityEngine.Application.isPlaying)
                {
                    Destroy(blockView.gameObject);
                }
                else
                {
                    DestroyImmediate(
                        blockView.gameObject);
                }
            }

            _viewsByPosition.Clear();
            _viewsByBlock.Clear();
        }

        private void KillActiveSequence()
        {
            if (_activeSequence == null)
            {
                return;
            }

            if (_activeSequence.IsActive())
            {
                _activeSequence.Kill();
            }

            _activeSequence = null;
        }

        private void OnDisable()
        {
            KillActiveSequence();
        }

        private readonly struct ActivationViewEntry
        {
            public SpecialBlockActivation Activation { get; }

            public BlockView View { get; }

            public ActivationViewEntry(
                SpecialBlockActivation activation,
                BlockView view)
            {
                Activation = activation;

                View = view
                    ?? throw new ArgumentNullException(
                        nameof(view));
            }
        }

        private sealed class SpecialCreationViewEntry
        {
            public BlockView View { get; }

            public Block Block { get; }

            public SpecialCreationViewEntry(
                BlockView view,
                Block block)
            {
                View = view
                    ?? throw new ArgumentNullException(
                        nameof(view));

                Block = block
                    ?? throw new ArgumentNullException(
                        nameof(block));
            }
        }
    }
}