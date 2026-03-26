/**
 * 场景管理器
 * 负责场景的切换、生命周期管理
 */

let instance;

export default class SceneManager {
  currentScene = null;
  scenes = {};
  ctx = null;

  constructor() {
    if (instance) return instance;
    instance = this;
  }

  /**
   * 初始化场景管理器
   * @param {CanvasRenderingContext2D} ctx - Canvas 2D 上下文
   */
  init(ctx) {
    this.ctx = ctx;
  }

  /**
   * 注册场景
   * @param {string} name - 场景名称
   * @param {Object} SceneClass - 场景类
   */
  register(name, SceneClass) {
    this.scenes[name] = SceneClass;
  }

  /**
   * 切换到指定场景
   * @param {string} name - 场景名称
   * @param {Object} params - 传递给场景的参数
   */
  switchTo(name, params = {}) {
    // 退出当前场景
    if (this.currentScene) {
      if (typeof this.currentScene.onExit === 'function') {
        this.currentScene.onExit();
      }
      this.currentScene = null;
    }

    // 创建新场景
    const SceneClass = this.scenes[name];
    if (!SceneClass) {
      console.error(`Scene "${name}" not found`);
      return;
    }

    this.currentScene = new SceneClass(params);

    // 进入新场景
    if (typeof this.currentScene.onEnter === 'function') {
      this.currentScene.onEnter();
    }

    console.log(`Switched to scene: ${name}`);
  }

  /**
   * 更新当前场景
   */
  update() {
    if (this.currentScene && typeof this.currentScene.update === 'function') {
      this.currentScene.update();
    }
  }

  /**
   * 渲染当前场景
   */
  render() {
    if (this.currentScene && typeof this.currentScene.render === 'function') {
      this.currentScene.render(this.ctx);
    }
  }

  /**
   * 处理触摸事件
   * @param {number} x - 触摸 x 坐标
   * @param {number} y - 触摸 y 坐标
   */
  onTouchStart(x, y) {
    if (this.currentScene && typeof this.currentScene.onTouchStart === 'function') {
      this.currentScene.onTouchStart(x, y);
    }
  }

  /**
   * 处理触摸结束事件
   * @param {number} x - 触摸 x 坐标
   * @param {number} y - 触摸 y 坐标
   */
  onTouchEnd(x, y) {
    if (this.currentScene && typeof this.currentScene.onTouchEnd === 'function') {
      this.currentScene.onTouchEnd(x, y);
    }
  }
}
