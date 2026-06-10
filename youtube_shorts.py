import json, os, sys, random, math, glob, tempfile
from pathlib import Path
from gtts import gTTS
from moviepy import *
from PIL import Image, ImageDraw, ImageFont, ImageFilter

BASE = Path(os.path.dirname(os.path.abspath(__file__)))
IMAGES_DIR = BASE / "images"
SHORTS_DIR = BASE / "output" / "shorts"
os.makedirs(SHORTS_DIR, exist_ok=True)

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

SHORTS_SIZE = (1080, 1920)

SHORTS_HOOKS = {
    "dental-health": [
        "Stop brushing your teeth wrong! #1 mistake 90% of people make",
        "Your toothpaste is destroying your gums. Here's the proof",
        "Dentists don't want you to know this secret",
        "I fixed my receding gums in 30 days with THIS",
        "The REAL reason your teeth are yellow (not what you think)",
    ],
    "blood-sugar": [
        "I ate THIS for breakfast and my blood sugar dropped 40%",
        "The 5-second trick that lowers blood sugar naturally",
        "Your blood sugar is spiking and you don't even know it",
        "Doctor shocked when my A1C went from 8.2 to 5.4",
        "Stop eating sugar? No. Do THIS instead.",
    ],
    "men-health": [
        "My testosterone went from 300 to 900 naturally in 60 days",
        "The #1 habit killing your testosterone (you do it daily)",
        "Men over 40 MUST do this every morning",
        "I reversed ED naturally. Here's exactly what I did",
        "This mineral deficiency is destroying men's health",
    ],
    "weight-loss": [
        "I lost 30 pounds in 60 days without the gym",
        "The 3AM snack that burns fat while you sleep",
        "Stop counting calories. Try THIS instead.",
        "Why you're NOT losing weight (fix this today)",
        "This one change melted my belly fat",
    ],
    "sleep-health": [
        "Falling asleep in 60 seconds? This breathing hack works",
        "The blue light lie they don't tell you about",
        "I cured my insomnia in 7 days without pills",
        "Melatonin is making your sleep WORSE. Do this instead",
        "Your bedroom is ruining your sleep (fix in 5 min)",
    ],
    "brain-health": [
        "I reversed brain fog in 2 weeks. Here's how",
        "The morning drink that makes you 10x smarter",
        "Neuroscientist: THIS destroys your memory every day",
        "Your brain is shrinking. Stop doing this NOW",
        "The 60-second exercise that boosts focus instantly",
    ],
    "gut-health": [
        "Your gut is screaming for help. Here are the signs",
        "I healed my leaky gut in 30 days (full protocol)",
        "The probiotic mistake 99% of people make",
        "Bloated ALL the time? Your gut is trying to tell you something",
        "This one food cured my bloating in 3 days",
    ],
    "eye-health": [
        "Your eyes are dying slowly from screens. Do this NOW",
        "I improved my eyesight naturally in 30 days",
        "The 20-20-20 rule changed my vision permanently",
        "These 3 nutrients saved my eyes from screens",
        "Optometrist: blue light glasses are a SCAM unless...",
    ],
    "stress-relief": [
        "Your cortisol is killing you. Here's how I fixed mine",
        "5 seconds to calm anxiety (backed by science)",
        "I was on anti-anxiety meds. Now I'm free. Here's how",
        "The vagus nerve hack that erased my stress",
        "Waking up at 3AM anxious? Your body is telling you this",
    ],
    "general-health": [
        "The supplement I take every day changed my life",
        "Doctors won't tell you this about supplements",
        "I tried 50 supplements so you don't have to (#7 shocked me)",
        "The green powder that replaced my coffee",
        "What happens to your body when you take THIS daily",
    ],
    "energy": [
        "I quit caffeine for 30 days. Here's what happened",
        "Tired all the time? Your mitochondria are starving",
        "The 2PM energy crash is NOT normal. Fix it",
        "I doubled my energy without coffee or energy drinks",
        "This supplement gave me more energy than espresso",
    ],
    "joint-health": [
        "I reversed my knee pain in 2 weeks (no surgery)",
        "Joint pain is NOT normal at any age. Here's why",
        "The collagen truth: what they don't tell you",
        "My arthritis pain vanished when I stopped doing THIS",
        "Glucosamine is a scam? Here's what actually works",
    ],
    "beauty": [
        "I reversed 10 years of aging in 3 months naturally",
        "The collagen mistake that makes you look OLDER",
        "My wrinkles disappeared when I took THIS every day",
        "The beauty secret Korean women don't share",
        "I stopped using expensive creams and did THIS instead",
    ],
    "detox": [
        "Your liver is clogged. Here are 5 warning signs",
        "I did a 7-day detox and this happened to my body",
        "The morning drink that cleanses your liver naturally",
        "Toxins are making you fat. Here's how to flush them",
        "I lost 10 pounds of water weight in one week",
    ],
}

SHORTS_CTAS = [
    "Link in bio to get yours with 60% OFF",
    "Limited time discount in the description below",
    "Get yours at the best price - link in description",
    "Try it risk-free with their money-back guarantee",
    "Click the link in description before the deal ends",
]

NICHE_COLORS = {
    "dental-health": "#1E90FF", "blood-sugar": "#32CD32", "men-health": "#4169E1",
    "sleep-health": "#6A5ACD", "gut-health": "#228B22", "weight-loss": "#FF6347",
    "brain-health": "#9932CC", "eye-health": "#00CED1", "stress-relief": "#DDA0DD",
    "detox": "#00FA9A", "joint-health": "#4682B4", "energy": "#FFD700", "beauty": "#FF69B4",
    "general-health": "#20B2AA", "minerals": "#87CEEB",
}

def get_background_images():
    exts = ("*.jpg", "*.jpeg", "*.png", "*.webp")
    files = []
    for ext in exts:
        files.extend(glob.glob(os.path.join(IMAGES_DIR, ext)))
        files.extend(glob.glob(os.path.join(IMAGES_DIR, ext.upper())))
    return sorted(set(files))

def load_bg_image(size=SHORTS_SIZE, niche="general-health"):
    if GEMINI_AVAILABLE:
        path = find_bg_image(niche, "portrait")
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
    img = Image.new("RGB", size, (random.randint(5,20), random.randint(5,20), random.randint(20,40)))
    return img

def wrap_text(text, font_size, max_width, font_path=None):
    try:
        font = ImageFont.truetype("arial.ttf", font_size)
    except:
        font = ImageFont.load_default()
    words = text.split()
    lines = []
    current = ""
    for w in words:
        test = current + " " + w if current else w
        bbox = font.getbbox(test)
        tw = bbox[2] - bbox[0]
        if tw > max_width:
            lines.append(current)
            current = w
        else:
            current = test
    if current:
        lines.append(current)
    return lines if lines else [text]

def create_shorts_frame(text, hook=True, color="#FFD700", overlay_intensity=80, niche="general-health"):
    bg = load_bg_image(niche=niche)
    bg = bg.filter(ImageFilter.GaussianBlur(random.randint(4, 8)))
    overlay = Image.new("RGBA", SHORTS_SIZE, (0, 0, 0, overlay_intensity))
    bg = Image.alpha_composite(bg.convert("RGBA"), overlay)

    r, g, b = int(color[1:3], 16), int(color[3:5], 16), int(color[5:7], 16)
    draw = ImageDraw.Draw(bg)

    accent_bar_h = 6
    for y_pos in [80, SHORTS_SIZE[1] - 100]:
        bar = Image.new("RGBA", (SHORTS_SIZE[0], accent_bar_h), (r, g, b, 200))
        bg.paste(bar, (0, y_pos), bar)

    corner_sz = 40
    margin = 30
    for cx, cy, hz, vt in [(margin, margin, 1, 1), (SHORTS_SIZE[0]-margin-corner_sz, margin, -1, 1),
                           (margin, SHORTS_SIZE[1]-margin-corner_sz, 1, -1), (SHORTS_SIZE[0]-margin-corner_sz, SHORTS_SIZE[1]-margin-corner_sz, -1, -1)]:
        for i in range(3, 7):
            s = i * 2
            draw.rectangle([cx, cy, cx + s * hz, cy + s * vt], outline=(r, g, b, 150), width=2)

    main_text_size = 64 if hook else 52
    line_height = main_text_size + 12

    lines = wrap_text(text, main_text_size, SHORTS_SIZE[0] - 120)
    total_h = len(lines) * line_height
    start_y = (SHORTS_SIZE[1] - total_h) // 2 - 40

    for i, line in enumerate(lines):
        ly = start_y + i * line_height
        bbox = draw.textbbox((0, 0), line, font_size=main_text_size)
        tw = bbox[2] - bbox[0]
        lx = (SHORTS_SIZE[0] - tw) // 2

        for dx, dy in [(0, 0), (3, 3)]:
            shadow = Image.new("RGBA", SHORTS_SIZE, (0, 0, 0, 0))
            sd = ImageDraw.Draw(shadow)
            fill_c = "#FFFFFF" if dx == 0 else (0, 0, 0, 200)
            sd.text((lx + dx, ly + dy), line, fill=fill_c, font_size=main_text_size)
            if dx == 0:
                bg = Image.alpha_composite(bg, shadow)

    glow_y = start_y - 10
    glow = Image.new("RGBA", (min(400, len(text) * 14), 3), (r, g, b, 180))
    gx = (SHORTS_SIZE[0] - glow.width) // 2
    bg.paste(glow, (gx, glow_y), glow)

    path = os.path.join(tempfile.gettempdir(), f"shorts_{random.randint(0,9999999)}.png")
    bg.save(path)
    return path

def create_shorts_ending(product_name, niche, affiliate_url):
    color = NICHE_COLORS.get(niche, "#20B2AA")
    r, g, b = int(color[1:3], 16), int(color[3:5], 16), int(color[5:7], 16)

    bg = load_bg_image(niche=niche)
    bg = bg.filter(ImageFilter.GaussianBlur(8))
    overlay = Image.new("RGBA", SHORTS_SIZE, (0, 0, 0, 140))
    bg = Image.alpha_composite(bg.convert("RGBA"), overlay)
    draw = ImageDraw.Draw(bg)

    cta = random.choice(SHORTS_CTAS)
    lines = wrap_text(cta, 56, SHORTS_SIZE[0] - 120)
    line_height = 68
    total_h = len(lines) * line_height
    start_y = (SHORTS_SIZE[1] - total_h) // 2 - 60

    for i, line in enumerate(lines):
        ly = start_y + i * line_height
        bbox = draw.textbbox((0, 0), line, font_size=56)
        lx = (SHORTS_SIZE[0] - (bbox[2] - bbox[0])) // 2
        for dx, dy in [(3, 3), (0, 0)]:
            c = Image.new("RGBA", SHORTS_SIZE, (0, 0, 0, 0))
            cd = ImageDraw.Draw(c)
            cd.text((lx + dx, ly + dy), line, fill=(0,0,0,200) if dx else color, font_size=56)
            if dx == 0:
                bg = Image.alpha_composite(bg, c)

    name_lines = wrap_text(product_name, 36, SHORTS_SIZE[0] - 120)
    for i, nl in enumerate(name_lines):
        ny = start_y + total_h + 40 + i * 48
        bbox = draw.textbbox((0, 0), nl, font_size=36)
        nx = (SHORTS_SIZE[0] - (bbox[2] - bbox[0])) // 2
        c = Image.new("RGBA", SHORTS_SIZE, (0, 0, 0, 0))
        cd = ImageDraw.Draw(c)
        cd.text((nx, ny), nl, fill=(255, 255, 255, 180), font_size=36)
        bg = Image.alpha_composite(bg, c)

    side = Image.new("RGBA", (6, SHORTS_SIZE[1]), (r, g, b, 200))
    bg.paste(side, (0, 0), side)
    side2 = Image.new("RGBA", (6, SHORTS_SIZE[1]), (r, g, b, 200))
    bg.paste(side2, (SHORTS_SIZE[0]-6, 0), side2)

    path = os.path.join(tempfile.gettempdir(), f"shorts_end_{random.randint(0,9999999)}.png")
    bg.save(path)
    return path

def create_shorts_intro(product_name, niche):
    color = NICHE_COLORS.get(niche, "#20B2AA")
    r, g, b = int(color[1:3], 16), int(color[3:5], 16), int(color[5:7], 16)
    bg = load_bg_image(niche=niche)
    bg = bg.filter(ImageFilter.GaussianBlur(5))
    overlay = Image.new("RGBA", SHORTS_SIZE, (0, 0, 0, 110))
    bg = Image.alpha_composite(bg.convert("RGBA"), overlay)
    draw = ImageDraw.Draw(bg)

    prod_img_path = None
    if PRODUCT_IMAGES_AVAILABLE:
        from product_image_fetcher import get_product_image
        prod_img_path = get_product_image(product_name, "")

    if prod_img_path and os.path.exists(prod_img_path):
        try:
            prod_img = Image.open(prod_img_path).convert("RGBA")
            pw, ph = prod_img.size
            scale = min(250 / pw, 250 / ph)
            pw, ph = int(pw * scale), int(ph * scale)
            prod_img = prod_img.resize((pw, ph), Image.LANCZOS)
            shadow = Image.new("RGBA", (pw+16, ph+16), (0,0,0,80))
            shadow = shadow.filter(ImageFilter.GaussianBlur(6))
            sx = (SHORTS_SIZE[0] - shadow.width) // 2
            sy = SHORTS_SIZE[1] // 2 - shadow.height // 2 - 40
            bg.paste(shadow, (sx, sy), shadow)
            px = (SHORTS_SIZE[0] - pw) // 2
            py = SHORTS_SIZE[1] // 2 - ph // 2 - 40
            bg.paste(prod_img, (px, py), prod_img)
            name_start_y = SHORTS_SIZE[1] // 2 + 180
        except:
            name_start_y = SHORTS_SIZE[1] // 2 - 60
    else:
        name_start_y = SHORTS_SIZE[1] // 2 - 60

    short_name = product_name if len(product_name) < 25 else product_name[:22] + "..."
    lines = wrap_text(short_name, 80, SHORTS_SIZE[0] - 100)
    line_height = 90
    total_h = len(lines) * line_height
    start_y = name_start_y - total_h // 2

    for i, line in enumerate(lines):
        ly = start_y + i * line_height
        bbox = draw.textbbox((0, 0), line, font_size=80)
        lx = (SHORTS_SIZE[0] - (bbox[2] - bbox[0])) // 2
        for dx, dy in [(0, 0), (4, 4)]:
            c = Image.new("RGBA", SHORTS_SIZE, (0, 0, 0, 0))
            cd = ImageDraw.Draw(c)
            cd.text((lx + dx, ly + dy), line, fill=(0,0,0,200) if dx else "#FFFFFF", font_size=80)
            if dx == 0:
                bg = Image.alpha_composite(bg, c)

    bar = Image.new("RGBA", (200, 4), (r, g, b, 200))
    bar_x = (SHORTS_SIZE[0] - 200) // 2
    bg.paste(bar, (bar_x, start_y - 15), bar)

    path = os.path.join(tempfile.gettempdir(), f"shorts_intro_{random.randint(0,9999999)}.png")
    bg.save(path)
    return path

def text_to_speech(text, lang="en"):
    tts = gTTS(text=text, lang=lang, slow=False)
    path = os.path.join(tempfile.gettempdir(), f"shorts_audio_{random.randint(0,9999999)}.mp3")
    tts.save(path)
    return path

def generate_shorts_video(product_name, niche, affiliate_url, lang="en"):
    hooks = SHORTS_HOOKS.get(niche, SHORTS_HOOKS["general-health"])
    selected_hooks = random.sample(hooks, min(3, len(hooks)))

    color = NICHE_COLORS.get(niche, "#20B2AA")

    sections = [
        ("intro", f"{product_name}", 3.0),
    ]

    for i, hook in enumerate(selected_hooks):
        sections.append((f"hook_{i}", hook, 8.0))

    full_text = " ".join(s[1] for s in sections)
    hook_text = " | ".join(selected_hooks)

    short_title = f"{selected_hooks[0][:60]} #shorts"
    short_desc = f"{hook_text}\n\nGet {product_name} with discount: {affiliate_url}\n\n#shorts #youtubeshorts #health #{niche.replace('-','')} #{product_name.replace(' ','')}"
    short_tags = ["shorts", "youtubeshorts", "health", niche.replace("-",""), product_name.replace(" ",""), product_name[:20]]

    print(f"  [Shorts] Creating: {short_title}")
    print(f"  [Shorts] Audio generation...")
    audio_path = text_to_speech(full_text, lang)
    audio_clip = AudioFileClip(audio_path)
    total_dur = audio_clip.duration

    print(f"  [Shorts] Generating frames ({total_dur:.1f}s)...")
    clips = []

    for i, (stype, text, min_dur) in enumerate(sections):
        if stype == "intro":
            frame = create_shorts_intro(product_name, niche)
        elif stype == "hook_0" or stype == "hook_1" or stype == "hook_2":
            frame = create_shorts_frame(text, hook=True, color=color, overlay_intensity=90 if random.random() > 0.5 else 70, niche=niche)
        else:
            frame = create_shorts_frame(text, hook=False, color=color, overlay_intensity=80, niche=niche)

        sec_dur = total_dur / len(sections)
        clip = ImageClip(frame, duration=sec_dur)
        clips.append(clip)

    video = concatenate_videoclips(clips, method="compose")
    video = video.with_audio(audio_clip)

    safe_name = product_name.replace(" ", "_").replace("-", "_")[:30]
    out_path = os.path.join(SHORTS_DIR, f"shorts_{safe_name}_{random.randint(100,999)}.mp4")
    video.write_videofile(out_path, fps=24, codec="libx264", audio_codec="aac", logger=None)
    print(f"  [OK] Short saved: {out_path}")

    return {
        "path": out_path,
        "title": short_title,
        "description": short_desc,
        "tags": short_tags,
        "product": product_name,
        "niche": niche,
    }

def generate_all_shorts():
    sys.path.insert(0, str(BASE))
    try:
        from youtube_content_automation import load_products
    except:
        print("[X] Cannot import products")
        return []

    products = load_products()
    if not products:
        print("[X] No products found")
        return []

    results = []
    for p in products:
        try:
            name = p["produkti"]
            url = p["url_origjinale"]
            niche = "general-health"
            for key, n in {
                "Gut":"gut-health","Dental":"dental-health","Blood":"blood-sugar","Sugar":"blood-sugar",
                "Men":"men-health","Prostate":"men-health","Testo":"men-health","Sleep":"sleep-health",
                "Insomni":"sleep-health","Brain":"brain-health","Memory":"brain-health","Vision":"eye-health",
                "Eye":"eye-health","Weight":"weight-loss","Keto":"weight-loss","Stress":"stress-relief",
                "Anxiety":"stress-relief","Joint":"joint-health","Collagen":"beauty","Beauty":"beauty",
                "Skin":"beauty","Detox":"detox","Energy":"energy","Mitochondrial":"energy",
            }.items():
                if key.lower() in name.lower():
                    niche = n
                    break
            result = generate_shorts_video(name, niche, url)
            results.append(result)
        except Exception as e:
            print(f"  [X] Short failed for {p.get('produkti','?')}: {e}")
    return results

if __name__ == "__main__":
    print("="*50)
    print("NaturalVitalityHub - YouTube Shorts Generator")
    print("="*50)
    results = generate_all_shorts()
    print(f"\nGenerated {len(results)} shorts in {SHORTS_DIR}")
