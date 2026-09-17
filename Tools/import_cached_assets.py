"""Restore selected owned Asset Store art locally; source assets are git-ignored."""
import tarfile, pathlib, json, gzip, tempfile, shutil, sys
root = pathlib.Path(__file__).resolve().parents[1]
cache = pathlib.Path.home() / 'AppData/Roaming/Unity/Asset Store-5.x'
out = root / 'Assets/LocalLicensed'
out.mkdir(parents=True, exist_ok=True)
records=[]
packages=list(cache.rglob('*.unitypackage'))
required=sys.argv[1:] or ['VINTAGE LIVING','City People FREE','School assets']
missing=[name for name in required if not any(name in p.name for p in packages)]
if missing: raise SystemExit('Download these free packs in Unity My Assets first: '+', '.join(missing))
for package in packages:
    if not any(k in package.name for k in required): continue
    with tempfile.TemporaryFile() as raw:
        with gzip.open(package,'rb') as gz: shutil.copyfileobj(gz,raw)
        raw.seek(0)
        archive=tarfile.open(fileobj=raw,mode='r:')
        members={m.name:m for m in archive.getmembers()}
        for member in archive.getmembers():
            if not member.name.endswith('/pathname'): continue
            path=archive.extractfile(member).read().decode('utf8').split('\n')[0].strip('\x00\r')
            prefix=member.name.rsplit('/',1)[0]
            if prefix+'/asset' not in members: continue
            original=pathlib.PurePosixPath(path)
            if original.suffix.lower() not in ('.fbx','.png','.mat','.prefab','.anim','.controller','.txt'): continue
            if '/Demo/' in path or '/Demo_Scenes/' in path: continue
            pack='School' if 'School assets' in package.name else 'CityPeople' if 'City People' in package.name else 'Vintage'
            relative=path.split('CityPeople/',1)[-1] if pack=='CityPeople' else str(pathlib.Path(*original.parts[2:]))
            target=(out / pack / relative).resolve()
            if not target.is_relative_to(out.resolve()): raise ValueError('Unsafe path')
            target.parent.mkdir(parents=True,exist_ok=True)
            target.write_bytes(archive.extractfile(members[prefix+'/asset']).read())
            if prefix+'/asset.meta' in members:
                target.with_name(target.name+'.meta').write_bytes(archive.extractfile(members[prefix+'/asset.meta']).read())
            records.append({'package':package.name,'source':path,'local':str(target.relative_to(root))})
(root/'Artifacts/overhaul').mkdir(parents=True,exist_ok=True)
(root/'Artifacts/overhaul/imports.json').write_text(json.dumps(records,indent=2))
print('Imported',len(records),'owned art files locally.')
