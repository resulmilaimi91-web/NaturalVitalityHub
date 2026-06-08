import json
from datetime import datetime

aff = "resulpaypald725"

products = [
    {"id": "478372", "produkti": "Insomniac - Sleep Therapy", "niche": "sleep-health"},
    {"id": "472943", "produkti": "Advanced Amino Formula", "niche": "fitness"},
    {"id": "413488", "produkti": "Primal Grow Pro - Men Health", "niche": "men-health"},
    {"id": "413868", "produkti": "Primal Ultraburst - Energy Booster", "niche": "energy"},
    {"id": "413871", "produkti": "Primal X8 Energy Formula", "niche": "energy"},
]

links = []
for p in products:
    links.append({
        "id": p["id"],
        "url_origjinale": f"https://www.digistore24-app.com/redir/{p['id']}/{aff}",
        "produkti": p["produkti"],
        "programi": "digistore24",
        "data_krijimi": datetime.now().isoformat(),
        "clicks": 0,
        "conversions": 0,
        "niche": p["niche"],
    })

with open("affiliate-bot/data/tracking.json", "w", encoding="utf-8") as f:
    json.dump({"links": links, "total": len(links), "updated": datetime.now().isoformat()}, f, indent=2, ensure_ascii=False)

print(f"tracking.json: {len(links)} products")
for l in links:
    print(f"  {l['id']}: {l['produkti']}")
