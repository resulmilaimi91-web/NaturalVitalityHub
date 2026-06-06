import random
from datetime import datetime

SOCIAL_PLATFORMS = ["tiktok", "youtube", "twitter", "instagram", "facebook"]

TIKTOK_CAPTION_TEMPLATES = [
    "{produkti} - Ja pse duhet ta kesh patjetër! #fyp #produkti #{niche}",
    "Nuk do ta besosh çfarë zbuluam! {produkti} #{niche} #review",
    "3 arsye pse {produkti} është më i miri #{niche} #produkti",
    "{produkti} për {price} - A ia vlen? Shiko videon! #{niche} #fyp",
    "Unboxing i {produkti} - Reagimi im i parë! #{niche} #unboxing"
]

YOUTUBE_DESCRIPTION_TEMPLATES = [
    """📌 {titull_artikulli}

🔗 Lidhje të dobishme:
{links}

📋 Përmbledhje:
{permbledhje}

⏱ Kapitulli:
{kapitujt}

📢 Nëse ju pëlqeu videoja, mos harroni të:
👍 Like
🔔 Subscribe
💬 Komentoni

*Ky video përmban linke affiliate. Ne mund të fitojmë një komision nëse blini nëpërmjet këtyre linkeve.*
""",
    """{titull_artikulli}

🎯 Çfarë do të mësoni në këtë video:
{permbledhje}

🛒 Produktet e përmendura:
{links}

#affiliate #{niche} #{niche2}
"""
]

TWITTER_THREAD_TEMPLATES = [
    "1/ {titull_artikulli}\n\n{intro}\n\nNjë thread i shkurtër për të gjithë ata që duan të dinë më shumë 👇\n\n2/ Arsyeja e parë pse {produkti} ia vlen:\n{arsye1}\n\n3/ Arsyeja e dytë:\n{arsye2}\n\n4/ Arsyeja e tretë:\n{arsye3}\n\n5/ Përfundimi:\n{perfundim}\n\n{link}",
    "{titull_artikulli}\n\n{intro}\n\nLinku: {link}\n\n#{niche} #{niche2}"
]

INSTAGRAM_CAPTION_TEMPLATES = [
    """✨ {titull_artikulli}

{intro}

💎 Pse e rekomandojmë?
✅ Cilësi e lartë
✅ Çmim i volitshëm
✅ Vlerë për para

🔗 Linku në bio!

{hashtags}""",
    """🔥 {titull_artikulli}

{intro}

🛒 {link_desc}

{hashtags}"""
]

HASHTAGS = {
    "tech-gadgets": "#tech #gadgets #teknologji #produkte #review #shqip #2026 #smart #gadget #techreview",
    "online-courses": "#kurs #onlinecourse #edukim #mesim #certifikim #online #shqip",
    "hosting": "#hosting #webhosting #website #wordpress #domain #shqip",
    "software-saas": "#software #saas #apps #mjete #produktivitet #shqip",
    "health-fitness": "#fitness #shendet #stervitje #sport #pajisje #shqip"
}

def gjenero_tiktok(produkti, niche):
    template = random.choice(TIKTOK_CAPTION_TEMPLATES)
    caption = template.format(
        produkti=produkti["name"],
        price=produkti["price_range"],
        niche=niche
    )
    hashtags = HASHTAGS.get(niche, f"#{niche}")
    return {"platform": "tiktok", "caption": caption + " " + hashtags, "produkti": produkti["name"]}

def gjenero_youtube(artikull, produktet, niche):
    template = random.choice(YOUTUBE_DESCRIPTION_TEMPLATES)
    links = "\n".join([f"- {p['name']}: [Link](affiliate-link-here)" for p in produktet[:5]])
    permbledhje = f"Në këtë video shqyrtojmë {', '.join([p['name'] for p in produktet[:3]])}."
    kapitujt = "\n".join([f"{i*2}:00 - {p['name']}" for i, p in enumerate(produktet[:5])])

    description = template.format(
        titull_artikulli=artikull.get("titull", ""),
        links=links,
        permbledhje=permbledhje,
        kapitujt=kapitujt,
        niche=niche,
        niche2=niche
    )
    return {"platform": "youtube", "description": description}

def gjenero_twitter(artikull, produktet, niche):
    template = random.choice(TWITTER_THREAD_TEMPLATES)
    produkti = produktet[0] if produktet else {"name": "Produkti"}
    intro = f"A po kërkoni për {produkti['name']}? Ja gjithçka që duhet të dini."
    arsye1 = f"Cilësi e lartë për një çmim të volitshëm ({produkti['price_range']})"
    arsye2 = "Lehtësi në përdorim dhe setup i shpejtë"
    arsye3 = "Mbështetje teknike dhe garanci e përfshirë"
    perfundim = f"{produkti['name']} ia vlen absolutisht. Rekomandohet!"

    thread = template.format(
        titull_artikulli=artikull.get("titull", ""),
        intro=intro,
        produkti=produkti["name"],
        arsye1=arsye1,
        arsye2=arsye2,
        arsye3=arsye3,
        perfundim=perfundim,
        link="[Linku i produktit]",
        niche=niche,
        niche2=niche
    )
    return {"platform": "twitter", "thread": thread}

def gjenero_instagram(artikull, produktet, niche):
    template = random.choice(INSTAGRAM_CAPTION_TEMPLATES)
    intro = f"A po kërkoni për produktet më të mira në {niche}? Ne i kemi provuar për ju!"
    link_desc = "Linku në bio për të gjitha produktet e përmendura!"
    hashtags = HASHTAGS.get(niche, "")

    caption = template.format(
        titull_artikulli=artikull.get("titull", ""),
        intro=intro,
        link_desc=link_desc,
        hashtags=hashtags
    )
    return {"platform": "instagram", "caption": caption}

def gjenero_postime(artikull, produktet, niche, count=5):
    postimet = []
    for _ in range(count):
        platform = random.choice(SOCIAL_PLATFORMS)
        if platform == "tiktok":
            p = gjenero_tiktok(random.choice(produktet), niche)
        elif platform == "youtube":
            p = gjenero_youtube(artikull, produktet, niche)
        elif platform == "twitter":
            p = gjenero_twitter(artikull, produktet, niche)
        elif platform == "instagram":
            p = gjenero_instagram(artikull, produktet, niche)
        else:
            p = gjenero_tiktok(random.choice(produktet), niche)
        postimet.append(p)
    return postimet
