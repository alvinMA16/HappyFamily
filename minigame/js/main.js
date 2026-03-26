import './render';
import DataBus from './databus';
import SceneManager from './scenes/sceneManager';
import HomeScene from './scenes/homeScene';
import PairMatchScene from './scenes/pairMatchScene';
import TidyUpScene from './scenes/tidyUpScene';
import ResultScene from './scenes/resultScene';

const ctx = canvas.getContext('2d');

// 全局数据管理
GameGlobal.databus = new DataBus();
GameGlobal.sceneManager = new SceneManager();

/**
 * 游戏主类
 */
export default class Main {
  aniId = 0;

  constructor() {
    this.init();
    this.bindEvents();
    this.start();
  }

  /**
   * 初始化场景管理器和注册场景
   */
  init() {
    const sceneManager = GameGlobal.sceneManager;
    sceneManager.init(ctx);

    // 注册所有场景
    sceneManager.register('home', HomeScene);
    sceneManager.register('pairMatch', PairMatchScene);
    sceneManager.register('tidyUp', TidyUpScene);
    sceneManager.register('result', ResultScene);
  }

  /**
   * 绑定触摸事件
   */
  bindEvents() {
    wx.onTouchStart((e) => {
      const touch = e.touches[0];
      GameGlobal.sceneManager.onTouchStart(touch.clientX, touch.clientY);
    });

    wx.onTouchEnd((e) => {
      const touch = e.changedTouches[0];
      GameGlobal.sceneManager.onTouchEnd(touch.clientX, touch.clientY);
    });
  }

  /**
   * 开始游戏
   */
  start() {
    // 切换到首页场景
    GameGlobal.sceneManager.switchTo('home');

    // 开始游戏循环
    cancelAnimationFrame(this.aniId);
    this.aniId = requestAnimationFrame(this.loop.bind(this));
  }

  /**
   * 游戏主循环
   */
  loop() {
    GameGlobal.databus.frame++;

    // 更新场景
    GameGlobal.sceneManager.update();

    // 清空画布
    ctx.clearRect(0, 0, canvas.width, canvas.height);

    // 渲染场景
    GameGlobal.sceneManager.render();

    // 继续循环
    this.aniId = requestAnimationFrame(this.loop.bind(this));
  }
}
