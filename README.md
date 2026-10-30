# Keyboard-operation-and-mouse-control

# 键盘操作鼠标 (Keyboard-operation-and-mouse-control)

一个基于 C# / .NET 的 Windows 控制台小工具： **用键盘替代鼠标** ，在不离开键盘的情况下完成鼠标移动、点击、滚轮、Home/End 等操作，移动时不会触发按键本身的字符输入。

---

## 简介

本项目通过全局键盘钩子监听按键状态，并使用 Win32 API（`SetCursorPos`、`mouse_event`）模拟鼠标行为，配合 WindowsInput 库实现按键拦截。适用于需要在键盘与鼠标之间频繁切换的场景（如阅读、远程操作、单手握持设备等）。

项目采用**整洁架构（Clean Architecture）**分层组织代码：`Input` 处理键盘状态与业务，`Mouse` 封装所有鼠标副作用，`Native` 集中 P/Invoke，`Audio` 负责提示音，`Program` 只做装配与启动。

---

## 功能特性

* **双重开关** ：`Alt + Q` 开启功能，`CapsLock` 切换监听状态，防止误触
* **八方向移动** ：I / K / J / L 四个方向 + 四个斜向组合
* **六档速度调节** ：按住空格 + `-` / `+` 增减挡位，带内嵌提示音
* **完整鼠标按键** ：左键、中键、右键均支持按下/松开（可拖拽）
* **滚轮控制** ：垂直上下滚动
* **补充按键** ：Home / End 快捷输入
* **多显示器支持** ：通过 `SetProcessDPIAware` + 虚拟桌面边界钳制，跨屏坐标不再溢出
* **按键屏蔽** ：移动/点击时通过 `e.Next_Hook_Enabled = false` 阻止按键字符输入

---

## 环境要求

* Windows 10 / 11
* .NET 10（项目目标框架为 `net10.0-windows`）
* 依赖库：[WindowsInput](https://github.com/michaelnoonan/inputsimulator) `6.4.1`（使用其 `WindowsInput.Events.Sources` 命名空间）

---

## 快速开始

1. 编译项目，确保 `功能手册.txt` 与音频资源（`Key2Mouse.Audio.*.wav`）已嵌入/置于输出目录。
2. 运行程序，控制台会打印功能手册并进入监听状态。
3. 按 `Alt + Q` 激活功能，再按 `CapsLock` 打开监听（二重锁）。
4. 使用下方操作速查表中的按键控制鼠标。
5. 再次按 `CapsLock` 可暂停监听，`Alt + Q` 可完全关闭功能。

---

## 操作速查表

### 开关与切换

| 按键         | 功能                                          |
| ------------ | --------------------------------------------- |
| `Alt + Q`  | 开启 / 关闭键盘模拟鼠标功能                   |
| `CapsLock` | 功能开启后切换监听状态（需先`Alt + Q`开启） |

### 鼠标移动

| 按键      | 功能     |
| --------- | -------- |
| `I`     | 上移     |
| `K`     | 下移     |
| `J`     | 左移     |
| `L`     | 右移     |
| `J + I` | 左上斜移 |
| `J + K` | 左下斜移 |
| `K + L` | 右下斜移 |
| `I + L` | 右上斜移 |

### 移动速度调节（按住 `Space` + 下列按键）

| 按键          | 功能                      |
| ------------- | ------------------------- |
| `Space + -` | 降低一档速度（最低 1 档） |
| `Space + +` | 提高一档速度（最高 6 档） |

> 基础速率初始为 12，每升一档 ×2，每降一档 ÷2（最低为 1）。切换挡位时播放对应数字的提示音。

### 鼠标按键

| 按键         | 功能                    |
| ------------ | ----------------------- |
| `A`或`;` | 鼠标左键（按下 / 松开） |
| `S`        | 鼠标中键（按下 / 松开） |
| `F`或`'` | 鼠标右键（按下 / 松开） |

### 鼠标滚轮

| 按键  | 功能     |
| ----- | -------- |
| `O` | 滚轮向上 |
| `H` | 滚轮向下 |

### 其他

| 按键  | 功能    |
| ----- | ------- |
| `U` | Home 键 |
| `P` | End 键  |

---

## 技术实现

### 核心依赖与 API

| 用途         | 实现方式                                                                   |
| ------------ | -------------------------------------------------------------------------- |
| 全局键盘监听 | `WindowsInput.Capture.Global.KeyboardAsync()`                            |
| 按键拦截     | `e.Next_Hook_Enabled = false`                                            |
| 鼠标坐标获取 | `user32.dll!GetCursorPos`                                                |
| 鼠标坐标设置 | `user32.dll!SetCursorPos`                                                |
| 滚轮模拟     | `user32.dll!mouse_event`（`MOUSEEVENTF_WHEEL`/`MOUSEEVENTF_HWHEEL`） |
| DPI 感知     | `SetProcessDPIAware`（避免多屏不同缩放下坐标错位）                       |
| 虚拟桌面边界 | `GetSystemMetrics(SM_X/Y/CXV/CYVIRTUALSCREEN)`                           |

### 多显示器坐标处理

早期版本使用 `Simulate.Events().MoveTo(x, y)`，该库内部以 `SM_CXSCREEN` 归一化坐标，导致 **副屏（尤其是位于主屏左侧/上方的副屏）坐标溢出，鼠标移动到第二屏后卡死不动** 。

当前版本已改为：

1. 调用 `SetProcessDPIAware()` 声明 DPI 感知；
2. 用 `GetCursorPos` 获取当前物理坐标；
3. 叠加增量后，用 **虚拟桌面边界** （`SM_XVIRTUALSCREEN` 等）做钳制；
4. 直接调用 `SetCursorPos(x, y)` 设置坐标，绕开库内部的归一化逻辑。

---

## 项目结构

采用整洁架构分层，各层职责单一、互相隔离：

```text
Key2Mouse/
├── Program.cs                      # 入口：DPI 感知、依赖装配、启动监听、打印手册
├── Key2Mouse.csproj                # net10.0-windows + WindowsInput 6.4.1 + 资源声明
├── 功能手册.txt                    # 启动时读取并打印到控制台
├── README.md
│
├── Native/                         # ← 平台互操作层
│   ├── NativeMethods.cs            #   所有 user32.dll P/Invoke 与常量
│   └── POINT.cs                    #   与 Win32 POINT 对齐的结构体
│
├── Audio/                          # ← 音频层
│   ├── EmbeddedBeepPlayer.cs       #   播放内嵌 wav 提示音
│   ├── 1.wav ~ 6.wav               #   六档速度提示音
│   ├── 开启键盘监听.wav
│   ├── 关闭键盘监听.wav
│   ├── 开启键盘模拟鼠标.wav
│   └── 关闭键盘模拟鼠标.wav
│
├── Mouse/                          # ← 鼠标动作层
│   ├── MouseController.cs          #   移动 / 滚动 / 调速 统一封装
│   └── MouseSpeedSettings.cs       #   速度状态 + 档位迁移规则
│
└── Input/                          # ← 输入业务层
    ├── KeyboardToMouseHandler.cs   #   键盘事件 → 鼠标动作（核心业务）
    └── KeyStateTracker.cs          #   按键状态跟踪（Alt/Space/I/J/K/L）
```



### 分层依赖方向

**text**

```
Program ──► Input ──► Mouse ──► Audio
   │           │         │
   └───────────┴─────────┴──► Native
```

* `Input` 只依赖 `Mouse` 的抽象接口；
* `Mouse` 依赖 `Audio` 与 `Native`；
* `Native` 是最底层，不依赖任何上层；
* `Program` 只负责装配与触发。

---

## 已知问题 / TODO

* **☑  **多显示器坐标卡死问题（已通过 `SetCursorPos` + 虚拟桌面边界钳制修复）
* **☑  **单文件"万能 Program.cs"重构为整洁架构多文件分层
* **☑  **斜向移动逻辑集中于 `KeyboardToMouseHandler.HandleDiagonal`，便于后续合并优化
* **□  **提示音文件依赖嵌入资源路径 `Key2Mouse.Audio.{name}.wav`，缺少时仅打印提示
* **□  **`功能手册.txt` 中"滚轮上/下"的描述与代码实际行为（H 向上、O 向下）待核实统一

---

## 更新日志

### v3.0 —— 架构重构

* 将单文件 `Program.cs` 拆分为整洁架构多目录（`Native` / `Audio` / `Mouse` / `Input`）
* 速度相关状态收拢到 `MouseSpeedSettings`
* 鼠标副作用统一由 `MouseController` 封装
* 键盘状态由 `KeyStateTracker` 独立管理
* 业务逻辑拆为 `KeyboardToMouseHandler` 中的多个语义化方法
* 命名空间由 `ConsoleApp1` 更名为 `Key2Mouse`

### v2.2

* 新增 `'` 键作为鼠标右键
* 完善多屏坐标处理（`SetCursorPos` + 虚拟桌面边界）
* 补充 `SetProcessDPIAware` DPI 感知

---

## 声明

本项目为个人效率工具，仅供学习与自用。使用全局键盘钩子可能被部分安全软件标记，请自行判断运行环境。
