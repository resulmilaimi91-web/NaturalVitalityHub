import csv, json, re, os
from datetime import datetime

AFF = "resulpaypald725"
CSV_FILE = os.path.join(os.path.dirname(os.path.dirname(__file__)), "digistore24-products.csv")

def parse_csv(path):
    products = []
    seen_ids = set()
    with open(path, "r", encoding="utf-8") as f:
        reader = csv.reader(f, delimiter=";")
        next(reader)
        for row in reader:
            if len(row) < 10:
                continue
            pid = row[2].strip()
            name = row[3].strip()
            vendor = row[1].strip()
            if pid in seen_ids or vendor == "MachtundNussbaumGbR" or "Funnel Master" in name:
                continue
            seen_ids.add(pid)
            # Skip upsells, keep only main offers
            if re.match(r"^(UP\d|DOWN\d|DS\d)", name):
                continue
            if "Extra" in name and ("Bottle" in name or "Flasche" in name):
                continue
            clean = re.sub(r"^(Main\d*|DOWN\d*|UP\d*\.?\d*|DS\d*)\s*-\s*", "", name).strip()
            clean = re.sub(r"\s*\(\d+\s*(Bottle|Bottles|Flasche|Flaschen)\)", "", clean).strip()
            cl = clean.lower()
            niche = "general-health"
            if "sleep" in cl or "insomniac" in cl: niche = "sleep-health"
            elif "gut" in cl or "primebiome" in cl: niche = "gut-health"
            elif "keto" in cl or "slim" in cl or "metabo" in cl: niche = "weight-loss"
            elif "curcuma" in cl: niche = "anti-aging"
            elif "cleanse" in cl or "detox" in cl: niche = "detox"
            elif "amino" in cl or "primal grow" in cl: niche = "men-health"
            elif "ultraburst" in cl or "x8" in cl or "energy" in cl: niche = "energy"
            products.append({"id": pid, "produkti": clean or name, "niche": niche})
    return products

fallback = [
    {"id": "478372", "produkti": "Insomniac - Sleep Therapy", "niche": "sleep-health"},
    {"id": "472943", "produkti": "Advanced Amino Formula", "niche": "fitness"},
    {"id": "413488", "produkti": "Primal Grow Pro - Men Health", "niche": "men-health"},
    {"id": "413868", "produkti": "Primal Ultraburst - Energy Booster", "niche": "energy"},
    {"id": "413871", "produkti": "Primal X8 Energy Formula", "niche": "energy"},
]

if os.path.exists(CSV_FILE):
    products = parse_csv(CSV_FILE)
    print(f"Parsed {len(products)} products from CSV")
else:
    products = fallback
    print("No CSV found, using fallback list")

links = []
for p in products:
    links.append({
        "id": p["id"],
        "url_origjinale": f"https://www.digistore24-app.com/redir/{p['id']}/{AFF}",
        "produkti": p["produkti"],
        "programi": "digistore24",
        "data_krijimi": datetime.now().isoformat(),
        "clicks": 0,
        "conversions": 0,
        "niche": p["niche"],
    })

tracking_path = os.path.join(os.path.dirname(os.path.dirname(__file__)), "affiliate-bot/data/tracking.json")
with open(tracking_path, "w", encoding="utf-8") as f:
    json.dump({"links": links, "total": len(links), "updated": datetime.now().isoformat()}, f, indent=2, ensure_ascii=False)

print(f"tracking.json updated: {len(links)} products")
