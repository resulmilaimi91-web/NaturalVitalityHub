#!/usr/bin/env python3
import os, sys

sys.path.insert(0, os.path.dirname(__file__))
os.chdir(os.path.dirname(__file__))

from video_creator import create_video_from_script, upload_to_youtube

def quick_post(title, sections, desc="", hashtags="", lang="en"):
    video_path = create_video_from_script(title, sections, desc, hashtags, lang=lang)
    url = upload_to_youtube(video_path, title, f"{desc}\n\n{hashtags}", hashtags.split(), "public")
    print(f"Video published: {url}")
    return url

if __name__ == "__main__":
    quick_post(
        title="AI Tools 2026",
        sections=[
            {"text": "Welcome to the top AI tools of 2026"},
            {"text": "ChatGPT leads for writing and reasoning"},
            {"text": "Runway dominates video generation"},
            {"text": "GitHub Copilot for coding productivity"},
            {"text": "Subscribe for more AI content!"}
        ],
        desc="Top AI tools transforming our world in 2026",
        hashtags="#AI2026 #AITools #Technology",
        lang="en"
    )
