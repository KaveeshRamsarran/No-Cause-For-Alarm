# NO CAUSE FOR ALARM — development log

## 2026-09-16 / production pass

Started from the repository's single README. Created a Unity 6000.4.7f1 project and configured URP 17.4.0, linear rendering, Windows x64, legacy keyboard/mouse input, and Git exclusions for generated assets.

Implemented twelve original characters, constrained three-infiltrator randomization, generated evidence, notebook observations and subjective tags, four-action hours, detention consequences, six endings, the lecturer's historical reveal, a passenger manifest, and atomic-save replacement with backup. Built first-person interaction, doors and keys, a fueled lighter, two calibrated candle tests, a ceiling breach, a short defensive chase, camera access, power repair, and the extraction damper.

Created a twelve-room campus from authored geometry and CC0 Styloo/Kenney assets. Generated an original portrait mesh in Blender and original synthetic ambience/SFX. Added local synthesized PA dialogue, captions, URP grading/bloom/grain, and adjustable camera motion/effects.

Added title/opening screens, main/pause menus, save/continue, dossier notebook, campus directory, settings, credits, restart, and ending reports. The development build includes an opt-in standalone test harness; it uses a separate save slot so it cannot overwrite a player's case.

## Decisions

- Runtime campus construction keeps the authored layout and prop placement in reviewable C#.
- Original procedural characters and animations avoid relying on account-gated character/animation downloads.
- Imported art retains source files and provenance; palette changes happen at runtime.
- Camera evidence uses authored archive notes beside a real reference camera view. It does not claim the live view is the recorded anomaly.
- Brief alibi/witness questions are free; costly observations, assistance and decisions advance the day.
- Save files preserve the investigation state. Short-lived candle animations and pursuits are reset on continue.

## Build issues resolved during development

- Unity 6.4's URP additional-lights mode property is read-only; use the supported pipeline defaults.
- Unity's package API updater migrated old GUID API uses in cached Shader Graph source.
- A build attempted during a script change had incompatible editor/player class layouts. Subsequent validation freezes runtime source for the duration of each build.

## Verification and remaining limits

The validation report and final build results are recorded in Documentation/QA.md after standalone testing.

## Requested aesthetic refinement

Added an original twelve-person photographic face atlas using the built-in image-generation tool. Mapped it onto the Blender mesh, corrected the FBX's preserved axis rotation, and gave exposed mimics the corresponding person's distorted face and clothing. Moved dialogue choices beside the person, restored Styloo's embedded palette texture, added selective shadows, and strengthened the green/olive grade. Full prompt and implementation notes: Documentation/ART_DIRECTION.md.

Visual QA found and fixed mirrored/oversized world signs, text rendering through walls, portrait camera framing, head import rotation, flame shader stripping, and an overlapping candle/chair. Native input QA found and fixed double Escape handling that prevented pause/resume. All twelve room doorways were traversed by the actual CharacterController. A second integration run uses ordinary actions from 10:00 through the final manifest, without injecting clues or setting the clock.

The requested 45–90 minute first-playthrough target requires human pacing tests. Procedural acting is deliberately limited; there is no full voice cast, rebinding UI, or commercial character-animation package. The complete loop is the implementation priority. These limitations should not be confused with measured duration or a claim of commercial-release QA.
