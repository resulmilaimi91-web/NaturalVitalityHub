import sys, json, os
sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.dirname(__file__))))
from pathlib import Path
from youtube_content_automation import create_video_from_package

scripts = sorted(Path("output/scripts").iterdir(), key=lambda p: p.stat().st_mtime, reverse=True)
if not scripts:
    print("No scripts found"); exit(1)
latest = scripts[0]
meta = json.loads((latest / "package.json").read_text())
title = (latest / "title.txt").read_text().strip()
script = (latest / "script.txt").read_text()
desc = (latest / "description.txt").read_text()
tags = ["health", meta["product"].replace(" ", "")]
pkg = {"title": title, "product": meta["product"], "description": desc, "script": script, "tags": tags, "affiliate_url": meta.get("affiliate_url", ""), "niche": meta.get("niche", "general")}
vp = create_video_from_package(pkg, meta["product"][:20])
if vp:
    print(f"VIDEO={vp}")
    with open(os.environ.get("GITHUB_ENV", "NUL"), "a") as f:
        f.write(f"VIDEO_PATH={vp}\n")
else:
    print("Video creation failed")
