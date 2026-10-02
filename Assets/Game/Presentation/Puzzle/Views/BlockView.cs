using System;
using DG.Tweening;
using PuzzleSpire.Domain.Puzzle;
using UnityEngine;

namespace PuzzleSpire.Presentation.Puzzle
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class BlockView : MonoBehaviour
    {
        private const string IndicatorObjectName =
            "SpecialIndicator";

        [SerializeField]
        private SpriteRenderer _spriteRenderer;

        private SpriteRenderer _specialIndicator;
        private BlockType? _previewType;

        public GridPosition Position { get; private set; }

        public Block Block { get; private set; }

        private void Awake()
        {
            EnsureSpriteRenderer();
            EnsureSpecialIndicator();
        }

        public void Initialize(
            GridPosition position,
            Block block)
        {
            Position = position;
            SetBlock(block);
        }

        public void SetGridPosition(
            GridPosition position)
        {
            Position = position;
            RefreshObjectName();
        }

        public void SetBlock(Block block)
        {
            Block = block
                ?? throw new ArgumentNullException(
                    nameof(block));

            _previewType = null;

            EnsureSpriteRenderer();
            EnsureSpecialIndicator();

            ApplyVisual();
            RefreshObjectName();
        }

        public void PreviewSpecialType(
            BlockType specialType)
        {
            if (Block == null)
            {
                throw new InvalidOperationException(
                    "BlockView is not initialized.");
            }

            if (specialType == BlockType.Normal)
            {
                throw new ArgumentException(
                    "Preview type must be a special block.",
                    nameof(specialType));
            }

            _previewType = specialType;
            ApplyVisual();
        }

        public Tween CreateSpecialTransformationTween(
            BlockType transformedType)
        {
            Sequence sequence =
                DOTween.Sequence();

            sequence.AppendCallback(
                () => PreviewSpecialType(transformedType));

            switch (transformedType)
            {
                case BlockType.HorizontalRocket:
                    sequence.Append(
                        transform.DOPunchScale(
                            new Vector3(
                                0.26f,
                                0.035f,
                                0f),
                            0.11f,
                            5,
                            0.7f));
                    break;

                case BlockType.VerticalRocket:
                    sequence.Append(
                        transform.DOPunchScale(
                            new Vector3(
                                0.035f,
                                0.26f,
                                0f),
                            0.11f,
                            5,
                            0.7f));
                    break;

                case BlockType.Bomb:
                    sequence.Join(
                        transform.DOPunchScale(
                            new Vector3(
                                0.18f,
                                0.18f,
                                0f),
                            0.13f,
                            5,
                            0.65f));

                    sequence.Join(
                        transform.DOPunchRotation(
                            new Vector3(
                                0f,
                                0f,
                                12f),
                            0.13f,
                            5,
                            0.65f));
                    break;

                case BlockType.ColorBomb:
                    sequence.Join(
                        transform.DORotate(
                            new Vector3(
                                0f,
                                0f,
                                180f),
                            0.16f,
                            RotateMode.FastBeyond360)
                            .SetRelative());

                    sequence.Join(
                        transform.DOPunchScale(
                            new Vector3(
                                0.24f,
                                0.24f,
                                0f),
                            0.16f,
                            5,
                            0.6f));
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(transformedType),
                        transformedType,
                        "Unsupported transformed block type.");
            }

            return sequence;
        }

        public Tween CreateSpecialCreationTween(
            BlockType blockType)
        {
            Sequence sequence = DOTween.Sequence();

            switch (blockType)
            {
                case BlockType.HorizontalRocket:
                    sequence.Append(
                        transform.DOPunchScale(
                            new Vector3(
                                0.42f,
                                0.05f,
                                0f),
                            0.32f,
                            8,
                            0.65f));
                    break;

                case BlockType.VerticalRocket:
                    sequence.Append(
                        transform.DOPunchScale(
                            new Vector3(
                                0.05f,
                                0.42f,
                                0f),
                            0.32f,
                            8,
                            0.65f));
                    break;

                case BlockType.Bomb:
                    sequence.Join(
                        transform.DOPunchScale(
                            new Vector3(
                                0.30f,
                                0.30f,
                                0f),
                            0.38f,
                            10,
                            0.6f));

                    sequence.Join(
                        transform.DOPunchRotation(
                            new Vector3(
                                0f,
                                0f,
                                24f),
                            0.38f,
                            10,
                            0.6f));
                    break;

                case BlockType.ColorBomb:
                    sequence.Join(
                        transform.DORotate(
                            new Vector3(
                                0f,
                                0f,
                                360f),
                            0.48f,
                            RotateMode.FastBeyond360)
                            .SetRelative()
                            .SetEase(Ease.OutBack));

                    sequence.Join(
                        transform.DOPunchScale(
                            new Vector3(
                                0.42f,
                                0.42f,
                                0f),
                            0.48f,
                            10,
                            0.55f));
                    break;

                case BlockType.Normal:
                    sequence.AppendInterval(0.01f);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(blockType),
                        blockType,
                        "Unknown block type.");
            }

            return sequence;
        }

        public Tween CreateSpecialActivationTween(
            BlockType blockType)
        {
            Sequence sequence =
                DOTween.Sequence();

            switch (blockType)
            {
                case BlockType.HorizontalRocket:
                    sequence.Append(
                        transform.DOPunchScale(
                            new Vector3(
                                0.28f,
                                0.025f,
                                0f),
                            0.10f,
                            4,
                            0.7f));
                    break;

                case BlockType.VerticalRocket:
                    sequence.Append(
                        transform.DOPunchScale(
                            new Vector3(
                                0.025f,
                                0.28f,
                                0f),
                            0.10f,
                            4,
                            0.7f));
                    break;

                case BlockType.Bomb:
                    sequence.Append(
                        transform.DOPunchScale(
                            new Vector3(
                                0.22f,
                                0.22f,
                                0f),
                            0.13f,
                            5,
                            0.65f));

                    sequence.Join(
                        transform.DOPunchRotation(
                            new Vector3(
                                0f,
                                0f,
                                18f),
                            0.13f,
                            5,
                            0.65f));
                    break;

                case BlockType.ColorBomb:
                    sequence.Append(
                        transform.DORotate(
                            new Vector3(
                                0f,
                                0f,
                                180f),
                            0.16f,
                            RotateMode.FastBeyond360)
                            .SetRelative()
                            .SetEase(Ease.OutCubic));

                    sequence.Join(
                        transform.DOPunchScale(
                            new Vector3(
                                0.30f,
                                0.30f,
                                0f),
                            0.16f,
                            5,
                            0.6f));
                    break;

                case BlockType.Normal:
                    sequence.AppendInterval(0.01f);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(blockType),
                        blockType,
                        "Unknown block type.");
            }

            return sequence;
        }

        private void ApplyVisual()
        {
            BlockType displayedType =
                _previewType ?? Block.Type;

            ApplyMainColor(displayedType);
            ApplySpecialIndicator(displayedType);
        }

        private void ApplyMainColor(
            BlockType displayedType)
        {
            switch (displayedType)
            {
                case BlockType.Normal:
                    ApplyNormalBlockColor();
                    return;

                case BlockType.HorizontalRocket:
                case BlockType.VerticalRocket:
                    _spriteRenderer.color =
                        new Color32(
                            70,
                            190,
                            235,
                            255);
                    return;

                case BlockType.Bomb:
                    _spriteRenderer.color =
                        new Color32(
                            88,
                            72,
                            58,
                            255);
                    return;

                case BlockType.ColorBomb:
                    _spriteRenderer.color =
                        new Color32(
                            35,
                            38,
                            52,
                            255);
                    return;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(displayedType),
                        displayedType,
                        "Unknown block type.");
            }
        }

        private void ApplyNormalBlockColor()
        {
            switch (Block.Color)
            {
                case BlockColor.Red:
                    _spriteRenderer.color =
                        new Color32(235, 82, 82, 255);
                    break;

                case BlockColor.Blue:
                    _spriteRenderer.color =
                        new Color32(72, 145, 235, 255);
                    break;

                case BlockColor.Green:
                    _spriteRenderer.color =
                        new Color32(82, 190, 112, 255);
                    break;

                case BlockColor.Yellow:
                    _spriteRenderer.color =
                        new Color32(245, 199, 72, 255);
                    break;

                case BlockColor.Purple:
                    _spriteRenderer.color =
                        new Color32(164, 99, 220, 255);
                    break;

                default:
                    _spriteRenderer.color = Color.white;
                    break;
            }
        }

        private void ApplySpecialIndicator(
            BlockType displayedType)
        {
            bool showIndicator =
                displayedType != BlockType.Normal;

            _specialIndicator.gameObject.SetActive(
                showIndicator);

            if (!showIndicator)
            {
                return;
            }

            _specialIndicator.transform.localPosition =
                Vector3.zero;

            _specialIndicator.transform.localRotation =
                Quaternion.identity;

            switch (displayedType)
            {
                case BlockType.HorizontalRocket:
                    _specialIndicator.color =
                        Color.white;

                    _specialIndicator.transform.localScale =
                        new Vector3(
                            0.76f,
                            0.14f,
                            1f);
                    break;

                case BlockType.VerticalRocket:
                    _specialIndicator.color =
                        Color.white;

                    _specialIndicator.transform.localScale =
                        new Vector3(
                            0.14f,
                            0.76f,
                            1f);
                    break;

                case BlockType.Bomb:
                    _specialIndicator.color =
                        new Color32(
                            255,
                            224,
                            92,
                            255);

                    _specialIndicator.transform.localScale =
                        new Vector3(
                            0.42f,
                            0.42f,
                            1f);

                    _specialIndicator.transform.localRotation =
                        Quaternion.Euler(
                            0f,
                            0f,
                            45f);
                    break;

                case BlockType.ColorBomb:
                    _specialIndicator.color =
                        new Color32(
                            95,
                            235,
                            255,
                            255);

                    _specialIndicator.transform.localScale =
                        new Vector3(
                            0.56f,
                            0.56f,
                            1f);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(displayedType),
                        displayedType,
                        "Unknown block type.");
            }
        }

        private void EnsureSpriteRenderer()
        {
            if (_spriteRenderer == null)
            {
                _spriteRenderer =
                    GetComponent<SpriteRenderer>();
            }

            if (_spriteRenderer == null)
            {
                throw new InvalidOperationException(
                    "SpriteRenderer is missing.");
            }
        }

        private void EnsureSpecialIndicator()
        {
            if (_specialIndicator != null)
            {
                return;
            }

            Transform existingIndicator =
                transform.Find(
                    IndicatorObjectName);

            if (existingIndicator != null)
            {
                _specialIndicator =
                    existingIndicator
                        .GetComponent<SpriteRenderer>();
            }

            if (_specialIndicator == null)
            {
                GameObject indicatorObject =
                    new GameObject(
                        IndicatorObjectName);

                indicatorObject.transform.SetParent(
                    transform,
                    false);

                _specialIndicator =
                    indicatorObject.AddComponent<
                        SpriteRenderer>();
            }

            _specialIndicator.sprite =
                _spriteRenderer.sprite;

            _specialIndicator.sortingLayerID =
                _spriteRenderer.sortingLayerID;

            _specialIndicator.sortingOrder =
                _spriteRenderer.sortingOrder + 1;

            _specialIndicator.gameObject.SetActive(false);
        }

        private void RefreshObjectName()
        {
            if (Block == null)
            {
                return;
            }

            gameObject.name =
                $"Block_{Block.Type}_" +
                $"{Position.X}_{Position.Y}";
        }

        private void OnValidate()
        {
            if (_spriteRenderer == null)
            {
                _spriteRenderer =
                    GetComponent<SpriteRenderer>();
            }
        }
    }
}