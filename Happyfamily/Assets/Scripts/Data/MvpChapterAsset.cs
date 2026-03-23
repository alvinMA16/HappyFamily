using System.Collections.Generic;
using UnityEngine;

namespace HappyFamily.Data
{
    [CreateAssetMenu(fileName = "MvpChapterAsset", menuName = "HappyFamily/Content/Chapter")]
    public class MvpChapterAsset : ScriptableObject
    {
        public string ChapterId;
        public string DisplayName;
        public List<MvpLevelAsset> Levels = new List<MvpLevelAsset>();
        public List<MvpRenovationNodeAsset> RenovationNodes = new List<MvpRenovationNodeAsset>();

        public MvpChapterDefinition ToDefinition()
        {
            var definition = new MvpChapterDefinition
            {
                Id = ChapterId,
                DisplayName = DisplayName,
                Levels = new List<MvpLevelDefinition>(),
                RenovationNodes = new List<MvpRenovationNodeDefinition>()
            };

            foreach (var levelAsset in Levels)
            {
                if (levelAsset != null)
                {
                    definition.Levels.Add(levelAsset.ToDefinition());
                }
            }

            foreach (var nodeAsset in RenovationNodes)
            {
                if (nodeAsset != null)
                {
                    definition.RenovationNodes.Add(nodeAsset.ToDefinition());
                }
            }

            return definition;
        }
    }
}
