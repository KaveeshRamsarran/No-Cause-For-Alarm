import bpy
from pathlib import Path
root=Path.cwd()
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath=str(root/'Assets/Resources/Props/schoolTable.fbx'))
for image in bpy.data.images:
 if image.packed_file:
  image.filepath_raw=str(root/'Assets/Resources/Props/SchoolPalette.png');image.file_format='PNG';image.save();print('Extracted original embedded palette',image.name)
