using UnityEngine;

namespace HappyFamily.UI
{
    public static class HappyFamilyScreenPrefabProvider
    {
        public static HomeScreenShell LoadHomeScreenShell()
        {
            return Resources.Load<HomeScreenShell>("UI/Prefabs/HomeScreenShell");
        }

        public static LevelScreenShell LoadLevelScreenShell()
        {
            return Resources.Load<LevelScreenShell>("UI/Prefabs/LevelScreenShell");
        }
    }
}
