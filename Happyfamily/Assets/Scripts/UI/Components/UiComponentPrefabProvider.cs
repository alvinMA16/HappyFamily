using UnityEngine;

namespace HappyFamily.UI.Components
{
    public static class UiComponentPrefabProvider
    {
        private const string CardPrefabPath = "UI/Prefabs/Components/CardPrefab";
        private const string ButtonPrefabPath = "UI/Prefabs/Components/ButtonPrefab";
        private const string TileButtonPrefabPath = "UI/Prefabs/Components/TileButtonPrefab";

        private static CardPrefab cachedCardPrefab;
        private static ButtonPrefab cachedButtonPrefab;
        private static TileButtonPrefab cachedTileButtonPrefab;

        public static CardPrefab LoadCardPrefab()
        {
            if (cachedCardPrefab != null)
            {
                return cachedCardPrefab;
            }

            cachedCardPrefab = Resources.Load<CardPrefab>(CardPrefabPath);
            return cachedCardPrefab;
        }

        public static ButtonPrefab LoadButtonPrefab()
        {
            if (cachedButtonPrefab != null)
            {
                return cachedButtonPrefab;
            }

            cachedButtonPrefab = Resources.Load<ButtonPrefab>(ButtonPrefabPath);
            return cachedButtonPrefab;
        }

        public static TileButtonPrefab LoadTileButtonPrefab()
        {
            if (cachedTileButtonPrefab != null)
            {
                return cachedTileButtonPrefab;
            }

            cachedTileButtonPrefab = Resources.Load<TileButtonPrefab>(TileButtonPrefabPath);
            return cachedTileButtonPrefab;
        }

        public static void ClearCache()
        {
            cachedCardPrefab = null;
            cachedButtonPrefab = null;
            cachedTileButtonPrefab = null;
        }
    }
}
