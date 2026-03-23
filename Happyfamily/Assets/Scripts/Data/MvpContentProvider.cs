using UnityEngine;

namespace HappyFamily.Data
{
    public static class MvpContentProvider
    {
        private const string FrontYardChapterPath = "Content/Chapters/FrontYardChapter";

        private static MvpChapterDefinition cachedFrontYardChapter;

        public static MvpChapterDefinition GetFrontYardChapter()
        {
            if (cachedFrontYardChapter != null)
            {
                return cachedFrontYardChapter;
            }

            var chapterAsset = Resources.Load<MvpChapterAsset>(FrontYardChapterPath);
            if (chapterAsset != null)
            {
                cachedFrontYardChapter = chapterAsset.ToDefinition();
                return cachedFrontYardChapter;
            }

            Debug.Log("[MvpContentProvider] Chapter asset not found, falling back to MvpContentFactory.");
            cachedFrontYardChapter = MvpContentFactory.CreateFrontYardChapter();
            return cachedFrontYardChapter;
        }

        public static void ClearCache()
        {
            cachedFrontYardChapter = null;
        }
    }
}
