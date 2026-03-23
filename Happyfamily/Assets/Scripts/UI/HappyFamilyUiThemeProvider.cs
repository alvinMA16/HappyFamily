using System;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace HappyFamily.UI
{
    public static class HappyFamilyUiThemeProvider
    {
        private static readonly string[] ProjectFontResourcePaths =
        {
            "Fonts/NotoSansCJKsc-Regular SDF",
            "Fonts/NotoSansSC-Regular SDF",
            "Fonts/UI-Regular SDF"
        };

        private static readonly string[] ProjectRawFontResourcePaths =
        {
            "Fonts/NotoSansCJKsc-Regular",
            "Fonts/NotoSansSC-Regular",
            "Fonts/UI-Regular"
        };

        private static HappyFamilyUiTheme cachedTheme;
        private static TMP_FontAsset cachedFallbackFontAsset;

        public static HappyFamilyUiTheme GetTheme()
        {
            if (cachedTheme != null)
            {
                return cachedTheme;
            }

            cachedTheme = Resources.Load<HappyFamilyUiTheme>("UI/HappyFamilyUiTheme");
            if (cachedTheme == null)
            {
                cachedTheme = ScriptableObject.CreateInstance<HappyFamilyUiTheme>();
                cachedTheme.hideFlags = HideFlags.HideAndDontSave;
            }

            return cachedTheme;
        }

        public static TMP_FontAsset GetResolvedPrimaryFont()
        {
            var theme = GetTheme();
            if (theme != null && IsUsableFontAsset(theme.PrimaryFont))
            {
                return theme.PrimaryFont;
            }

            return GetFallbackFontAsset();
        }

        private static TMP_FontAsset GetFallbackFontAsset()
        {
            if (cachedFallbackFontAsset != null)
            {
                return cachedFallbackFontAsset;
            }

            foreach (var resourcePath in ProjectFontResourcePaths)
            {
                var fontAsset = Resources.Load<TMP_FontAsset>(resourcePath);
                if (IsUsableFontAsset(fontAsset))
                {
                    cachedFallbackFontAsset = fontAsset;
                    return cachedFallbackFontAsset;
                }
            }

            foreach (var resourcePath in ProjectRawFontResourcePaths)
            {
                var projectFont = Resources.Load<Font>(resourcePath);
                if (projectFont == null)
                {
                    continue;
                }

                cachedFallbackFontAsset = TMP_FontAsset.CreateFontAsset(
                    projectFont,
                    128,
                    8,
                    GlyphRenderMode.SDFAA_HINTED,
                    2048,
                    2048,
                    AtlasPopulationMode.Dynamic,
                    true);
                if (cachedFallbackFontAsset != null)
                {
                    return cachedFallbackFontAsset;
                }
            }

            var systemFont = TryCreateSystemFont();
            if (systemFont != null)
            {
                cachedFallbackFontAsset = TMP_FontAsset.CreateFontAsset(
                    systemFont,
                    128,
                    8,
                    GlyphRenderMode.SDFAA_HINTED,
                    2048,
                    2048,
                    AtlasPopulationMode.Dynamic,
                    true);
            }

            cachedFallbackFontAsset ??= TMP_Settings.defaultFontAsset;
            cachedFallbackFontAsset ??= Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            return cachedFallbackFontAsset;
        }

        private static bool IsUsableFontAsset(TMP_FontAsset fontAsset)
        {
            if (fontAsset == null)
            {
                return false;
            }

            if (fontAsset.material == null)
            {
                return false;
            }

            if (fontAsset.atlasTextures == null || fontAsset.atlasTextures.Length == 0)
            {
                return false;
            }

            for (var index = 0; index < fontAsset.atlasTextures.Length; index++)
            {
                if (fontAsset.atlasTextures[index] == null)
                {
                    return false;
                }
            }

            return true;
        }

        private static Font TryCreateSystemFont()
        {
            try
            {
                return Font.CreateDynamicFontFromOSFont(
                    new[]
                    {
                        "PingFang SC",
                        "Hiragino Sans GB",
                        "Heiti SC",
                        "Microsoft YaHei",
                        "SimHei",
                        "Arial Unicode MS"
                    },
                    32);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[HappyFamilyUiThemeProvider] Failed to create system font: {exception.Message}");
                return null;
            }
        }
    }
}
