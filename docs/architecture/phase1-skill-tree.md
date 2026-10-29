# WAM Phase 1: data-driven skills

## Overview

Phase 1 replaces the 18-entry C# catalogue with 12 ScriptableObject definitions
and an ordered SkillTree in `Assets/Resources/Skills`. Their levels reproduce all
18 original IMGUI shop rows. No gameplay content or balance values are added.
The source of truth for old costs/labels/order is the pre-refactor table, captured
independently in `Assets/Tests/EditMode/LegacySkillTable.json`.

## Player fantasy

The same ship, hammer and auto-shooter upgrades, bought through the same shop.

## Detailed rules

- SkillDefinition owns stable id, display name, branch, starting row/tier,
  prerequisites, effect classification and an ordered list of levels. Each level
  owns its label, Stardust/Metal costs, migration-only legacy id and modifiers.
- SkillProgress is the shared state/purchase/aggregation implementation. ISkillSave
  and ISkillWallet allow tests without scene objects or developer saves.
- SkillService keeps its original MonoScript GUID and scene component. Its Current
  property makes the same implementation available without GameServices in Shop
  and Settings. Existing null-safe gameplay helpers are thin stat-query wrappers.
- Buying advances exactly one level. A prerequisite definition must be at its
  final level, preserving the old row chain. A higher level implies earlier levels
  of that skill; it never grants unrelated prerequisite skills during migration.
- Successful purchases deduct both costs using the legacy shop's exact wallet
  keys, `Economy.Stardust` and `Economy.Metal`, and save wallet + ownership together.
  Like the old shop, spending is independent of UseEconomy/InfiniteCurrency. The
  standalone Shop has no EconomyService; gameplay loads that wallet on scene entry.
  On successful changes, the facade also refreshes a live EconomyService cache,
  preventing later earnings from overwriting deductions if an in-game caller buys.
- DevMode.AllSkillsOwned supplies maximum levels dynamically without persisting
  them. Unlike the old separate shop ownership cache, shop rows now share this
  override with gameplay.
- The old `All[id]` purchase lookup incorrectly treated legacy ids as visual array
  indexes. Stable-id purchases fix this wiring: a clicked row buys its displayed
  definition and level for its displayed cost. The old mismatched purchases are
  migrated according to the actual persisted legacy key, without inventing intent.

## Formulas

The highest owned level supplies a complete modifier snapshot. All shipped
modifiers use Set, so Fire Rate level 2 returns the float literal 2.0 rather than
1.5 multiplied by 2.0. Add and Multiply are supported for authored modifiers but
unused by this phase. Existing defaults (multipliers = 1, hammer score = 2,
charges/flags/bounces/bonus = 0) are unchanged. An exhaustive test compares IEEE
float bits for 13 outputs over all 2^18 original ownership states.

## Edge cases and migration

New keys are `Skills.<stable id>` (integer highest owned level) and
`Skills.SaveVersion = 1`. Before version 1, read every old key once; only the
integer value 1 counts as owned. Merge the highest legacy level with existing new
progress, clamp corrupt new levels, then save the version marker last. Repeating
migration before the marker is safe. After the marker, no legacy keys are read.
Old keys are retained for rollback. Reset deletes new ownership and keeps the
version marker, so retained rollback keys cannot resurrect purchases. Downgrading
to the old implementation does not reflect subsequent new purchases or resets.

| Legacy id | Name | Level | New stable id |
|---|---|---|---|
| 0 | Fire Rate | 1 | autoshooter.fire_rate |
| 1 | Fire Rate | 2 | autoshooter.fire_rate |
| 2 | Accuracy | 1 | autoshooter.accuracy |
| 3 | Proj. Speed | 1 | autoshooter.projectile_speed |
| 4 | AOE Radius | 1 | hammer.radius |
| 5 | AOE Radius | 2 | hammer.radius |
| 6 | Score Mult | 1 | hammer.score |
| 7 | Slam Wave | 1 | hammer.slam_wave |
| 8 | Move Speed | 1 | ship.move_speed |
| 9 | Move Speed | 2 | ship.move_speed |
| 10 | Boost Duration | 1 | ship.boost_duration |
| 11 | Invincibility | 1 | ship.invincibility |
| 12 | Target Priority | 1 | autoshooter.target_priority |
| 13 | Shield | 1 | ship.shield |
| 14 | Shield | 2 | ship.shield |
| 15 | Lightning | 1 | hammer.lightning |
| 16 | Lightning | 2 | hammer.lightning |
| 17 | Lightning | 3 | hammer.lightning |

## Dependencies

Unity 2022.3.29f1, C# 9 and the already-installed Test Framework 1.1.33. A small
auto-referenced Wam.Skills assembly contains only the new data/core/adapters.
Assembly-CSharp remains intact; an Editor-only NUnit asmdef references Wam.Skills.
Scene/shop integration tests use reflection to exercise existing Assembly-CSharp
controllers without moving live scripts or changing their assembly identity.
Unit tests use in-memory saves; the PlayerPrefs adapter test uses a unique
`Wam.Phase1.Tests.<guid>.` prefix and deletes only its own keys in finally.

## Tuning knobs

Designers can edit the assets; for Phase 1 every authored value must stay equal
to the old table and formulas. Stable ids are save identities and must never be
reused. Levels contain full snapshots, not increments. Prerequisites must exist
and form a DAG. Validate detects duplicate ids/legacy ids, missing prerequisites,
cycles, invalid branch/row data, overlapping rows, bad costs and invalid modifiers.

## Acceptance criteria and verification

- Exact 18-row labels, costs, order and prerequisite relationships match the
  independent golden table; all 18 purchases spend their displayed costs.
- Purchase failures, double purchase, prerequisite/level ordering, empty/corrupt
  saves, every legacy id, migration idempotence, reset and dev override are tested.
- All 262,144 legacy ownership combinations return bit-identical modifiers.
- Game and Shop scenes and every Assets prefab load without missing scripts or
  serialized references; automated shop row-state/purchase wiring is tested.
- Run Unity batch compile and EditMode tests headlessly, read NUnit XML, then run
  both license-free check scripts against staged files. Logs/results live under
  `C:\Users\dotan\terminals\mario-runs\wam-phase1\qa` and are not committed.
- Play mode feel, WebGL, real devices, touch, actual graphical shop click-through,
  and migration of a real old player save require separate human verification.

See `phase1-script-reference-audit.md` for every duplicate-system keep/remove
decision and its original scene/prefab/code evidence. No live scripts were moved.
