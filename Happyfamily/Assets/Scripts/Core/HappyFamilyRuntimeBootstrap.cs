using UnityEngine;

namespace HappyFamily.Core
{
    public static class HappyFamilyRuntimeBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CreateApp()
        {
            if (Object.FindObjectOfType<HappyFamilyGameApp>() != null)
            {
                return;
            }

            var appObject = new GameObject("HappyFamilyApp");
            Object.DontDestroyOnLoad(appObject);
            appObject.AddComponent<HappyFamilyGameApp>();
        }
    }
}
