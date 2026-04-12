# Sperlich.Sequencer
A component-based UI animation sequencer for Unity, built on top of **PrimeTween** and **UI Toolkit**.  
Define, configure, and chain UI animations entirely through the Inspector — no code required.

---

## Requirements

| Dependency | Version |
|---|---|
| Unity | 6.0+ |
| [PrimeTween](https://assetstore.unity.com/packages/tools/animation/primetween-high-performance-animations-and-sequences-252960) | Latest |
| UI Toolkit | Included with Unity |
| TextMeshPro | Included with Unity |

> PrimeTween is detected automatically via a Version Define in the `.asmdef`. The package will not compile without it.

---

## Installation

```
https://github.com/MrPifo/UnitySperlich.git?path=/Sequencer
```

Add via **Window → Package Manager → + → Add package from git URL**.

---

## Screenshots

<!-- Replace with your own screenshots -->
| Inspector Overview | Sequence In Action | Step Configuration |
|---|---|---|
| ![Inspector](screenshot_inspector.png) | ![Preview](screenshot_preview.gif) | ![Step](screenshot_step.png) |

---

## Overview

`AnimSequencer` is a MonoBehaviour that manages one or more named `AnimSequence` instances on a GameObject.  
Each sequence holds an ordered list of steps, each defining an animation type, target, duration, easing, and optional delay.

Sequences can play automatically based on pointer or lifecycle events, or be driven entirely from code.  
Steps within a sequence can run **sequentially** or **in parallel** depending on their `StepMode`.

---

## Animation Types

Listed in enum order (`AnimType`):

| Type | Description |
|---|---|
| `Fade` | Animates the `CanvasGroup` alpha or a target's opacity from one value to another |
| `Scale` | Tweens the Transform's local scale from a start to an end value |
| `Slide` | Moves the element along X and/or Y using `anchoredPosition` |
| `Rotate` | Rotates the Transform to/from a given Z angle |
| `PunchRotate` | Applies a rotation punch that oscillates and springs back to origin |
| `Bounce` | Punches scale outward and returns — useful for pop-in feedback |
| `ColorTint` | Animates the color of an `Image` or `TMP_Text` component |
| `PunchScale` | Applies a scale punch that overshoots and springs back |
| `TypeWriter` | Reveals a `TMP_Text` string character by character over the duration |
| `TextCounter` | Animates a numeric `TMP_Text` label between two float values |
| `SetTransform` | Instantly sets `LocalPosition`, `LocalRotation`, or `LocalScale` without animation |
| `Wait` | Pauses the sequence for a given number of seconds or frames |
| `SetText` | Instantly assigns a string value to a `TMP_Text` component |
| `SetColor` | Instantly sets the color on an `Image` or `TMP_Text` |
| `SetActive` | Calls `SetActive(true/false)` on a target GameObject |
| `Trigger` | Fires a named sequence on another `AnimSequencer` component |
| `Event` | Invokes a `UnityEvent` at this point in the sequence |
| `SetSprite` | Instantly swaps the sprite on a `SpriteRenderer` component |
| `FadeSpriteColor` | Animates the color of a `SpriteRenderer` between two values |
| `SetImage` | Instantly swaps the sprite on a UI `Image` component |
| `Anchor` | Tweens the `anchoredPosition` of a `RectTransform` to a target position |
| `Repeat` | Repeats the preceding step or group a given number of times |
| `WaitUntil` | Pauses the sequence until a boolean condition becomes true (polling-based) |
| `SetFade` | Instantly sets the `CanvasGroup` alpha to a specific value without animation |

---

## Trigger Types

Each `AnimSequence` has a trigger that determines when it auto-plays:

| Trigger | Description |
|---|---|
| `OnEnable` | Plays when the GameObject is enabled |
| `OnDisable` | Plays when the GameObject is disabled — keeps it alive until the sequence completes |
| `OnClick` | Fires on a pointer click event |
| `OnPointerEnter` | Fires when the pointer enters the element |
| `OnPointerExit` | Fires when the pointer leaves the element |
| `OnPointerDown` | Fires on pointer press |
| `OnPointerUp` | Fires on pointer release |
| `Manual` | Only plays when explicitly triggered from code |
| `OnBecameInteractable` | Fires when a linked `Selectable` becomes interactable |
| `OnBecameNonInteractable` | Fires when a linked `Selectable` becomes non-interactable |

---

## Step Modes

| Mode | Description |
|---|---|
| `Sequential` | Each step waits for the previous to finish before starting |
| `Parallel` | Step starts at the same time as the previous one |

---

## Code API

### Playing Sequences

```csharp
// Play all sequences matching a trigger
sequencer.Play(TriggerType.OnEnable);

// Play a specific sequence by label
sequencer.PlaySequence("intro");

// Pause / Resume / Stop
sequencer.Pause("intro");
sequencer.Resume("intro");
sequencer.StopByLabel("intro");
```

### Creating Sequences at Runtime

```csharp
var seq = sequencer.CreateSequence("popup", TriggerType.Manual);
```

### Appending Steps

Steps are added via typed `AnimConfig` structs:

```csharp
sequencer.AppendStep("popup", new FadeConfig { from = 0f, to = 1f, duration = 0.3f });
sequencer.AppendStep("popup", new ScaleConfig { from = Vector3.zero, to = Vector3.one, duration = 0.4f });
sequencer.AppendStep("popup", new WaitConfig { seconds = 0.5f });
```

### Getting & Modifying Steps by Tag

```csharp
// Read current config
var config = sequencer.GetConfig<FadeConfig>("popup", "fade-in");

// Overwrite config
sequencer.SetConfig("popup", "fade-in", new FadeConfig { from = 0f, to = 1f, duration = 0.2f });
```

### Fluent Extension API

`AnimSequence` supports fluent chaining via extension methods on both the sequence and individual steps:

```csharp
sequencer.GetSequence("popup")
    .SetDuration("fade-in", 0.5f)
    .SetDelay("fade-in", 0.1f)
    .SetRelative("slide", true)
    .Play();
```

### WaitUntil with Lambda

```csharp
// Assign a condition — the sequence resumes as soon as it returns true
sequencer.GetSequence("loading")
    .SetWaitCondition("wait-data", () => dataIsReady)
    .Play();

// Or resolve it manually
sequencer.GetSequence("loading").SetWaitReady("wait-data");
```

---

## Notes

- `OnDisable` sequences keep the GameObject alive until complete, then deactivate it cleanly.
- All durations use `InvariantCulture` formatting for locale safety.
- The Inspector is built with Unity UI Toolkit for a clean, performant editor layout.
- Steps are serialized as typed config structs, keeping data explicit and Inspector-friendly.
