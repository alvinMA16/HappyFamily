/**
 * 存档系统
 * 使用微信小游戏的本地存储
 */

const SAVE_KEY = 'happyfamily_save';

let instance;

export default class SaveData {
  // 存档数据
  data = {
    totalStars: 0,
    completedLevelIds: [],
    levelStars: {},  // { levelId: stars }
    renovationSelections: [],  // 翻新选择记录
  };

  constructor() {
    if (instance) return instance;
    instance = this;
    this.load();
  }

  /**
   * 加载存档
   */
  load() {
    try {
      const saved = wx.getStorageSync(SAVE_KEY);
      if (saved) {
        this.data = { ...this.data, ...saved };
        console.log('SaveData loaded:', this.data);
      }
    } catch (e) {
      console.error('SaveData load error:', e);
    }
  }

  /**
   * 保存存档
   */
  save() {
    try {
      wx.setStorageSync(SAVE_KEY, this.data);
      console.log('SaveData saved:', this.data);
    } catch (e) {
      console.error('SaveData save error:', e);
    }
  }

  /**
   * 获取总星星数
   */
  getTotalStars() {
    return this.data.totalStars;
  }

  /**
   * 检查关卡是否已完成
   * @param {string} levelId
   */
  isLevelCompleted(levelId) {
    return this.data.completedLevelIds.includes(levelId);
  }

  /**
   * 获取关卡星星数
   * @param {string} levelId
   */
  getLevelStars(levelId) {
    return this.data.levelStars[levelId] || 0;
  }

  /**
   * 完成关卡
   * @param {string} levelId - 关卡ID
   * @param {number} stars - 获得的星星数
   */
  completeLevel(levelId, stars) {
    // 检查是否是首次完成
    const isFirstCompletion = !this.data.completedLevelIds.includes(levelId);

    if (isFirstCompletion) {
      this.data.completedLevelIds.push(levelId);
    }

    // 更新最高星星数
    const previousStars = this.data.levelStars[levelId] || 0;
    if (stars > previousStars) {
      this.data.levelStars[levelId] = stars;
      // 更新总星星数
      this.data.totalStars += (stars - previousStars);
    }

    this.save();
  }

  /**
   * 记录翻新选择
   * @param {string} nodeId - 翻新节点ID
   * @param {string} optionId - 选择的选项ID
   */
  setRenovationSelection(nodeId, optionId) {
    // 移除旧选择
    this.data.renovationSelections = this.data.renovationSelections.filter(
      s => s.nodeId !== nodeId
    );
    // 添加新选择
    this.data.renovationSelections.push({ nodeId, optionId });
    this.save();
  }

  /**
   * 获取翻新选择
   * @param {string} nodeId
   */
  getRenovationSelection(nodeId) {
    const selection = this.data.renovationSelections.find(s => s.nodeId === nodeId);
    return selection ? selection.optionId : null;
  }

  /**
   * 重置存档
   */
  reset() {
    this.data = {
      totalStars: 0,
      completedLevelIds: [],
      levelStars: {},
      renovationSelections: [],
    };
    this.save();
  }
}
