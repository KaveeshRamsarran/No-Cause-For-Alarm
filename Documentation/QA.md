# NO CAUSE FOR ALARM — validation record

Validated on 2026-09-16 using Unity 6000.4.7f1, Windows x64 and an NVIDIA GeForce RTX 3060.

## Results

- Windows standalone build succeeded. Packaged player size is approximately 101 MB unpacked and 38 MB zipped.
- **7,010 story assertions passed**, covering 1,000 seeds, constrained identities, generated clues, JSON roundtrips, power loss, the complete action clock and all six endings.
- **44 standalone checks passed**, including twelve physical doorway traversals, ten non-black rendered screenshots, opening-to-play transition, access gates, helper rewards, the reference and supernatural candles, the ceiling breach, containment, power repair, manifest, ending, save/continue and restart.
- A second chronological run used ordinary interactions and the action economy from 10:00 to 18:00. It collected combined evidence, confronted Venn, contained the ceiling organism, detained the infiltrators and reached the survival ending. This run did not set the clock or inject evidence.
- Native keyboard/mouse automation used the ordinary menu and opening, moved the player, ignited the lighter, paused/resumed, opened the notebook and directory, and exited. Movement and fuel consumption were read back from the actual save file.
- The final standalone harness reported **zero errors or runtime exceptions**.

Measured during the rendered test at 1280×720, medium settings: **16.67 ms median and 16.67 ms p95 frame time**, with **115 MiB Unity allocated memory**. These figures describe this machine and scripted route, not a minimum-spec guarantee or whole-system RAM measurement.

Machine-readable reports: [story](Validation/story.txt), [standalone](Validation/standalone.txt), [native input](Validation/input.txt).

Screenshots: [menu](Screenshots/menu.png), [conversation](Screenshots/conversation.png), [classroom](Screenshots/classroom.png).

## Issues found and corrected

World text was initially mirrored, too large and drawn through walls. A dedicated depth-tested shader and corrected scale/orientation fixed it. The first portrait pass lost the FBX axis rotation; retaining the imported transform fixed the head projection. Flame emission now uses a dedicated shader to avoid runtime keyword stripping. A candle was moved away from a chair. NPC paths now route through doorway centers and around classroom furniture. Escape handling was centralized after native input testing found immediate re-pausing. Ambient ballast audio now follows SFX volume and power state.

## Remaining limits

- The 45–90 minute initial-playthrough target has not been measured with human players. The test harness accelerates candle waiting and moves directly between some interactions; it is not a timing study or a substitute for subjective horror/balance testing.
- Character acting is procedural, and conversations are written rather than fully voiced. PA announcements are synthesized speech.
- CCTV presents authored archive observations beside a live reference view. It is not a collection of fully animated recorded sequences.
- Saving preserves investigation state but resets transient pursuers and candle animation on continue.
- URP may report that it reduces punctual shadow resolution to fit the shadow atlas. The renderer handles this automatically; it did not prevent play or fail the performance checks.
- There is no rebinding screen or broad hardware/display compatibility matrix yet.

The deliverable is a complete playable investigation loop with a Windows build. These limits remain visible rather than being represented as commercial-release QA.
