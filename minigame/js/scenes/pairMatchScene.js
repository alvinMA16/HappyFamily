import { SCREEN_WIDTH, SCREEN_HEIGHT } from '../render';
import { COLORS, FONTS, SIZES, getFont } from '../config/theme';
import PairMatchBoard, { PairMatchMoveState } from '../gameplay/pairMatchBoard';
import Tile from '../ui/tile';
import Button from '../ui/button';

/**
 * 配对消除场景
 */
export default class PairMatchScene {
  board = null;           // 游戏逻辑板
  tiles = [];             // UI图块列表
  levelDef = null;        // 关卡定义
  gridCols = 4;           // 网格列数
  gridOffsetX = 0;        // 网格X偏移
  gridOffsetY = 0;        // 网格Y偏移

  // UI元素
  hintButton = null;
  shuffleButton = null;
  backButton = null;

  // 动画状态
  isAnimating = false;

  constructor(params = {}) {
    this.levelDef = params.levelDef || this.getDefaultLevel();
    this.initBoard();
    this.initUI();
  }

  /**
   * 获取默认关卡（用于测试）
   */
  getDefaultLevel() {
    return {
      id: 'test_level',
      displayName: '测试关卡',
      chapterDisplayName: '测试章节',
      familyGoal: '测试目标',
      stepBudget: 20,
      starReward: 1,
      itemIds: ['wooden_chair', 'wooden_table', 'bamboo_chair', 'flower_pot'],
    };
  }

  /**
   * 初始化游戏板
   */
  initBoard() {
    // 创建游戏逻辑
    this.board = PairMatchBoard.create(this.levelDef);

    // 计算网格布局
    const tileCount = this.board.tiles.length;
    this.gridCols = Math.ceil(Math.sqrt(tileCount));
    const gridRows = Math.ceil(tileCount / this.gridCols);

    // 计算图块尺寸（根据屏幕自适应）
    const availableWidth = SCREEN_WIDTH - 40;
    const availableHeight = SCREEN_HEIGHT - 310;
    const maxTileWidth = Math.floor(availableWidth / this.gridCols) - 8;
    const maxTileHeight = Math.floor(availableHeight / gridRows) - 8;
    const tileSize = Math.min(maxTileWidth, maxTileHeight, 85);
    const gap = 8;

    const gridWidth = this.gridCols * (tileSize + gap) - gap;
    const gridHeight = gridRows * (tileSize + gap) - gap;

    this.gridOffsetX = (SCREEN_WIDTH - gridWidth) / 2;
    this.gridOffsetY = 128;

    // 创建UI图块
    this.tiles = [];
    for (const tileData of this.board.tiles) {
      const col = tileData.index % this.gridCols;
      const row = Math.floor(tileData.index / this.gridCols);

      const tile = new Tile({
        index: tileData.index,
        label: tileData.displayLabel,
        imageSrc: tileData.imageSrc,
        x: this.gridOffsetX + col * (tileSize + gap),
        y: this.gridOffsetY + row * (tileSize + gap),
        width: tileSize,
        height: tileSize,
      });

      this.tiles.push(tile);
    }
  }

  /**
   * 初始化UI元素
   */
  initUI() {
    const buttonY = SCREEN_HEIGHT - 70;
    const buttonWidth = 90;
    const buttonHeight = 50;
    const gap = 15;

    // 计算按钮总宽度并居中
    const totalWidth = buttonWidth * 3 + gap * 2;
    const startX = (SCREEN_WIDTH - totalWidth) / 2;

    // 返回按钮
    this.backButton = new Button({
      text: '返回',
      x: startX,
      y: buttonY,
      width: buttonWidth,
      height: buttonHeight,
      bgColor: COLORS.textSecondary,
      onClick: () => this.onBack(),
    });

    // 提示按钮
    this.hintButton = new Button({
      text: '提示',
      x: startX + buttonWidth + gap,
      y: buttonY,
      width: buttonWidth,
      height: buttonHeight,
      bgColor: COLORS.success,
      onClick: () => this.onHint(),
    });

    // 洗牌按钮
    this.shuffleButton = new Button({
      text: '洗牌',
      x: startX + (buttonWidth + gap) * 2,
      y: buttonY,
      width: buttonWidth,
      height: buttonHeight,
      bgColor: COLORS.warning,
      onClick: () => this.onShuffle(),
    });
  }

  /**
   * 进入场景
   */
  onEnter() {
    console.log('PairMatchScene: onEnter');
    GameGlobal.databus.startLevel('pairMatch', this.levelDef.id);
  }

  /**
   * 离开场景
   */
  onExit() {
    console.log('PairMatchScene: onExit');
  }

  /**
   * 更新逻辑
   */
  update() {
    // 检查游戏结束
    if (!this.isAnimating && GameGlobal.databus.isPlaying()) {
      if (this.board.isCompleted) {
        this.onGameWin();
      } else if (this.board.isFailed) {
        this.onGameLose();
      }
    }
  }

  /**
   * 渲染场景
   */
  render(ctx) {
    // 绘制背景
    ctx.fillStyle = COLORS.background;
    ctx.fillRect(0, 0, SCREEN_WIDTH, SCREEN_HEIGHT);

    // 绘制顶部状态栏
    this.renderHeader(ctx);

    // 绘制图块
    for (const tile of this.tiles) {
      tile.render(ctx);
    }

    // 绘制按钮
    this.backButton.render(ctx);
    this.hintButton.render(ctx);
    this.shuffleButton.render(ctx);
  }

  /**
   * 渲染顶部状态栏
   */
  renderHeader(ctx) {
    // 背景
    ctx.fillStyle = COLORS.primaryLight;
    ctx.fillRect(0, 0, SCREEN_WIDTH, 108);

    ctx.textAlign = 'center';
    ctx.textBaseline = 'middle';

    if (this.levelDef.chapterDisplayName) {
      ctx.fillStyle = COLORS.textSecondary;
      ctx.font = getFont(16, 'bold');
      ctx.fillText(this.levelDef.chapterDisplayName, SCREEN_WIDTH / 2, 18);
    }

    ctx.fillStyle = COLORS.textPrimary;
    ctx.font = getFont(26, 'bold');
    ctx.fillText(this.levelDef.displayName, SCREEN_WIDTH / 2, 46);

    if (this.levelDef.familyGoal) {
      const shortGoal = this.levelDef.familyGoal.length > 18
        ? `${this.levelDef.familyGoal.slice(0, 18)}…`
        : this.levelDef.familyGoal;
      ctx.fillStyle = COLORS.textSecondary;
      ctx.font = getFont(16);
      ctx.fillText(shortGoal, SCREEN_WIDTH / 2, 72);
    }

    // 剩余步数和配对数
    ctx.font = getFont(22);
    ctx.textAlign = 'left';
    ctx.fillText(`步数: ${this.board.remainingSteps}`, 20, 96);

    ctx.textAlign = 'right';
    ctx.fillText(`配对: ${this.board.remainingPairs}`, SCREEN_WIDTH - 20, 96);
  }

  /**
   * 处理触摸开始
   */
  onTouchStart(x, y) {
    this.backButton.onTouchStart(x, y);
    this.hintButton.onTouchStart(x, y);
    this.shuffleButton.onTouchStart(x, y);
  }

  /**
   * 处理触摸结束
   */
  onTouchEnd(x, y) {
    // 检查按钮
    if (this.backButton.onTouchEnd(x, y)) return;
    if (this.hintButton.onTouchEnd(x, y)) return;
    if (this.shuffleButton.onTouchEnd(x, y)) return;

    // 检查图块点击
    if (this.isAnimating || !GameGlobal.databus.isPlaying()) return;

    for (const tile of this.tiles) {
      if (tile.containsPoint(x, y)) {
        this.onTileClick(tile.index);
        break;
      }
    }
  }

  /**
   * 处理图块点击
   */
  onTileClick(index) {
    const result = this.board.trySelect(index);
    const tile = this.tiles[index];

    switch (result.state) {
      case PairMatchMoveState.Selected:
        tile.setSelected(true);
        tile.playSelectAnimation();
        break;

      case PairMatchMoveState.Deselected:
        tile.setSelected(false);
        break;

      case PairMatchMoveState.Matched:
        this.isAnimating = true;
        // 找到两个匹配的图块
        const matchedTiles = this.board.tiles
          .filter(t => t.isRemoved && !this.tiles[t.index].isRemoved);

        let animCount = 0;
        for (const td of matchedTiles) {
          this.tiles[td.index].playMatchAnimation(() => {
            animCount++;
            if (animCount >= 2) {
              this.isAnimating = false;
            }
          });
        }
        break;

      case PairMatchMoveState.Mismatched:
        // 取消之前选中的图块的选中状态
        for (const t of this.tiles) {
          t.setSelected(false);
        }
        break;
    }

    // 同步UI状态
    this.syncTileStates();
  }

  /**
   * 同步图块UI状态与逻辑状态
   */
  syncTileStates() {
    for (let i = 0; i < this.board.tiles.length; i++) {
      const data = this.board.tiles[i];
      const tile = this.tiles[i];
      tile.setSelected(data.isSelected);
      tile.label = data.displayLabel;
      tile.setImage(data.imageSrc);
      if (data.isRemoved && !tile.isRemoved) {
        tile.setRemoved(true);
      }
    }
  }

  /**
   * 提示功能
   */
  onHint() {
    if (this.isAnimating || !GameGlobal.databus.isPlaying()) return;

    const hint = this.board.findHint();
    if (hint) {
      // 高亮提示的两个图块
      this.tiles[hint.leftIndex].setSelected(true);
      this.tiles[hint.rightIndex].setSelected(true);

      // 2秒后取消高亮
      setTimeout(() => {
        this.tiles[hint.leftIndex].setSelected(false);
        this.tiles[hint.rightIndex].setSelected(false);
        this.syncTileStates();
      }, 2000);
    }
  }

  /**
   * 洗牌功能
   */
  onShuffle() {
    if (this.isAnimating || !GameGlobal.databus.isPlaying()) return;

    this.board.shuffleRemaining();
    this.syncTileStates();
  }

  /**
   * 返回首页
   */
  onBack() {
    GameGlobal.sceneManager.switchTo('home');
  }

  /**
   * 游戏胜利
   */
  onGameWin() {
    GameGlobal.databus.setLevelResult(true, this.levelDef.starReward, this.board.remainingSteps);
    GameGlobal.sceneManager.switchTo('result', {
      isWin: true,
      levelDef: this.levelDef,
      stars: this.levelDef.starReward,
      score: this.board.remainingSteps,
    });
  }

  /**
   * 游戏失败
   */
  onGameLose() {
    GameGlobal.databus.setLevelResult(false, 0, 0);
    GameGlobal.sceneManager.switchTo('result', {
      isWin: false,
      levelDef: this.levelDef,
      stars: 0,
      score: 0,
    });
  }
}
