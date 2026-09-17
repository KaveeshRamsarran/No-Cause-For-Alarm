"""Pose the CC0 WRAD hand for the first-person lighter."""
import bpy, bmesh, pathlib, math
from mathutils import Vector, Matrix
root=pathlib.Path(__file__).resolve().parents[1]
source=root/'SourceAssets/Overhaul'
# Retain the detailed imported hand topology and texture, pose its fingers into a lighter grip.
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath=str(source/'hands/arms.fbx'))
rig=next(o for o in bpy.context.scene.objects if o.type=='ARMATURE')
for b in rig.pose.bones:
 if b.name.startswith('finger_') and '.r' in b.name and '_end' not in b.name:
  b.rotation_mode='XYZ';b.rotation_euler.x=math.radians(38 if 'thumb' in b.name else 55)
bpy.context.view_layer.update()
obj=next(o for o in bpy.context.scene.objects if o.type=='MESH')
deps=bpy.context.evaluated_depsgraph_get()
mesh=bpy.data.meshes.new_from_object(obj.evaluated_get(deps),depsgraph=deps)
grip=bpy.data.objects.new('WRAD right hand / lighter grip',mesh);bpy.context.collection.objects.link(grip)
grip.matrix_world=obj.matrix_world.copy()
bm=bmesh.new();bm.from_mesh(mesh)
bmesh.ops.delete(bm,geom=[v for v in bm.verts if (grip.matrix_world@v.co).x<3.2],context='VERTS')
bm.to_mesh(mesh);bm.free()
wrist=rig.matrix_world@rig.data.bones['wrist.r'].head_local
up=(rig.matrix_world@rig.data.bones['finger_middle1.r'].tail_local-wrist).normalized()
right=(rig.matrix_world@rig.data.bones['finger_pinky1.r'].head_local-rig.matrix_world@rig.data.bones['finger_index1.r'].head_local).normalized()
forward=right.cross(up).normalized();right=up.cross(forward).normalized()
for v in mesh.vertices:
 p=grip.matrix_world@v.co-wrist
 # Blender z -> Unity y. Unity front is -Blender y.
 v.co=Vector((p.dot(right),-p.dot(forward),p.dot(up)))*.082
grip.matrix_world=Matrix.Identity(4)
for o in list(bpy.context.scene.objects):
 if o!=grip: bpy.data.objects.remove(o,do_unlink=True)
out=root/'Assets/Art/Hands';out.mkdir(parents=True,exist_ok=True)
bpy.ops.export_scene.fbx(filepath=str(out/'LighterGrip.fbx'),add_leaf_bones=False,bake_anim=False,axis_forward='-Z',axis_up='Y')
bpy.ops.wm.save_as_mainfile(filepath=str(root/'SourceAssets/LighterGrip.blend'))
print('OVERHAUL_CC0_MODELS_READY')

