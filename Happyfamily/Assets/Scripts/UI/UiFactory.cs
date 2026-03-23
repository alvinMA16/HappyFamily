using System;
using HappyFamily.UI.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HappyFamily.UI
{
    public readonly struct TileButtonView
    {
        public TileButtonView(Button button, TextMeshProUGUI text)
        {
            Button = button;
            Text = text;
        }

        public Button Button { get; }
        public TextMeshProUGUI Text { get; }
    }

    public static class UiFactory
    {
        public static Canvas CreateRootCanvas(string name, Transform parent)
        {
            var theme = HappyFamilyUiThemeProvider.GetTheme();
            var canvasObject = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(parent, false);

            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = theme.ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 1f;

            return canvas;
        }

        public static RectTransform CreateScreenRoot(Transform parent, string name)
        {
            var theme = HappyFamilyUiThemeProvider.GetTheme();
            var root = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(RectMask2D));
            root.transform.SetParent(parent, false);

            var rectTransform = root.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);

            var image = root.GetComponent<Image>();
            image.color = theme.ScreenBackground;

            FitRectToPortrait(rectTransform, parent as RectTransform, theme.ReferenceResolution, theme.ViewportMargin);
            return rectTransform;
        }

        public static RectTransform CreatePanel(Transform parent, string name, Color backgroundColor)
        {
            var panel = new GameObject(name, typeof(Image), typeof(VerticalLayoutGroup));
            panel.transform.SetParent(parent, false);

            var image = panel.GetComponent<Image>();
            image.color = backgroundColor;

            var layout = panel.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(48, 48, 60, 60);
            layout.spacing = 24;
            layout.childControlHeight = false;
            layout.childForceExpandHeight = false;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.childAlignment = TextAnchor.UpperCenter;

            var rectTransform = panel.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            return rectTransform;
        }

        public static RectTransform CreateAbsolutePanel(Transform parent, string name, Color backgroundColor)
        {
            var panel = new GameObject(name, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);

            var image = panel.GetComponent<Image>();
            image.color = backgroundColor;

            return panel.GetComponent<RectTransform>();
        }

        public static RectTransform CreateCard(Transform parent, string name, Color backgroundColor)
        {
            var card = new GameObject(name, typeof(Image), typeof(VerticalLayoutGroup), typeof(LayoutElement), typeof(ContentSizeFitter));
            card.transform.SetParent(parent, false);

            var image = card.GetComponent<Image>();
            image.color = backgroundColor;

            var layout = card.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(32, 32, 28, 28);
            layout.spacing = 18;
            layout.childControlHeight = false;
            layout.childForceExpandHeight = false;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.childAlignment = TextAnchor.UpperLeft;

            var layoutElement = card.GetComponent<LayoutElement>();
            layoutElement.minHeight = 0;
            layoutElement.flexibleHeight = 0;

            var fitter = card.GetComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            return card.GetComponent<RectTransform>();
        }

        public static TextMeshProUGUI CreateLabel(
            Transform parent,
            string name,
            string content,
            int fontSize,
            FontStyle fontStyle,
            TextAnchor alignment,
            Color textColor,
            int minHeight = 0)
        {
            var labelObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement), typeof(ContentSizeFitter));
            labelObject.transform.SetParent(parent, false);

            var label = labelObject.GetComponent<TextMeshProUGUI>();
            label.font = HappyFamilyUiThemeProvider.GetResolvedPrimaryFont();
            label.fontSize = fontSize;
            label.fontStyle = ToTmpFontStyle(fontStyle);
            label.alignment = ToTmpAlignment(alignment);
            label.color = textColor;
            label.text = content;
            label.enableWordWrapping = true;
            label.overflowMode = TextOverflowModes.Overflow;
            label.raycastTarget = false;
            label.extraPadding = false;
            label.isTextObjectScaleStatic = true;

            var layout = labelObject.GetComponent<LayoutElement>();
            layout.minHeight = minHeight;
            layout.preferredHeight = -1;

            var fitter = labelObject.GetComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            return label;
        }

        public static Button CreateButton(
            Transform parent,
            string name,
            string label,
            Action onClick,
            Color backgroundColor,
            int height,
            int width = -1)
        {
            var theme = HappyFamilyUiThemeProvider.GetTheme();
            var buttonObject = new GameObject(name, typeof(Image), typeof(Button), typeof(LayoutElement));
            buttonObject.transform.SetParent(parent, false);

            var image = buttonObject.GetComponent<Image>();
            image.color = backgroundColor;

            var button = buttonObject.GetComponent<Button>();
            var colors = button.colors;
            colors.normalColor = backgroundColor;
            colors.highlightedColor = backgroundColor * 1.05f;
            colors.pressedColor = backgroundColor * 0.92f;
            colors.selectedColor = backgroundColor;
            colors.disabledColor = new Color(0.70f, 0.70f, 0.70f, 1f);
            button.colors = colors;
            button.onClick.AddListener(() => onClick?.Invoke());

            var layoutElement = buttonObject.GetComponent<LayoutElement>();
            layoutElement.preferredHeight = height;
            if (width > 0)
            {
                layoutElement.preferredWidth = width;
            }

            var buttonText = CreateLabel(buttonObject.transform, "Label", label, theme.ButtonTextSize, FontStyle.Bold, TextAnchor.MiddleCenter, theme.LightText, height - 20);
            buttonText.enableAutoSizing = true;
            buttonText.fontSizeMin = 20;
            buttonText.fontSizeMax = theme.ButtonTextSize;
            var buttonTextFitter = buttonText.GetComponent<ContentSizeFitter>();
            buttonTextFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            buttonTextFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
            var rectTransform = buttonText.rectTransform;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = new Vector2(24f, 16f);
            rectTransform.offsetMax = new Vector2(-24f, -16f);

            return button;
        }

        public static RectTransform CreateHorizontalGroup(Transform parent, string name, int spacing, TextAnchor alignment)
        {
            var group = new GameObject(name, typeof(HorizontalLayoutGroup), typeof(LayoutElement), typeof(ContentSizeFitter));
            group.transform.SetParent(parent, false);

            var layout = group.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = spacing;
            layout.padding = new RectOffset(0, 0, 12, 12);
            layout.childControlHeight = false;
            layout.childControlWidth = false;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = false;
            layout.childAlignment = alignment;

            var layoutElement = group.GetComponent<LayoutElement>();
            layoutElement.minHeight = 116;

            var fitter = group.GetComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            return group.GetComponent<RectTransform>();
        }

        public static RectTransform CreateGrid(Transform parent, string name, int columns, Vector2 cellSize, Vector2 spacing, RectOffset padding)
        {
            var grid = new GameObject(name, typeof(Image), typeof(GridLayoutGroup), typeof(LayoutElement));
            grid.transform.SetParent(parent, false);

            var image = grid.GetComponent<Image>();
            image.color = new Color(0.97f, 0.98f, 0.96f, 1f);

            var layout = grid.GetComponent<GridLayoutGroup>();
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = columns;
            layout.cellSize = cellSize;
            layout.spacing = spacing;
            layout.padding = padding;
            layout.childAlignment = TextAnchor.UpperCenter;

            var layoutElement = grid.GetComponent<LayoutElement>();
            layoutElement.preferredHeight = 700;

            return grid.GetComponent<RectTransform>();
        }

        public static TileButtonView CreateTileButton(Transform parent, string name, string label, Action onClick)
        {
            var theme = HappyFamilyUiThemeProvider.GetTheme();
            var button = CreateButton(parent, name, label, onClick, theme.TileBackground, 132);
            var text = button.GetComponentInChildren<TextMeshProUGUI>();
            text.color = theme.HeadingText;
            return new TileButtonView(button, text);
        }

        public static RectTransform CreateCardWithPrefab(Transform parent, string name, Color backgroundColor)
        {
            var prefab = UiComponentPrefabProvider.LoadCardPrefab();
            if (prefab == null || !prefab.IsValid)
            {
                return CreateCard(parent, name, backgroundColor);
            }

            var instance = UnityEngine.Object.Instantiate(prefab, parent, false);
            instance.name = name;
            instance.SetBackgroundColor(backgroundColor);
            return instance.ContentRoot;
        }

        public static Button CreateButtonWithPrefab(
            Transform parent,
            string name,
            string label,
            Action onClick,
            Color backgroundColor,
            int height,
            int width = -1)
        {
            var prefab = UiComponentPrefabProvider.LoadButtonPrefab();
            if (prefab == null || !prefab.IsValid)
            {
                return CreateButton(parent, name, label, onClick, backgroundColor, height, width);
            }

            var instance = UnityEngine.Object.Instantiate(prefab, parent, false);
            instance.name = name;
            instance.Configure(label, onClick, backgroundColor, height, width);
            return instance.Button;
        }

        public static TileButtonView CreateTileButtonWithPrefab(Transform parent, string name, string label, Action onClick)
        {
            var theme = HappyFamilyUiThemeProvider.GetTheme();
            var prefab = UiComponentPrefabProvider.LoadTileButtonPrefab();
            if (prefab == null || !prefab.IsValid)
            {
                return CreateTileButton(parent, name, label, onClick);
            }

            var instance = UnityEngine.Object.Instantiate(prefab, parent, false);
            instance.name = name;
            instance.Configure(label, onClick, theme.TileBackground, theme.HeadingText);
            return new TileButtonView(instance.Button, instance.Label);
        }

        public static void ClearChildren(Transform parent)
        {
            for (var index = parent.childCount - 1; index >= 0; index--)
            {
                UnityEngine.Object.Destroy(parent.GetChild(index).gameObject);
            }
        }

        private static FontStyles ToTmpFontStyle(FontStyle fontStyle)
        {
            return fontStyle switch
            {
                FontStyle.Bold => FontStyles.Bold,
                FontStyle.Italic => FontStyles.Italic,
                FontStyle.BoldAndItalic => FontStyles.Bold | FontStyles.Italic,
                _ => FontStyles.Normal
            };
        }

        private static TextAlignmentOptions ToTmpAlignment(TextAnchor alignment)
        {
            return alignment switch
            {
                TextAnchor.UpperLeft => TextAlignmentOptions.TopLeft,
                TextAnchor.UpperCenter => TextAlignmentOptions.Top,
                TextAnchor.UpperRight => TextAlignmentOptions.TopRight,
                TextAnchor.MiddleLeft => TextAlignmentOptions.Left,
                TextAnchor.MiddleCenter => TextAlignmentOptions.Center,
                TextAnchor.MiddleRight => TextAlignmentOptions.Right,
                TextAnchor.LowerLeft => TextAlignmentOptions.BottomLeft,
                TextAnchor.LowerCenter => TextAlignmentOptions.Bottom,
                TextAnchor.LowerRight => TextAlignmentOptions.BottomRight,
                _ => TextAlignmentOptions.Center
            };
        }

        private static void FitRectToPortrait(RectTransform target, RectTransform parent, Vector2 referenceSize, float margin)
        {
            if (target == null || parent == null)
            {
                return;
            }

            var parentSize = parent.rect.size;
            var availableWidth = Mathf.Max(0f, parentSize.x - margin * 2f);
            var availableHeight = Mathf.Max(0f, parentSize.y - margin * 2f);
            var targetAspect = referenceSize.x / referenceSize.y;
            var availableAspect = availableHeight <= 0f ? targetAspect : availableWidth / availableHeight;

            float width;
            float height;
            if (availableAspect > targetAspect)
            {
                height = availableHeight;
                width = height * targetAspect;
            }
            else
            {
                width = availableWidth;
                height = width / targetAspect;
            }

            target.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            target.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            target.anchoredPosition = Vector2.zero;
        }
    }
}
