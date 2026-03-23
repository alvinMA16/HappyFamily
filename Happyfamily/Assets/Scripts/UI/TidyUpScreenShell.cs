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
        [SerializeField] private Text titleLabel;
        [SerializeField] private Text descriptionLabel;

        [Header("Game Area")]
        [SerializeField] private RectTransform tilesContainer;
        [SerializeField] private RectTransform slotsContainer;

        [Header("Footer")]
        [SerializeField] private Button backButton;
        [SerializeField] private Text statusLabel;

        private TidyUpBoard _board;
        private TidyUpLevelDefinition _levelDefinition;
        private List<GameObject> _tileObjects = new List<GameObject>();
        private List<Image> _slotImages = new List<Image>();
        private Dictionary<string, Sprite> _spriteCache = new Dictionary<string, Sprite>();

        private Action _onBack;
        private Action<bool> _onLevelComplete;

        public void Initialize(TidyUpLevelDefinition levelDefinition, Action onBack, Action<bool> onLevelComplete)
        {
            _levelDefinition = levelDefinition;
            _onBack = onBack;
            _onLevelComplete = onLevelComplete;

            // Load sprites
            LoadSprites();

            // Setup UI
            SetupHeader();
            SetupBoard();
            SetupSlots();
            SetupFooter();
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
            }

            if (descriptionLabel != null)
            {
                descriptionLabel.text = _levelDefinition.Description;
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
            var tileWidth = containerRect.width / (_levelDefinition.GridWidth + 1);
            var tileHeight = containerRect.height / (_levelDefinition.GridHeight + 1);
            var tileSize = Mathf.Min(tileWidth, tileHeight, 80f);

            // Layer offset for 3D stacking effect
            var layerOffset = new Vector2(8f, -8f);

            // Create tile objects
            foreach (var tile in _board.Tiles)
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
            var baseX = (tile.GridX - _levelDefinition.GridWidth / 2f + 0.5f) * tileSize;
            var baseY = (tile.GridY - _levelDefinition.GridHeight / 2f + 0.5f) * tileSize;
            var layerOffsetX = tile.Layer * layerOffset.x;
            var layerOffsetY = tile.Layer * layerOffset.y;

            rectTransform.anchoredPosition = new Vector2(baseX + layerOffsetX, baseY + layerOffsetY);
            rectTransform.sizeDelta = new Vector2(tileSize - 4f, tileSize - 4f);

            // Background image
            var bgImage = tileObj.AddComponent<Image>();
            bgImage.color = new Color(0.95f, 0.9f, 0.85f);

            // Item sprite
            var spriteObj = new GameObject("Sprite");
            spriteObj.transform.SetParent(tileObj.transform, false);
            var spriteRect = spriteObj.AddComponent<RectTransform>();
            spriteRect.anchorMin = Vector2.zero;
            spriteRect.anchorMax = Vector2.one;
            spriteRect.offsetMin = new Vector2(4f, 4f);
            spriteRect.offsetMax = new Vector2(-4f, -4f);

            var spriteImage = spriteObj.AddComponent<Image>();
            if (_spriteCache.TryGetValue(tile.ItemId, out var sprite))
            {
                spriteImage.sprite = sprite;
                spriteImage.preserveAspect = true;
            }

            // Button for interaction
            var button = tileObj.AddComponent<Button>();
            var capturedIndex = tile.Index;
            button.onClick.AddListener(() => OnTileClicked(capturedIndex));

            // Shadow effect for depth
            var shadowObj = new GameObject("Shadow");
            shadowObj.transform.SetParent(tileObj.transform, false);
            shadowObj.transform.SetAsFirstSibling();
            var shadowRect = shadowObj.AddComponent<RectTransform>();
            shadowRect.anchorMin = Vector2.zero;
            shadowRect.anchorMax = Vector2.one;
            shadowRect.offsetMin = new Vector2(2f, -2f);
            shadowRect.offsetMax = new Vector2(4f, 0f);
            var shadowImage = shadowObj.AddComponent<Image>();
            shadowImage.color = new Color(0f, 0f, 0f, 0.2f);

            return tileObj;
        }

        private void SetupSlots()
        {
            // Clear existing slots
            foreach (var img in _slotImages)
            {
                if (img != null && img.transform.parent != null)
                {
                    Destroy(img.transform.parent.gameObject);
                }
            }
            _slotImages.Clear();

            // Create slot objects
            var slotSize = 60f;
            var spacing = 8f;
            var totalWidth = TidyUpBoard.SlotCount * slotSize + (TidyUpBoard.SlotCount - 1) * spacing;
            var startX = -totalWidth / 2f + slotSize / 2f;

            for (var i = 0; i < TidyUpBoard.SlotCount; i++)
            {
                var slotObj = new GameObject($"Slot_{i}");
                slotObj.transform.SetParent(slotsContainer, false);

                var rectTransform = slotObj.AddComponent<RectTransform>();
                rectTransform.anchoredPosition = new Vector2(startX + i * (slotSize + spacing), 0f);
                rectTransform.sizeDelta = new Vector2(slotSize, slotSize);

                // Slot background
                var bgImage = slotObj.AddComponent<Image>();
                bgImage.color = new Color(0.85f, 0.85f, 0.85f);

                // Item image in slot
                var itemObj = new GameObject("Item");
                itemObj.transform.SetParent(slotObj.transform, false);
                var itemRect = itemObj.AddComponent<RectTransform>();
                itemRect.anchorMin = Vector2.zero;
                itemRect.anchorMax = Vector2.one;
                itemRect.offsetMin = new Vector2(4f, 4f);
                itemRect.offsetMax = new Vector2(-4f, -4f);

                var itemImage = itemObj.AddComponent<Image>();
                itemImage.preserveAspect = true;
                itemImage.enabled = false;

                _slotImages.Add(itemImage);
            }
        }

        private void SetupFooter()
        {
            if (backButton != null)
            {
                backButton.onClick.RemoveAllListeners();
                backButton.onClick.AddListener(() => _onBack?.Invoke());
            }

            UpdateStatus();
        }

        private void OnTileClicked(int tileIndex)
        {
            if (_board == null) return;

            var result = _board.TryPickTile(tileIndex);

            switch (result.State)
            {
                case TidyUpMoveState.None:
                    // Tile is blocked or already removed
                    ShowMessage("这个物品被压住了");
                    break;

                case TidyUpMoveState.Picked:
                    UpdateTileVisuals();
                    UpdateSlotVisuals();
                    UpdateStatus();
                    break;

                case TidyUpMoveState.Matched:
                    UpdateTileVisuals();
                    UpdateSlotVisuals();
                    ShowMessage($"整理好了 {result.ItemName}！");
                    UpdateStatus();

                    // Check win condition
                    if (_board.IsCompleted)
                    {
                        ShowMessage("全部整理完毕！");
                        _onLevelComplete?.Invoke(true);
                    }
                    break;

                case TidyUpMoveState.SlotsFull:
                    ShowMessage("收纳栏满了！");
                    UpdateStatus();

                    // Check lose condition
                    if (_board.IsFailed)
                    {
                        ShowMessage("空间不够了...");
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

                // Hide removed tiles
                tileObj.SetActive(!tile.IsRemoved);

                // Update interactability based on blocking
                var button = tileObj.GetComponent<Button>();
                var bgImage = tileObj.GetComponent<Image>();

                if (button != null && bgImage != null)
                {
                    var canPick = _board.CanPickTile(i);
                    button.interactable = canPick;

                    // Visual feedback for blocked tiles
                    bgImage.color = canPick
                        ? new Color(0.95f, 0.9f, 0.85f)
                        : new Color(0.8f, 0.75f, 0.7f);
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

            statusLabel.text = $"剩余物品: {remaining}  收纳栏: {slotsUsed}/{TidyUpBoard.SlotCount}";
        }

        private void ShowMessage(string message)
        {
            Debug.Log($"[TidyUp] {message}");
            // TODO: Show toast or popup message
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
