# HappyFamily

## 当前状态

项目已接入第一版 MVP vertical slice 骨架，目标是验证：

- 前院首页
- 配对消除主玩法
- 通关获得幸福星
- 解锁并选择“修院门”焕新节点

UI 字体现已切到 TextMeshPro，后续正式字体优先从 `Assets/Resources/Fonts` 加载。

## 运行方式

1. 用 Unity 2022.3.62t6 打开 `Happyfamily` 工程。
2. 打开 [SampleScene.scene](/Users/peizhengma/Documents/HappyFamily/Happyfamily/Assets/Scenes/SampleScene.scene)。
3. 直接点击 Play。

运行时会自动创建 UI，不需要先手动摆场景节点。

## 代码入口

- [HappyFamilyRuntimeBootstrap.cs](/Users/peizhengma/Documents/HappyFamily/Happyfamily/Assets/Scripts/Core/HappyFamilyRuntimeBootstrap.cs)
- [HappyFamilyGameApp.cs](/Users/peizhengma/Documents/HappyFamily/Happyfamily/Assets/Scripts/Core/HappyFamilyGameApp.cs)
- [MvpContentFactory.cs](/Users/peizhengma/Documents/HappyFamily/Happyfamily/Assets/Scripts/Data/MvpContentFactory.cs)
- [PairMatchBoard.cs](/Users/peizhengma/Documents/HappyFamily/Happyfamily/Assets/Scripts/Gameplay/PairMatchBoard.cs)
- [PlayerProgressService.cs](/Users/peizhengma/Documents/HappyFamily/Happyfamily/Assets/Scripts/Save/PlayerProgressService.cs)

## 下一步建议

- 把当前代码内置内容改成 ScriptableObject 配置
- 把首页和关卡页从运行时代码 UI，迁移为正式场景和预制体
- 加入第一章前院的 5-10 个关卡
- 接入第一个真实焕新节点表现

## 正式字体接入

推荐使用开源的 `Noto Sans SC` 作为项目内正式中文 UI 字体。

当前代码会优先尝试加载以下 TMP 字体资源：

- `Assets/Resources/Fonts/NotoSansCJKsc-Regular SDF.asset`
- `Assets/Resources/Fonts/NotoSansSC-Regular SDF.asset`
- `Assets/Resources/Fonts/UI-Regular SDF.asset`

建议在 Unity 中导入 OTF/TTF 后生成对应的 `TMP Font Asset`，放进上述路径之一。
