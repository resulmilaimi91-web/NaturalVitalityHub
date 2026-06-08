import requests, json, os, sys
from datetime import datetime

API_URL = "https://www.digistore24.com/api/call/listProducts"
AFFILIATE_ID = "resulpaypald725"
BASE_DIR = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
TRACKING_PATH = os.path.join(BASE_DIR, "affiliate-bot", "data", "tracking.json")

NICHE_MAP = {
    "sleep": "sleep-health", "insomniac": "sleep-health",
    "gut": "gut-health", "primebiome": "gut-health",
    "keto": "weight-loss", "slim": "weight-loss", "metabo": "weight-loss",
    "curcuma": "anti-aging", "cleanse": "detox", "detox": "detox",
    "amino": "fitness", "primal grow": "men-health",
    "ultraburst": "energy", "x8": "energy", "energy": "energy",
    "vision": "eye-health", "eye": "eye-health",
    "lion care": "general-health", "wellness": "general-health",
    "provadent": "dental-health", "dental": "dental-health",
    "nitric": "men-health", "pineal": "brain-health",
    "probiotic": "gut-health", "collagen": "beauty",
    "weight loss": "weight-loss", "sugar": "blood-sugar",
    "gluco": "blood-sugar", "liver": "detox",
    "joint": "joint-health", "magnesium": "minerals",
    "ashwagandha": "stress-relief", "stress": "stress-relief",
}

def get_niche(name):
    cl = name.lower()
    for kw, niche in NICHE_MAP.items():
        if kw in cl:
            return niche
    return "general-health"

def fetch_products(api_key):
    r = requests.get(f"{API_URL}?apiKey={api_key}", timeout=30)
    r.raise_for_status()
    data = r.json()
    if isinstance(data, dict) and "error" in data:
        print(f"[X] API error: {data['error']}")
        sys.exit(1)
    if isinstance(data, dict) and "data" in data:
        data = data["data"]
    return data if isinstance(data, list) else []

def build_tracking(products):
    links = []
    seen_ids = set()
    for p in products:
        pid = str(p.get("id", ""))
        name = p.get("name", "").strip()
        if not pid or not name:
            continue
        if pid in seen_ids:
            continue
        seen_ids.add(pid)
        links.append({
            "id": pid,
            "url_origjinale": f"https://www.digistore24-app.com/redir/{pid}/{AFFILIATE_ID}",
            "produkti": name,
            "programi": "digistore24",
            "data_krijimi": datetime.now().isoformat(),
            "clicks": 0,
            "conversions": 0,
            "niche": get_niche(name),
        })
    return links

def merge_with_existing(new_links):
    existing = {"links": [], "total": 0}
    if os.path.exists(TRACKING_PATH):
        with open(TRACKING_PATH, "r") as f:
            existing = json.load(f)
    existing_ids = {l["id"] for l in existing.get("links", [])}
    merged = list(existing.get("links", []))
    for nl in new_links:
        if nl["id"] not in existing_ids:
            merged.append(nl)
            existing_ids.add(nl["id"])
    return merged

if __name__ == "__main__":
    api_key = os.environ.get("DIGISTORE24_API_KEY", "")
    if not api_key:
        print("[X] DIGISTORE24_API_KEY not set")
        sys.exit(1)
    print("[*] Fetching products from Digistore24 API...")
    products = fetch_products(api_key)
    print(f"[*] Got {len(products)} products from API")
    new_links = build_tracking(products)
    merged = merge_with_existing(new_links)
    output = {"links": merged, "total": len(merged), "updated": datetime.now().isoformat()}
    os.makedirs(os.path.dirname(TRACKING_PATH), exist_ok=True)
    with open(TRACKING_PATH, "w", encoding="utf-8") as f:
        json.dump(output, f, indent=2, ensure_ascii=False)
    print(f"[OK] tracking.json updated: {len(merged)} products total ({len(new_links)} new from API)")
