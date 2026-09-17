# NO CAUSE FOR ALARM - 1.2 validation

Validated on 2026-09-17 using Unity 6000.4.7f1, Windows x64 and an NVIDIA GeForce RTX 3060.

## Checks

- Windows build succeeds; 7,026 story assertions cover 1,000 seeds and all six endings.
- 108 standalone integration checks cover twelve doorway traversals, the opening, locked rooms, evidence, both candles, ceiling encounter/containment, power repair, evacuation, ending, save/load and restart.
- The integration run checks all nine physical supplies, inventory persistence, item consumption, successful help deliveries and duplicate-reward prevention. A second chronological investigation reaches the survival ending through ordinary actions without setting the clock or injecting clues.
- 319 graphics checks cover imported humanoid avatars, authored animation movement, natural arm poses, requested School assets usage, correct chair backrests, podium dimensions, all seventeen emergency sconces, supply support surfaces and furniture/wall clearance.
- Exclusive fullscreen, borderless windowed and windowed modes are applied in the standalone player and verified through Screen.fullScreenMode. Each selection survives Preferences.Save/Load.
- The hand and priest are absent from game Resources. The lighter is the actual Poly Haven mesh with moving lid and animated flame. Candles share the animated flame effect.

- Native keyboard/mouse input passed: moved 3.87 metres, consumed lighter fuel, paused/resumed, opened the notebook and directory, then quit. The existing user save and its backup were restored after testing. Desktop test drivers explicitly activate the player before keyboard/mouse and fullscreen checks.
- The Windows package is 49.4 MB compressed (approximately 127 MB player data), with an integrity-checked ZIP and SHA-256 recorded in Distribution/build-info.json.

Reports: [story](Validation/story.txt), [graphics and display](Validation/graphics.txt), [standalone gameplay](Validation/standalone.txt), [native input](Validation/input.txt).

Visual review images: [classroom](Screenshots/classroom.png), [bathroom](Screenshots/bathroom.png), [lighter](Screenshots/lighter.png), [candle](Screenshots/candle.png), [requests](Screenshots/requests.png), [settings](Screenshots/settings.png), [clock](Screenshots/wall-clock.png), [manifest](Screenshots/manifest.png), [ending](Screenshots/ending-the-last-bus.png).

The 1.2 graphics run also checks all twelve home positions against furniture, all twelve cached cast portraits, three working stalls and flat wall clocks. The six ending reports and portrait manifest were visually inspected in the player.

## Issues corrected

Removed the mismatched priest and the first-person hand at the owner's request. Cross-pack acting was replaced with City People's own clips. The requested school pack now supplies furniture; chairs face desks and toilets face the stall entrances. Fire cabinets and fuse/damper controls are mounted to walls, windows no longer overlap older window panels, emergency lights clear partitions, and clues/supplies sit on desks or counters. Introductory actors stand in clear aisles. The evidence list scrolls when the new clues exceed its visible area.

Version 1.2 adds teacher work areas, storage, projector fittings, books, computer seating and secondary workstations. The bathroom notice is wall mounted and cubicles have working doors. Stall leaves swing outward to leave room for closing from inside. Door locks permit closing and emergency exit; both operating plates remain reachable when leaves are open. Player clearance pauses a moving leaf, and NPC schedules respect manual closure. Passenger selections persist after case-note review and save/load. Endings report concrete outcomes and account for humans excluded from boarding.

## Limits

The 45-90 minute first-playthrough target still needs human pacing tests. Scripted checks accelerate candle waiting and move between some interactions. Conversations are written; PA announcements use local synthesized speech. Tests cover this Windows machine rather than a broad hardware matrix. URP can reduce shadow atlas resolution automatically. A graphics-driver timestamp warning was observed during display switching; the mode checks completed successfully. Saving resets transient pursuers and candle timing.

Free Asset Store art is incorporated in the build but excluded from public raw source. Follow GRAPHICS_OVERHAUL.md to restore it on another workstation. The supplied footstep recording is used on the owner's confirmation of free use.
