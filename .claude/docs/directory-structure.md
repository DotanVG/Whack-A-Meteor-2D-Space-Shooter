# Directory Structure

Unity 2022.3.29f1 project layout:

```text
/
├── CLAUDE.md                         # Master configuration
├── .claude/                          # Agent definitions, skills, hooks, rules, docs
├── Assets/
│   ├── Scenes/                       # Unity scenes (SplashScreen, MainMenu, Game, Settings)
│   ├── Scripts/
│   │   ├── GameManager.cs            # Singleton: score, lives, pause, game-over, HUD
│   │   ├── Input/
│   │   │   └── InputManager.cs       # Singleton: New Input System + legacy fallbacks
│   │   ├── Meteors/                  # MeteorSplit, MeteorSpawner, etc.
│   │   ├── Player/                   # PlayerController, HammerCursorController, etc.
│   │   ├── Utilities/
│   │   │   ├── GameConstants.cs      # Score values, lives, invincibility duration
│   │   │   └── CameraShake.cs        # Singleton: CameraShake.Instance.Shake()
│   │   └── ...
│   ├── Prefabs/                      # Meteor prefabs, player, projectiles, etc.
│   ├── Sprites/                      # Kenney Space Shooter Redux sprites (CC0)
│   └── ...
├── design/                           # Game design documents (gdd, balance)
├── docs/                             # Technical documentation
├── production/                       # Production management (sprints, milestones, releases)
│   ├── session-state/                # Ephemeral session state (active.md — gitignored)
│   └── session-logs/                 # Session audit trail (gitignored)
├── ProjectSettings/                  # Unity project settings (tracked in git)
└── Packages/                         # Unity package manifest
```
