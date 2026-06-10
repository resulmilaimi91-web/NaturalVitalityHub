import json, os, sys, re, hashlib, io, requests, tempfile
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

HEADERS = {"User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36"}

def save_cache():
    CACHE_FILE.write_text(json.dumps(IMAGE_CACHE, indent=2))

def get_product_image(product_name, affiliate_url, force_fetch=False):
    key = hashlib.md5(product_name.encode()).hexdigest()[:16]
    local_path = PRODUCT_IMAGES_DIR / f"{key}.png"

    if local_path.exists() and not force_fetch:
        return str(local_path)

    if key in IMAGE_CACHE and not force_fetch:
        cached_url = IMAGE_CACHE[key]
        result = _download_and_save(cached_url, local_path, product_name)
        if result:
            return result

    found_url = _scrape_product_image(affiliate_url, product_name)
    if found_url:
        IMAGE_CACHE[key] = found_url
        save_cache()
        result = _download_and_save(found_url, local_path, product_name)
        if result:
            return result

    found_url = _try_google_images(product_name)
    if found_url:
        IMAGE_CACHE[key] = found_url
        save_cache()
        result = _download_and_save(found_url, local_path, product_name)
        if result:
            return result

    return None

def _download_and_save(url, local_path, product_name):
    try:
        resp = requests.get(url, headers=HEADERS, timeout=15)
        if resp.status_code != 200:
            return None
        img_data = resp.content
        img = Image.open(io.BytesIO(img_data))
        img = img.convert("RGBA")
        img.thumbnail((400, 400), Image.LANCZOS)
        final = Image.new("RGBA", img.size, (0, 0, 0, 0))
        if img.mode == "RGBA":
            final.paste(img, (0, 0), img)
        else:
            final.paste(img, (0, 0))
        final.save(local_path, "PNG")
        print(f"  [PROD IMG] {product_name}")
        return str(local_path)
    except Exception as e:
        return None

def _scrape_product_image(url, product_name):
    clean_url = url.split("#")[0]
    try:
        resp = requests.get(clean_url, headers=HEADERS, timeout=15, allow_redirects=True)
        if resp.status_code != 200:
            return None
        final_url = resp.url
        soup = BeautifulSoup(resp.text, "html.parser")

        og_img = soup.find("meta", property="og:image")
        if og_img and og_img.get("content"):
            val = og_img["content"]
            return urljoin(final_url, val) if not val.startswith("http") else val

        for img in soup.find_all("img"):
            src = img.get("src", "") or img.get("data-src", "") or ""
            if not src:
                continue
            keywords = ["bottle", "product", "pack", "supplement", "main-img", "hero-img",
                       "bottle-img", "product-img", "featured", "main-image", "item-img"]
            classes = " ".join(img.get("class", [])).lower()
            if any(k in src.lower() for k in keywords) or any(k in classes for k in keywords):
                full = urljoin(final_url, src)
                if full.startswith("http"):
                    return full

        for img in soup.find_all("img"):
            src = img.get("src", "") or img.get("data-src", "") or ""
            if not src or src.endswith((".svg", ".ico", ".gif")):
                continue
            full = urljoin(final_url, src)
            if full.startswith("http"):
                return full
    except:
        pass
    return None

def _try_google_images(product_name):
    search_name = product_name.replace(" - ", " ").split(" - ")[0].strip()
    search_query = search_name.replace(" ", "+") + "+supplement+bottle"
    url = f"https://www.google.com/search?tbm=isch&q={search_query}"
    try:
        resp = requests.get(url, headers={**HEADERS, "Accept": "text/html"}, timeout=10)
        if resp.status_code != 200:
            return None
        urls = re.findall(r'<img[^>]+src=["\'](https?://[^"\']+)["\']', resp.text)
        for img_url in urls:
            if any(k in img_url.lower() for k in [".jpg", ".png", ".webp", ".jpeg"]):
                if not any(k in img_url.lower() for k in ["logo", "icon", "avatar", "google", "gstatic"]):
                    return img_url
    except:
        pass
    return None

def ensure_product_image_for_all(products):
    count = 0
    for p in products:
        name = p.get("produkti", "")
        url = p.get("url_origjinale", "")
        if not name:
            continue
        path = get_product_image(name, url)
        if path:
            count += 1
        else:
            print(f"  [NO IMG] {name}")
    return count

if __name__ == "__main__":
    print("="*50)
    print("Product Image Fetcher - Fetch ALL")
    print("="*50)
    sys.path.insert(0, str(BASE))
    from youtube_content_automation import load_products
    products = load_products()
    total = len(products)
    got = ensure_product_image_for_all(products)
    print(f"\n{got}/{total} produkte me imazh")
