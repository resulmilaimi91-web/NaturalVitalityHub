#!/usr/bin/env python3
import sys, os, json
sys.path.insert(0, r'D:\ANDROID\opencode')
os.chdir(r'D:\ANDROID\opencode')

from youtube_full_auto import create_video_from_script
from youtube_upload import upload_video

SCRIPT_FILE = r'D:\ANDROID\opencode\video_script.txt'
LANG = "en"

if not os.path.exists(SCRIPT_FILE):
    print("Error: video_script.txt not found")
    sys.exit(1)

with open(SCRIPT_FILE, 'r', encoding='utf-8') as f:
    content = f.read()

parts = [p.strip() for p in content.split("---") if p.strip()]

title = parts[0] if len(parts) > 0 else "Video Title"
desc = parts[1] if len(parts) > 1 else ""
hashtags = parts[2] if len(parts) > 2 else ""
sections_text = parts[3:-2] if len(parts) > 5 else parts[3:]
privacy = parts[-2] if len(parts) > 1 else "public"
lang = parts[-1] if len(parts) > 1 and parts[-1] in ("en", "sq") else "en"

sections = [{"text": s, "duration": 5} for s in sections_text if s]

print(f"Title: {title}")
print(f"Sections: {len(sections)}")
print(f"Language: {lang}")
print(f"Privacy: {privacy}")
print("Creating professional video...")

video_path = create_video_from_script(title, sections, desc, hashtags, lang=lang)

print(f"\nVideo created: {video_path}")
print("Uploading to YouTube...")

url = upload_video(video_path, title, f"{desc}\n\n{hashtags}", hashtags.split(), privacy)
result = {"success": True, "url": url, "video_path": video_path}
print(json.dumps(result))
