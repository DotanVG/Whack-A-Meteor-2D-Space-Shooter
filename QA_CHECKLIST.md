# Whack-A-Meteor — Playtester QA Checklist

> Open the Unity project, load `Assets/Scenes/Game.unity`, and press **Play**.
> Work through each section. Mark PASS / FAIL / SKIP for each item.
> SKIP items that require a skill you haven't purchased yet.

---

## 1. Core Game Loop

- [ ] Game loads and starts — meteors fall from the top
- [ ] Ship moves with **W/A/S/D** (or left stick)
- [ ] Ship fires with **Space** (or LT/X)
- [ ] Hammer cursor follows mouse (or right stick) and swings on **LMB** (or RT)
- [ ] Projectile hitting a BigMeteor splits it into 2–3 Medium meteors
- [ ] Medium → Small → Tiny (terminal, no further split)
- [ ] Score increments correctly (Big=5, Medium=10, Small=15, Tiny=20)
- [ ] Hammer hit awards **2× score** vs projectile
- [ ] Lives display in the HUD (top-left icons)
- [ ] Colliding with a meteor loses a life and triggers 3s invincibility (ship flashes)
- [ ] Game-Over overlay appears when lives reach 0
- [ ] **R** restarts the game; **Enter/Escape** returns to Main Menu

---

## 2. Wave System

- [ ] First wave spawns meteors
- [ ] Between waves a gap of ~5s appears, then next wave starts
- [ ] Meteor spawn rate increases each wave (gets harder)
- [ ] Meteor speed increases each wave
- [ ] Wave number is tracked (check GameLogger in Console if needed)

---

## 3. Enemy Factions (appear at Wave 4)

- [ ] **Blue** enemy ships appear and move toward player (speed ≈ 2.5)
- [ ] **Black** enemy ships appear — slower but tanky (HP=3)
- [ ] **Green** enemy ships appear — fast but fragile (HP=1)
- [ ] **Red** enemy ships appear and **fire green projectiles** at the player
- [ ] Enemy projectile (green laser) damages the player on contact
- [ ] Killing an enemy awards 25 score points + Metal currency
- [ ] Metal amounts per faction: Black=5, Blue=2, Green=1, Red=3
- [ ] Enemy cap of 12 is respected (no more than 12 live enemies at once)

### Cross-Faction Combat

- [ ] Enemies occasionally fire at enemies of **other factions** (observe in play — ~25% chance per shot cycle)
- [ ] A cross-faction shot visually hits and damages the target enemy
- [ ] Enemies can be killed by other enemies (faction warfare)

---

## 4. Boss (appears every 5 waves)

- [ ] Boss spawns at Wave 5 (large black ship, ~2.5× scale)
- [ ] Boss has 20 HP (takes multiple projectile hits)
- [ ] Boss fires **red projectiles** at the player
- [ ] Defeating the boss:
  - [ ] Awards 20 Metal
  - [ ] Triggers a **large red explosion** (BossExplosion VFX)
  - [ ] Score popup appears
- [ ] Regular enemy deaths trigger a **blue explosion** (EnemyExplosion VFX)

---

## 5. Economy & Store

- [ ] **Stardust** counter visible in HUD (top area)
- [ ] **Metal** counter visible in HUD
- [ ] Meteor kills earn Stardust (Big=3, Medium=2, Small/Tiny=1)
- [ ] Enemy kills earn Metal (per faction, see above)
- [ ] Open the **Store** between runs — Stardust and Metal balances show correctly
- [ ] Purchasing a skill deducts the correct currency
- [ ] Skill persists after restarting (PlayerPrefs saved)

---

## 6. Skill Tree (15 nodes + 3 Lightning)

Open Store → navigate to skill tree.

### AutoShooter column
- [ ] **Fire Rate Lv1** (500 SD): ship fires noticeably faster
- [ ] **Fire Rate Lv2** (800 SD, requires Lv1): fire rate increases further
- [ ] **Accuracy Lv1** (600 SD, requires FR Lv2): bullets spread less
- [ ] **Proj Speed Lv1** (700 SD, requires Acc): bullets travel faster
- [ ] **Target Priority** (900 SD, requires PS): autoshooter prefers enemy ships over meteors

### Hammer column
- [ ] **AOE Radius Lv1** (500 SD): hammer AOE circle visibly larger
- [ ] **AOE Radius Lv2** (800 SD): even larger
- [ ] **Score Mult Lv1** (1000 SD): hammer kills award 3× score (not 2×)
- [ ] **Slam Wave** (500 SD + 800 Metal): hammer swing also hits a secondary outer ring (~2× radius)
- [ ] **Lightning Lv1** (700 SD + 200 Metal, requires Slam Wave): after hammer hit, 2 lightning arcs jump to nearby targets
- [ ] **Lightning Lv2** (1000 SD + 400 Metal): 4 bounces
- [ ] **Lightning Lv3** (1400 SD + 700 Metal): 6 bounces + +1 bonus damage per hop

### Ship column
- [ ] **Move Speed Lv1** (400 SD): ship moves 15% faster
- [ ] **Move Speed Lv2** (650 SD): ship moves 30% faster
- [ ] **Boost Duration** (750 SD): Left Shift boost lasts longer (2.25s → not visible directly, verify via feel)
- [ ] **Shield Lv1** (600 SD, requires Boost): 1 shield charge — first hit is absorbed, ship flashes blue
- [ ] **Shield Lv2** (900 SD + 400 Metal): 2 charges
- [ ] **Invincibility Lv1** (1200 SD + 600 Metal): post-hit invincibility lasts 4.5s instead of 3s

---

## 7. Powerups (from enemies)

Enemy kills have a 15% chance to drop a powerup.

- [ ] **Shield Recharge** (blue shield icon): adds 1 shield charge
- [ ] **Speed Boost** (green icon): ship moves 1.5× faster for 5s — HUD shows "SPEED Xs" timer
- [ ] **Double Fire** (yellow icon): fire rate doubled for 10s — HUD shows "2x FIRE Xs"
- [ ] **Score Multiplier** (purple icon): score ×2 for 10s — HUD shows "2x SCORE Xs"
- [ ] **Extra Life** (life icon): +1 life immediately

---

## 8. Tier Collectibles (from meteors)

Meteor kills drop tier collectibles at low chance; Big meteors drop 2× as often.

- [ ] **BoltTier** (blue bolt icon): collecting upgrades the Bolt tier (0→1→2→3), HUD badge in top-right shows ◆/◆◆/◆◆◆
- [ ] **ShieldTier** (blue shield icon): upgrades Shield tier, shown in HUD
- [ ] **StarTier** (blue star icon): upgrades Star tier, shown in HUD
- [ ] Tier badge section appears in top-right HUD when any tier > 0

---

## 9. Pill Drops (from meteors)

- [ ] **PillHealth** (red pill): +1 life immediately
- [ ] **PillLaserBoost** (blue pill): boosts auto-shooter / laser for 5s — HUD shows "LASER+ Xs" timer in blue

---

## 10. Shield Visual

> Requires Shield Lv1 purchased in skill tree.

- [ ] After buying Shield Lv1, start a new run — a pulsing blue ring appears around the ship
- [ ] Ring fades in on run start
- [ ] Alpha pulses gently while shields are active
- [ ] Hit absorbed — ring disappears (shield depleted)
- [ ] Collecting ShieldRecharge powerup restores the ring

---

## 11. Lightning Chain

> Requires Lightning Lv1+ purchased.

- [ ] Swing hammer at a group of enemies/meteors — lightning arcs draw between targets
- [ ] Arc is white/blue with slight randomization (not perfectly straight)
- [ ] Each hop deals 1 damage to enemies
- [ ] At Lv3: each hop deals 2 damage
- [ ] Arc fades out within ~0.12s

---

## 12. Player Damage Visual

> Requires PlayerDamageVisual component on Player GO with sprites assigned.
> *Note: May need manual wiring in Inspector — Player GO → Add Component → PlayerDamageVisual → assign playerShip1_damage1/2/3 sprites from Assets/Sprites/Damage/*

- [ ] At 2 lives: light damage overlay appears on ship
- [ ] At 1 life: heavy damage overlay
- [ ] At 3 lives: no overlay (clean ship)
- [ ] Overlay updates immediately on taking damage

---

## 13. Level Progression

- [ ] A level/XP display is visible somewhere in HUD (level number)
- [ ] Killing enemies/meteors earns XP (score = XP earned)
- [ ] On level up, a popup or notification appears
- [ ] Level persists across runs (saved in PlayerPrefs)
- [ ] XP curve: Level 1→2 requires 5000 XP at default settings

---

## 14. Wave Telemetry (Console)

Open Unity Console and check for log messages:
- [ ] `[Wave]` messages when waves start/end
- [ ] `[Enemy]` spawn and kill messages
- [ ] `[Boss]` spawn and death messages
- [ ] `[Powerup]` activation messages when powerups are collected
- [ ] `[Shield]` messages when shield absorbs a hit
- [ ] No red error spam during normal gameplay

---

## 15. Edge Cases & Stability

- [ ] Meteor cap (80): spawn lots of meteors; game doesn't crash or lag severely
- [ ] Enemy cap (12): enemy count never exceeds 12 simultaneously
- [ ] Game Over then Restart: all state resets (score=0, lives=3, tier levels reset)
- [ ] Pausing with **P/Escape**: game freezes; unpausing resumes correctly
- [ ] Return to Main Menu from pause: works correctly
- [ ] No NullReferenceExceptions in Console during a full game session

---

## Known Manual Inspector Steps Still Required

These items cannot be wired from code — open Unity Editor → select the specified object → assign in Inspector:

1. **Player GO** → Add Component → `PlayerDamageVisual` → assign `Assets/Sprites/Damage/playerShip1_damage1/2/3.png` to `damageSprites[0/1/2]`
2. **Player GO** → `ShieldController` component → assign `shield1`, `shield2`, `shield3` sprites (from `Assets/Sprites/Effects/`) to `shieldFrames[0/1/2]`
3. **MeteorSpawner GO** → verify `hitParticles` field on all 20 meteor prefab entries is wired to `Explosion.prefab`
4. **EnemySpawner GO** → verify enemy prefab slots include Enemy_Blue/Black/Green/Red
5. **SpawnDirector GO** → verify boss prefab slot references `Boss.prefab`
