# AGENTS.md â€” Whack-A-Meteor

Unity 2022.3.29f1 (C#) 2D space shooter: fly the ship and swing a hammer cursor at splitting meteors. Kenney Space Shooter Redux assets (CC0). New Input System (`com.unity.inputsystem`) with legacy fallbacks.

## Build and test

Everything goes through the Unity Editor (no build script). Tests: Window â†’ General â†’ Test Runner â†’ Run All.
Batch build (Windows):
`"C:\Program Files\Unity\Hub\Editor\2022.3.29f1\Editor\Unity.exe" -batchmode -quit -projectPath <repo> -buildTarget StandaloneWindows64 -buildPath Build/win64`

## Conventions â€” easy to get wrong

- Gameplay HUD, pause and game-over overlays use legacy `OnGUI`/`GUIStyle` in `GameManager`, not uGUI/Canvas. Game over is an in-scene overlay; the separate GameOver/Credits scenes are not in the build order.
- Get input only via `InputManager.GetOrCreateInstance()`. Every input path checks `inputManager != null` and keeps an `else` fallback to legacy `Input.*`.
- Collisions are tag-based (`BigBrownMeteor`, `TinyGreyMeteor`, ...). A new destructible type needs: a tag in `ProjectSettings/TagManager.asset`, a case in `GameConstants.GetScoreByTag()`, and the relevant `OnTriggerEnter2D`/`OnCollisionEnter2D` handlers.
- Meteor prefab arrays are set in the Inspector on `MeteorSpawner`; `MeteorSplit.Awake()` copies them when its own arrays are empty, so runtime splits keep their prefabs. Keep that path working.
- Meteor-meteor splits have a 0.5 s spawn grace period to prevent self-collision â€” keep it when touching `MeteorSplit`.
- Hammer hit = 2Ã— base score + camera shake (`CameraShake.Instance.Shake()`).