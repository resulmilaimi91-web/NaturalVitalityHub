import sys, json
d = json.load(sys.stdin)
if isinstance(d, list):
    d = d[0] if d else {}
print(f"{d.get('status','?')} - {d.get('conclusion','?')}")
