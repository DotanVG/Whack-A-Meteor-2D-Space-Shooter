# AGENTS.md

This file provides guidance to AI coding assistants (Claude Code, Cursor, Warp, Copilot, etc.) when working with code in this repository.

## Project Overview

**Whack-A-Meteor** is a 2D space shooter built in Unity 2022.3.29f1 (C#). The player pilots a ship and controls a hammer cursor to destroy incoming meteors. Assets are from Kenney's Space Shooter Redux (CC0).

## Unity Version & Key Packages

- Unity Editor: **2022.3.29f1**
- Input System: `com.unity.inputsystem` 1.7.0 (new Input System)
- 2D Feature bundle: `com.unity.feature.2d` 2.0.0
- Test Framework: `com.unity.test-framework` 1.1.33

## Building & Running

All build and test operations go through the Unity Editor — there is no standalone CLI build script.

**Open the project:** Unity Hub → Add → select the `Whack-A-Meteor-2D-Space-Shooter/` directory, then open with Unity 2022.3.29f1.

**Play in editor:** Open `Assets/Scenes/SplashScreen.unity` (or `Game.unity`), press the Play button.

**CLI batch build (Windows):**
```
"C:\Program Files\Unity\Hub\Editor\2022.3.29f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\path\to\Whack-A-Meteor-2D-Space-Shooter" -buildTarget StandaloneWindows64 -buildPath "Build/win64"
```

**Run tests:** Unity Editor → Window → General → Test Runner → Run All (uses `com.unity.test-framework`).

## Scene Flow

```
SplashScreen → MainMenu → Game
                       → Settings → MainMenu
```

Scenes are in `Assets/Scenes/`. The build order (index 0–3) is: SplashScreen, MainMenu, Game, Settings. A GameOver scene and Credits scene exist in the project but are not in the current build order; the in-game game over state is handled via `GameManager.OnGUI()` directly in the Game scene.

## Architecture

### Singleton Pattern

Three singletons control global state:

- **`GameManager`** (`Assets/Scripts/GameManager.cs`) — owns `Score`, `Lives`, countdown timer, pause/resume, and game-over state. Also handles the legacy IMGUI HUD (score digits, life icons). Accessed via `GameManager.Instance`.
- **`InputManager`** (`Assets/Scripts/Input/InputManager.cs`) — centralised input layer over Unity's new Input System. Supports keyboard/mouse and any gamepad simultaneously, detecting the active device per-frame. Persists across scenes via `DontDestroyOnLoad`. **All scripts obtain it with `InputManager.GetOrCreateInstance()`** (auto-creates the object if it doesn't exist). Every input query has a legacy `Input.*` fallback.
- **`CameraShake`** (`Assets/Scripts/Utilities/CameraShake.cs`) — coroutine-based screen shake singleton; called via `CameraShake.Instance.Shake()`.

### Meteor Hierarchy & Tags

Meteors come in two colour variants (Brown, Grey) and four sizes: Big → Medium → Small → Tiny. Unity **tags** are used for collision identification (e.g. `BigBrownMeteor`, `TinyGreyMeteor`). Use `GameConstants.GetScoreByTag(tag)` to get a meteor's score value.

When a meteor is hit by a projectile or hammer, `MeteorSplit` spawns 2–3 smaller meteors inheriting direction+speed (with ±30° offset). Tiny meteors are the terminal size and don't split further. Meteors also split on **meteor-meteor collision**: `MeteorSplit.OnCollisionEnter2D` triggers a split when relative speed exceeds the threshold (default 0.5), with a 0.5s spawn-delay grace period to prevent immediate self-collision. Meteor-player collisions are handled by `PlayerHealth`, which triggers `GameManager.LoseLife()` and a 3-second invincibility coroutine (flashing sprite).

Scoring: projectile hit = base score; hammer hit = **2× base score** + camera shake.

### Dual Combat System

| Mechanic | Script | Keyboard/Mouse | Gamepad |
|----------|--------|----------------|---------|
| Ship rotate | `PlayerController` | A / D | Left stick X / D-pad left/right |
| Ship forward/brake | `PlayerController` | W / S | Left stick Y / D-pad up / A button |
| Shoot | `PlayerController` | Space | LT or X button |
| Boost (3×, 1.5s, 5s cooldown) | `PlayerController` | Left Shift | LB or LS-click |
| Hammer swing | `HammerCursorController` | Mouse LMB | RT |
| Hammer aim | `HammerCursorController` | Mouse cursor | Right stick (relative movement) |
| Screen wrap | `ScreenWrapper` | — | — |

`HammerCursorController` uses `Physics2D.OverlapCircleAll` at the peak of the 3-phase swing animation (backswing → forward → return) to detect hits. The hammer position is driven by `InputManager.GetHammerPosition()`, which unifies mouse and gamepad right-stick into a single tracked screen coordinate.

### Game State & Controls

**Pause:** P or Escape (keyboard) / Start (gamepad) — toggles pause. While paused: Q (keyboard) / B button (gamepad) returns to Main Menu.

**Game Over overlay** (handled in-scene by `GameManager.OnGUI`):
- R (keyboard) / Y button (gamepad) — Restart
- Enter or Escape (keyboard) / A or B button (gamepad) — Main Menu
- Auto-returns to Main Menu after 3 seconds

### Score Constants

Defined in `Assets/Scripts/Utilities/GameConstants.cs`:
- Big=5, Medium=10, Small=15, Tiny=20 pts (×2 for hammer hit)
- Enemy=25 pts
- Starting lives=3, invincibility duration=3s

### Physics Layers

| Layer | Used by |
|-------|---------|
| Player | Player ship |
| Enemies | Enemy ships |
| Meteors | All meteor prefabs (set in `MeteorSpawner`) |

### Controllers (UI / Scenes)

Each non-gameplay scene has a simple `OnGUI`-based controller:
- `MainMenuController` — New Game / Settings / Quit (gamepad D-pad navigation supported)
- `SettingsController` — Placeholder slider
- `GameOverController` — Shows final score from `GameManager.Instance.Score`; Enter/A or B → Main Menu (used in the separate GameOver scene, not the in-game overlay)
- `SplashScreenController` — Auto-loads MainMenu after `delay` seconds

## Key Conventions

- **No Unity UI (Canvas/uGUI) in gameplay** — the HUD, pause overlay, and game-over screen all use legacy `OnGUI`/`GUIStyle` calls in `GameManager`.
- **Input dual-path:** Always check `inputManager != null` before using the new Input System; provide an `else` fallback using legacy `Input.*`.
- **Meteor prefab arrays** are assigned in the Inspector on `MeteorSpawner`. `MeteorSplit.Awake()` copies them from the spawner if its own arrays are empty — this means newly runtime-spawned split meteors still have the correct prefab references.
- **Tag-based collision:** Collision logic compares `gameObject.tag` strings. When adding new destructible types, register them in `ProjectSettings/TagManager.asset` and add cases to `GameConstants.GetScoreByTag()` and the relevant `OnTriggerEnter2D`/`OnCollisionEnter2D` handlers.
