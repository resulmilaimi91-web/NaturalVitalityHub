AFFILIATE_PROGRAMS = {
    "clickbank": {
        "name": "ClickBank",
        "url": "https://www.clickbank.com",
        "signup_url": "https://accounts.clickbank.com/",
        "payment_frequency": "weekly",
        "min_payout": 10,
        "commission_type": "high-ticket",
        "payment_methods": ["PayPal", "Direct Deposit"],
        "niches": ["health", "wealth", "dating", "business", "marketing", "tech"],
        "avg_commission_pct": 60,
        "notes": "Best for digital products, high commissions up to 75%"
    },
    "shareasale": {
        "name": "ShareASale",
        "url": "https://www.shareasale.com",
        "signup_url": "https://www.shareasale.com/join/",
        "payment_frequency": "monthly",
        "min_payout": 50,
        "commission_type": "varied",
        "payment_methods": ["PayPal", "Check", "Direct Deposit"],
        "niches": ["general", "fashion", "tech", "home", "garden"],
        "avg_commission_pct": 15,
        "notes": "Huge merchant network, reliable payments"
    },
    "amazon_associates": {
        "name": "Amazon Associates",
        "url": "https://affiliate-program.amazon.com",
        "signup_url": "https://affiliate-program.amazon.com/join",
        "payment_frequency": "monthly",
        "min_payout": 10,
        "commission_type": "percentage",
        "payment_methods": ["PayPal", "Gift Card", "Direct Deposit"],
        "niches": ["all"],
        "avg_commission_pct": 3,
        "notes": "Low commission but high conversion rate, cookie lasts 24h"
    },
    "impact": {
        "name": "Impact / Impact Radius",
        "url": "https://www.impact.com",
        "signup_url": "https://impact.com/partners/",
        "payment_frequency": "monthly",
        "min_payout": 50,
        "commission_type": "varied",
        "payment_methods": ["PayPal", "Wire Transfer"],
        "niches": ["tech", "gaming", "fashion", "finance"],
        "avg_commission_pct": 20,
        "notes": "Enterprise-grade, used by large brands"
    },
    "digistore24": {
        "name": "Digistore24",
        "url": "https://www.digistore24.com",
        "signup_url": "https://www.digistore24.com/signup",
        "payment_frequency": "weekly",
        "min_payout": 20,
        "commission_type": "percentage",
        "payment_methods": ["PayPal", "Wire"],
        "niches": ["marketing", "business", "health", "courses"],
        "avg_commission_pct": 50,
        "notes": "European-focused, high commissions, weekly payouts"
    },
    "partnerstack": {
        "name": "PartnerStack",
        "url": "https://www.partnerstack.com",
        "signup_url": "https://partnerstack.com/partners",
        "payment_frequency": "monthly",
        "min_payout": 25,
        "commission_type": "varied",
        "payment_methods": ["PayPal", "Stripe"],
        "niches": ["saas", "software", "tech"],
        "avg_commission_pct": 25,
        "notes": "Best for SaaS products, recurring commissions"
    },
    "cj_affiliate": {
        "name": "CJ Affiliate (Commission Junction)",
        "url": "https://www.cj.com",
        "signup_url": "https://www.cj.com/join",
        "payment_frequency": "monthly",
        "min_payout": 50,
        "commission_type": "varied",
        "payment_methods": ["PayPal", "Direct Deposit"],
        "niches": ["general", "retail", "tech", "travel"],
        "avg_commission_pct": 10,
        "notes": "One of the oldest networks, reliable"
    }
}

PRODUCT_TEMPLATES = {
    "health-supplements": {
        "name": "Health & Supplements",
        "keywords": ["health supplements", "natural remedies", "wellness", "fitness"],
        "example_products": [
            {"name": "PrimeBiome - Gut & Skin Health", "price_range": "$217.80", "commission": "65%", "program": "Digistore24", "aov": 217.80},
            {"name": "Nitric Boost Ultra - Male Health", "price_range": "$186.39", "commission": "70%", "program": "Digistore24", "aov": 186.39},
            {"name": "Pineal XT - Mental Clarity", "price_range": "$221.43", "commission": "55%", "program": "Digistore24", "aov": 221.43},
            {"name": "Provadent - Dental Health", "price_range": "$186.39", "commission": "70%", "program": "Digistore24", "aov": 186.39},
            {"name": "Insomniac - Sleep Therapy", "price_range": "$37.03", "commission": "60%", "program": "Digistore24", "aov": 37.03},
            {"name": "Encyclopedia of Power Foods", "price_range": "$194.35", "commission": "65%", "program": "Digistore24", "aov": 194.35},
            {"name": "100 Easy Keto Carbs", "price_range": "$40.47", "commission": "60%", "program": "Digistore24", "aov": 40.47}
        ]
    },
    "survival": {
        "name": "Survival & Preparedness",
        "keywords": ["survival", "prepping", "off-grid", "self-sufficiency"],
        "example_products": [
            {"name": "No Grid Survival Projects", "price_range": "$55.84", "commission": "75%", "program": "Digistore24", "aov": 55.84},
            {"name": "The Ultimate Energizer Guide", "price_range": "$55.16", "commission": "90%", "program": "Digistore24", "aov": 55.16},
            {"name": "Pocket Farm - Food Stockpile", "price_range": "$44.37", "commission": "75%", "program": "Digistore24", "aov": 44.37},
            {"name": "Home Doctor - Survival Remedies", "price_range": "$73.73", "commission": "75%", "program": "Digistore24", "aov": 73.73}
        ]
    },
    "dating-relationships": {
        "name": "Dating & Relationships",
        "keywords": ["dating", "relationships", "love", "attraction"],
        "example_products": [
            {"name": "Unlock the Scrambler", "price_range": "$130.53", "commission": "90%", "program": "Digistore24", "aov": 130.53}
        ]
    },
    "business-investment": {
        "name": "Business & Investment",
        "keywords": ["make money online", "investment", "crypto", "business"],
        "example_products": [
            {"name": "YouTube Success Course", "price_range": "$97.00", "commission": "60%", "program": "Digistore24", "aov": 97.00},
            {"name": "Crypto Trading Academy", "price_range": "$147.00", "commission": "50%", "program": "Digistore24", "aov": 147.00}
        ]
    },
    "tech-gadgets": {
        "name": "Tech Gadgets",
        "keywords": ["best gadgets 2026", "tech reviews", "smart home", "gadget deals"],
        "example_products": [
            {"name": "Noise-Canceling Headphones", "price_range": "$50-$350"},
            {"name": "Smart Watch", "price_range": "$30-$500"},
            {"name": "Portable Charger", "price_range": "$15-$80"},
            {"name": "Wireless Earbuds", "price_range": "$20-$200"},
            {"name": "Tablet Stand", "price_range": "$10-$60"}
        ]
    },
    "online-courses": {
        "name": "Online Courses / Education",
        "keywords": ["online courses", "learn online", "certification", "e-learning"],
        "example_products": [
            {"name": "Web Development Bootcamp", "price_range": "$10-$200"},
            {"name": "Digital Marketing Course", "price_range": "$15-$150"},
            {"name": "Photography Masterclass", "price_range": "$20-$180"}
        ]
    },
    "hosting": {
        "name": "Web Hosting",
        "keywords": ["best hosting", "web hosting", "vps", "wordpress hosting"],
        "example_products": [
            {"name": "Shared Hosting", "price_range": "$3-$15/mo"},
            {"name": "VPS Hosting", "price_range": "$10-$50/mo"},
            {"name": "WordPress Hosting", "price_range": "$5-$30/mo"}
        ]
    },
    "software-saas": {
        "name": "Software & SaaS",
        "keywords": ["best software", "saas tools", "productivity apps"],
        "example_products": [
            {"name": "Project Management Tool", "price_range": "$10-$50/mo"},
            {"name": "Email Marketing Platform", "price_range": "$15-$200/mo"},
            {"name": "CRM Software", "price_range": "$12-$100/mo"}
        ]
    },
    "health-fitness": {
        "name": "Health & Fitness",
        "keywords": ["fitness equipment", "supplements", "workout programs"],
        "example_products": [
            {"name": "Resistance Bands Set", "price_range": "$15-$40"},
            {"name": "Yoga Mat", "price_range": "$10-$60"},
            {"name": "Fitness Tracker", "price_range": "$30-$150"}
        ]
    }
}

NICHES = {
    "health-supplements": "Shendet dhe suplemente",
    "survival": "Mbijetese dhe pergatitje",
    "dating-relationships": "Takime dhe marredhenie",
    "business-investment": "Biznes dhe investime",
    "tech-gadgets": "Teknologji dhe pajisje",
    "online-courses": "Kurse online dhe edukim",
    "hosting": "Web hosting dhe domain",
    "software-saas": "Software dhe mjete SaaS",
    "health-fitness": "Shendet dhe fitness",
    "finance": "Financa dhe investime",
    "travel": "Udhetime dhe akomodime",
    "fashion": "Mode dhe aksesore",
    "beauty": "Bukuri dhe kujdes personal",
    "gaming": "Loje dhe gaming"
}
