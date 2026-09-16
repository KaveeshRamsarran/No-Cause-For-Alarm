"""Run with Blender --background --python Tools/create_portrait.py."""
import bpy
from pathlib import Path
root=Path(__file__).resolve().parents[1]
bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete(use_global=False)
bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=3, radius=1)
head=bpy.context.object
head.name='Portrait / faceted human base'
for vert in head.data.vertices:
    x,y,z=vert.co
    # Forward is -Y in Blender; keep brow broad, taper lower jaw.
    width= .84 if z>-.25 else .69+max(0,z+.9)*.22
    vert.co.x=x*width
    vert.co.y=y*.79
    vert.co.z=z*1.04
    if y<-.5 and -.55<z<.45: vert.co.y=-.72+(z-.12)**2*.15
for polygon in head.data.polygons: polygon.use_smooth=False
front=bpy.data.materials.new('Portrait atlas front')
back=bpy.data.materials.new('Dark hair and rear skull')
head.data.materials.append(front);head.data.materials.append(back)
uv=head.data.uv_layers.active or head.data.uv_layers.new(name='Frontal portrait projection')
for polygon in head.data.polygons:
    polygon.material_index=0 if sum(head.data.vertices[i].co.y for i in polygon.vertices)/len(polygon.vertices)<-.02 else 1
    for loop_index in polygon.loop_indices:
        vertex=head.data.vertices[head.data.loops[loop_index].vertex_index].co
        uv.data[loop_index].uv=(max(.01,min(.99,vertex.x/1.68+.5)),max(.01,min(.99,vertex.z/2.08+.5)))
out=root/'Assets/Resources/Characters'
out.mkdir(parents=True,exist_ok=True)
bpy.ops.wm.save_as_mainfile(filepath=str(root/'SourceAssets/Portrait.blend'))
bpy.ops.export_scene.fbx(filepath=str(out/'portrait.fbx'),use_selection=True,axis_forward='-Z',axis_up='Y',apply_unit_scale=True,bake_anim=False)
print('Original portrait exported')
