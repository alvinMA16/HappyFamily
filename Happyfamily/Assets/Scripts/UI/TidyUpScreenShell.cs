using System;
using System.Collections.Generic;
using HappyFamily.Data;
using HappyFamily.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace HappyFamily.UI
{
    public class TidyUpScreenShell : MonoBehaviour
    {
        [Header("Header")]
        [SerializeField] private Button backButton;
        [SerializeField] private Text titleLabel;
        [SerializeField] private Text descriptionLabel;

        [Header("Game Area")]
        [SerializeField] private RectTransform tilesContainer;
        [SerializeField] private RectTransform slotsContainer;

        [Header("Status")]
        [SerializeField] private Text statusLabel;

        // Visual settings
        private static readonly Color CardColor = new Color(0.98f, 0.96f, 0.93f);
        private static readonly Color CardShadowColor = new Color(0.75f, 0.72f, 0.68f);
        private static readonly Color CardBlockedColor = new Color(0.88f, 0.86f, 0.83f);
        private static readonly Color SlotBgColor = new Color(0.92f, 0.90f, 0.87f, 0.6f);
        private static readonly Color SlotBorderColor = new Color(0.80f, 0.77f, 0.73f);

        private TidyUpBoard _board;
        private TidyUpLevelDefinition _levelDefinition;
        private List<GameObject> _tileObjects = new List<GameObject>();
        private List<GameObject> _slotObjects = new List<GameObject>();
        private List<Image> _slotItemImages = new List<Image>();
        private Dictionary<string, Sprite> _spriteCache = new Dictionary<string, Sprite>();

        private Action _onBack;
        private Action<bool> _onLevelComplete;

        public void Initialize(TidyUpLevelDefinition levelDefinition, Action onBack, Action<bool> onLevelComplete)
        {
            _levelDefinition = levelDefinition;
            _onBack = onBack;
            _onLevelComplete = onLevelComplete;

            LoadSprites();
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
                else
                {
                    Debug.LogWarning($"Failed to load sprite: {item.SpritePath}");
                }
            }
        }

        private void SetupHeader()
        {
            if (titleLabel != null)
            {
                titleLabel.text = _levelDefinition.DisplayName;
                titleLabel.fontSize = 36;
                titleLabel.fontStyle = FontStyle.Bold;
                titleLabel.color = new Color(0.3f, 0.28f, 0.25f);
            }

            if (descriptionLabel != null)
            {
                descriptionLabel.text = _levelDefinition.Description;
                descriptionLabel.fontSize = 20;
                descriptionLabel.color = new Color(0.5f, 0.48f, 0.45f);
            }
        }

        private void SetupBackButton()
        {
            if (backButton != null)
            {
                backButton.onClick.RemoveAllListeners();
                backButton.onClick.AddListener(() => _onBack?.Invoke());

                // Style the back button
                var buttonImage = backButton.GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.color = new Color(1f, 1f, 1f, 0f); // Transparent background
                }

                var buttonText = backButton.GetComponentInChildren<Text>();
                if (buttonText != null)
                {
                    buttonText.text = "< 返回";
                    buttonText.fontSize = 24;
                    buttonText.color = new Color(0.4f, 0.38f, 0.35f);
                }
            }
        }

        private void SetupBoard()
        {
            // Clear existing tiles
            foreach (var obj in _tileObjects)
            {
                if (obj != null) Destroy(obj);
            }
            _tileObjects.Clear();

            // Create the board
            _board = TidyUpBoard.Create(_levelDefinition);

            // Calculate tile size based on container
            var containerRect = tilesContainer.rect;
            var padding = 20f;
            var availableWidth = containerRect.width - padding * 2;
            var availableHeight = containerRect.height - padding * 2;

            // Account for layer offset in size calculation
            var layerOffset = new Vector2(6f, -6f);
            var maxLayerOffset = (_levelDefinition.MaxLayers - 1) * Mathf.Abs(layerOffset.x);

            var tileWidth = (availableWidth - maxLayerOffset) / _levelDefinition.GridWidth;
            var tileHeight = (availableHeight - maxLayerOffset) / _levelDefinition.GridHeight;
            var tileSize = Mathf.Min(tileWidth, tileHeight, 90f);

            // Create tile objects (sorted by layer for proper rendering order)
            var sortedTiles = new List<StackedTile>(_board.Tiles);
            sortedTiles.Sort((a, b) => a.Layer.CompareTo(b.Layer));

            foreach (var tile in sortedTiles)
            {
                var tileObj = CreateTileObject(tile, tileSize, layerOffset);
                _tileObjects.Add(tileObj);
            }

            UpdateTileVisuals();
        }

        private GameObject CreateTileObject(StackedTile tile, float tileSize, Vector2 layerOffset)
        {
            var tileObj = new GameObject($"Tile_{tile.Index}");
            tileObj.transform.SetParent(tilesContainer, false);

            // Position based on grid position and layer
            var rectTransform = tileObj.AddComponent<RectTransform>();
            var gridCenterX = (_levelDefinition.GridWidth - 1) / 2f;
            var gridCenterY = (_levelDefinition.GridHeight - 1) / 2f;
            var baseX = (tile.GridX - gridCenterX) * tileSize;
            var baseY = (tile.GridY - gridCenterY) * tileSize;
            var layerOffsetX = tile.Layer * layerOffset.x;
            var layerOffsetY = tile.Layer * layerOffset.y;

            rectTransform.anchoredPosition = new Vector2(baseX + layerOffsetX, baseY + layerOffsetY);
            rectTransform.sizeDelta = new Vector2(tileSize - 4f, tileSize - 4f);

            // Card shadow (underneath)
            var shadowObj = new GameObject("Shadow");
            shadowObj.transform.SetParent(tileObj.transform, false);
            var shadowRect = shadowObj.AddComponent<RectTransform>();
            shadowRect.anchorMin = Vector2.zero;
            shadowRect.anchorMax = Vector2.one;
            shadowRect.offsetMin = new Vector2(3f, -3f);
            shadowRect.offsetMax = new Vector2(5f, -1f);
            var shadowImage = shadowObj.AddComponent<Image>();
            shadowImage.color = CardShadowColor;
            shadowImage.raycastTarget = false;

            // Card background (white/cream card)
            var cardObj = new GameObject("Card");
            cardObj.transform.SetParent(tileObj.transform, false);
            var cardRect = cardObj.AddComponent<RectTransform>();
            cardRect.anchorMin = Vector2.zero;
            cardRect.anchorMax = Vector2.one;
            cardRect.offsetMin = Vector2.zero;
            cardRect.offsetMax = Vector2.zero;
            var cardImage = cardObj.AddComponent<Image>();
            cardImage.color = CardColor;
            cardImage.raycastTarget = false;

            // Item sprite (centered in card with padding)
            var spriteObj = new GameObject("Sprite");
            spriteObj.transform.SetParent(tileObj.transform, false);
            var spriteRect = spriteObj.AddComponent<RectTransform>();
            spriteRect.anchorMin = new Vector2(0.1f, 0.1f);
            spriteRect.anchorMax = new Vector2(0.9f, 0.9f);
            spriteRect.offsetMin = Vector2.zero;
            spriteRect.offsetMax = Vector2.zero;

            var spriteImage = spriteObj.AddComponent<Image>();
            spriteImage.raycastTarget = false;
            if (_spriteCache.TryGetValue(tile.ItemId, out var sprite))
            {
                spriteImage.sprite = sprite;
                spriteImage.preserveAspect = true;
            }

            // Invisible button overlay for interaction
            var buttonImage = tileObj.AddComponent<Image>();
            buttonImage.color = new Color(1f, 1f, 1f, 0f); // Fully transparent

            var button = tileObj.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            var capturedIndex = tile.Index;
            button.onClick.AddListener(() => OnTileClicked(capturedIndex));

            // Store reference to card image for visual updates
            tileObj.name = $"Tile_{tile.Index}";

            return tileObj;
        }

        private void SetupSlots()
        {
            // Clear existing slots
            foreach (var obj in _slotObjects)
            {
                if (obj != null) Destroy(obj);
            }
            _slotObjects.Clear();
            _slotItemImages.Clear();

            var slotSize = 56f;
            var spacing = 6f;
            var totalWidth = TidyUpBoard.SlotCount * slotSize + (TidyUpBoard.SlotCount - 1) * spacing;
            var startX = -totalWidth / 2f + slotSize / 2f;

            for (var i = 0; i < TidyUpBoard.SlotCount; i++)
            {
                var slotObj = new GameObject($"Slot_{i}");
                slotObj.transform.SetParent(slotsContainer, false);

                var rectTransform = slotObj.AddComponent<RectTransform>();
                rectTransform.anchoredPosition = new Vector2(startX + i * (slotSize + spacing), 0f);
                rectTransform.sizeDelta = new Vector2(slotSize, slotSize);

                // Slot background with subtle border effect
                var bgImage = slotObj.AddComponent<Image>();
                bgImage.color = SlotBgColor;

                // Inner area for item
                var innerObj = new GameObject("Inner");
                innerObj.transform.SetParent(slotObj.transform, false);
                var innerRect = innerObj.AddComponent<RectTransform>();
                innerRect.anchorMin = Vector2.zero;
                innerRect.anchorMax = Vector2.one;
                innerRect.offsetMin = new Vector2(2f, 2f);
                innerRect.offsetMax = new Vector2(-2f, -2f);

                // Item image
                var itemImage = innerObj.AddComponent<Image>();
                itemImage.preserveAspect = true;
                itemImage.enabled = false;
                itemImage.raycastTarget = false;

                _slotObjects.Add(slotObj);
                _slotItemImages.Add(itemImage);
            }
        }

        private void OnTileClicked(int tileIndex)
        {
            if (_board == null) return;

            var result = _board.TryPickTile(tileIndex);

            switch (result.State)
            {
                case TidyUpMoveState.None:
                    // Tile is blocked - maybe add subtle shake animation
                    break;

                case TidyUpMoveState.Picked:
                    UpdateTileVisuals();
                    UpdateSlotVisuals();
                    UpdateStatus();
                    break;

                case TidyUpMoveState.Matched:
                    UpdateTileVisuals();
                    UpdateSlotVisuals();
                    UpdateStatus();

                    if (_board.IsCompleted)
                    {
                        _onLevelComplete?.Invoke(true);
                    }
                    break;

                case TidyUpMoveState.SlotsFull:
                    UpdateTileVisuals();
                    UpdateSlotVisuals();
                    UpdateStatus();

                    if (_board.IsFailed)
                    {
                        _onLevelComplete?.Invoke(false);
                    }
                    break;
            }
        }

        private void UpdateTileVisuals()
        {
            foreach (var tileObj in _tileObjects)
            {
                if (tileObj == null) continue;

                // Extract tile index from name
                var nameParts = tileObj.name.Split('_');
                if (nameParts.Length < 2 || !int.TryParse(nameParts[1], out var tileIndex))
                    continue;

                if (tileIndex < 0 || tileIndex >= _board.Tiles.Count)
                    continue;

                var tile = _board.Tiles[tileIndex];

                // Hide removed tiles
                tileObj.SetActive(!tile.IsRemoved);

                if (tile.IsRemoved) continue;

                // Update interactability and visual feedback
                var button = tileObj.GetComponent<Button>();
                var cardTransform = tileObj.transform.Find("Card");

                if (button != null && cardTransform != null)
                {
                    var canPick = _board.CanPickTile(tileIndex);
                    button.interactable = canPick;

                    var cardImage = cardTransform.GetComponent<Image>();
                    if (cardImage != null)
                    {
                        // Slightly darker for blocked tiles
                        cardImage.color = canPick ? CardColor : CardBlockedColor;
                    }

                    // Dim the sprite for blocked tiles
                    var spriteTransform = tileObj.transform.Find("Sprite");
                    if (spriteTransform != null)
                    {
                        var spriteImage = spriteTransform.GetComponent<Image>();
                        if (spriteImage != null)
                        {
                            spriteImage.color = canPick ? Color.white : new Color(0.85f, 0.85f, 0.85f);
                        }
                    }
                }
            }
        }

        private void UpdateSlotVisuals()
        {
            for (var i = 0; i < _board.Slots.Length && i < _slotItemImages.Count; i++)
            {
                var slot = _board.Slots[i];
                var itemImage = _slotItemImages[i];

                if (itemImage == null) continue;

                if (slot.IsEmpty)
                {
                    itemImage.enabled = false;
                }
                else
                {
                    itemImage.enabled = true;
                    if (_spriteCache.TryGetValue(slot.ItemId, out var sprite))
                    {
                        itemImage.sprite = sprite;
                    }
                }
            }
        }

        private void UpdateStatus()
        {
            if (statusLabel == null) return;

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

            statusLabel.text = $"剩余: {remaining}";
            statusLabel.fontSize = 18;
            statusLabel.color = new Color(0.5f, 0.48f, 0.45f);
        }

        private void OnDestroy()
        {
            foreach (var obj in _tileObjects)
            {
                if (obj != null) Destroy(obj);
            }
            _tileObjects.Clear();

            foreach (var obj in _slotObjects)
            {
                if (obj != null) Destroy(obj);
            }
            _slotObjects.Clear();
            _slotItemImages.Clear();
        }
    }
}
