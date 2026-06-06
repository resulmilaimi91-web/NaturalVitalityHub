import json, shutil
src = "affiliate-bot/data/tracking.json"
bak = "affiliate-bot/data/tracking.json.bak"
shutil.copy2(src, bak)
print(f"Backup: {bak}")

with open(src, encoding="utf-8") as f:
    data = json.load(f)

links = data["links"]

# Keep only health/natural products [0-44], remove dating/survival/courses [45-104]
health = links[:45]
removed = links[45:]

print(f"Total: {len(links)}")
print(f"Health products kept: {len(health)}")
print(f"Removed (non-health): {len(removed)}")

for r in removed:
    print(f"  REMOVED: {r['produkti']}")

# Check for unique URLs in kept products
urls = set()
for p in health:
    urls.add(p["url_origjinale"])
print(f"\nUnique URLs in kept products: {len(urls)}")
for u in sorted(urls):
    count = sum(1 for p in health if p["url_origjinale"] == u)
    print(f"  {u[:70]}... -> {count} products")

data["links"] = health
with open(src, "w", encoding="utf-8") as f:
    json.dump(data, f, indent=2, ensure_ascii=False)
print(f"\nSaved {len(health)} products to {src}")
