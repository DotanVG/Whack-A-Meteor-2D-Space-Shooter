# Technical Preferences

## Engine & Language

- **Engine**: Unity 2022.3.29f1
- **Language**: C# (.NET Standard 2.1)
- **Rendering**: Built-in Render Pipeline (2D)
- **Physics**: Physics2D (Unity built-in)

## Input & Platform

- **Target Platforms**: PC (Windows primary), StandaloneWindows64
- **Input Methods**: Keyboard/Mouse + Gamepad
- **Primary Input**: Keyboard/Mouse (KB=movement/shoot/boost, Mouse=hammer aim/swing)
- **Gamepad Support**: Full (Unity New Input System + legacy fallbacks)
- **Touch Support**: None
- **Platform Notes**: Always check `inputManager != null` and provide `else Input.*` fallback

## Naming Conventions

- **Classes**: PascalCase (e.g. `GameManager`, `MeteorSplit`)
- **Variables**: camelCase private, PascalCase public properties
- **Signals/Events**: PascalCase (C# events/delegates)
- **Files**: PascalCase matching the primary class name
- **Scenes/Prefabs**: PascalCase (e.g. `SplashScreen`, `BigBrownMeteor`)
- **Constants**: SCREAMING_SNAKE_CASE for true constants, PascalCase for static readonly
- **Tags**: PascalCase size+colour+type (e.g. `BigBrownMeteor`, `TinyGreyMeteor`)

## Performance Budgets

- **Target Framerate**: 60 fps
- **Frame Budget**: 16.6ms
- **Draw Calls**: Minimize — use sprite atlasing; no Canvas/uGUI (IMGUI only for HUD)
- **Memory Ceiling**: Not formally set — avoid runtime allocations in Update/physics callbacks

## Testing

- **Framework**: Unity Test Runner (EditMode + PlayMode, NUnit)
- **Minimum Coverage**: Critical gameplay systems (scoring, meteor split logic, constants)
- **Required Tests**: GameConstants scoring lookups, MeteorSplit spawn math, GameManager state transitions

## Forbidden Patterns

- Canvas/uGUI for HUD, pause overlay, or game-over screen — use `OnGUI`/`GUIStyle` only
- Accessing `InputManager` without null-check and legacy fallback
- Hardcoding score values outside `GameConstants.GetScoreByTag()`
- Skipping tag registration in `TagManager.asset` for new destructibles

## Allowed Libraries / Addons

- Unity New Input System (com.unity.inputsystem)
- Unity MCP Bridge (com.ivanmurzak.unity.mcp) — dev tooling only
- Kenney Space Shooter Redux assets (CC0)

## Architecture Decisions Log

- Singletons for GameManager, InputManager, CameraShake — access via `.Instance` / `GetOrCreateInstance()`
- IMGUI-only HUD: avoids Canvas overhead for a simple 2D arcade game
- New Input System + legacy fallbacks: ensures gamepad and keyboard both work reliably

## Engine Specialists

- **Primary**: `unity-specialist`
- **Language/Code Specialist**: `unity-specialist` (C#)
- **Shader Specialist**: `unity-shader-specialist`
- **UI Specialist**: `unity-ui-specialist` (note: IMGUI only — no uGUI)
- **Additional Specialists**: `unity-addressables-specialist` (if asset bundles added), `unity-dots-specialist` (not currently used)
- **Routing Notes**: Default to `unity-specialist` for scene/prefab/MonoBehaviour work; `gameplay-programmer` for pure game logic

### File Extension Routing

| File Extension / Type | Specialist to Spawn |
|-----------------------|---------------------|
| `.cs` (MonoBehaviour, ScriptableObject) | `unity-specialist` |
| `.cs` (pure game logic, no Unity API) | `gameplay-programmer` |
| `.shader` / `.hlsl` | `unity-shader-specialist` |
| `.cs` (IMGUI / OnGUI) | `unity-ui-specialist` |
| `.unity` scene files | `unity-specialist` |
| `.prefab` files | `unity-specialist` |
| Native extension / plugin files | `unity-specialist` |
| General architecture review | `technical-director` |
