import { SCREEN_WIDTH, SCREEN_HEIGHT } from '../render';
import { COLORS, FONTS, SIZES, getFont } from '../config/theme';
import TidyUpBoard, { TidyUpMoveState } from '../gameplay/tidyUpBoard';
import Tile from '../ui/tile';
import SlotBar from '../ui/slotBar';
import Button from '../ui/button';

/**
 * 收纳整理场景
 */
export default class TidyUpScene {
  board = null;           // 游戏逻辑板
  tiles = [];             // UI图块列表（按层排序）
  levelDef = null;        // 关卡定义
  slotBar = null;         // 收集槽UI

  // 布局参数
  gridOffsetX = 0;
  gridOffsetY = 0;
  tileWidth = 70;
  tileHeight = 70;
  layerOffsetX = 6;       // 层之间的X偏移（创造立体感）
  layerOffsetY = 6;       // 层之间的Y偏移

  // UI元素
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
      id: 'test_tidyup',
      displayName: '整理测试',
      chapterDisplayName: '测试章节',
      familyGoal: '测试目标',
      starReward: 1,
      itemIds: ['wooden_chair', 'wooden_table', 'bamboo_chair', 'flower_pot'],
      gridWidth: 4,
      gridHeight: 3,
      maxLayers: 2,
    };
  }

  /**
   * 初始化游戏板
   */
  initBoard() {
    // 创建游戏逻辑
    this.board = TidyUpBoard.create(this.levelDef);

    // 计算布局
    const gridWidth = this.board.gridWidth;
    const gridHeight = this.board.gridHeight;
    const maxLayers = this.board.maxLayers;

    // 计算可用空间（考虑顶部状态栏和底部收集槽）
    const availableWidth = SCREEN_WIDTH - 40 - (maxLayers - 1) * this.layerOffsetX;
    const availableHeight = SCREEN_HEIGHT - 300;

    // 计算单个图块尺寸
    const maxTileWidth = Math.floor(availableWidth / gridWidth) - 6;
    const maxTileHeight = Math.floor(availableHeight / gridHeight) - 6;
    this.tileWidth = Math.min(maxTileWidth, maxTileHeight, 70);
    this.tileHeight = this.tileWidth;

    // 计算网格起始位置
    const totalWidth = gridWidth * (this.tileWidth + 6) - 6 + (maxLayers - 1) * this.layerOffsetX;
    this.gridOffsetX = (SCREEN_WIDTH - totalWidth) / 2;
    this.gridOffsetY = 116;

    // 创建UI图块（按层从下到上排序，确保渲染顺序正确）
    this.tiles = [];
    const sortedTiles = [...this.board.tiles].sort((a, b) => a.layer - b.layer);

    for (const tileData of sortedTiles) {
      const tile = new Tile({
        index: tileData.index,
        label: tileData.displayName,
        imageSrc: tileData.imageSrc,
        width: this.tileWidth,
        height: this.tileHeight,
      });

      // 计算位置（考虑层偏移）
      const x = this.gridOffsetX + tileData.gridX * (this.tileWidth + 6) + tileData.layer * this.layerOffsetX;
      const y = this.gridOffsetY + tileData.gridY * (this.tileHeight + 6) - tileData.layer * this.layerOffsetY;
      tile.setPosition(x, y);

      // 存储层信息
      tile.layer = tileData.layer;
      tile.tileData = tileData;

      this.tiles.push(tile);
    }

    // 更新阻挡状态
    this.updateBlockedStates();
  }

  /**
   * 初始化UI元素
   */
  initUI() {
    // 计算收集槽尺寸
    const slotSize = Math.min(45, (SCREEN_WIDTH - 60) / 7 - 6);
    const slotBarWidth = 7 * slotSize + 6 * 6 + 20;

    this.slotBar = new SlotBar({
      x: (SCREEN_WIDTH - slotBarWidth) / 2,
      y: SCREEN_HEIGHT - slotSize - 90,
      slots: this.board.slots,
      slotSize: slotSize,
    });

    // 返回按钮
    this.backButton = new Button({
      text: '返回',
      x: (SCREEN_WIDTH - 100) / 2,
      y: SCREEN_HEIGHT - 60,
      width: 100,
      height: 45,
      bgColor: COLORS.textSecondary,
      onClick: () => this.onBack(),
    });
  }

  /**
   * 更新图块的阻挡状态
   */
  updateBlockedStates() {
    for (const tile of this.tiles) {
      const canPick = this.board.canPickTile(tile.index);
      tile.setBlocked(!canPick);
    }
  }

  /**
   * 进入场景
   */
  onEnter() {
    console.log('TidyUpScene: onEnter');
    GameGlobal.databus.startLevel('tidyUp', this.levelDef.id);
  }

  /**
   * 离开场景
   */
  onExit() {
    console.log('TidyUpScene: onExit');
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

    // 绘制图块（从下层到上层）
    for (const tile of this.tiles) {
      tile.render(ctx);
    }

    // 绘制收集槽
    this.slotBar.render(ctx);

    // 绘制按钮
    this.backButton.render(ctx);
  }

  /**
   * 渲染顶部状态栏
   */
  renderHeader(ctx) {
    // 背景
    ctx.fillStyle = COLORS.primaryLight;
    ctx.fillRect(0, 0, SCREEN_WIDTH, 98);

    ctx.textAlign = 'center';
    ctx.textBaseline = 'middle';

    if (this.levelDef.chapterDisplayName) {
      ctx.fillStyle = COLORS.textSecondary;
      ctx.font = getFont(16, 'bold');
      ctx.fillText(this.levelDef.chapterDisplayName, SCREEN_WIDTH / 2, 18);
    }

    ctx.fillStyle = COLORS.textPrimary;
    ctx.font = getFont(24, 'bold');
    ctx.fillText(this.levelDef.displayName, SCREEN_WIDTH / 2, 42);

    if (this.levelDef.familyGoal) {
      const shortGoal = this.levelDef.familyGoal.length > 18
        ? `${this.levelDef.familyGoal.slice(0, 18)}…`
        : this.levelDef.familyGoal;
      ctx.fillStyle = COLORS.textSecondary;
      ctx.font = getFont(16);
      ctx.fillText(shortGoal, SCREEN_WIDTH / 2, 64);
    }

    // 剩余物品数
    const remaining = this.board.countRemainingTiles();
    ctx.font = getFont(20);
    ctx.fillText(`剩余: ${remaining}`, SCREEN_WIDTH / 2, 86);
  }

  /**
   * 处理触摸开始
   */
  onTouchStart(x, y) {
    this.backButton.onTouchStart(x, y);
  }

  /**
   * 处理触摸结束
   */
  onTouchEnd(x, y) {
    // 检查按钮
    if (this.backButton.onTouchEnd(x, y)) return;

    // 检查图块点击（从上层到下层检测）
    if (this.isAnimating || !GameGlobal.databus.isPlaying()) return;

    // 反向遍历以检测上层图块
    for (let i = this.tiles.length - 1; i >= 0; i--) {
      const tile = this.tiles[i];
      if (tile.containsPoint(x, y) && !tile.isBlocked && !tile.isRemoved) {
        this.onTileClick(tile.index);
        break;
      }
    }
  }

  /**
   * 处理图块点击
   */
  onTileClick(index) {
    const result = this.board.tryPickTile(index);

    switch (result.state) {
      case TidyUpMoveState.Picked:
        // 播放拾取动画
        this.playPickAnimation(index);
        break;

      case TidyUpMoveState.Matched:
        // 播放拾取 + 三消动画
        this.playPickAnimation(index, () => {
          this.slotBar.playMatchAnimation(this.board.tiles[index].itemId);
        });
        break;

      case TidyUpMoveState.SlotsFull:
        // 槽位已满，游戏失败
        console.log('Slots full!');
        break;

      case TidyUpMoveState.None:
        // 无法拾取（被阻挡）
        console.log('Cannot pick - blocked');
        break;
    }

    // 更新UI状态
    this.updateBlockedStates();
    this.slotBar.setSlots(this.board.slots);
  }

  /**
   * 播放拾取动画
   */
  playPickAnimation(index, callback) {
    const tile = this.tiles.find(t => t.index === index);
    if (tile) {
      this.isAnimating = true;
      tile.playMatchAnimation(() => {
        this.isAnimating = false;
        if (callback) callback();
      });
    }
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
    GameGlobal.databus.setLevelResult(true, this.levelDef.starReward, 0);
    GameGlobal.sceneManager.switchTo('result', {
      isWin: true,
      levelDef: this.levelDef,
      stars: this.levelDef.starReward,
      score: 0,
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
