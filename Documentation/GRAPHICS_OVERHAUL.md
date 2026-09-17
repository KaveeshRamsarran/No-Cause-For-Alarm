# Graphics and interaction update - 1.2

The twelve cast assignments use nine adult meshes from **City People FREE Samples**, with native male/female idle, walk, jog and gesture clips blended onto humanoid avatars. The ceiling creature is a distorted City People character. The priest and first-person hand have been removed from the game and build resources.

The player's lighter is **Vintage Lighter by Slinc / Poly Haven**. It has textured metal surfaces, an opening lid and an animated flame with a blue base, warm core, soft edges and a wavering tip. Calibration candles use the same animated effect. The supplied concrete recording provides ten short footfall variations; walking and sprinting choose different timing and volume.

## Furniture and placement

**School assets by A.R.S|T.**, the exact requested pack, supplies desks, chairs, cafeteria tables and attached stools, sink counters, display counters, lockers, shelving, boards, books, computer props and extinguishers. Chair backrests are normalized from their actual mesh geometry so seats face the desks. The pack has no toilet model; the existing CC0 Kenney toilets remain and have been turned to face the stall entrances. Supplemental clocks, bins, telephone, radio and vending-machine props remain from the earlier CC0 packs.

Vintage Living Room supplies walls, floors, ceilings, windows, doors/frames and curtains. Its office furniture has been replaced with the school pack. The classroom lectern is now a compact school desk at a sensible teaching height. Emergency sconces face into the room and clear the wall. Clues and supplies sit on real furniture; overlapping evidence pedestals were removed. Wall controls have readable labels and mounts.

Version 1.2 expands teaching desks, storage racks, books, worksheets, ceiling projectors, computer seating and secondary work areas using the same free packs. Clocks are oriented and centered against the wall. Three numbered bathroom cubicles now have working imported door leaves and latches. Door operating plates remain reachable from both sides; locks never prevent closing or emergency exit. A manually closed door stays closed when an NPC reaches it.

## Play changes

- The evacuation checklist includes cached portraits rendered from the actual cast models, their roles, availability and player-assigned trust tags. Passenger choices survive reopening the checklist and save/load.
- Final reports describe the causal outcome and list passengers, exclusions, containment and innocent losses. The historical branch only refers to the recovered recording when collected. Leaving three or more available humans behind counts toward A Clean Register.
- Settings offer exclusive fullscreen, borderless windowed and ordinary windowed display modes. Apply and Return saves the selection.
- Characters have individual greetings, request reminders and specific responses when an item is delivered.
- Nine physical supplies can be picked up, carried, saved and delivered: radio batteries, a key sign-out book, a fuse, sample kit, USB drive, inhaler, can opener, cassette and insulated gloves.
- Help requests grant access, evidence, repairs or health. Supplies disappear when collected and are consumed on delivery. Repeating a completed request cannot duplicate its reward.
- The notebook has a Supplies / Requests page. Asking what someone needs and picking up supplies are free; completing a request uses one action. Evidence-based requests still require the matching documents.

## Free assets and provenance

| Asset | Source and terms |
|---|---|
| City People FREE Samples - Denys Almaral | https://assetstore.unity.com/packages/3d/characters/city-people-free-samples-260446 - free, Standard Unity Asset Store EULA |
| School assets - A.R.S\|T. | https://assetstore.unity.com/packages/3d/environments/school-assets-146253 - free, Standard Unity Asset Store EULA |
| Vintage Living Room - ZNS3D | https://assetstore.unity.com/packages/3d/environments/vintage-living-room-3d-game-pack-314464 - free, Standard Unity Asset Store EULA |
| Vintage Lighter - Slinc / Poly Haven | https://polyhaven.com/a/vintage_lighter - CC0, https://polyhaven.com/license |
| Styloo School Classrooms / Kenney Furniture | Existing CC0 supplemental props; see ASSET_CREDITS.txt |
| Supplied concrete footstep MP3 | Project owner confirmed free use on 2026-09-17. This is owner-provided permission, not an independently verified CC0 license. |

No paid assets were purchased. Asset Store packs are free to acquire under their EULA; their raw files and derived prefabs/clips remain local in ignored LocalLicensed folders. The compiled game includes the incorporated artwork. The CC0 lighter mesh, textures and attribution are included in source control. Owner-supplied audio sources and derived samples stay local but are incorporated into the game build.

## Restore the graphics on another workstation

1. Download the three free Unity Store packs above through your account's My Assets.
2. Install Unity 6000.4.7f1, Blender, and Python with `numpy scipy soundfile`.
3. Run `powershell -ExecutionPolicy Bypass -File Tools/setup_graphics.ps1`. This imports cached art, downloads the CC0 lighter if necessary, converts it in Blender and prepares Unity materials/prefabs/animations.
4. To restore the supplied recording, run `python Tools/prepare_footsteps.py "PATH/Walk On Concrete - Sound Effect for editing.mp3"`. Otherwise footsteps use the original synthesized fallback.
5. Run `Tools/build.ps1`, or open `Assets/Scenes/EastWing.unity` in Unity.

The downloadable Windows build is already prepared. `Tools/graphics-test.ps1` checks character animations and graphics placements, and captures review images. `Tools/smoke-test.ps1` checks the investigation, deliveries, save/load and door traversal. Both use separate test saves.
