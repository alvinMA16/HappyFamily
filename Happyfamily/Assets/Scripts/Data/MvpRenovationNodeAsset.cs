using System;
using System.Collections.Generic;
using UnityEngine;

namespace HappyFamily.Data
{
    [CreateAssetMenu(fileName = "MvpRenovationNodeAsset", menuName = "HappyFamily/Content/Renovation Node")]
    public class MvpRenovationNodeAsset : ScriptableObject
    {
        public string NodeId;
        public string DisplayName;
        public int RequiredStars;
        [TextArea(2, 4)]
        public string MemoryTitle;
        [TextArea(3, 6)]
        public string MemoryText;
        public List<MvpRenovationOptionData> Options = new List<MvpRenovationOptionData>();

        public MvpRenovationNodeDefinition ToDefinition()
        {
            var definition = new MvpRenovationNodeDefinition
            {
                Id = NodeId,
                DisplayName = DisplayName,
                RequiredStars = RequiredStars,
                MemoryTitle = MemoryTitle,
                MemoryText = MemoryText,
                Options = new List<MvpRenovationOptionDefinition>()
            };

            foreach (var optionData in Options)
            {
                definition.Options.Add(new MvpRenovationOptionDefinition
                {
                    Id = optionData.OptionId,
                    DisplayName = optionData.DisplayName,
                    Description = optionData.Description
                });
            }

            return definition;
        }
    }

    [Serializable]
    public class MvpRenovationOptionData
    {
        public string OptionId;
        public string DisplayName;
        [TextArea(2, 4)]
        public string Description;
    }
}
