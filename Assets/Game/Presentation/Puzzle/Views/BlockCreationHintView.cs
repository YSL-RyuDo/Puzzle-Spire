using System;
using System.Collections.Generic;
using PuzzleSpire.Domain.Puzzle;
using UnityEngine;

namespace PuzzleSpire.Presentation.Puzzle
{
    [DisallowMultipleComponent]
    public sealed class BlockCreationHintView : MonoBehaviour
    {
        private const string HintRootName = "CreationHint";
        private const string ShadowName = "HintShadow";
        private const string HighlightName = "HintHighlight";

        private static readonly Dictionary<BlockType, Sprite>
            GeneratedSprites =
                new Dictionary<BlockType, Sprite>();

        [Header("Optional Hint Sprites")]

        [SerializeField]
        private Sprite _rocketHintSprite;

        [SerializeField]
        private Sprite _bombHintSprite;

        [SerializeField]
        private Sprite _bearJellyHintSprite;

        [Header("Emboss Style")]

        [SerializeField]
        [Range(0.2f, 0.8f)]
        private float _hintScale = 0.52f;

        [SerializeField]
        private Color _shadowColor =
            new Color(0.02f, 0.04f, 0.08f, 0.28f);

        [SerializeField]
        private Color _highlightColor =
            new Color(1f, 1f, 1f, 0.16f);

        private SpriteRenderer _mainRenderer;
        private Transform _hintRoot;
        private SpriteRenderer _shadowRenderer;
        private SpriteRenderer _highlightRenderer;

        public void Show(BlockType blockType)
        {
            if (blockType == BlockType.Normal)
            {
                Hide();
                return;
            }

            EnsureRenderers();

            Sprite hintSprite =
                ResolveHintSprite(blockType);

            _hintRoot.gameObject.SetActive(true);

            _hintRoot.localPosition =
                new Vector3(0f, 0f, -0.1f);

            _hintRoot.localRotation =
                Quaternion.identity;

            _hintRoot.localScale =
                new Vector3(
                    _hintScale,
                    _hintScale,
                    1f);

            _shadowRenderer.sprite = hintSprite;
            _shadowRenderer.color = _shadowColor;

            _shadowRenderer.transform.localPosition =
                new Vector3(
                    0.018f,
                    -0.018f,
                    0f);

            _shadowRenderer.transform.localRotation =
                Quaternion.identity;

            _shadowRenderer.transform.localScale =
                Vector3.one;

            _highlightRenderer.sprite = hintSprite;
            _highlightRenderer.color = _highlightColor;

            _highlightRenderer.transform.localPosition =
                new Vector3(
                    -0.018f,
                    0.018f,
                    -0.01f);

            _highlightRenderer.transform.localRotation =
                Quaternion.identity;

            _highlightRenderer.transform.localScale =
                Vector3.one;
        }

        public void Hide()
        {
            if (_hintRoot != null)
            {
                _hintRoot.gameObject.SetActive(false);
            }
        }

        private Sprite ResolveHintSprite(
            BlockType blockType)
        {
            switch (blockType)
            {
                case BlockType.HorizontalRocket:
                case BlockType.VerticalRocket:
                    return _rocketHintSprite != null
                        ? _rocketHintSprite
                        : GetOrCreateGeneratedSprite(
                            BlockType.HorizontalRocket);

                case BlockType.Bomb:
                    return _bombHintSprite != null
                        ? _bombHintSprite
                        : GetOrCreateGeneratedSprite(
                            BlockType.Bomb);

                case BlockType.ColorBomb:
                    return _bearJellyHintSprite != null
                        ? _bearJellyHintSprite
                        : GetOrCreateGeneratedSprite(
                            BlockType.ColorBomb);

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(blockType),
                        blockType,
                        "Unsupported hint block type.");
            }
        }

        private void EnsureRenderers()
        {
            if (_hintRoot != null)
            {
                return;
            }

            _mainRenderer =
                GetComponent<SpriteRenderer>();

            if (_mainRenderer == null)
            {
                throw new InvalidOperationException(
                    "BlockCreationHintView requires SpriteRenderer.");
            }

            Transform existingRoot =
                transform.Find(HintRootName);

            if (existingRoot == null)
            {
                GameObject rootObject =
                    new GameObject(HintRootName);

                rootObject.transform.SetParent(
                    transform,
                    false);

                existingRoot = rootObject.transform;
            }

            _hintRoot = existingRoot;

            _shadowRenderer =
                GetOrCreateRenderer(
                    _hintRoot,
                    ShadowName,
                    _mainRenderer.sortingOrder + 2);

            _highlightRenderer =
                GetOrCreateRenderer(
                    _hintRoot,
                    HighlightName,
                    _mainRenderer.sortingOrder + 3);

            _hintRoot.gameObject.SetActive(false);
        }

        private SpriteRenderer GetOrCreateRenderer(
            Transform parent,
            string objectName,
            int sortingOrder)
        {
            Transform child =
                parent.Find(objectName);

            if (child == null)
            {
                GameObject childObject =
                    new GameObject(objectName);

                childObject.transform.SetParent(
                    parent,
                    false);

                child = childObject.transform;
            }

            SpriteRenderer renderer =
                child.GetComponent<SpriteRenderer>();

            if (renderer == null)
            {
                renderer =
                    child.gameObject
                        .AddComponent<SpriteRenderer>();
            }

            renderer.sortingLayerID =
                _mainRenderer.sortingLayerID;

            renderer.sortingOrder =
                sortingOrder;

            return renderer;
        }

        private static Sprite GetOrCreateGeneratedSprite(
            BlockType blockType)
        {
            BlockType cacheKey =
                blockType == BlockType.VerticalRocket
                    ? BlockType.HorizontalRocket
                    : blockType;

            if (GeneratedSprites.TryGetValue(
                    cacheKey,
                    out Sprite existingSprite))
            {
                return existingSprite;
            }

            const int textureSize = 32;

            Texture2D texture =
                new Texture2D(
                    textureSize,
                    textureSize,
                    TextureFormat.RGBA32,
                    false);

            texture.name =
                $"GeneratedHint_{cacheKey}";

            texture.filterMode =
                FilterMode.Bilinear;

            texture.wrapMode =
                TextureWrapMode.Clamp;

            texture.hideFlags =
                HideFlags.HideAndDontSave;

            Color32[] pixels =
                new Color32[
                    textureSize * textureSize];

            Color32 clear =
                new Color32(0, 0, 0, 0);

            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = clear;
            }

            Color32 solid =
                new Color32(255, 255, 255, 255);

            switch (cacheKey)
            {
                case BlockType.HorizontalRocket:
                    DrawRocket(
                        pixels,
                        textureSize,
                        solid);
                    break;

                case BlockType.Bomb:
                    DrawBomb(
                        pixels,
                        textureSize,
                        solid);
                    break;

                case BlockType.ColorBomb:
                    DrawBearJelly(
                        pixels,
                        textureSize,
                        solid);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(blockType),
                        blockType,
                        "Unsupported generated hint type.");
            }

            texture.SetPixels32(pixels);
            texture.Apply();

            Sprite sprite =
                Sprite.Create(
                    texture,
                    new Rect(
                        0f,
                        0f,
                        textureSize,
                        textureSize),
                    new Vector2(0.5f, 0.5f),
                    textureSize);

            sprite.name =
                $"GeneratedHint_{cacheKey}";

            sprite.hideFlags =
                HideFlags.HideAndDontSave;

            GeneratedSprites.Add(
                cacheKey,
                sprite);

            return sprite;
        }

        private static void DrawRocket(
            Color32[] pixels,
            int size,
            Color32 color)
        {
            FillRectangle(
                pixels,
                size,
                7,
                13,
                20,
                18,
                color);

            for (int x = 21; x <= 27; x++)
            {
                int distance =
                    27 - x;

                int minimumY =
                    15 - distance;

                int maximumY =
                    16 + distance;

                for (int y = minimumY;
                     y <= maximumY;
                     y++)
                {
                    SetPixel(
                        pixels,
                        size,
                        x,
                        y,
                        color);
                }
            }

            FillRectangle(
                pixels,
                size,
                4,
                10,
                9,
                13,
                color);

            FillRectangle(
                pixels,
                size,
                4,
                18,
                9,
                21,
                color);
        }

        private static void DrawBomb(
            Color32[] pixels,
            int size,
            Color32 color)
        {
            FillCircle(
                pixels,
                size,
                15,
                14,
                8,
                color);

            for (int i = 0; i < 7; i++)
            {
                SetPixel(
                    pixels,
                    size,
                    20 + i,
                    20 + i,
                    color);

                SetPixel(
                    pixels,
                    size,
                    20 + i,
                    21 + i,
                    color);
            }

            FillCircle(
                pixels,
                size,
                27,
                27,
                2,
                color);
        }

        private static void DrawBearJelly(
            Color32[] pixels,
            int size,
            Color32 color)
        {
            FillCircle(
                pixels,
                size,
                16,
                16,
                8,
                color);

            FillCircle(
                pixels,
                size,
                9,
                23,
                4,
                color);

            FillCircle(
                pixels,
                size,
                23,
                23,
                4,
                color);

            FillCircle(
                pixels,
                size,
                16,
                8,
                6,
                color);
        }

        private static void FillRectangle(
            Color32[] pixels,
            int size,
            int minimumX,
            int minimumY,
            int maximumX,
            int maximumY,
            Color32 color)
        {
            for (int x = minimumX;
                 x <= maximumX;
                 x++)
            {
                for (int y = minimumY;
                     y <= maximumY;
                     y++)
                {
                    SetPixel(
                        pixels,
                        size,
                        x,
                        y,
                        color);
                }
            }
        }

        private static void FillCircle(
            Color32[] pixels,
            int size,
            int centerX,
            int centerY,
            int radius,
            Color32 color)
        {
            int radiusSquared =
                radius * radius;

            for (int x = centerX - radius;
                 x <= centerX + radius;
                 x++)
            {
                for (int y = centerY - radius;
                     y <= centerY + radius;
                     y++)
                {
                    int differenceX =
                        x - centerX;

                    int differenceY =
                        y - centerY;

                    int distanceSquared =
                        differenceX * differenceX
                        + differenceY * differenceY;

                    if (distanceSquared > radiusSquared)
                    {
                        continue;
                    }

                    SetPixel(
                        pixels,
                        size,
                        x,
                        y,
                        color);
                }
            }
        }

        private static void SetPixel(
            Color32[] pixels,
            int size,
            int x,
            int y,
            Color32 color)
        {
            if (x < 0
                || x >= size
                || y < 0
                || y >= size)
            {
                return;
            }

            pixels[y * size + x] =
                color;
        }

        private void OnDisable()
        {
            Hide();
        }
    }
}