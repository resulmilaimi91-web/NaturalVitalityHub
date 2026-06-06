#!/usr/bin/env python3
import os, sys, tempfile, random, urllib.request, hashlib, time
from gtts import gTTS
from moviepy import *
from PIL import Image, ImageDraw, ImageFont, ImageFilter
import requests

CACHE_DIR = os.path.join(tempfile.gettempdir(), "video_images")
os.makedirs(CACHE_DIR, exist_ok=True)

PEXELS_KEYS = [
    "563492ad6f917000010000016f7e4e8f6c0640b9b8f3f7c0a1f9f5c0",
    "563492ad6f91700001000001e6f7e4e8f6c0640b9b8f3f7c0a1f9f5c0",
]

TOPICS = {
    "ai": ["artificial intelligence", "robot", "technology", "computer", "data"],
    "coding": ["programming", "code", "laptop", "developer", "software"],
    "video": ["video production", "camera", "film", "editing", "studio"],
    "data": ["data analysis", "dashboard", "analytics", "charts", "business"],
    "design": ["graphic design", "creative", "art", "color", "design"],
    "future": ["future technology", "innovation", "digital", "network", "smart"],
    "default": ["technology", "workspace", "modern", "office", "creative"],
}

def get_topic_keywords(text):
    text_lower = text.lower()
    for topic, keywords in TOPICS.items():
        if topic in text_lower or any(kw in text_lower for kw in keywords[:2]):
            return random.choice(keywords)
    return random.choice(TOPICS["default"])

def download_image(query, size="1920x1080"):
    safe_name = hashlib.md5(query.encode()).hexdigest() + ".jpg"
    cache_path = os.path.join(CACHE_DIR, safe_name)
    if os.path.exists(cache_path) and os.path.getsize(cache_path) > 10000:
        return cache_path
    try:
        url = f"https://source.unsplash.com/{size}/?{query.replace(' ', '+')}"
        urllib.request.urlretrieve(url, cache_path)
        if os.path.getsize(cache_path) > 10000:
            return cache_path
    except Exception:
        pass
    try:
        url = f"https://picsum.photos/1920/1080?random={random.randint(1,99999)}"
        urllib.request.urlretrieve(url, cache_path)
        if os.path.getsize(cache_path) > 10000:
            return cache_path
    except Exception:
        pass
    return None

def get_background(text):
    keyword = get_topic_keywords(text)
    img_path = download_image(keyword)
    if img_path and os.path.exists(img_path):
        try:
            img = Image.open(img_path).convert("RGB")
            img = img.resize((1920, 1080), Image.LANCZOS)
            return img
        except Exception:
            pass
    img = Image.new("RGB", (1920, 1080), (20, 20, 40))
    draw = ImageDraw.Draw(img)
    for y in range(1080):
        ratio = y / 1080
        r = int(20 + ratio * 30)
        g = int(20 + ratio * 10)
        b = int(40 + ratio * 60)
        draw.line([(0, y), (1920, y)], fill=(r, g, b))
    return img

def darken_image(img, factor=0.4):
    from PIL import ImageEnhance
    enhancer = ImageEnhance.Brightness(img)
    return enhancer.enhance(factor)

def draw_text_with_shadow(draw, position, text, font_size, fill=(255,255,255), shadow_color=(0,0,0)):
    x, y = position
    try:
        font = ImageFont.truetype("arial.ttf", font_size)
    except Exception:
        try:
            font = ImageFont.truetype("C:/Windows/Fonts/arial.ttf", font_size)
        except Exception:
            font = ImageFont.load_default()
    draw.text((x+3, y+3), text, fill=shadow_color, font=font)
    draw.text((x, y), text, fill=fill, font=font)

def create_slide(text, theme_color=None, duration=5):
    bg_img = get_background(text)
    bg_img = darken_image(bg_img, 0.35)
    draw = ImageDraw.Draw(bg_img)

    words = text.split()
    lines, current = [], ""
    max_chars = 28
    for w in words:
        if len(current + " " + w) <= max_chars:
            current += " " + w if current else w
        else:
            lines.append(current)
            current = w
    if current:
        lines.append(current)

    font_size = 72
    line_spacing = 95
    total_h = len(lines) * line_spacing
    start_y = max((1080 - total_h) // 2, 120)

    box_pad = 50
    try:
        font = ImageFont.truetype("arial.ttf", font_size)
    except Exception:
        try:
            font = ImageFont.truetype("C:/Windows/Fonts/arial.ttf", font_size)
        except Exception:
            font = ImageFont.load_default()

    max_line_w = 0
    for line in lines:
        bbox = draw.textbbox((0, 0), line, font=font)
        w = bbox[2] - bbox[0]
        max_line_w = max(max_line_w, w)
    box_w = min(max_line_w + box_pad * 2, 1720)

    box_x = (1920 - box_w) // 2
    box_y = start_y - box_pad
    box_h = total_h + box_pad * 2

    overlay = Image.new("RGBA", (box_w, box_h), (0, 0, 0, 140))
    bg_img.paste(Image.alpha_composite(
        Image.new("RGBA", (box_w, box_h), (0, 0, 0, 0)), overlay
    ).convert("RGB"), (box_x, box_y))

    accent_color = theme_color or (0, 200, 255)
    draw.rectangle([box_x, box_y, box_x + 8, box_y + box_h], fill=accent_color)
    draw.rectangle([box_x + box_w - 8, box_y, box_x + box_w, box_y + box_h], fill=accent_color)

    for i, line in enumerate(lines):
        bbox = draw.textbbox((0, 0), line, font=font)
        lw = bbox[2] - bbox[0]
        x = box_x + (box_w - lw) // 2
        y = start_y + i * line_spacing
        draw.text((x+3, y+3), line, fill=(0, 0, 0), font=font)
        draw.text((x, y), line, fill=(255, 255, 255), font=font)

    dot_y = start_y + len(lines) * line_spacing + 30
    for j in range(3):
        dx = 1920 // 2 - 40 + j * 40
        draw.ellipse([dx, dot_y, dx + 20, dot_y + 20], fill=accent_color)

    path = os.path.join(tempfile.gettempdir(), f"slide_{random.randint(0,999999)}.png")
    bg_img.save(path, quality=95)
    return path

def text_to_speech(text, lang="en", slow=False):
    tts = gTTS(text=text, lang=lang, slow=slow)
    path = os.path.join(tempfile.gettempdir(), "narration.mp3")
    tts.save(path)
    return path

def create_video_from_script(title="Video", script_sections=None, desc="", hashtags="", output_dir=None, lang="en"):
    if script_sections is None:
        script_sections = [{"text": "Hello"}]
    if output_dir is None:
        output_dir = tempfile.gettempdir()

    print("Generating audio narration...")
    all_text = " ".join(s["text"] for s in script_sections)
    audio_path = text_to_speech(all_text, lang=lang)
    audio_clip = AudioFileClip(audio_path)
    total_duration = audio_clip.duration

    print(f"Creating video with real images... ({total_duration:.1f}s)")
    clips = []
    sec_per = total_duration / len(script_sections)
    colors = [(0,200,255), (255,200,0), (0,255,150), (255,100,200), (100,200,255), (255,150,50)]

    for i, section in enumerate(script_sections):
        print(f"  Slide {i+1}/{len(script_sections)}: {section['text'][:40]}...")
        color = colors[i % len(colors)]
        slide_path = create_slide(section["text"], theme_color=color, duration=sec_per)
        clip = ImageClip(slide_path, duration=sec_per)
        clips.append(clip)

    video = concatenate_videoclips(clips, method="compose")
    video = video.with_audio(audio_clip)

    output_path = os.path.join(output_dir, f"{title[:30].replace(' ','_')}.mp4")
    video.write_videofile(output_path, fps=24, codec="libx264", audio_codec="aac")
    return output_path

if __name__ == "__main__":
    print("YouTube Professional Video Creator (Real Images)")
    print("=" * 50)
    title = input("Video Title: ")
    print("Script sections (separate with '---'). Ctrl+Z+Enter to finish:")
    text = sys.stdin.read()
    sections = [{"text": s.strip()} for s in text.split("---") if s.strip()]
    desc = input("\nDescription: ")
    hashtags = input("Hashtags: ")
    lang = input("Language (en/sq): ").strip() or "en"

    path = create_video_from_script(title, sections, desc, hashtags, lang=lang)
    print(f"\nVideo: {path}")
