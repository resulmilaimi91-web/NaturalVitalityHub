import json, os, sys, re, hashlib, io, requests
from pathlib import Path
from PIL import Image
from urllib.parse import urljoin, urlparse
from bs4 import BeautifulSoup

BASE = Path(os.path.dirname(os.path.abspath(__file__)))
PRODUCT_IMAGES_DIR = BASE / "images" / "products"
os.makedirs(PRODUCT_IMAGES_DIR, exist_ok=True)

CACHE_FILE = BASE / "images" / "product_image_cache.json"
if CACHE_FILE.exists():
    IMAGE_CACHE = json.loads(CACHE_FILE.read_text())
else:
    IMAGE_CACHE = {}

def save_cache():
    CACHE_FILE.write_text(json.dumps(IMAGE_CACHE, indent=2))

def get_product_image(product_name, affiliate_url, force_fetch=False):
    key = hashlib.md5(product_name.encode()).hexdigest()[:16]
    local_path = PRODUCT_IMAGES_DIR / f"{key}.png"

    if local_path.exists() and not force_fetch:
        return str(local_path)

    if key in IMAGE_CACHE and not force_fetch:
        cached_url = IMAGE_CACHE[key]
        return _download_and_save(cached_url, local_path, product_name)

    found_url = _scrape_product_image(affiliate_url, product_name)
    if found_url:
        IMAGE_CACHE[key] = found_url
        save_cache()
        return _download_and_save(found_url, local_path, product_name)

    found_url = _try_known_urls(product_name)
    if found_url:
        IMAGE_CACHE[key] = found_url
        save_cache()
        return _download_and_save(found_url, local_path, product_name)

    return None

def _download_and_save(url, local_path, product_name):
    try:
        headers = {"User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36"}
        resp = requests.get(url, headers=headers, timeout=10)
        if resp.status_code != 200:
            return None
        img = Image.open(io.BytesIO(resp.content))
        img = img.convert("RGBA")
        img.thumbnail((400, 400), Image.LANCZOS)
        final = Image.new("RGBA", img.size, (0, 0, 0, 0))
        if img.mode == "RGBA":
            final.paste(img, (0, 0), img)
        else:
            final.paste(img, (0, 0))
        final.save(local_path, "PNG")
        print(f"  [OK] Saved product image: {product_name}")
        return str(local_path)
    except Exception as e:
        print(f"  [i] Could not download {product_name}: {e}")
        return None

def _scrape_product_image(url, product_name):
    headers = {
        "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36"
    }
    clean_url = url.split("#")[0]
    try:
        resp = requests.get(clean_url, headers=headers, timeout=15, allow_redirects=True)
        if resp.status_code != 200:
            return None
        soup = BeautifulSoup(resp.text, "html.parser")
        og_img = soup.find("meta", property="og:image")
        if og_img and og_img.get("content"):
            val = og_img["content"]
            if val.startswith("http"):
                return val
            return urljoin(clean_url, val)
        for img in soup.find_all("img"):
            src = img.get("src", "") or img.get("data-src", "") or ""
            if not src:
                continue
            src_lower = src.lower()
            keywords = ["bottle", "product", "pack", "supplement", "main-img", "hero-img",
                       "bottle-img", "product-img", "featured", "main-image"]
            classes = " ".join(img.get("class", [])).lower()
            if any(k in src_lower for k in keywords) or any(k in classes for k in keywords):
                full = urljoin(clean_url, src)
                if full.startswith("http"):
                    return full
        for img in soup.find_all("img"):
            src = img.get("src", "") or img.get("data-src", "") or ""
            if not src or src.endswith((".svg", ".ico", ".gif")):
                continue
            full = urljoin(clean_url, src)
            if full.startswith("http"):
                return full
    except Exception as e:
        print(f"  [i] Scrape failed for {product_name}: {e}")
    return None

def _try_known_urls(product_name):
    name_lower = product_name.lower()
    domain_map = {
        "advanced amino": "advancedbionutritionals.com/images/products/advanced-amino-muscle-mass.png",
        "prostate": "advancedbionutritionals.com/images/products/prostate-formula.png",
        "memory": "advancedbionutritionals.com/images/products/memory-formula.png",
        "mitochondrial": "advancedbionutritionals.com/images/products/mitochondrial-formula.png",
        "collagen": "advancedbionutritionals.com/images/products/collagen-formula.png",
        "digestive": "advancedbionutritionals.com/images/products/digestive-formula.png",
        "prime perform": "advancedbionutritionals.com/images/products/prime-perform.png",
        "endopeak": "advancedbionutritionals.com/images/products/endopeak.png",
        "audifort": "advancedbionutritionals.com/images/products/audifort.png",
        "cellucare": "advancedbionutritionals.com/images/products/cellucare.png",
        "vision": "advancedbionutritionals.com/images/products/advanced-vision-formula.png",
        "insomniac": "advancedbionutritionals.com/images/products/insomniac.png",
        "lion care": "advancedbionutritionals.com/images/products/lion-care.png",
        "pineal guardian": "advancedbionutritionals.com/images/products/pineal-guardian.png",
        "detox": "advancedbionutritionals.com/images/products/liver-detox-cleanse.png",
        "immune": "advancedbionutritionals.com/images/products/immune-boost-system.png",
        "probiotic 40": "advancedbionutritionals.com/images/products/probiotic-40-billion.png",
        "omega 3": "advancedbionutritionals.com/images/products/omega-3-fish-oil.png",
        "vitamin d3": "advancedbionutritionals.com/images/products/vitamin-d3-k2.png",
        "green superfood": "advancedbionutritionals.com/images/products/green-superfood.png",
        "mct oil": "advancedbionutritionals.com/images/products/mct-oil-premium.png",
        "magnesium": "advancedbionutritionals.com/images/products/magnesium-glycinate.png",
        "ashwagandha": "advancedbionutritionals.com/images/products/ashwagandha-stress.png",
        "turmeric": "advancedbionutritionals.com/images/products/turmeric-curcumin.png",
        "joint ease": "advancedbionutritionals.com/images/products/joint-ease.png",
        "energy boosting": "advancedbionutritionals.com/images/products/energy-boosting-formula.png",
        "anti aging cream": "advancedbionutritionals.com/images/products/anti-aging-cream.png",
        "retinol": "advancedbionutritionals.com/images/products/retinol-night-cream.png",
        "hyaluronic": "advancedbionutritionals.com/images/products/hyaluronic-acid.png",
        "vitamin c": "advancedbionutritionals.com/images/products/vitamin-c-serum.png",
        "collagen peptides": "advancedbionutritionals.com/images/products/collagen-peptides.png",
    }
    for key, path in domain_map.items():
        if key in name_lower:
            return f"https://www.{path}"
    return None

def ensure_all_product_images(products):
    results = []
    for p in products:
        name = p.get("produkti", "")
        url = p.get("url_origjinale", "")
        if not name or not url:
            continue
        path = get_product_image(name, url)
        if path:
            results.append((name, path))
        else:
            print(f"  [X] {name}: no image")
    return results

if __name__ == "__main__":
    print("="*50)
    print("Product Image Fetcher")
    print("="*50)
    sys.path.insert(0, str(BASE))
    from youtube_content_automation import load_products
    products = load_products()[:10]
    results = ensure_all_product_images(products)
    print(f"\nGot {len(results)}/{len(products)} product images")
