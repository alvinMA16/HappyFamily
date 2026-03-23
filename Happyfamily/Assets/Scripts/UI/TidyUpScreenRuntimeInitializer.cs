using System;
using System.Collections.Generic;
using HappyFamily.Data;
using HappyFamily.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HappyFamily.UI
{
    /// <summary>
    /// Runtime initializer for TidyUpScreenShell when creating UI dynamically.
    /// </summary>
    public class TidyUpScreenRuntimeInitializer : MonoBehaviour
    {
        private TidyUpBoard _board;
        private TidyUpLevelDefinition _levelDefinition;
        private RectTransform _tilesContainer;
        private RectTransform _slotsContainer;
        private Button _backButton;
        private TextMeshProUGUI _statusLabel;
        private Action _onBack;
        private Action<bool> _onLevelComplete;

        private List<GameObject> _tileObjects = new List<GameObject>();
        private List<Image> _slotImages = new List<Image>();
        private Dictionary<string, Sprite> _spriteCache = new Dictionary<string, Sprite>();

        public void Initialize(TidyUpLevelDefinition levelDefinition,
            RectTransform tilesContainer, RectTransform slotsContainer, Button backButton, TextMeshProUGUI statusLabel,
            Action onBack, Action<bool> onLevelComplete)
        {
            _levelDefinition = levelDefinition;
            _tilesContainer = tilesContainer;
            _slotsContainer = slotsContainer;
            _backButton = backButton;
            _statusLabel = statusLabel;
            _onBack = onBack;
            _onLevelComplete = onLevelComplete;

            LoadSprites();
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
                else
                {
                    Debug.LogWarning($"Failed to load sprite: {item.SpritePath}");
                }
            }
        }

        private void SetupBoard()
        {
            foreach (var obj in _tileObjects)
            {
                if (obj != null) Destroy(obj);
            }
            _tileObjects.Clear();

            _board = TidyUpBoard.Create(_levelDefinition);

            var containerRect = _tilesContainer.rect;
            // Calculate tile size based on 3:4 aspect ratio (width:height)
            var maxTileWidth = containerRect.width / (_levelDefinition.GridWidth + 0.5f);
            var maxTileHeight = containerRect.height / (_levelDefinition.GridHeight + 0.5f);
            // For 3:4 ratio, height = width * 4/3
            var tileWidth = Mathf.Min(maxTileWidth, maxTileHeight * 0.75f, 100f);
            var tileHeight = tileWidth * 4f / 3f;

            var layerOffset = new Vector2(12f, -12f);

            foreach (var tile in _board.Tiles)
            {
                var tileObj = CreateTileObject(tile, tileWidth, tileHeight, layerOffset);
                _tileObjects.Add(tileObj);
            }

            UpdateTileVisuals();
        }

        private GameObject CreateTileObject(StackedTile tile, float tileWidth, float tileHeight, Vector2 layerOffset)
        {
            var tileObj = new GameObject($"Tile_{tile.Index}");
            tileObj.transform.SetParent(_tilesContainer, false);

            var rectTransform = tileObj.AddComponent<RectTransform>();
            var baseX = (tile.GridX - _levelDefinition.GridWidth / 2f + 0.5f) * tileWidth;
            var baseY = (tile.GridY - _levelDefinition.GridHeight / 2f + 0.5f) * tileHeight;
            var layerOffsetX = tile.Layer * layerOffset.x;
            var layerOffsetY = tile.Layer * layerOffset.y;

            rectTransform.anchoredPosition = new Vector2(baseX + layerOffsetX, baseY + layerOffsetY);
            rectTransform.sizeDelta = new Vector2(tileWidth - 4f, tileHeight - 4f);

            // Shadow for depth (behind the sprite)
            var shadowObj = new GameObject("Shadow");
            shadowObj.transform.SetParent(tileObj.transform, false);
            var shadowRect = shadowObj.AddComponent<RectTransform>();
            shadowRect.anchorMin = Vector2.zero;
            shadowRect.anchorMax = Vector2.one;
            shadowRect.offsetMin = new Vector2(4f, -4f);
            shadowRect.offsetMax = new Vector2(6f, -2f);
            var shadowImage = shadowObj.AddComponent<Image>();
            shadowImage.color = new Color(0f, 0f, 0f, 0.2f + tile.Layer * 0.08f);

            // Main image - either sprite or fallback background
            var mainImage = tileObj.AddComponent<Image>();
            if (_spriteCache.TryGetValue(tile.ItemId, out var sprite))
            {
                mainImage.sprite = sprite;
                mainImage.preserveAspect = true;
                mainImage.color = Color.white;
            }
            else
            {
                // Fallback: light background with text
                mainImage.color = new Color(0.95f, 0.93f, 0.9f, 1f);

                var textObj = new GameObject("Label");
                textObj.transform.SetParent(tileObj.transform, false);
                var textRect = textObj.AddComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;

                var text = textObj.AddComponent<TextMeshProUGUI>();
                text.text = tile.DisplayName;
                text.fontSize = 16;
                text.alignment = TextAlignmentOptions.Center;
                text.color = Color.black;
                text.font = HappyFamilyUiThemeProvider.GetResolvedPrimaryFont();
            }

            // Button for interaction
            var button = tileObj.AddComponent<Button>();
            button.targetGraphic = mainImage;
            var capturedIndex = tile.Index;
            button.onClick.AddListener(() => OnTileClicked(capturedIndex));

            return tileObj;
        }

        private void SetupSlots()
        {
            foreach (var img in _slotImages)
            {
                if (img != null && img.transform.parent != null)
                {
                    Destroy(img.transform.parent.gameObject);
                }
            }
            _slotImages.Clear();

            // 3:4 ratio slots (width:height)
            var slotWidth = 54f;
            var slotHeight = 72f;
            var spacing = 6f;
            var totalWidth = TidyUpBoard.SlotCount * slotWidth + (TidyUpBoard.SlotCount - 1) * spacing;
            var startX = -totalWidth / 2f + slotWidth / 2f;

            for (var i = 0; i < TidyUpBoard.SlotCount; i++)
            {
                var slotObj = new GameObject($"Slot_{i}");
                slotObj.transform.SetParent(_slotsContainer, false);

                var rectTransform = slotObj.AddComponent<RectTransform>();
                rectTransform.anchoredPosition = new Vector2(startX + i * (slotWidth + spacing), 0f);
                rectTransform.sizeDelta = new Vector2(slotWidth, slotHeight);

                var bgImage = slotObj.AddComponent<Image>();
                bgImage.color = new Color(0.92f, 0.9f, 0.88f, 0.5f);

                var itemObj = new GameObject("Item");
                itemObj.transform.SetParent(slotObj.transform, false);
                var itemRect = itemObj.AddComponent<RectTransform>();
                itemRect.anchorMin = Vector2.zero;
                itemRect.anchorMax = Vector2.one;
                itemRect.offsetMin = new Vector2(2f, 2f);
                itemRect.offsetMax = new Vector2(-2f, -2f);

                var itemImage = itemObj.AddComponent<Image>();
                itemImage.preserveAspect = true;
                itemImage.enabled = false;

                _slotImages.Add(itemImage);
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

        private void OnTileClicked(int tileIndex)
        {
            if (_board == null) return;

            var result = _board.TryPickTile(tileIndex);

            switch (result.State)
            {
                case TidyUpMoveState.None:
                    ShowMessage("这个物品被压住了，先整理上面的");
                    break;

                case TidyUpMoveState.Picked:
                    UpdateTileVisuals();
                    UpdateSlotVisuals();
                    UpdateStatus();
                    break;

                case TidyUpMoveState.Matched:
                    UpdateTileVisuals();
                    UpdateSlotVisuals();
                    ShowMessage($"整理好了「{result.ItemName}」！");
                    UpdateStatus();

                    if (_board.IsCompleted)
                    {
                        ShowMessage("全部整理完毕！太棒了！");
                        _onLevelComplete?.Invoke(true);
                    }
                    break;

                case TidyUpMoveState.SlotsFull:
                    UpdateTileVisuals();
                    UpdateSlotVisuals();
                    ShowMessage("收纳栏满了！");
                    UpdateStatus();

                    if (_board.IsFailed)
                    {
                        ShowMessage("空间不够了...再试一次吧");
                        _onLevelComplete?.Invoke(false);
                    }
                    break;
            }
        }

        private void UpdateTileVisuals()
        {
            for (var i = 0; i < _board.Tiles.Count && i < _tileObjects.Count; i++)
            {
                var tile = _board.Tiles[i];
                var tileObj = _tileObjects[i];

                if (tileObj == null) continue;

                tileObj.SetActive(!tile.IsRemoved);

                var button = tileObj.GetComponent<Button>();
                var mainImage = tileObj.GetComponent<Image>();

                if (button != null && mainImage != null)
                {
                    var canPick = _board.CanPickTile(i);
                    button.interactable = canPick;

                    // Dim blocked tiles by reducing alpha
                    var currentColor = mainImage.color;
                    mainImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, canPick ? 1f : 0.5f);
                }
            }
        }

        private void UpdateSlotVisuals()
        {
            for (var i = 0; i < _board.Slots.Length && i < _slotImages.Count; i++)
            {
                var slot = _board.Slots[i];
                var slotImage = _slotImages[i];

                if (slotImage == null) continue;

                if (slot.IsEmpty)
                {
                    slotImage.enabled = false;
                }
                else
                {
                    slotImage.enabled = true;
                    if (_spriteCache.TryGetValue(slot.ItemId, out var sprite))
                    {
                        slotImage.sprite = sprite;
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

            var slotsUsed = 0;
            foreach (var slot in _board.Slots)
            {
                if (!slot.IsEmpty) slotsUsed++;
            }

            _statusLabel.text = $"剩余物品: {remaining}   收纳栏: {slotsUsed}/{TidyUpBoard.SlotCount}";
        }

        private void ShowMessage(string message)
        {
            Debug.Log($"[TidyUp] {message}");
            // The status label can also show the message temporarily
            if (_statusLabel != null)
            {
                var currentText = _statusLabel.text;
                _statusLabel.text = message;
                // Reset after a delay - using coroutine would be better but this is simpler
                Invoke(nameof(UpdateStatus), 1.5f);
            }
        }

        private void OnDestroy()
        {
            foreach (var obj in _tileObjects)
            {
                if (obj != null) Destroy(obj);
            }
            _tileObjects.Clear();

            foreach (var img in _slotImages)
            {
                if (img != null && img.transform.parent != null)
                {
                    Destroy(img.transform.parent.gameObject);
                }
            }
            _slotImages.Clear();
        }
    }
}
