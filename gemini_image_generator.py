import json, os, sys, random, math, io, hashlib
from pathlib import Path
from PIL import Image, ImageDraw, ImageFilter, ImageEnhance

BASE = Path(os.path.dirname(os.path.abspath(__file__)))
IMAGES_DIR = BASE / "images"
os.makedirs(IMAGES_DIR, exist_ok=True)

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

def find_bg_image(niche, size="landscape"):
    for f in sorted(IMAGES_DIR.glob(f"human_{niche}_*.jpg"), reverse=True):
        return str(f)
    for f in sorted(IMAGES_DIR.glob("human_general-health_*.jpg")):
        return str(f)
    for f in sorted(IMAGES_DIR.glob(f"gen_{niche}_*.png")):
        return str(f)
    for f in sorted(IMAGES_DIR.glob("gen_general-health_*.png")):
        return str(f)
    return None

def load_bg_image(size=(1920, 1080), niche="general-health"):
    path = find_bg_image(niche, "landscape" if size[0] > size[1] else "portrait")
    if path:
        try:
            bg = Image.open(path).convert("RGB")
            return bg.resize(size, Image.LANCZOS)
        except:
            pass
    return create_procedural_bg(size, niche)

def create_procedural_bg(size=(1920, 1080), niche="general-health", seed=None):
    if seed is not None:
        random.seed(seed)
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
    r2, g2, b2 = palette["accent"]
    for _ in range(random.randint(3, 5)):
        cx = random.randint(100, size[0]-100)
        cy = random.randint(100, size[1]-100)
        rv = random.randint(60, 200)
        pts = []
        for i in range(random.randint(6, 10)):
            a = (i / 10) * 2 * math.pi
            vr = rv * random.uniform(0.6, 1.3)
            pts.append((cx + vr*math.cos(a), cy + vr*math.sin(a)))
        draw.polygon(pts, fill=(r2, g2, b2, random.randint(15, 40)))
    for _ in range(random.randint(20, 40)):
        x = random.randint(0, size[0])
        y = random.randint(0, size[1])
        pr = random.randint(1, 4)
        draw.ellipse([x-pr, y-pr, x+pr, y+pr], fill=(r2, g2, b2, random.randint(20, 80)))
    vn = Image.new("RGBA", size, (0, 0, 0, 0))
    vd = ImageDraw.Draw(vn)
    for i in range(200):
        a = int(max(0, 160 - i * 0.8))
        vd.ellipse([i, i, size[0]-i, size[1]-i], outline=(0, 0, 0, a), width=1)
    vn = vn.filter(ImageFilter.GaussianBlur(20))
    img = Image.alpha_composite(img, vn)
    ov = Image.new("RGBA", size, (0, 0, 0, 50))
    img = Image.alpha_composite(img, ov)
    img = img.filter(ImageFilter.GaussianBlur(random.uniform(0.5, 1.0)))
    return img.convert("RGB")

def generate_all_backgrounds():
    try:
        from human_images import generate_all
        generate_all()
    except:
        pass

if __name__ == "__main__":
    print("="*50)
    print("Image Generator - Human + Procedural")
    print("="*50)
    generate_all_backgrounds()
