using HappyFamily.Data;
using HappyFamily.Gameplay;
using HappyFamily.Save;
using HappyFamily.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

namespace HappyFamily.Core
{
    public class HappyFamilyGameApp : MonoBehaviour
    {
        private const string PrimaryButtonText = "开始整理";
        private const string ContinueButtonText = "继续前进";
        private const string ResetButtonText = "重置进度";

        private PlayerProgressService progressService;
        private HappyFamilySaveData saveData;
        private MvpChapterDefinition chapterDefinition;
        private TidyUpChapterDefinition tidyUpChapter;
        private PairMatchBoard activeBoard;
        private MvpLevelDefinition activeLevel;
        private string currentLevelMessage = string.Empty;

        private Canvas rootCanvas;
        private RectTransform screenRoot;
        private TextMeshProUGUI levelStatusText;
        private readonly List<TileButtonView> tileViews = new List<TileButtonView>();

        private HappyFamilyUiTheme Theme => HappyFamilyUiThemeProvider.GetTheme();

        private void Awake()
        {
            progressService = new PlayerProgressService();
            saveData = progressService.Load();
            chapterDefinition = MvpContentProvider.GetFrontYardChapter();
            tidyUpChapter = TidyUpContentFactory.CreateTidyUpChapter();
        }

        private void Start()
        {
            EnsureUi();
            ShowHome();
        }

        private void EnsureUi()
        {
            if (rootCanvas == null)
            {
                rootCanvas = UiFactory.CreateRootCanvas("HappyFamilyCanvas", transform);
                screenRoot = UiFactory.CreateScreenRoot(rootCanvas.transform, "ScreenRoot");
                var viewportFitter = screenRoot.gameObject.AddComponent<MobileViewportFitter>();
                viewportFitter.Initialize(rootCanvas.GetComponent<RectTransform>(), Theme.ReferenceResolution, Theme.ViewportMargin);
            }

            if (EventSystem.current == null)
            {
                var eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                DontDestroyOnLoad(eventSystemObject);
            }
        }

        private void ShowHome()
        {
            activeBoard = null;
            activeLevel = null;
            levelStatusText = null;
            currentLevelMessage = string.Empty;
            tileViews.Clear();

            UiFactory.ClearChildren(screenRoot);

            var shellPrefab = HappyFamilyScreenPrefabProvider.LoadHomeScreenShell();
            var shell = BuildHomeShell(shellPrefab);

            UiFactory.CreateLabel(shell.TitleRoot, "Title", "幸福人家", Theme.HomeTitleSize, FontStyle.Bold, TextAnchor.MiddleCenter, Theme.HeadingText);
            UiFactory.CreateLabel(shell.TitleRoot, "Subtitle", "MVP 起步闭环：前院整理 -> 幸福星 -> 修院门", Theme.HomeSubtitleSize, FontStyle.Normal, TextAnchor.MiddleCenter, Theme.BodyText);

            var progressCard = UiFactory.CreateCard(shell.ProgressRoot, "ProgressCard", Theme.CardBackground);
            UiFactory.CreateLabel(progressCard, "ChapterName", chapterDefinition.DisplayName, Theme.HomeCardTitleSize, FontStyle.Bold, TextAnchor.MiddleLeft, Theme.HeadingText);
            UiFactory.CreateLabel(progressCard, "Stars", $"幸福星：{saveData.TotalStars}", Theme.HomeBodySize, FontStyle.Normal, TextAnchor.MiddleLeft, Theme.BodyText);
            UiFactory.CreateLabel(progressCard, "Levels", $"已完成关卡：{saveData.CompletedLevelIds.Count}/{chapterDefinition.Levels.Count}", Theme.HomeBodySize, FontStyle.Normal, TextAnchor.MiddleLeft, Theme.BodyText);

            var nextLevel = GetNextLevel();
            var mainButtonText = nextLevel != null ? (saveData.CompletedLevelIds.Count == 0 ? PrimaryButtonText : ContinueButtonText) : "已完成当前 Slice";
            var mainButton = UiFactory.CreateButton(shell.ActionRoot, "StartButton", mainButtonText, OnPrimaryButtonClicked, Theme.PrimaryButton, 108);
            mainButton.interactable = nextLevel != null;

            foreach (var node in chapterDefinition.RenovationNodes)
            {
                RenderRenovationNode(shell.ProgressRoot, node);
            }

            UiFactory.CreateButton(shell.ActionRoot, "ResetButton", ResetButtonText, ResetProgress, Theme.NeutralButton, 92);

            // TidyUp mode button
            if (tidyUpChapter != null && tidyUpChapter.Levels.Count > 0)
            {
                UiFactory.CreateButton(shell.ActionRoot, "TidyUpButton", "整理收纳模式", ShowTidyUpLevelSelect, Theme.InfoButton, 92);
            }
        }

        private void RenderRenovationNode(RectTransform parent, MvpRenovationNodeDefinition nodeDefinition)
        {
            var nodeCard = UiFactory.CreateCard(parent, $"{nodeDefinition.Id}Card", Theme.CardBackground);
            UiFactory.CreateLabel(nodeCard, "NodeTitle", nodeDefinition.DisplayName, 38, FontStyle.Bold, TextAnchor.MiddleLeft, Theme.HeadingText);

            var unlocked = saveData.TotalStars >= nodeDefinition.RequiredStars;
            var statusText = unlocked
                ? $"已解锁，可以开始这个焕新节点。"
                : $"解锁条件：累计获得 {nodeDefinition.RequiredStars} 颗幸福星。";
            UiFactory.CreateLabel(nodeCard, "NodeStatus", statusText, Theme.HomeSubtitleSize, FontStyle.Normal, TextAnchor.MiddleLeft, Theme.BodyText);

            if (!unlocked)
            {
                return;
            }

            if (!saveData.TryGetRenovationSelection(nodeDefinition.Id, out var selectedOptionId))
            {
                UiFactory.CreateLabel(nodeCard, "ChooseHint", "选择一个院门样式，完成前院第一步焕新。", Theme.HomeSubtitleSize, FontStyle.Normal, TextAnchor.MiddleLeft, Theme.BodyText);
                foreach (var option in nodeDefinition.Options)
                {
                    var optionText = $"{option.DisplayName} · {option.Description}";
                    UiFactory.CreateButton(nodeCard, option.Id, optionText, () => SelectRenovationOption(nodeDefinition, option), Theme.SecondaryButton, 94);
                }

                return;
            }

            var selectedOption = nodeDefinition.GetOption(selectedOptionId);
            if (selectedOption == null)
            {
                UiFactory.CreateLabel(nodeCard, "SelectionError", "装修选择状态异常，请重置进度。", 26, FontStyle.Normal, TextAnchor.MiddleLeft, Theme.BodyText);
                return;
            }

            UiFactory.CreateLabel(nodeCard, "SelectedLabel", $"当前方案：{selectedOption.DisplayName}", Theme.HomeBodySize, FontStyle.Bold, TextAnchor.MiddleLeft, Theme.HeadingText);
            UiFactory.CreateLabel(nodeCard, "MemoryTitle", $"回忆卡片：{nodeDefinition.MemoryTitle}", Theme.HomeSubtitleSize, FontStyle.Bold, TextAnchor.MiddleLeft, Theme.HeadingText);
            UiFactory.CreateLabel(nodeCard, "MemoryText", nodeDefinition.MemoryText, 26, FontStyle.Normal, TextAnchor.UpperLeft, Theme.BodyText, 120);
        }

        private void OnPrimaryButtonClicked()
        {
            var nextLevel = GetNextLevel();
            if (nextLevel == null)
            {
                return;
            }

            ShowLevel(nextLevel);
        }

        private void ShowLevel(MvpLevelDefinition levelDefinition)
        {
            activeLevel = levelDefinition;
            activeBoard = PairMatchBoard.Create(levelDefinition);

            UiFactory.ClearChildren(screenRoot);
            tileViews.Clear();

            var shellPrefab = HappyFamilyScreenPrefabProvider.LoadLevelScreenShell();
            var shell = BuildLevelShell(shellPrefab);

            UiFactory.CreateLabel(shell.HeaderRoot, "LevelTitle", levelDefinition.DisplayName, Theme.LevelTitleSize, FontStyle.Bold, TextAnchor.MiddleCenter, Theme.HeadingText);
            UiFactory.CreateLabel(shell.HeaderRoot, "LevelGoal", levelDefinition.Description, Theme.LevelBodySize, FontStyle.Normal, TextAnchor.MiddleCenter, Theme.BodyText);

            levelStatusText = UiFactory.CreateLabel(shell.HeaderRoot, "LevelStatus", string.Empty, Theme.LevelBodySize, FontStyle.Normal, TextAnchor.MiddleCenter, Theme.BodyText);
            RefreshLevelStatus("点两个相同物件即可配对。先完成一次完整闭环。");

            var boardWidth = screenRoot.rect.width > 0f ? screenRoot.rect.width - Theme.HorizontalPadding * 2f : 702f;
            var cellWidth = Mathf.Floor((boardWidth - Theme.BoardPadding * 2f - Theme.TileGap * 2f) / 3f);
            var cellHeight = 120f;
            var boardPadding = Mathf.RoundToInt(Theme.BoardPadding);
            var boardRoot = UiFactory.CreateGrid(shell.BoardRoot, "BoardGrid", 3, new Vector2(cellWidth, cellHeight), new Vector2(Theme.TileGap, Theme.TileGap), new RectOffset(boardPadding, boardPadding, boardPadding, boardPadding));
            StretchRect(boardRoot, 0f, 0f, 0f, 0f);
            for (var index = 0; index < activeBoard.Tiles.Count; index++)
            {
                var tile = activeBoard.Tiles[index];
                var capturedIndex = index;
                var tileView = UiFactory.CreateTileButton(boardRoot, $"Tile{index}", tile.DisplayLabel, () => OnTileClicked(capturedIndex));
                tileViews.Add(tileView);
            }

            shell.ToolbarRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, boardWidth);
            UiFactory.CreateButton(shell.ToolbarRoot, "HintButton", "提示", ShowHint, Theme.WarningButton, Mathf.RoundToInt(Theme.BottomToolbarHeight), 180);
            UiFactory.CreateButton(shell.ToolbarRoot, "ShuffleButton", "洗牌", ShuffleBoard, Theme.InfoButton, Mathf.RoundToInt(Theme.BottomToolbarHeight), 180);
            UiFactory.CreateButton(shell.ToolbarRoot, "BackButton", "返回前院", ShowHome, Theme.NeutralButton, Mathf.RoundToInt(Theme.BottomToolbarHeight), 180);

            RefreshBoardUi();
        }

        private void OnTileClicked(int index)
        {
            if (activeBoard == null)
            {
                return;
            }

            var result = activeBoard.TrySelect(index);
            switch (result.State)
            {
                case PairMatchMoveState.Selected:
                    RefreshLevelStatus($"已选中：{result.PrimaryLabel}");
                    break;
                case PairMatchMoveState.Deselected:
                    RefreshLevelStatus("已取消选择。");
                    break;
                case PairMatchMoveState.Matched:
                    RefreshLevelStatus($"配对成功：{result.PrimaryLabel}。");
                    break;
                case PairMatchMoveState.Mismatched:
                    RefreshLevelStatus("没有配成，再试一次。");
                    break;
                default:
                    return;
            }

            RefreshBoardUi();

            if (activeBoard.IsCompleted)
            {
                CompleteLevel();
                return;
            }

            if (activeBoard.IsFailed)
            {
                RefreshLevelStatus("步数用完了，这一关会直接重开。");
                ShowLevel(activeLevel);
            }
        }

        private void ShowHint()
        {
            if (activeBoard == null)
            {
                return;
            }

            var hint = activeBoard.FindHint();
            if (!hint.HasValue)
            {
                RefreshLevelStatus("当前没有可用提示，试试洗牌。");
                return;
            }

            var leftTile = activeBoard.Tiles[hint.Value.LeftIndex];
            RefreshLevelStatus($"提示：试试配对“{leftTile.DisplayLabel}”。");
        }

        private void ShuffleBoard()
        {
            if (activeBoard == null)
            {
                return;
            }

            activeBoard.ShuffleRemaining();
            RefreshLevelStatus("已洗牌，盘面重新整理好了。");
            RefreshBoardUi();
        }

        private void CompleteLevel()
        {
            saveData.CompleteLevel(activeLevel.Id, activeLevel.StarReward);
            progressService.Save(saveData);

            var rewardNode = chapterDefinition.GetFirstUnlockableNode(saveData.TotalStars);
            if (rewardNode != null && !saveData.TryGetRenovationSelection(rewardNode.Id, out _))
            {
                RefreshLevelStatus($"通关成功，获得 {activeLevel.StarReward} 颗幸福星，前院焕新已解锁。");
            }
            else
            {
                RefreshLevelStatus($"通关成功，获得 {activeLevel.StarReward} 颗幸福星。");
            }

            ShowHome();
        }

        private void SelectRenovationOption(MvpRenovationNodeDefinition nodeDefinition, MvpRenovationOptionDefinition optionDefinition)
        {
            saveData.SetRenovationSelection(nodeDefinition.Id, optionDefinition.Id);
            progressService.Save(saveData);
            ShowHome();
        }

        private void ResetProgress()
        {
            saveData = HappyFamilySaveData.CreateDefault();
            progressService.Save(saveData);
            ShowHome();
        }

        private MvpLevelDefinition GetNextLevel()
        {
            foreach (var level in chapterDefinition.Levels)
            {
                if (!saveData.IsLevelCompleted(level.Id))
                {
                    return level;
                }
            }

            return null;
        }

        #region TidyUp Mode

        private void ShowTidyUpLevelSelect()
        {
            UiFactory.ClearChildren(screenRoot);

            // 主面板
            var panel = UiFactory.CreateAbsolutePanel(screenRoot, "TidyUpSelectPanel", Theme.HomeBackground);
            StretchRect(panel, 0f, 0f, 0f, 0f);

            // 顶部标题区域 (固定高度)
            var headerRoot = new GameObject("HeaderRoot", typeof(RectTransform), typeof(VerticalLayoutGroup)).GetComponent<RectTransform>();
            headerRoot.SetParent(panel, false);
            headerRoot.anchorMin = new Vector2(0f, 1f);
            headerRoot.anchorMax = new Vector2(1f, 1f);
            headerRoot.pivot = new Vector2(0.5f, 1f);
            headerRoot.anchoredPosition = Vector2.zero;
            headerRoot.sizeDelta = new Vector2(0f, 120f);
            var headerLayout = headerRoot.GetComponent<VerticalLayoutGroup>();
            headerLayout.padding = new RectOffset(24, 24, 24, 12);
            headerLayout.spacing = 8;
            headerLayout.childAlignment = TextAnchor.MiddleCenter;
            headerLayout.childControlHeight = false;
            headerLayout.childForceExpandHeight = false;

            UiFactory.CreateLabel(headerRoot, "Title", tidyUpChapter.DisplayName, Theme.HomeTitleSize, FontStyle.Bold, TextAnchor.MiddleCenter, Theme.HeadingText);
            UiFactory.CreateLabel(headerRoot, "Subtitle", "选择关卡开始整理收纳", Theme.HomeSubtitleSize, FontStyle.Normal, TextAnchor.MiddleCenter, Theme.BodyText);

            // 底部按钮区域 (固定高度)
            var footerRoot = new GameObject("FooterRoot", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
            footerRoot.SetParent(panel, false);
            footerRoot.anchorMin = new Vector2(0f, 0f);
            footerRoot.anchorMax = new Vector2(1f, 0f);
            footerRoot.pivot = new Vector2(0.5f, 0f);
            footerRoot.anchoredPosition = Vector2.zero;
            footerRoot.sizeDelta = new Vector2(0f, 100f);
            footerRoot.GetComponent<Image>().color = Theme.HomeBackground;

            var backButton = UiFactory.CreateButton(footerRoot, "BackButton", "返回首页", ShowHome, Theme.NeutralButton, 56, 200);
            var backRect = backButton.GetComponent<RectTransform>();
            backRect.anchorMin = new Vector2(0.5f, 0.5f);
            backRect.anchorMax = new Vector2(0.5f, 0.5f);
            backRect.anchoredPosition = Vector2.zero;

            // 中间滚动区域 (关卡列表)
            var scrollViewObj = new GameObject("ScrollView", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
            var scrollRect = scrollViewObj.GetComponent<RectTransform>();
            scrollRect.SetParent(panel, false);
            scrollRect.anchorMin = Vector2.zero;
            scrollRect.anchorMax = Vector2.one;
            scrollRect.offsetMin = new Vector2(0f, 100f); // 底部留出按钮空间
            scrollRect.offsetMax = new Vector2(0f, -120f); // 顶部留出标题空间
            scrollViewObj.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f); // 透明背景

            // 内容容器
            var contentObj = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            var contentRect = contentObj.GetComponent<RectTransform>();
            contentRect.SetParent(scrollRect, false);
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(0f, 0f);

            var contentLayout = contentObj.GetComponent<VerticalLayoutGroup>();
            contentLayout.padding = new RectOffset(24, 24, 16, 24);
            contentLayout.spacing = 16;
            contentLayout.childAlignment = TextAnchor.UpperCenter;
            contentLayout.childControlHeight = false;
            contentLayout.childForceExpandHeight = false;
            contentLayout.childControlWidth = true;
            contentLayout.childForceExpandWidth = true;

            var contentFitter = contentObj.GetComponent<ContentSizeFitter>();
            contentFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // 设置ScrollRect
            var scrollComponent = scrollViewObj.GetComponent<ScrollRect>();
            scrollComponent.content = contentRect;
            scrollComponent.horizontal = false;
            scrollComponent.vertical = true;
            scrollComponent.movementType = ScrollRect.MovementType.Elastic;
            scrollComponent.elasticity = 0.1f;

            // Viewport (裁剪区域)
            var viewportObj = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            var viewportRect = viewportObj.GetComponent<RectTransform>();
            viewportRect.SetParent(scrollRect, false);
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;
            viewportObj.GetComponent<Image>().color = Color.white;
            viewportObj.GetComponent<Mask>().showMaskGraphic = false;
            scrollComponent.viewport = viewportRect;

            // 重新设置content的父级为viewport
            contentRect.SetParent(viewportRect, false);
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;

            // 添加关卡卡片
            foreach (var level in tidyUpChapter.Levels)
            {
                var capturedLevel = level;
                var levelCard = UiFactory.CreateCard(contentRect, $"Level_{level.Id}", Theme.CardBackground);
                UiFactory.CreateLabel(levelCard, "LevelName", level.DisplayName, Theme.HomeCardTitleSize, FontStyle.Bold, TextAnchor.MiddleLeft, Theme.HeadingText);
                UiFactory.CreateLabel(levelCard, "LevelDesc", level.Description, Theme.HomeBodySize, FontStyle.Normal, TextAnchor.MiddleLeft, Theme.BodyText);
                UiFactory.CreateLabel(levelCard, "LevelInfo", $"物品: {level.Items.Count}种  网格: {level.GridWidth}x{level.GridHeight}  层数: {level.MaxLayers}", Theme.HomeSubtitleSize, FontStyle.Normal, TextAnchor.MiddleLeft, Theme.BodyText);
                UiFactory.CreateButton(levelCard, "PlayButton", "开始整理", () => ShowTidyUpLevel(capturedLevel), Theme.PrimaryButton, 60);
            }
        }

        private void ShowTidyUpLevel(TidyUpLevelDefinition levelDefinition)
        {
            UiFactory.ClearChildren(screenRoot);

            // Create main panel with warm background
            var panel = UiFactory.CreateAbsolutePanel(screenRoot, "TidyUpPanel", new Color(0.96f, 0.94f, 0.90f));
            StretchRect(panel, 0f, 0f, 0f, 0f);

            // Top bar with back button and title
            var topBar = new GameObject("TopBar", typeof(RectTransform)).GetComponent<RectTransform>();
            topBar.SetParent(panel, false);
            topBar.anchorMin = new Vector2(0f, 1f);
            topBar.anchorMax = new Vector2(1f, 1f);
            topBar.pivot = new Vector2(0.5f, 1f);
            topBar.anchoredPosition = Vector2.zero;
            topBar.sizeDelta = new Vector2(0f, 80f);

            // Back button (top left, text only style)
            var backButton = UiFactory.CreateButton(topBar.transform, "BackButton", "返回", () => { }, Theme.NeutralButton, 44, 80);
            var backRect = backButton.GetComponent<RectTransform>();
            backRect.anchorMin = new Vector2(0f, 0.5f);
            backRect.anchorMax = new Vector2(0f, 0.5f);
            backRect.pivot = new Vector2(0f, 0.5f);
            backRect.anchoredPosition = new Vector2(12f, 0f);
            // Make button background transparent
            var backImage = backButton.GetComponent<Image>();
            if (backImage != null) backImage.color = new Color(1f, 1f, 1f, 0f);
            var backText = backButton.GetComponentInChildren<TextMeshProUGUI>();
            if (backText != null)
            {
                backText.color = new Color(0.45f, 0.42f, 0.38f);
                backText.fontSize = 20;
            }

            // Title (centered)
            var titleLabel = UiFactory.CreateLabel(topBar.transform, "Title", levelDefinition.DisplayName, 32, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.3f, 0.28f, 0.25f));
            var titleRect = titleLabel.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.2f, 0f);
            titleRect.anchorMax = new Vector2(0.8f, 1f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;

            // Status label (top right)
            var statusLabel = UiFactory.CreateLabel(topBar.transform, "StatusLabel", "", 20, FontStyle.Normal, TextAnchor.MiddleRight, new Color(0.5f, 0.48f, 0.45f));
            var statusRect = statusLabel.GetComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(1f, 0.5f);
            statusRect.anchorMax = new Vector2(1f, 0.5f);
            statusRect.pivot = new Vector2(1f, 0.5f);
            statusRect.anchoredPosition = new Vector2(-20f, 0f);
            statusRect.sizeDelta = new Vector2(120f, 40f);

            // Tiles container (main game area)
            var tilesContainer = new GameObject("TilesContainer", typeof(RectTransform)).GetComponent<RectTransform>();
            tilesContainer.SetParent(panel, false);
            tilesContainer.anchorMin = Vector2.zero;
            tilesContainer.anchorMax = Vector2.one;
            tilesContainer.offsetMin = new Vector2(12f, 120f);
            tilesContainer.offsetMax = new Vector2(-12f, -90f);

            // Slots container (bottom bar)
            var slotsContainer = new GameObject("SlotsContainer", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
            slotsContainer.SetParent(panel, false);
            slotsContainer.anchorMin = new Vector2(0f, 0f);
            slotsContainer.anchorMax = new Vector2(1f, 0f);
            slotsContainer.pivot = new Vector2(0.5f, 0f);
            slotsContainer.anchoredPosition = new Vector2(0f, 24f);
            slotsContainer.sizeDelta = new Vector2(-32f, 72f);
            var slotsImage = slotsContainer.GetComponent<Image>();
            slotsImage.color = new Color(0.88f, 0.85f, 0.80f);

            // Initialize the TidyUp screen with runtime initializer
            var runtimeInit = panel.gameObject.AddComponent<TidyUpScreenRuntimeInitializer>();
            runtimeInit.Initialize(levelDefinition, tilesContainer, slotsContainer, backButton, titleLabel, statusLabel,
                ShowTidyUpLevelSelect, OnTidyUpLevelComplete);
        }

        private void OnTidyUpLevelComplete(bool success)
        {
            if (success)
            {
                Debug.Log("TidyUp level completed successfully!");
            }
            else
            {
                Debug.Log("TidyUp level failed.");
            }

            // Return to level select after a delay
            Invoke(nameof(ShowTidyUpLevelSelect), 1.5f);
        }

        #endregion

        private void RefreshBoardUi()
        {
            if (activeBoard == null)
            {
                return;
            }

            for (var index = 0; index < tileViews.Count; index++)
            {
                var tile = activeBoard.Tiles[index];
                var view = tileViews[index];

                view.Text.text = tile.IsRemoved ? "已整理" : tile.DisplayLabel;
                view.Button.interactable = !tile.IsRemoved;

                var colors = view.Button.colors;
                colors.normalColor = tile.IsRemoved
                    ? Theme.TileRemovedBackground
                    : tile.IsSelected
                        ? Theme.TileSelectedBackground
                        : Theme.TileBackground;
                colors.highlightedColor = colors.normalColor;
                colors.selectedColor = colors.normalColor;
                colors.pressedColor = tile.IsSelected ? new Color(0.88f, 0.73f, 0.40f, 1f) : new Color(0.89f, 0.92f, 0.88f, 1f);
                colors.disabledColor = new Color(0.78f, 0.80f, 0.78f, 1f);
                view.Button.colors = colors;
            }

            ApplyLevelStatus();
        }

        private void RefreshLevelStatus(string message)
        {
            currentLevelMessage = message;
            ApplyLevelStatus();
        }

        private void ApplyLevelStatus()
        {
            if (levelStatusText == null || activeBoard == null)
            {
                return;
            }

            var prefix = $"剩余步数：{activeBoard.RemainingSteps}    剩余配对：{activeBoard.RemainingPairs}";
            levelStatusText.text = $"{prefix}\n{currentLevelMessage}";
        }

        private static void StretchRect(RectTransform rectTransform, float left, float bottom, float right, float top)
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

        private HomeScreenShell BuildFallbackHomeShell()
        {
            var panel = UiFactory.CreatePanel(screenRoot, "HomePanel", Theme.HomeBackground);
            var titleRoot = new GameObject("TitleRoot", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter)).GetComponent<RectTransform>();
            titleRoot.SetParent(panel, false);

            var progressRoot = new GameObject("ProgressRoot", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter)).GetComponent<RectTransform>();
            progressRoot.SetParent(panel, false);

            var actionRoot = new GameObject("ActionRoot", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter)).GetComponent<RectTransform>();
            actionRoot.SetParent(panel, false);

            ConfigureColumn(titleRoot, 12);
            ConfigureColumn(progressRoot, 20);
            ConfigureColumn(actionRoot, 18);

            var shell = panel.gameObject.AddComponent<HomeScreenShell>();
            shell.Bind(panel, titleRoot, progressRoot, actionRoot);
            return shell;
        }

        private HomeScreenShell BuildHomeShell(HomeScreenShell shellPrefab)
        {
            if (shellPrefab == null)
            {
                return BuildFallbackHomeShell();
            }

            var instance = Instantiate(shellPrefab, screenRoot, false);
            if (instance == null || !instance.IsValid)
            {
                if (instance != null)
                {
                    Destroy(instance.gameObject);
                }

                return BuildFallbackHomeShell();
            }

            var rectTransform = instance.GetComponent<RectTransform>();
            StretchRect(rectTransform, 0f, 0f, 0f, 0f);
            return instance;
        }

        private LevelScreenShell BuildFallbackLevelShell()
        {
            var panel = UiFactory.CreateAbsolutePanel(screenRoot, "LevelPanel", Theme.LevelBackground);
            StretchRect(panel, 0f, 0f, 0f, 0f);

            var headerRoot = UiFactory.CreateAbsolutePanel(panel, "HeaderRoot", Theme.HeaderBackground);
            SetTopRect(headerRoot, Theme.HorizontalPadding, Theme.HorizontalPadding, Theme.HorizontalPadding, Theme.HeaderHeight);
            ConfigureAbsoluteColumn(headerRoot, new RectOffset(28, 28, 22, 18), 8);

            var boardRoot = UiFactory.CreateAbsolutePanel(panel, "BoardRoot", Theme.BoardBackground);
            StretchBetween(boardRoot, Theme.HorizontalPadding, Theme.BoardBottomInset, Theme.HorizontalPadding, Theme.BoardTopInset);

            var toolbarRoot = new GameObject("ToolbarRoot", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter)).GetComponent<RectTransform>();
            toolbarRoot.SetParent(panel, false);
            toolbarRoot.anchorMin = new Vector2(0.5f, 0f);
            toolbarRoot.anchorMax = new Vector2(0.5f, 0f);
            toolbarRoot.pivot = new Vector2(0.5f, 0f);
            toolbarRoot.anchoredPosition = new Vector2(0f, Theme.BottomToolbarInset);
            var toolbarLayout = toolbarRoot.GetComponent<HorizontalLayoutGroup>();
            toolbarLayout.spacing = 14;
            toolbarLayout.padding = new RectOffset(0, 0, 12, 12);
            toolbarLayout.childAlignment = TextAnchor.MiddleCenter;
            toolbarLayout.childControlHeight = false;
            toolbarLayout.childForceExpandHeight = false;
            toolbarLayout.childControlWidth = false;
            toolbarLayout.childForceExpandWidth = false;
            var fitter = toolbarRoot.GetComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var shell = panel.gameObject.AddComponent<LevelScreenShell>();
            shell.Bind(panel, headerRoot, boardRoot, toolbarRoot);
            return shell;
        }

        private LevelScreenShell BuildLevelShell(LevelScreenShell shellPrefab)
        {
            if (shellPrefab == null)
            {
                return BuildFallbackLevelShell();
            }

            var instance = Instantiate(shellPrefab, screenRoot, false);
            if (instance == null || !instance.IsValid)
            {
                if (instance != null)
                {
                    Destroy(instance.gameObject);
                }

                return BuildFallbackLevelShell();
            }

            StretchRect(instance.Root, 0f, 0f, 0f, 0f);
            return instance;
        }

        private static void ConfigureColumn(RectTransform rectTransform, int spacing)
        {
            var layout = rectTransform.GetComponent<VerticalLayoutGroup>();
            layout.spacing = spacing;
            layout.childControlHeight = false;
            layout.childForceExpandHeight = false;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.childAlignment = TextAnchor.UpperCenter;

            var fitter = rectTransform.GetComponent<ContentSizeFitter>();
            if (fitter != null)
            {
                fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
        }

        private static void ConfigureAbsoluteColumn(RectTransform rectTransform, RectOffset padding, int spacing)
        {
            var layout = rectTransform.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = padding;
            layout.spacing = spacing;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlHeight = false;
            layout.childForceExpandHeight = false;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
        }
    }
}
