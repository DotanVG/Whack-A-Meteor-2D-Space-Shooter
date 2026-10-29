# Phase 2 manual acceptance checklist

Automated evidence on Atlas: Unity 2022.3.29f1 batch compile has zero `error CS` lines;
Game + HyperdriveShop and 42 prefabs pass missing-script/reference checks. EditMode:
86 passed / 0 failed / 0 skipped. Gate v2: 76 C# scripts, 527 metas, 49 folders,
zero errors; all 31 checker self-tests pass. Local evidence lives in the Phase 2 run's
`qa` folder and is summarized in its REPORT.md. This is not a play-feel sign-off.

1. Start New Game. During the three-second countdown, no encounter should spawn or
   deal damage. On launch the HUD should show Copper Belt / Wave 1 / a 25-second clock.
2. Pause mid-wave, wait, then resume. The wave clock, spawning and attacks should resume
   from the paused state. Survive 25 seconds: see +52 Stardust / +8 Metal, then 3s regroup.
   Existing hazards may remain during regroup; these are survival waves.
3. Compare swift tiny shards, brown splitting boulders and tinted gray armored ore.
   At wave one, projectiles take 1/3/5 hits respectively; hammer damage starts at 2.
   Destroy ore and check the extra 4 Metal. Split children should travel with momentum.
4. Reach wave 2: red aimed shooters fire toward the ship; green divers turn briefly then
   commit to a pass. Escaped ships should leave the encounter rather than accumulate.
5. Reach wave 5: see ELITE, a larger 25-HP ship, and no ordinary enemy spawning that wave.
   Kill it for 46 Metal bounty plus the normal wave-survival reward. Let one escape in
   another run: it must not grant a kill bounty. Wave 6 changes to Raider Crossing;
   wave 11 changes to Iron Storm. Watch the different mixes and continued difficulty ramp.
6. Open HyperdriveShop. Scroll by mouse wheel and vertical keyboard/gamepad navigation.
   All later rows must be reachable, with names, effect text, costs and prerequisites.
   Original rows retain their order and costs. Buy a new upgrade, restart, verify ownership.
7. Buy Piercing Rounds, Twin Shots and Critical Rounds after their prerequisites. Test
   manual and auto fire: three distinct targets per piercing projectile, two diverging
   shots without recursive multiplication, occasional yellow double-damage labels.
   Original Projectile Speed should now give actual speed 65 instead of 50 when owned.
8. Buy Heavy Impact: hammer damage becomes 3. Buy Long Arc after maxing Lightning:
   arcs can reach 9 world units instead of 6. Check short rings/flashes and readable damage
   labels without persistent line objects or excessive screen shake.
9. Buy Shield Pulse: consuming a shield charge emits a blue radius-3 ring and 2 damage to
   nearby targets, once per charge. Multiple hits inside the absorption window should
   not consume extra charges or bypass that protection. Buy Salvage Magnet: pickups
   within radius 4 move toward the ship and apply exactly once.
10. With a backed-up old save, verify migration, purchase/relaunch, then intentional reset.
    New skills begin unowned. Reset must not resurrect old purchases after relaunch.
    Verify a release build ignores F1-F4 development cheats. Test actual WebGL hosting,
    input, sound and performance in a browser and on intended devices before main.

NOT TESTED by this headless run: graphical layout/click-through, actual collision-driven
pulse/twin instantiation, play feel, audio, real old saves, browser/itch hosting, devices,
touch and performance/frame-time behavior. No device/emulator test was started.
