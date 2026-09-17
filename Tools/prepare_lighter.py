"""Convert Poly Haven's CC0 Vintage Lighter to Unity, preserving the lid pivot."""
from pathlib import Path
import bpy
import numpy as np
root=Path(__file__).resolve().parents[1]
source=root/'SourceAssets/Overhaul/lighter'
out=root/'Assets/Art/Lighter'
out.mkdir(parents=True,exist_ok=True)
bpy.ops.wm.open_mainfile(filepath=str(source/'vintage_lighter.blend'))
for image in bpy.data.images:
    if image.filepath:
        image.filepath=str(source/'textures'/Path(image.filepath.replace('\\','/')).name)
        image.reload()
def pixels(name):
    key=name.replace('vintage_lighter_','').replace('.png','')
    extension='jpg' if key=='diff' else 'png' if key=='alpha' else 'exr'
    image=bpy.data.images.load(str(source/'textures'/('vintage_lighter_'+key+'_1k.'+extension)),check_existing=False)
    return np.array(image.pixels[:],dtype=np.float32).reshape(-1,4),tuple(image.size)
def save(name,data,size):
    image=bpy.data.images.new(name,width=size[0],height=size[1],alpha=True)
    image.pixels.foreach_set(data.reshape(-1))
    image.filepath_raw=str(out/(name+'.png'));image.file_format='PNG';image.save()
diff,size=pixels('vintage_lighter_diff.png')
alpha,_=pixels('vintage_lighter_alpha.png')
diff[:,3]=alpha[:,0]
save('LighterBase',diff,size)
metal,size=pixels('vintage_lighter_metal.png')
rough,_=pixels('vintage_lighter_rough.png')
metal[:,1:3]=0;metal[:,3]=1-rough[:,0]
save('LighterMetalSmoothness',metal,size)
normal,size=pixels('vintage_lighter_nor_gl.png')
save('LighterNormal',normal,size)
for o in bpy.data.objects:
    if o.type=='MESH':
        print(o.name,'rotation',tuple(o.rotation_euler),'bounds',[(round(min((o.matrix_world@__import__('mathutils').Vector(c))[i] for c in o.bound_box),4),round(max((o.matrix_world@__import__('mathutils').Vector(c))[i] for c in o.bound_box),4)) for i in range(3)])
bpy.data.objects['vintage_lighter_hinge'].rotation_euler.y=0
bpy.ops.export_scene.fbx(filepath=str(out/'VintageLighter.fbx'),object_types={'MESH'},add_leaf_bones=False,bake_anim=False,axis_forward='-Z',axis_up='Y')
for material in bpy.data.materials:
    if material.node_tree:
        for node in material.node_tree.nodes:
            if node.type=='TEX_IMAGE' and node.image:
                key=node.image.name.split('.')[0].replace('vintage_lighter_','')
                extension='jpg' if key=='diff' else 'png' if key=='alpha' else 'exr'
                path=source/'textures'/('vintage_lighter_'+key+'_1k.'+extension)
                if path.exists():node.image=bpy.data.images.load(str(path),check_existing=False);node.image.pack()
bpy.ops.wm.save_as_mainfile(filepath=str(root/'SourceAssets/VintageLighter.blend'))
print('VINTAGE_LIGHTER_READY')
