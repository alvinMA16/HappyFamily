import { SCREEN_WIDTH, SCREEN_HEIGHT } from '../render';
import { COLORS, FONTS, SIZES, getFont } from '../config/theme';
import Button from '../ui/button';
import SaveData from '../data/saveData';
import {
  getLevels,
  getFirstUnlockedPendingRenovationNode,
  getNextStoryLevel,
  getStoryChapterByLevelId,
  isChapterCompleted,
} from '../data/contentFactory';

function drawCenteredWrappedText(ctx, text, centerX, y, maxWidth, lineHeight, maxLines = 2) {
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
  visibleLines.forEach((line, index) => {
    ctx.fillText(line, centerX, y + index * lineHeight);
  });
}

/**
 * 结果场景
 * 显示关卡完成或失败的结果
 */
export default class ResultScene {
  isWin = false;
  levelDef = null;
  stars = 0;
  score = 0;
  gameMode = 'pairMatch';

  // UI元素
  retryButton = null;
  nextButton = null;
  homeButton = null;

  // 存档
  saveData = null;
  storyChapter = null;
  nextStoryLevel = null;
  pendingRenovationNode = null;
  chapterCompleted = false;

  constructor(params = {}) {
    this.isWin = params.isWin || false;
    this.levelDef = params.levelDef || { displayName: '关卡' };
    this.stars = params.stars || 0;
    this.score = params.score || 0;
    this.gameMode = GameGlobal.databus.gameMode || 'pairMatch';

    this.saveData = new SaveData();

    // 如果胜利，保存进度
    if (this.isWin && this.levelDef.id) {
      this.saveData.completeLevel(this.levelDef.id, this.stars);
    }

    if (this.levelDef.isStoryLevel) {
      this.storyChapter = getStoryChapterByLevelId(this.levelDef.id);
      this.nextStoryLevel = getNextStoryLevel(this.levelDef.id);
      this.pendingRenovationNode = getFirstUnlockedPendingRenovationNode(this.saveData);
      this.chapterCompleted = this.storyChapter ? isChapterCompleted(this.saveData, this.storyChapter.id) : false;
    }

    this.initUI();
  }

  /**
   * 初始化UI
   */
  initUI() {
    const centerX = SCREEN_WIDTH / 2;
    const buttonWidth = SIZES.buttonWidth;
    const buttonY = SCREEN_HEIGHT * 0.72;

    if (this.isWin) {
      // 胜利界面：下一关和返回首页
      this.nextButton = new Button({
        text: '下一关',
        x: centerX - buttonWidth / 2,
        y: buttonY,
        width: buttonWidth,
        height: SIZES.buttonHeight,
        bgColor: COLORS.success,
        onClick: () => this.onNext(),
      });

      this.homeButton = new Button({
        text: '返回首页',
        x: centerX - buttonWidth / 2,
        y: buttonY + SIZES.buttonHeight + SIZES.paddingMedium,
        width: buttonWidth,
        height: SIZES.buttonHeight,
        bgColor: COLORS.primary,
        onClick: () => this.onHome(),
      });
    } else {
      // 失败界面：重试和返回首页
      this.retryButton = new Button({
        text: '再试一次',
        x: centerX - buttonWidth / 2,
        y: buttonY,
        width: buttonWidth,
        height: SIZES.buttonHeight,
        bgColor: COLORS.accent,
        onClick: () => this.onRetry(),
      });

      this.homeButton = new Button({
        text: '返回首页',
        x: centerX - buttonWidth / 2,
        y: buttonY + SIZES.buttonHeight + SIZES.paddingMedium,
        width: buttonWidth,
        height: SIZES.buttonHeight,
        bgColor: COLORS.primary,
        onClick: () => this.onHome(),
      });
    }
  }

  /**
   * 进入场景
   */
  onEnter() {
    console.log('ResultScene: onEnter', this.isWin ? 'WIN' : 'LOSE');
  }

  /**
   * 离开场景
   */
  onExit() {
    console.log('ResultScene: onExit');
  }

  /**
   * 更新逻辑
   */
  update() {}

  /**
   * 渲染场景
   */
  render(ctx) {
    // 背景
    ctx.fillStyle = this.isWin ? COLORS.background : COLORS.backgroundDark;
    ctx.fillRect(0, 0, SCREEN_WIDTH, SCREEN_HEIGHT);

    // 结果标题
    const titleY = SCREEN_HEIGHT * 0.2;
    ctx.fillStyle = this.isWin ? COLORS.success : COLORS.accent;
    ctx.font = getFont(FONTS.sizeTitle, 'bold');
    ctx.textAlign = 'center';
    ctx.textBaseline = 'middle';
    ctx.fillText(this.isWin ? '恭喜过关！' : '再接再厉', SCREEN_WIDTH / 2, titleY);

    // 关卡名称
    ctx.fillStyle = COLORS.textPrimary;
    ctx.font = getFont(FONTS.sizeSubtitle);
    ctx.fillText(this.levelDef.displayName, SCREEN_WIDTH / 2, titleY + 60);

    if (this.isWin) {
      // 显示星星
      this.renderStars(ctx, SCREEN_WIDTH / 2, titleY + 140);

      // 显示剩余步数（作为分数）
      ctx.fillStyle = COLORS.textSecondary;
      ctx.font = getFont(FONTS.sizeNormal);
      ctx.fillText(`剩余步数: ${this.score}`, SCREEN_WIDTH / 2, titleY + 200);

      if (this.levelDef.isStoryLevel) {
        this.renderStoryWinInfo(ctx, titleY + 228);
      }
    } else {
      // 失败提示
      ctx.fillStyle = COLORS.textSecondary;
      ctx.font = getFont(FONTS.sizeNormal);
      ctx.fillText('步数用完了', SCREEN_WIDTH / 2, titleY + 120);
      ctx.fillText('换个策略再试试吧！', SCREEN_WIDTH / 2, titleY + 160);
    }

    // 渲染按钮
    if (this.nextButton) this.nextButton.render(ctx);
    if (this.retryButton) this.retryButton.render(ctx);
    if (this.homeButton) this.homeButton.render(ctx);
  }

  /**
   * 渲染星星
   */
  renderStars(ctx, centerX, y) {
    const starSize = 50;
    const starGap = 20;
    const totalWidth = 3 * starSize + 2 * starGap;
    const startX = centerX - totalWidth / 2;

    for (let i = 0; i < 3; i++) {
      const x = startX + i * (starSize + starGap) + starSize / 2;
      const filled = i < this.stars;

      this.drawStar(ctx, x, y, starSize / 2, filled);
    }
  }

  /**
   * 绘制五角星
   */
  drawStar(ctx, cx, cy, radius, filled) {
    const points = 5;
    const innerRadius = radius * 0.4;

    ctx.beginPath();
    for (let i = 0; i < points * 2; i++) {
      const r = i % 2 === 0 ? radius : innerRadius;
      const angle = (Math.PI / 2) + (i * Math.PI / points);
      const x = cx + Math.cos(angle) * r;
      const y = cy - Math.sin(angle) * r;
      if (i === 0) {
        ctx.moveTo(x, y);
      } else {
        ctx.lineTo(x, y);
      }
    }
    ctx.closePath();

    if (filled) {
      ctx.fillStyle = COLORS.warning;
      ctx.fill();
    }
    ctx.strokeStyle = COLORS.warning;
    ctx.lineWidth = 2;
    ctx.stroke();
  }

  renderStoryWinInfo(ctx, startY) {
    if (!this.storyChapter) {
      return;
    }

    ctx.fillStyle = COLORS.textPrimary;
    ctx.font = getFont(FONTS.sizeSmall, 'bold');
    ctx.fillText(this.storyChapter.displayName, SCREEN_WIDTH / 2, startY);

    ctx.fillStyle = COLORS.textSecondary;
    ctx.font = getFont(FONTS.sizeSmall);

    if (this.pendingRenovationNode && this.pendingRenovationNode.chapterId === this.storyChapter.id) {
      drawCenteredWrappedText(ctx, `焕新已解锁：${this.pendingRenovationNode.displayName}`, SCREEN_WIDTH / 2, startY + 34, SCREEN_WIDTH - 60, 28, 2);
      drawCenteredWrappedText(ctx, '回到首页可以立即挑一个方案落地。', SCREEN_WIDTH / 2, startY + 90, SCREEN_WIDTH - 60, 28, 2);
      return;
    }

    if (this.chapterCompleted) {
      drawCenteredWrappedText(ctx, this.storyChapter.closingBeat, SCREEN_WIDTH / 2, startY + 34, SCREEN_WIDTH - 60, 28, 2);
      return;
    }

    if (this.nextStoryLevel) {
      drawCenteredWrappedText(ctx, `下一步：${this.nextStoryLevel.displayName}`, SCREEN_WIDTH / 2, startY + 34, SCREEN_WIDTH - 60, 28, 2);
      drawCenteredWrappedText(ctx, this.storyChapter.familyGoal, SCREEN_WIDTH / 2, startY + 90, SCREEN_WIDTH - 60, 28, 2);
    }
  }

  /**
   * 处理触摸开始
   */
  onTouchStart(x, y) {
    if (this.nextButton) this.nextButton.onTouchStart(x, y);
    if (this.retryButton) this.retryButton.onTouchStart(x, y);
    if (this.homeButton) this.homeButton.onTouchStart(x, y);
  }

  /**
   * 处理触摸结束
   */
  onTouchEnd(x, y) {
    if (this.nextButton && this.nextButton.onTouchEnd(x, y)) return;
    if (this.retryButton && this.retryButton.onTouchEnd(x, y)) return;
    if (this.homeButton && this.homeButton.onTouchEnd(x, y)) return;
  }

  /**
   * 重试当前关卡
   */
  onRetry() {
    const mode = GameGlobal.databus.gameMode || 'pairMatch';
    GameGlobal.sceneManager.switchTo(mode, { levelDef: this.levelDef });
  }

  /**
   * 下一关
   */
  onNext() {
    if (this.levelDef.isStoryLevel) {
      const nextLevel = getNextStoryLevel(this.levelDef.id);
      if (nextLevel) {
        GameGlobal.sceneManager.switchTo(nextLevel.mode, { levelDef: nextLevel });
      } else {
        GameGlobal.sceneManager.switchTo('home');
      }
      return;
    }

    // 获取当前模式的关卡列表
    const levels = getLevels(this.gameMode);
    const currentIndex = levels.findIndex(l => l.id === this.levelDef.id);

    // 如果有下一关，进入下一关
    if (currentIndex >= 0 && currentIndex < levels.length - 1) {
      const nextLevel = levels[currentIndex + 1];
      GameGlobal.sceneManager.switchTo(this.gameMode, { levelDef: nextLevel });
    } else {
      // 没有下一关，返回首页
      GameGlobal.sceneManager.switchTo('home');
    }
  }

  /**
   * 返回首页
   */
  onHome() {
    GameGlobal.sceneManager.switchTo('home');
  }
}
