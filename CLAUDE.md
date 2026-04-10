# Whack-A-Meteor

Unity 2022.3.29f1 (C#) 2D space shooter. Player pilots ship + swings hammer cursor to destroy meteors. Assets: Kenney Space Shooter Redux (CC0).

## Build & Run

- Open: Unity Hub → Add repo dir → Unity 2022.3.29f1
- Play: Open `Assets/Scenes/SplashScreen.unity` → Press Play
- Tests: Window → General → Test Runner → Run All
- CLI batch build:
  ```
  "C:\Program Files\Unity\Hub\Editor\2022.3.29f1\Editor\Unity.exe" -batchmode -quit -projectPath "<path>" -buildTarget StandaloneWindows64 -buildPath "Build/win64"
  ```

## Scene Flow
`SplashScreen(0) → MainMenu(1) → Game(2) / Settings(3) → MainMenu`
GameOver/Credits scenes exist but are NOT in build order; game-over handled by `GameManager.OnGUI()` inside Game scene.

## Architecture

**Singletons:**
- `GameManager` (`Assets/Scripts/GameManager.cs`) — Score, Lives, timer, pause, game-over, IMGUI HUD. Access: `GameManager.Instance`
- `InputManager` (`Assets/Scripts/Input/InputManager.cs`) — New Input System + legacy fallbacks, `DontDestroyOnLoad`. Always use `InputManager.GetOrCreateInstance()`
- `CameraShake` (`Assets/Scripts/Utilities/CameraShake.cs`) — `CameraShake.Instance.Shake()`

**Meteor System:** Two colours (Brown/Grey), four sizes Big→Medium→Small→Tiny. Tags e.g. `BigBrownMeteor`. Score lookup: `GameConstants.GetScoreByTag(tag)`. `MeteorSplit` spawns 2–3 smaller on hit (±30° offset); Tiny is terminal. Meteor-meteor split on relative speed > 0.5 with 0.5s spawn grace. Scoring: projectile = base; hammer = 2× base + camera shake.

**Input:**
| Mechanic | Script | KB/Mouse | Gamepad |
|---|---|---|---|
| Rotate | `PlayerController` | A/D | Left stick X / D-pad |
| Move | `PlayerController` | W/S | Left stick Y / D-pad / A |
| Shoot | `PlayerController` | Space | LT or X |
| Boost (3×, 1.5s, 5s CD) | `PlayerController` | Shift | LB/LS-click |
| Hammer swing | `HammerCursorController` | LMB | RT |
| Hammer aim | `HammerCursorController` | Mouse | Right stick |

`HammerCursorController`: `Physics2D.OverlapCircleAll` at swing peak (backswing→forward→return). Position via `InputManager.GetHammerPosition()`.

**Game State:**
- Pause: P/Esc/Start. While paused: Q/B → Main Menu
- Game Over (`GameManager.OnGUI`): R/Y=Restart, Enter/Esc/A/B=Main Menu, auto-return 3s

**Constants** (`Assets/Scripts/Utilities/GameConstants.cs`): Big=5, Med=10, Sm=15, Tiny=20 (×2 hammer), Enemy=25. Lives=3, invincibility=3s.

**Physics Layers:** Player, Enemies, Meteors.

## Conventions
- HUD/pause/game-over: legacy `OnGUI`/`GUIStyle` only — no Canvas/uGUI
- Input: always check `inputManager != null`, provide `else Input.*` fallback
- Meteor prefabs: assigned in Inspector on `MeteorSpawner`; `MeteorSplit.Awake()` copies from spawner if own arrays are empty
- New destructibles: add tag in `TagManager.asset`, `GameConstants.GetScoreByTag()`, and relevant `OnTriggerEnter2D`/`OnCollisionEnter2D` handlers
