import json, os, sys, random, math, io, hashlib
from pathlib import Path
from PIL import Image, ImageDraw, ImageFilter, ImageFont, ImageEnhance

BASE = Path(os.path.dirname(os.path.abspath(__file__)))
IMAGES_DIR = BASE / "images"
os.makedirs(IMAGES_DIR, exist_ok=True)

sys.path.insert(0, str(BASE))
try:
    from google import genai
    GEMINI_API_KEY = os.environ.get("GEMINI_API_KEY", "")
    if not GEMINI_API_KEY:
        cfg = BASE / "config.json"
        if cfg.exists():
            GEMINI_API_KEY = json.loads(cfg.read_text()).get("GEMINI_API_KEY", "")
    GEMINI_CLIENT = genai.Client(api_key=GEMINI_API_KEY) if GEMINI_API_KEY else None
except:
    GEMINI_CLIENT = None

NICHE_PALETTES = {
    "dental-health": {"bg1": (10, 30, 60), "bg2": (30, 60, 100), "accent": (30, 144, 255), "glow": "blue"},
    "blood-sugar": {"bg1": (10, 40, 10), "bg2": (30, 70, 30), "accent": (50, 205, 50), "glow": "green"},
    "men-health": {"bg1": (20, 20, 50), "bg2": (40, 40, 80), "accent": (65, 105, 225), "glow": "blue"},
    "sleep-health": {"bg1": (15, 10, 40), "bg2": (30, 25, 60), "accent": (106, 90, 205), "glow": "purple"},
    "gut-health": {"bg1": (10, 35, 15), "bg2": (20, 55, 30), "accent": (34, 139, 34), "glow": "green"},
    "weight-loss": {"bg1": (50, 15, 10), "bg2": (80, 30, 20), "accent": (255, 99, 71), "glow": "red"},
    "brain-health": {"bg1": (30, 10, 40), "bg2": (50, 20, 60), "accent": (153, 50, 204), "glow": "purple"},
    "eye-health": {"bg1": (10, 30, 35), "bg2": (20, 50, 55), "accent": (0, 206, 209), "glow": "cyan"},
    "stress-relief": {"bg1": (40, 20, 40), "bg2": (60, 35, 55), "accent": (221, 160, 221), "glow": "pink"},
    "detox": {"bg1": (10, 40, 20), "bg2": (20, 60, 35), "accent": (0, 250, 154), "glow": "green"},
    "joint-health": {"bg1": (20, 30, 40), "bg2": (35, 50, 65), "accent": (70, 130, 180), "glow": "blue"},
    "energy": {"bg1": (40, 30, 10), "bg2": (60, 45, 20), "accent": (255, 215, 0), "glow": "gold"},
    "beauty": {"bg1": (40, 15, 30), "bg2": (60, 25, 45), "accent": (255, 105, 180), "glow": "pink"},
    "general-health": {"bg1": (15, 35, 30), "bg2": (25, 55, 45), "accent": (32, 178, 170), "glow": "teal"},
    "minerals": {"bg1": (20, 25, 35), "bg2": (35, 45, 55), "accent": (135, 206, 235), "glow": "blue"},
}

def get_gemini_style_prompt(niche, product_name=""):
    if not GEMINI_CLIENT:
        return None
    try:
        resp = GEMINI_CLIENT.models.generate_content(
            model="gemini-2.0-flash",
            contents=f"Describe a beautiful abstract health-themed background image for a video about {product_name or niche} ({niche} niche). Describe shapes, colors, lighting, patterns. Make it sound visually stunning. Max 3 sentences."
        )
        return resp.text.strip() if resp.text else None
    except:
        return None

def create_organic_shape(draw, size, palette, complexity=0.7):
    cx, cy = size[0] * random.uniform(0.15, 0.85), size[1] * random.uniform(0.15, 0.85)
    r = random.randint(80, 250)
    points = []
    segments = random.randint(8, 16)
    for i in range(segments):
        angle = (i / segments) * 2 * math.pi
        variance = random.uniform(0.6, 1.4) * complexity
        pr = r * variance
        x = cx + pr * math.cos(angle)
        y = cy + pr * math.sin(angle)
        points.append((x, y))
    r2, g2, b2 = palette["accent"]
    alpha = random.randint(20, 60)
    draw.polygon(points, fill=(r2, g2, b2, alpha), outline=None)
    for i in range(len(points)):
        p1 = points[i]
        p2 = points[(i + 1) % len(points)]
        draw.line([p1, p2], fill=(r2, g2, b2, min(alpha + 30, 200)), width=random.randint(1, 3))

def create_circles(draw, size, palette):
    for _ in range(random.randint(8, 20)):
        x = random.randint(0, size[0])
        y = random.randint(0, size[1])
        r = random.randint(10, 100)
        r2, g2, b2 = palette["accent"]
        alpha = random.randint(10, 40)
        draw.ellipse([x-r, y-r, x+r, y+r], outline=(r2, g2, b2, alpha), width=random.randint(1, 3))

def create_light_leaks(draw, size, palette):
    for _ in range(random.randint(3, 6)):
        x = random.randint(0, size[0])
        y = random.randint(0, size[1])
        r = random.randint(100, 300)
        r2, g2, b2 = palette["accent"]
        alpha = random.randint(5, 20)
        for i in range(5):
            ir = r - i * 20
            if ir <= 0: break
            draw.ellipse([x-ir, y-ir, x+ir, y+ir], outline=(r2, g2, b2, max(0, alpha - i * 3)), width=1)

def create_grid_lines(draw, size, palette):
    step = random.randint(60, 120)
    r2, g2, b2 = palette["accent"]
    for x in range(0, size[0], step):
        draw.line([(x, 0), (x, size[1])], fill=(r2, g2, b2, random.randint(5, 15)))

def create_particles(draw, size, palette):
    r2, g2, b2 = palette["accent"]
    for _ in range(random.randint(30, 80)):
        x = random.randint(0, size[0])
        y = random.randint(0, size[1])
        pr = random.randint(1, 5)
        alpha = random.randint(30, 120)
        draw.ellipse([x-pr, y-pr, x+pr, y+pr], fill=(r2, g2, b2, alpha))

def create_diagonal_stripes(draw, size, palette):
    r2, g2, b2 = palette["accent"]
    step = random.randint(40, 80)
    for i in range(-size[1], size[0] + size[1], step):
        alpha = random.randint(3, 10)
        draw.line([(i, 0), (i + size[1], size[1])], fill=(r2, g2, b2, alpha))

ELEMENTS = [
    create_organic_shape, create_circles, create_light_leaks,
    create_grid_lines, create_particles, create_diagonal_stripes
]

def generate_procedural_bg(size=(1920, 1080), niche="general-health", seed=None):
    if seed is not None:
        random.seed(seed)
    else:
        random.seed(random.randint(0, 999999))

    palette = NICHE_PALETTES.get(niche, NICHE_PALETTES["general-health"])
    img = Image.new("RGBA", size)
    draw = ImageDraw.Draw(img)

    bg1, bg2 = palette["bg1"], palette["bg2"]
    for y in range(size[1]):
        t = y / size[1]
        r = int(bg1[0] + (bg2[0] - bg1[0]) * t)
        g = int(bg1[1] + (bg2[1] - bg1[1]) * t)
        b = int(bg1[2] + (bg2[2] - bg1[2]) * t)
        draw.line([(0, y), (size[0], y)], fill=(r, g, b))

    num_elements = random.randint(3, 5)
    selected = random.sample(ELEMENTS, min(num_elements, len(ELEMENTS)))
    for element in selected:
        element(draw, size, palette)

    vignette = Image.new("RGBA", size, (0, 0, 0, 0))
    vd = ImageDraw.Draw(vignette)
    for i in range(300):
        alpha = int(max(0, 180 - i * 0.6))
        vd.ellipse([i, i, size[0]-i, size[1]-i], outline=(0, 0, 0, alpha), width=1)
    vignette = vignette.filter(ImageFilter.GaussianBlur(25))
    img = Image.alpha_composite(img, vignette)

    overlay = Image.new("RGBA", size, (0, 0, 0, 60))
    img = Image.alpha_composite(img, overlay)

    img = img.filter(ImageFilter.GaussianBlur(random.uniform(0.5, 1.5)))
    enhancer = ImageEnhance.Contrast(img.convert("RGB"))
    img = enhancer.enhance(random.uniform(1.05, 1.15))
    img = img.convert("RGBA")

    return img

def get_cache_key(niche, product_name="", size_type="landscape"):
    raw = f"{niche}_{product_name}_{size_type}"
    return hashlib.md5(raw.encode()).hexdigest()[:12]

def get_bg_image(size=(1920, 1080), niche="general-health", product_name=""):
    seed_str = f"{niche}_{product_name or niche}"
    seed_val = abs(hash(seed_str)) % (10**8)
    return generate_procedural_bg(size, niche, seed=seed_val)

def generate_and_cache(niche, product_name="", count=3, size_type="landscape"):
    w, h = (1920, 1080) if size_type == "landscape" else (1080, 1920)
    generated = []
    for i in range(count):
        base_seed = abs(hash(f"{niche}_{product_name}_{i}")) % (10**8)
        img = generate_procedural_bg((w, h), niche, seed=base_seed + i)
        fname = IMAGES_DIR / f"gemini_{niche}_{product_name[:10] if product_name else ''}_{i}.png"
        fname = IMAGES_DIR / f"gen_{niche}_{i}.png"
        img.convert("RGB").save(fname, quality=90)
        generated.append(str(fname))
    return generated

def ensure_niche_images(niche):
    existing = list(IMAGES_DIR.glob(f"gen_{niche}_*.png"))
    if len(existing) >= 2:
        return [str(f) for f in existing[:2]]
    return generate_and_cache(niche)

def generate_all_backgrounds():
    print("Generating procedural backgrounds for all niches...")
    for niche in NICHE_PALETTES:
        ensure_niche_images(niche)
        print(f"  [OK] {niche}")
    print("Done!")

def find_niche_image(niche):
    files = sorted(IMAGES_DIR.glob(f"gen_{niche}_*.png"))
    if files:
        return str(random.choice(files))
    files = sorted(IMAGES_DIR.glob("gen_general-health_*.png"))
    if files:
        return str(random.choice(files))
    for f in sorted(IMAGES_DIR.glob("gen_*.png")):
        return str(f)
    return None

if __name__ == "__main__":
    print("="*50)
    print("Gemini Procedural Image Generator")
    print("="*50)
    generate_all_backgrounds()
    print("\nSample images ready!")
