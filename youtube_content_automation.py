#!/usr/bin/env python3
"""
NaturalVitalityHub - YouTube Content Automation System 2026
Full pipeline: Product -> Script -> Video -> Upload -> Monetize
"""

import json, os, sys, random, subprocess, time
from datetime import datetime, timedelta
from pathlib import Path

try:
    import schedule
    SCHEDULE_AVAILABLE = True
except ImportError:
    SCHEDULE_AVAILABLE = False

BASE_DIR = Path(__file__).parent
TRACKING_FILE = BASE_DIR / "affiliate-bot" / "data" / "tracking.json"
PUBLISHED_FILE = BASE_DIR / "published_products.json"
OUTPUT_DIR = BASE_DIR / "output"
VIDEO_DIR = OUTPUT_DIR / "videos"
SCRIPTS_DIR = OUTPUT_DIR / "scripts"
os.makedirs(VIDEO_DIR, exist_ok=True)
os.makedirs(SCRIPTS_DIR, exist_ok=True)

def load_published():
    if not PUBLISHED_FILE.exists():
        return {}
    with open(PUBLISHED_FILE, "r") as f:
        return json.load(f)

def save_published(data):
    with open(PUBLISHED_FILE, "w") as f:
        json.dump(data, f, indent=2)

def get_next_unpublished():
    products = load_products()
    published = load_published()
    for p in products:
        name = p["produkti"]
        # Match if name appears in any published key (handles renamed/reformatted names)
        is_published = any(name.lower() in k.lower() or k.lower() in name.lower() for k in published)
        if not is_published:
            return p
    return None

PRODUCT_NICHES = {
    "PrimeBiome": "gut-health",
    "Nitric Boost Ultra": "men-health",
    "Provadent": "dental-health",
    "Insomniac": "sleep-health",
    "Advanced Vision Formula": "eye-health",
    "Pineal XT": "brain-health",
    "ProDentim": "dental-health",
    "Sugar Defender": "blood-sugar",
    "Aizen Power": "men-health",
    "Primal Grow Pro": "men-health",
    "Gluco Freedom": "blood-sugar",
    "Liver Detox": "detox",
    "Collagen Peptides": "beauty",
    "Ashwagandha": "stress-relief",
    "Sleep Gummies": "sleep-health",
    "Weight Loss Drops": "weight-loss",
    "Keto Boost": "weight-loss",
    "Energy Boosting": "energy",
    "Magnesium Glycinate": "minerals",
    "Joint Ease": "joint-health",
    "Probiotic 40 Billion": "gut-health",
    "Primal UltraBurst": "energy",
    "Primal X8": "energy",
    "Advanced Amino": "general-health",
    "Advanced Mitochondrial": "energy",
    "Advanced Collagen": "beauty",
    "Integrative Digestive": "gut-health",
    "Advanced Prostate": "men-health",
    "Prime Perform": "men-health",
    "EndoPeak": "men-health",
    "Audifort": "brain-health",
    "CelluCare": "blood-sugar",
    "Nitric Boost": "men-health",
    "Keto After 50": "weight-loss",
    "Patriot Slim Shot": "weight-loss",
    "Advanced Memory Formula": "brain-health",
    "Encyclopedia of Power Foods": "general-health",
    "GlucoTonic": "blood-sugar",
    "Sight Fresh": "eye-health",
    "Pep-Tonic": "beauty",
    "ZenCortex": "brain-health",
    "The Smoothie Diet": "weight-loss",
    "Advanced Muscle Plus": "men-health",
    "CogniCare Pro": "brain-health",
    "iGenics": "eye-health",
    "Blood Sugar Blaster": "blood-sugar",
    "Pineal Guardian": "brain-health",
    "Primal Grow Pro 24": "men-health",
    "Advanced Mitochondrial Formula": "energy",
    "Nitric Boost": "men-health",
    "Advanced Prostate Formula": "men-health",
    "Advanced Amino Muscle Mass": "fitness",
}

def load_products():
    if not TRACKING_FILE.exists():
        print(f"[X] Tracking file not found: {TRACKING_FILE}")
        return []
    with open(TRACKING_FILE, "r", encoding="utf-8") as f:
        data = json.load(f)
    return data.get("links", [])

def get_niche(product_name):
    for key, niche in PRODUCT_NICHES.items():
        if key.lower() in product_name.lower():
            return niche
    return "general-health"

def generate_description(product_name, niche, affiliate_url, script):
    sections = script.split("\n\n")
    hook = ""
    benefits = []
    for s in sections:
        if s.startswith("[HOOK]"):
            hook = s.replace("[HOOK]", "").strip()
        elif s.startswith("[BENEFITS]"):
            ben_text = s.replace("[BENEFITS]", "").strip()
            benefits = [b.strip() for b in ben_text.split(",") if b.strip()]
    
    niche_icons = {
        "dental-health": "🦷", "blood-sugar": "🩸", "men-health": "💪",
        "sleep-health": "😴", "gut-health": "🦠", "weight-loss": "⚖️",
        "brain-health": "🧠", "eye-health": "👁️", "stress-relief": "🧘",
        "detox": "🌿", "joint-health": "🦴", "energy": "⚡",
        "beauty": "✨", "general-health": "💚", "minerals": "💊",
    }
    icon = niche_icons.get(niche, "💚")
    
    product_tag = product_name.replace(' ', '').replace('-', '').replace('.', '')
    
    hashtags = [
        f"#NaturalVitalityHub",
        f"#{product_tag}",
        f"#{product_tag}Review",
        f"#{product_tag}Results",
        f"#SupplementReview2026",
        f"#NaturalHealth",
        f"#WellnessJourney",
        f"#{niche.replace('-', '')}",
        f"#NaturalSupplements",
        f"#HealthSupplements",
        f"#SupplementsThatWork",
        f"#HonestReview",
        f"#DoesItWork",
        f"#2026Health",
        f"#WellnessTips",
        f"#Biohacking",
        f"#NaturalRemedies",
        f"#HealthOptimization",
    ]
    
    desc = f"""{hook}

{icon} WHY {product_name.upper()}?
"""
    for b in benefits[:5]:
        desc += f"✅ {b}\n"
    
    desc += f"""

🔥 GET {product_name.upper()} AT BEST PRICE:
👉 {affiliate_url}

🎯 LIMITED TIME OFFER - Click the link above to claim your discount!

⚠️ DISCLAIMER: Results may vary. This video is for informational purposes only. 
Consult your healthcare provider before starting any supplement.

{' '.join(hashtags)}

🔔 SUBSCRIBE for more honest reviews!
▶️ https://www.youtube.com/@NaturalVitalityHub-y4d

---
💡 AFFILIATE DISCLOSURE: This description contains affiliate links. 
If you purchase through these links, I may earn a commission at no extra cost to you.
This helps support the channel and allows me to create more content like this.

📌 TIMESTAMPS:
0:00 - Introduction
1:30 - The Problem
3:00 - The Solution
4:30 - Key Ingredients
6:00 - Benefits & Results
7:30 - Final Verdict
"""
    return desc


def generate_youtube_package(product):
    name = product["produkti"]
    url = product["url_origjinale"]
    niche = get_niche(name)
    content = SCRIPTS.get(niche, SCRIPTS["general-health"])
    
    title = content["title_template"].replace("{product}", name)
    script = "\n\n".join([s.replace("{product}", name) for s in content["sections"]])
    description = generate_description(name, niche, url, script)
    tags_raw = content.get("tags", "")
    
    try:
        from gemini_script_generator import generate_script, generate_viral_elements
        print(f"  [i] Generating AI script for {name}...")
        ai_sections = generate_script(name, niche, url)
        if ai_sections and len(ai_sections) >= 6:
            script = "\n\n".join(ai_sections)
            print(f"  [OK] AI script generated ({sum(len(s.split()) for s in ai_sections)} words)")
            
            viral = generate_viral_elements(name, niche, url)
            if viral:
                if viral.get("title"):
                    title = viral["title"]
                if viral.get("hashtags"):
                    tags_raw = " ".join(viral["hashtags"])
                    
        from gemini_script_generator import generate_rich_description
        ai_desc = generate_rich_description(name, niche, url, script)
        if ai_desc:
            description = ai_desc
    except Exception as e:
        print(f"  [i] AI generation skipped ({e}), using templates")
    
    tags = [
        name, f"{name} Review", f"{name} 2026", f"{name} Results",
        "Health Supplements", "Natural Health", "Supplement Review", "Wellness",
        "Natural Supplements", "Health 2026", "Best Supplements",
        "Supplement Results", "Honest Review", "Does It Work",
        niche.replace("-", " ").title(), niche.replace("-", ""),
        tags_raw
    ]
    
    thumbnail_prompt = content["thumbnail_prompt"].replace("{product}", name)
    
    return {
        "title": title,
        "script": script,
        "description": description,
        "tags": [t for t in tags if t],
        "thumbnail_prompt": thumbnail_prompt,
        "product": name,
        "affiliate_url": url,
        "niche": niche,
        "generated_at": datetime.now().isoformat()
    }

SCRIPTS = {
    "dental-health": {
        "title_template": "{product} Review 2026 - Does It Really Fix Your Teeth and Gums? Honest Results",
        "sections": [
            "[HOOK] Imagine waking up every morning with perfectly white teeth, fresh breath, and zero gum pain... without spending a fortune at the dentist. Sounds impossible? Today I am reviewing {product} - one of the most talked about oral health supplements right now.",
            "[PROBLEM] Most people do not realize this: your mouth has a delicate ecosystem of good and bad bacteria. Most toothpastes and mouthwashes kill everything - including the good bacteria your gums need. This leads to yellow teeth, bleeding gums, chronic bad breath, and expensive dental bills.",
            "[SOLUTION] {product} is a unique probiotic supplement that delivers millions of beneficial bacteria directly to your mouth. Unlike anything else on the market, it works with your body natural biology.",
            "[INGREDIENTS] The formula includes clinically studied ingredients: Probiotic strains that support oral microbiome, Natural minerals for enamel strength, Anti-inflammatory compounds for gum health. All 100% natural with no harsh chemicals.",
            "[BENEFITS] Users consistently report: Whiter teeth within 2-3 weeks, Fresher breath throughout the day, Stronger gums with less bleeding, Reduced tooth sensitivity, Overall better oral health confidence.",
            "[RESULTS] Thousands of verified users share their success stories. Most see visible improvements within the first month. Many report saving hundreds of dollars on dental treatments.",
            "[CONCLUSION] If you want to finally solve your dental health issues naturally, {product} is worth trying. The official link with the best available discount is in the description below. Click it, check the current offers, and see what works best for you.",
        ],
        "thumbnail_prompt": "Before and after perfect white teeth smile, {product} supplement bottle center, blue white clean background, big bold text 'Works?', red arrow pointing to teeth improvement, professional YouTube thumbnail, high contrast, cinematic lighting, 4K",
        "tags": "Oral Health, Teeth Whitening, Gum Health"
    },
    "blood-sugar": {
        "title_template": "{product} Review 2026 - Lower Blood Sugar Naturally? Real Results",
        "sections": [
            "[HOOK] Are you tired of constant fatigue, sugar cravings, and worrying about your blood sugar levels? What if a natural solution could help you take control? Today we are reviewing {product} - the supplement thousands are talking about.",
            "[PROBLEM] High blood sugar affects millions worldwide. It drains your energy, causes weight gain, damages your organs over time, and increases your risk of serious health conditions. Most people do not even know they have a problem until it is too late.",
            "[SOLUTION] {product} is a powerful natural formula designed to support healthy blood sugar levels. It combines clinically researched ingredients that work together to help your body process sugar more effectively.",
            "[INGREDIENTS] Key ingredients: Chromium for glucose metabolism, Cinnamon bark extract, Bitter melon, Gymnema Sylvestre, Banaba leaf, and other natural compounds. Each ingredient is carefully selected based on scientific research.",
            "[BENEFITS] Users report: More stable energy throughout the day, Reduced sugar cravings, Better blood sugar readings, Improved mental clarity, Healthy weight management support.",
            "[RESULTS] In clinical studies, key ingredients have shown significant improvements in blood sugar markers within 4-8 weeks. Real users report feeling more energetic and in control of their health.",
            "[CONCLUSION] {product} could be the natural solution you have been looking for. The official link with the current discounts is in the description. Check it out and see if it is right for you.",
        ],
        "thumbnail_prompt": "Blood sugar meter showing high to normal range, {product} bottle prominent, red and green contrast, bold text 'Blood Sugar Solution?', professional lighting, medical style, clean background, YouTube thumbnail 4K",
        "tags": "Blood Sugar, Diabetes Support, Natural Health"
    },
    "men-health": {
        "title_template": "{product} Review 2026 - Better Performance Naturally? Honest Review",
        "sections": [
            "[HOOK] As men age, energy levels drop, performance declines, and confidence takes a hit. But what if you could reverse that naturally? Today I am reviewing {product} - one of the fastest growing male health supplements.",
            "[PROBLEM] Low testosterone, poor circulation, stress, and poor sleep affect every man over 30. The result is low energy, weak performance, mood swings, and a general feeling of getting older faster than you should.",
            "[SOLUTION] {product} is formulated with natural ingredients that target the root causes of male health decline. It supports healthy blood flow, hormone balance, and energy production.",
            "[INGREDIENTS] Key ingredients include: L-Citrulline for blood flow, Tribulus Terrestris for hormone support, Maca root for stamina, Zinc for testosterone production, and other carefully selected natural compounds.",
            "[BENEFITS] Men consistently report: Stronger and longer-lasting performance, Increased energy and stamina, Better mood and confidence, Improved recovery after workouts, Overall vitality boost.",
            "[RESULTS] Clinical studies on key ingredients show significant improvements in male health markers within 4-6 weeks. Thousands of men have shared their positive experiences.",
            "[CONCLUSION] If you want to feel like your younger self again, {product} is absolutely worth trying. Grab it with the current discount through the official link in the description.",
        ],
        "thumbnail_prompt": "Athletic male silhouette with energy glow, {product} bottle foreground, dark blue background, luxury style, bold text 'Performance Boost?', professional YouTube thumbnail, high quality",
        "tags": "Men Health, Testosterone, Vitality, Performance"
    },
    "sleep-health": {
        "title_template": "{product} Review 2026 - Fall Asleep Faster & Wake Up Refreshed? Real Review",
        "sections": [
            "[HOOK] Lying awake at 3 AM staring at the ceiling again? You are not alone. Millions struggle with sleep every night. Today I am reviewing {product} - the natural sleep solution that is changing lives.",
            "[PROBLEM] Poor sleep affects every aspect of your life. Low energy, brain fog, weight gain, weakened immune system, and increased risk of serious health issues. Sleeping pills have dangerous side effects. There has to be a better way.",
            "[SOLUTION] {product} uses a unique blend of natural sleep-promoting ingredients that work with your body natural sleep cycle. No harsh chemicals, no morning grogginess.",
            "[INGREDIENTS] Key ingredients: Melatonin for sleep cycle regulation, Magnesium for muscle relaxation, Chamomile and Lavender for calming effects, L-Theanine for stress reduction, GABA for deep sleep support.",
            "[BENEFITS] Users report: Falling asleep 2x faster, Deeper more restorative sleep, Waking up refreshed without grogginess, Better focus and mood during the day, Reduced stress and anxiety at bedtime.",
            "[RESULTS] Studies show that quality sleep supplementation can significantly improve sleep quality within 1-2 weeks. Real users report life-changing improvements in their daily energy and mood.",
            "[CONCLUSION] If better sleep would transform your life, {product} is the most natural and effective option available. The official link with discounts is in the description below.",
        ],
        "thumbnail_prompt": "Peaceful bedroom scene, person sleeping deeply, {product} bottle with glowing effect, moon and stars background, dark blue tones, text 'Deep Sleep?', professional YouTube thumbnail",
        "tags": "Sleep Health, Insomnia Relief, Natural Sleep Aid"
    },
    "gut-health": {
        "title_template": "{product} Review 2026 - Transform Your Digestion Naturally? Honest Results",
        "sections": [
            "[HOOK] Bloating after every meal? Low energy? Skin issues? It could all start in your gut. Today I am reviewing {product} - the gut health supplement that is helping thousands feel better from the inside out.",
            "[PROBLEM] Your gut microbiome affects everything - digestion, immunity, mood, weight, skin, and even brain function. Processed foods, stress, antibiotics, and age destroy your gut bacteria balance. This leads to chronic health problems.",
            "[SOLUTION] {product} delivers a powerful dose of targeted probiotics and prebiotics that restore your gut microbiome. It helps repopulate your intestines with beneficial bacteria.",
            "[INGREDIENTS] 10+ clinically studied probiotic strains, Prebiotic fiber for feeding good bacteria, Digestive enzymes for better absorption, Soothing herbs for gut lining health. All natural and non-GMO.",
            "[BENEFITS] Users consistently report: Better digestion and less bloating, More energy throughout the day, Clearer skin, Stronger immune system, Improved mood and mental clarity, Healthy weight management.",
            "[RESULTS] Thousands of verified reviews show significant improvements in digestive health within 2-4 weeks. Many call it life-changing.",
            "[CONCLUSION] If you want to transform your health from the inside out, {product} is the answer. Check the official link in the description for the current best price.",
        ],
        "thumbnail_prompt": "Happy person with flat stomach, {product} bottle with probiotics visual, green and blue natural background, gut microbiome illustration, bold text 'Gut Health?', professional YouTube thumbnail",
        "tags": "Gut Health, Probiotics, Digestion, Microbiome"
    },
    "weight-loss": {
        "title_template": "{product} Review 2026 - Lose Weight Fast Naturally? Does It Really Work?",
        "sections": [
            "[HOOK] Tired of trying every diet and still not seeing results? What if there was a natural way to boost your metabolism and burn fat without starvation? Today we review {product}.",
            "[PROBLEM] Weight loss is hard. Diets fail because they fight your biology. Your body thinks starvation is coming and holds onto fat. The result is yo-yo dieting, frustration, and giving up.",
            "[SOLUTION] {product} works with your metabolism, not against it. It helps your body switch into fat-burning mode naturally, suppress appetite, and maintain lean muscle.",
            "[INGREDIENTS] Key ingredients: Green tea extract for metabolism, Garcinia Cambogia for appetite control, B vitamins for energy, L-Carnitine for fat transport, natural thermogenic compounds.",
            "[BENEFITS] Users report: Faster fat burning especially belly fat, Reduced appetite and cravings, More energy for workouts, Better mood during dieting, Sustainable weight loss without rebound.",
            "[RESULTS] Clinical studies show ingredients can boost metabolism by up to 14-16%. Combined with diet and exercise, users report 2-5 lbs per week loss.",
            "[CONCLUSION] If you are serious about losing weight and keeping it off, {product} can be your secret weapon. Official link with offers in the description.",
        ],
        "thumbnail_prompt": "Before and after weight loss transformation, {product} bottle in front, measuring tape around waist, dramatic transformation, bold text 'Lose Weight?', professional YouTube thumbnail, motivational style",
        "tags": "Weight Loss, Fat Burn, Metabolism, Diet Support"
    },
    "general-health": {
        "title_template": "{product} Review 2026 - The Complete Honest Review. Does It Work?",
        "sections": [
            "[HOOK] Today I am doing a complete honest review of {product} - one of the most popular health supplements right now. Does it really work? Let us find out.",
            "[PROBLEM] Most people are looking for natural ways to improve their health without relying on pharmaceuticals. The supplement market is full of scams, so finding something that actually works is hard.",
            "[SOLUTION] {product} is formulated with natural ingredients designed to support your body optimal function. It is manufactured in FDA-approved facilities following strict quality standards.",
            "[INGREDIENTS] The formula contains carefully selected natural ingredients backed by scientific research. Each ingredient plays a specific role in supporting your health goals.",
            "[BENEFITS] Users report noticeable improvements in their overall wellbeing, more energy, better focus, and improved quality of life.",
            "[RESULTS] With consistent use, most users begin to see results within 2-4 weeks. The longer you use it, the better the results.",
            "[CONCLUSION] {product} is worth considering if you are looking for a natural health solution. Check the official link in the description for the current best price and special offers.",
        ],
        "thumbnail_prompt": "{product} supplement bottle on clean white background, medical style, blue and green professional tones, bold product name, 'Honest Review' text, professional YouTube thumbnail, high quality 4K",
        "tags": "Health Supplements, Natural Health, Wellness"
    },
    "brain-health": {
        "title_template": "{product} Review 2026 - Boost Brain Power Naturally? Real Results",
        "sections": [
            "[HOOK] Brain fog, poor memory, lack of focus - does this sound familiar? What if you could sharpen your mind naturally? Today I am reviewing {product} for cognitive enhancement.",
            "[PROBLEM] Modern life destroys your brain health. Constant screen time, stress, poor sleep, and aging all contribute to declining cognitive function. Brain fog, forgetfulness, and lack of mental clarity become the new normal.",
            "[SOLUTION] {product} is a nootropic formula designed to support brain function. It enhances blood flow to the brain, provides essential nutrients for neural health, and protects against cognitive decline.",
            "[INGREDIENTS] Key ingredients: Bacopa Monnieri for memory, Ginkgo Biloba for blood flow, Phosphatidylserine for cell health, Omega-3s for brain structure, B-vitamins for energy metabolism.",
            "[BENEFITS] Users report: Sharper memory and recall, Better focus and concentration, Clearer thinking, More mental energy, Reduced brain fog, Better mood.",
            "[RESULTS] Clinical research supports these ingredients for cognitive enhancement. Users typically notice improvements within 3-6 weeks of daily use.",
            "[CONCLUSION] If you want to think clearer, remember more, and perform better mentally, {product} is worth trying. Link in description.",
        ],
        "thumbnail_prompt": "Brain with glowing neural connections, {product} bottle in foreground, blue and purple background, cognitive enhancement theme, bold text 'Brain Boost?', professional YouTube thumbnail",
        "tags": "Brain Health, Nootropics, Memory, Focus, Cognitive"
    },
    "eye-health": {
        "title_template": "{product} Review 2026 - Protect Your Vision Naturally? Honest Review",
        "sections": [
            "[HOOK] Staring at screens all day? Worried about your vision getting worse? Today we review {product} - the supplement designed to protect and improve your eye health naturally.",
            "[PROBLEM] Digital eye strain, blue light damage, and age-related vision decline affect almost everyone. Most people do nothing until it is too late. Vision loss can be prevented with the right nutrition.",
            "[SOLUTION] {product} provides concentrated doses of the specific nutrients your eyes need to stay healthy. It targets the root causes of vision decline and supports retinal health.",
            "[INGREDIENTS] Lutein and Zeaxanthin for macular health, Bilberry extract for night vision, Vitamin A for overall eye function, Zinc for retinal health, Omega-3s for dry eye relief.",
            "[BENEFITS] Users report: Reduced eye strain after screen time, Better night vision, Sharper overall vision, Less dry eye irritation, Protection against age-related decline.",
            "[RESULTS] Studies show these nutrients can significantly slow vision decline and in some cases improve visual function within 3 months.",
            "[CONCLUSION] Your vision is precious. {product} offers scientifically backed nutritional support. Check the official link in the description.",
        ],
        "thumbnail_prompt": "Eye with natural green iris, {product} bottle, vision chart background, blue light protection concept, bold text 'Vision Support?', professional medical style YouTube thumbnail",
        "tags": "Eye Health, Vision Support, Blue Light Protection"
    },
    "stress-relief": {
        "title_template": "{product} Review 2026 - Reduce Stress & Anxiety Naturally? Real Results",
        "sections": [
            "[HOOK] Stressed, anxious, and struggling to relax? You are not alone. Today I am reviewing {product} - the natural stress relief supplement taking the health world by storm.",
            "[PROBLEM] Chronic stress is the silent killer of the modern world. It destroys your sleep, your energy, your relationships, and your health. Most people turn to caffeine and alcohol making things worse.",
            "[SOLUTION] {product} uses adaptogenic herbs that help your body adapt to stress. It works with your nervous system to promote calm without drowsiness.",
            "[INGREDIENTS] Ashwagandha for cortisol regulation, L-Theanine for calm focus, Rhodiola for stress resilience, Magnesium for muscle relaxation, Chamomile for gentle calming.",
            "[BENEFITS] Users report: Significantly reduced stress and anxiety, Better sleep quality, Improved mood throughout the day, More mental clarity under pressure, Greater emotional balance.",
            "[RESULTS] Adaptogenic herbs have been used for centuries and are now backed by modern science. Most users feel calmer within 1-2 weeks.",
            "[CONCLUSION] If stress is affecting your quality of life, {product} offers a natural effective solution. Official link in the description.",
        ],
        "thumbnail_prompt": "Calm peaceful person meditating, {product} bottle, nature background, blue and green calm tones, bold text 'Stress Relief?', professional YouTube thumbnail, serene atmosphere",
        "tags": "Stress Relief, Anxiety Support, Adaptogens, Calm"
    },
    "detox": {
        "title_template": "{product} Review 2026 - Cleanse Your Body Naturally? Does It Work?",
        "sections": [
            "[HOOK] Feel sluggish, bloated, and heavy? Your body might be telling you it needs a reset. Today I am reviewing {product} - the natural detox formula that helps cleanse your system.",
            "[PROBLEM] Toxins from food, water, and the environment build up in your body over time. This leads to fatigue, brain fog, digestive issues, skin problems, and slow metabolism.",
            "[SOLUTION] {product} supports your body natural detoxification pathways. It helps your liver, kidneys, and digestive system function optimally to eliminate toxins.",
            "[INGREDIENTS] Milk Thistle for liver support, Dandelion root for kidney health, Activated charcoal for toxin binding, Ginger for digestion, Antioxidants for cellular protection.",
            "[BENEFITS] Users report: More energy and vitality, Clearer skin, Better digestion, Reduced bloating, Stronger immune system, Mental clarity.",
            "[RESULTS] Most users feel a significant difference within the first week of use. A full 30-day cleanse cycle delivers the best results.",
            "[CONCLUSION] If you feel like your body needs a reset, {product} is a great natural option. Check the official link in the description.",
        ],
        "thumbnail_prompt": "Body with glowing detox pathways, {product} bottle, clean fresh water and nature theme, green and blue colors, bold text 'Body Cleanse?', professional YouTube thumbnail",
        "tags": "Detox, Body Cleanse, Liver Health, Natural Cleanse"
    },
    "joint-health": {
        "title_template": "{product} Review 2026 - End Joint Pain Naturally? Honest Results",
        "sections": [
            "[HOOK] Aching knees, stiff hips, painful joints making every movement difficult? Today I am reviewing {product} - the natural joint relief supplement that helps thousands move freely again.",
            "[PROBLEM] Joint pain affects millions. It limits mobility, ruins workouts, and makes aging painful. Anti-inflammatory drugs only mask symptoms while causing side effects.",
            "[SOLUTION] {product} targets joint health at the source. It supports cartilage repair, reduces inflammation naturally, and improves joint lubrication.",
            "[INGREDIENTS] Glucosamine and Chondroitin for cartilage, MSM for joint flexibility, Turmeric/Curcumin for inflammation, Collagen for connective tissue, Hyaluronic acid for lubrication.",
            "[BENEFITS] Users report: Reduced joint pain and stiffness, Better mobility and flexibility, Faster recovery after exercise, Less inflammation, Improved quality of life.",
            "[RESULTS] Clinical studies show significant improvement in joint comfort and mobility within 4-8 weeks of daily supplementation.",
            "[CONCLUSION] If joint pain is holding you back, {product} is a natural solution worth trying. Description has the official link.",
        ],
        "thumbnail_prompt": "Person moving freely running or stretching, {product} bottle, joint highlighting with glowing effect, blue and green health theme, bold text 'Joint Relief?', professional YouTube thumbnail",
        "tags": "Joint Health, Joint Pain Relief, Mobility, Inflammation"
    },
    "energy": {
        "title_template": "{product} Review 2026 - All Day Energy Without the Crash? Real Review",
        "sections": [
            "[HOOK] Tired of relying on coffee and energy drinks that leave you crashing hours later? Today I am reviewing {product} - the natural energy booster that keeps you going all day.",
            "[PROBLEM] Low energy is the most common health complaint. People turn to caffeine, sugar, and stimulants that provide temporary energy followed by a devastating crash.",
            "[SOLUTION] {product} provides clean sustainable energy by supporting your body natural energy production at the cellular level. No crash, no jitters.",
            "[INGREDIENTS] CoQ10 for cellular energy, B-complex vitamins for metabolism, Iron for oxygen transport, Green tea for gentle stimulation, Adaptogens for stress resilience.",
            "[BENEFITS] Users report: Steady energy all day without crashes, Better mental focus, Improved physical performance, No jitters or anxiety, Better sleep at night.",
            "[RESULTS] Most users notice improved energy levels within the first week. Sustained use leads to long-term vitality improvements.",
            "[CONCLUSION] If you want real energy without the crash, {product} is a game changer. Official link with current offers in the description.",
        ],
        "thumbnail_prompt": "Energetic person with lightning bolt effect, {product} bottle, sunrise background, gold and yellow energy theme, bold text 'Natural Energy?', professional YouTube thumbnail",
        "tags": "Energy Boost, Natural Energy, Vitality, No Crash"
    },
    "beauty": {
        "title_template": "{product} Review 2026 - Younger Looking Skin Naturally? Does It Work?",
        "sections": [
            "[HOOK] Want glowing skin, strong hair, and healthy nails without expensive treatments? Today I am reviewing {product} - the beauty supplement that works from the inside out.",
            "[PROBLEM] Aging, pollution, and poor diet damage your skin, hair, and nails. Topical products only work on the surface. True beauty comes from within.",
            "[SOLUTION] {product} provides essential nutrients that your body needs to produce collagen, keratin, and elastin - the building blocks of youth.",
            "[INGREDIENTS] Hydrolyzed Collagen for skin elasticity, Biotin for hair and nails, Vitamin C for collagen synthesis, Hyaluronic acid for hydration, Antioxidants for skin protection.",
            "[BENEFITS] Users report: Smoother more youthful skin, Stronger faster-growing nails, Thicker shinier hair, Reduced fine lines and wrinkles, Better overall complexion.",
            "[RESULTS] Visible improvements typically appear within 3-6 weeks. The best results come with consistent daily use over 3 months.",
            "[CONCLUSION] If you want to look and feel younger naturally, {product} delivers real results. Check the official link in the description.",
        ],
        "thumbnail_prompt": "Beautiful glowing skin face, {product} bottle, collagen molecules visible, pink and gold luxury theme, bold text 'Younger Skin?', professional beauty YouTube thumbnail",
        "tags": "Beauty, Collagen, Anti-Aging, Skin Care, Hair Health"
    },
}

def generate_all_packages():
    products = load_products()
    if not products:
        print("[X] No products found in tracking.json")
        return []
    
    packages = []
    for product in products:
        name = product["produkti"]
        url = product["url_origjinale"]
        try:
            pkg = generate_youtube_package(product)
            packages.append(pkg)
            print(f"  [OK] {pkg['product']} -> {pkg['title'][:60]}...")
        except Exception as e:
            print(f"  [X] Failed for {name}: {e}")
    
    manifest = {
        "generated_at": datetime.now().isoformat(),
        "total": len(packages),
        "packages": [{
            "product": p["product"],
            "title": p["title"],
            "niche": p["niche"]
        } for p in packages]
    }
    
    with open(SCRIPTS_DIR / "_manifest.json", "w", encoding="utf-8") as f:
        json.dump(manifest, f, indent=2, ensure_ascii=False)
    
    return packages

def save_package(pkg, index=0):
    folder = SCRIPTS_DIR / f"{index:03d}_{pkg['product'][:30].replace(' ', '_')}"
    os.makedirs(folder, exist_ok=True)
    
    with open(folder / "title.txt", "w", encoding="utf-8") as f:
        f.write(pkg["title"])
    with open(folder / "script.txt", "w", encoding="utf-8") as f:
        f.write(pkg["script"])
    with open(folder / "description.txt", "w", encoding="utf-8") as f:
        f.write(pkg["description"])
    with open(folder / "thumbnail_prompt.txt", "w", encoding="utf-8") as f:
        f.write(pkg["thumbnail_prompt"])
    with open(folder / "tags.json", "w", encoding="utf-8") as f:
        json.dump(pkg["tags"], f, indent=2)
    
    with open(folder / "package.json", "w", encoding="utf-8") as f:
        json.dump({
            "title": pkg["title"],
            "product": pkg["product"],
            "affiliate_url": pkg["affiliate_url"],
            "niche": pkg["niche"],
            "generated_at": pkg["generated_at"],
            "files": {
                "title": "title.txt",
                "script": "script.txt",
                "description": "description.txt",
                "thumbnail_prompt": "thumbnail_prompt.txt",
                "tags": "tags.json"
            }
        }, f, indent=2, ensure_ascii=False)
    
    return folder

def create_video_from_package(pkg, output_name=None):
    script_sections = [{"text": s.split("]", 1)[-1].strip(), "duration": 8} 
                      for s in pkg["script"].split("\n\n") if s.strip()]
    
    if output_name is None:
        output_name = f"NVH_{pkg['product'][:20].replace(' ', '_')}"
    
    video_path = os.path.join(VIDEO_DIR, f"{output_name}.mp4")
    
    sys.path.insert(0, str(BASE_DIR))
    try:
        from youtube_full_auto import create_video_from_script
        result = create_video_from_script(
            title=pkg["title"],
            script_sections=script_sections,
            desc=pkg["description"],
            hashtags=" ".join(pkg["tags"][:5]),
            output_dir=str(VIDEO_DIR),
            lang="en",
            product_name=pkg.get("product", ""),
            niche=pkg.get("niche", "general-health"),
            affiliate_url=pkg.get("affiliate_url", "")
        )
        print(f"  [OK] Video created: {result}")
        return result
    except Exception as e:
        print(f"  [X] Video creation failed: {e}")
        print("  [i] Script and description saved - you can create video manually.")
        return None

def post_pin_comment(video_id, message):
    try:
        from googleapiclient.discovery import build
        import pickle
        token_file = BASE_DIR / "token.pickle"
        if not token_file.exists():
            print("  [i] No token.pickle, skipping comment")
            return
        with open(token_file, "rb") as f:
            creds = pickle.load(f)
        youtube = build("youtube", "v3", credentials=creds)
        body = {
            "snippet": {
                "videoId": video_id,
                "topLevelComment": {
                    "snippet": {"textOriginal": message}
                }
            }
        }
        youtube.commentThreads().insert(part="snippet", body=body).execute()
        print(f"  [OK] Comment posted (pinning not supported by YouTube API)")
    except Exception as e:
        print(f"  [i] Comment skipped: {e}")

def upload_video_to_youtube(video_path, pkg, privacy="public"):
    sys.path.insert(0, str(BASE_DIR))
    try:
        from youtube_upload import upload_video
        url = upload_video(
            video_path,
            pkg["title"],
            pkg["description"],
            pkg["tags"],
            privacy
        )
        print(f"  [OK] Uploaded: {url}")
        vid = url.split("/")[-1]
        msg = pkg.get("affiliate_url", "")
        post_pin_comment(vid, msg)
        return url
    except Exception as e:
        print(f"  [X] Upload failed: {e}")
        return None

def cmd_generate_all():
    print(f"\n{'='*60}")
    print(f" NaturalVitalityHub - Content Generator")
    print(f" Date: {datetime.now().strftime('%Y-%m-%d %H:%M')}")
    print(f"{'='*60}\n")
    
    packages = generate_all_packages()
    if not packages:
        return
    
    print(f"\n Generated {len(packages)} video packages.\n")
    
    for i, pkg in enumerate(packages):
        folder = save_package(pkg, i)
        print(f"  [{i+1}/{len(packages)}] {pkg['product']}")
        print(f"       Title: {pkg['title'][:70]}...")
        print(f"       Niche: {pkg['niche']}")
        print(f"       Saved: {folder}\n")
    
    print(f"{'='*60}")
    print(f" All packages saved to: {SCRIPTS_DIR}")
    print("=" * 60)

def cmd_generate_single(product_name=None, auto_yes=False, auto_privacy="unlisted"):
    if not product_name:
        product_name = input("Product name to generate: ").strip()
    
    products = load_products()
    matches = [p for p in products if product_name.lower() in p["produkti"].lower()]
    
    if not matches:
        print(f"[X] No product found matching '{product_name}'")
        available = [p["produkti"] for p in products[:20]]
        print(f"    Available: {', '.join(available[:10])}...")
        return
    
    product = matches[0]
    print(f"\n Generating package for: {product['produkti']}\n")
    
    pkg = generate_youtube_package(product)
    folder = save_package(pkg, 0)
    
    print(f"  Title: {pkg['title']}")
    print(f"  Niche: {pkg['niche']}")
    print(f"  Saved to: {folder}\n")
    print(f"  Script preview (first 200 chars):")
    print(f"  {pkg['script'][:200]}...\n")
    
    if auto_yes:
        create = "yes"
    else:
        create = input("Create video now? (yes/no): ").lower()
    if create in ("yes", "y", "po"):
        video_path = create_video_from_package(pkg)
        if video_path:
            if auto_yes:
                upload = "yes"
                privacy = auto_privacy
            else:
                upload = input("Upload to YouTube? (yes/no): ").lower()
                if upload in ("yes", "y", "po"):
                    privacy = input("Privacy (public/unlisted/private) [public]: ").strip() or "public"
            if auto_yes or upload in ("yes", "y", "po"):
                url = upload_video_to_youtube(video_path, pkg, privacy)
                if url:
                    published = load_published()
                    published[product["produkti"]] = {"url": url, "date": datetime.now().isoformat(), "title": pkg["title"], "niche": pkg["niche"]}
                    save_published(published)
                    remaining = len(load_products()) - len(published)
                    print(f"  Published: {product['produkti']} -> {url}")
                    print(f"  Remaining unpublished: {remaining}")

def cmd_list_products():
    products = load_products()
    if not products:
        print("[X] No products found.")
        return
    
    print(f"\n Products in Digistore24 ({len(products)} total):\n")
    
    niches = {}
    for p in products:
        niche = get_niche(p["produkti"])
        if niche not in niches:
            niches[niche] = []
        niches[niche].append(p["produkti"])
    
    published = load_published()
    for niche, items in sorted(niches.items()):
        pub_count = sum(1 for i in items if i in published)
        print(f"  [{niche}] ({len(items)}, {pub_count} published)")
        for item in items[:5]:
            status = "✓" if item in published else " "
            print(f"    [{status}] {item}")
        if len(items) > 5:
            print(f"    ... and {len(items)-5} more")
        print()
    
    print(f"  Use: python {__file__} generate <product_name>")

def cmd_schedule():
    print(f"\n{'='*60}")
    print(f" 24/7 SCHEDULER - NaturalVitalityHub")
    print(f"{'='*60}\n")
    
    if not SCHEDULE_AVAILABLE:
        print(" [X] 'schedule' module not installed.")
        print("     Install: pip install schedule\n")
        print("     For now, run this manually every time:")
        print(f"     python {__file__} all\n")
        return
    
    interval = input("Hours between uploads [24]: ").strip() or "24"
    interval = int(interval)
    
    print(f"\n Scheduler will post every {interval} hours.")
    print(f" Starting at: {datetime.now().strftime('%Y-%m-%d %H:%M')}\n")
    print(" Press Ctrl+C to stop.\n")
    
    products = load_products()
    random.shuffle(products)
    product_cycle = iter(products * 100)
    
    def job():
        try:
            product = next(product_cycle)
            print(f"[{datetime.now().strftime('%H:%M')}] Generating: {product['produkti']}")
            pkg = generate_youtube_package(product)
            folder = save_package(pkg, random.randint(0, 9999))
            print(f"  [OK] Package saved: {folder}")
            
            video_path = create_video_from_package(pkg)
            if video_path:
                url = upload_video_to_youtube(video_path, pkg, "public")
                if url:
                    print(f"  [OK] LIVE: {url}")
                else:
                    print(f"  [X] Upload failed, video saved at: {video_path}")
            else:
                print(f"  [i] Script saved, no video created.")
        except Exception as e:
            print(f"  [X] Error: {e}")
    
    job()
    schedule.every(interval).hours.do(job)
    
    try:
        while True:
            schedule.run_pending()
            time.sleep(60)
    except KeyboardInterrupt:
        print("\n\n Scheduler stopped.")

def cmd_export_to_csv():
    products = load_products()
    if not products:
        print("[X] No products found.")
        return
    
    csv_path = BASE_DIR / "digistore24_products_export.csv"
    with open(csv_path, "w", encoding="utf-8") as f:
        f.write("Product,Affiliate URL,Commission,Category\n")
        for p in products:
            niche = get_niche(p["produkti"])
            f.write(f'"{p["produkti"]}","{p["url_origjinale"]}","60-75%","{niche}"\n')
    
    print(f"\n Exported {len(products)} products to: {csv_path}")
    print(f" Open in Excel or Google Sheets.")

def cmd_dashboard():
    products = load_products()
    total = len(products)
    
    niches = {}
    for p in products:
        n = get_niche(p["produkti"])
        niches[n] = niches.get(n, 0) + 1
    
    print(f"\n{'='*60}")
    print(f" NaturalVitalityHub - DASHBOARD")
    print(f"{'='*60}")
    print(f" Channel: https://www.youtube.com/@NaturalVitalityHub-y4d")
    print(f" PayPal: resul.paypal@gmail.com")
    print(f" Digistore24 Products: {total}")
    published = load_published()
    print(f" Published: {len(published)} / {total}")
    print(f"{'='*60}\n")
    print(f" Products by Category:")
    for niche, count in sorted(niches.items(), key=lambda x: -x[1]):
        bar = "#" * count
        print(f"  {niche:20s} ({count:2d}) {bar}")
    print(f"\n{'='*60}")
    print(f" Commands:")
    print(f"  python {__file__} all        - Generate all video packages")
    print(f"  python {__file__} list       - List all products")
    print(f"  python {__file__} generate   - Generate single product")
    print(f"  python {__file__} generate-auto - Auto-pick next & publish")
    print(f"  python {__file__} schedule   - Start 24/7 scheduler")
    print(f"  python {__file__} export     - Export products to CSV")
    print(f"{'='*60}")

def main():
    if len(sys.argv) < 2:
        cmd_dashboard()
        return
    
    cmd = sys.argv[1].lower()
    
    if cmd == "all":
        cmd_generate_all()
    elif cmd == "list":
        cmd_list_products()
    elif cmd == "generate":
        product = " ".join(sys.argv[2:]) if len(sys.argv) > 2 else None
        cmd_generate_single(product)
    elif cmd in ("generate-auto", "genauto"):
        product = None
        privacy = "public"
        if len(sys.argv) > 2:
            arg1 = sys.argv[2]
            if arg1 in ("public", "unlisted", "private"):
                privacy = arg1
            else:
                product = arg1
                if len(sys.argv) > 3 and sys.argv[3] in ("public", "unlisted", "private"):
                    privacy = sys.argv[3]
        if not product:
            next_p = get_next_unpublished()
            if not next_p:
                print("[OK] All products published! Resetting tracker for re-publish.")
                save_published({})
                next_p = get_next_unpublished()
            product = next_p["produkti"] if next_p else "Provadent"
        print(f"[Auto] Product: {product} (privacy: {privacy})")
        cmd_generate_single(product, auto_yes=True, auto_privacy=privacy)
    elif cmd == "schedule":
        cmd_schedule()
    elif cmd == "export":
        cmd_export_to_csv()
    elif cmd == "dashboard":
        cmd_dashboard()
    elif cmd == "help":
        print(f"""
 NaturalVitalityHub - YouTube Automation System

 Commands:
   python {__file__} all          Generate video packages for ALL products
   python {__file__} list         List all Digistore24 products
   python {__file__} generate     Generate package for one product
   python {__file__} schedule     Start 24/7 auto-publishing scheduler
   python {__file__} export       Export products to CSV
   python {__file__} dashboard    Show dashboard
""")
    else:
        print(f"[X] Unknown command: {cmd}")
        print(f"    Use: python {__file__} help")

if __name__ == "__main__":
    main()
