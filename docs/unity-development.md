# NumberSlide Unity 版开发规格

本文档是 Unity 版的实现基准。按本文档搭建场景、Prefab 和组件即可开始开发，不需要再次估算棋盘或方块尺寸。

## 1. 固定环境

| 项目 | 约定 |
| --- | --- |
| Unity | `2022.3.62f3` |
| UI | `com.unity.ugui 1.0.0` |
| 文本 | TextMeshPro `3.0.7` |
| IDE | Rider 2026.2 |
| 根解决方案 | `NumberSlide.sln` |
| Unity 解决方案 | `src/NumberSlideUnity/NumberSlideUnity.sln` |
| Unity 工程 | `src/NumberSlideUnity` |
| 设计分辨率 | `1280 × 720` |
| 画布缩放 | `Scale With Screen Size` |
| 参考分辨率 | `1280 × 720` |
| Match | `0.5` |

`NumberSlide.sln` 是日常开发入口。Unity 自动生成的 `.sln` 和 `.csproj` 是派生物，不手工修改。

## 2. 图片素材

素材根目录：

```text
src/NumberSlideUnity/Assets/Images/
```

目录结构：

```text
Images/
├── asset-manifest.json
├── 3x3/
│   ├── Board.png
│   ├── Tile.png
│   ├── TileCorrect.png
│   └── TileHint.png
├── 4x4/
├── 5x5/
├── 6x6/
├── 7x7/
├── 8x8/
├── 9x9/
└── 10x10/
```

每阶目录都有四个文件：

| 文件 | 用途 |
| --- | --- |
| `Board.png` | 棋盘底板和空格槽 |
| `Tile.png` | 普通方块 |
| `TileCorrect.png` | 已处于正确位置的方块 |
| `TileHint.png` | 提示高亮方块 |

方块图不包含数字。数字统一使用 TextMeshPro 子对象显示，避免生成 1～99 的重复图片。

### 2.1 原生像素规格

所有 `Board.png` 全部为 `1024 × 1024`，不随难度改变。难度通过方块尺寸和间距改变。

| 地图 | 棋盘图片 | 方块图片 | 间距 | 棋盘内容区 | 居中偏移 |
| --- | ---: | ---: | ---: | ---: | ---: |
| 3×3 | 1024×1024 | 312×312 | 12 | 960 | 32 |
| 4×4 | 1024×1024 | 231×231 | 12 | 960 | 32 |
| 5×5 | 1024×1024 | 182×182 | 12 | 958 | 33 |
| 6×6 | 1024×1024 | 150×150 | 12 | 960 | 32 |
| 7×7 | 1024×1024 | 126×126 | 12 | 954 | 35 |
| 8×8 | 1024×1024 | 109×109 | 12 | 956 | 34 |
| 9×9 | 1024×1024 | 96×96 | 12 | 960 | 32 |
| 10×10 | 1024×1024 | 85×85 | 12 | 958 | 33 |

`Tile.png`、`TileCorrect.png`、`TileHint.png` 在同一阶目录中尺寸完全一致。

计算公式：

```text
contentPixels = grid * tilePixels + (grid - 1) * 12
offsetPixels = floor((1024 - contentPixels) / 2)
cellX = offsetPixels + column * (tilePixels + 12)
cellY = offsetPixels + row * (tilePixels + 12)
```

不要在运行时重新计算方块图片尺寸。根据当前难度加载对应目录的 Sprite。

### 2.2 游戏内显示尺寸

棋盘在 `1280 × 720` 画布中的显示区域固定为 `640 × 640`。

从 `1024` 缩放到 `640` 的统一比例为：

```text
displayScale = 640 / 1024 = 0.625
displayGap = 12 * 0.625 = 7.5 px
```

显示尺寸：

| 地图 | 原生方块 | 显示方块 | 方格步进 | 内容区显示 |
| --- | ---: | ---: | ---: | ---: |
| 3×3 | 312 | 195.000 | 202.5 | 600.000 |
| 4×4 | 231 | 144.375 | 151.875 | 600.000 |
| 5×5 | 182 | 113.750 | 121.250 | 598.750 |
| 6×6 | 150 | 93.750 | 101.250 | 600.000 |
| 7×7 | 126 | 78.750 | 86.250 | 596.250 |
| 8×8 | 109 | 68.125 | 75.625 | 597.500 |
| 9×9 | 96 | 60.000 | 67.500 | 600.000 |
| 10×10 | 85 | 53.125 | 60.625 | 598.750 |

显示坐标：

```text
displayOffset = offsetPixels * 0.625
displayTile = tilePixels * 0.625
displayCellX = displayOffset + column * (displayTile + 7.5)
displayCellY = displayOffset + row * (displayTile + 7.5)
```

`RectTransform` 允许小数像素。不要为了取整改变每个方块的尺寸，否则大尺寸棋盘会累积错位。

### 2.3 数字字号

数字使用 `TextMeshProUGUI`，中心对齐，`Best Fit` 关闭，使用下表中的固定字号：

| 地图 | 字号 |
| --- | ---: |
| 3×3 | 96 |
| 4×4 | 72 |
| 5×5 | 58 |
| 6×6 | 48 |
| 7×7 | 40 |
| 8×8 | 34 |
| 9×9 | 30 |
| 10×10 | 26 |

数字文本的 RectTransform 拉伸填充方块，左右内边距不少于显示方块宽度的 `6%`。数字内容为方块值，空格 `0` 不创建数字对象。

### 2.4 UI 配色

| 用途 | 色值 |
| --- | --- |
| 页面背景 | `#F6E7C8` |
| 棋盘顶部渐变 | `#C7B5A4` |
| 棋盘底部渐变 | `#A9917E` |
| 棋盘边框 | `#7D6A5B` |
| 普通方块顶部 | `#F1E3C9` |
| 普通方块底部 | `#D8C09E` |
| 普通方块边框 | `#B69E83` |
| 正确位置方块顶部 | `#C25A35` |
| 正确位置方块底部 | `#92361D` |
| 提示方块顶部 | `#EDB650` |
| 提示方块底部 | `#C98522` |
| 普通数字 | `#5C4B3A` |
| 正确位置数字 | `#FFF8E9` |
| 提示数字 | `#4B3214` |
| 主按钮 | `#B04A28` |
| 次按钮 | `#8D7562` |
| 按钮文字 | `#FFF8E9` |

### 2.5 Sprite 导入设置

每个 PNG 使用以下导入配置：

| 设置 | 值 |
| --- | --- |
| Texture Type | Sprite (2D and UI) |
| Sprite Mode | Single |
| Pixels Per Unit | 100 |
| Mesh Type | Full Rect |
| Extrude Edges | 0 |
| Wrap Mode | Clamp |
| Filter Mode | Bilinear |
| Compression | None |
| Generate Mip Maps | Off |
| Alpha Is Transparency | On |
| Read/Write | Off |

棋盘和方块都保持原始宽高比，不使用九宫格拉伸。不同难度使用独立预渲染图片。

重新生成素材：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File tools\generate-unity-images.ps1
```

生成完成后回到 Unity，等待 `AssetDatabase` 刷新。必须提交 Unity 生成的 `.meta` 文件。

## 3. 场景

Build Settings 顺序：

1. `Assets/Scenes/MainMenu.unity`
2. `Assets/Scenes/Game.unity`

### 3.1 MainMenu

根节点：

```text
MainMenu
├── Canvas
│   ├── Background
│   ├── MainCard
│   │   ├── Title
│   │   ├── Subtitle
│   │   ├── ModeButtons
│   │   │   ├── BeginnerButton
│   │   │   ├── ClassicButton
│   │   │   └── CustomButton
│   │   ├── CustomSizePanel
│   │   │   ├── SizeLabel
│   │   │   ├── SizeSlider
│   │   │   ├── SizeValue
│   │   │   ├── StartButton
│   │   │   └── CancelButton
│   │   └── QuitButton
│   └── VersionText
└── EventSystem
```

画布组件：

```text
Canvas
├── Render Mode: Screen Space - Overlay
├── Canvas Scaler: Scale With Screen Size
├── Reference Resolution: 1280 × 720
├── Screen Match Mode: Match Width Or Height
└── Match: 0.5
```

布局像素：

| 元素 | RectTransform |
| --- | --- |
| Background | 全屏拉伸，四边为 0 |
| MainCard | 760×560，居中，锚点 Center |
| Title | 680×110，卡片顶部内边距 48 |
| Subtitle | 680×48，位于标题下方 12 |
| ModeButtons | 420×248，按钮间距 16 |
| 模式按钮 | 420×72 |
| CustomSizePanel | 520×300，居中 |
| Start/Cancel 按钮 | 220×56，间距 24 |
| QuitButton | 180×52，卡片底部内边距 32 |

按钮文字：

| 按钮 | 文案 |
| --- | --- |
| BeginnerButton | `3 × 3 入门` |
| ClassicButton | `4 × 4 经典` |
| CustomButton | `5～10 自定义` |
| StartButton | `开始游戏` |
| CancelButton | `返回` |
| QuitButton | `退出游戏` |

难度选择后通过 `SceneManager.LoadScene("Game")` 进入游戏。难度参数使用 `GameSettings` 静态类或 ScriptableObject 传递，不能依赖场景对象查找。

### 3.2 Game

根节点：

```text
Game
├── Canvas
│   ├── Background
│   ├── BoardArea
│   │   ├── BoardImage
│   │   └── TileContainer
│   ├── InfoPanel
│   │   ├── SizeText
│   │   ├── MoveText
│   │   ├── TimeText
│   │   ├── HintButton
│   │   ├── UndoButton
│   │   ├── RestartButton
│   │   └── MenuButton
│   └── WinPanel
│       ├── Dimmer
│       ├── WinCard
│       │   ├── WinTitle
│       │   ├── WinSummary
│       │   ├── PlayAgainButton
│       │   └── MainMenuButton
└── EventSystem
```

布局：

| 元素 | 尺寸 | 位置 |
| --- | ---: | --- |
| BoardArea | 640×640 | 画布中心 X = -220，Y = 0 |
| BoardImage | 640×640 | 拉伸填满 BoardArea，四边为 0 |
| TileContainer | 640×640 | 与 BoardImage 完全重合 |
| InfoPanel | 360×640 | 画布中心 X = 400，Y = 0 |
| SizeText | 320×44 | InfoPanel 顶部内边距 24 |
| MoveText | 320×40 | SizeText 下方 24 |
| TimeText | 320×40 | MoveText 下方 8 |
| 功能按钮 | 156×56 | 两列，水平间距 24，垂直间距 16 |
| WinPanel | 全屏 | 默认 inactive |
| WinCard | 620×380 | 居中 |

功能按钮：

| 按钮 | 文案 |
| --- | --- |
| UndoButton | `撤销` |
| HintButton | `提示` |
| RestartButton | `重新开始` |
| MenuButton | `返回菜单` |

按钮字号统一为 `24`。主按钮使用 `#B04A28`，次按钮使用 `#8D7562`。

## 4. Prefab

### 4.1 Board.prefab

```text
Board
├── RectTransform: 640×640
├── CanvasRenderer
├── Image
│   ├── Source Image: 当前难度 Board.png
│   ├── Type: Simple
│   ├── Preserve Aspect: On
│   └── Raycast Target: Off
└── BoardView (MonoBehaviour)
```

`BoardView` 序列化字段：

```text
Image boardImage
RectTransform tileContainer
TileView tilePrefab
TileSkin[] tileSkins
```

`TileSkin` 包含：

```text
byte gridSize
Sprite board
Sprite tile
Sprite tileCorrect
Sprite tileHint
int tilePixels
int gapPixels
int offsetPixels
int numberFontSize
```

直接按 `asset-manifest.json` 填写上表数值，不在运行时从文件系统读取。

### 4.2 Tile.prefab

```text
Tile
├── RectTransform: tilePixels * 0.625 正方形
├── CanvasRenderer
├── Image
│   ├── Source Image: Tile.png
│   ├── Raycast Target: Off
│   └── Preserve Aspect: On
├── TileView (MonoBehaviour)
└── Number
    ├── RectTransform: 拉伸，四边为 0
    ├── TextMeshProUGUI
    ├── Alignment: Center / Middle
    ├── Font Size: numberFontSize
    ├── Raycast Target: Off
    └── Color: 普通数字色
```

`TileView` 公开接口：

```text
void SetIcon(Sprite tileSprite)
void SetValue(byte value, int fontSize)
void SetState(TileVisualState state)
```

`TileVisualState`：

```text
Normal
CorrectPosition
Hint
```

数字为 `0` 时不创建 Tile；对应位置只显示 Board.png 中的空格槽。

### 4.3 WinPanel.prefab

组件：

```text
Dimmer: Image，全屏黑色，Alpha 0.55
WinCard: Image，暖色面板，圆角由九宫格 Sprite 控制
WinTitle: TextMeshProUGUI，字号 52
WinSummary: TextMeshProUGUI，字号 26
PlayAgainButton: Button，220×56
MainMenuButton: Button，220×56
```

## 5. Unity 与 Core 的边界

Unity 表现层不能复制移动、可解性、撤销和完成判断逻辑，必须调用 `NumberSlide.Core`。

当前 Core 的职责：

```text
Board
├── BoardMap
├── Width
├── Empty
├── InitMap()
├── CanSolve()
├── SetEmpty()
└── Swap(...)

Game
├── board
├── MoveStack
├── Move(Direction)
├── Undo()
├── Restart()
└── IsComplete()
```

推荐流程：

```text
Unity Input
→ GameController
→ game.Move(Direction)
→ BoardView.Render(game.board)
→ HudView.Update(...)
→ if game.IsComplete() 显示 WinPanel
```

`BoardView` 只读取 `BoardMap` 和 `Empty`，不得直接交换格子。

### 5.1 Core 程序集接入

当前 `NumberSlide.Core` 目标是 `net10.0`，Unity 2022.3 不能直接引用这个目标框架的程序集。

开始写 Unity 脚本前，必须二选一：

1. 增加 `netstandard2.1` 构建目标，生成 Unity 可引用的 DLL。
2. 把 Core 整理成带 asmdef 的本地 UPM 包。

推荐第一阶段使用 DLL：

```text
src/NumberSlideUnity/Assets/Plugins/NumberSlide.Core/
├── NumberSlide.Core.dll
└── NumberSlide.Core.pdb
```

构建输出必须放在 Unity `Assets` 目录内，Unity 才会作为插件加载。不要把 `bin/obj` 目录放进 Assets。

Core 中的 `Random.Shared` 等 API 在 `netstandard2.1` 中不可用，生成 Unity 版本前需要替换为兼容实现。

## 6. 输入

Unity 2022.3 当前工程未安装 Input System，使用旧版 Input Manager。

```text
LeftArrow / A → Direction.Left
UpArrow    / W → Direction.Up
RightArrow / D → Direction.Right
DownArrow  / S → Direction.Down
Z              → Undo
R              → Restart
Esc            → 返回菜单或关闭胜利面板
```

方向键的含义是方块移动方向。按左键时，方块向左滑入空格，空格实际向右移动。

输入读取放在 `GameController.Update()`，求解器不允许逐帧运行。

## 7. 棋盘渲染

`BoardView.Render(Board board)` 不得每帧执行。仅在以下事件调用：

- 新局生成。
- 有效移动。
- 撤销。
- 重开。
- 提示状态改变。

首次生成 Tile 时创建并缓存所有 `TileView`。后续更新只修改数字、Sprite 和激活状态，不能每步 `Destroy/Instantiate`，也不能使用 `GameObject.Find`。

坐标原点与锚点：

```text
TileContainer Anchor: Top Left
TileContainer Pivot: Top Left
Tile Anchor: Top Left
Tile Pivot: Top Left
```

位置计算：

```text
x = displayOffset + column * (displayTile + displayGap)
y = -(displayOffset + row * (displayTile + displayGap))
```

UI 的 Y 轴向上，因此行向下移动时使用负 Y。

## 8. 状态显示

### 8.1 数字颜色

| 状态 | Sprite | 数字颜色 |
| --- | --- | --- |
| 普通 | Tile.png | `#5C4B3A` |
| 正确位置 | TileCorrect.png | `#FFF8E9` |
| 提示 | TileHint.png | `#4B3214` |

“正确位置”只在方块数字与目标位置完全一致时启用。

### 8.2 HUD

第一阶段 HUD 显示：

```text
棋盘：4 × 4
步数：42
时间：01:18
```

`Game.MoveCount` 尚未在 Core 中实现时，先在 Core 增加计数，不要在 `GameController` 复制一份。无效移动不增加步数，撤销是否减少步数按 README 规则执行。

## 9. 动画

方块移动动画建议使用协程和 `Mathf.SmoothStep`，不引入第三方 Tween 插件。

| 动画 | 时长 | 参数 |
| --- | ---: | --- |
| 方块滑动 | 0.10 s | Ease Out，位置线性插值 |
| 无效方向抖动 | 0.08 s | 左右各 3 px |
| 胜利变亮 | 0.20 s | 按方块编号延迟 0.02 s |

核心状态必须在动画开始前完成。动画只改变视觉 RectTransform，不允许阻塞下一次有效移动超过 0.10 秒。

## 10. 实现顺序

1. 创建 `MainMenu.unity` 和 `Game.unity`。
2. 完成 Canvas、Background、BoardArea、InfoPanel 布局。
3. 导入并检查 32 张 PNG 的 Sprite 设置。
4. 创建 `Tile.prefab` 和 `Board.prefab`。
5. 接入 `netstandard2.1` 版本的 `NumberSlide.Core`。
6. 实现 `BoardView` 的按难度取图和坐标计算。
7. 实现 `GameController` 的方向键、撤销、重开。
8. 实现 HUD 和胜利面板。
9. 增加滑动动画和提示状态。
10. 在 1280×720、1920×1080、16:9 窗口下逐个难度手动验收。

## 11. 验收清单

- MainMenu 是 Build Settings 第一场景。
- 3×3～10×10 都能创建合法且可解的棋盘。
- 每种难度的 Board.png 都显示为正方形。
- 每种难度的方块尺寸与本文档表格一致。
- 3×3 方块 312×312，10×10 方块 85×85。
- 数字 0 不显示 Tile。
- 方向键移动的是方块，空格向反方向移动。
- 无效移动不增加步数。
- 撤销恢复上一步可见棋盘。
- 重开清空历史和步数。
- 完成后显示 WinPanel，禁止继续移动。
- 所有 Sprite 的 `.meta` 文件已提交到 Git。
- 场景中不存在高频 `GameObject.Find`。
- 求解器只在提示和测试中运行。

## 12. 已知前置问题

在 Unity 脚本开始引用 Core 前，必须完成以下事项：

- 把 Core 编译为 Unity 可加载的 `netstandard2.1` 程序集。
- 修正 Core 当前与 Unity 目标框架不兼容的 API。
- 在 Core 中补充 Unity HUD 所需的步数状态。
- 固定 `NumberSlide.sln` 中 `Assembly-CSharp.csproj` 的生成顺序。

这些属于 Core 接入任务，不应在 `BoardView` 或 `GameController` 中用重复逻辑绕过。
