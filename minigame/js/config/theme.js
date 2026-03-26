/**
 * 主题配置 - 适老化 UI 设计
 * 面向50-70岁中老年用户
 */

// 颜色配置 - 高对比度暖色调
export const COLORS = {
  // 背景色
  background: '#FFF8E7',       // 米白色背景
  backgroundDark: '#F5E6C8',   // 深米色

  // 主色调
  primary: '#D4A574',          // 暖棕色
  primaryDark: '#B8956A',      // 深暖棕
  primaryLight: '#E8C9A0',     // 浅暖棕

  // 强调色
  accent: '#E85D4C',           // 暖红色（用于重要操作）
  accentLight: '#FF8A7A',      // 浅红
  success: '#6BAF5C',          // 成功绿
  warning: '#F5A623',          // 警告橙

  // 文字颜色
  textPrimary: '#4A3728',      // 深棕色主文字
  textSecondary: '#7A6558',    // 次要文字
  textLight: '#FFFFFF',        // 浅色文字

  // 图块颜色（用于绘制游戏图块）
  tileBackground: '#FFFFFF',
  tileBorder: '#D4A574',
  tileSelected: '#FFE4B5',
  tileMatched: '#90EE90',
  tileBlocked: '#CCCCCC',

  // 收集槽颜色
  slotEmpty: '#F0E0C8',
  slotFilled: '#FFFFFF',
  slotBorder: '#C4A484',
};

// 字体配置 - 大字体易读
export const FONTS = {
  // 字体族
  family: 'sans-serif',

  // 字体大小
  sizeTitle: 48,               // 标题
  sizeSubtitle: 36,            // 副标题
  sizeNormal: 28,              // 正文
  sizeTile: 32,                // 图块文字
  sizeButton: 32,              // 按钮文字
  sizeSmall: 24,               // 小字

  // 行高
  lineHeight: 1.5,
};

// 尺寸配置 - 大触摸目标
export const SIZES = {
  // 最小触摸目标（适老化要求 >= 80px）
  minTouchTarget: 80,

  // 按钮尺寸
  buttonWidth: 240,
  buttonHeight: 80,
  buttonRadius: 16,

  // 图块尺寸
  tileSize: 90,                // 配对消除图块大小
  tileGap: 8,                  // 图块间距
  tileRadius: 12,              // 图块圆角

  // 收集槽
  slotSize: 70,
  slotGap: 8,

  // 边距
  paddingLarge: 32,
  paddingMedium: 20,
  paddingSmall: 12,

  // 顶部状态栏高度
  headerHeight: 100,
};

// 动画配置
export const ANIMATION = {
  // 动画时长（毫秒）
  durationShort: 150,
  durationNormal: 300,
  durationLong: 500,

  // 缓动函数类型
  easeOut: 'easeOut',
  easeIn: 'easeIn',
  easeInOut: 'easeInOut',
};

// 游戏配置
export const GAME = {
  // 配对消除
  pairMatch: {
    cols: 4,                   // 默认列数
    maxTilesPerLevel: 16,      // 每关最大图块数
  },

  // 收纳整理
  tidyUp: {
    slotCount: 7,              // 收集槽数量
    matchRequired: 3,          // 消除需要的相同物品数
    gridWidth: 4,              // 网格宽度
    gridHeight: 4,             // 网格高度
    maxLayers: 3,              // 最大层数
  },
};

// 工具函数
export function getFont(size, weight = 'normal') {
  return `${weight} ${size}px ${FONTS.family}`;
}

export function hexToRgba(hex, alpha = 1) {
  const r = parseInt(hex.slice(1, 3), 16);
  const g = parseInt(hex.slice(3, 5), 16);
  const b = parseInt(hex.slice(5, 7), 16);
  return `rgba(${r}, ${g}, ${b}, ${alpha})`;
}
