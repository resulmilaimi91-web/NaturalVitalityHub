import json, os, sys, random, io, base64, time
from pathlib import Path
from google import genai
from PIL import Image, ImageDraw, ImageFont, ImageFilter, ImageEnhance
import requests

BASE = Path(os.path.dirname(os.path.abspath(__file__)))
IMAGES_DIR = BASE / "images"
os.makedirs(IMAGES_DIR, exist_ok=True)

GEMINI_API_KEY = os.environ.get("GEMINI_API_KEY", "")
if not GEMINI_API_KEY:
    config_path = BASE / "config.json"
    if config_path.exists():
        try:
            import json as _json
            GEMINI_API_KEY = _json.loads(config_path.read_text()).get("GEMINI_API_KEY", "")
        except:
            pass

if not GEMINI_API_KEY:
    print("[X] GEMINI_API_KEY not set")
    GEMINI_API_KEY = ""

client = genai.Client(api_key=GEMINI_API_KEY) if GEMINI_API_KEY else None

NICHE_PROMPTS = {
    "dental-health": "dental clinic sterile clean, professional teeth model, blue white medical, bright lighting",
    "blood-sugar": "fresh vegetables fruits, blood glucose meter, warm healthy kitchen, natural organic",
    "men-health": "athletic silhouette, dramatic dark blue lighting, luxury masculine, gym fitness",
    "sleep-health": "peaceful bedroom, moonlight, calm serene atmosphere, dark blue relaxing",
    "gut-health": "fresh vegetables garden, probiotics, healthy digestion, green natural vibrant",
    "weight-loss": "measuring tape, fit body, motivational fitness, healthy lifestyle transformation",
    "brain-health": "brain neural network glowing, purple blue futuristic, cognitive mental",
    "eye-health": "eye vision, green nature, optometry chart, blue light protection",
    "stress-relief": "calm meditation zen, nature spa peaceful, lavender warm relaxing",
    "detox": "clean water lemon mint, fresh green detox, liver cleanse natural",
    "joint-health": "active stretching, joint mobility, warm orange, healthy movement",
    "energy": "sunrise morning energy, lightning bolt vitality, gold yellow dynamic",
    "beauty": "glowing skin, collagen beauty, pink gold luxury, wellness spa",
    "general-health": "natural supplements, wooden table, green leaves, clean wellness",
    "minerals": "colorful vitamins minerals, health supplement ingredients, scientific clean",
}

def generate_niche_backgrounds():
    for niche, style in NICHE_PROMPTS.items():
        for i in range(2):
            fname = IMAGES_DIR / f"gemini_{niche}_{i}.jpg"
            if fname.exists():
                continue
            try:
                prompt = f"Professional stock photo: {style}. 1920x1080, 16:9 landscape, soft natural lighting, photorealistic, no text, no people, 4K quality"
                resp = client.models.generate_content(
                    model="gemini-3.1-flash-image",
                    contents=prompt
                )
                for part in resp.candidates[0].content.parts:
                    if hasattr(part, 'inline_data') and part.inline_data:
                        img = Image.open(io.BytesIO(part.inline_data.data))
                        img = img.convert("RGB").resize((1920, 1080), Image.LANCZOS)
                        img.save(fname, quality=92)
                        print(f"  [OK] {fname}")
                        break
                else:
                    continue
                time.sleep(1)
            except Exception as e:
                print(f"  [X] Failed {niche} #{i}: {e}")
                if "quota" in str(e).lower() or "429" in str(e):
                    time.sleep(30)

def generate_all_backgrounds():
    print("Generating Gemini backgrounds for all niches...")
    generate_niche_backgrounds()
    print("Done!")

def get_image(prompt, niche, size="landscape"):
    dims = "1920x1080 16:9" if size == "landscape" else "1080x1920 9:16"
    full_prompt = f"Professional stock photo: {prompt}. {dims}, soft natural lighting, photorealistic, no text, 4K"
    for attempt in range(3):
        try:
            resp = client.models.generate_content(
                model="gemini-3.1-flash-image",
                contents=full_prompt
            )
            for part in resp.candidates[0].content.parts:
                if hasattr(part, 'inline_data') and part.inline_data:
                    img = Image.open(io.BytesIO(part.inline_data.data))
                    img = img.convert("RGB")
                    return img
        except Exception as e:
            if "quota" in str(e).lower() or "429" in str(e):
                time.sleep(30)
            else:
                break
    return None

def find_local_gemini_image(niche):
    for f in sorted(IMAGES_DIR.glob(f"gemini_{niche}_*.jpg"), reverse=True):
        return str(f)
    for f in sorted(IMAGES_DIR.glob("gemini_general-health_*.jpg"), reverse=True):
        return str(f)
    for f in sorted(IMAGES_DIR.glob("gemini_*.jpg")):
        return str(f)
    return None

generate = generate_all_backgrounds if __name__ == "__main__" else lambda: None
cleanup = lambda: None
