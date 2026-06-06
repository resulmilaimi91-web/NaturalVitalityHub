#!/usr/bin/env python3
import json, os, sys, tempfile, random
from gtts import gTTS
from moviepy import *
from PIL import Image, ImageDraw, ImageFont

THEMES = [
    {"bg1": (25, 25, 112), "bg2": (0, 0, 0), "accent": (255, 215, 0), "text": (255, 255, 255)},
    {"bg1": (0, 100, 80), "bg2": (0, 0, 0), "accent": (0, 255, 200), "text": (255, 255, 255)},
    {"bg1": (139, 0, 0), "bg2": (0, 0, 0), "accent": (255, 200, 0), "text": (255, 255, 255)},
    {"bg1": (0, 0, 128), "bg2": (25, 25, 112), "accent": (100, 200, 255), "text": (255, 255, 255)},
    {"bg1": (75, 0, 130), "bg2": (0, 0, 0), "accent": (255, 100, 255), "text": (255, 255, 255)},
]

def draw_gradient(draw, size, color1, color2):
    for y in range(size[1]):
        ratio = y / size[1]
        r = int(color1[0] * (1 - ratio) + color2[0] * ratio)
        g = int(color1[1] * (1 - ratio) + color2[1] * ratio)
        b = int(color1[2] * (1 - ratio) + color2[2] * ratio)
        draw.line([(0, y), (size[0], y)], fill=(r, g, b))

def wrap_text(text, max_chars=25):
    words = text.split()
    lines, current = [], ""
    for w in words:
        if len(current + " " + w) <= max_chars:
            current += " " + w if current else w
        else:
            lines.append(current)
            current = w
    if current: lines.append(current)
    return lines

def create_slide(text, theme=None, duration=5, size=(1920, 1080)):
    if theme is None:
        theme = random.choice(THEMES)
    img = Image.new("RGB", size)
    draw = ImageDraw.Draw(img)
    draw_gradient(draw, size, theme["bg1"], theme["bg2"])

    lines = wrap_text(text, 30)
    font_large = 80
    line_spacing = 100
    total_h = len(lines) * line_spacing
    start_y = max((size[1] - total_h) // 2, 100)

    box_pad = 40
    box_x = 100
    max_line_w = 0
    for line in lines:
        bbox = draw.textbbox((0, 0), line, font_size=font_large)
        w = bbox[2] - bbox[0]
        max_line_w = max(max_line_w, w)
    box_w = min(max_line_w + box_pad * 2, size[0] - 200)

    box_y = start_y - box_pad
    box_h = total_h + box_pad * 2
    overlay = Image.new("RGBA", (box_w, box_h), (0, 0, 0, 120))
    img.paste(overlay, (box_x, box_y), overlay)

    for i, line in enumerate(lines):
        bbox = draw.textbbox((0, 0), line, font_size=font_large)
        x = box_x + (box_w - (bbox[2] - bbox[0])) // 2
        y = start_y + i * line_spacing
        draw.text((x + 3, y + 3), line, fill=(0, 0, 0, 180), font_size=font_large)
        draw.text((x, y), line, fill=theme["text"], font_size=font_large)

    accent_y = start_y + len(lines) * line_spacing + 20
    for j in range(3):
        ax = size[0] // 2 - 60 + j * 60
        draw.ellipse([ax, accent_y, ax + 30, accent_y + 30], fill=theme["accent"])

    slide_path = os.path.join(tempfile.gettempdir(), f"slide_{random.randint(0,999999)}.png")
    img.save(slide_path)
    return slide_path

def text_to_speech(text, lang="en", slow=False):
    tts = gTTS(text=text, lang=lang, slow=slow)
    path = os.path.join(tempfile.gettempdir(), "narration.mp3")
    tts.save(path)
    return path

def create_video_from_script(title="Video Title", script_sections=None, desc="", hashtags="", output_dir=None, lang="en"):
    if script_sections is None:
        script_sections = [{"text": "Hello World", "duration": 5}]
    if output_dir is None:
        output_dir = tempfile.gettempdir()

    print("Generating audio...")
    all_text = " ".join(s["text"] for s in script_sections)
    audio_path = text_to_speech(all_text, lang=lang)
    audio_clip = AudioFileClip(audio_path)
    total_duration = audio_clip.duration

    print(f"Creating professional slides... ({total_duration:.1f}s audio)")
    clips = []
    sec_per_section = total_duration / len(script_sections)

    for i, section in enumerate(script_sections):
        theme = THEMES[i % len(THEMES)]
        slide_path = create_slide(section["text"], theme=theme, duration=sec_per_section)
        clip = ImageClip(slide_path, duration=sec_per_section)
        clips.append(clip)

    video = concatenate_videoclips(clips, method="compose")
    video = video.with_audio(audio_clip)

    output_path = os.path.join(output_dir, f"{title[:30].replace(' ','_')}.mp4")
    video.write_videofile(output_path, fps=24, codec="libx264", audio_codec="aac")
    return output_path

if __name__ == "__main__":
    print("YouTube Professional Video Creator")
    print("=" * 40)
    title = input("Video Title: ")
    print("Enter script (sections separated by '---' on a new line). Press Enter then Ctrl+Z+Enter to finish:")
    text = sys.stdin.read()
    sections = [{"text": s.strip(), "duration": 5} for s in text.split("---") if s.strip()]
    if not sections:
        sections = [{"text": text.strip(), "duration": 5}] if text.strip() else [{"text": "No content", "duration": 5}]
    desc = input("\nDescription: ")
    hashtags = input("Hashtags: ")
    lang = input("Language (en/sq): ").strip() or "en"

    video_path = create_video_from_script(title, sections, desc, hashtags, lang=lang)
    print(f"\nVideo created: {video_path}")

    upload = input("\nUpload to YouTube? (yes/no): ").lower()
    if upload in ("yes", "po"):
        from youtube_upload import upload_video
        url = upload_video(video_path, title, f"{desc}\n\n{hashtags}", hashtags.split(), "public")
        print(f"Video published: {url}")
    else:
        print("Video ready for manual upload.")
