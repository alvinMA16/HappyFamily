/**
 * 收纳整理游戏逻辑
 * 移植自 Unity TidyUpBoard.cs
 */

import { getLevelItems } from '../data/contentFactory';

// 移动结果状态
export const TidyUpMoveState = {
  None: 'none',
  Picked: 'picked',
  Matched: 'matched',
  SlotsFull: 'slotsFull',
};

// 常量
const SLOT_COUNT = 7;
const MATCH_REQUIRED = 3;

/**
 * 收集槽
 */
class CollectionSlot {
  constructor() {
    this.itemId = -1;
    this.displayName = '';
    this.imageSrc = null;
  }

  get isEmpty() {
    return this.itemId === -1;
  }

  clear() {
    this.itemId = -1;
    this.displayName = '';
    this.imageSrc = null;
  }

  set(itemId, displayName, imageSrc) {
    this.itemId = itemId;
    this.displayName = displayName;
    this.imageSrc = imageSrc;
  }
}

/**
 * 收纳整理游戏板
 */
export default class TidyUpBoard {
  tiles = [];                    // 所有图块
  slots = [];                    // 收集槽（7个）
  gridWidth = 4;
  gridHeight = 4;
  maxLayers = 3;

  constructor() {
    // 初始化收集槽
    this.slots = [];
    for (let i = 0; i < SLOT_COUNT; i++) {
      this.slots.push(new CollectionSlot());
    }
  }

  /**
   * 工厂方法：创建游戏板
   * @param {Object} levelDefinition - 关卡定义
   * @returns {TidyUpBoard}
   */
  static create(levelDefinition) {
    const board = new TidyUpBoard();

    board.gridWidth = levelDefinition.gridWidth || 4;
    board.gridHeight = levelDefinition.gridHeight || 4;
    board.maxLayers = levelDefinition.maxLayers || 3;

    // 获取关卡物品列表
    const items = getLevelItems(levelDefinition);

    // 创建图块（每个物品出现3次，用于三消）
    const allTiles = [];
    items.forEach((item) => {
      for (let i = 0; i < MATCH_REQUIRED; i++) {
        allTiles.push({
          index: -1,  // 稍后分配
          itemId: item.id,
          displayName: item.label,
          imageSrc: item.image,
          layer: 0,
          gridX: 0,
          gridY: 0,
          isRemoved: false,
          blockedBy: [],
        });
      }
    });

    // 打乱顺序
    TidyUpBoard.shuffle(allTiles);

    // 分配索引和位置
    allTiles.forEach((tile, index) => {
      tile.index = index;
    });

    // 分配堆叠位置
    TidyUpBoard.assignStackedPositions(
      allTiles,
      board.gridWidth,
      board.gridHeight,
      board.maxLayers
    );

    // 计算阻挡关系
    TidyUpBoard.calculateBlocking(allTiles);

    board.tiles = allTiles;
    return board;
  }

  /**
   * Fisher-Yates 洗牌算法
   */
  static shuffle(array) {
    for (let i = array.length - 1; i > 0; i--) {
      const j = Math.floor(Math.random() * (i + 1));
      [array[i], array[j]] = [array[j], array[i]];
    }
    return array;
  }

  /**
   * 分配堆叠位置
   * @param {Array} tiles - 图块列表
   * @param {number} gridWidth - 网格宽度
   * @param {number} gridHeight - 网格高度
   * @param {number} maxLayers - 最大层数
   */
  static assignStackedPositions(tiles, gridWidth, gridHeight, maxLayers) {
    const positionsPerLayer = gridWidth * gridHeight;

    tiles.forEach((tile, index) => {
      tile.layer = Math.min(Math.floor(index / positionsPerLayer), maxLayers - 1);
      const posInLayer = index % positionsPerLayer;
      tile.gridX = posInLayer % gridWidth;
      tile.gridY = Math.floor(posInLayer / gridWidth);
    });
  }

  /**
   * 计算阻挡关系
   * 上层的图块会阻挡下层相邻位置的图块
   */
  static calculateBlocking(tiles) {
    for (const tile of tiles) {
      tile.blockedBy = [];

      // 找出所有在更高层的图块
      const higherTiles = tiles.filter(t =>
        t.layer > tile.layer && !t.isRemoved
      );

      // 检查是否有重叠或相邻的图块
      for (const higher of higherTiles) {
        const dx = Math.abs(higher.gridX - tile.gridX);
        const dy = Math.abs(higher.gridY - tile.gridY);
        const manhattan = dx + dy;

        // 如果在相邻范围内（包括对角线方向的部分重叠）
        if (dx <= 1 && dy <= 1 && manhattan <= 1) {
          tile.blockedBy.push(higher.index);
        }
      }
    }
  }

  /**
   * 检查图块是否可以被拾取
   * @param {number} tileIndex - 图块索引
   * @returns {boolean}
   */
  canPickTile(tileIndex) {
    if (tileIndex < 0 || tileIndex >= this.tiles.length) {
      return false;
    }

    const tile = this.tiles[tileIndex];

    // 已移除的图块不能拾取
    if (tile.isRemoved) {
      return false;
    }

    // 检查是否被其他图块阻挡
    for (const blockerIndex of tile.blockedBy) {
      if (!this.tiles[blockerIndex].isRemoved) {
        return false;
      }
    }

    return true;
  }

  /**
   * 尝试拾取图块
   * @param {number} tileIndex - 图块索引
   * @returns {Object} 移动结果 { state, itemName, matchCount }
   */
  tryPickTile(tileIndex) {
    if (!this.canPickTile(tileIndex)) {
      return { state: TidyUpMoveState.None, itemName: '', matchCount: 0 };
    }

    const tile = this.tiles[tileIndex];

    // 查找空槽位
    const emptySlotIndex = this.findEmptySlot();
    if (emptySlotIndex === -1) {
      return { state: TidyUpMoveState.SlotsFull, itemName: tile.displayName, matchCount: 0 };
    }

    // 移除图块
    tile.isRemoved = true;

    // 将物品放入槽位
    this.slots[emptySlotIndex].set(tile.itemId, tile.displayName, tile.imageSrc);

    // 检查是否达成三消
    const matchCount = this.countMatchingInSlots(tile.itemId);
    if (matchCount >= MATCH_REQUIRED) {
      // 移除匹配的物品
      this.removeMatchingFromSlots(tile.itemId);
      return { state: TidyUpMoveState.Matched, itemName: tile.displayName, matchCount };
    }

    // 检查是否槽位已满且无法消除
    if (this.isSlotsFull() && !this.hasPendingMatch()) {
      return { state: TidyUpMoveState.SlotsFull, itemName: tile.displayName, matchCount: 0 };
    }

    return { state: TidyUpMoveState.Picked, itemName: tile.displayName, matchCount };
  }

  /**
   * 查找空槽位
   * @returns {number} 空槽位索引，无空槽位返回 -1
   */
  findEmptySlot() {
    for (let i = 0; i < this.slots.length; i++) {
      if (this.slots[i].isEmpty) {
        return i;
      }
    }
    return -1;
  }

  /**
   * 检查槽位是否已满
   */
  isSlotsFull() {
    return this.slots.every(slot => !slot.isEmpty);
  }

  /**
   * 检查是否有待消除的匹配
   */
  hasPendingMatch() {
    const counts = {};
    for (const slot of this.slots) {
      if (!slot.isEmpty) {
        counts[slot.itemId] = (counts[slot.itemId] || 0) + 1;
        if (counts[slot.itemId] >= MATCH_REQUIRED) {
          return true;
        }
      }
    }
    return false;
  }

  /**
   * 统计槽位中指定物品的数量
   * @param {number} itemId
   */
  countMatchingInSlots(itemId) {
    return this.slots.filter(slot => slot.itemId === itemId).length;
  }

  /**
   * 从槽位中移除匹配的物品
   * @param {number} itemId
   */
  removeMatchingFromSlots(itemId) {
    let removed = 0;
    for (const slot of this.slots) {
      if (slot.itemId === itemId && removed < MATCH_REQUIRED) {
        slot.clear();
        removed++;
      }
    }
    // 压缩槽位
    this.compactSlots();
  }

  /**
   * 压缩槽位（移除空隙）
   */
  compactSlots() {
    const items = this.slots
      .filter(slot => !slot.isEmpty)
      .map(slot => ({ itemId: slot.itemId, displayName: slot.displayName, imageSrc: slot.imageSrc }));

    // 清空所有槽位
    for (const slot of this.slots) {
      slot.clear();
    }

    // 重新填充
    items.forEach((item, i) => {
      this.slots[i].set(item.itemId, item.displayName, item.imageSrc);
    });
  }

  /**
   * 计算剩余图块数
   */
  countRemainingTiles() {
    return this.tiles.filter(t => !t.isRemoved).length;
  }

  /**
   * 是否完成（所有图块已消除）
   */
  get isCompleted() {
    return this.countRemainingTiles() === 0;
  }

  /**
   * 是否失败（槽位已满且无法消除，且还有图块未消除）
   */
  get isFailed() {
    return this.isSlotsFull() && !this.hasPendingMatch() && !this.isCompleted;
  }

  /**
   * 获取所有可拾取的图块索引
   */
  getPickableTiles() {
    return this.tiles
      .filter((_, index) => this.canPickTile(index))
      .map(tile => tile.index);
  }
}
