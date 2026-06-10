import json
from pathlib import Path

pub = json.loads(Path("published_products.json").read_text())
tracking = json.loads(Path("affiliate-bot/data/tracking.json").read_text(encoding="utf-8"))

total = len(tracking["links"])
published = len(pub)
videos = len(list(Path("output/videos").glob("*.mp4")))
remaining = total - published

print(f"Total products in tracking: {total}")
print(f"Published to YouTube: {published}")
print(f"Videos created locally: {videos}")
print(f"Pending upload: {remaining}")
print()
print("=== PUBLISHED VIDEOS ===")
for name, info in sorted(pub.items()):
    print(f"  {name}")
    print(f"    URL: {info['url']}")
    print()
