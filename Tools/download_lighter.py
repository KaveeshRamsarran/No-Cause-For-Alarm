"""Download the CC0 Vintage Lighter through Poly Haven's public asset API."""
from pathlib import Path
import json,urllib.request,hashlib
root=Path(__file__).resolve().parents[1]/'SourceAssets/Overhaul/lighter'
root.mkdir(parents=True,exist_ok=True)
request=urllib.request.Request('https://api.polyhaven.com/files/vintage_lighter',headers={'User-Agent':'NCFA-Asset-Setup/1.0'})
data=json.load(urllib.request.urlopen(request))
model=data['blend']['1k']['blend']
for name,info in {'vintage_lighter.blend':model,**model['include']}.items():
    target=(root/name).resolve()
    if not target.is_relative_to(root.resolve()):raise ValueError('Unsafe asset path')
    target.parent.mkdir(parents=True,exist_ok=True)
    contents=urllib.request.urlopen(info['url']).read()
    if hashlib.md5(contents).hexdigest()!=info['md5']:raise ValueError('Asset checksum mismatch')
    target.write_bytes(contents)
print('Downloaded Poly Haven Vintage Lighter, CC0: https://polyhaven.com/a/vintage_lighter')
