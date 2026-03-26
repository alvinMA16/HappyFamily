import Pool from './base/pool';

let instance;

/**
 * 全局状态管理器
 * 负责管理游戏的状态数据
 */
export default class DataBus {
  // 游戏帧数
  frame = 0;

  // 对象池
  pool = new Pool();

  // 当前游戏模式: 'pairMatch' | 'tidyUp'
  gameMode = null;

  // 当前关卡ID
  currentLevelId = null;

  // 游戏状态: 'playing' | 'paused' | 'won' | 'lost'
  gameState = 'playing';

  // 当前关卡结果
  levelResult = {
    stars: 0,
    score: 0,
    isWin: false,
  };

  constructor() {
    if (instance) return instance;
    instance = this;
  }

  /**
   * 重置游戏状态
   */
  reset() {
    this.frame = 0;
    this.gameMode = null;
    this.currentLevelId = null;
    this.gameState = 'playing';
    this.levelResult = {
      stars: 0,
      score: 0,
      isWin: false,
    };
  }

  /**
   * 开始新关卡
   * @param {string} mode - 游戏模式
   * @param {string} levelId - 关卡ID
   */
  startLevel(mode, levelId) {
    this.gameMode = mode;
    this.currentLevelId = levelId;
    this.gameState = 'playing';
    this.levelResult = {
      stars: 0,
      score: 0,
      isWin: false,
    };
  }

  /**
   * 设置关卡结果
   * @param {boolean} isWin - 是否胜利
   * @param {number} stars - 获得星星数
   * @param {number} score - 得分
   */
  setLevelResult(isWin, stars = 0, score = 0) {
    this.gameState = isWin ? 'won' : 'lost';
    this.levelResult = { isWin, stars, score };
  }

  /**
   * 暂停游戏
   */
  pause() {
    this.gameState = 'paused';
  }

  /**
   * 继续游戏
   */
  resume() {
    this.gameState = 'playing';
  }

  /**
   * 检查游戏是否在进行中
   */
  isPlaying() {
    return this.gameState === 'playing';
  }

  /**
   * 检查游戏是否结束
   */
  isGameOver() {
    return this.gameState === 'won' || this.gameState === 'lost';
  }
}
