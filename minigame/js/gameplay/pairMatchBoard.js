/**
 * 配对消除游戏逻辑
 * 移植自 Unity PairMatchBoard.cs
 */

import { getLevelItems } from '../data/contentFactory';

// 移动结果状态
export const PairMatchMoveState = {
  None: 'none',
  Selected: 'selected',
  Deselected: 'deselected',
  Matched: 'matched',
  Mismatched: 'mismatched',
};

/**
 * 配对消除游戏板
 */
export default class PairMatchBoard {
  tiles = [];          // 所有图块
  remainingSteps = 0;  // 剩余步数
  selectedIndex = -1;  // 当前选中的图块索引

  constructor() {}

  /**
   * 工厂方法：创建游戏板
   * @param {Object} levelDefinition - 关卡定义
   * @returns {PairMatchBoard}
   */
  static create(levelDefinition) {
    const board = new PairMatchBoard();

    // 获取关卡物品列表
    const items = getLevelItems(levelDefinition);

    // 复制物品创建配对（每个物品出现两次）
    const tileData = [];
    for (const item of items) {
      // 每个物品添加两次形成配对
      tileData.push({ ...item });
      tileData.push({ ...item });
    }

    // 打乱顺序
    PairMatchBoard.shuffle(tileData);

    // 创建图块
    board.tiles = tileData.map((item, index) => ({
      index,
      pairId: item.id,
      displayLabel: item.label,
      imageSrc: item.image,
      isRemoved: false,
      isSelected: false,
    }));

    // 设置步数预算
    board.remainingSteps = levelDefinition.stepBudget;
    board.selectedIndex = -1;

    return board;
  }

  /**
   * Fisher-Yates 洗牌算法
   * @param {Array} array - 要打乱的数组
   */
  static shuffle(array) {
    for (let i = array.length - 1; i > 0; i--) {
      const j = Math.floor(Math.random() * (i + 1));
      [array[i], array[j]] = [array[j], array[i]];
    }
    return array;
  }

  /**
   * 尝试选择图块
   * @param {number} tileIndex - 图块索引
   * @returns {Object} 移动结果 { state, primaryLabel }
   */
  trySelect(tileIndex) {
    // 验证索引
    if (tileIndex < 0 || tileIndex >= this.tiles.length) {
      return { state: PairMatchMoveState.None, primaryLabel: '' };
    }

    const tile = this.tiles[tileIndex];

    // 已移除的图块不能选择
    if (tile.isRemoved) {
      return { state: PairMatchMoveState.None, primaryLabel: '' };
    }

    // 点击已选中的图块 - 取消选择
    if (this.selectedIndex === tileIndex) {
      tile.isSelected = false;
      this.selectedIndex = -1;
      return { state: PairMatchMoveState.Deselected, primaryLabel: tile.displayLabel };
    }

    // 没有选中图块 - 第一次选择
    if (this.selectedIndex === -1) {
      tile.isSelected = true;
      this.selectedIndex = tileIndex;
      return { state: PairMatchMoveState.Selected, primaryLabel: tile.displayLabel };
    }

    // 已有选中图块 - 尝试配对
    const selectedTile = this.tiles[this.selectedIndex];

    // 消耗步数
    this.remainingSteps--;

    // 检查是否匹配
    if (tile.pairId === selectedTile.pairId) {
      // 匹配成功
      tile.isRemoved = true;
      selectedTile.isRemoved = true;
      tile.isSelected = false;
      selectedTile.isSelected = false;
      this.selectedIndex = -1;
      return { state: PairMatchMoveState.Matched, primaryLabel: tile.displayLabel };
    } else {
      // 匹配失败
      selectedTile.isSelected = false;
      this.selectedIndex = -1;
      return { state: PairMatchMoveState.Mismatched, primaryLabel: tile.displayLabel };
    }
  }

  /**
   * 查找提示（找出一对可匹配的图块）
   * @returns {Object|null} { leftIndex, rightIndex } 或 null
   */
  findHint() {
    const activeTiles = this.tiles.filter(t => !t.isRemoved);

    for (let i = 0; i < activeTiles.length; i++) {
      for (let j = i + 1; j < activeTiles.length; j++) {
        if (activeTiles[i].pairId === activeTiles[j].pairId) {
          return {
            leftIndex: activeTiles[i].index,
            rightIndex: activeTiles[j].index,
          };
        }
      }
    }

    return null;
  }

  /**
   * 洗牌剩余图块
   */
  shuffleRemaining() {
    // 收集未移除图块的数据
    const activeIndices = [];
    const activeData = [];

    for (const tile of this.tiles) {
      if (!tile.isRemoved) {
        activeIndices.push(tile.index);
        activeData.push({
          displayLabel: tile.displayLabel,
          imageSrc: tile.imageSrc,
          pairId: tile.pairId,
        });
      }
    }

    // 打乱数据
    PairMatchBoard.shuffle(activeData);

    // 重新分配数据到图块
    for (let i = 0; i < activeIndices.length; i++) {
      const tile = this.tiles[activeIndices[i]];
      tile.displayLabel = activeData[i].displayLabel;
      tile.imageSrc = activeData[i].imageSrc;
      tile.pairId = activeData[i].pairId;
    }

    // 清除选中状态
    if (this.selectedIndex !== -1) {
      this.tiles[this.selectedIndex].isSelected = false;
      this.selectedIndex = -1;
    }
  }

  /**
   * 计算剩余图块数
   */
  countRemainingTiles() {
    return this.tiles.filter(t => !t.isRemoved).length;
  }

  /**
   * 剩余配对数
   */
  get remainingPairs() {
    return Math.floor(this.countRemainingTiles() / 2);
  }

  /**
   * 是否完成（所有图块已消除）
   */
  get isCompleted() {
    return this.countRemainingTiles() === 0;
  }

  /**
   * 是否失败（步数用完且未完成）
   */
  get isFailed() {
    return this.remainingSteps <= 0 && !this.isCompleted;
  }
}
