#!/usr/bin/env python3
import os, sys, json, tempfile, random, urllib.request, hashlib
from PIL import Image, ImageDraw, ImageFont, ImageFilter, ImageEnhance
from moviepy import *

CACHE_DIR = os.path.join(tempfile.gettempdir(), "livestream_images")
os.makedirs(CACHE_DIR, exist_ok=True)

def get_font(size):
    paths = ["arial.ttf", "C:/Windows/Fonts/arial.ttf"]
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

def get_background_music():
    music_urls = [
        "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-1.mp3",
        "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-2.mp3",
        "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-3.mp3",
    ]
    music_path = os.path.join(tempfile.gettempdir(), "livestream_music.mp3")
    if os.path.exists(music_path) and os.path.getsize(music_path) > 10000:
        return music_path
    for url in music_urls:
        try:
            urllib.request.urlretrieve(url, music_path)
            if os.path.getsize(music_path) > 10000:
                return music_path
        except: continue
    return None

def create_livestream_slide(query, color):
    img_path = download_image(query)
    if img_path and os.path.exists(img_path):
        try:
            img = Image.open(img_path).convert("RGB")
            img = img.resize((1920, 1080), Image.LANCZOS)
            enhancer = ImageEnhance.Brightness(img)
            img = enhancer.enhance(0.5)
        except:
            img = Image.new("RGB", (1920, 1080), (10, 10, 30))
    else:
        img = Image.new("RGB", (1920, 1080), (10, 10, 30))

    draw = ImageDraw.Draw(img)
    font = get_font(36)
    draw.text((50, 50), "LIVE 24/7", fill=color, font=font)
    draw.text((50, 100), "Music Stream", fill=(255,255,255), font=get_font(28))

    path = os.path.join(tempfile.gettempdir(), f"live_slide_{random.randint(0,999999)}.png")
    img.save(path, quality=95)
    return path

def create_livestream_video(title="24/7 Music Live", hours=1, output_dir=None):
    if not output_dir: output_dir = tempfile.gettempdir()

    total_seconds = hours * 3600
    print(f"Creating {hours}h livestream video...")

    music_path = get_background_music()
    if not music_path:
        raise Exception("Could not download music")

    music = AudioFileClip(music_path)
    music_duration = music.duration
    num_loops = int(total_seconds / music_duration) + 1
    music = concatenate_audioclips([music] * num_loops)
    music = music.subclipped(0, total_seconds)

    slides_data = [
        ("technology", (0, 200, 255)),
        ("nature", (0, 255, 150)),
        ("abstract", (255, 200, 0)),
        ("space", (100, 200, 255)),
        ("ocean", (0, 150, 255)),
        ("mountain", (255, 150, 50)),
        ("city", (200, 100, 255)),
        ("forest", (100, 200, 100)),
    ]

    clips = []
    sec_per_slide = 60

    for i in range(int(total_seconds / sec_per_slide)):
        query, color = slides_data[i % len(slides_data)]
        print(f"  Slide {i+1}...")
        slide_path = create_livestream_slide(query, color)
        clips.append(ImageClip(slide_path, duration=sec_per_slide))

    video = concatenate_videoclips(clips, method="compose")
    video = video.with_audio(music)

    output_path = os.path.join(output_dir, f"LIVE_{title[:25].replace(' ','_').replace('/','_')}.mp4")
    video.write_videofile(output_path, fps=24, codec="libx264", audio_codec="aac")
    return output_path

if __name__ == "__main__":
    print("YouTube 24/7 Livestream Creator")
    print("=" * 40)
    title = input("Stream Title: ").strip() or "24/7 Music Live"
    hours = int(input("Hours (1-24): ").strip() or "1")
    path = create_livestream_video(title, hours)
    print(f"\nVideo: {path}")
    print("\nTo stream this to YouTube:")
    print("1. Open OBS Studio")
    print("2. Add Media Source -> select this video")
    print("3. Set to Loop")
    print("4. Stream to YouTube using Stream Key")
