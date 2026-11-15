"""Add a release to manifest.json (Jellyfin plugin repository format)."""
import hashlib, json, os, sys
from datetime import datetime, timezone

zip_path, version, repo, tag = sys.argv[1:5]
meta = json.load(open("meta.json"))
checksum = hashlib.md5(open(zip_path, "rb").read()).hexdigest()

try:
    manifest = json.load(open("manifest.json"))
except (FileNotFoundError, json.JSONDecodeError):
    manifest = []

plugin = next((p for p in manifest if p["guid"] == meta["guid"]), None)
if plugin is None:
    plugin = {"versions": []}
    manifest.append(plugin)

plugin.update({
    "guid": meta["guid"],
    "name": meta["name"],
    "description": meta["description"],
    "overview": meta["overview"],
    "owner": meta["owner"],
    "category": meta["category"],
})

plugin["versions"] = [v for v in plugin["versions"] if v["version"] != version]
plugin["versions"].insert(0, {
    "version": version,
    "changelog": os.environ.get("CHANGELOG", meta.get("changelog", "")),
    "targetAbi": meta["targetAbi"],
    "sourceUrl": f"https://github.com/{repo}/releases/download/{tag}/{os.path.basename(zip_path)}",
    "checksum": checksum,
    "timestamp": datetime.now(timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ"),
})

json.dump(manifest, open("manifest.json", "w"), indent=2)
print(f"manifest.json: {meta['name']} {version} md5={checksum}")
