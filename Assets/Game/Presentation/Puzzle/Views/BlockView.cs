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

            EnsureSpriteRenderer();
            EnsureSpecialIndicator();

            ApplyVisual();
            RefreshObjectName();
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
            ApplyMainColor();
            ApplySpecialIndicator();
        }

        private void ApplyMainColor()
        {
            if (Block.Type == BlockType.ColorBomb)
            {
                _spriteRenderer.color =
                    new Color32(
                        35,
                        38,
                        52,
                        255);

                return;
            }

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

        private void ApplySpecialIndicator()
        {
            bool showIndicator =
                Block.Type != BlockType.Normal;

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

            switch (Block.Type)
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
                        nameof(Block.Type),
                        Block.Type,
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