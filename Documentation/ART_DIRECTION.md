# NO CAUSE FOR ALARM — art direction

**1.1 update:** The current cast uses full City People models and native authored animations. School assets supplies the furniture; Vintage Living Room supplies building surfaces and doors; Poly Haven supplies the lighter. The first-person hand and priest are removed. The green/olive lighting, close framing and restrained analog treatment remain. See [the current overhaul record](GRAPHICS_OVERHAUL.md). The sections below document the historical 1.0 portrait approach and are superseded where they describe current character meshes or architecture.

The user's reference is *No, I'm not a Human*. We use broad visual qualities: uncomfortable close framing, sallow green lighting, rough photographic skin, asymmetric faces, constrained color, and an ordinary interior made oppressive. We do not use the reference game's characters, textures, scenes, logos, writing, UI artwork, or recordings.

## Environment packs actually used

- Styloo's School Classrooms Asset Pack: classroom tables and chairs, lectern, lockers, vending machine, fire extinguisher, radio and telephone. Imported meshes are in `Assets/Resources/Props`; the room assembly is in `Campus.cs`.
- Kenney Furniture Kit: office furniture, storage, shelving, bathroom and kitchen fittings, computer props and waste bins.
- Materials are remapped into the game's muted palette. Architectural pieces and navigable door frames are authored geometry, allowing consistent widths and collision checks.

The other suggested packs are alternatives or optional additions, not claimed dependencies. No account-gated asset is represented as imported.

## Original faces

Workspace asset: `Assets/Resources/Characters/FaceAtlas.png`.

Generated with the built-in image-generation tool, then copied into this repository. The twelve identities are original. The atlas uses three columns and four rows, matching the cast order in `Cast.cs`. Blender's `Tools/create_portrait.py` creates the frontal UV projection and assigns a separate material to the back of the head. Unity selects the corresponding atlas cell for each actor. The source mesh remains in `SourceAssets/Portrait.blend`.

Final generation prompt:

> Use case: stylized-concept. Asset type: a single production-ready game character face texture atlas for an original first-person psychological horror game NO CAUSE FOR ALARM. Create ONE regular atlas with exactly 3 columns and 4 rows, twelve equally sized square cells, no gutters or borders, total image portrait 3:4. Each cell contains a tightly cropped strictly straight-on front view of a different original adult human head, showing entire scalp to bottom of chin and both ears, NO neck, NO shoulders. All faces have identical framing: eyes at 43 percent of cell height from top, mouth at 73 percent, face occupying 85 percent width and 95 percent height. Plain charcoal olive background in each cell. It will be UV-mapped onto a low-poly head. Style: deliberately uncanny rough photogrammetry, distressed scanned photographs painted over with gritty charcoal texture, harsh side/top lighting, sallow muted gray-green/brown skin, natural pores, dark under-eyes, slightly asymmetric real human features, tired uncomfortable direct eye contact, uneasy mundane people, not glamorous, not cartoon, not clean 3D renders. Faint grain baked in, bruised olive and dirty cream palette. Preserve human ambiguity: no fangs, alien eyes, gore, monsters or fantasy. Make all twelve different and original. Row1: older lean male lecturer with receding gray hair and wire spectacles; stern middle-aged woman security officer with close cropped dark hair; exhausted older woman caretaker with gray hair pulled back. Row2: East Asian young adult woman engineer with uneven black fringe; Black young adult woman nursing student with close natural hair; young adult pale male IT technician with long narrow face, dark hair and thin glasses. Row3: young adult brown-skinned woman class representative with center-part hair; anxious young adult man with freckles and cropped reddish hair; young adult Korean woman with straight black bob. Row4: middle-aged Latino man cafeteria worker with thinning hair and small mustache; middle-aged woman chemistry lecturer with angular face and short auburn hair; middle-aged male maintenance worker with stubble and deep forehead creases. Avoid smiles, makeup, text, labels, logos, dividers, clothing, props, weapons, copies of any existing videogame characters. Original faces only. Output high-resolution crisp atlas suitable for direct use as a single texture.

## Presentation

The conversation camera moves close to the person, with choices at the side instead of covering the face. The campus uses olive/cream surfaces, green fluorescent light and sparse red emergency fixtures. URP adds restrained bloom, grain, contrast and desaturation. A depth-tested text shader keeps signs on their walls. A dedicated emissive shader ensures fire remains visible in the standalone build. Analog effects and camera movement can be reduced to zero.
