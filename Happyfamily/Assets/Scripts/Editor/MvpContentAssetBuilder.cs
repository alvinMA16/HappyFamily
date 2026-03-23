#if UNITY_EDITOR
using HappyFamily.Data;
using UnityEditor;
using UnityEngine;

namespace HappyFamily.EditorTools
{
    public static class MvpContentAssetBuilder
    {
        private const string ContentDirectory = "Assets/Resources/Content";
        private const string ChaptersDirectory = "Assets/Resources/Content/Chapters";
        private const string LevelsDirectory = "Assets/Resources/Content/Levels";
        private const string RenovationNodesDirectory = "Assets/Resources/Content/RenovationNodes";

        [MenuItem("HappyFamily/Content/Build Content Assets")]
        public static void BuildContentAssets()
        {
            EnsureFolder("Assets/Resources");
            EnsureFolder(ContentDirectory);
            EnsureFolder(ChaptersDirectory);
            EnsureFolder(LevelsDirectory);
            EnsureFolder(RenovationNodesDirectory);

            var chapterDefinition = MvpContentFactory.CreateFrontYardChapter();
            BuildChapterAsset(chapterDefinition);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("HappyFamily", "内容资产已生成完毕。", "OK");
        }

        private static void BuildChapterAsset(MvpChapterDefinition definition)
        {
            var chapterAsset = ScriptableObject.CreateInstance<MvpChapterAsset>();
            chapterAsset.ChapterId = definition.Id;
            chapterAsset.DisplayName = definition.DisplayName;

            foreach (var levelDefinition in definition.Levels)
            {
                var levelAsset = BuildLevelAsset(levelDefinition);
                chapterAsset.Levels.Add(levelAsset);
            }

            foreach (var nodeDefinition in definition.RenovationNodes)
            {
                var nodeAsset = BuildRenovationNodeAsset(nodeDefinition);
                chapterAsset.RenovationNodes.Add(nodeAsset);
            }

            SaveAsset(chapterAsset, $"{ChaptersDirectory}/FrontYardChapter.asset");
        }

        private static MvpLevelAsset BuildLevelAsset(MvpLevelDefinition definition)
        {
            var assetPath = $"{LevelsDirectory}/{definition.Id}.asset";
            var levelAsset = ScriptableObject.CreateInstance<MvpLevelAsset>();
            levelAsset.LevelId = definition.Id;
            levelAsset.DisplayName = definition.DisplayName;
            levelAsset.Description = definition.Description;
            levelAsset.StepBudget = definition.StepBudget;
            levelAsset.StarReward = definition.StarReward;
            levelAsset.PairLabels = definition.PairLabels;

            return SaveAndLoadAsset<MvpLevelAsset>(levelAsset, assetPath);
        }

        private static MvpRenovationNodeAsset BuildRenovationNodeAsset(MvpRenovationNodeDefinition definition)
        {
            var assetPath = $"{RenovationNodesDirectory}/{definition.Id}.asset";
            var nodeAsset = ScriptableObject.CreateInstance<MvpRenovationNodeAsset>();
            nodeAsset.NodeId = definition.Id;
            nodeAsset.DisplayName = definition.DisplayName;
            nodeAsset.RequiredStars = definition.RequiredStars;
            nodeAsset.MemoryTitle = definition.MemoryTitle;
            nodeAsset.MemoryText = definition.MemoryText;

            foreach (var optionDefinition in definition.Options)
            {
                nodeAsset.Options.Add(new MvpRenovationOptionData
                {
                    OptionId = optionDefinition.Id,
                    DisplayName = optionDefinition.DisplayName,
                    Description = optionDefinition.Description
                });
            }

            return SaveAndLoadAsset<MvpRenovationNodeAsset>(nodeAsset, assetPath);
        }

        private static T SaveAndLoadAsset<T>(T asset, string path) where T : Object
        {
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null)
            {
                EditorUtility.CopySerialized(asset, existing);
                EditorUtility.SetDirty(existing);
                Object.DestroyImmediate(asset);
                return existing;
            }

            AssetDatabase.CreateAsset(asset, path);
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }

        private static void SaveAsset(Object asset, string path)
        {
            var existing = AssetDatabase.LoadAssetAtPath<Object>(path);
            if (existing != null)
            {
                EditorUtility.CopySerialized(asset, existing);
                EditorUtility.SetDirty(existing);
                Object.DestroyImmediate(asset);
            }
            else
            {
                AssetDatabase.CreateAsset(asset, path);
            }
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            var lastSlash = path.LastIndexOf('/');
            var parent = path.Substring(0, lastSlash);
            var folderName = path.Substring(lastSlash + 1);
            if (!AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, folderName);
        }
    }
}
#endif
