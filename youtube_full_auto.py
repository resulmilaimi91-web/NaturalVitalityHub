import json, os, sys, tempfile, random, glob, math
from pathlib import Path
from gtts import gTTS
from moviepy import *
from PIL import Image, ImageDraw, ImageFont, ImageFilter

BASE = Path(os.path.dirname(os.path.abspath(__file__)))
IMAGES_DIR = BASE / "images"
os.makedirs(IMAGES_DIR, exist_ok=True)

sys.path.insert(0, str(BASE))
try:
    from gemini_image_generator import find_bg_image, create_procedural_bg
    GEMINI_AVAILABLE = True
except:
    GEMINI_AVAILABLE = False

try:
    from product_image_fetcher import get_product_image
    PRODUCT_IMAGES_AVAILABLE = True
except:
    PRODUCT_IMAGES_AVAILABLE = False

THEMES = [
    {"accent": "#FFD700", "text": "#FFFFFF", "bar": "#FFD700", "fill": (0, 0, 0, 120)},
    {"accent": "#00FFC8", "text": "#FFFFFF", "bar": "#00FFC8", "fill": (0, 0, 0, 110)},
    {"accent": "#FF4466", "text": "#FFFFFF", "bar": "#FF4466", "fill": (0, 0, 0, 115)},
    {"accent": "#64C8FF", "text": "#FFFFFF", "bar": "#64C8FF", "fill": (0, 0, 0, 120)},
    {"accent": "#FF64FF", "text": "#FFFFFF", "bar": "#FF64FF", "fill": (0, 0, 0, 110)},
    {"accent": "#FF8844", "text": "#FFFFFF", "bar": "#FF8844", "fill": (0, 0, 0, 115)},
    {"accent": "#44FF88", "text": "#FFFFFF", "bar": "#44FF88", "fill": (0, 0, 0, 110)},
]

NICHE_COLORS = {
    "dental-health": "#1E90FF", "blood-sugar": "#32CD32", "men-health": "#4169E1",
    "sleep-health": "#6A5ACD", "gut-health": "#228B22", "weight-loss": "#FF6347",
    "brain-health": "#9932CC", "eye-health": "#00CED1", "stress-relief": "#DDA0DD",
    "detox": "#00FA9A", "joint-health": "#4682B4", "energy": "#FFD700", "beauty": "#FF69B4",
    "general-health": "#20B2AA", "minerals": "#87CEEB",
}

def create_gradient_bg(size=(1920, 1080), color1=(10, 15, 30), color2=(30, 45, 70)):
    img = Image.new("RGB", size)
    draw = ImageDraw.Draw(img)
    for y in range(size[1]):
        r = int(color1[0] + (color2[0] - color1[0]) * y / size[1])
        g = int(color1[1] + (color2[1] - color1[1]) * y / size[1])
        b = int(color1[2] + (color2[2] - color1[2]) * y / size[1])
        draw.line([(0, y), (size[0], y)], fill=(r, g, b))
    for _ in range(20):
        x, y = random.randint(0, size[0]), random.randint(0, size[1])
        r = random.randint(20, 50)
        c = (random.randint(25, 60), random.randint(40, 80), random.randint(50, 100))
        draw.ellipse([x-r, y-r, x+r, y+r], fill=c, outline=None)
    return img.filter(ImageFilter.GaussianBlur(15))

def get_background_images():
    exts = ("*.jpg", "*.jpeg", "*.png", "*.webp")
    files = []
    for ext in exts:
        files.extend(glob.glob(os.path.join(IMAGES_DIR, ext)))
        files.extend(glob.glob(os.path.join(IMAGES_DIR, ext.upper())))
    return sorted(set(files))

def load_bg_image(size=(1920, 1080), niche="general-health"):
    if GEMINI_AVAILABLE:
        path = find_bg_image(niche, "landscape" if size[0] > size[1] else "portrait")
        if not path:
            img = create_procedural_bg(size, niche)
            if img:
                return img.resize(size, Image.LANCZOS)
        else:
            try:
                bg = Image.open(path).convert("RGB")
                return bg.resize(size, Image.LANCZOS)
            except:
                pass
    images = get_background_images()
    if images:
        path = random.choice(images)
        try:
            bg = Image.open(path).convert("RGB")
            bg = bg.resize(size, Image.LANCZOS)
            return bg
        except:
            pass
    return create_gradient_bg(size)

def create_glitch_overlay(size=(1920, 1080), intensity=0.15):
    overlay = Image.new("RGBA", size, (0, 0, 0, 0))
    draw = ImageDraw.Draw(overlay)
    for _ in range(int(size[1] * 0.08)):
        y = random.randint(0, size[1])
        h = random.randint(1, 3)
        alpha = random.randint(20, 60)
        draw.rectangle([(0, y), (size[0], y + h)], fill=(255, 255, 255, alpha))
    for _ in range(6):
        y = random.randint(0, size[1])
        h = random.randint(1, 2)
        alpha = random.randint(30, 80)
        draw.rectangle([(0, y), (size[0], y + h)], fill=(255, 50, 50, alpha))
    for _ in range(4):
        y = random.randint(0, size[1])
        h = random.randint(1, 2)
        alpha = random.randint(30, 80)
        draw.rectangle([(0, y), (size[0], y + h)], fill=(50, 100, 255, alpha))
    return overlay

def create_vignette(size=(1920, 1080)):
    img = Image.new("RGBA", size, (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    for i in range(400):
        alpha = int(max(0, 200 - i * 0.5))
        draw.ellipse([i, i, size[0]-i, size[1]-i], outline=(0, 0, 0, alpha), width=1)
    return img.filter(ImageFilter.GaussianBlur(30))

def create_scanline_overlay(size=(1920, 1080)):
    overlay = Image.new("RGBA", size, (0, 0, 0, 0))
    draw = ImageDraw.Draw(overlay)
    for y in range(0, size[1], 4):
        draw.line([(0, y), (size[0], y)], fill=(0, 0, 0, 15))
    return overlay

def create_gradient_overlay(size=(1920, 1080), accent_color=(255, 215, 0)):
    overlay = Image.new("RGBA", size, (0, 0, 0, 0))
    draw = ImageDraw.Draw(overlay)
    for y in range(size[1]):
        alpha = int(80 * (1 - y / size[1]) * (y / size[1]) * 4)
        draw.line([(0, y), (size[0], y)], fill=(accent_color[0], accent_color[1], accent_color[2], min(alpha, 60)))
    return overlay

def create_corner_accent(size=(1920, 1080), accent_color=(255, 215, 0)):
    overlay = Image.new("RGBA", size, (0, 0, 0, 0))
    draw = ImageDraw.Draw(overlay)
    for i in range(3, 8):
        w = i * 2
        draw.rectangle([(size[0] - w - 20, 20), (size[0] - 20, 20 + w)], fill=(*accent_color, 180))
    for i in range(3, 8):
        w = i * 2
        draw.rectangle([(20, 20), (20 + w, 20 + w)], fill=(*accent_color, 120))
    return overlay

def create_side_accent(size=(1920, 1080), accent_color=(255, 215, 0)):
    accent = Image.new("RGBA", (8, size[1]), (*accent_color, 200))
    base = Image.new("RGBA", size, (0, 0, 0, 0))
    base.paste(accent, (0, 0), accent)
    return base

def create_geometric_deco(size=(1920, 1080), accent_color=(255, 215, 0)):
    overlay = Image.new("RGBA", size, (0, 0, 0, 0))
    draw = ImageDraw.Draw(overlay)
    for _ in range(3):
        x = random.randint(50, size[0] - 100)
        y = random.randint(50, size[1] - 100)
        vertices = []
        cx, cy = x, y
        r = random.randint(30, 80)
        for i in range(6):
            angle = i * 60 + random.randint(-15, 15)
            rad = math.radians(angle)
            vertices.append((cx + r * math.cos(rad), cy + r * math.sin(rad)))
        draw.polygon(vertices, outline=(*accent_color, random.randint(40, 80)), width=2)
    return overlay

def create_product_card(product_name, niche="general-health", size=(1920, 1080), affiliate_url=""):
    color = NICHE_COLORS.get(niche, "#20B2AA")
    r, g, b = int(color[1:3], 16), int(color[3:5], 16), int(color[5:7], 16)
    
    bg = load_bg_image(size, niche=niche)
    bg = bg.filter(ImageFilter.GaussianBlur(4))
    
    overlay = Image.new("RGBA", size, (0, 0, 0, 130))
    bg = Image.alpha_composite(bg.convert("RGBA"), overlay).convert("RGB")
    
    gradient = create_gradient_overlay(size, (r, g, b))
    bg = Image.alpha_composite(bg.convert("RGBA"), gradient).convert("RGB")
    
    side = create_side_accent(size, (r, g, b))
    bg = Image.alpha_composite(bg.convert("RGBA"), side).convert("RGB")
    
    deco = create_geometric_deco(size, (r, g, b))
    bg = Image.alpha_composite(bg.convert("RGBA"), deco).convert("RGB")
    
    glitch = create_glitch_overlay(size, 0.1)
    bg = Image.alpha_composite(bg.convert("RGBA"), glitch)
    
    vignette = create_vignette(size)
    bg = Image.alpha_composite(bg, vignette)
    
    draw = ImageDraw.Draw(bg)
    
    prod_img_path = None
    if PRODUCT_IMAGES_AVAILABLE and affiliate_url:
        prod_img_path = get_product_image(product_name, affiliate_url)
    
    if prod_img_path and os.path.exists(prod_img_path):
        try:
            prod_img = Image.open(prod_img_path).convert("RGBA")
            pw, ph = prod_img.size
            max_w, max_h = 350, 350
            scale = min(max_w / pw, max_h / ph, 1.0)
            pw, ph = int(pw * scale), int(ph * scale)
            prod_img = prod_img.resize((pw, ph), Image.LANCZOS)
            shadow_img = Image.new("RGBA", (pw + 20, ph + 20), (0, 0, 0, 80))
            shadow_img = shadow_img.filter(ImageFilter.GaussianBlur(8))
            sx = (size[0] - shadow_img.width) // 2
            sy = size[1] // 2 - shadow_img.height // 2 - 20
            bg = Image.alpha_composite(bg.convert("RGBA"), shadow_img)
            bg.paste(shadow_img, (sx, sy), shadow_img)
            px = (size[0] - pw) // 2
            py = size[1] // 2 - ph // 2 - 20
            bg.paste(prod_img, (px, py), prod_img)
            bg = bg.convert("RGBA")
            name_y = size[1] // 2 + 200
        except:
            prod_img_path = None
            name_y = size[1] // 2 - 100
    
    if not prod_img_path or not os.path.exists(prod_img_path) if prod_img_path else True:
        name_y = size[1] // 2 - 100
    
    name = product_name if len(product_name) < 25 else product_name[:22] + "..."
    bbox = draw.textbbox((0, 0), name, font_size=100)
    nx = (size[0] - (bbox[2] - bbox[0])) // 2
    shadow = (0, 0, 0, 160)
    for dx, dy in [(4, 4), (0, 0)]:
        c = Image.new("RGBA", size, (0, 0, 0, 0))
        cd = ImageDraw.Draw(c)
        cd.text((nx + dx, name_y + dy), name, fill=shadow if dx != 0 else "#FFFFFF", font_size=100)
        if dx == 0:
            bg = Image.alpha_composite(bg, c)
    
    glow_bar_y = name_y + 100
    glow_bar = Image.new("RGBA", (min(600, len(name) * 18), 4), (r, g, b, 200))
    gx = (size[0] - glow_bar.width) // 2
    bg.paste(glow_bar, (gx, glow_bar_y), glow_bar)
    
    deco_line1 = Image.new("RGBA", size, (0, 0, 0, 0))
    d1 = ImageDraw.Draw(deco_line1)
    d1.rectangle([(gx - 80, glow_bar_y - 2), (gx - 20, glow_bar_y + 2)], fill=(r, g, b, 150))
    d1.rectangle([(gx + glow_bar.width + 20, glow_bar_y - 2), (gx + glow_bar.width + 80, glow_bar_y + 2)], fill=(r, g, b, 150))
    bg = Image.alpha_composite(bg, deco_line1)
    
    dots_y = glow_bar_y + 80
    for j in range(5):
        dx = size[0] // 2 - 120 + j * 60
        alpha = 255 if j == 2 else 80
        dot = Image.new("RGBA", (12, 12), (r, g, b, alpha))
        bg.paste(dot, (dx, dots_y), dot)
    
    scanlines = create_scanline_overlay(size)
    bg = Image.alpha_composite(bg, scanlines)
    
    corner = create_corner_accent(size, (r, g, b))
    bg = Image.alpha_composite(bg, corner)
    
    path = os.path.join(tempfile.gettempdir(), f"product_card_{random.randint(0,999999)}.png")
    bg.save(path)
    return path

def create_slide(text="", theme=None, duration=5, size=(1920, 1080), product_card_path=None, affiliate_url="", niche="general-health"):
    if theme is None:
        theme = random.choice(THEMES)
    r, g, b = int(theme["accent"][1:3], 16), int(theme["accent"][3:5], 16), int(theme["accent"][5:7], 16)
    
    img = load_bg_image(size, niche=niche)
    img = img.filter(ImageFilter.GaussianBlur(3))
    
    overlay = Image.new("RGBA", size, theme["fill"])
    img = Image.alpha_composite(img.convert("RGBA"), overlay).convert("RGB")
    
    gradient = create_gradient_overlay(size, (r, g, b))
    img = Image.alpha_composite(img.convert("RGBA"), gradient).convert("RGB")
    
    side = create_side_accent(size, (r, g, b))
    img = Image.alpha_composite(img.convert("RGBA"), side).convert("RGB")
    
    deco = create_geometric_deco(size, (r, g, b))
    img = Image.alpha_composite(img.convert("RGBA"), deco).convert("RGB")
    
    glitch = create_glitch_overlay(size, 0.12)
    img = Image.alpha_composite(img.convert("RGBA"), glitch)
    
    vignette = create_vignette(size)
    img = Image.alpha_composite(img, vignette)
    
    scanlines = create_scanline_overlay(size)
    img = Image.alpha_composite(img, scanlines)
    
    draw = ImageDraw.Draw(img)
    
    accent_glow = Image.new("RGBA", (400, 3), (r, g, b, 200))
    img.paste(accent_glow, (size[0] // 2 - 200, 80), accent_glow)
    accent_glow2 = Image.new("RGBA", (400, 3), (r, g, b, 200))
    img.paste(accent_glow2, (size[0] // 2 - 200, size[1] - 100), accent_glow2)
    
    label = "PRODUCT SPOTLIGHT"
    bbox = draw.textbbox((0, 0), label, font_size=28)
    lx = (size[0] - (bbox[2] - bbox[0])) // 2
    lb = Image.new("RGBA", size, (0, 0, 0, 0))
    ld = ImageDraw.Draw(lb)
    ld.text((lx, 110), label, fill=(r, g, b, 180), font_size=28)
    img = Image.alpha_composite(img, lb)
    
    if product_card_path and os.path.exists(product_card_path):
        try:
            card = Image.open(product_card_path).convert("RGBA")
            preview_size = card.copy()
            preview_size.thumbnail((500, 500), Image.LANCZOS)
            pw, ph = preview_size.size
            pcx = (size[0] - pw) // 2
            pcy = (size[1] - ph) // 2 + 40
            overlay_card = Image.new("RGBA", size, (0, 0, 0, 0))
            overlay_card.paste(preview_size, (pcx, pcy), preview_size)
            img = Image.alpha_composite(img, overlay_card)
        except:
            pass
    
    path = os.path.join(tempfile.gettempdir(), f"slide_{random.randint(0,999999)}.png")
    img.save(path)
    return path

def create_ending_card(product_name, niche="general-health", size=(1920, 1080), affiliate_url=""):
    color = NICHE_COLORS.get(niche, "#20B2AA")
    r, g, b = int(color[1:3], 16), int(color[3:5], 16), int(color[5:7], 16)
    
    img = load_bg_image(size, niche=niche)
    img = img.filter(ImageFilter.GaussianBlur(6))
    overlay = Image.new("RGBA", size, (0, 0, 0, 140))
    img = Image.alpha_composite(img.convert("RGBA"), overlay).convert("RGB")
    
    gradient = create_gradient_overlay(size, (r, g, b))
    img = Image.alpha_composite(img.convert("RGBA"), gradient).convert("RGB")
    
    glitch = create_glitch_overlay(size, 0.08)
    img = Image.alpha_composite(img.convert("RGBA"), glitch)
    
    vignette = create_vignette(size)
    img = Image.alpha_composite(img, vignette)
    
    draw = ImageDraw.Draw(img)
    
    cta = "CHECK OFFICIAL LINK"
    bbox = draw.textbbox((0, 0), cta, font_size=72)
    cx = (size[0] - (bbox[2] - bbox[0])) // 2
    for dx, dy in [(3, 3), (0, 0)]:
        c = Image.new("RGBA", size, (0, 0, 0, 0))
        cd = ImageDraw.Draw(c)
        cd.text((cx + dx, size[1] // 2 - 80 + dy), cta, fill=(0, 0, 0, 200) if dx != 0 else color, font_size=72)
        if dx == 0:
            img = Image.alpha_composite(img, c)
    
    sub = "Link in Description"
    bbox = draw.textbbox((0, 0), sub, font_size=36)
    sx = (size[0] - (bbox[2] - bbox[0])) // 2
    img_c = Image.new("RGBA", size, (0, 0, 0, 0))
    sd = ImageDraw.Draw(img_c)
    sd.text((sx, size[1] // 2 + 30), sub, fill=(255, 255, 255, 180), font_size=36)
    img = Image.alpha_composite(img, img_c)
    
    side = create_side_accent(size, (r, g, b))
    img = Image.alpha_composite(img, side)
    
    path = os.path.join(tempfile.gettempdir(), f"ending_{random.randint(0,999999)}.png")
    img.save(path)
    return path

def text_to_speech(text, lang="en", slow=False):
    tts = gTTS(text=text, lang=lang, slow=slow)
    path = os.path.join(tempfile.gettempdir(), "narration.mp3")
    tts.save(path)
    return path

def crossfade_transition(clip1, clip2, duration=0.5):
    return CompositeVideoClip([clip1, clip2.with_start(clip1.duration - duration).with_duration(duration).crossfadein(duration)])

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
    n_sections = len(script_sections)
    sec_per_section = total_duration / n_sections if n_sections > 0 else total_duration

    clips = []
    
    intro_path = create_product_card(product_name or title, niche, affiliate_url=affiliate_url)
    intro_clip = ImageClip(intro_path, duration=sec_per_section * 0.8)
    clips.append(intro_clip)

    for i, section in enumerate(script_sections):
        theme = THEMES[i % len(THEMES)]
        slide_path = create_slide(section["text"], theme=theme, duration=sec_per_section, product_card_path=product_card_path, affiliate_url=affiliate_url, niche=niche)
        clip = ImageClip(slide_path, duration=sec_per_section)
        clips.append(clip)

    ending_path = create_ending_card(product_name or title, niche, affiliate_url=affiliate_url)
    ending_clip = ImageClip(ending_path, duration=sec_per_section * 0.8)
    clips.append(ending_clip)

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
    print("YouTube Professional Video Creator v2")
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
