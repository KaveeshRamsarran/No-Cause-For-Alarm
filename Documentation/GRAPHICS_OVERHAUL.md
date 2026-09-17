# Graphics overhaul — 1.1

The cast now uses **City People FREE Samples**, the pack from the owner's Unity account. Twelve cast assignments use nine distinct adult meshes, with palette variants for repeated models. The security officer, caretaker, academics and maintenance worker have role-appropriate models. The ceiling creature uses a stretched and distorted City People model, keeping it consistent with the campus cast.

Humanoid avatars drive real skinned meshes. City People's male/female idle, walking and jogging clips blend with alternate idle poses during conversation and the pack’s self-check gesture for uneasy reactions. Movement remains controlled by the existing navigation and collision system; animation root motion cannot move a character through a door. Animation pauses with menus, and the original investigation rules remain intact.

The first-person hand comes from **WRAD ARMS**. Its original textured topology is posed in Blender for the lighter, with wrist movement, raising/lowering and a moving lighter lid. The supplied MP3 is divided into ten brief footfall variations, selected without consecutive repeats and timed to walking/sprinting. This prevents a full walking recording from continuing after a single step.

The building uses **Vintage Living Room** wall, floor, ceiling, window, door leaf and frame meshes, converted from HDRP materials to URP. Offices also use its shelves, curtains, lamp and sofa. These visual modules cover the existing collision shell so the campus layout and access gates are preserved. The school furniture from Styloo and Kenney remains. The lectern has been reduced to classroom scale. Emergency sconces have a wall-facing local frame instead of a wide ceiling-light mesh intersecting the partitions.

## Assets and use rights

No paid assets were purchased or downloaded.

| Asset | Source / license | Distribution |
|---|---|---|
| City People FREE Samples — Denys Almaral | [Official free listing](https://assetstore.unity.com/packages/3d/characters/city-people-free-samples-260446), Standard Unity Asset Store EULA | Used in the game; raw pack and derived prefabs remain local |
| Vintage Living Room — ZNS3D | [Official free listing](https://assetstore.unity.com/packages/3d/environments/vintage-living-room-3d-game-pack-314464), Standard Unity Asset Store EULA | Used in the game; raw pack and derived prefabs remain local |
| WRAD ARMS — wriks | [Creator page](https://wriks.itch.io/wrad-arms), CC0 | Adapted hand mesh, texture and license included |
| Walk On Concrete - Sound Effect for editing.mp3 | Supplied by project owner, who confirmed “it is free to use” on 2026-09-17 | Included as derived footfall samples in the local/game build; source audio is not redistributed in the repository |

The MP3's use is based on the owner's confirmation, not an independently verified CC0 license. A matching Sound Library upload is at https://www.youtube.com/watch?v=yHDh_GDHKKI; its description does not establish the original recording's creator or exact license. Keep the owner's source/license record with release documentation.

The [Unity Asset Store EULA](https://unity.com/legal/as-terms) permits incorporated game use under its conditions and does not turn free Store downloads into freely redistributable source packs. Accordingly, `Assets/LocalLicensed` and `Assets/Resources/LocalLicensed` are excluded from this public repository. Screenshots and the compiled game can show the integrated art. The original CC0 Styloo/Kenney credits remain in `ASSET_CREDITS.txt`.

## Restore the graphics on another workstation

1. Add/download the two free Unity Store packs above in Unity's **My Assets** using your own account.
2. Install Python dependencies with `python -m pip install numpy scipy soundfile`.
3. Run `powershell -ExecutionPolicy Bypass -File Tools/setup_graphics.ps1`. This imports the cached packs, downloads only the free CC0 hand archive if required, poses the hand with Blender, and prepares URP prefabs and humanoid clips.
4. To use the owner's recording locally, run `python Tools/prepare_footsteps.py "PATH/Walk On Concrete - Sound Effect for editing.mp3"`. Without these local samples the existing original synthesized step remains a fallback.
5. Build with `Tools/build.ps1`. Open `Assets/Scenes/EastWing.unity` to play in the editor.

`Tools/graphics-test.ps1` runs the opt-in graphics review. It uses a separate save, checks imported meshes/avatars/animation movement and records screenshots. `Tools/smoke-test.ps1` exercises the complete investigation and door traversal after the overhaul.
