#!/usr/bin/env python3
import json, os, sys, tempfile, random, glob
from gtts import gTTS
from moviepy import *
from PIL import Image, ImageDraw, ImageFont, ImageFilter

BASE = Path(os.path.dirname(os.path.abspath(__file__)))
IMAGES_DIR = BASE / "images"
os.makedirs(IMAGES_DIR, exist_ok=True)

THEMES = [
    {"accent": (255, 215, 0), "text": (255, 255, 255), "shadow": True, "fill": (0, 0, 0, 160)},
    {"accent": (0, 255, 200), "text": (255, 255, 255), "shadow": True, "fill": (0, 0, 0, 150)},
    {"accent": (255, 200, 0), "text": (255, 255, 255), "shadow": True, "fill": (0, 0, 0, 140)},
    {"accent": (100, 200, 255), "text": (255, 255, 255), "shadow": True, "fill": (0, 0, 0, 155)},
    {"accent": (255, 100, 255), "text": (255, 255, 255), "shadow": True, "fill": (0, 0, 0, 145)},
]

def create_default_bg(size=(1920, 1080)):
    img = Image.new("RGB", size, (20, 30, 50))
    draw = ImageDraw.Draw(img)
    for i in range(30):
        x, y = random.randint(0, size[0]), random.randint(0, size[1])
        r = random.randint(30, 100)
        c = (random.randint(30, 80), random.randint(50, 120), random.randint(60, 130))
        draw.ellipse([x-r, y-r, x+r, y+r], fill=c, outline=None)
    return img.filter(ImageFilter.GaussianBlur(20))

def get_background_images():
    exts = ("*.jpg", "*.jpeg", "*.png", "*.webp")
    files = []
    for ext in exts:
        files.extend(glob.glob(os.path.join(IMAGES_DIR, ext)))
        files.extend(glob.glob(os.path.join(IMAGES_DIR, ext.upper())))
    return sorted(set(files))

def load_bg_image(size=(1920, 1080)):
    images = get_background_images()
    if images:
        path = random.choice(images)
        try:
            bg = Image.open(path).convert("RGB")
            bg = bg.resize(size, Image.LANCZOS)
            return bg
        except:
            pass
    return create_default_bg()

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
    img = load_bg_image(size)
    overlay = Image.new("RGBA", size, theme["fill"])
    img = Image.alpha_composite(img.convert("RGBA"), overlay).convert("RGB")
    draw = ImageDraw.Draw(img)

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

    for i, line in enumerate(lines):
        bbox = draw.textbbox((0, 0), line, font_size=font_large)
        x = box_x + (box_w - (bbox[2] - bbox[0])) // 2
        y = start_y + i * line_spacing
        draw.text((x + 3, y + 3), line, fill=(0, 0, 0, 200), font_size=font_large)
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
