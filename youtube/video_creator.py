#!/usr/bin/env python3
import os, sys, json, tempfile, random, urllib.request, hashlib, time
from gtts import gTTS
from moviepy import *
from PIL import Image, ImageDraw, ImageFont, ImageFilter, ImageEnhance

CACHE_DIR = os.path.join(tempfile.gettempdir(), "video_images")
os.makedirs(CACHE_DIR, exist_ok=True)

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

def darken_image(img, factor=0.35):
    enhancer = ImageEnhance.Brightness(img)
    return enhancer.enhance(factor)

def get_font(size):
    paths = ["arial.ttf", "C:/Windows/Fonts/arial.ttf", "/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf"]
    for p in paths:
        try: return ImageFont.truetype(p, size)
        except: pass
    return ImageFont.load_default()

def create_slide(text, theme_color=None, duration=5):
    bg_img = get_background(text)
    bg_img = darken_image(bg_img, 0.35)
    draw = ImageDraw.Draw(bg_img)

    words = text.split()
    lines, current = [], ""
    for w in words:
        if len(current + " " + w) <= 28:
            current += " " + w if current else w
        else:
            lines.append(current)
            current = w
    if current: lines.append(current)

    font = get_font(72)
    line_spacing = 95
    total_h = len(lines) * line_spacing
    start_y = max((1080 - total_h) // 2, 120)

    max_line_w = max(draw.textbbox((0,0), line, font=font)[2] for line in lines)
    box_w = min(max_line_w + 100, 1720)
    box_x = (1920 - box_w) // 2
    box_y = start_y - 50
    box_h = total_h + 100

    overlay = Image.new("RGBA", (box_w, box_h), (0, 0, 0, 140))
    bg_img.paste(Image.alpha_composite(Image.new("RGBA", (box_w, box_h), (0,0,0,0)), overlay).convert("RGB"), (box_x, box_y))

    accent = theme_color or (0, 200, 255)
    draw.rectangle([box_x, box_y, box_x+8, box_y+box_h], fill=accent)
    draw.rectangle([box_x+box_w-8, box_y, box_x+box_w, box_y+box_h], fill=accent)

    for i, line in enumerate(lines):
        lw = draw.textbbox((0,0), line, font=font)[2]
        x = box_x + (box_w - lw) // 2
        y = start_y + i * line_spacing
        draw.text((x+3, y+3), line, fill=(0,0,0), font=font)
        draw.text((x, y), line, fill=(255,255,255), font=font)

    path = os.path.join(tempfile.gettempdir(), f"slide_{random.randint(0,999999)}.png")
    bg_img.save(path, quality=95)
    return path

def text_to_speech(text, lang="en"):
    tts = gTTS(text=text, lang=lang)
    path = os.path.join(tempfile.gettempdir(), "narration.mp3")
    tts.save(path)
    return path

def get_background_music():
    music_urls = [
        "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-1.mp3",
        "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-2.mp3",
        "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-3.mp3",
    ]
    music_path = os.path.join(tempfile.gettempdir(), "bg_music.mp3")
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

def add_background_music(video_clip, music_path, volume=0.15):
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

def create_music_slide(query, color, duration=30):
    bg_img = get_background(query)
    bg_img = darken_image(bg_img, 0.6)
    path = os.path.join(tempfile.gettempdir(), f"music_slide_{random.randint(0,999999)}.png")
    bg_img.save(path, quality=95)
    return path

def create_music_video(title="Music Mix", duration_minutes=40, output_dir=None):
    if not output_dir: output_dir = tempfile.gettempdir()

    total_seconds = duration_minutes * 60
    print(f"Creating {duration_minutes}min music video...")

    music_path = get_background_music()
    if not music_path:
        raise Exception("Could not download music")

    music = AudioFileClip(music_path)
    music_duration = music.duration
    num_loops = int(total_seconds / music_duration) + 1
    music = concatenate_audioclips([music] * num_loops)
    music = music.subclipped(0, total_seconds)

    print("Creating image slides with real photos...")
    slides_data = [
        ("technology computer", (0, 200, 255)),
        ("nature landscape mountain", (0, 255, 150)),
        ("abstract colorful art", (255, 200, 0)),
        ("space galaxy stars", (100, 200, 255)),
        ("ocean waves sea", (0, 150, 255)),
        ("mountain forest", (255, 150, 50)),
        ("city night lights", (200, 100, 255)),
        ("forest trees nature", (100, 200, 100)),
    ]

    clips = []
    sec_per_slide = 30

    for i in range(int(total_seconds / sec_per_slide)):
        query, color = slides_data[i % len(slides_data)]
        print(f"  Slide {i+1}: {query}...")
        slide_path = create_music_slide(query, color, sec_per_slide)
        clips.append(ImageClip(slide_path, duration=sec_per_slide))

    video = concatenate_videoclips(clips, method="compose")
    video = video.with_audio(music)

    output_path = os.path.join(output_dir, f"MUSIC_{title[:25].replace(' ','_')}_{duration_minutes}min.mp4")
    video.write_videofile(output_path, fps=24, codec="libx264", audio_codec="aac")
    return output_path

def create_video_from_script(title="Video", script_sections=None, desc="", hashtags="", output_dir=None, lang="en", use_music=True):
    if not script_sections: script_sections = [{"text": "Hello"}]
    if not output_dir: output_dir = tempfile.gettempdir()

    print("Generating audio...")
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
        slide_path = create_slide(section["text"], theme_color=colors[i % len(colors)])
        clips.append(ImageClip(slide_path, duration=sec_per))

    video = concatenate_videoclips(clips, method="compose")
    video = video.with_audio(audio_clip)

    if use_music:
        print("Adding background music...")
        music_path = get_background_music()
        if music_path:
            video = add_background_music(video, music_path, volume=0.12)
            print("  Music added!")

    output_path = os.path.join(output_dir, f"{title[:30].replace(' ','_')}.mp4")
    video.write_videofile(output_path, fps=24, codec="libx264", audio_codec="aac")
    return output_path

def upload_to_youtube(video_path, title, desc, tags, privacy="public"):
    sys.path.insert(0, os.path.dirname(__file__))
    from youtube_upload import upload_video
    return upload_video(video_path, title, desc, tags, privacy)

if __name__ == "__main__":
    print("YouTube Auto Creator - Full Professional Video Maker")
    print("=" * 55)
    title = input("Video Title: ")
    print("Script sections (separate with '---'). Ctrl+Z+Enter to finish:")
    text = sys.stdin.read()
    sections = [{"text": s.strip()} for s in text.split("---") if s.strip()]
    desc = input("\nDescription: ")
    hashtags = input("Hashtags: ")
    lang = input("Language (en/sq): ").strip() or "en"
    video_path = create_video_from_script(title, sections, desc, hashtags, lang=lang)
    print(f"\nVideo: {video_path}")

    upload = input("\nUpload to YouTube? (y/n): ").lower()
    if upload == "y":
        url = upload_to_youtube(video_path, title, f"{desc}\n\n{hashtags}", hashtags.split(), "public")
        print(f"Published: {url}")
