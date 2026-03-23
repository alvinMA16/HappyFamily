using System;
using UnityEngine;

namespace HappyFamily.Data
{
    [CreateAssetMenu(fileName = "MvpLevelAsset", menuName = "HappyFamily/Content/Level")]
    public class MvpLevelAsset : ScriptableObject
    {
        public string LevelId;
        public string DisplayName;
        [TextArea(2, 4)]
        public string Description;
        public int StepBudget;
        public int StarReward;
        public string[] PairLabels;

        public MvpLevelDefinition ToDefinition()
        {
            return new MvpLevelDefinition
            {
                Id = LevelId,
                DisplayName = DisplayName,
                Description = Description,
                StepBudget = StepBudget,
                StarReward = StarReward,
                PairLabels = PairLabels ?? Array.Empty<string>()
            };
        }
    }
}
