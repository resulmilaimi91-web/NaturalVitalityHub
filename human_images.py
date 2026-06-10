import json, os, sys, random, math, io, hashlib, requests
from pathlib import Path
from PIL import Image, ImageDraw, ImageFilter, ImageEnhance
from urllib.parse import quote

BASE = Path(os.path.dirname(os.path.abspath(__file__)))
IMAGES_DIR = BASE / "images"
os.makedirs(IMAGES_DIR, exist_ok=True)

NICHE_SEARCH = {
    "dental-health": "smiling person showing teeth dental checkup",
    "blood-sugar": "person checking blood glucose kitchen healthy food",
    "men-health": "fit muscular man training gym workout",
    "sleep-health": "person sleeping peacefully bedroom relaxation",
    "gut-health": "person holding fresh vegetables healthy digestion",
    "weight-loss": "fit person measuring waist weight loss transformation",
    "brain-health": "person meditating focused mental clarity brain",
    "eye-health": "person looking at nature eyes vision",
    "stress-relief": "person meditating peaceful yoga relaxation",
    "detox": "person drinking green juice detox healthy lifestyle",
    "joint-health": "active senior stretching joint pain relief",
    "energy": "athletic person running sunrise energetic workout",
    "beauty": "beautiful woman glowing skincare wellness",
    "general-health": "healthy person active lifestyle wellness",
    "minerals": "person holding vitamins supplements health",
}

def download_stock_image(search_query, size="landscape", index=0):
    w = 1920 if size == "landscape" else 1080
    h = 1080 if size == "landscape" else 1920
    orientation = "landscape" if size == "landscape" else "portrait"
    query = quote(search_query + " fitness health person")
    
    unsplash_url = f"https://images.unsplash.com/photo-{random.choice(range(1500000000, 1600000000))}?w={w}&h={h}&fit=crop&q=80"
    
    sources = [
        f"https://source.unsplash.com/{w}x{h}/?{query}",
        f"https://picsum.photos/{w}/{h}?random={index}",
    ]
    
    for url in sources:
        try:
            resp = requests.get(url, headers={"User-Agent": "Mozilla/5.0"}, timeout=15, allow_redirects=True)
            if resp.status_code == 200 and len(resp.content) > 5000:
                img = Image.open(io.BytesIO(resp.content))
                img = img.convert("RGB").resize((w, h), Image.LANCZOS)
                return img
        except:
            continue
    return None

def generate_niche_images(niche, product_name="", count=2):
    search = NICHE_SEARCH.get(niche, NICHE_SEARCH["general-health"])
    results = []
    
    for i in range(count):
        fname = IMAGES_DIR / f"human_{niche}_{i}.jpg"
        if fname.exists():
            results.append(str(fname))
            continue
        
        img = download_stock_image(search, "landscape" if i % 2 == 0 else "portrait", i)
        if img:
            img.save(fname, quality=90)
            results.append(str(fname))
            print(f"  [OK] {fname.name}")
        else:
            print(f"  [X] Failed {niche} #{i}")
    
    return results

def ensure_niche_human_images(niche):
    existing = list(IMAGES_DIR.glob(f"human_{niche}_*.jpg"))
    if len(existing) >= 2:
        return [str(f) for f in existing]
    return generate_niche_images(niche)

def generate_all():
    for niche in NICHE_SEARCH:
        ensure_niche_human_images(niche)
    print("Done!")

def find_human_image(niche):
    files = sorted(IMAGES_DIR.glob(f"human_{niche}_*.jpg"))
    if files:
        return str(random.choice(files))
    files = sorted(IMAGES_DIR.glob("human_general-health_*.jpg"))
    if files:
        return str(random.choice(files))
    for f in sorted(IMAGES_DIR.glob("human_*.jpg")):
        return str(f)
    return None

if __name__ == "__main__":
    print("="*50)
    generate_all()
