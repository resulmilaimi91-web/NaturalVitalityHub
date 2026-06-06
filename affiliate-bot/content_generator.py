import os, json, random
from datetime import datetime
from config.affiliate_programs import PRODUCT_TEMPLATES, NICHES

CONTENT_TYPES = ["review", "comparison", "top10", "guide"]
CONTENT_DIR = os.path.join(os.path.dirname(__file__), "output", "articles")

REMINDER = "\n\n*Ky postim përmban linke affiliate. Ne mund të fitojmë një komision nëse blini nëpërmjet këtyre linkeve, pa kosto shtesë për ju.*"

import re as _re

def _slugify(text):
    text = text.lower()
    text = text.encode('ascii', 'ignore').decode('ascii')
    text = _re.sub(r'[^a-z0-9\s-]', '', text)
    text = text.replace(" ", "-")
    text = _re.sub(r'-+', '-', text)
    return text.strip('-')[:50]

def _kategoria_shqip(niche):
    return NICHES.get(niche, niche)

def gjenero_review(produkti, niche, paypal_email):
    data = datetime.now().strftime("%Y-%m-%d")
    kategoria = _kategoria_shqip(niche)
    titull = f"Rishikim i Plotë: {produkti['name']} ({produkti['price_range']}) - A Vlen ta Bleni?"
    slug = _slugify(titull)
    meta_desc = f"Lexo rishikimin tonë të detajuar për {produkti['name']}. Avantazhet, disavantazhet, çmimi dhe a ia vlen ta blini në {data}."

    body = f"""# {titull}

**Publikuar:** {data} | **Kategoria:** {kategoria}

## Përmbledhje
{produkti['name']} është një nga produktet më të diskutuara në {kategoria} për {data}. 
Në këtë rishikim të detajuar, do të shohim gjithçka që duhet të dini para se të blini.

## Çfarë është {produkti['name']}?
{produkti['name']} ofron një zgjidhje të shkëlqyer për ata që kërkojnë cilësi dhe vlerë për para. 
Me një çmim prej {produkti['price_range']}, ky produkt pozicionohet në treg si një opsion konkurrues.

## Karakteristikat Kryesore
- Cilësi e lartë ndërtimi
- Raport i shkëlqyer çmim/cilësi
- Mbështetje teknike e përfshirë (për produktet digitale)
- Garancion dhe mbështetje pas shitjes

## Avantazhet
- Çmim konkurrues në treg
- Lehtësi në përdorim
- Vlerë e mirë për para
- Përditësime të rregullta (nëse aplikohet)

## Disavantazhet
- Mund të ketë alternativë më të lirë
- Kërkon pak kohë për t'u mësuar

## Përfundim
{produkti['name']} është një zgjedhje e shkëlqyer për këdo që kërkon një zgjidhje të besueshme në {kategoria}. 
E rekomandojmë për përdoruesit që duan cilësi dhe nuk duan të komprometojnë.

[Merr {produkti['name']} tani ->]
{REMINDER}"""
    return {"titull": titull, "slug": slug, "meta_description": meta_desc, "body": body, "type": "review", "produkti": produkti["name"]}

def gjenero_krahasim(produktet, niche):
    data = datetime.now().strftime("%Y-%m-%d")
    kategoria = _kategoria_shqip(niche)
    emrat = [p["name"] for p in produktet[:3]]
    titull = f"Krahasim: {' vs '.join(emrat)} - Cili është më i miri për ju?"
    slug = _slugify(titull)

    body = f"""# {titull}

**Publikuar:** {data} | **Kategoria:** {kategoria}

## Hyrje
Në këtë krahasim do të shohim produktet më të mira në {kategoria}. 
Cili prej tyre ofron më shumë vlerë për para?

## Tabela e Krahasimit

| Produkti | Çmimi | Vlerësimi | Më i miri për |
"""
    for p in produktet[:4]:
        body += f"| {p['name']} | {p['price_range']} | ⭐ 4.5/5 | Përdoruesit që kërkojnë cilësi |\n"

    body += f"""
## Krahasimi i Detajuar

### 1. {emrat[0] if len(emrat) > 0 else ""}
Produkti i parë në krahasim ofron një balancë të shkëlqyer midis çmimit dhe cilësisë.
Është ideal për fillestarët dhe përdoruesit e mesëm.

### 2. {emrat[1] if len(emrat) > 1 else ""}
Opsioni i dytë është më i avancuar dhe ofron më shumë funksione.
Përshtatet më mirë për përdoruesit profesionistë.

### 3. {emrat[2] if len(emrat) > 2 else ""}
Produkti i tretë është alternativa më ekonomike, perfekte për ata me buxhet të kufizuar.

## Cilin të zgjidhni?
Zgjedhja varet nga nevojat tuaja specifike dhe buxheti. 
Rekomandojmë të filloni me {emrat[0] if len(emrat) > 0 else "produktin e parë"} nëse jeni fillestar.

[Shiko çmimet më të mira ->]
{REMINDER}"""
    return {"titull": titull, "slug": slug, "body": body, "type": "comparison"}

def gjenero_top10(produktet, niche):
    data = datetime.now().strftime("%Y-%m-%d")
    kategoria = _kategoria_shqip(niche)
    titull = f"Top 10 {kategoria} më të Mirë për {data}"
    slug = _slugify(titull)
    meta_desc = f"Zbuloni 10 {kategoria.lower()} më të mira të vitit {data}. Lista e përditësuar me çmime, vlerësime dhe rekomandime."

    body = f"""# {titull}

**Publikuar:** {data} | **Kategoria:** {kategoria}

## Hyrje
Pas kërkimeve të gjata dhe testimit të produkteve më të mira, kemi përpiluar listën 
me 10 {kategoria.lower()} më të mira për {data}.

## Top 10 Lista

"""
    for i, p in enumerate(produktet[:10], 1):
        body += f"""### {i}. {p['name']}
**Çmimi:** {p['price_range']}
{p['name']} është një zgjedhje e shkëlqyer për ata që kërkojnë cilësi dhe performancë.
[Shiko më shumë ->]

"""
    body += f"""## Përfundim
Kjo listë përfshin produktet më të mira të {data}. Secili prej tyre ofron vlerë të shkëlqyer.

[Shiko të gjitha produktet ->]
{REMINDER}"""
    return {"titull": titull, "slug": slug, "meta_description": meta_desc, "body": body, "type": "top10"}

def gjenero_udhezues(produktet, niche):
    data = datetime.now().strftime("%Y-%m-%d")
    kategoria = _kategoria_shqip(niche)
    titull = f"Udhëzues i Plotë për {kategoria} - {data}"
    slug = _slugify(titull)

    body = f"""# {titull}

**Publikuar:** {data}

## Hyrje
Ky udhëzues i detajuar do t'ju ndihmojë të kuptoni gjithçka që duhet të dini për {kategoria.lower()}.

## Çfarë duhet të keni parasysh para se të blini?
Para se të investoni në {kategoria.lower()}, ka disa faktorë të rëndësishëm që duhet të merrni parasysh:

1. **Buxheti** - Sa jeni gati të shpenzoni?
2. **Nevojat** - Çfarë kërkoni nga produkti?
3. **Cilësia** - A ia vlen të paguani më shumë për cilësi më të mirë?

## Rekomandimet Tona

"""
    for i, p in enumerate(produktet[:5], 1):
        body += f"""### Rekomandimi {i}: {p['name']}
**Çmimi:** {p['price_range']}
- Ideal për përdoruesit që kërkojnë vlerë për para
- Cilësi e garantuar
[Shiko ofertën ->]

"""
    body += f"""## Pyetje të Shpeshta

**Sa kushtojnë {kategoria.lower()}?**
Çmimet variojnë nga opsione ekonomike deri tek ato premium.

**Cila është zgjedhja më e mirë për fillestarët?**
Rekomandojmë të filloni me produkte me çmim mesatar dhe vlerësim të lartë.

{REMINDER}"""
    return {"titull": titull, "slug": slug, "body": body, "type": "guide"}

def gjenero_artikuj(produktet, niche, count=3):
    os.makedirs(CONTENT_DIR, exist_ok=True)
    artikujt = []
    tipe = random.choices(CONTENT_TYPES, k=count)

    for tipi in tipe[:count]:
        if tipi == "review":
            artikull = gjenero_review(random.choice(produktet), niche, "")
        elif tipi == "comparison":
            artikull = gjenero_krahasim(produktet, niche)
        elif tipi == "top10":
            artikull = gjenero_top10(produktet, niche)
        else:
            artikull = gjenero_udhezues(produktet, niche)
        artikujt.append(artikull)

        filepath = os.path.join(CONTENT_DIR, f"{artikull['slug']}.md")
        with open(filepath, "w", encoding="utf-8") as f:
            f.write(artikull["body"])

    manifest = {
        "generated": datetime.now().isoformat(),
        "niche": niche,
        "articles": [{"titull": a["titull"], "slug": a["slug"], "file": f"{a['slug']}.md", "type": a["type"]} for a in artikujt]
    }
    manifest_path = os.path.join(CONTENT_DIR, "_manifest.json")
    with open(manifest_path, "w", encoding="utf-8") as f:
        json.dump(manifest, f, indent=2, ensure_ascii=False)

    return artikujt

def lexo_artikujt():
    manifest_path = os.path.join(CONTENT_DIR, "_manifest.json")
    if os.path.exists(manifest_path):
        with open(manifest_path, "r", encoding="utf-8") as f:
            return json.load(f)
    return {"articles": []}

def gjenero_seo_meta(artikull):
    return {
        "title": artikull.get("titull", ""),
        "description": artikull.get("meta_description", ""),
        "slug": artikull.get("slug", ""),
        "tags": artikull.get("titull", "").lower().split()[:5]
    }
