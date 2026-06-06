import json, sys
sys.stdout.reconfigure(encoding="utf-8")
with open("affiliate-bot/data/tracking.json", encoding="utf-8") as f:
    data = json.load(f)
with open("published_products.json") as f:
    pub = json.load(f)
links = data["links"]
names = [p["produkti"] for p in links]
print("=== PUBLISHED IN NEW LIST ===")
for name in list(pub.keys()):
    if name in names:
        print(f"  [OK] {name} -> {pub[name]['url']}")
    else:
        print(f"  [X] {name} -> REMOVED")
        del pub[name]
with open("published_products.json", "w", encoding="utf-8") as f:
    json.dump(pub, f, indent=2, ensure_ascii=False)
print(f"\nPublished tracker: {len(pub)} products\n")
print("=== REMAINING PRODUCTS ===")
for i, p in enumerate(links):
    url = p["url_origjinale"]
    sid = url.split("/redir/")[1].split("/")[0] if "/redir/" in url else "?"
    print(f"  [{i:02d}] {p['produkti'][:45]:45s} ID={sid}")
