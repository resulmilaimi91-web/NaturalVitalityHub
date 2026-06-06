import sys, os
sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.dirname(__file__))))
from pathlib import Path
videos = list(Path("output/videos").glob("*.mp4"))
if not videos:
    print("No video found")
    sys.exit(0)
vp = str(sorted(videos, key=lambda p: p.stat().st_mtime, reverse=True)[0])
scripts = sorted(Path("output/scripts").iterdir(), key=lambda p: p.stat().st_mtime, reverse=True)
if scripts:
    import json
    m = json.loads((scripts[0] / "package.json").read_text())
    title = m["title"]
    desc = (scripts[0] / "description.txt").read_text()
    tags = ["health", m["product"].replace(" ", "")]
else:
    title = os.path.basename(vp)
    desc = "Auto-generated"
    tags = ["health"]
privacy = os.environ.get("PRIVACY", "unlisted")
from youtube_upload import upload_video
url = upload_video(vp, title, desc, tags, privacy)
print(f"UPLOADED={url}")
