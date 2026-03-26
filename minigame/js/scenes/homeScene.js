import { SCREEN_WIDTH, SCREEN_HEIGHT } from '../render';
import { COLORS, getFont } from '../config/theme';
import Button from '../ui/button';
import SaveData from '../data/saveData';
import {
  getCampaign,
  getCurrentChapterForSave,
  getFirstUnlockedPendingRenovationNode,
  getLatestSelectedRenovation,
  getNextStoryLevelForSave,
  getStoryProgress,
} from '../data/contentFactory';
import { fillRoundRect, strokeRoundRect } from '../utils/canvasHelper';

function drawWrappedText(ctx, text, x, y, maxWidth, lineHeight, maxLines = 3) {
  const chars = Array.from(text || '');
  const lines = [];
  let current = '';

  for (const char of chars) {
    const testLine = current + char;
    if (ctx.measureText(testLine).width > maxWidth && current) {
      lines.push(current);
      current = char;
      if (lines.length >= maxLines - 1) {
        break;
      }
    } else {
      current = testLine;
    }
  }

  if (current) {
    lines.push(current);
  }

  const visibleLines = lines.slice(0, maxLines);
  if (lines.length > maxLines && visibleLines.length > 0) {
    const lastIndex = visibleLines.length - 1;
    visibleLines[lastIndex] = `${visibleLines[lastIndex].slice(0, Math.max(0, visibleLines[lastIndex].length - 1))}…`;
  }

  visibleLines.forEach((line, index) => {
    ctx.fillText(line, x, y + index * lineHeight);
  });
}

function drawCard(ctx, x, y, width, height) {
  fillRoundRect(ctx, x, y, width, height, 20, '#FFFDF8');
  strokeRoundRect(ctx, x, y, width, height, 20, COLORS.primaryLight, 2);
}

/**
 * 主线首页
 * 显示当前章节、剧情目标、焕新节点和继续主线入口
 */
export default class HomeScene {
  saveData = null;
  campaign = getCampaign();

  storyButton = null;
  resetButton = null;
  renovationOptionButtons = [];

  nextStoryLevel = null;
  currentChapter = null;
  pendingRenovationNode = null;
  latestRenovation = null;
  storyProgress = null;
  layout = {
    goalY: 118,
    chapterY: 228,
    renovationY: 386,
    renovationHeight: 110,
  };

  constructor() {
    this.saveData = new SaveData();
    this.initUI();
    this.refreshState();
  }

  initUI() {
    const cardWidth = SCREEN_WIDTH - 40;
    const buttonWidth = cardWidth;
    const buttonHeight = 68;

    this.storyButton = new Button({
      text: '继续主线',
      x: 20,
      y: SCREEN_HEIGHT - 170,
      width: buttonWidth,
      height: buttonHeight,
      bgColor: COLORS.accent,
      onClick: () => this.startNextStoryLevel(),
    });

    this.resetButton = new Button({
      text: '重置进度',
      x: (SCREEN_WIDTH - 180) / 2,
      y: SCREEN_HEIGHT - 60,
      width: 180,
      height: 44,
      bgColor: COLORS.textSecondary,
      onClick: () => this.resetProgress(),
    });
  }

  refreshState() {
    this.storyProgress = getStoryProgress(this.saveData);
    this.nextStoryLevel = getNextStoryLevelForSave(this.saveData);
    this.currentChapter = getCurrentChapterForSave(this.saveData);
    this.pendingRenovationNode = getFirstUnlockedPendingRenovationNode(this.saveData);
    this.latestRenovation = getLatestSelectedRenovation(this.saveData);

    this.storyButton.text = this.nextStoryLevel ? (this.storyProgress.completedLevels === 0 ? '开始主线' : '继续主线') : '团圆已完成';
    this.storyButton.isDisabled = !this.nextStoryLevel;

    const renovationHeight = this.pendingRenovationNode ? 136 : 104;
    const chapterHeight = 146;
    const goalHeight = 92;

    this.layout = {
      renovationHeight,
      goalY: 118,
      chapterY: 118 + goalHeight + 18,
      renovationY: 118 + goalHeight + 18 + chapterHeight + 12,
    };

    this.storyButton.y = Math.max(this.layout.renovationY + renovationHeight + 18, SCREEN_HEIGHT - 128);
    this.resetButton.y = SCREEN_HEIGHT - 60;

    this.buildRenovationButtons();
  }

  buildRenovationButtons() {
    this.renovationOptionButtons = [];

    if (!this.pendingRenovationNode) {
      return;
    }

    const buttonWidth = (SCREEN_WIDTH - 52) / 2;
    const optionY = this.layout.renovationY + 74;

    this.pendingRenovationNode.options.forEach((option, index) => {
      const button = new Button({
        text: option.displayName,
        x: 20 + index * (buttonWidth + 12),
        y: optionY,
        width: buttonWidth,
        height: 52,
        bgColor: index === 0 ? COLORS.primary : COLORS.primaryDark,
        onClick: () => this.selectRenovationOption(this.pendingRenovationNode, option),
      });

      this.renovationOptionButtons.push(button);
    });
  }

  startNextStoryLevel() {
    if (!this.nextStoryLevel) {
      return;
    }

    GameGlobal.sceneManager.switchTo(this.nextStoryLevel.mode, { levelDef: this.nextStoryLevel });
  }

  selectRenovationOption(node, option) {
    this.saveData.setRenovationSelection(node.id, option.id);
    this.refreshState();
  }

  resetProgress() {
    this.saveData.reset();
    this.refreshState();
  }

  onEnter() {
    this.saveData.load();
    this.refreshState();
  }

  onExit() {}

  update() {}

  render(ctx) {
    ctx.fillStyle = COLORS.background;
    ctx.fillRect(0, 0, SCREEN_WIDTH, SCREEN_HEIGHT);

    this.renderHeader(ctx);
    this.renderGoalCard(ctx);
    this.renderChapterCard(ctx);
    this.renderRenovationCard(ctx);

    this.storyButton.render(ctx);
    this.resetButton.render(ctx);
    this.renovationOptionButtons.forEach(button => button.render(ctx));
  }

  renderHeader(ctx) {
    ctx.fillStyle = COLORS.textPrimary;
    ctx.font = getFont(42, 'bold');
    ctx.textAlign = 'center';
    ctx.textBaseline = 'middle';
    ctx.fillText(this.campaign.displayName, SCREEN_WIDTH / 2, 50);

    ctx.fillStyle = COLORS.textSecondary;
    ctx.font = getFont(18);
    const shortSummary = this.campaign.summary.length > 20
      ? `${this.campaign.summary.slice(0, 20)}…`
      : this.campaign.summary;
    ctx.fillText(shortSummary, SCREEN_WIDTH / 2, 84);
    ctx.fillText(`幸福星 ${this.saveData.getTotalStars()}`, SCREEN_WIDTH / 2, 106);
  }

  renderGoalCard(ctx) {
    const x = 20;
    const y = this.layout.goalY;
    const width = SCREEN_WIDTH - 40;
    const height = 92;
    drawCard(ctx, x, y, width, height);

    ctx.fillStyle = COLORS.textSecondary;
    ctx.font = getFont(18, 'bold');
    ctx.textAlign = 'left';
    ctx.fillText('主线目标', x + 16, y + 28);

    ctx.fillStyle = COLORS.textPrimary;
    ctx.font = getFont(22, 'bold');
    drawWrappedText(ctx, this.campaign.longTermGoal, x + 16, y + 54, width - 32, 24, 2);

    ctx.fillStyle = COLORS.textSecondary;
    ctx.font = getFont(18);
    ctx.fillText(`章节进度 ${this.storyProgress.completedChapters}/${this.storyProgress.totalChapters}`, x + 16, y + 84);
    ctx.fillText(`关卡进度 ${this.storyProgress.completedLevels}/${this.storyProgress.totalLevels}`, x + width - 150, y + 84);
  }

  renderChapterCard(ctx) {
    const x = 20;
    const y = this.layout.chapterY;
    const width = SCREEN_WIDTH - 40;
    const height = 146;
    drawCard(ctx, x, y, width, height);

    if (!this.currentChapter) {
      ctx.fillStyle = COLORS.textPrimary;
      ctx.font = getFont(24, 'bold');
      ctx.textAlign = 'center';
      ctx.fillText('老屋已经准备好了', SCREEN_WIDTH / 2, y + 62);
      ctx.font = getFont(18);
      ctx.fillStyle = COLORS.textSecondary;
      ctx.fillText('现在就等家里人推门回来团圆。', SCREEN_WIDTH / 2, y + 96);
      return;
    }

    ctx.textAlign = 'left';
    ctx.fillStyle = COLORS.textPrimary;
    ctx.font = getFont(26, 'bold');
    ctx.fillText(this.currentChapter.displayName, x + 16, y + 28);

    ctx.fillStyle = COLORS.textSecondary;
    ctx.font = getFont(18, 'bold');
    ctx.fillText(`空间：${this.currentChapter.spaceTheme}`, x + 16, y + 56);

    ctx.fillStyle = COLORS.textPrimary;
    ctx.font = getFont(18);
    drawWrappedText(ctx, this.currentChapter.chapterHook, x + 16, y + 80, width - 32, 20, 3);

    ctx.fillStyle = COLORS.textSecondary;
    ctx.font = getFont(18, 'bold');
    drawWrappedText(ctx, `本章牵引：${this.currentChapter.familyGoal}`, x + 16, y + 132, width - 32, 20, 1);
  }

  renderRenovationCard(ctx) {
    const x = 20;
    const y = this.layout.renovationY;
    const width = SCREEN_WIDTH - 40;
    const height = this.layout.renovationHeight;
    drawCard(ctx, x, y, width, height);

    ctx.textAlign = 'left';

    if (this.pendingRenovationNode) {
      ctx.fillStyle = COLORS.textPrimary;
      ctx.font = getFont(24, 'bold');
      ctx.fillText(this.pendingRenovationNode.displayName, x + 16, y + 28);

      ctx.fillStyle = COLORS.textSecondary;
      ctx.font = getFont(18);
      drawWrappedText(ctx, this.pendingRenovationNode.storyBeatText, x + 16, y + 58, width - 32, 24, 2);
      ctx.fillText(`回忆：${this.pendingRenovationNode.memoryTitle} · ${this.pendingRenovationNode.memoryItem}`, x + 16, y + 108);
      return;
    }

    if (this.latestRenovation) {
      ctx.fillStyle = COLORS.textPrimary;
      ctx.font = getFont(24, 'bold');
      ctx.fillText(`${this.latestRenovation.node.displayName}`, x + 16, y + 28);

      ctx.fillStyle = COLORS.textSecondary;
      ctx.font = getFont(18);
      ctx.fillText(`当前方案：${this.latestRenovation.option.displayName}`, x + 16, y + 62);
      drawWrappedText(ctx, this.latestRenovation.node.memoryText, x + 16, y + 92, width - 32, 22, 2);
      return;
    }

    ctx.fillStyle = COLORS.textPrimary;
    ctx.font = getFont(22, 'bold');
    ctx.fillText('焕新节点还没解锁', x + 16, y + 34);
    ctx.fillStyle = COLORS.textSecondary;
    ctx.font = getFont(18);
    drawWrappedText(ctx, '继续推进主线、拿到更多幸福星，老屋就会一处处亮堂起来。', x + 16, y + 70, width - 32, 24, 2);
  }

  onTouchStart(x, y) {
    this.storyButton.onTouchStart(x, y);
    this.resetButton.onTouchStart(x, y);
    this.renovationOptionButtons.forEach(button => button.onTouchStart(x, y));
  }

  onTouchEnd(x, y) {
    if (this.storyButton.onTouchEnd(x, y)) return;
    if (this.resetButton.onTouchEnd(x, y)) return;

    for (const button of this.renovationOptionButtons) {
      if (button.onTouchEnd(x, y)) return;
    }
  }
}
