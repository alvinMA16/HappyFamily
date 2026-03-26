import { COLORS, FONTS, SIZES, getFont } from '../config/theme';
import { drawRoundRect } from '../utils/canvasHelper';

// 图片缓存
const imageCache = {};

function loadImage(src) {
  if (!imageCache[src]) {
    const img = wx.createImage();
    img.src = src;
    imageCache[src] = img;
  }
  return imageCache[src];
}

/**
 * 收集槽组件
 * 显示7个收集槽，用于收纳整理玩法
 */
export default class SlotBar {
  slots = [];         // 槽位数据引用
  x = 0;
  y = 0;
  width = 0;
  height = 0;

  // 每个槽位的位置
  slotPositions = [];

  constructor(options = {}) {
    this.x = options.x || 0;
    this.y = options.y || 0;
    this.slots = options.slots || [];

    // 计算尺寸
    const slotCount = this.slots.length || 7;
    const slotSize = options.slotSize || 50;
    const gap = 6;

    this.slotSize = slotSize;
    this.width = slotCount * slotSize + (slotCount - 1) * gap + 20;
    this.height = slotSize + 20;

    // 计算每个槽位的位置
    const startX = this.x + 10;
    const slotY = this.y + 10;

    this.slotPositions = [];
    for (let i = 0; i < slotCount; i++) {
      this.slotPositions.push({
        x: startX + i * (slotSize + gap),
        y: slotY,
        width: slotSize,
        height: slotSize,
      });
    }
  }

  /**
   * 更新槽位数据引用
   */
  setSlots(slots) {
    this.slots = slots;
  }

  /**
   * 渲染收集槽
   */
  render(ctx) {
    // 绘制背景
    drawRoundRect(ctx, this.x, this.y, this.width, this.height, 12);
    ctx.fillStyle = COLORS.primaryLight;
    ctx.fill();

    // 绘制每个槽位
    const radius = 6;

    for (let i = 0; i < this.slotPositions.length; i++) {
      const pos = this.slotPositions[i];
      const slot = this.slots[i];
      const isEmpty = !slot || slot.isEmpty;

      // 槽位背景
      drawRoundRect(ctx, pos.x, pos.y, pos.width, pos.height, radius);
      ctx.fillStyle = isEmpty ? COLORS.slotEmpty : COLORS.slotFilled;
      ctx.fill();
      ctx.strokeStyle = COLORS.slotBorder;
      ctx.lineWidth = 2;
      ctx.stroke();

      // 如果有物品，显示图片或名称
      if (!isEmpty) {
        if (slot.imageSrc) {
          // 显示图片
          const img = loadImage(slot.imageSrc);
          if (img && img.complete) {
            const padding = 4;
            const imgSize = pos.width - padding * 2;
            ctx.drawImage(img, pos.x + padding, pos.y + padding, imgSize, imgSize);
          }
        } else if (slot.displayName) {
          // 显示文字
          ctx.fillStyle = COLORS.textPrimary;
          ctx.font = getFont(16, 'bold');
          ctx.textAlign = 'center';
          ctx.textBaseline = 'middle';

          // 截取前两个字符显示
          const text = slot.displayName.substring(0, 2);
          ctx.fillText(text, pos.x + pos.width / 2, pos.y + pos.height / 2);
        }
      }
    }
  }

  /**
   * 播放物品进入动画
   * @param {number} slotIndex - 槽位索引
   */
  playEnterAnimation(slotIndex) {
    // TODO: 实现进入动画
  }

  /**
   * 播放三消动画
   * @param {number} itemId - 物品ID
   * @param {Function} callback - 完成回调
   */
  playMatchAnimation(itemId, callback) {
    // TODO: 实现三消动画
    if (callback) {
      setTimeout(callback, 300);
    }
  }
}
