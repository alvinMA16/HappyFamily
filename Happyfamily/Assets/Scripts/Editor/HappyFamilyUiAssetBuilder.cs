#if UNITY_EDITOR
using HappyFamily.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace HappyFamily.EditorTools
{
    public static class HappyFamilyUiAssetBuilder
    {
        private const string RawFontPath = "Assets/Resources/Fonts/NotoSansCJKsc-Regular.otf";
        private const string FontAssetPath = "Assets/Resources/Fonts/NotoSansCJKsc-Regular SDF.asset";
        private const string ThemeAssetDirectory = "Assets/Resources/UI";
        private const string ThemeAssetPath = "Assets/Resources/UI/HappyFamilyUiTheme.asset";

        [MenuItem("HappyFamily/UI/Build Theme Assets")]
        public static void BuildThemeAssets()
        {
            var rawFont = AssetDatabase.LoadAssetAtPath<Font>(RawFontPath);
            if (rawFont == null)
            {
                EditorUtility.DisplayDialog("HappyFamily", $"Missing font file:\n{RawFontPath}", "OK");
                return;
            }

            EnsureFolder("Assets/Resources");
            EnsureFolder("Assets/Resources/Fonts");
            EnsureFolder(ThemeAssetDirectory);

            var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
            if (fontAsset == null)
            {
                fontAsset = TMP_FontAsset.CreateFontAsset(
                    rawFont,
                    128,
                    8,
                    GlyphRenderMode.SDFAA_HINTED,
                    2048,
                    2048,
                    AtlasPopulationMode.Dynamic,
                    true);
                AssetDatabase.CreateAsset(fontAsset, FontAssetPath);
                AttachFontSubAssets(fontAsset);
            }
            else
            {
                AttachFontSubAssets(fontAsset);
            }

            var theme = AssetDatabase.LoadAssetAtPath<HappyFamilyUiTheme>(ThemeAssetPath);
            if (theme == null)
            {
                theme = ScriptableObject.CreateInstance<HappyFamilyUiTheme>();
                AssetDatabase.CreateAsset(theme, ThemeAssetPath);
            }

            theme.PrimaryFont = IsUsableFontAsset(fontAsset) ? fontAsset : null;
            EditorUtility.SetDirty(theme);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("HappyFamily", "TMP 字体资产和 UI Theme 已生成。", "OK");
        }

        private static void AttachFontSubAssets(TMP_FontAsset fontAsset)
        {
            if (fontAsset == null)
            {
                return;
            }

            if (fontAsset.material != null && AssetDatabase.GetAssetPath(fontAsset.material) == string.Empty)
            {
                AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
            }

            if (fontAsset.atlasTextures == null)
            {
                return;
            }

            foreach (var atlasTexture in fontAsset.atlasTextures)
            {
                if (atlasTexture != null && AssetDatabase.GetAssetPath(atlasTexture) == string.Empty)
                {
                    AssetDatabase.AddObjectToAsset(atlasTexture, fontAsset);
                }
            }

            EditorUtility.SetDirty(fontAsset);
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

            foreach (var atlasTexture in fontAsset.atlasTextures)
            {
                if (atlasTexture == null)
                {
                    return false;
                }
            }

            return true;
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
