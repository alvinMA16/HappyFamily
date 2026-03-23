using HappyFamily.Data;
using HappyFamily.Gameplay;
using HappyFamily.Save;
using HappyFamily.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
        private PairMatchBoard activeBoard;
        private MvpLevelDefinition activeLevel;
        private string currentLevelMessage = string.Empty;

        private Canvas rootCanvas;
        private RectTransform screenRoot;
        private TextMeshProUGUI levelStatusText;
        private readonly List<TileButtonView> tileViews = new List<TileButtonView>();

        private void Awake()
        {
            progressService = new PlayerProgressService();
            saveData = progressService.Load();
            chapterDefinition = MvpContentFactory.CreateFrontYardChapter();
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
                viewportFitter.Initialize(rootCanvas.GetComponent<RectTransform>(), new Vector2(750f, 1334f), 24f);
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

            var panel = UiFactory.CreatePanel(screenRoot, "HomePanel", new Color(0.95f, 0.91f, 0.82f, 1f));
            UiFactory.CreateLabel(panel, "Title", "幸福人家", 58, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.29f, 0.19f, 0.13f));
            UiFactory.CreateLabel(panel, "Subtitle", "MVP 起步闭环：前院整理 -> 幸福星 -> 修院门", 28, FontStyle.Normal, TextAnchor.MiddleCenter, new Color(0.36f, 0.25f, 0.18f));

            var progressCard = UiFactory.CreateCard(panel, "ProgressCard", new Color(0.99f, 0.98f, 0.94f, 1f));
            UiFactory.CreateLabel(progressCard, "ChapterName", chapterDefinition.DisplayName, 42, FontStyle.Bold, TextAnchor.MiddleLeft, new Color(0.24f, 0.17f, 0.12f));
            UiFactory.CreateLabel(progressCard, "Stars", $"幸福星：{saveData.TotalStars}", 34, FontStyle.Normal, TextAnchor.MiddleLeft, new Color(0.33f, 0.25f, 0.18f));
            UiFactory.CreateLabel(progressCard, "Levels", $"已完成关卡：{saveData.CompletedLevelIds.Count}/{chapterDefinition.Levels.Count}", 30, FontStyle.Normal, TextAnchor.MiddleLeft, new Color(0.33f, 0.25f, 0.18f));

            var nextLevel = GetNextLevel();
            var mainButtonText = nextLevel != null ? (saveData.CompletedLevelIds.Count == 0 ? PrimaryButtonText : ContinueButtonText) : "已完成当前 Slice";
            var mainButton = UiFactory.CreateButton(panel, "StartButton", mainButtonText, OnPrimaryButtonClicked, new Color(0.73f, 0.35f, 0.18f, 1f), 108);
            mainButton.interactable = nextLevel != null;

            foreach (var node in chapterDefinition.RenovationNodes)
            {
                RenderRenovationNode(panel, node);
            }

            UiFactory.CreateButton(panel, "ResetButton", ResetButtonText, ResetProgress, new Color(0.50f, 0.54f, 0.56f, 1f), 92);
        }

        private void RenderRenovationNode(RectTransform parent, MvpRenovationNodeDefinition nodeDefinition)
        {
            var nodeCard = UiFactory.CreateCard(parent, $"{nodeDefinition.Id}Card", new Color(0.99f, 0.97f, 0.90f, 1f));
            UiFactory.CreateLabel(nodeCard, "NodeTitle", nodeDefinition.DisplayName, 38, FontStyle.Bold, TextAnchor.MiddleLeft, new Color(0.28f, 0.19f, 0.13f));

            var unlocked = saveData.TotalStars >= nodeDefinition.RequiredStars;
            var statusText = unlocked
                ? $"已解锁，可以开始这个焕新节点。"
                : $"解锁条件：累计获得 {nodeDefinition.RequiredStars} 颗幸福星。";
            UiFactory.CreateLabel(nodeCard, "NodeStatus", statusText, 28, FontStyle.Normal, TextAnchor.MiddleLeft, new Color(0.35f, 0.26f, 0.19f));

            if (!unlocked)
            {
                return;
            }

            if (!saveData.TryGetRenovationSelection(nodeDefinition.Id, out var selectedOptionId))
            {
                UiFactory.CreateLabel(nodeCard, "ChooseHint", "选择一个院门样式，完成前院第一步焕新。", 28, FontStyle.Normal, TextAnchor.MiddleLeft, new Color(0.35f, 0.26f, 0.19f));
                foreach (var option in nodeDefinition.Options)
                {
                    var optionText = $"{option.DisplayName} · {option.Description}";
                    UiFactory.CreateButton(nodeCard, option.Id, optionText, () => SelectRenovationOption(nodeDefinition, option), new Color(0.42f, 0.58f, 0.32f, 1f), 94);
                }

                return;
            }

            var selectedOption = nodeDefinition.GetOption(selectedOptionId);
            if (selectedOption == null)
            {
                UiFactory.CreateLabel(nodeCard, "SelectionError", "装修选择状态异常，请重置进度。", 26, FontStyle.Normal, TextAnchor.MiddleLeft, new Color(0.55f, 0.22f, 0.18f));
                return;
            }

            UiFactory.CreateLabel(nodeCard, "SelectedLabel", $"当前方案：{selectedOption.DisplayName}", 30, FontStyle.Bold, TextAnchor.MiddleLeft, new Color(0.42f, 0.31f, 0.18f));
            UiFactory.CreateLabel(nodeCard, "MemoryTitle", $"回忆卡片：{nodeDefinition.MemoryTitle}", 28, FontStyle.Bold, TextAnchor.MiddleLeft, new Color(0.42f, 0.31f, 0.18f));
            UiFactory.CreateLabel(nodeCard, "MemoryText", nodeDefinition.MemoryText, 26, FontStyle.Normal, TextAnchor.UpperLeft, new Color(0.33f, 0.25f, 0.18f), 120);
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

            var panel = UiFactory.CreateAbsolutePanel(screenRoot, "LevelPanel", new Color(0.88f, 0.93f, 0.88f, 1f));
            StretchRect(panel, 0f, 0f, 0f, 0f);

            var headerCard = UiFactory.CreateAbsolutePanel(panel, "HeaderCard", new Color(0.90f, 0.94f, 0.89f, 1f));
            SetTopRect(headerCard, 24f, 24f, 24f, 190f);
            var headerLayout = headerCard.gameObject.AddComponent<VerticalLayoutGroup>();
            headerLayout.padding = new RectOffset(28, 28, 22, 18);
            headerLayout.spacing = 8;
            headerLayout.childAlignment = TextAnchor.UpperCenter;
            headerLayout.childControlHeight = false;
            headerLayout.childForceExpandHeight = false;
            headerLayout.childControlWidth = true;
            headerLayout.childForceExpandWidth = true;

            UiFactory.CreateLabel(headerCard, "LevelTitle", levelDefinition.DisplayName, 38, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.18f, 0.22f, 0.18f));
            UiFactory.CreateLabel(headerCard, "LevelGoal", levelDefinition.Description, 22, FontStyle.Normal, TextAnchor.MiddleCenter, new Color(0.29f, 0.34f, 0.29f));

            levelStatusText = UiFactory.CreateLabel(headerCard, "LevelStatus", string.Empty, 22, FontStyle.Normal, TextAnchor.MiddleCenter, new Color(0.22f, 0.26f, 0.22f));
            RefreshLevelStatus("点两个相同物件即可配对。先完成一次完整闭环。");

            var boardContainer = UiFactory.CreateAbsolutePanel(panel, "BoardContainer", new Color(0.95f, 0.97f, 0.92f, 1f));
            StretchBetween(boardContainer, 24f, 120f, 24f, 230f);

            var boardWidth = screenRoot.rect.width > 0f ? screenRoot.rect.width - 48f : 702f;
            var cellWidth = Mathf.Floor((boardWidth - 24f - 32f) / 3f);
            var cellHeight = 120f;
            var boardRoot = UiFactory.CreateGrid(boardContainer, "BoardGrid", 3, new Vector2(cellWidth, cellHeight), new Vector2(16f, 16f), new RectOffset(12, 12, 12, 12));
            StretchRect(boardRoot, 0f, 0f, 0f, 0f);
            for (var index = 0; index < activeBoard.Tiles.Count; index++)
            {
                var tile = activeBoard.Tiles[index];
                var tileView = UiFactory.CreateTileButton(boardRoot, $"Tile{index}", tile.DisplayLabel, () => OnTileClicked(index));
                tileViews.Add(tileView);
            }

            var toolbar = UiFactory.CreateHorizontalGroup(panel, "Toolbar", 14, TextAnchor.MiddleCenter);
            toolbar.anchorMin = new Vector2(0.5f, 0f);
            toolbar.anchorMax = new Vector2(0.5f, 0f);
            toolbar.pivot = new Vector2(0.5f, 0f);
            toolbar.anchoredPosition = new Vector2(0f, 24f);
            toolbar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, boardWidth);
            UiFactory.CreateButton(toolbar, "HintButton", "提示", ShowHint, new Color(0.81f, 0.55f, 0.08f, 1f), 78, 180);
            UiFactory.CreateButton(toolbar, "ShuffleButton", "洗牌", ShuffleBoard, new Color(0.12f, 0.34f, 0.55f, 1f), 78, 180);
            UiFactory.CreateButton(toolbar, "BackButton", "返回前院", ShowHome, new Color(0.39f, 0.41f, 0.43f, 1f), 78, 180);

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
                    ? new Color(0.82f, 0.84f, 0.82f, 1f)
                    : tile.IsSelected
                        ? new Color(0.94f, 0.79f, 0.46f, 1f)
                        : new Color(0.96f, 0.97f, 0.94f, 1f);
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
    }
}
