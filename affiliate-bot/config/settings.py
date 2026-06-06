import os, json

CONFIG_FILE = os.path.join(os.path.dirname(__file__), "..", "data", "config.json")

DEFAULT_CONFIG = {
    "paypal_email": "your-paypal@email.com",
    "niche": "tech-gadgets",
    "content_schedule": {
        "articles_per_week": 3,
        "social_posts_per_product": 5
    },
    "payment_method": "paypal",       # paypal, paysera, or both
    "paysera_email": "",
    "bitly_api_token": "",
    "blog_platform": "blogger",
    "blogger_api_key": "",
    "blog_id": "",
    "youtube_api_key": "",
    "twitter_api_key": "",
    "output_dir": "output",
    "language": "sq",
    "min_commission_rate": 10
}

class Settings:
    def __init__(self):
        self.config = DEFAULT_CONFIG.copy()
        self._load()

    def _load(self):
        if os.path.exists(CONFIG_FILE):
            with open(CONFIG_FILE, "r", encoding="utf-8") as f:
                saved = json.load(f)
                self.config.update(saved)

    def save(self):
        os.makedirs(os.path.dirname(CONFIG_FILE), exist_ok=True)
        with open(CONFIG_FILE, "w", encoding="utf-8") as f:
            json.dump(self.config, f, indent=2, ensure_ascii=False)

    def get(self, key, default=None):
        return self.config.get(key, default)

    def set(self, key, value):
        self.config[key] = value
        self.save()

    def __getitem__(self, key):
        return self.config[key]

    def __setitem__(self, key, value):
        self.set(key, value)

settings = Settings()
