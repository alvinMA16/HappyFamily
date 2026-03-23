using TMPro;
using UnityEngine;

namespace HappyFamily.UI
{
    [CreateAssetMenu(fileName = "HappyFamilyUiTheme", menuName = "HappyFamily/UI Theme")]
    public class HappyFamilyUiTheme : ScriptableObject
    {
        [Header("Font")]
        public TMP_FontAsset PrimaryFont;

        [Header("Viewport")]
        public Vector2 ReferenceResolution = new Vector2(750f, 1334f);
        public float ViewportMargin = 24f;

        [Header("Colors")]
        public Color ScreenBackground = new Color(0.98f, 0.97f, 0.93f, 1f);
        public Color HomeBackground = new Color(0.95f, 0.91f, 0.82f, 1f);
        public Color LevelBackground = new Color(0.88f, 0.93f, 0.88f, 1f);
        public Color CardBackground = new Color(0.99f, 0.98f, 0.94f, 1f);
        public Color HeaderBackground = new Color(0.90f, 0.94f, 0.89f, 1f);
        public Color BoardBackground = new Color(0.95f, 0.97f, 0.92f, 1f);
        public Color TileBackground = new Color(0.96f, 0.97f, 0.94f, 1f);
        public Color TileSelectedBackground = new Color(0.94f, 0.79f, 0.46f, 1f);
        public Color TileRemovedBackground = new Color(0.82f, 0.84f, 0.82f, 1f);
        public Color PrimaryButton = new Color(0.73f, 0.35f, 0.18f, 1f);
        public Color SecondaryButton = new Color(0.42f, 0.58f, 0.32f, 1f);
        public Color WarningButton = new Color(0.81f, 0.55f, 0.08f, 1f);
        public Color InfoButton = new Color(0.12f, 0.34f, 0.55f, 1f);
        public Color NeutralButton = new Color(0.39f, 0.41f, 0.43f, 1f);
        public Color BodyText = new Color(0.33f, 0.25f, 0.18f, 1f);
        public Color HeadingText = new Color(0.24f, 0.17f, 0.12f, 1f);
        public Color LightText = Color.white;

        [Header("Type")]
        public int HomeTitleSize = 58;
        public int HomeSubtitleSize = 28;
        public int HomeCardTitleSize = 42;
        public int HomeBodySize = 30;
        public int LevelTitleSize = 38;
        public int LevelBodySize = 22;
        public int ButtonTextSize = 30;

        [Header("Layout")]
        public float HeaderHeight = 190f;
        public float BottomToolbarInset = 24f;
        public float BottomToolbarHeight = 78f;
        public float HorizontalPadding = 24f;
        public float BoardBottomInset = 120f;
        public float BoardTopInset = 230f;
        public float TileGap = 16f;
        public float BoardPadding = 12f;
    }
}
