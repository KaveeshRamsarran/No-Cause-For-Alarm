"""Package the tested player without debug-symbol folders; include player instructions."""
from pathlib import Path
from zipfile import ZipFile, ZIP_DEFLATED
import hashlib, json
root=Path(__file__).resolve().parents[1]
build=root/'Builds/Windows'
output=root/'Distribution'
output.mkdir(exist_ok=True)
archive=output/'NO-CAUSE-FOR-ALARM-Windows.zip'
with ZipFile(archive,'w',ZIP_DEFLATED,compresslevel=9) as z:
    for file in sorted(build.rglob('*')):
        if file.is_file() and not any('DoNotShip' in p for p in file.parts):
            z.write(file,Path('NO CAUSE FOR ALARM')/file.relative_to(build))
    for name in ['README.txt','ASSET_CREDITS.txt']:
        z.write(root/name,Path('NO CAUSE FOR ALARM')/name)
with ZipFile(archive) as z:
    assert z.testzip() is None
    assert 'NO CAUSE FOR ALARM/NO CAUSE FOR ALARM.exe' in z.namelist()
    assert 'NO CAUSE FOR ALARM/UnityPlayer.dll' in z.namelist()
data=archive.read_bytes()
metadata={'title':'NO CAUSE FOR ALARM','version':'1.0.0','platform':'Windows x64','unity':'6000.4.7f1','file':archive.name,'bytes':len(data),'sha256':hashlib.sha256(data).hexdigest()}
(output/'build-info.json').write_text(json.dumps(metadata,indent=2)+'\n')
print(json.dumps(metadata,indent=2))
