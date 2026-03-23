using HappyFamily.Data;
using System;
using System.Collections.Generic;

namespace HappyFamily.Gameplay
{
    public enum PairMatchMoveState
    {
        None,
        Selected,
        Deselected,
        Matched,
        Mismatched
    }

    public readonly struct PairMatchMoveResult
    {
        public PairMatchMoveResult(PairMatchMoveState state, string primaryLabel)
        {
            State = state;
            PrimaryLabel = primaryLabel;
        }

        public PairMatchMoveState State { get; }
        public string PrimaryLabel { get; }
    }

    [Serializable]
    public class PairMatchTileState
    {
        public int Index;
        public string PairId;
        public string DisplayLabel;
        public bool IsRemoved;
        public bool IsSelected;
    }

    public readonly struct PairMatchHint
    {
        public PairMatchHint(int leftIndex, int rightIndex)
        {
            LeftIndex = leftIndex;
            RightIndex = rightIndex;
        }

        public int LeftIndex { get; }
        public int RightIndex { get; }
    }

    public class PairMatchBoard
    {
        private int selectedIndex = -1;

        private PairMatchBoard(List<PairMatchTileState> tiles, int remainingSteps)
        {
            Tiles = tiles;
            RemainingSteps = remainingSteps;
        }

        public List<PairMatchTileState> Tiles { get; }
        public int RemainingSteps { get; private set; }
        public int RemainingPairs => CountRemainingTiles() / 2;
        public bool IsCompleted => CountRemainingTiles() == 0;
        public bool IsFailed => !IsCompleted && RemainingSteps <= 0;

        public static PairMatchBoard Create(MvpLevelDefinition levelDefinition)
        {
            var tiles = new List<PairMatchTileState>();
            for (var index = 0; index < levelDefinition.PairLabels.Length; index++)
            {
                var pairId = $"pair_{index}";
                tiles.Add(new PairMatchTileState
                {
                    PairId = pairId,
                    DisplayLabel = levelDefinition.PairLabels[index]
                });
                tiles.Add(new PairMatchTileState
                {
                    PairId = pairId,
                    DisplayLabel = levelDefinition.PairLabels[index]
                });
            }

            Shuffle(tiles);
            for (var index = 0; index < tiles.Count; index++)
            {
                tiles[index].Index = index;
            }

            return new PairMatchBoard(tiles, levelDefinition.StepBudget);
        }

        public PairMatchMoveResult TrySelect(int tileIndex)
        {
            if (tileIndex < 0 || tileIndex >= Tiles.Count)
            {
                return new PairMatchMoveResult(PairMatchMoveState.None, string.Empty);
            }

            var tile = Tiles[tileIndex];
            if (tile.IsRemoved)
            {
                return new PairMatchMoveResult(PairMatchMoveState.None, string.Empty);
            }

            if (selectedIndex == tileIndex)
            {
                tile.IsSelected = false;
                selectedIndex = -1;
                return new PairMatchMoveResult(PairMatchMoveState.Deselected, tile.DisplayLabel);
            }

            if (selectedIndex == -1)
            {
                tile.IsSelected = true;
                selectedIndex = tileIndex;
                return new PairMatchMoveResult(PairMatchMoveState.Selected, tile.DisplayLabel);
            }

            var selectedTile = Tiles[selectedIndex];
            tile.IsSelected = true;
            RemainingSteps = Math.Max(0, RemainingSteps - 1);

            if (selectedTile.PairId == tile.PairId)
            {
                selectedTile.IsSelected = false;
                tile.IsSelected = false;
                selectedTile.IsRemoved = true;
                tile.IsRemoved = true;
                selectedIndex = -1;
                return new PairMatchMoveResult(PairMatchMoveState.Matched, tile.DisplayLabel);
            }

            selectedTile.IsSelected = false;
            tile.IsSelected = false;
            selectedIndex = -1;
            return new PairMatchMoveResult(PairMatchMoveState.Mismatched, tile.DisplayLabel);
        }

        public PairMatchHint? FindHint()
        {
            for (var left = 0; left < Tiles.Count; left++)
            {
                if (Tiles[left].IsRemoved)
                {
                    continue;
                }

                for (var right = left + 1; right < Tiles.Count; right++)
                {
                    if (Tiles[right].IsRemoved)
                    {
                        continue;
                    }

                    if (Tiles[left].PairId == Tiles[right].PairId)
                    {
                        return new PairMatchHint(left, right);
                    }
                }
            }

            return null;
        }

        public void ShuffleRemaining()
        {
            var remaining = new List<(string PairId, string DisplayLabel)>();
            foreach (var tile in Tiles)
            {
                if (!tile.IsRemoved)
                {
                    tile.IsSelected = false;
                    remaining.Add((tile.PairId, tile.DisplayLabel));
                }
            }

            selectedIndex = -1;
            Shuffle(remaining);

            var remainingIndex = 0;
            for (var index = 0; index < Tiles.Count; index++)
            {
                if (Tiles[index].IsRemoved)
                {
                    continue;
                }

                Tiles[index].PairId = remaining[remainingIndex].PairId;
                Tiles[index].DisplayLabel = remaining[remainingIndex].DisplayLabel;
                remainingIndex++;
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

        private static void Shuffle(List<PairMatchTileState> tiles)
        {
            for (var index = tiles.Count - 1; index > 0; index--)
            {
                var swapIndex = UnityEngine.Random.Range(0, index + 1);
                (tiles[index], tiles[swapIndex]) = (tiles[swapIndex], tiles[index]);
            }
        }

        private static void Shuffle(List<(string PairId, string DisplayLabel)> tiles)
        {
            for (var index = tiles.Count - 1; index > 0; index--)
            {
                var swapIndex = UnityEngine.Random.Range(0, index + 1);
                (tiles[index], tiles[swapIndex]) = (tiles[swapIndex], tiles[index]);
            }
        }
    }
}
