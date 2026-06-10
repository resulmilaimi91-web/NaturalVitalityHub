import json, sys, os, time
sys.path.insert(0, '.')
from pathlib import Path
from youtube_content_automation import get_niche, load_products, generate_youtube_package, create_video_from_package
from youtube_content_automation import PUBLISHED_FILE, load_published, save_published

published = load_published()
products = load_products()

unpublished = []
for p in products:
    name = p["produkti"]
    is_published = any(name.lower() in k.lower() or k.lower() in name.lower() for k in published)
    if not is_published:
        unpublished.append(p)

print(f"Unpublished: {len(unpublished)}")

for i, p in enumerate(unpublished):
    print(f"\n[{i+1}/{len(unpublished)}] {p['produkti']}...")
    try:
        pkg = generate_youtube_package(p)
        path = create_video_from_package(pkg)
        if path:
            print(f"  [OK] {path}")
    except Exception as e:
        print(f"  [X] Error: {e}")
    print()

print("DONE")
