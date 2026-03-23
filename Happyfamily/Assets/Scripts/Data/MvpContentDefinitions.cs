using System;
using System.Collections.Generic;

namespace HappyFamily.Data
{
    [Serializable]
    public class MvpChapterDefinition
    {
        public string Id;
        public string DisplayName;
        public List<MvpLevelDefinition> Levels = new List<MvpLevelDefinition>();
        public List<MvpRenovationNodeDefinition> RenovationNodes = new List<MvpRenovationNodeDefinition>();

        public MvpRenovationNodeDefinition GetFirstUnlockableNode(int totalStars)
        {
            foreach (var node in RenovationNodes)
            {
                if (totalStars >= node.RequiredStars)
                {
                    return node;
                }
            }

            return null;
        }
    }

    [Serializable]
    public class MvpLevelDefinition
    {
        public string Id;
        public string DisplayName;
        public string Description;
        public int StepBudget;
        public int StarReward;
        public string[] PairLabels;
    }

    [Serializable]
    public class MvpRenovationNodeDefinition
    {
        public string Id;
        public string DisplayName;
        public int RequiredStars;
        public string MemoryTitle;
        public string MemoryText;
        public List<MvpRenovationOptionDefinition> Options = new List<MvpRenovationOptionDefinition>();

        public MvpRenovationOptionDefinition GetOption(string optionId)
        {
            foreach (var option in Options)
            {
                if (option.Id == optionId)
                {
                    return option;
                }
            }

            return null;
        }
    }

    [Serializable]
    public class MvpRenovationOptionDefinition
    {
        public string Id;
        public string DisplayName;
        public string Description;
    }
}
