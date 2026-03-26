import { COLORS, FONTS, SIZES, getFont } from '../config/theme';
import { drawRoundRect } from '../utils/canvasHelper';

/**
 * 按钮组件
 * 适老化设计：大触摸目标、高对比度
 */
export default class Button {
  constructor(options = {}) {
    this.text = options.text || '按钮';
    this.x = options.x || 0;
    this.y = options.y || 0;
    this.width = options.width || SIZES.buttonWidth;
    this.height = options.height || SIZES.buttonHeight;

    // 颜色配置
    this.bgColor = options.bgColor || COLORS.primary;
    this.bgColorPressed = options.bgColorPressed || COLORS.primaryDark;
    this.textColor = options.textColor || COLORS.textLight;
    this.borderColor = options.borderColor || COLORS.primaryDark;

    // 状态
    this.isPressed = false;
    this.isDisabled = options.isDisabled || false;

    // 回调
    this.onClick = options.onClick || null;
  }

  /**
   * 设置按钮位置（居中对齐时使用）
   */
  setPosition(x, y) {
    this.x = x;
    this.y = y;
  }

  /**
   * 居中按钮（水平方向）
   * @param {number} screenWidth - 屏幕宽度
   */
  centerHorizontally(screenWidth) {
    this.x = (screenWidth - this.width) / 2;
  }

  /**
   * 检测点是否在按钮区域内
   */
  containsPoint(px, py) {
    return (
      px >= this.x &&
      px <= this.x + this.width &&
      py >= this.y &&
      py <= this.y + this.height
    );
  }

  /**
   * 处理触摸开始
   */
  onTouchStart(x, y) {
    if (this.isDisabled) return false;
    if (this.containsPoint(x, y)) {
      this.isPressed = true;
      return true;
    }
    return false;
  }

  /**
   * 处理触摸结束
   */
  onTouchEnd(x, y) {
    if (this.isDisabled) return false;
    const wasPressed = this.isPressed;
    this.isPressed = false;

    if (wasPressed && this.containsPoint(x, y) && this.onClick) {
      this.onClick();
      return true;
    }
    return false;
  }

  /**
   * 渲染按钮
   */
  render(ctx) {
    const radius = SIZES.buttonRadius;

    // 背景颜色
    let bgColor = this.bgColor;
    if (this.isDisabled) {
      bgColor = COLORS.tileBlocked;
    } else if (this.isPressed) {
      bgColor = this.bgColorPressed;
    }

    // 绘制圆角矩形背景
    drawRoundRect(ctx, this.x, this.y, this.width, this.height, radius);
    ctx.fillStyle = bgColor;
    ctx.fill();

    // 绘制边框
    ctx.strokeStyle = this.isDisabled ? COLORS.textSecondary : this.borderColor;
    ctx.lineWidth = 2;
    ctx.stroke();

    // 绘制文字（根据按钮大小调整字体）
    const fontSize = Math.min(24, this.height * 0.4, this.width * 0.25);
    ctx.fillStyle = this.isDisabled ? COLORS.textSecondary : this.textColor;
    ctx.font = getFont(fontSize, 'bold');
    ctx.textAlign = 'center';
    ctx.textBaseline = 'middle';
    ctx.fillText(this.text, this.x + this.width / 2, this.y + this.height / 2);
  }
}
