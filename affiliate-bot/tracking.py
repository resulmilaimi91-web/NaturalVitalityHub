import json, os, random, string
from datetime import datetime, timedelta
from urllib.parse import urlencode

TRACKING_FILE = os.path.join(os.path.dirname(__file__), "data", "tracking.json")
CACHE_FILE = os.path.join(os.path.dirname(__file__), "data", "cache.json")

def _gjenero_kod(gjatesi=8):
    return ''.join(random.choices(string.ascii_lowercase + string.digits, k=gjatesi))

class LinkTracker:
    def __init__(self):
        self.data = self._ngarko()

    def _ngarko(self):
        if os.path.exists(TRACKING_FILE):
            with open(TRACKING_FILE, "r", encoding="utf-8") as f:
                return json.load(f)
        return {"links": [], "clicks": [], "conversions": []}

    def _ruaj(self):
        os.makedirs(os.path.dirname(TRACKING_FILE), exist_ok=True)
        with open(TRACKING_FILE, "w", encoding="utf-8") as f:
            json.dump(self.data, f, indent=2, ensure_ascii=False)

    def shto_link(self, url_origjinale, produkti, programi):
        kodi = _gjenero_kod()
        linku = {
            "id": kodi,
            "url_origjinale": url_origjinale,
            "produkti": produkti,
            "programi": programi,
            "data_krijimi": datetime.now().isoformat(),
            "clicks": 0,
            "conversions": 0
        }
        self.data["links"].append(linku)
        self._ruaj()
        return f"https://go.yoursite.com/{kodi}", linku

    def regjistro_click(self, link_id):
        for link in self.data["links"]:
            if link["id"] == link_id:
                link["clicks"] += 1
                click = {
                    "link_id": link_id,
                    "timestamp": datetime.now().isoformat(),
                    "user_agent": "",
                    "referrer": ""
                }
                self.data["clicks"].append(click)
                self._ruaj()
                return True
        return False

    def regjistro_konvertim(self, link_id, shuma=0):
        for link in self.data["links"]:
            if link["id"] == link_id:
                link["conversions"] += 1
                konvertimi = {
                    "link_id": link_id,
                    "shuma": shuma,
                    "timestamp": datetime.now().isoformat()
                }
                self.data["conversions"].append(konvertimi)
                self._ruaj()
                return True
        return False

    def statistikat(self):
        total_clicks = sum(l["clicks"] for l in self.data["links"])
        total_conversions = sum(l["conversions"] for l in self.data["links"])
        conversion_rate = round((total_conversions / total_clicks * 100), 2) if total_clicks > 0 else 0

        return {
            "total_links": len(self.data["links"]),
            "total_clicks": total_clicks,
            "total_conversions": total_conversions,
            "conversion_rate_pct": conversion_rate,
            "links": self.data["links"]
        }

    def raport_mujor(self, muaji=None, viti=None):
        tani = datetime.now()
        muaji = muaji or tani.month
        viti = viti or tani.year

        klikime_muaj = [c for c in self.data["clicks"]
                        if datetime.fromisoformat(c["timestamp"]).month == muaji
                        and datetime.fromisoformat(c["timestamp"]).year == viti]
        konvertime_muaj = [c for c in self.data["conversions"]
                           if datetime.fromisoformat(c["timestamp"]).month == muaji
                           and datetime.fromisoformat(c["timestamp"]).year == viti]
        total_shuma = sum(c["shuma"] for c in konvertime_muaj)

        return {
            "muaji": muaji,
            "viti": viti,
            "klikime": len(klikime_muaj),
            "konvertime": len(konvertime_muaj),
            "fitime_total": round(total_shuma, 2),
            "konvertim_rate": round((len(konvertime_muaj) / len(klikime_muaj) * 100), 2) if klikime_muaj else 0
        }

tracker = LinkTracker()
