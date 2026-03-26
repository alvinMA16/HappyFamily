import { COLORS, FONTS, SIZES, getFont } from '../config/theme';
import { drawRoundRect } from '../utils/canvasHelper';

// 图片缓存
const imageCache = {};

/**
 * 加载图片
 * @param {string} src - 图片路径
 * @returns {Image}
 */
function loadImage(src) {
  if (!imageCache[src]) {
    const img = wx.createImage();
    img.src = src;
    imageCache[src] = img;
  }
  return imageCache[src];
}

/**
 * 图块组件
 * 用于配对消除和收纳整理的图块显示
 * 支持文字和图片两种显示模式
 */
export default class Tile {
  constructor(options = {}) {
    // 数据
    this.index = options.index || 0;
    this.label = options.label || '';
    this.imageSrc = options.imageSrc || null;  // 图片路径
    this.image = null;  // 图片对象

    // 位置和尺寸
    this.x = options.x || 0;
    this.y = options.y || 0;
    this.width = options.width || SIZES.tileSize;
    this.height = options.height || SIZES.tileSize;

    // 状态
    this.isSelected = false;
    this.isRemoved = false;
    this.isBlocked = options.isBlocked || false;
    this.isMatching = false;  // 匹配动画状态

    // 动画
    this.scale = 1;
    this.alpha = 1;

    // 加载图片
    if (this.imageSrc) {
      this.image = loadImage(this.imageSrc);
    }
  }

  /**
   * 设置位置
   */
  setPosition(x, y) {
    this.x = x;
    this.y = y;
  }

  /**
   * 设置图片
   */
  setImage(src) {
    this.imageSrc = src;
    if (src) {
      this.image = loadImage(src);
    } else {
      this.image = null;
    }
  }

  /**
   * 设置选中状态
   */
  setSelected(selected) {
    this.isSelected = selected;
  }

  /**
   * 设置移除状态
   */
  setRemoved(removed) {
    this.isRemoved = removed;
    if (removed) {
      this.alpha = 0;
    }
  }

  /**
   * 设置阻挡状态
   */
  setBlocked(blocked) {
    this.isBlocked = blocked;
  }

  /**
   * 检测点是否在图块区域内
   */
  containsPoint(px, py) {
    if (this.isRemoved) return false;
    return (
      px >= this.x &&
      px <= this.x + this.width &&
      py >= this.y &&
      py <= this.y + this.height
    );
  }

  /**
   * 获取图块的显示颜色
   */
  getBackgroundColor() {
    if (this.isBlocked) {
      return COLORS.tileBlocked;
    }
    if (this.isMatching) {
      return COLORS.tileMatched;
    }
    if (this.isSelected) {
      return COLORS.tileSelected;
    }
    return COLORS.tileBackground;
  }

  /**
   * 渲染图块
   */
  render(ctx) {
    if (this.isRemoved) return;

    const radius = SIZES.tileRadius;

    ctx.save();

    // 应用透明度
    ctx.globalAlpha = this.alpha;

    // 应用缩放（以中心点为原点）
    if (this.scale !== 1) {
      const cx = this.x + this.width / 2;
      const cy = this.y + this.height / 2;
      ctx.translate(cx, cy);
      ctx.scale(this.scale, this.scale);
      ctx.translate(-cx, -cy);
    }

    // 绘制阴影（仅非阻挡状态）
    if (!this.isBlocked) {
      ctx.shadowColor = 'rgba(0, 0, 0, 0.15)';
      ctx.shadowBlur = 8;
      ctx.shadowOffsetX = 2;
      ctx.shadowOffsetY = 4;
    }

    // 绘制背景
    drawRoundRect(ctx, this.x, this.y, this.width, this.height, radius);
    ctx.fillStyle = this.getBackgroundColor();
    ctx.fill();

    // 重置阴影
    ctx.shadowColor = 'transparent';
    ctx.shadowBlur = 0;
    ctx.shadowOffsetX = 0;
    ctx.shadowOffsetY = 0;

    // 绘制边框
    ctx.strokeStyle = this.isSelected ? COLORS.accent : COLORS.tileBorder;
    ctx.lineWidth = this.isSelected ? 4 : 2;
    ctx.stroke();

    // 绘制内容（图片或文字）
    if (this.image && this.image.complete) {
      // 绘制图片（留出边距）
      const padding = 8;
      const imgSize = Math.min(this.width, this.height) - padding * 2;
      const imgX = this.x + (this.width - imgSize) / 2;
      const imgY = this.y + (this.height - imgSize) / 2;

      // 如果被阻挡，降低图片透明度
      if (this.isBlocked) {
        ctx.globalAlpha = this.alpha * 0.5;
      }

      ctx.drawImage(this.image, imgX, imgY, imgSize, imgSize);

      // 恢复透明度
      if (this.isBlocked) {
        ctx.globalAlpha = this.alpha;
      }
    } else {
      // 绘制文字
      ctx.fillStyle = this.isBlocked ? COLORS.textSecondary : COLORS.textPrimary;
      ctx.font = getFont(Math.min(FONTS.sizeTile, this.width * 0.35), 'bold');
      ctx.textAlign = 'center';
      ctx.textBaseline = 'middle';

      // 处理长文本
      const maxWidth = this.width - 16;
      const text = this.label;
      ctx.fillText(text, this.x + this.width / 2, this.y + this.height / 2, maxWidth);
    }

    ctx.restore();
  }

  /**
   * 播放选中动画
   */
  playSelectAnimation() {
    this.scale = 1.1;
    // 简单动画：150ms后恢复
    setTimeout(() => {
      this.scale = 1;
    }, 150);
  }

  /**
   * 播放匹配动画
   */
  playMatchAnimation(callback) {
    this.isMatching = true;
    this.scale = 1.2;
    setTimeout(() => {
      this.scale = 1;
      this.alpha = 0;
      this.isMatching = false;
      this.isRemoved = true;
      if (callback) callback();
    }, 300);
  }
}
