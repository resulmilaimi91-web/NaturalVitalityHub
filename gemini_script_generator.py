import json, os, sys, re
from pathlib import Path
from google import genai

GEMINI_API_KEY = os.environ.get("GEMINI_API_KEY", "")
if not GEMINI_API_KEY:
    config_path = Path(__file__).parent / "config.json"
    if config_path.exists():
        try:
            import json as _json
            GEMINI_API_KEY = _json.loads(config_path.read_text()).get("GEMINI_API_KEY", "")
        except:
            pass

if not GEMINI_API_KEY:
    print("[X] GEMINI_API_KEY not set. Set env var or add to config.json")
    GEMINI_API_KEY = ""

MODEL = "gemini-2.0-flash"

client = genai.Client(api_key=GEMINI_API_KEY)

NICHE_ANGLES = {
    "dental-health": "tooth pain, bad breath, expensive dentist, confidence to smile",
    "blood-sugar": "fatigue, sugar cravings, energy crashes, diabetes fear",
    "men-health": "low energy, performance anxiety, aging, confidence loss",
    "sleep-health": "insomnia, tired mornings, brain fog, dark circles",
    "gut-health": "bloating, digestion issues, skin problems, low energy",
    "weight-loss": "belly fat, failed diets, slow metabolism, frustration",
    "brain-health": "memory loss, brain fog, focus issues, aging mind",
    "eye-health": "screen strain, blurry vision, eye fatigue, aging eyes",
    "stress-relief": "anxiety, overwhelm, burnout, can't relax",
    "detox": "sluggish, toxic buildup, low vitality, skin issues",
    "joint-health": "joint pain, stiffness, limited mobility, aging",
    "energy": "tired all day, coffee dependency, no motivation",
    "beauty": "wrinkles, aging skin, hair loss, dull complexion",
    "minerals": "deficiency, weakness, poor health, nutrient gaps",
    "fitness": "muscle loss, slow gains, recovery, workout plateau",
    "general-health": "feeling unwell, low immunity, poor health, aging"
}

def generate_script(product_name, niche="general-health", affiliate_url=""):
    angle = NICHE_ANGLES.get(niche, "health struggles, frustration, looking for solutions")
    
    prompt = f"""You are a top YouTube scriptwriter for health supplements. Write a 60-90 second video script that sells.

PRODUCT: {product_name}
NICHE: {niche}
PAIN POINTS: {angle}
AFFILIATE URL: {affiliate_url}

Write a curiosity-driven, emotional script with EXACTLY these 6 sections separated by '---':

1. HOOK (10-15 seconds) - Start with a relatable pain point that grabs attention. Make them think "this is me!"
2. PROBLEM (10-15 seconds) - Deepen the pain. Make them feel the urgency to change.
3. SOLUTION (10-15 seconds) - Introduce the product as the natural answer. Build trust.
4. INGREDIENTS (10-15 seconds) - Natural, safe, science-backed. 3 key ingredients.
5. BENEFITS (10-15 seconds) - Transformational results. How life changes after using it.
6. CONCLUSION (10-15 seconds) - Urgency, CTA, "link in description", discount mention.

RULES:
- Natural conversational tone, NOT robotic or salesy
- Use "you" and "your" - speak directly to the viewer
- Short punchy sentences. Every word must earn its place.
- End each section wanting more.
- NO reviewspeak like "today we are reviewing" - instead make it a story/discovery
- Generate curiosity - hint at transformation
- Total script must be 60-90 seconds of spoken content

Return ONLY the 6 sections separated by '---'. No extra text."""

    try:
        resp = client.models.generate_content(model=MODEL, contents=prompt)
        text = resp.text.strip()
        
        sections_raw = [s.strip() for s in text.split("---") if s.strip()]
        
        sections = []
        for s in sections_raw[:6]:
            clean = re.sub(r'^\d+\.\s*\*{0,2}(HOOK|PROBLEM|SOLUTION|INGREDIENTS|BENEFITS|CONCLUSION)\*{0,2}\s*:?\s*', '', s, flags=re.IGNORECASE)
            clean = clean.strip().strip('"').strip("'")
            if clean:
                sections.append(clean)
        
        while len(sections) < 6:
            sections.append(f"Discover how {product_name} can transform your health naturally. Check the link below.")

        return sections[:6]
    except Exception as e:
        print(f"  [X] Gemini script generation failed: {e}")
        print("  [i] Falling back to template script")
        return None


def generate_viral_elements(product_name, niche="general-health", affiliate_url=""):
    prompt = f"""Generate viral YouTube content for {product_name} in the {niche} niche.

Return ONLY valid JSON with exactly these fields:
{{
  "title": "A curiosity-driven YouTube title (max 70 chars, includes {product_name})",
  "hook": "A single powerful attention-grabbing sentence (max 20 words)",
  "hashtags": ["#tag1", "#tag2", "#tag3", "#tag4", "#tag5"],
  "description_hook": "A 2-3 sentence description that builds curiosity (max 50 words)"
}}

Rules for title:
- MUST include product name
- Create curiosity/wonder
- Examples: "I Tried {product_name} for 30 Days (HERE'S WHAT HAPPENED)"
- Max 70 characters
- No clickbait that lies, but create curiosity

Rules for description_hook:
- Build on the curiosity
- Make them want to scroll to the link
- Natural and trustworthy tone

Return ONLY valid JSON. No other text."""

    try:
        resp = client.models.generate_content(model=MODEL, contents=prompt)
        text = resp.text.strip()
        text = re.sub(r'^```(?:json)?\s*', '', text)
        text = re.sub(r'\s*```$', '', text)
        data = json.loads(text)
        return data
    except Exception as e:
        print(f"  [X] Gemini viral elements failed: {e}")
        return None


def generate_rich_description(product_name, niche, affiliate_url, script):
    sections_raw = script.split("\n\n") if isinstance(script, str) else script
    hook = sections_raw[0][:200] if sections_raw else ""
    
    prompt = f"""Write a YouTube description for a video about {product_name} ({niche}).

Product link: {affiliate_url}
Channel: NaturalVitalityHub

Write a description that:
1. Opens with curiosity (build on this hook: "{hook}")
2. Lists 5 key benefits briefly
3. Includes the affiliate link prominently
4. Has a strong call to action
5. Ends with a disclaimer

Format:
- Use emojis (✅ 🔥 👉 🎁 ⚠️ 🔔)
- Keep it natural and scannable
- No more than 300 words total
- Link should be on its own line
- Include: #NaturalVitalityHub #{product_name.replace(' ', '')} #{niche} #SupplementReview #HealthTips

Return the full description text."""

    try:
        resp = client.models.generate_content(model=MODEL, contents=prompt)
        return resp.text.strip()
    except Exception as e:
        print(f"  [X] Gemini description failed: {e}")
        return None


def generate_hashtags(product_name, niche):
    prompt = f"""Generate 20 YouTube hashtags for a video about {product_name} ({niche}).
Include: product name tag, niche tags, general health tags, trending health tags.
Return them space-separated starting with #. Example: #ProductName #Niche #HealthTips
No other text, just the hashtags."""

    try:
        resp = client.models.generate_content(model=MODEL, contents=prompt)
        return resp.text.strip()
    except Exception as e:
        return f"#{product_name.replace(' ', '')} #{niche} #NaturalHealth #Supplement"


def cleanup():
    pass

if __name__ == "__main__":
    test_name = "Blood Sugar Blaster"
    test_niche = "blood-sugar"
    
    print(f"Generating script for {test_name}...")
    sections = generate_script(test_name, test_niche)
    if sections:
        for i, s in enumerate(sections):
            print(f"\n  Section {i+1}: {s[:100]}...")
    else:
        print("Failed to generate")
    
    print(f"\nGenerating viral elements...")
    viral = generate_viral_elements(test_name, test_niche)
    if viral:
        print(f"  Title: {viral.get('title', 'N/A')}")
        print(f"  Hook: {viral.get('hook', 'N/A')}")
        print(f"  Hashtags: {viral.get('hashtags', [])}")
