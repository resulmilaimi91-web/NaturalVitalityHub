#!/usr/bin/env python3
import json, os, sys, tempfile, random, glob
from pathlib import Path
from gtts import gTTS
from moviepy import *
from PIL import Image, ImageDraw, ImageFont, ImageFilter

BASE = Path(os.path.dirname(os.path.abspath(__file__)))
IMAGES_DIR = BASE / "images"
os.makedirs(IMAGES_DIR, exist_ok=True)

THEMES = [
    {"accent": "#FFD700", "text": "#FFFFFF", "bar": "#FFD700", "fill": (0, 0, 0, 160)},
    {"accent": "#00FFC8", "text": "#FFFFFF", "bar": "#00FFC8", "fill": (0, 0, 0, 150)},
    {"accent": "#FFC800", "text": "#FFFFFF", "bar": "#FFC800", "fill": (0, 0, 0, 140)},
    {"accent": "#64C8FF", "text": "#FFFFFF", "bar": "#64C8FF", "fill": (0, 0, 0, 155)},
    {"accent": "#FF64FF", "text": "#FFFFFF", "bar": "#FF64FF", "fill": (0, 0, 0, 145)},
]

NICHE_COLORS = {
    "dental-health": "#1E90FF", "blood-sugar": "#32CD32", "men-health": "#4169E1",
    "sleep-health": "#6A5ACD", "gut-health": "#228B22", "weight-loss": "#FF6347",
    "brain-health": "#9932CC", "eye-health": "#00CED1", "stress-relief": "#DDA0DD",
    "detox": "#00FA9A", "joint-health": "#4682B4", "energy": "#FFD700", "beauty": "#FF69B4",
    "general-health": "#20B2AA", "minerals": "#87CEEB",
}

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

def extract_bullets(text, max_bullets=3):
    sentences = [s.strip() for s in text.replace("!", ".").replace("?", ".").split(".") if len(s.strip()) > 10]
    bullets = []
    for s in sentences:
        words = s.split()
        short = " ".join(words[:6])
        if short and len(short) > 15 and short not in bullets:
            bullets.append(short)
        if len(bullets) >= max_bullets:
            break
    if not bullets:
        words = text.split()
        for i in range(0, min(len(words), 12), 4):
            chunk = " ".join(words[i:i+4])
            if chunk:
                bullets.append(chunk)
    return bullets[:max_bullets]

def create_product_card(product_name, niche="general-health", size=(1920, 1080), affiliate_url=""):
    color = NICHE_COLORS.get(niche, "#20B2AA")
    r, g, b = int(color[1:3], 16), int(color[3:5], 16), int(color[5:7], 16)
    bg = Image.new("RGB", size, (15, 20, 35))
    draw = ImageDraw.Draw(bg)
    for i in range(40):
        x, y = random.randint(0, size[0]), random.randint(0, size[1])
        rad = random.randint(15, 80)
        cr = random.randint(25, 60)
        cg = random.randint(35, 90)
        cb = random.randint(45, 100)
        draw.ellipse([x-rad, y-rad, x+rad, y+rad], fill=(cr, cg, cb))
    bg = bg.filter(ImageFilter.GaussianBlur(25))
    draw = ImageDraw.Draw(bg)
    overlay = Image.new("RGBA", size, (0, 0, 0, 120))
    bg = Image.alpha_composite(bg.convert("RGBA"), overlay).convert("RGB")
    draw = ImageDraw.Draw(bg)

    accent_line = Image.new("RGBA", (6, size[1]), (r, g, b, 200))
    bg.paste(accent_line, (0, 0), accent_line)

    box_w, box_h = 800, 400
    box_x, box_y = (size[0] - box_w) // 2, (size[1] - box_h) // 2 - 60
    box_overlay = Image.new("RGBA", (box_w, box_h), (r, g, b, 40))
    bg.paste(box_overlay, (box_x, box_y), box_overlay)
    draw.rectangle([box_x, box_y, box_x + box_w, box_y + box_h], outline=color, width=3)

    name = product_name if len(product_name) < 25 else product_name[:22] + "..."
    bbox = draw.textbbox((0, 0), name, font_size=90)
    nx = (size[0] - (bbox[2] - bbox[0])) // 2
    draw.text((nx + 3, box_y + 55 + 3), name, fill=(0, 0, 0, 200), font_size=90)
    draw.text((nx, box_y + 55), name, fill="#FFFFFF", font_size=90)

    niche_label = niche.replace("-", " ").title()
    bbox = draw.textbbox((0, 0), niche_label, font_size=36)
    nnx = (size[0] - (bbox[2] - bbox[0])) // 2
    draw.text((nnx, box_y + 180), niche_label, fill=color, font_size=36)

    badge = "OFFICIAL WEBSITE"
    bbox = draw.textbbox((0, 0), badge, font_size=32)
    bx = (size[0] - (bbox[2] - bbox[0])) // 2
    badge_pad = 20
    badge_w = (bbox[2] - bbox[0]) + badge_pad * 2
    badge_h = (bbox[3] - bbox[1]) + badge_pad * 2
    badge_x = bx - badge_pad
    badge_y = box_y + 240
    draw.rounded_rectangle([badge_x, badge_y, badge_x + badge_w, badge_y + badge_h], radius=8, fill=color)
    draw.text((bx, badge_y + 8), badge, fill="#000000", font_size=32)

    dots_y = box_y + box_h + 40
    for j in range(5):
        dx = size[0] // 2 - 120 + j * 60
        draw.ellipse([dx, dots_y, dx + 20, dots_y + 20], fill=color)

    if affiliate_url:
        short_url = affiliate_url[:55] + "..." if len(affiliate_url) > 55 else affiliate_url
        bbox = draw.textbbox((0, 0), short_url, font_size=32)
        ux = (size[0] - (bbox[2] - bbox[0])) // 2
        draw.text((ux + 2, dots_y + 60 + 2), short_url, fill=(0, 0, 0, 200), font_size=32)
        draw.text((ux, dots_y + 60), short_url, fill=color, font_size=32)

    path = os.path.join(tempfile.gettempdir(), f"product_card_{random.randint(0,999999)}.png")
    bg.save(path)
    return path

def wrap_text(text, max_chars=22):
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

def create_slide(text, theme=None, duration=5, size=(1920, 1080), product_card_path=None, affiliate_url=""):
    if theme is None:
        theme = random.choice(THEMES)
    img = load_bg_image(size)
    overlay = Image.new("RGBA", size, theme["fill"])
    img = Image.alpha_composite(img.convert("RGBA"), overlay).convert("RGB")
    draw = ImageDraw.Draw(img)

    bullets = extract_bullets(text, 3)
    if not bullets:
        bullets = ["Learn more about", "this product", "in the description"]

    marker = " ▸ "
    bullet_texts = [marker + b for b in bullets]
    font_big = 72
    font_small = 48
    line_h = 110
    start_y = (size[1] - len(bullet_texts) * line_h) // 2 - 40

    for i, bt in enumerate(bullet_texts):
        y = start_y + i * line_h
        draw.text((130, y + 3), bt, fill=(0, 0, 0, 200), font_size=font_big)
        draw.text((130, y), bt, fill=theme["text"], font_size=font_big)

    if product_card_path and os.path.exists(product_card_path):
        try:
            card = Image.open(product_card_path).convert("RGBA")
            card = card.resize((220, 220), Image.LANCZOS)
            cx, cy = size[0] - 270, size[1] - 270
            img.paste(card, (cx, cy), card)
        except:
            pass

    if affiliate_url:
        short_url = affiliate_url[:55] + "..." if len(affiliate_url) > 55 else affiliate_url
        bbox = draw.textbbox((0, 0), short_url, font_size=24)
        lx = (size[0] - (bbox[2] - bbox[0])) // 2
        draw.text((lx, size[1] - 50), short_url, fill=theme["text"], font_size=24)

    slide_path = os.path.join(tempfile.gettempdir(), f"slide_{random.randint(0,999999)}.png")
    img.save(slide_path)
    return slide_path

def text_to_speech(text, lang="en", slow=False):
    tts = gTTS(text=text, lang=lang, slow=slow)
    path = os.path.join(tempfile.gettempdir(), "narration.mp3")
    tts.save(path)
    return path

def create_video_from_script(title="Video Title", script_sections=None, desc="", hashtags="", output_dir=None, lang="en", product_name="", niche="general-health", affiliate_url=""):
    if script_sections is None:
        script_sections = [{"text": "Hello World", "duration": 5}]
    if output_dir is None:
        output_dir = tempfile.gettempdir()

    print("Generating product card...")
    product_card_path = create_product_card(product_name or title, niche, affiliate_url=affiliate_url)

    print("Generating audio...")
    all_text = " ".join(s["text"] for s in script_sections)
    audio_path = text_to_speech(all_text, lang=lang)
    audio_clip = AudioFileClip(audio_path)
    total_duration = audio_clip.duration

    print(f"Creating slides... ({total_duration:.1f}s audio)")
    clips = []
    n_sections = len(script_sections)
    sec_per_section = total_duration / n_sections if n_sections > 0 else total_duration

    product_card_clip = ImageClip(product_card_path, duration=sec_per_section * 1.5)
    clips.append(product_card_clip)

    for i, section in enumerate(script_sections):
        theme = THEMES[i % len(THEMES)]
        slide_path = create_slide(section["text"], theme=theme, duration=sec_per_section, product_card_path=product_card_path, affiliate_url=affiliate_url)
        clip = ImageClip(slide_path, duration=sec_per_section)
        clips.append(clip)

    product_card_end = ImageClip(product_card_path, duration=sec_per_section)
    clips.append(product_card_end)

    video = concatenate_videoclips(clips, method="compose")
    total_video_duration = video.duration
    if total_video_duration < total_duration:
        factor = total_duration / total_video_duration
        video = video.with_duration(total_video_duration * factor)
        video = video.with_audio(audio_clip.set_duration(total_video_duration * factor))
    else:
        audio_clip = audio_clip.with_duration(total_duration)
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
