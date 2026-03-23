using System;
using System.Collections.Generic;

namespace HappyFamily.Save
{
    [Serializable]
    public class HappyFamilySaveData
    {
        public int TotalStars;
        public List<string> CompletedLevelIds = new List<string>();
        public List<RenovationSelectionRecord> RenovationSelections = new List<RenovationSelectionRecord>();

        public static HappyFamilySaveData CreateDefault()
        {
            return new HappyFamilySaveData();
        }

        public void Normalize()
        {
            CompletedLevelIds ??= new List<string>();
            RenovationSelections ??= new List<RenovationSelectionRecord>();
        }

        public bool IsLevelCompleted(string levelId)
        {
            return CompletedLevelIds.Contains(levelId);
        }

        public void CompleteLevel(string levelId, int starsAwarded)
        {
            if (CompletedLevelIds.Contains(levelId))
            {
                return;
            }

            CompletedLevelIds.Add(levelId);
            TotalStars += starsAwarded;
        }

        public void SetRenovationSelection(string nodeId, string optionId)
        {
            for (var index = 0; index < RenovationSelections.Count; index++)
            {
                if (RenovationSelections[index].NodeId == nodeId)
                {
                    RenovationSelections[index].OptionId = optionId;
                    return;
                }
            }

            RenovationSelections.Add(new RenovationSelectionRecord
            {
                NodeId = nodeId,
                OptionId = optionId
            });
        }

        public bool TryGetRenovationSelection(string nodeId, out string optionId)
        {
            for (var index = 0; index < RenovationSelections.Count; index++)
            {
                if (RenovationSelections[index].NodeId == nodeId)
                {
                    optionId = RenovationSelections[index].OptionId;
                    return true;
                }
            }

            optionId = string.Empty;
            return false;
        }
    }

    [Serializable]
    public class RenovationSelectionRecord
    {
        public string NodeId;
        public string OptionId;
    }
}
