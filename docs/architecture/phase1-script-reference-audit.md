# Phase 1 script reference audit

Snapshot: supplied feature/unity-mcp-1.0 content (HEAD efbcdb6), before cleanup. References are snapshot-relative `path:line`; executable C#, documentation/diagnostic strings and comments are distinguished.

## Scope and method

- Scanned all 50 tracked scene/prefab files under Assets (8 scenes, 42 prefabs), including scenes outside the build order.
- Read all 66 tracked C# source files under Assets from efbcdb6. Matched class identifiers outside comments and strings, then separately inspected all string hits for string-based AddComponent/GetComponent/type loading. Comment-only occurrences are listed separately.
- Searched every other local Assets file as bytes for script GUIDs, excluding only its own .cs.meta. Also checked serialized .asset/.anim/.controller/.json files for class-name strings. No additional references were found for any removal candidate.
- None of the audited scripts lives under Resources. Game code has no reflection/Activator/Assembly type construction, string-based component construction, SendMessage or BroadcastMessage loader. Existing Resources.Load sites load cursor Texture2D and balance TextAsset only (MenuCursor.cs:17,21; CSVBalanceParser.cs:31). Plugin-generated Assets/Plugins is outside the game system; no game-type dynamic loader was found in game source.
- Unity lifecycle methods inside an unreferenced script are not external references. Own source-file occurrences are excluded. Tooltip/Header/log strings and tag strings are recorded as such; they do not instantiate component classes.
- No live scripts are moved or renamed. No serialized attachment plus no executable external reference is required for removal; merely sounding like an obsolete duplicate is insufficient.

## Decisions

| Script | GUID | Scene/prefab references | External executable code references | Decision |
|---|---|---|---|---|
| `Assets/Scripts/Enemy/BossController.cs` | `8513a4b00ed78934ab8e9fc3380df75d` | `Assets/Prefabs/Enemies/Boss.prefab:245` | `Assets/Scripts/Enemy/EnemyShooter.cs:21`<br>`Assets/Scripts/GameLoop/EnemyHealth.cs:65` | KEEP: serialized attachment |
| `Assets/Scripts/Enemy/EnemyActiveTracker.cs` | `2bf37c47b60cf4947ac9ff0f41ef34e4` | `Assets/Prefabs/Enemies/Boss.prefab:233`<br>`Assets/Prefabs/Enemies/Enemy_Black.prefab:243`<br>`Assets/Prefabs/Enemies/Enemy_Blue.prefab:243`<br>`Assets/Prefabs/Enemies/Enemy_Green.prefab:243`<br>`Assets/Prefabs/Enemies/Enemy_Red.prefab:229` | none | KEEP: serialized attachment |
| `Assets/Scripts/Enemy/EnemyCombatAI.cs` | `26a0964afb52d344e92140534af1e8a9` | `Assets/Prefabs/Enemies/Enemy_Black.prefab:255`<br>`Assets/Prefabs/Enemies/Enemy_Blue.prefab:255`<br>`Assets/Prefabs/Enemies/Enemy_Green.prefab:255`<br>`Assets/Prefabs/Enemies/Enemy_Red.prefab:255` | none | KEEP: serialized attachment |
| `Assets/Scripts/Enemy/EnemyController.cs` | `bf57a609f54cb4949b4d4b4775a3c0af` | `Assets/Prefabs/Enemies/Boss.prefab:192`<br>`Assets/Prefabs/Enemies/Enemy_Black.prefab:202`<br>`Assets/Prefabs/Enemies/Enemy_Blue.prefab:202`<br>`Assets/Prefabs/Enemies/Enemy_Green.prefab:202`<br>`Assets/Prefabs/Enemies/Enemy_Red.prefab:188` | `Assets/Scripts/Enemy/BossController.cs:20`<br>`Assets/Scripts/GameLoop/AutoShooter.cs:191` | KEEP: serialized attachment |
| `Assets/Scripts/Enemy/EnemyFaction.cs` | `c340dd7617877b14c81143fb01cc3ccc` | `Assets/Prefabs/Enemies/Boss.prefab:220`<br>`Assets/Prefabs/Enemies/Enemy_Black.prefab:230`<br>`Assets/Prefabs/Enemies/Enemy_Blue.prefab:230`<br>`Assets/Prefabs/Enemies/Enemy_Green.prefab:230`<br>`Assets/Prefabs/Enemies/Enemy_Red.prefab:216` | `Assets/Scripts/Enemy/EnemyCombatAI.cs:14`<br>`Assets/Scripts/Enemy/EnemyCombatAI.cs:19`<br>`Assets/Scripts/Enemy/EnemyCombatAI.cs:26`<br>`Assets/Scripts/Enemy/EnemyCombatAI.cs:64`<br>`Assets/Scripts/Enemy/EnemyController.cs:9`<br>`Assets/Scripts/GameLoop/EnemyHealth.cs:25`<br>`Assets/Scripts/GameLoop/EnemyHealth.cs:62` | KEEP: serialized attachment |
| `Assets/Scripts/Enemy/EnemyProjectile.cs` | `97e9469d991da9d49b23e0ea2b1e8a82` | `Assets/Prefabs/Enemies/BossProjectile.prefab:171`<br>`Assets/Prefabs/Enemies/EnemyProjectile.prefab:171`<br>`Assets/Prefabs/Enemies/EnemyProjectile_Green.prefab:171` | none | KEEP: serialized attachment |
| `Assets/Scripts/Enemy/EnemyShooter.cs` | `952f3517a4acfe843892d815f22fe2de` | `Assets/Prefabs/Enemies/Boss.prefab:257`<br>`Assets/Prefabs/Enemies/Enemy_Red.prefab:241` | none | KEEP: serialized attachment |
| `Assets/Scripts/Enemy/EnemySpawner.cs` | `baf51e79327910a4b87ffa68e595afd8` | `Assets/Scenes/Game.unity:470` | `Assets/Scripts/Enemy/EnemyActiveTracker.cs:9`<br>`Assets/Scripts/Enemy/EnemyActiveTracker.cs:10`<br>`Assets/Scripts/Enemy/SpawnDirector.cs:17` | KEEP: serialized attachment |
| `Assets/Scripts/Enemy/SpawnDirector.cs` | `0fde36d5a8816a04f9af2493869b859d` | `Assets/Scenes/Game.unity:151` | `Assets/Scripts/GameLoop/WaveManager.cs:27`<br>`Assets/Scripts/GameLoop/WaveManager.cs:57` | KEEP: serialized attachment |
| `Assets/Scripts/GameLoop/AutoShooter.cs` | `a6f8225c5f75328498558a89a76fd911` | `Assets/Scenes/Game.unity:909` | none | KEEP: serialized attachment |
| `Assets/Scripts/GameLoop/Bullet.cs` | `791b37aa739b3444bbe1fd16f4b201f3` | none | none | REMOVE .cs and .meta: no serialized attachment or executable external reference |
| `Assets/Scripts/GameLoop/CameraLook.cs` | `e88ef08b401b19943906623098ee9318` | none | none | REMOVE .cs and .meta: no serialized attachment or executable external reference |
| `Assets/Scripts/GameLoop/CustomCursor.cs` | `86b00ce6442d6574ab397800698b8f59` | none | none | REMOVE .cs and .meta: no serialized attachment or executable external reference |
| `Assets/Scripts/GameLoop/EnemyHealth.cs` | `aa9df5092addaf545b2bd1f75d0601f6` | `Assets/Prefabs/Enemies/Boss.prefab:205`<br>`Assets/Prefabs/Enemies/Enemy_Black.prefab:215`<br>`Assets/Prefabs/Enemies/Enemy_Blue.prefab:215`<br>`Assets/Prefabs/Enemies/Enemy_Green.prefab:215`<br>`Assets/Prefabs/Enemies/Enemy_Red.prefab:201` | `Assets/Scripts/Enemy/BossController.cs:17`<br>`Assets/Scripts/Enemy/EnemyController.cs:27`<br>`Assets/Scripts/GameLoop/Bullet.cs:25`<br>`Assets/Scripts/Player/HammerCursorController.cs:317`<br>`Assets/Scripts/Player/HammerCursorController.cs:336`<br>`Assets/Scripts/Player/ProjectileController.cs:28`<br>`Assets/Scripts/VFX/LightningChain.cs:78` | KEEP: serialized attachment |
| `Assets/Scripts/GameLoop/MeteorMover.cs` | `f3c38de7ce458aa4ba3538e622b6aac5` | none | `Assets/Scripts/GameLoop/AutoShooter.cs:181`<br>`Assets/Scripts/GameLoop/MeteorSpawnerGL.cs:78` | KEEP: queried by live AutoShooter even though no serialized attachment |
| `Assets/Scripts/GameLoop/MeteorSpawnerGL.cs` | `b359f5ea816ac0c4d9482b0f01226de2` | none | none | REMOVE .cs and .meta: no serialized attachment or executable external reference |
| `Assets/Scripts/GameLoop/MoveObject.cs` | `133e7beee13eab34a864426c4ac750a4` | none | none | REMOVE .cs and .meta: no serialized attachment or executable external reference |
| `Assets/Scripts/GameLoop/PlayerHealthGL.cs` | `4c663b24cc0426949baed85e7ac2d40e` | none | none | REMOVE .cs and .meta: no serialized attachment or executable external reference |
| `Assets/Scripts/GameLoop/PlayerMovement.cs` | `2608ebb05948e9c47b956840ba82b9f7` | none | none | REMOVE .cs and .meta: no serialized attachment or executable external reference |
| `Assets/Scripts/GameLoop/ScoreManager.cs` | `878c83dea763e3041a8480358c39b590` | none | `Assets/Scripts/GameLoop/PlayerHealthGL.cs:58` | UNSURE / LEFT IN PLACE: only executable consumer is dead PlayerHealthGL; conservative dead-chain retention |
| `Assets/Scripts/GameLoop/UIManager.cs` | `d0ddfdf984d9c0742929cea52e3306ed` | none | `Assets/Scripts/GameLoop/PlayerHealthGL.cs:28`<br>`Assets/Scripts/GameLoop/PlayerHealthGL.cs:46`<br>`Assets/Scripts/GameLoop/PlayerHealthGL.cs:59`<br>`Assets/Scripts/GameLoop/ScoreManager.cs:30`<br>`Assets/Scripts/GameLoop/ScoreManager.cs:37`<br>`Assets/Scripts/GameLoop/WaveManager.cs:74` | KEEP: called by live WaveManager even though no serialized attachment |
| `Assets/Scripts/GameLoop/WaveManager.cs` | `7bce0cd6cd6a66646a7b44aa2b7c88dc` | `Assets/Scenes/Game.unity:1206` | `Assets/Scripts/Enemy/BossController.cs:34`<br>`Assets/Scripts/GameManager.cs:104`<br>`Assets/Scripts/GameManager.cs:241`<br>`Assets/Scripts/GameManager.cs:267` | KEEP: serialized attachment |
| `Assets/Scripts/Meteor/MeteorMovement.cs` | `5dc9bee990417704399ee3c0ea4ebabc` | `Assets/Prefabs/Meteors/BigBrownMeteor_1.prefab:162`<br>`Assets/Prefabs/Meteors/MediumBrownMeteor_1.prefab:162`<br>`Assets/Prefabs/Meteors/MediumBrownMeteor_2.prefab:162`<br>`Assets/Prefabs/Meteors/MediumGreyMeteor_1.prefab:162`<br>`Assets/Prefabs/Meteors/MediumGreyMeteor_2.prefab:162`<br>`Assets/Prefabs/Meteors/SmallBrownMeteor_1.prefab:162`<br>`Assets/Prefabs/Meteors/SmallBrownMeteor_2.prefab:162`<br>`Assets/Prefabs/Meteors/SmallGreyMeteor_1.prefab:162`<br>`Assets/Prefabs/Meteors/SmallGreyMeteor_2.prefab:162`<br>`Assets/Prefabs/Meteors/TinyBrownMeteor_1.prefab:162`<br>`Assets/Prefabs/Meteors/TinyBrownMeteor_2.prefab:162`<br>`Assets/Prefabs/Meteors/TinyGreyMeteor_1.prefab:162`<br>`Assets/Prefabs/Meteors/TinyGreyMeteor_2.prefab:162` | `Assets/Scripts/GameLoop/AutoShooter.cs:186`<br>`Assets/Scripts/Meteor/MeteorSpawner.cs:65`<br>`Assets/Scripts/Meteor/MeteorSpawner.cs:81`<br>`Assets/Scripts/Meteor/MeteorSplit.cs:113`<br>`Assets/Scripts/Meteor/MeteorSplit.cs:122`<br>`Assets/Scripts/Meteor/MeteorSplit.cs:132`<br>`Assets/Scripts/Meteor/MeteorSplit.cs:164`<br>`Assets/Scripts/Meteor/MeteorSplit.cs:193`<br>`Assets/Scripts/Meteor/MeteorSplit.cs:230`<br>`Assets/Scripts/Meteor/MeteorSplit.cs:237` | KEEP: serialized attachment |
| `Assets/Scripts/Meteor/MeteorSpawner.cs` | `0f30a180d0702a246b992730f4c7afb8` | `Assets/Scenes/Game.unity:1148` | `Assets/Scripts/GameLoop/WaveManager.cs:26`<br>`Assets/Scripts/GameManager.cs:17`<br>`Assets/Scripts/GameManager.cs:93`<br>`Assets/Scripts/Meteor/MeteorMovement.cs:37`<br>`Assets/Scripts/Meteor/MeteorSplit.cs:25` | KEEP: serialized attachment |
| `Assets/Scripts/Meteor/MeteorSplit.cs` | `f7c76ed595be7d648a51d18319da6fe6` | `Assets/Prefabs/Meteors/BigBrownMeteor_1.prefab:176`<br>`Assets/Prefabs/Meteors/MediumBrownMeteor_1.prefab:176`<br>`Assets/Prefabs/Meteors/MediumBrownMeteor_2.prefab:176`<br>`Assets/Prefabs/Meteors/MediumGreyMeteor_1.prefab:176`<br>`Assets/Prefabs/Meteors/MediumGreyMeteor_2.prefab:176`<br>`Assets/Prefabs/Meteors/SmallBrownMeteor_1.prefab:176`<br>`Assets/Prefabs/Meteors/SmallBrownMeteor_2.prefab:176`<br>`Assets/Prefabs/Meteors/SmallGreyMeteor_1.prefab:176`<br>`Assets/Prefabs/Meteors/SmallGreyMeteor_2.prefab:176`<br>`Assets/Prefabs/Meteors/TinyBrownMeteor_1.prefab:176`<br>`Assets/Prefabs/Meteors/TinyBrownMeteor_2.prefab:176`<br>`Assets/Prefabs/Meteors/TinyGreyMeteor_1.prefab:176`<br>`Assets/Prefabs/Meteors/TinyGreyMeteor_2.prefab:176` | `Assets/Scripts/Player/HammerCursorController.cs:313`<br>`Assets/Scripts/Player/HammerCursorController.cs:333`<br>`Assets/Scripts/Player/ProjectileController.cs:41`<br>`Assets/Scripts/VFX/LightningChain.cs:71` | KEEP: serialized attachment |
| `Assets/Scripts/Player/HammerCursorController.cs` | `7d9433e3a4f00554bb99ad9e6f5a9e89` | `Assets/Scenes/Game.unity:374` | none | KEEP: serialized attachment |
| `Assets/Scripts/Player/PlayerController.cs` | `a4aa43fc83a75f64fa4dd9fc854b4cd1` | `Assets/Scenes/Game.unity:756` | `Assets/Scripts/GameLoop/AutoShooter.cs:71` | KEEP: serialized attachment |
| `Assets/Scripts/Player/PlayerHealth.cs` | `96466bce46ce49f6aba90eaf0a4ec57e` | `Assets/Scenes/Game.unity:897` | `Assets/Scripts/GameManager.cs:18`<br>`Assets/Scripts/GameManager.cs:97` | KEEP: serialized attachment |
| `Assets/Scripts/Player/PlayerPowerupHandler.cs` | `c5b05827a900c72478fc534401fd0d56` | none | `Assets/Scripts/GameLoop/AutoShooter.cs:122`<br>`Assets/Scripts/GameManager.cs:200`<br>`Assets/Scripts/Player/PlayerController.cs:36`<br>`Assets/Scripts/Player/PlayerHealth.cs:17`<br>`Assets/Scripts/Powerups/PowerupBase.cs:54` | KEEP: runtime code reference |
| `Assets/Scripts/Player/ProjectileController.cs` | `b236692a58f5aac49be7a97a83529331` | `Assets/Prefabs/Projectiles/Projectile.prefab:171` | none | KEEP: serialized attachment |
| `Assets/Scripts/Player/ShieldController.cs` | `680cbf33cc1bf0c42992da61118cbc7b` | `Assets/Scenes/Game.unity:927` | `Assets/Scripts/Player/PlayerHealth.cs:9`<br>`Assets/Scripts/Player/PlayerHealth.cs:15`<br>`Assets/Scripts/Player/PlayerPowerupHandler.cs:26` | KEEP: serialized attachment |

## Removal evidence and conservative exceptions

- Remove exactly seven scripts and their metadata: Bullet, CameraLook, CustomCursor, MeteorSpawnerGL, MoveObject, PlayerHealthGL, PlayerMovement (all under Assets/Scripts/GameLoop). Their class/GUID searches found no scene/prefab/other serialized attachment or executable external identifier/type-loader reference. Comment and Tooltip mentions do not instantiate them.
- MeteorSpawnerGL and PlayerHealthGL are absent from every scene/prefab. The live equivalents are MeteorSpawner (Game.unity:1148) and PlayerHealth (Game.unity:897). No live scene or prefab is changed to remove them.
- ScoreManager is left as unsure. Its only executable consumer is PlayerHealthGL.cs:58; ScoreManager itself has no scene/prefab GUID references. Deleting the caller makes it apparently orphaned, but this audit deliberately does not cascade deletion through that chain.
- MeteorMover is left because AutoShooter.cs:181 (a component in Game.unity:909) queries it. MeteorSpawnerGL.cs:78 also queries it; removing the spawner does not remove the live AutoShooter dependency.
- UIManager is left because WaveManager.cs:74 (a component in Game.unity:1206) calls UpdateWave. It also serves PlayerHealthGL and ScoreManager. The lack of a current serialized UIManager attachment does not make the class safe to remove.
- AutoShooter, EnemyHealth and WaveManager in GameLoop are live and remain. All Player, Meteor and Enemy scripts remain; PlayerPowerupHandler is added at runtime by PlayerHealth.Start(). EnemyFaction derives from qualified UnityEngine.MonoBehaviour and is included.
- EnemyProjectile references in PlayerHealth.CompareTag are tag strings, while actual serialized EnemyProjectile attachments independently prove the class is live. AutoShooter diagnostic log strings and documentation comments are not counted as callers.

## String and comment mentions

### BossController

External literal-string mentions (inspected; no dynamic type loader): none.

Comment-only external mentions: none.

### EnemyActiveTracker

External literal-string mentions (inspected; no dynamic type loader): none.

Comment-only external mentions: none.

### EnemyCombatAI

External literal-string mentions (inspected; no dynamic type loader): none.

Comment-only external mentions: none.

### EnemyController

External literal-string mentions (inspected; no dynamic type loader): none.

Comment-only external mentions: `Assets/Scripts/Enemy/BossController.cs:4`, `Assets/Scripts/Enemy/EnemyActiveTracker.cs:4`, `Assets/Scripts/Enemy/EnemyFaction.cs:4`, `Assets/Scripts/GameLoop/AutoShooter.cs:175`.

### EnemyFaction

External literal-string mentions (inspected; no dynamic type loader): none.

Comment-only external mentions: `Assets/Scripts/Enemy/EnemyCombatAI.cs:10`.

### EnemyProjectile

External literal-string mentions (inspected; no dynamic type loader): `Assets/Scripts/Player/PlayerHealth.cs:42`, `Assets/Scripts/Player/PlayerHealth.cs:66`.

Comment-only external mentions: `Assets/Scripts/Enemy/EnemyCombatAI.cs:12`, `Assets/Scripts/Enemy/EnemyShooter.cs:5`, `Assets/Scripts/Player/PlayerHealth.cs:68`.

### EnemyShooter

External literal-string mentions (inspected; no dynamic type loader): none.

Comment-only external mentions: `Assets/Scripts/Enemy/EnemyCombatAI.cs:10`, `Assets/Scripts/Enemy/EnemyCombatAI.cs:12`.

### EnemySpawner

External literal-string mentions (inspected; no dynamic type loader): none.

Comment-only external mentions: `Assets/Scripts/Enemy/EnemyActiveTracker.cs:5`.

### SpawnDirector

External literal-string mentions (inspected; no dynamic type loader): `Assets/Scripts/Utilities/GameLogger.cs:125`, `Assets/Scripts/Utilities/GameLogger.cs:127`.

Comment-only external mentions: `Assets/Scripts/Enemy/EnemySpawner.cs:7`, `Assets/Scripts/GameLoop/WaveManager.cs:55`, `Assets/Scripts/GameLoop/WaveManager.cs:66`, `Assets/Scripts/Utilities/GameFeatureFlags.cs:22`.

### AutoShooter

External literal-string mentions (inspected; no dynamic type loader): `Assets/Scripts/Utilities/GameLogger.cs:111`.

Comment-only external mentions: `Assets/Scripts/GameLoop/Bullet.cs:9`, `Assets/Scripts/GameLoop/CameraLook.cs:12`, `Assets/Scripts/GameLoop/CameraLook.cs:13`, `Assets/Scripts/GameLoop/EnemyHealth.cs:9`, `Assets/Scripts/GameLoop/MoveObject.cs:13`, `Assets/Scripts/Player/PlayerController.cs:99`, `Assets/Scripts/Player/PlayerController.cs:151`, `Assets/Scripts/Services/SkillService.cs:27`, `Assets/Scripts/Services/SkillService.cs:32`, `Assets/Scripts/Services/SkillService.cs:36`, `Assets/Scripts/Services/SkillService.cs:88`, `Assets/Scripts/Services/SkillService.cs:96`, `Assets/Scripts/Services/SkillService.cs:99`, `Assets/Scripts/Services/SkillService.cs:102`, `Assets/Scripts/Utilities/GameLogger.cs:10`, `Assets/Scripts/Utilities/GameLogger.cs:14`, `Assets/Scripts/Utilities/GameLogger.cs:20`, `Assets/Scripts/Utilities/GameLogger.cs:106`.

### Bullet

External literal-string mentions (inspected; no dynamic type loader): none.

Comment-only external mentions: `Assets/Scripts/GameLoop/EnemyHealth.cs:9`, `Assets/Scripts/GameLoop/EnemyHealth.cs:35`.

### CameraLook

External literal-string mentions (inspected; no dynamic type loader): none.

Comment-only external mentions: `Assets/Scripts/GameLoop/PlayerMovement.cs:10`, `Assets/Scripts/GameLoop/PlayerMovement.cs:31`.

### CustomCursor

External literal-string mentions (inspected; no dynamic type loader): none.

Comment-only external mentions: none.

### EnemyHealth

External literal-string mentions (inspected; no dynamic type loader): `Assets/Scripts/GameLoop/MeteorSpawnerGL.cs:21`.

Comment-only external mentions: `Assets/Scripts/Enemy/BossController.cs:4`, `Assets/Scripts/Enemy/BossController.cs:7`, `Assets/Scripts/Enemy/BossController.cs:28`, `Assets/Scripts/Enemy/EnemyFaction.cs:4`, `Assets/Scripts/GameLoop/Bullet.cs:5`, `Assets/Scripts/GameLoop/Bullet.cs:10`, `Assets/Scripts/GameLoop/MeteorMover.cs:8`, `Assets/Scripts/GameLoop/MeteorSpawnerGL.cs:10`, `Assets/Scripts/Player/ProjectileController.cs:21`, `Assets/Scripts/Powerups/PowerupSpawner.cs:6`.

### MeteorMover

External literal-string mentions (inspected; no dynamic type loader): `Assets/Scripts/GameLoop/MeteorSpawnerGL.cs:21`, `Assets/Scripts/GameLoop/MeteorSpawnerGL.cs:37`.

Comment-only external mentions: `Assets/Scripts/GameLoop/AutoShooter.cs:13`, `Assets/Scripts/GameLoop/AutoShooter.cs:173`, `Assets/Scripts/GameLoop/EnemyHealth.cs:8`, `Assets/Scripts/GameLoop/MeteorSpawnerGL.cs:10`, `Assets/Scripts/GameLoop/MeteorSpawnerGL.cs:63`, `Assets/Scripts/GameLoop/MeteorSpawnerGL.cs:77`, `Assets/Scripts/GameLoop/MoveObject.cs:13`, `Assets/Scripts/GameLoop/MoveObject.cs:22`.

### MeteorSpawnerGL

External literal-string mentions (inspected; no dynamic type loader): `Assets/Scripts/GameLoop/MeteorMover.cs:18`, `Assets/Scripts/GameLoop/MeteorMover.cs:21`.

Comment-only external mentions: `Assets/Scripts/GameLoop/MeteorMover.cs:9`, `Assets/Scripts/GameLoop/MeteorMover.cs:34`, `Assets/Scripts/GameLoop/MoveObject.cs:13`, `Assets/Scripts/GameLoop/MoveObject.cs:22`, `Assets/Scripts/GameLoop/WaveManager.cs:6`, `Assets/Scripts/GameLoop/WaveManager.cs:9`, `Assets/Scripts/GameLoop/WaveManager.cs:10`.

### MoveObject

External literal-string mentions (inspected; no dynamic type loader): none.

Comment-only external mentions: none.

### PlayerHealthGL

External literal-string mentions (inspected; no dynamic type loader): none.

Comment-only external mentions: `Assets/Scripts/GameLoop/ScoreManager.cs:41`, `Assets/Scripts/GameLoop/UIManager.cs:6`, `Assets/Scripts/GameLoop/UIManager.cs:47`, `Assets/Scripts/GameLoop/UIManager.cs:60`.

### PlayerMovement

External literal-string mentions (inspected; no dynamic type loader): none.

Comment-only external mentions: none.

### ScoreManager

External literal-string mentions (inspected; no dynamic type loader): none.

Comment-only external mentions: `Assets/Scripts/GameLoop/PlayerHealthGL.cs:53`, `Assets/Scripts/GameLoop/UIManager.cs:6`, `Assets/Scripts/GameLoop/UIManager.cs:41`.

### UIManager

External literal-string mentions (inspected; no dynamic type loader): none.

Comment-only external mentions: `Assets/Scripts/GameLoop/PlayerHealthGL.cs:5`, `Assets/Scripts/GameLoop/PlayerHealthGL.cs:11`, `Assets/Scripts/GameLoop/PlayerHealthGL.cs:52`, `Assets/Scripts/GameLoop/ScoreManager.cs:5`, `Assets/Scripts/GameLoop/ScoreManager.cs:9`, `Assets/Scripts/GameLoop/ScoreManager.cs:33`.

### WaveManager

External literal-string mentions (inspected; no dynamic type loader): none.

Comment-only external mentions: `Assets/Scripts/Enemy/SpawnDirector.cs:5`, `Assets/Scripts/Enemy/SpawnDirector.cs:19`, `Assets/Scripts/GameLoop/MeteorMover.cs:14`, `Assets/Scripts/GameLoop/MeteorSpawnerGL.cs:12`, `Assets/Scripts/GameLoop/UIManager.cs:53`, `Assets/Scripts/GameManager.cs:103`.

### MeteorMovement

External literal-string mentions (inspected; no dynamic type loader): `Assets/Scripts/Meteor/MeteorSplit.cs:116`.

Comment-only external mentions: `Assets/Scripts/GameLoop/AutoShooter.cs:174`.

### MeteorSpawner

External literal-string mentions (inspected; no dynamic type loader): none.

Comment-only external mentions: `Assets/Scripts/Enemy/EnemySpawner.cs:5`, `Assets/Scripts/GameLoop/MeteorSpawnerGL.cs:6`, `Assets/Scripts/Meteor/MeteorMovement.cs:9`.

### MeteorSplit

External literal-string mentions (inspected; no dynamic type loader): none.

Comment-only external mentions: `Assets/Scripts/Meteor/MeteorMovement.cs:8`, `Assets/Scripts/Powerups/MeteorDropSpawner.cs:13`.

### HammerCursorController

External literal-string mentions (inspected; no dynamic type loader): none.

Comment-only external mentions: `Assets/Scripts/VFX/LightningChain.cs:47`.

### PlayerController

External literal-string mentions (inspected; no dynamic type loader): `Assets/Scripts/GameLoop/AutoShooter.cs:23`, `Assets/Scripts/GameLoop/AutoShooter.cs:80`, `Assets/Scripts/GameLoop/AutoShooter.cs:82`.

Comment-only external mentions: `Assets/Scripts/GameLoop/AutoShooter.cs:68`.

### PlayerHealth

External literal-string mentions (inspected; no dynamic type loader): none.

Comment-only external mentions: `Assets/Scripts/Enemy/EnemyProjectile.cs:5`, `Assets/Scripts/Enemy/EnemyProjectile.cs:25`, `Assets/Scripts/GameLoop/PlayerHealthGL.cs:6`, `Assets/Scripts/Player/PlayerPowerupHandler.cs:4`, `Assets/Scripts/Player/ShieldController.cs:7`.

### PlayerPowerupHandler

External literal-string mentions (inspected; no dynamic type loader): none.

Comment-only external mentions: `Assets/Scripts/Powerups/PowerupBase.cs:23`, `Assets/Scripts/Services/PowerupLevelService.cs:10`.

### ProjectileController

External literal-string mentions (inspected; no dynamic type loader): `Assets/Scripts/GameLoop/AutoShooter.cs:40`.

Comment-only external mentions: `Assets/Scripts/GameLoop/AutoShooter.cs:11`, `Assets/Scripts/GameLoop/AutoShooter.cs:258`.

### ShieldController

External literal-string mentions (inspected; no dynamic type loader): none.

Comment-only external mentions: `Assets/Scripts/VFX/ScreenFlash.cs:6`, `Assets/Scripts/VFX/ShieldVisual.cs:7`, `Assets/Scripts/VFX/ShieldVisual.cs:54`, `Assets/Scripts/VFX/ShieldVisual.cs:61`.
