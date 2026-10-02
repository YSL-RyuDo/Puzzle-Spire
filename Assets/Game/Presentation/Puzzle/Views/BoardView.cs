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
        [SerializeField] private BlockView _blockPrefab;
        [SerializeField] private ObstacleViewCatalog _obstacleViewCatalog;
        [SerializeField] private Transform _obstacleRoot;
        [SerializeField] private Transform _blockRoot;

        [Header("Board Layout")]
        [SerializeField, Min(0.1f)] private float _cellSize = 1f;
        [SerializeField, Range(0f, 0.45f)] private float _blockGap = 0.08f;

        [Header("Special Animation")]
        [SerializeField, Min(0f)] private float _chainStepDelay = 0.01f;
        [SerializeField, Min(0f)] private float _transformationStepDelay = 0.035f;
        [SerializeField, Min(0f)] private float _transformedActivationStepDelay = 0.045f;
        [SerializeField, Min(0.01f)] private float _combinationMergeDuration = 0.18f;
        [SerializeField, Min(0f)] private float _combinationMergeStagger = 0.02f;
        [SerializeField, Min(0.01f)] private float _combinationImpactDuration = 0.12f;

        [Header("Removal Animation")]
        [SerializeField, Min(0.01f)] private float _removalDuration = 0.16f;

        [Header("Gravity Animation")]
        [SerializeField, Min(0.01f)] private float _gravityBaseDuration = 0.10f;
        [SerializeField, Min(0f)] private float _gravityDurationPerCell = 0.035f;

        [Header("Spawn Animation")]
        [SerializeField, Min(0.01f)] private float _spawnBaseDuration = 0.18f;
        [SerializeField, Min(0f)] private float _spawnDurationPerCell = 0.025f;
        [SerializeField, Min(0f)] private float _spawnStagger = 0.025f;

        private readonly Dictionary<GridPosition, BlockView> _viewsByPosition =
            new Dictionary<GridPosition, BlockView>();

        private readonly Dictionary<Block, BlockView> _viewsByBlock =
            new Dictionary<Block, BlockView>();

        private readonly Dictionary<ObstacleKey, DamageableObstacleView> _obstacleViews =
            new Dictionary<ObstacleKey, DamageableObstacleView>();

        private Board _board;
        private Sequence _activeSequence;

        private Transform BlockRoot => _blockRoot != null ? _blockRoot : transform;
        private Transform ObstacleRoot => _obstacleRoot != null ? _obstacleRoot : transform;

        public void Rebuild(Board board)
        {
            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            ValidateReferences();
            KillActiveSequence();
            ClearViews();

            _board = board;

            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    GridPosition position = new GridPosition(x, y);

                    if (!board.IsPlayable(position))
                    {
                        continue;
                    }

                    CreateObstacleViewsAt(position);

                    Block block = board.GetBlock(position);

                    if (block != null)
                    {
                        CreateBlockView(
                            block,
                            position,
                            CalculateLocalPosition(position));
                    }
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
                    "BoardView가 초기화되지 않았습니다.");
            }

            if (_activeSequence != null && _activeSequence.IsActive())
            {
                throw new InvalidOperationException(
                    "이미 턴 애니메이션이 실행 중입니다.");
            }

            List<MergeViewEntry> mergeEntries =
                PrepareMergeViews(result.MergedSpecialBlocks);

            List<TransformationViewEntry> transformationEntries =
                PrepareTransformationViews(result.TransformedSpecialBlocks);

            List<ActivationViewEntry> activationEntries =
                PrepareActivationViews(result.ActivatedSpecialBlocks);

            List<BlockView> removedViews =
                PrepareRemovedViews(result.RemovedPositions);

            List<ObstacleChangeViewEntry> obstacleEntries =
                PrepareObstacleChanges(result.ObstacleChanges);

            SpecialCreationViewEntry specialCreationEntry =
                PrepareSpecialBlockCreation(result.CreatedSpecialBlock);

            PrepareGravityMappings(result.GravityMoves);

            Sequence turnSequence = DOTween.Sequence();

            AppendSpecialMergeAnimation(turnSequence, mergeEntries);
            AppendSpecialEffectAnimation(
                turnSequence,
                transformationEntries,
                activationEntries);
            AppendRemovalAnimation(turnSequence, removedViews);
            AppendObstacleChangeAnimation(turnSequence, obstacleEntries);
            AppendSpecialBlockCreationAnimation(
                turnSequence,
                specialCreationEntry);
            AppendGravityAnimation(turnSequence, result.GravityMoves);

            _activeSequence = turnSequence;

            turnSequence.OnComplete(() =>
            {
                _activeSequence = null;
                PlaySpawnAnimation(result.SpawnedBlocks, onCompleted);
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
                BlockRoot.InverseTransformPoint(worldPosition);

            float startX = CalculateStartX(_board.Width);
            float startY = CalculateStartY(_board.Height);
            float boardMinX = startX - _cellSize * 0.5f;
            float boardMinY = startY - _cellSize * 0.5f;

            int x = Mathf.FloorToInt(
                (localPosition.x - boardMinX) / _cellSize);

            int y = Mathf.FloorToInt(
                (localPosition.y - boardMinY) / _cellSize);

            GridPosition candidate = new GridPosition(x, y);

            if (!_board.IsPlayable(candidate))
            {
                return false;
            }

            gridPosition = candidate;
            return true;
        }

        public bool TryGetBlockView(
            GridPosition position,
            out BlockView blockView)
        {
            return _viewsByPosition.TryGetValue(
                position,
                out blockView);
        }

        private void CreateObstacleViewsAt(GridPosition position)
        {
            for (int layerIndex = 0;
                 layerIndex < (int)ObstacleLayer.Count;
                 layerIndex++)
            {
                ObstacleLayer layer = (ObstacleLayer)layerIndex;

                ICellObstacle obstacle =
                    _board.GetObstacle(position, layer);

                if (obstacle is IDamageableObstacle damageableObstacle)
                {
                    CreateObstacleView(
                        position,
                        damageableObstacle);
                }
            }
        }

        private void CreateObstacleView(
            GridPosition position,
            IDamageableObstacle obstacle)
        {
            DamageableObstacleView prefab =
                _obstacleViewCatalog.GetPrefab(obstacle.Type);

            DamageableObstacleView view = Instantiate(
                prefab,
                ObstacleRoot);

            view.transform.localPosition =
                CalculateLocalPosition(position);

            view.transform.localRotation = Quaternion.identity;
            view.transform.localScale = Vector3.one;
            view.Initialize(position, obstacle);

            SpriteRenderer spriteRenderer = view.SpriteRenderer;

            if (spriteRenderer == null || spriteRenderer.sprite == null)
            {
                throw new InvalidOperationException(
                    $"{obstacle.Type} View의 SpriteRenderer 또는 Sprite가 없습니다.");
            }

            float targetSize = Mathf.Max(
                0.05f,
                _cellSize - _blockGap);

            Vector2 spriteSize =
                spriteRenderer.sprite.bounds.size;

            float largestSide =
                Mathf.Max(spriteSize.x, spriteSize.y);

            if (largestSide <= 0f)
            {
                throw new InvalidOperationException(
                    $"{obstacle.Type} Sprite 크기가 올바르지 않습니다.");
            }

            float scale = targetSize / largestSide;

            view.transform.localScale =
                new Vector3(scale, scale, 1f);

            _obstacleViews.Add(
                new ObstacleKey(position, obstacle.Layer),
                view);
        }

        private List<ObstacleChangeViewEntry> PrepareObstacleChanges(
            IReadOnlyList<ObstacleStateChange> changes)
        {
            var entries =
                new List<ObstacleChangeViewEntry>(changes.Count);

            for (int i = 0; i < changes.Count; i++)
            {
                ObstacleStateChange change = changes[i];
                var key = new ObstacleKey(
                    change.Position,
                    change.Layer);

                if (!_obstacleViews.TryGetValue(
                        key,
                        out DamageableObstacleView view))
                {
                    throw new InvalidOperationException(
                        $"장애물 View를 찾을 수 없습니다: {key.Position}, {key.Layer}");
                }

                entries.Add(
                    new ObstacleChangeViewEntry(
                        view,
                        change));

                if (change.WasDestroyed)
                {
                    _obstacleViews.Remove(key);
                }
            }

            return entries;
        }

        private static void AppendObstacleChangeAnimation(
            Sequence turnSequence,
            IReadOnlyList<ObstacleChangeViewEntry> entries)
        {
            if (entries.Count == 0)
            {
                return;
            }

            Sequence obstacleSequence = DOTween.Sequence();

            for (int i = 0; i < entries.Count; i++)
            {
                ObstacleChangeViewEntry entry = entries[i];

                obstacleSequence.Join(
                    entry.View.CreateStateChangeTween(
                        entry.Change));
            }

            turnSequence.Append(obstacleSequence);

            turnSequence.AppendCallback(() =>
            {
                for (int i = 0; i < entries.Count; i++)
                {
                    ObstacleChangeViewEntry entry = entries[i];

                    if (entry.Change.WasDestroyed &&
                        entry.View != null)
                    {
                        Destroy(entry.View.gameObject);
                    }
                }
            });
        }

        private List<MergeViewEntry> PrepareMergeViews(
            IReadOnlyList<SpecialBlockMerge> merges)
        {
            var entries = new List<MergeViewEntry>(merges.Count);

            for (int i = 0; i < merges.Count; i++)
            {
                SpecialBlockMerge merge = merges[i];

                if (!_viewsByPosition.TryGetValue(
                        merge.SourcePosition,
                        out BlockView sourceView))
                {
                    throw new InvalidOperationException(
                        $"Merge source BlockView가 없습니다: {merge.SourcePosition}");
                }

                if (!_viewsByPosition.TryGetValue(
                        merge.TargetPosition,
                        out BlockView targetView))
                {
                    throw new InvalidOperationException(
                        $"Merge target BlockView가 없습니다: {merge.TargetPosition}");
                }

                entries.Add(
                    new MergeViewEntry(
                        merge,
                        sourceView,
                        targetView));
            }

            return entries;
        }

        private List<TransformationViewEntry> PrepareTransformationViews(
            IReadOnlyList<SpecialBlockTransformation> transformations)
        {
            var entries =
                new List<TransformationViewEntry>(transformations.Count);

            for (int i = 0; i < transformations.Count; i++)
            {
                SpecialBlockTransformation transformation =
                    transformations[i];

                if (!_viewsByPosition.TryGetValue(
                        transformation.Position,
                        out BlockView blockView))
                {
                    throw new InvalidOperationException(
                        $"Transformation BlockView가 없습니다: {transformation.Position}");
                }

                entries.Add(
                    new TransformationViewEntry(
                        transformation,
                        blockView));
            }

            return entries;
        }

        private List<ActivationViewEntry> PrepareActivationViews(
            IReadOnlyList<SpecialBlockActivation> activations)
        {
            var entries =
                new List<ActivationViewEntry>(activations.Count);

            for (int i = 0; i < activations.Count; i++)
            {
                SpecialBlockActivation activation = activations[i];

                if (!_viewsByPosition.TryGetValue(
                        activation.Position,
                        out BlockView blockView))
                {
                    throw new InvalidOperationException(
                        $"Activation BlockView가 없습니다: {activation.Position}");
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
            var removedViews =
                new List<BlockView>(removedPositions.Count);

            for (int i = 0; i < removedPositions.Count; i++)
            {
                GridPosition position = removedPositions[i];

                if (!_viewsByPosition.TryGetValue(
                        position,
                        out BlockView blockView))
                {
                    throw new InvalidOperationException(
                        $"제거할 BlockView가 없습니다: {position}");
                }

                _viewsByPosition.Remove(position);
                _viewsByBlock.Remove(blockView.Block);
                removedViews.Add(blockView);
            }

            return removedViews;
        }

        private SpecialCreationViewEntry PrepareSpecialBlockCreation(
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
                    $"특수 블록 생성 View가 없습니다: {creation.Position}");
            }

            _viewsByBlock.Remove(blockView.Block);
            _viewsByBlock.Add(creation.Block, blockView);

            return new SpecialCreationViewEntry(
                blockView,
                creation.Block);
        }

        private void PrepareGravityMappings(
            IReadOnlyList<GravityMove> gravityMoves)
        {
            for (int i = 0; i < gravityMoves.Count; i++)
            {
                GravityMove move = gravityMoves[i];

                if (!_viewsByBlock.ContainsKey(move.Block))
                {
                    throw new InvalidOperationException(
                        $"중력 BlockView가 없습니다: {move.From} -> {move.To}");
                }

                _viewsByPosition.Remove(move.From);
            }

            for (int i = 0; i < gravityMoves.Count; i++)
            {
                GravityMove move = gravityMoves[i];
                BlockView blockView = _viewsByBlock[move.Block];

                blockView.SetGridPosition(move.To);
                _viewsByPosition.Add(move.To, blockView);
            }
        }

        private void AppendSpecialMergeAnimation(
            Sequence turnSequence,
            IReadOnlyList<MergeViewEntry> entries)
        {
            if (entries.Count == 0)
            {
                return;
            }

            Sequence mergeSequence = DOTween.Sequence();
            float lastArrivalTime = 0f;

            for (int i = 0; i < entries.Count; i++)
            {
                MergeViewEntry entry = entries[i];
                float startDelay =
                    entry.Merge.MergeOrder *
                    _combinationMergeStagger;

                float arrivalTime =
                    startDelay +
                    _combinationMergeDuration;

                lastArrivalTime =
                    Mathf.Max(lastArrivalTime, arrivalTime);

                mergeSequence.Insert(
                    startDelay,
                    entry.SourceView.transform
                        .DOLocalMove(
                            CalculateLocalPosition(
                                entry.Merge.TargetPosition),
                            _combinationMergeDuration)
                        .SetEase(Ease.InCubic));
            }

            MergeViewEntry firstEntry = entries[0];

            mergeSequence.Insert(
                lastArrivalTime,
                firstEntry.TargetView.transform.DOPunchScale(
                    new Vector3(0.24f, 0.24f, 0f),
                    _combinationImpactDuration,
                    6,
                    0.65f));

            turnSequence.Append(mergeSequence);
        }

        private void AppendSpecialEffectAnimation(
            Sequence turnSequence,
            IReadOnlyList<TransformationViewEntry> transformationEntries,
            IReadOnlyList<ActivationViewEntry> activationEntries)
        {
            if (transformationEntries.Count == 0)
            {
                AppendFastActivationAnimation(
                    turnSequence,
                    activationEntries);
                return;
            }

            var transformedPositions =
                new HashSet<GridPosition>();

            for (int i = 0;
                 i < transformationEntries.Count;
                 i++)
            {
                transformedPositions.Add(
                    transformationEntries[i]
                        .Transformation.Position);
            }

            var preActivations =
                new List<ActivationViewEntry>();

            var transformedActivations =
                new List<ActivationViewEntry>();

            var postActivations =
                new List<ActivationViewEntry>();

            bool foundTransformedActivation = false;

            for (int i = 0;
                 i < activationEntries.Count;
                 i++)
            {
                ActivationViewEntry entry =
                    activationEntries[i];

                if (transformedPositions.Contains(
                        entry.Activation.Position))
                {
                    foundTransformedActivation = true;
                    transformedActivations.Add(entry);
                }
                else if (!foundTransformedActivation)
                {
                    preActivations.Add(entry);
                }
                else
                {
                    postActivations.Add(entry);
                }
            }

            AppendFastActivationAnimation(
                turnSequence,
                preActivations);

            AppendTransformationAnimation(
                turnSequence,
                transformationEntries);

            AppendTransformedActivationAnimation(
                turnSequence,
                transformedActivations);

            AppendFastActivationAnimation(
                turnSequence,
                postActivations);
        }

        private void AppendTransformationAnimation(
            Sequence turnSequence,
            IReadOnlyList<TransformationViewEntry> entries)
        {
            if (entries.Count == 0)
            {
                return;
            }

            Sequence sequence = DOTween.Sequence();

            for (int i = 0; i < entries.Count; i++)
            {
                TransformationViewEntry entry = entries[i];

                sequence.Insert(
                    i * _transformationStepDelay,
                    entry.View.CreateSpecialTransformationTween(
                        entry.Transformation.TransformedType));
            }

            turnSequence.Append(sequence);
        }

        private void AppendTransformedActivationAnimation(
            Sequence turnSequence,
            IReadOnlyList<ActivationViewEntry> entries)
        {
            if (entries.Count == 0)
            {
                return;
            }

            Sequence sequence = DOTween.Sequence();

            for (int i = 0; i < entries.Count; i++)
            {
                ActivationViewEntry entry = entries[i];

                sequence.Insert(
                    i * _transformedActivationStepDelay,
                    entry.View.CreateSpecialActivationTween(
                        entry.Activation.Type));
            }

            turnSequence.Append(sequence);
        }

        private void AppendFastActivationAnimation(
            Sequence turnSequence,
            IReadOnlyList<ActivationViewEntry> entries)
        {
            if (entries.Count == 0)
            {
                return;
            }

            Sequence sequence = DOTween.Sequence();
            float maximumTotalDelay = 0.06f;

            float stepDelay = entries.Count <= 1
                ? 0f
                : Mathf.Min(
                    _chainStepDelay,
                    maximumTotalDelay /
                    (entries.Count - 1));

            for (int i = 0; i < entries.Count; i++)
            {
                ActivationViewEntry entry = entries[i];

                sequence.Insert(
                    i * stepDelay,
                    entry.View.CreateSpecialActivationTween(
                        entry.Activation.Type));
            }

            turnSequence.Append(sequence);
        }

        private void AppendRemovalAnimation(
            Sequence turnSequence,
            IReadOnlyList<BlockView> removedViews)
        {
            if (removedViews.Count == 0)
            {
                return;
            }

            Sequence sequence = DOTween.Sequence();

            for (int i = 0; i < removedViews.Count; i++)
            {
                sequence.Join(
                    removedViews[i].transform
                        .DOScale(
                            Vector3.zero,
                            _removalDuration)
                        .SetEase(Ease.InBack));
            }

            turnSequence.Append(sequence);
            turnSequence.AppendCallback(
                () => DestroyRemovedViews(removedViews));
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
                () => entry.View.SetBlock(entry.Block));

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

            Sequence sequence = DOTween.Sequence();

            for (int i = 0; i < gravityMoves.Count; i++)
            {
                GravityMove move = gravityMoves[i];
                BlockView blockView = _viewsByBlock[move.Block];

                int distance = Mathf.Max(
                    1,
                    Mathf.Abs(move.From.Y - move.To.Y));

                float duration =
                    _gravityBaseDuration +
                    distance *
                    _gravityDurationPerCell;

                sequence.Join(
                    blockView.transform
                        .DOLocalMove(
                            CalculateLocalPosition(move.To),
                            duration)
                        .SetEase(Ease.InQuad));
            }

            turnSequence.Append(sequence);
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

            var spawnOrderByColumn =
                new Dictionary<int, int>();

            Sequence sequence = DOTween.Sequence();

            for (int i = 0; i < spawnedBlocks.Count; i++)
            {
                BlockSpawn spawn = spawnedBlocks[i];
                int column = spawn.Position.X;

                if (!spawnOrderByColumn.TryGetValue(
                        column,
                        out int spawnOrder))
                {
                    spawnOrder = 0;
                }

                spawnOrderByColumn[column] =
                    spawnOrder + 1;

                Vector3 targetPosition =
                    CalculateLocalPosition(spawn.Position);

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
                        startPosition.y -
                        targetPosition.y) /
                    _cellSize;

                float duration =
                    _spawnBaseDuration +
                    dropDistance *
                    _spawnDurationPerCell;

                sequence.Insert(
                    spawnOrder * _spawnStagger,
                    blockView.transform
                        .DOLocalMove(
                            targetPosition,
                            duration)
                        .SetEase(Ease.OutCubic));
            }

            _activeSequence = sequence;

            sequence.OnComplete(() =>
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
            ValidateBlockPrefab();

            BlockView blockView =
                Instantiate(_blockPrefab, BlockRoot);

            blockView.transform.localPosition =
                localPosition;

            blockView.transform.localRotation =
                Quaternion.identity;

            float blockScale = Mathf.Max(
                0.05f,
                _cellSize - _blockGap);

            blockView.transform.localScale =
                new Vector3(
                    blockScale,
                    blockScale,
                    1f);

            blockView.Initialize(position, block);

            _viewsByPosition.Add(position, blockView);
            _viewsByBlock.Add(block, blockView);

            return blockView;
        }

        private Vector3 CalculateLocalPosition(
            GridPosition position)
        {
            return new Vector3(
                CalculateStartX(_board.Width) +
                position.X * _cellSize,
                CalculateStartY(_board.Height) +
                position.Y * _cellSize,
                0f);
        }

        private Vector3 CalculateSpawnStartPosition(
            GridPosition targetPosition,
            int spawnOrder)
        {
            Vector3 targetLocalPosition =
                CalculateLocalPosition(targetPosition);

            float spawnY =
                CalculateStartY(_board.Height) +
                _board.Height * _cellSize +
                spawnOrder * _cellSize;

            return new Vector3(
                targetLocalPosition.x,
                spawnY,
                0f);
        }

        private void DestroyRemovedViews(
            IReadOnlyList<BlockView> removedViews)
        {
            for (int i = 0; i < removedViews.Count; i++)
            {
                BlockView blockView = removedViews[i];

                if (blockView != null)
                {
                    Destroy(blockView.gameObject);
                }
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

        private void ValidateReferences()
        {
            ValidateBlockPrefab();

            if (_obstacleViewCatalog == null)
            {
                throw new InvalidOperationException(
                    "ObstacleViewCatalog이 연결되지 않았습니다.");
            }

            _obstacleViewCatalog.ValidateOrThrow();
        }

        private void ValidateBlockPrefab()
        {
            if (_blockPrefab == null)
            {
                throw new InvalidOperationException(
                    "BlockView 프리팹이 연결되지 않았습니다.");
            }
        }

        private void ClearViews()
        {
            foreach (BlockView blockView in _viewsByBlock.Values)
            {
                DestroyView(blockView);
            }

            foreach (DamageableObstacleView obstacleView in _obstacleViews.Values)
            {
                DestroyView(obstacleView);
            }

            _viewsByPosition.Clear();
            _viewsByBlock.Clear();
            _obstacleViews.Clear();
        }

        private static void DestroyView(Component view)
        {
            if (view == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(view.gameObject);
            }
            else
            {
                DestroyImmediate(view.gameObject);
            }
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

        private readonly struct MergeViewEntry
        {
            public SpecialBlockMerge Merge { get; }
            public BlockView SourceView { get; }
            public BlockView TargetView { get; }

            public MergeViewEntry(
                SpecialBlockMerge merge,
                BlockView sourceView,
                BlockView targetView)
            {
                Merge = merge;
                SourceView = sourceView
                    ?? throw new ArgumentNullException(
                        nameof(sourceView));
                TargetView = targetView
                    ?? throw new ArgumentNullException(
                        nameof(targetView));
            }
        }

        private readonly struct TransformationViewEntry
        {
            public SpecialBlockTransformation Transformation { get; }
            public BlockView View { get; }

            public TransformationViewEntry(
                SpecialBlockTransformation transformation,
                BlockView view)
            {
                Transformation = transformation;
                View = view
                    ?? throw new ArgumentNullException(
                        nameof(view));
            }
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

        private readonly struct ObstacleChangeViewEntry
        {
            public DamageableObstacleView View { get; }
            public ObstacleStateChange Change { get; }

            public ObstacleChangeViewEntry(
                DamageableObstacleView view,
                ObstacleStateChange change)
            {
                View = view
                    ?? throw new ArgumentNullException(
                        nameof(view));
                Change = change;
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