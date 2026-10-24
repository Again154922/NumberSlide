# NumberSlide Unity 版开发规格

本文档是 Unity 版的实现基准。按本文档搭建场景、Prefab 和组件即可开始开发，不需要再次估算棋盘或方块尺寸。

## 1. 固定环境

| 项目 | 约定 |
| --- | --- |
| Unity | `2022.3.62f3` |
| UI | `com.unity.ugui 1.0.0` |
| 文本 | TextMeshPro `3.0.7` |
| IDE | Rider 2026.2 |
| 控制台解决方案 | `NumberSlideConsole/NumberSlideConsole.sln` |
| Unity 解决方案 | `NumberSlideUnity/NumberSlideUnity.sln` |
| Unity 工程 | `NumberSlideUnity` |
| 设计分辨率 | `1280 × 720` |
| 画布缩放 | `Scale With Screen Size` |
| 参考分辨率 | `1280 × 720` |
| Match | `0.5` |

Unity 开发使用 `NumberSlideUnity.sln`，控制台和 Core 开发使用 `NumberSlideConsole.sln`。Unity 自动生成的 `.sln` 和 `.csproj` 是派生物，不手工修改。

## 2. 图片素材

素材根目录：

```text
NumberSlideUnity/Assets/Images/
```

目录结构：

```text
Images/
├── asset-manifest.json
├── 3x3/
│   ├── Board.png
│   └── Tile.png
├── 4x4/
├── 5x5/
├── 6x6/
├── 7x7/
├── 8x8/
├── 9x9/
└── 10x10/
```

每阶目录包含一张棋盘图和一张空白方块：

| 文件 | 用途 |
| --- | --- |
| `Board.png` | 棋盘底板和空格槽 |
| `Tile.png` | 所有数字共用的空白方块 |

数字不烘焙进方块图片，而是由 Tile Prefab 中的 TextMeshPro 子对象显示。

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

`Tile.png` 的尺寸由难度决定，下表仍然适用。

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

### 2.3 TextMeshPro 数字字号

数字使用 `TextMeshProUGUI` 居中显示，建议字号：

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

数字为 `0` 时不创建 Tile；棋盘底图的槽位直接作为空格。

### 2.4 UI 配色

| 用途 | 色值 |
| --- | --- |
| 页面背景 | `#F6E7C8` |
| 棋盘顶部渐变 | `#C7B5A4` |
| 棋盘底部渐变 | `#A9917E` |
| 棋盘边框 | `#7D6A5B` |
| 方块顶部 | `#F1E3C9` |
| 方块底部 | `#D8C09E` |
| 方块边框 | `#B69E83` |
| 正确位置高亮 | `#C25A35` |
| 提示高亮 | `#E0A33A` |
| 普通数字 | `#5C4B3A` |
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

### 2.6 Unity 世界单位与像素换算

所有 Sprite 的 `Pixels Per Unit` 固定为 `100`，因此：

```text
1 Unity 世界单位 = 100 像素
1 像素 = 0.01 Unity 世界单位
```

当 SpriteRenderer 的 `Transform.localScale = (1, 1, 1)` 时：

| 资源 | 像素尺寸 | 世界尺寸 |
| --- | ---: | ---: |
| Board.png | 1024×1024 | 10.24×10.24 |
| 3×3 Tile | 312×312 | 3.12×3.12 |
| 4×4 Tile | 231×231 | 2.31×2.31 |
| 5×5 Tile | 182×182 | 1.82×1.82 |
| 6×6 Tile | 150×150 | 1.50×1.50 |
| 7×7 Tile | 126×126 | 1.26×1.26 |
| 8×8 Tile | 109×109 | 1.09×1.09 |
| 9×9 Tile | 96×96 | 0.96×0.96 |
| 10×10 Tile | 85×85 | 0.85×0.85 |
| 格间距 | 12 | 0.12 |

如果把整个棋盘作为 SpriteRenderer 放进世界：

```text
1024 px / 100 PPU = 10.24 Unity 世界单位
```

当 `orthographicSize = 6` 时，摄像机可视高度是 `12` 世界单位，因此 `10.24×10.24` 的棋盘可以在高度方向完整显示。

注意：棋盘 JSON 的坐标单位是 Unity 世界单位，不是 Canvas 参考像素。若使用 World Space Canvas 并把 Canvas Scale 设为 `1`，世界坐标可以直接作为 RectTransform 的局部坐标；若使用 `640×640` 的 Screen Space 棋盘，则需要乘以 `640 / 10.24 = 62.5` 转换成 Canvas 单位。

重新生成素材：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File tools\generate-unity-images.ps1
```

生成完成后回到 Unity，等待 `AssetDatabase` 刷新。必须提交 Unity 生成的 `.meta` 文件。

### 2.7 棋盘格子坐标

每个难度的全部格子坐标位于：

```text
NumberSlideUnity/Assets/Resources/BoardLayouts/
├── 3x3.json
├── 4x4.json
├── 5x5.json
├── 6x6.json
├── 7x7.json
├── 8x8.json
├── 9x9.json
└── 10x10.json
```

坐标系统：

```text
原点：10.24×10.24 棋盘中心
X 轴：向右为正
Y 轴：向上为正
单位：Unity 世界单位
```

每个 JSON 包含当前棋盘全部 `grid × grid` 个格子，空格所在的坐标同样存在。以下情况可以近似直接使用这些局部坐标：

```text
World Space Canvas: localScale = 1
Board RectTransform: 10.24 × 10.24
Board: Anchor Min/Max = (0.5, 0.5), Pivot = (0.5, 0.5)
Tile:  Anchor Min/Max = (0.5, 0.5), Pivot = (0.5, 0.5)
```

如果使用 Screen Space Canvas 的 `640×640` 棋盘，则要先执行 `position *= 62.5`。

示例：

```json
[
  [
    [3.24, 3.24],
    [0.0, 3.24],
    [-3.24, 3.24]
  ],
  [
    [3.24, 0.0],
    [0.0, 0.0],
    [-3.24, 0.0]
  ],
  [
    [3.24, -3.24],
    [0.0, -3.24],
    [-3.24, -3.24]
  ]
]
```

JSON 顶层是行数组，每一行是列数组，每个格子是 `[x, y]`，坐标单位是世界单位。索引方式：

```csharp
Vector2 localPosition = new(values[row][column][0], values[row][column][1]);
```

由于文件顶层是数组，Unity 自带的 `JsonUtility` 不能直接反序列化。可以使用 Newtonsoft.Json 的 `JsonConvert.DeserializeObject<float[][][]>`，或实现一个只支持数字数组的轻量解析器。

`System.Text.Json.JsonSerializer` 在 Unity 2022.3 中不是默认引用的 API。它会自动支持顶层数组，但需要额外安装兼容的 `System.Text.Json` NuGet 包。`System.Web.Script.Serialization.JavaScriptSerializer` 也不属于 Unity 默认可用 API，不建议在 Unity 项目中使用。

重新生成布局 JSON：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File tools\generate-board-layouts.ps1
```

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
├── Pixel Perfect: Off
├── Sort Order: 0
├── Canvas Scaler: Scale With Screen Size
├── Reference Resolution: 1280 × 720
├── Screen Match Mode: Match Width Or Height
├── Match: 0.5
├── Reference Pixels Per Unit: 100
└── Graphic Raycaster: On
```

每个场景只保留一个根 Canvas 和一个 Canvas Scaler。Tile Prefab 内部不要再放 Canvas，也不要再放 Canvas Scaler。Tile 内的 `TextMeshProUGUI` 会自动继承根 Canvas。

额外 Canvas 只用于确实需要独立排序层、独立渲染批次或独立缩放的特殊 UI。给 99 个 Tile 每个加一个 Canvas 会制造 99 个独立 Canvas，增加绘制开销并破坏合批。

Tile 中 TMP 的设置：

```text
Component: TextMeshProUGUI
Font Asset: 项目统一选择的 TMP Font Asset
Font Size: 按下文难度表
Auto Size: Off
Alignment: Center / Middle
Rich Text: Off（纯数字不需要）
Raycast Target: Off
```

如果使用普通 SpriteRenderer 而非 UGUI，则文本组件应使用 `TextMeshPro`，不是 `TextMeshProUGUI`，并且需要 MeshRenderer 和正确的 Sorting Layer/Order。

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
int tilePixels
int gapPixels
int offsetPixels
```

每个难度只需要引用对应目录的一张 `Tile.png`。数字由 Tile Prefab 内的 TextMeshPro 子对象显示。直接按下表和 `asset-manifest.json` 填写尺寸，不在运行时从文件系统读取。

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
    ├── RectTransform: 拉伸填满 Tile，四边为 0
    ├── TextMeshProUGUI
    ├── Alignment: Center / Middle
    ├── Raycast Target: Off
    └── Color: #5C4B3A
```

`TileView` 公开接口：

```text
void SetNumber(byte value)
void SetHighlight(bool enabled)
```

数字 TextMeshPro 子对象不需要额外 Canvas。它会自动继承 Tile 所在的根 Canvas。不要为每个 Tile 添加 Canvas 或 Canvas Scaler。数字为 `0` 时不创建 Tile；对应位置只显示 `Board.png` 中的空格槽。

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
NumberSlideUnity/Assets/Plugins/NumberSlide.Core/
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
Board/TileContainer Anchor Min: (0.5, 0.5)
Board/TileContainer Anchor Max: (0.5, 0.5)
Board/TileContainer Pivot: (0.5, 0.5)
Tile Anchor Min: (0.5, 0.5)
Tile Anchor Max: (0.5, 0.5)
Tile Pivot: (0.5, 0.5)
```

按 `Resources/BoardLayouts/{grid}x{grid}.json` 设置位置：

```csharp
var cell = values[row][column];
Vector2 worldPosition = new(cell[0], cell[1]);

// World Space Canvas，Scale = 1，Board = 10.24 × 10.24
rectTransform.localPosition = worldPosition;

// 或者 Screen Space Canvas，Board = 640 × 640
rectTransform.anchoredPosition = worldPosition * 62.5f;
```

当 Board 和 Tile 的 Pivot 都是中心，且两者 Anchor 都位于中心时，`anchoredPosition` 与 `localPosition` 在同一坐标单位下数值相同。JSON 本身使用世界单位，不能直接当作 640 Canvas 的像素坐标。

### 7.1 Camera 正交尺寸与 RectTransform

Unity 的 `Camera.orthographicSize` 表示可见高度的**一半**：

```text
可视世界高度 = orthographicSize × 2
可视顶部 Y   = orthographicSize
可视底部 Y   = -orthographicSize
```

当 `orthographicSize = 6` 时，摄像机中心在 `Y = 0`，可视顶部是 `Y = 6`。

如果希望 RectTransform 的 `localPosition = (0, 6)` 正好表示对象上沿位于屏幕顶部，需要：

```text
父物体/Canvas Pivot: (0.5, 0.5)
对象 Anchor Min: (0.5, 0.5)
对象 Anchor Max: (0.5, 0.5)
对象 Pivot: (0.5, 1.0)
对象 localPosition / anchoredPosition: (0, 6)
```

此时 RectTransform 的原点位于对象顶部中心，`(0, 6)` 正好是顶部坐标。

如果使用 Top-Center Anchor，则更常见的设置是：

```text
Anchor Min/Max: (0.5, 1)
Pivot: (0.5, 1)
anchoredPosition: (0, 0)
```

这种设置会让对象上沿始终贴合父物体顶部，但不保证 `localPosition` 等于 `(0, 6)`。

注意：

- `orthographicSize = 6` 的可视高度是 `12`，不是 `6`。
- 如果可视高度真的只有 `6`，则 `orthographicSize = 3`，顶部 Y 是 `3`，此时坐标应为 `(0, 3)`。
- Screen Space UI 的 `localPosition` 单位是 Canvas 参考像素，不是世界单位。若要直接用世界坐标 `(0, 6)`，应使用 World Space Canvas，或按 Canvas 缩放进行坐标转换。

## 8. 状态显示

### 8.1 方块显示

| 状态 | 显示方式 |
| --- | --- |
| 普通 | `Tile.png`，数字由 TMP 子对象显示 |
| 正确位置 | 保持原 Sprite，添加 `Outline`，颜色 `#C25A35` |
| 提示 | 保持原 Sprite，添加 `Outline`，颜色 `#E0A33A`，可放大至 `1.04` |

正确位置和提示效果不增加图片文件，使用 UGUI 的 `Outline`、`Image.color` 或一个不显示文字的彩色 Overlay 实现。

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
- 分别使用 Unity 与控制台的解决方案，不要把 Unity 自动生成项目加入控制台解决方案。

这些属于 Core 接入任务，不应在 `BoardView` 或 `GameController` 中用重复逻辑绕过。
