import json
d = json.load(open("published_products.json"))
print(f"Published: {len(d)}")
for k, v in d.items():
    print(f"  {k} -> {v.get('url','?')}")
