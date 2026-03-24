using System;
using System.Collections.Generic;
using HappyFamily.Data;
using HappyFamily.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HappyFamily.UI
{
    public class TidyUpScreenRuntimeInitializer : MonoBehaviour
    {
        // 卡片宽高比 (宽:高) 约 256:341 ≈ 0.75
        private const float TileAspectRatio = 0.75f;
        // 横排固定7个（和收纳栏槽位数一致）
        private const int TilesPerRow = 7;

        private TidyUpBoard _board;
        private TidyUpLevelDefinition _levelDefinition;
        private RectTransform _tilesContainer;
        private RectTransform _slotsContainer;
        private Button _backButton;
        private TextMeshProUGUI _titleLabel;
        private TextMeshProUGUI _statusLabel;
        private Action _onBack;
        private Action<bool> _onLevelComplete;

        private List<TileView> _tileViews = new List<TileView>();
        private List<SlotView> _slotViews = new List<SlotView>();
        private Dictionary<string, Sprite> _spriteCache = new Dictionary<string, Sprite>();

        // 统一的卡片尺寸（游戏区和收纳栏共用）
        private float _tileWidth;
        private float _tileHeight;

        private class TileView
        {
            public GameObject Root;
            public int TileIndex;
            public Image SpriteImage;
            public Button Button;
        }

        private class SlotView
        {
            public GameObject Root;
            public Image BgImage;
            public Image ItemImage;
        }

        public void Initialize(
            TidyUpLevelDefinition levelDefinition,
            RectTransform tilesContainer,
            RectTransform slotsContainer,
            Button backButton,
            TextMeshProUGUI titleLabel,
            TextMeshProUGUI statusLabel,
            Action onBack,
            Action<bool> onLevelComplete)
        {
            _levelDefinition = levelDefinition;
            _tilesContainer = tilesContainer;
            _slotsContainer = slotsContainer;
            _backButton = backButton;
            _titleLabel = titleLabel;
            _statusLabel = statusLabel;
            _onBack = onBack;
            _onLevelComplete = onLevelComplete;

            LoadSprites();
            CalculateTileSize();
            SetupHeader();
            SetupBoard();
            SetupSlots();
            SetupBackButton();
            UpdateStatus();
        }

        private void LoadSprites()
        {
            _spriteCache.Clear();
            foreach (var item in _levelDefinition.Items)
            {
                var sprite = Resources.Load<Sprite>(item.SpritePath);
                if (sprite != null)
                {
                    _spriteCache[item.ItemId] = sprite;
                }
            }
        }

        private void CalculateTileSize()
        {
            // 基于游戏区计算卡片尺寸
            var tilesRect = _tilesContainer.rect;
            var padding = 30f;

            // 考虑层叠偏移
            var layerOffsetTotal = 10f * Mathf.Max(0, _levelDefinition.MaxLayers - 1);

            var availableWidth = tilesRect.width - padding * 2 - layerOffsetTotal;
            var availableHeight = tilesRect.height - padding * 2 - layerOffsetTotal;

            // 基于网格计算最大卡片尺寸
            var maxTileWidth = availableWidth / _levelDefinition.GridWidth;
            var maxTileHeight = availableHeight / _levelDefinition.GridHeight;

            // 保持宽高比，取能放下的尺寸
            var widthFromHeight = maxTileHeight * TileAspectRatio;

            _tileWidth = Mathf.Min(maxTileWidth, widthFromHeight);
            _tileHeight = _tileWidth / TileAspectRatio;

            // 设置合理的最小和最大值
            _tileWidth = Mathf.Clamp(_tileWidth, 60f, 150f);
            _tileHeight = _tileWidth / TileAspectRatio;
        }

        private void SetupHeader()
        {
            if (_titleLabel != null)
            {
                _titleLabel.text = _levelDefinition.DisplayName;
            }
        }

        private void SetupBackButton()
        {
            if (_backButton != null)
            {
                _backButton.onClick.RemoveAllListeners();
                _backButton.onClick.AddListener(() => _onBack?.Invoke());
            }
        }

        private void SetupBoard()
        {
            foreach (var view in _tileViews)
            {
                if (view.Root != null) Destroy(view.Root);
            }
            _tileViews.Clear();

            _board = TidyUpBoard.Create(_levelDefinition);

            // 层叠偏移
            var layerOffset = new Vector2(8f, -8f);

            // 排序tiles按层级
            var sortedTiles = new List<StackedTile>(_board.Tiles);
            sortedTiles.Sort((a, b) => a.Layer.CompareTo(b.Layer));

            foreach (var tile in sortedTiles)
            {
                var view = CreateTileView(tile, layerOffset);
                _tileViews.Add(view);
            }

            UpdateTileVisuals();
        }

        private TileView CreateTileView(StackedTile tile, Vector2 layerOffset)
        {
            var view = new TileView { TileIndex = tile.Index };

            view.Root = new GameObject($"Tile_{tile.Index}");
            view.Root.transform.SetParent(_tilesContainer, false);

            var rectTransform = view.Root.AddComponent<RectTransform>();

            // 计算网格中心位置
            var gridCenterX = (_levelDefinition.GridWidth - 1) / 2f;
            var gridCenterY = (_levelDefinition.GridHeight - 1) / 2f;
            var baseX = (tile.GridX - gridCenterX) * _tileWidth;
            var baseY = (tile.GridY - gridCenterY) * _tileHeight;

            rectTransform.anchoredPosition = new Vector2(
                baseX + tile.Layer * layerOffset.x,
                baseY + tile.Layer * layerOffset.y
            );
            rectTransform.sizeDelta = new Vector2(_tileWidth - 2f, _tileHeight - 2f);

            // 直接在Root上显示素材
            view.SpriteImage = view.Root.AddComponent<Image>();
            view.SpriteImage.preserveAspect = true;
            view.SpriteImage.raycastTarget = true;

            if (_spriteCache.TryGetValue(tile.ItemId, out var sprite))
            {
                view.SpriteImage.sprite = sprite;
                view.SpriteImage.color = Color.white;
            }
            else
            {
                // 找不到素材时显示半透明占位
                view.SpriteImage.sprite = null;
                view.SpriteImage.color = new Color(0.7f, 0.68f, 0.65f, 0.5f);
            }

            // 按钮交互
            view.Button = view.Root.AddComponent<Button>();
            view.Button.targetGraphic = view.SpriteImage;
            view.Button.transition = Selectable.Transition.None;

            var capturedIndex = tile.Index;
            view.Button.onClick.AddListener(() => OnTileClicked(capturedIndex));

            return view;
        }

        private void SetupSlots()
        {
            foreach (var view in _slotViews)
            {
                if (view.Root != null) Destroy(view.Root);
            }
            _slotViews.Clear();

            // 收纳栏的卡片尺寸：基于收纳栏宽度，确保7个能放下
            var slotsRect = _slotsContainer.rect;
            var slotSpacing = 4f;
            var slotTileWidth = (slotsRect.width - 20f - slotSpacing * (TidyUpBoard.SlotCount - 1)) / TidyUpBoard.SlotCount;
            var slotTileHeight = slotTileWidth / TileAspectRatio;

            // 如果收纳栏高度不够，按高度缩放
            var maxSlotHeight = slotsRect.height - 10f;
            if (slotTileHeight > maxSlotHeight)
            {
                slotTileHeight = maxSlotHeight;
                slotTileWidth = slotTileHeight * TileAspectRatio;
            }

            var totalWidth = TidyUpBoard.SlotCount * slotTileWidth + (TidyUpBoard.SlotCount - 1) * slotSpacing;
            var startX = -totalWidth / 2f + slotTileWidth / 2f;

            for (var i = 0; i < TidyUpBoard.SlotCount; i++)
            {
                var view = new SlotView();

                view.Root = new GameObject($"Slot_{i}");
                view.Root.transform.SetParent(_slotsContainer, false);

                var rectTransform = view.Root.AddComponent<RectTransform>();
                rectTransform.anchoredPosition = new Vector2(startX + i * (slotTileWidth + slotSpacing), 0f);
                rectTransform.sizeDelta = new Vector2(slotTileWidth, slotTileHeight);

                // 空槽位背景
                view.BgImage = view.Root.AddComponent<Image>();
                view.BgImage.color = new Color(0.82f, 0.80f, 0.76f, 0.4f);

                // 物品图片（子对象）
                var itemObj = new GameObject("Item");
                itemObj.transform.SetParent(view.Root.transform, false);
                var itemRect = itemObj.AddComponent<RectTransform>();
                itemRect.anchorMin = Vector2.zero;
                itemRect.anchorMax = Vector2.one;
                itemRect.offsetMin = Vector2.zero;
                itemRect.offsetMax = Vector2.zero;

                view.ItemImage = itemObj.AddComponent<Image>();
                view.ItemImage.preserveAspect = true;
                view.ItemImage.raycastTarget = false;
                view.ItemImage.enabled = false;

                _slotViews.Add(view);
            }
        }

        private void OnTileClicked(int tileIndex)
        {
            if (_board == null) return;

            var result = _board.TryPickTile(tileIndex);

            switch (result.State)
            {
                case TidyUpMoveState.None:
                    break;

                case TidyUpMoveState.Picked:
                case TidyUpMoveState.Matched:
                case TidyUpMoveState.SlotsFull:
                    UpdateTileVisuals();
                    UpdateSlotVisuals();
                    UpdateStatus();

                    if (_board.IsCompleted)
                    {
                        _onLevelComplete?.Invoke(true);
                    }
                    else if (_board.IsFailed)
                    {
                        _onLevelComplete?.Invoke(false);
                    }
                    break;
            }
        }

        private void UpdateTileVisuals()
        {
            foreach (var view in _tileViews)
            {
                if (view.Root == null) continue;

                var tileIndex = view.TileIndex;
                if (tileIndex < 0 || tileIndex >= _board.Tiles.Count) continue;

                var tile = _board.Tiles[tileIndex];

                view.Root.SetActive(!tile.IsRemoved);
                if (tile.IsRemoved) continue;

                var canPick = _board.CanPickTile(tileIndex);
                view.Button.interactable = canPick;

                // 被遮挡的卡片显示半透明
                if (view.SpriteImage != null)
                {
                    view.SpriteImage.color = canPick ? Color.white : new Color(0.6f, 0.6f, 0.6f, 0.7f);
                }
            }
        }

        private void UpdateSlotVisuals()
        {
            for (var i = 0; i < _board.Slots.Length && i < _slotViews.Count; i++)
            {
                var slot = _board.Slots[i];
                var view = _slotViews[i];

                if (view.ItemImage == null) continue;

                if (slot.IsEmpty)
                {
                    view.ItemImage.enabled = false;
                    view.BgImage.color = new Color(0.82f, 0.80f, 0.76f, 0.4f);
                }
                else
                {
                    view.ItemImage.enabled = true;
                    view.BgImage.color = new Color(0.92f, 0.90f, 0.86f);
                    if (_spriteCache.TryGetValue(slot.ItemId, out var sprite))
                    {
                        view.ItemImage.sprite = sprite;
                    }
                }
            }
        }

        private void UpdateStatus()
        {
            if (_statusLabel == null || _board == null) return;

            var remaining = 0;
            foreach (var tile in _board.Tiles)
            {
                if (!tile.IsRemoved) remaining++;
            }

            _statusLabel.text = $"剩余: {remaining}";
        }

        private void OnDestroy()
        {
            foreach (var view in _tileViews)
            {
                if (view.Root != null) Destroy(view.Root);
            }
            _tileViews.Clear();

            foreach (var view in _slotViews)
            {
                if (view.Root != null) Destroy(view.Root);
            }
            _slotViews.Clear();
        }
    }
}
