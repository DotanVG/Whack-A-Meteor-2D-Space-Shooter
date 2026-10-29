# WAM Phase 2 design

## Overview
The current Game scene has auto-aimed fire, a manually aimed hammer, splitting meteors,
four enemy factions, a boss prefab, shields, chain lightning, pickups and persistent
Stardust/Metal. HyperdriveShop already consumes the Phase 1 SkillService catalogue.
WaveManager currently compounds interval reductions and advances after a short timer;
enemy movement also uses rotated local down, so downward-facing ships can move upward.

## Player fantasy
Survive a readable meteor belt, break into a raider sector, then face an elite encounter.
The hammer clears space while upgraded shots handle durable targets. Each completed
wave should yield visible progress toward the next shop purchase.

## Detailed rules
Reuse the existing scene, prefabs, tags, scoring and skill save API. Author sector/wave
and combat profiles as committed ScriptableObject data. Give waves a finite active
window, a short regroup interval, a visible name/countdown and explicit survival rewards.
Difficulty derives from absolute wave number rather than mutating the previous wave.
Milestones use the existing boss with deliberate health initialization and cleanup.
Meteor profiles distinguish a swift shard, a splitting boulder and a metal-rich armored
rock. Enemy profiles distinguish an aimed shooter from a turning pursuit/diver.

Add approximately seven single-level skills: piercing, twin shots, critical hits,
hammer impact damage, lightning reach, shield pulse and pickup magnet. Append enum values
and use stable IDs with legacyId=-1; preserve every original ID/cost/modifier bit pattern.
The shop must scroll so later rows remain reachable. Reuse existing score popups, hit
flashes and lightning; add short procedural rings/damage labels with bounded lifetimes.

## Formulas
Use authored base wave settings plus a bounded absolute-wave difficulty curve:
spawn intervals never increase, speed/health/rewards never decrease as waves progress.
Distinct sector labels/mixes may repeat while difficulty continues upward. Health is
initialized before damage can arrive; death rewards are guarded against duplicate hits.
Critical damage uses an injected roll in pure logic tests; piercing counts distinct targets.
Exact final values and costs are in the committed assets and the PR's skill/balance tables.

## Edge cases
Pause/countdown/game over stop wave time and spawning; restarting starts wave one.
Cap entities and remove escaped enemies. Never grant a boss-kill bounty for an escaped
boss. Split children inherit momentum and receive appropriate bounded durability.
New skills stay unowned on existing saves, including interrupted legacy migration.
No save adapter in tests may alter the real wallet. Headless tests prove data/integration,
not visual quality or play feel.

## Dependencies and alternatives
Keep the existing SkillProgress/SkillService, BalanceService, GameManager, prefabs and
Kenney CC0 sprites. No paid assets or additional package/network runtime dependency.
Reviewed [ScriptableObject-Architecture](https://github.com/DanielEverland/ScriptableObject-Architecture)
(MIT, archived) and [Unity Open Project's event channels](https://github.com/UnityTechnologies/open-project-1/wiki/Event-system)
(official example architecture). Their broad asset/event frameworks would duplicate the
existing small catalogue and add maintenance/import surface. Adapt the data-versus-runtime
separation in-place instead; no external source code is copied. MCP remains Editor tooling.

## Tuning knobs
Sector duration/regroup time, spawn interval/caps, profile speed/HP/metal, enemy turn rate
and shot cadence, milestone frequency/HP, survival rewards and new SkillDefinition values.
Existing Phase 1 skill numbers remain unchanged.

## Acceptance criteria
Data validates with unique IDs, valid prerequisites and no cycles. Tests cover monotonic
difficulty/rewards, new modifier aggregation, damage/critical/piercing logic and scene/prefab
references. Final batch compile has zero error-CS lines; all EditMode tests and both Gate v2
commands pass. Manual checklist covers the new waves, variants, shop reachability and attacks.
No browser/device/play-feel claim is made without observation.
