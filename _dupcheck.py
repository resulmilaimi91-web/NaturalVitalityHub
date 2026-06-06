import json
with open("affiliate-bot/data/tracking.json", encoding="utf-8") as f:
    data = json.load(f)
links = data.get("links", [])
print(f"Total products: {len(links)}")

names = {}
for i, p in enumerate(links):
    n = p["produkti"].strip().lower()
    names.setdefault(n, []).append(i)

print("\n=== DUPLICATE NAMES ===")
for name, indices in names.items():
    if len(indices) > 1:
        prod = links[indices[0]]["produkti"]
        print(f"  {prod}: {len(indices)} kopje")
        for idx in indices:
            print(f"    [{idx}] {links[idx]['url_origjinale'][:70]}")

urls = {}
for i, p in enumerate(links):
    u = p["url_origjinale"].strip()
    urls.setdefault(u, []).append(i)

print("\n=== DUPLICATE URLs ===")
for url, indices in urls.items():
    if len(indices) > 1:
        print(f"  URL: {url[:70]}")
        for idx in indices:
            print(f"    [{idx}] {links[idx]['produkti']}")
