/**
 * Canvas 绘图辅助函数
 * 兼容微信小游戏 Canvas API
 */

/**
 * 绘制圆角矩形路径
 * @param {CanvasRenderingContext2D} ctx - Canvas 上下文
 * @param {number} x - 左上角 X 坐标
 * @param {number} y - 左上角 Y 坐标
 * @param {number} width - 宽度
 * @param {number} height - 高度
 * @param {number} radius - 圆角半径
 */
export function drawRoundRect(ctx, x, y, width, height, radius) {
  // 确保圆角半径不超过宽高的一半
  const r = Math.min(radius, width / 2, height / 2);

  ctx.beginPath();
  ctx.moveTo(x + r, y);
  ctx.lineTo(x + width - r, y);
  ctx.arcTo(x + width, y, x + width, y + r, r);
  ctx.lineTo(x + width, y + height - r);
  ctx.arcTo(x + width, y + height, x + width - r, y + height, r);
  ctx.lineTo(x + r, y + height);
  ctx.arcTo(x, y + height, x, y + height - r, r);
  ctx.lineTo(x, y + r);
  ctx.arcTo(x, y, x + r, y, r);
  ctx.closePath();
}

/**
 * 绘制并填充圆角矩形
 * @param {CanvasRenderingContext2D} ctx - Canvas 上下文
 * @param {number} x - 左上角 X 坐标
 * @param {number} y - 左上角 Y 坐标
 * @param {number} width - 宽度
 * @param {number} height - 高度
 * @param {number} radius - 圆角半径
 * @param {string} fillColor - 填充颜色
 */
export function fillRoundRect(ctx, x, y, width, height, radius, fillColor) {
  drawRoundRect(ctx, x, y, width, height, radius);
  ctx.fillStyle = fillColor;
  ctx.fill();
}

/**
 * 绘制圆角矩形边框
 * @param {CanvasRenderingContext2D} ctx - Canvas 上下文
 * @param {number} x - 左上角 X 坐标
 * @param {number} y - 左上角 Y 坐标
 * @param {number} width - 宽度
 * @param {number} height - 高度
 * @param {number} radius - 圆角半径
 * @param {string} strokeColor - 边框颜色
 * @param {number} lineWidth - 边框宽度
 */
export function strokeRoundRect(ctx, x, y, width, height, radius, strokeColor, lineWidth = 1) {
  drawRoundRect(ctx, x, y, width, height, radius);
  ctx.strokeStyle = strokeColor;
  ctx.lineWidth = lineWidth;
  ctx.stroke();
}
