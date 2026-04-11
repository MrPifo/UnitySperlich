# 🧰 UnitySperlich

A personal collection of modular Unity tools and utilities, distributed as individual UPM packages from a single monorepo.  
Each module lives in its own subfolder with its own `package.json` and `.asmdef` — install only what you need.

---

## 📦 Modules

| Module | Description |
|---|---|
| [Core](#-core) | Shared base utilities used across other modules |
| [Types](#-types) | Custom data types and structs |
| [Sequencer](#-sequencer) | UI animation sequencer powered by PrimeTween |
| [AudioManager](#-audiomanager) | Audio playback and management |
| [CodeScheduler](#-codescheduler) | Deferred and scheduled code execution |
| [EasingCurves](#-easingcurves) | Easing function library |
| [GameLoop](#-gameloop) | Centralized update/tick loop management |
| [GraphicsManager](#-graphicsmanager) | Graphics and rendering settings |
| [Input](#-input) | Input abstraction layer |
| [Json](#-json) | JSON serialization helpers |
| [Logger](#-logger) | Structured debug logging |
| [Monitoring](#-monitoring) | Runtime stats and performance monitoring |
| [PauseManager](#-pausemanager) | Global pause state handling |
| [PrefabManager](#-prefabmanager) | Prefab pooling and instantiation |

---

## 🔧 Installation

Each module can be installed independently via the Unity Package Manager using a Git URL with the `?path=` parameter.

In Unity, go to **Window → Package Manager → + → Add package from git URL** and enter:

```
https://github.com/MrPifo/UnitySperlich.git?path=/ModuleName
```

Replace `ModuleName` with the folder name of the desired module (e.g. `Core`, `Sequencer`).

---

## 📁 Core

**`https://github.com/MrPifo/UnitySperlich.git?path=/Core`**

Shared base utilities, extension methods, and helper classes used by other modules in this collection.

---

## 📁 Types

**`https://github.com/MrPifo/UnitySperlich.git?path=/Types`**

Custom data types, enums, and structs for use across Unity projects.

---

## 📁 Sequencer

**`https://github.com/MrPifo/UnitySperlich.git?path=/Sequencer`**

A UI animation sequencer built on top of [PrimeTween](https://github.com/KyryloKuzyk/PrimeTween) and Unity UIToolkit.  
Supports Fade, Scale, Slide, Rotate, Bounce, TypeWriter, ColorTint, and more — fully configurable via a custom inspector.

> ⚠️ **Requires** [PrimeTween](https://assetstore.unity.com/packages/tools/animation/primetween-high-performance-animations-and-sequences-252960) (available on the Asset Store).  
> Optional dependency is handled via `Version Defines` in the `.asmdef`.

---

## 📁 AudioManager

**`https://github.com/MrPifo/UnitySperlich.git?path=/AudioManager`**

A centralized audio manager for handling sound effects and music playback.

---

## 📁 CodeScheduler

**`https://github.com/MrPifo/UnitySperlich.git?path=/CodeScheduler`**

Utilities for scheduling and deferring code execution across frames or time intervals.

---

## 📁 EasingCurves

**`https://github.com/MrPifo/UnitySperlich.git?path=/EasingCurves`**

A library of standard easing functions (Sine, Cubic, Elastic, Bounce, etc.) usable without any tween dependency.

---

## 📁 GameLoop

**`https://github.com/MrPifo/UnitySperlich.git?path=/GameLoop`**

Centralized tick and update loop management, decoupled from MonoBehaviour inheritance.

---

## 📁 GraphicsManager

**`https://github.com/MrPifo/UnitySperlich.git?path=/GraphicsManager`**

Runtime graphics and rendering settings manager (resolution, quality, post-processing toggles).

---

## 📁 Input

**`https://github.com/MrPifo/UnitySperlich.git?path=/Input`**

A thin abstraction layer over Rewired's input system.

---

## 📁 Json

**`https://github.com/MrPifo/UnitySperlich.git?path=/Json`**

JSON serialization and deserialization helpers, including support for polymorphic types.

---

## 📁 Logger

**`https://github.com/MrPifo/UnitySperlich.git?path=/Logger`**

A structured debug logger with log levels, categories, and optional editor coloring.

---

## 📁 Monitoring

**`https://github.com/MrPifo/UnitySperlich.git?path=/Monitoring`**

Runtime monitoring utilities for FPS, memory, and custom performance metrics.

---

## 📁 PauseManager

**`https://github.com/MrPifo/UnitySperlich.git?path=/PauseManager`**

Global pause state management with event callbacks and `Time.timeScale` control.

---

## 📁 PrefabManager

**`https://github.com/MrPifo/UnitySperlich.git?path=/PrefabManager`**

Prefab pooling and runtime instantiation utilities to reduce GC overhead.

---

*Made by [MrPifo](https://github.com/MrPifo)*
