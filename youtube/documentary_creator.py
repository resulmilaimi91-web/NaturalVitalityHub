#!/usr/bin/env python3
import os, sys, json, random, tempfile, urllib.request, hashlib
from PIL import Image, ImageDraw, ImageFont, ImageFilter, ImageEnhance
from gtts import gTTS
from moviepy import *
import numpy as np

CACHE_DIR = os.path.join(tempfile.gettempdir(), "doc_images")
os.makedirs(CACHE_DIR, exist_ok=True)

def get_font(size):
    paths = ["arial.ttf", "C:/Windows/Fonts/arial.ttf", "C:/Windows/Fonts/arialbd.ttf"]
    for p in paths:
        try: return ImageFont.truetype(p, size)
        except: pass
    return ImageFont.load_default()

def download_image(query):
    safe = hashlib.md5(query.encode()).hexdigest() + ".jpg"
    path = os.path.join(CACHE_DIR, safe)
    if os.path.exists(path) and os.path.getsize(path) > 5000:
        return path
    try:
        url = f"https://source.unsplash.com/1920x1080/?{query.replace(' ', '+')}"
        urllib.request.urlretrieve(url, path)
        if os.path.getsize(path) > 5000:
            return path
    except: pass
    try:
        url = f"https://picsum.photos/1920/1080?random={random.randint(1,99999)}"
        urllib.request.urlretrieve(url, path)
        if os.path.getsize(path) > 5000:
            return path
    except: pass
    return None

def create_3d_effect(img):
    arr = np.array(img)
    height, width = arr.shape[:2]
    for y in range(height):
        shift = int(3 * np.sin(y / 50.0))
        arr[y] = np.roll(arr[y], shift, axis=0)
    result = Image.fromarray(arr)
    return result

def create_gradient_overlay(size, color1, color2, direction="vertical"):
    img = Image.new("RGBA", size, (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    for i in range(size[0] if direction == "horizontal" else size[1]):
        ratio = i / (size[0] if direction == "horizontal" else size[1])
        r = int(color1[0] + (color2[0] - color1[0]) * ratio)
        g = int(color1[1] + (color2[1] - color1[1]) * ratio)
        b = int(color1[2] + (color2[2] - color1[2]) * ratio)
        if direction == "horizontal":
            draw.line([(i, 0), (i, size[1])], fill=(r, g, b, 60))
        else:
            draw.line([(0, i), (size[0], i)], fill=(r, g, b, 60))
    return img

def darken_image(img, factor=0.4):
    enhancer = ImageEnhance.Brightness(img)
    return enhancer.enhance(factor)

def get_background_music():
    music_urls = [
        "https://cdn.pixabay.com/audio/2022/10/25/audio_4a60f5907a.mp3",
        "https://cdn.pixabay.com/audio/2022/02/22/audio_807ccf680a.mp3",
        "https://cdn.pixabay.com/audio/2023/09/04/audio_5dc592e85c.mp3",
    ]
    music_path = os.path.join(tempfile.gettempdir(), "doc_music.mp3")
    if os.path.exists(music_path) and os.path.getsize(music_path) > 10000:
        return music_path
    for url in music_urls:
        try:
            urllib.request.urlretrieve(url, music_path)
            if os.path.getsize(music_path) > 10000:
                return music_path
        except Exception:
            continue
    return None

def add_background_music(video_clip, music_path, volume=0.12):
    if not music_path or not os.path.exists(music_path):
        return video_clip
    try:
        music = AudioFileClip(music_path)
        if music.duration < video_clip.duration:
            loops = int(video_clip.duration / music.duration) + 1
            music = concatenate_audioclips([music] * loops)
        music = music.subclipped(0, video_clip.duration)
        music = music.with_volume_scaled(volume)
        if video_clip.audio:
            final_audio = CompositeAudioClip([video_clip.audio, music])
            return video_clip.with_audio(final_audio)
        else:
            return video_clip.with_audio(music)
    except Exception as e:
        print(f"Music error: {e}")
        return video_clip

def create_documentary_slide(text, title="Documentary", slide_num=1, total_slides=5):
    bg = Image.new("RGB", (1920, 1080), (8, 8, 18))
    draw = ImageDraw.Draw(bg)

    keywords = [w.lower() for w in text.split() if len(w) > 3]
    query = random.choice(keywords) if keywords else "documentary"
    img_path = download_image(query)
    if img_path and os.path.exists(img_path):
        try:
            bg_img = Image.open(img_path).convert("RGB")
            bg_img = bg_img.resize((1920, 1080), Image.LANCZOS)
            bg_img = darken_image(bg_img, 0.4)
            bg_img = create_3d_effect(bg_img)
            gradient = create_gradient_overlay((1920, 1080), (0, 100, 200), (100, 0, 150), "vertical")
            bg = Image.alpha_composite(bg_img.convert("RGBA"), gradient).convert("RGB")
            draw = ImageDraw.Draw(bg)
        except: pass

    font_title = get_font(52)
    font_med = get_font(58)
    font_small = get_font(28)

    draw.text((80, 60), title.upper(), fill=(0, 200, 255), font=font_title)
    draw.line([(80, 130), (500, 130)], fill=(0, 200, 255), width=3)

    words = text.split()
    lines, current = [], ""
    for w in words:
        if len(current + " " + w) <= 26:
            current += " " + w if current else w
        else:
            lines.append(current)
            current = w
    if current: lines.append(current)

    line_spacing = 80
    total_h = len(lines) * line_spacing
    start_y = max((1080 - total_h) // 2, 200)

    max_line_w = max(draw.textbbox((0, 0), line, font=font_med)[2] for line in lines)
    box_w = min(max_line_w + 120, 1700)
    box_x = (1920 - box_w) // 2
    box_y = start_y - 60
    box_h = total_h + 120

    shadow = Image.new("RGBA", (box_w + 10, box_h + 10), (0, 0, 0, 80))
    shadow = shadow.filter(ImageFilter.GaussianBlur(10))
    bg.paste(Image.alpha_composite(Image.new("RGBA", (box_w + 10, box_h + 10), (0,0,0,0)), shadow).convert("RGB"), (box_x - 5, box_y - 5))

    box_overlay = Image.new("RGBA", (box_w, box_h), (0, 0, 0, 160))
    bg.paste(Image.alpha_composite(Image.new("RGBA", (box_w, box_h), (0,0,0,0)), box_overlay).convert("RGB"), (box_x, box_y))

    accent_colors = [(0, 200, 255), (255, 200, 0), (0, 255, 150), (255, 100, 200)]
    accent = accent_colors[slide_num % len(accent_colors)]
    draw.rectangle([box_x, box_y, box_x + 6, box_y + box_h], fill=accent)
    draw.rectangle([box_x + box_w - 6, box_y, box_x + box_w, box_y + box_h], fill=accent)

    for i, line in enumerate(lines):
        bbox = draw.textbbox((0, 0), line, font=font_med)
        lw = bbox[2] - bbox[0]
        x = box_x + (box_w - lw) // 2
        y = start_y + i * 80
        draw.text((x+2, y+2), line, fill=(0, 0, 0), font=font_med)
        draw.text((x, y), line, fill=(255, 255, 255), font=font_med)

    progress = f"{slide_num}/{total_slides}"
    draw.text((1920 - 150, 1080 - 60), progress, fill=(0, 200, 255), font=font_small)

    bar_width = 300
    bar_x = 1920 - 150 - bar_width - 20
    bar_y = 1080 - 50
    draw.rectangle([bar_x, bar_y, bar_x + bar_width, bar_y + 8], fill=(50, 50, 50))
    fill_width = int(bar_width * (slide_num / total_slides))
    draw.rectangle([bar_x, bar_y, bar_x + fill_width, bar_y + 8], fill=accent)

    path = os.path.join(tempfile.gettempdir(), f"doc_slide_{random.randint(0,999999)}.png")
    bg.save(path, quality=95)
    return path

def text_to_speech(text, lang="en"):
    tts = gTTS(text=text, lang=lang)
    path = os.path.join(tempfile.gettempdir(), "narration.mp3")
    tts.save(path)
    return path

def create_documentary_video(title, script_sections, lang="en", output_dir=None, use_music=True):
    if not output_dir: output_dir = tempfile.gettempdir()

    print("Generating documentary narration...")
    all_text = " ".join(s["text"] for s in script_sections)
    audio_path = text_to_speech(all_text, lang=lang)
    audio_clip = AudioFileClip(audio_path)
    total_duration = audio_clip.duration

    print(f"Creating 3D documentary slides... ({total_duration:.1f}s)")
    clips = []
    sec_per = total_duration / len(script_sections)

    for i, section in enumerate(script_sections):
        print(f"  Slide {i+1}/{len(script_sections)}: {section['text'][:40]}...")
        slide_path = create_documentary_slide(section["text"], title, i+1, len(script_sections))
        clip = ImageClip(slide_path, duration=sec_per)
        clips.append(clip)

    video = concatenate_videoclips(clips, method="compose")
    video = video.with_audio(audio_clip)

    if use_music:
        print("Adding background music...")
        music_path = get_background_music()
        if music_path:
            video = add_background_music(video, music_path, volume=0.12)
            print("  Music added!")

    output_path = os.path.join(output_dir, f"DOC_{title[:25].replace(' ','_')}.mp4")
    video.write_videofile(output_path, fps=24, codec="libx264", audio_codec="aac")
    return output_path

def upload_to_youtube(video_path, title, desc, tags, privacy="public"):
    sys.path.insert(0, os.path.dirname(__file__))
    from youtube_upload import upload_video
    return upload_video(video_path, title, desc, tags, privacy)

if __name__ == "__main__":
    print("YouTube 3D Documentary Video Creator")
    print("=" * 50)
    title = input("Documentary Topic: ")
    print("Script (separate sections with '---'). Ctrl+Z+Enter to finish:")
    text = sys.stdin.read()
    sections = [{"text": s.strip()} for s in text.split("---") if s.strip()]
    if not sections:
        sections = [{"text": text.strip()}]
    lang = input("Language (en/sq): ").strip() or "en"

    path = create_documentary_video(title, sections, lang)
    print(f"\nDocumentary: {path}")
