#if UNITY_EDITOR
using HappyFamily.UI;
using HappyFamily.UI.Components;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace HappyFamily.EditorTools
{
    public static class HappyFamilyScreenPrefabBuilder
    {
        private const string PrefabDirectory = "Assets/Resources/UI/Prefabs";
        private const string ComponentsPrefabDirectory = "Assets/Resources/UI/Prefabs/Components";
        private const string HomePrefabPath = "Assets/Resources/UI/Prefabs/HomeScreenShell.prefab";
        private const string LevelPrefabPath = "Assets/Resources/UI/Prefabs/LevelScreenShell.prefab";
        private const string CardPrefabPath = "Assets/Resources/UI/Prefabs/Components/CardPrefab.prefab";
        private const string ButtonPrefabPath = "Assets/Resources/UI/Prefabs/Components/ButtonPrefab.prefab";
        private const string TileButtonPrefabPath = "Assets/Resources/UI/Prefabs/Components/TileButtonPrefab.prefab";

        [MenuItem("HappyFamily/UI/Build Screen Prefabs")]
        public static void BuildScreenPrefabs()
        {
            EnsureFolder("Assets/Resources");
            EnsureFolder("Assets/Resources/UI");
            EnsureFolder(PrefabDirectory);

            BuildHomeScreenPrefab();
            BuildLevelScreenPrefab();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("HappyFamily", "首页和关卡页 shell prefab 已生成。", "OK");
        }

        [MenuItem("HappyFamily/UI/Build Component Prefabs")]
        public static void BuildComponentPrefabs()
        {
            EnsureFolder("Assets/Resources");
            EnsureFolder("Assets/Resources/UI");
            EnsureFolder(PrefabDirectory);
            EnsureFolder(ComponentsPrefabDirectory);

            BuildCardPrefab();
            BuildButtonPrefab();
            BuildTileButtonPrefab();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("HappyFamily", "组件预制体已生成。", "OK");
        }

        private static void BuildHomeScreenPrefab()
        {
            var theme = HappyFamilyUiThemeProvider.GetTheme();
            var root = CreateRect("HomeScreenShell");
            var image = root.gameObject.AddComponent<Image>();
            image.color = theme.HomeBackground;
            Stretch(root, 0f, 0f, 0f, 0f);

            var layout = root.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(48, 48, 60, 60);
            layout.spacing = 24;
            layout.childControlHeight = false;
            layout.childForceExpandHeight = false;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.childAlignment = TextAnchor.UpperCenter;

            var titleRoot = CreateRect("TitleRoot", root);
            var titleLayout = titleRoot.gameObject.AddComponent<VerticalLayoutGroup>();
            titleLayout.spacing = 12;
            titleLayout.childControlHeight = false;
            titleLayout.childForceExpandHeight = false;
            titleLayout.childControlWidth = true;
            titleLayout.childForceExpandWidth = true;
            var titleFitter = titleRoot.gameObject.AddComponent<ContentSizeFitter>();
            titleFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            titleFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var progressRoot = CreateRect("ProgressRoot", root);
            var progressLayout = progressRoot.gameObject.AddComponent<VerticalLayoutGroup>();
            progressLayout.spacing = 20;
            progressLayout.childControlHeight = false;
            progressLayout.childForceExpandHeight = false;
            progressLayout.childControlWidth = true;
            progressLayout.childForceExpandWidth = true;
            var progressFitter = progressRoot.gameObject.AddComponent<ContentSizeFitter>();
            progressFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            progressFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var actionRoot = CreateRect("ActionRoot", root);
            var actionLayout = actionRoot.gameObject.AddComponent<VerticalLayoutGroup>();
            actionLayout.spacing = 18;
            actionLayout.childControlHeight = false;
            actionLayout.childForceExpandHeight = false;
            actionLayout.childControlWidth = true;
            actionLayout.childForceExpandWidth = true;
            var actionFitter = actionRoot.gameObject.AddComponent<ContentSizeFitter>();
            actionFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            actionFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var shell = root.gameObject.AddComponent<HomeScreenShell>();
            shell.Bind(root, titleRoot, progressRoot, actionRoot);

            SavePrefab(root.gameObject, HomePrefabPath);
        }

        private static void BuildLevelScreenPrefab()
        {
            var theme = HappyFamilyUiThemeProvider.GetTheme();
            var root = CreateRect("LevelScreenShell");
            var image = root.gameObject.AddComponent<Image>();
            image.color = theme.LevelBackground;
            Stretch(root, 0f, 0f, 0f, 0f);

            var header = CreateRect("HeaderRoot", root);
            header.gameObject.AddComponent<Image>().color = theme.HeaderBackground;
            SetTopRect(header, theme.HorizontalPadding, theme.HorizontalPadding, theme.HorizontalPadding, theme.HeaderHeight);
            var headerLayout = header.gameObject.AddComponent<VerticalLayoutGroup>();
            headerLayout.padding = new RectOffset(28, 28, 22, 18);
            headerLayout.spacing = 8;
            headerLayout.childAlignment = TextAnchor.UpperCenter;
            headerLayout.childControlHeight = false;
            headerLayout.childForceExpandHeight = false;
            headerLayout.childControlWidth = true;
            headerLayout.childForceExpandWidth = true;

            var board = CreateRect("BoardRoot", root);
            board.gameObject.AddComponent<Image>().color = theme.BoardBackground;
            StretchBetween(board, theme.HorizontalPadding, theme.BoardBottomInset, theme.HorizontalPadding, theme.BoardTopInset);

            var toolbar = CreateRect("ToolbarRoot", root);
            toolbar.anchorMin = new Vector2(0.5f, 0f);
            toolbar.anchorMax = new Vector2(0.5f, 0f);
            toolbar.pivot = new Vector2(0.5f, 0f);
            toolbar.anchoredPosition = new Vector2(0f, theme.BottomToolbarInset);
            toolbar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, theme.ReferenceResolution.x - theme.HorizontalPadding * 2f);
            var toolbarLayout = toolbar.gameObject.AddComponent<HorizontalLayoutGroup>();
            toolbarLayout.spacing = 14;
            toolbarLayout.padding = new RectOffset(0, 0, 12, 12);
            toolbarLayout.childAlignment = TextAnchor.MiddleCenter;
            toolbarLayout.childControlHeight = false;
            toolbarLayout.childForceExpandHeight = false;
            toolbarLayout.childControlWidth = false;
            toolbarLayout.childForceExpandWidth = false;

            var shell = root.gameObject.AddComponent<LevelScreenShell>();
            shell.Bind(root, header, board, toolbar);

            SavePrefab(root.gameObject, LevelPrefabPath);
        }

        private static void BuildCardPrefab()
        {
            var theme = HappyFamilyUiThemeProvider.GetTheme();
            var root = new GameObject("CardPrefab", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(LayoutElement), typeof(ContentSizeFitter));
            var rectTransform = root.GetComponent<RectTransform>();

            var image = root.GetComponent<Image>();
            image.color = theme.CardBackground;

            var layout = root.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(32, 32, 28, 28);
            layout.spacing = 18;
            layout.childControlHeight = false;
            layout.childForceExpandHeight = false;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.childAlignment = TextAnchor.UpperLeft;

            var layoutElement = root.GetComponent<LayoutElement>();
            layoutElement.minHeight = 0;
            layoutElement.flexibleHeight = 0;

            var fitter = root.GetComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var cardPrefab = root.AddComponent<CardPrefab>();
            cardPrefab.Bind(rectTransform, image);

            SavePrefab(root, CardPrefabPath);
        }

        private static void BuildButtonPrefab()
        {
            var theme = HappyFamilyUiThemeProvider.GetTheme();
            var root = new GameObject("ButtonPrefab", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));

            var image = root.GetComponent<Image>();
            image.color = theme.PrimaryButton;

            var button = root.GetComponent<Button>();
            var colors = button.colors;
            colors.normalColor = theme.PrimaryButton;
            colors.highlightedColor = theme.PrimaryButton * 1.05f;
            colors.pressedColor = theme.PrimaryButton * 0.92f;
            colors.selectedColor = theme.PrimaryButton;
            colors.disabledColor = new Color(0.70f, 0.70f, 0.70f, 1f);
            button.colors = colors;

            var layoutElement = root.GetComponent<LayoutElement>();
            layoutElement.preferredHeight = 108;

            var labelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement), typeof(ContentSizeFitter));
            labelObject.transform.SetParent(root.transform, false);

            var label = labelObject.GetComponent<TextMeshProUGUI>();
            label.font = HappyFamilyUiThemeProvider.GetResolvedPrimaryFont();
            label.fontSize = theme.ButtonTextSize;
            label.fontStyle = FontStyles.Bold;
            label.alignment = TextAlignmentOptions.Center;
            label.color = theme.LightText;
            label.text = "Button";
            label.enableWordWrapping = true;
            label.overflowMode = TextOverflowModes.Overflow;
            label.raycastTarget = false;
            label.enableAutoSizing = true;
            label.fontSizeMin = 20;
            label.fontSizeMax = theme.ButtonTextSize;

            var labelLayout = labelObject.GetComponent<LayoutElement>();
            labelLayout.minHeight = 88;
            labelLayout.preferredHeight = -1;

            var labelFitter = labelObject.GetComponent<ContentSizeFitter>();
            labelFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            labelFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

            var labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(24f, 16f);
            labelRect.offsetMax = new Vector2(-24f, -16f);

            var buttonPrefab = root.AddComponent<ButtonPrefab>();
            buttonPrefab.Bind(button, image, label, layoutElement);

            SavePrefab(root, ButtonPrefabPath);
        }

        private static void BuildTileButtonPrefab()
        {
            var theme = HappyFamilyUiThemeProvider.GetTheme();
            var root = new GameObject("TileButtonPrefab", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));

            var image = root.GetComponent<Image>();
            image.color = theme.TileBackground;

            var button = root.GetComponent<Button>();
            var colors = button.colors;
            colors.normalColor = theme.TileBackground;
            colors.highlightedColor = theme.TileBackground * 1.05f;
            colors.pressedColor = theme.TileBackground * 0.92f;
            colors.selectedColor = theme.TileBackground;
            colors.disabledColor = new Color(0.78f, 0.80f, 0.78f, 1f);
            button.colors = colors;

            var layoutElement = root.GetComponent<LayoutElement>();
            layoutElement.preferredHeight = 132;

            var labelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement), typeof(ContentSizeFitter));
            labelObject.transform.SetParent(root.transform, false);

            var label = labelObject.GetComponent<TextMeshProUGUI>();
            label.font = HappyFamilyUiThemeProvider.GetResolvedPrimaryFont();
            label.fontSize = theme.ButtonTextSize;
            label.fontStyle = FontStyles.Bold;
            label.alignment = TextAlignmentOptions.Center;
            label.color = theme.HeadingText;
            label.text = "Tile";
            label.enableWordWrapping = true;
            label.overflowMode = TextOverflowModes.Overflow;
            label.raycastTarget = false;
            label.enableAutoSizing = true;
            label.fontSizeMin = 20;
            label.fontSizeMax = theme.ButtonTextSize;

            var labelLayout = labelObject.GetComponent<LayoutElement>();
            labelLayout.minHeight = 112;
            labelLayout.preferredHeight = -1;

            var labelFitter = labelObject.GetComponent<ContentSizeFitter>();
            labelFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            labelFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

            var labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(24f, 16f);
            labelRect.offsetMax = new Vector2(-24f, -16f);

            var tilePrefab = root.AddComponent<TileButtonPrefab>();
            tilePrefab.Bind(button, image, label);

            SavePrefab(root, TileButtonPrefabPath);
        }

        private static RectTransform CreateRect(string name, RectTransform parent = null)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = go.GetComponent<RectTransform>();
            if (parent != null)
            {
                rect.SetParent(parent, false);
            }

            return rect;
        }

        private static void SavePrefab(GameObject root, string path)
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing == null)
            {
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            else
            {
                PrefabUtility.SaveAsPrefabAssetAndConnect(root, path, InteractionMode.AutomatedAction);
            }

            Object.DestroyImmediate(root);
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

        private static void Stretch(RectTransform rectTransform, float left, float bottom, float right, float top)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = new Vector2(left, bottom);
            rectTransform.offsetMax = new Vector2(-right, -top);
        }

        private static void SetTopRect(RectTransform rectTransform, float left, float top, float right, float height)
        {
            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(1f, 1f);
            rectTransform.pivot = new Vector2(0.5f, 1f);
            rectTransform.offsetMin = new Vector2(left, -top - height);
            rectTransform.offsetMax = new Vector2(-right, -top);
        }

        private static void StretchBetween(RectTransform rectTransform, float left, float bottom, float right, float top)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = new Vector2(left, bottom);
            rectTransform.offsetMax = new Vector2(-right, -top);
        }
    }
}
#endif
