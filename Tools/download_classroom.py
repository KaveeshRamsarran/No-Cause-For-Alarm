"""Retrieve the creator's publicly offered free CC0 classroom archive."""
import urllib.request, urllib.parse, http.cookiejar, re, json, pathlib, zipfile
root = pathlib.Path(__file__).resolve().parents[1]
opener = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(http.cookiejar.CookieJar()))
url = 'https://styloo.itch.io/classroom-asset-pack'
page = opener.open(url + '/purchase').read().decode()
token = re.search(r'name="csrf_token" value="([^"]+)', page).group(1)
def post(endpoint, token):
    request = urllib.request.Request(endpoint, data=urllib.parse.urlencode({'csrf_token': token}).encode(), headers={'Referer': url})
    return json.loads(opener.open(request).read())
download = post(url + '/download_url', token)
page = opener.open(download['url']).read().decode()
token = re.search(r'name="csrf_token" value="([^"]+)', page).group(1)
asset = post(url + '/file/13229322?source=game_download', token)
archive = root / 'SourceAssets/classroom.zip'
archive.write_bytes(opener.open(asset['url']).read())
destination = (root / 'SourceAssets/Classroom').resolve()
with zipfile.ZipFile(archive) as z:
    for member in z.infolist():
        target = (destination / member.filename).resolve()
        if not target.is_relative_to(destination):
            raise ValueError('Archive member outside extraction folder')
    z.extractall(destination)
print('CC0 classroom pack downloaded and extracted.')
