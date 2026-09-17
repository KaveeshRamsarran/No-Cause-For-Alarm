"""Download an explicitly free itch.io upload through its public download flow."""
import sys, pathlib, re, json, urllib.request, urllib.parse, http.cookiejar, zipfile
url, name = sys.argv[1:3]
root = pathlib.Path(__file__).resolve().parents[1] / 'SourceAssets' / 'Overhaul'
root.mkdir(parents=True, exist_ok=True)
opener = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(http.cookiejar.CookieJar()))
page = opener.open(url + '/purchase').read().decode()
token = re.search(r'name="csrf_token" value="([^"]+)', page).group(1)
def post(endpoint, token):
    return json.loads(opener.open(urllib.request.Request(endpoint, data=urllib.parse.urlencode({'csrf_token':token}).encode(), headers={'Referer':url})).read())
download = post(url + '/download_url', token)
page = opener.open(download['url']).read().decode()
(root / (name + '-download.html')).write_text(page, encoding='utf8')
token = re.search(r'name="csrf_token" value="([^"]+)', page).group(1)
ids = re.findall(r'data-upload_id="(\d+)"', page)
if not ids: raise RuntimeError('No free download found')
asset = post(url + '/file/' + ids[0] + '?source=game_download', token)
archive = root / (name + '.zip')
archive.write_bytes(opener.open(asset['url']).read())
destination = (root / name).resolve()
with zipfile.ZipFile(archive) as z:
    for member in z.infolist():
        if not (destination / member.filename).resolve().is_relative_to(destination): raise ValueError('Unsafe path')
    z.extractall(destination)
print(name, archive.stat().st_size, 'bytes', len(ids), 'available free upload(s)')
