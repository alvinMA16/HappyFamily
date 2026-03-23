using System;
using System.Collections.Generic;
using HappyFamily.Data;
using UnityEngine;

namespace HappyFamily.Gameplay
{
    [Serializable]
    public class StackedTile
    {
        public int Index;
        public string ItemId;
        public string DisplayName;
        public int Layer;        // 0 = bottom, higher = on top
        public int GridX;
        public int GridY;
        public bool IsRemoved;

        public List<int> BlockedBy = new List<int>();  // Indices of tiles blocking this one
    }

    [Serializable]
    public class CollectionSlot
    {
        public string ItemId;
        public string DisplayName;
        public bool IsEmpty => string.IsNullOrEmpty(ItemId);

        public void Clear()
        {
            ItemId = null;
            DisplayName = null;
        }

        public void Set(string itemId, string displayName)
        {
            ItemId = itemId;
            DisplayName = displayName;
        }
    }

    public enum TidyUpMoveState
    {
        None,
        Picked,
        Matched,
        SlotsFull
    }

    public readonly struct TidyUpMoveResult
    {
        public TidyUpMoveResult(TidyUpMoveState state, string itemName, int matchCount = 0)
        {
            State = state;
            ItemName = itemName;
            MatchCount = matchCount;
        }

        public TidyUpMoveState State { get; }
        public string ItemName { get; }
        public int MatchCount { get; }
    }

    public class TidyUpBoard
    {
        public const int SlotCount = 7;
        public const int MatchRequired = 3;

        private TidyUpBoard(List<StackedTile> tiles, CollectionSlot[] slots)
        {
            Tiles = tiles;
            Slots = slots;
        }

        public List<StackedTile> Tiles { get; }
        public CollectionSlot[] Slots { get; }

        public bool IsCompleted => CountRemainingTiles() == 0;
        public bool IsFailed => IsSlotsFull() && !HasPendingMatch();

        public static TidyUpBoard Create(TidyUpLevelDefinition levelDefinition)
        {
            var tiles = new List<StackedTile>();
            var tileIndex = 0;

            // Create tiles based on level definition
            foreach (var itemDef in levelDefinition.Items)
            {
                // Each item appears 3 times (for match-3)
                for (var copy = 0; copy < MatchRequired; copy++)
                {
                    tiles.Add(new StackedTile
                    {
                        Index = tileIndex++,
                        ItemId = itemDef.ItemId,
                        DisplayName = itemDef.DisplayName
                    });
                }
            }

            // Shuffle tiles
            Shuffle(tiles);

            // Assign positions in a stacked grid layout
            AssignStackedPositions(tiles, levelDefinition.GridWidth, levelDefinition.GridHeight, levelDefinition.MaxLayers);

            // Calculate blocking relationships
            CalculateBlocking(tiles);

            // Initialize empty slots
            var slots = new CollectionSlot[SlotCount];
            for (var i = 0; i < SlotCount; i++)
            {
                slots[i] = new CollectionSlot();
            }

            return new TidyUpBoard(tiles, slots);
        }

        public bool CanPickTile(int tileIndex)
        {
            if (tileIndex < 0 || tileIndex >= Tiles.Count)
            {
                return false;
            }

            var tile = Tiles[tileIndex];
            if (tile.IsRemoved)
            {
                return false;
            }

            // Check if blocked by any non-removed tile
            foreach (var blockerIndex in tile.BlockedBy)
            {
                if (!Tiles[blockerIndex].IsRemoved)
                {
                    return false;
                }
            }

            return true;
        }

        public TidyUpMoveResult TryPickTile(int tileIndex)
        {
            if (!CanPickTile(tileIndex))
            {
                return new TidyUpMoveResult(TidyUpMoveState.None, string.Empty);
            }

            var tile = Tiles[tileIndex];

            // Find empty slot
            var slotIndex = FindEmptySlot();
            if (slotIndex < 0)
            {
                return new TidyUpMoveResult(TidyUpMoveState.SlotsFull, tile.DisplayName);
            }

            // Pick the tile
            tile.IsRemoved = true;
            Slots[slotIndex].Set(tile.ItemId, tile.DisplayName);

            // Check for match
            var matchCount = CountMatchingInSlots(tile.ItemId);
            if (matchCount >= MatchRequired)
            {
                RemoveMatchingFromSlots(tile.ItemId);
                return new TidyUpMoveResult(TidyUpMoveState.Matched, tile.DisplayName, matchCount);
            }

            // Check if slots are now full with no pending match
            if (IsSlotsFull() && !HasPendingMatch())
            {
                return new TidyUpMoveResult(TidyUpMoveState.SlotsFull, tile.DisplayName);
            }

            return new TidyUpMoveResult(TidyUpMoveState.Picked, tile.DisplayName);
        }

        public int FindEmptySlot()
        {
            for (var i = 0; i < Slots.Length; i++)
            {
                if (Slots[i].IsEmpty)
                {
                    return i;
                }
            }
            return -1;
        }

        public bool IsSlotsFull()
        {
            for (var i = 0; i < Slots.Length; i++)
            {
                if (Slots[i].IsEmpty)
                {
                    return false;
                }
            }
            return true;
        }

        public bool HasPendingMatch()
        {
            var counts = new Dictionary<string, int>();
            foreach (var slot in Slots)
            {
                if (!slot.IsEmpty)
                {
                    if (!counts.ContainsKey(slot.ItemId))
                    {
                        counts[slot.ItemId] = 0;
                    }
                    counts[slot.ItemId]++;
                    if (counts[slot.ItemId] >= MatchRequired)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private int CountMatchingInSlots(string itemId)
        {
            var count = 0;
            foreach (var slot in Slots)
            {
                if (!slot.IsEmpty && slot.ItemId == itemId)
                {
                    count++;
                }
            }
            return count;
        }

        private void RemoveMatchingFromSlots(string itemId)
        {
            var removed = 0;
            for (var i = 0; i < Slots.Length && removed < MatchRequired; i++)
            {
                if (!Slots[i].IsEmpty && Slots[i].ItemId == itemId)
                {
                    Slots[i].Clear();
                    removed++;
                }
            }

            // Compact slots (move items to fill gaps)
            CompactSlots();
        }

        private void CompactSlots()
        {
            var writeIndex = 0;
            for (var readIndex = 0; readIndex < Slots.Length; readIndex++)
            {
                if (!Slots[readIndex].IsEmpty)
                {
                    if (writeIndex != readIndex)
                    {
                        Slots[writeIndex].Set(Slots[readIndex].ItemId, Slots[readIndex].DisplayName);
                        Slots[readIndex].Clear();
                    }
                    writeIndex++;
                }
            }
        }

        private int CountRemainingTiles()
        {
            var count = 0;
            foreach (var tile in Tiles)
            {
                if (!tile.IsRemoved)
                {
                    count++;
                }
            }
            return count;
        }

        private static void AssignStackedPositions(List<StackedTile> tiles, int gridWidth, int gridHeight, int maxLayers)
        {
            var positionIndex = 0;
            var tilesPerLayer = gridWidth * gridHeight;

            foreach (var tile in tiles)
            {
                var layer = positionIndex / tilesPerLayer;
                var posInLayer = positionIndex % tilesPerLayer;

                tile.Layer = Mathf.Min(layer, maxLayers - 1);
                tile.GridX = posInLayer % gridWidth;
                tile.GridY = posInLayer / gridWidth;

                positionIndex++;
            }
        }

        private static void CalculateBlocking(List<StackedTile> tiles)
        {
            // A tile is blocked by tiles on higher layers that overlap its position
            foreach (var tile in tiles)
            {
                tile.BlockedBy.Clear();

                foreach (var other in tiles)
                {
                    if (other.Index == tile.Index)
                    {
                        continue;
                    }

                    // Check if other tile is on a higher layer and overlaps position
                    if (other.Layer > tile.Layer)
                    {
                        // Check for overlap (tiles on higher layers block if they're at same or adjacent position)
                        var dx = Mathf.Abs(other.GridX - tile.GridX);
                        var dy = Mathf.Abs(other.GridY - tile.GridY);

                        // Direct overlap or partial overlap (adjacent with layer difference)
                        if (dx <= 1 && dy <= 1 && (dx + dy) <= 1)
                        {
                            tile.BlockedBy.Add(other.Index);
                        }
                    }
                }
            }
        }

        private static void Shuffle(List<StackedTile> tiles)
        {
            for (var i = tiles.Count - 1; i > 0; i--)
            {
                var j = UnityEngine.Random.Range(0, i + 1);
                (tiles[i], tiles[j]) = (tiles[j], tiles[i]);
            }

            // Re-assign indices after shuffle
            for (var i = 0; i < tiles.Count; i++)
            {
                tiles[i].Index = i;
            }
        }
    }
}
