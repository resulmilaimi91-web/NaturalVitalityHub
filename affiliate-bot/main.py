#!/usr/bin/env python3
"""
AI Affiliate Bot - Sistemi i plote per affiliate marketing
"""

import os, sys, json, random
from datetime import datetime

from config.settings import settings
from config.affiliate_programs import AFFILIATE_PROGRAMS, PRODUCT_TEMPLATES, NICHES
from content_generator import *
from social_posts import gjenero_postime
from seo_optimizer import kontrollo_seo, llogarit_dendesine_fjalesh, gjenero_keywords, optimize_titull, optimize_meta_description
from tracking import tracker
from reporting import gjenero_html_raport

VERSION = "1.0.0"

sys.stdout.reconfigure(encoding='utf-8', errors='replace')

def cmd_konfiguro():
    print("=== KONFIGURIMI I SISTEMIT ===\n")
    paypal = input(f"PayPal email [{settings['paypal_email']}]: ").strip()
    if paypal:
        settings["paypal_email"] = paypal

    print("\nNiches ne dispozicion:")
    for key, name in NICHES.items():
        print(f"  {key} - {name}")
    niche = input(f"\nZgjidh niche [{settings['niche']}]: ").strip()
    if niche and niche in NICHES:
        settings["niche"] = niche

    artikuj = input(f"Artikuj ne jave [{settings['content_schedule']['articles_per_week']}]: ").strip()
    if artikuj.isdigit():
        settings["content_schedule"]["articles_per_week"] = int(artikuj)

    settings.save()
    print("\n[OK] Konfigurimi u ruajt!")

def cmd_gjenero():
    niche = settings["niche"]
    if niche not in PRODUCT_TEMPLATES:
        print(f"[X] Niche '{niche}' nuk u gjet. Perdor 'konfiguro' per te vendosur nje niche.")
        return

    template = PRODUCT_TEMPLATES[niche]
    produktet = template["example_products"]
    count = settings["content_schedule"]["articles_per_week"]

    print(f"\n=== GJENERIM I ARTIKUJVE ({count}) ===")
    print(f"Niche: {niche}")
    print(f"Produkte: {[p['name'] for p in produktet]}\n")

    artikujt = gjenero_artikuj(produktet, niche, count)

    for a in artikujt:
        print(f"  [OK] {a['type'].upper()}: {a['titull']}")
        seo = kontrollo_seo(a["body"], keyword=niche, titull=a["titull"])
        print(f"       SEO Score: {seo['score']}/100 | Fjalet: {seo.get('word_count', 0)}")

    print(f"\n=== GJENERIM I POSTIMEVE SOCIALE ({count * 5}) ===")
    for a in artikujt:
        postimet = gjenero_postime(a, produktet, niche, 5)
        for p in postimet:
            tekst = str(p.get('caption', p.get('thread', p.get('description', ''))))
            print(f"  [{p['platform'].upper()}] {tekst[:60]}...")

    print(f"\n[OK] Artikujt u ruajten ne: output/articles/")
    print(f"[OK] Total: {len(artikujt)} artikuj, {len(artikujt)*5} postime sociale")

def cmd_programet():
    print("\n=== PROGRAMET AFFILIATE ===\n")
    for key, prog in AFFILIATE_PROGRAMS.items():
        print(f"  * {prog['name']}")
        print(f"    URL: {prog['url']}")
        print(f"    Komision: deri {prog['avg_commission_pct']}%")
        print(f"    Pagesa: {prog['payment_frequency']} (min ${prog['min_payout']})")
        print(f"    Metodat: {', '.join(prog['payment_methods'])}")
        print(f"    Regjistrohu: {prog['signup_url']}")
        print()

def cmd_linke():
    print("\n=== MENAXHIMI I LINKEVE AFFILIATE ===\n")
    url = input("URL origjinale e produktit: ").strip()
    if not url:
        print("[X] URL e zbrazet.")
        return
    produkti = input("Emri i produktit: ").strip()
    programi = input("Programi affiliate (clickbank, shareasale, amazon, impact, digistore24): ").strip().lower()

    if programi not in AFFILIATE_PROGRAMS:
        print(f"[X] Programi '{programi}' nuk njihet.")
        return

    short_url, link_data = tracker.shto_link(url, produkti, programi)
    print(f"\n[OK] Linku u krijua!")
    print(f"   Link i shkurter: {short_url}")
    print(f"   Produkti: {produkti}")
    print(f"   Programi: {AFFILIATE_PROGRAMS[programi]['name']}")

def cmd_statistika():
    stats = tracker.statistikat()
    print("\n=== STATISTIKAT E TRACKING-UT ===\n")
    print(f"  * Linke totale: {stats['total_links']}")
    print(f"  * Klikime totale: {stats['total_clicks']}")
    print(f"  * Konvertime totale: {stats['total_conversions']}")
    print(f"  * Conversion rate: {stats['conversion_rate_pct']}%")
    print()
    if stats["links"]:
        print("  Linket:")
        for l in stats["links"]:
            print(f"    - {l['produkti']}: {l['clicks']} klikime, {l['conversions']} konvertime")

def cmd_raport():
    print("\n=== RAPORTI MUJOR ===\n")
    tani = datetime.now()
    muaji = int(input(f"Muaji (1-12) [{tani.month}]: ").strip() or tani.month)
    viti = int(input(f"Viti [{tani.year}]: ").strip() or tani.year)

    html_path = gjenero_html_raport(muaji, viti)
    print(f"\n[OK] Raporti u gjenerua!")
    print(f"   HTML: {html_path}")
    print(f"   JSON: output/reports/raport_{muaji:02d}_{viti}.json")

def cmd_seo():
    artikujt = lexo_artikujt()
    if not artikujt["articles"]:
        print("[X] Nuk ka artikuj. Gjenero fillimisht me 'gjenero'.")
        return

    print("\n=== ANALIZA SEO ===\n")
    for i, a in enumerate(artikujt["articles"], 1):
        print(f"  {i}. {a['titull']}")

    zgjedhja = int(input("\nZgjidh artikullin (numri): ").strip())
    if zgjedhja < 1 or zgjedhja > len(artikujt["articles"]):
        print("[X] Zgjedhje e pavlefshme.")
        return

    artikull_data = artikujt["articles"][zgjedhja - 1]
    filepath = os.path.join(os.path.dirname(__file__), "output", "articles", artikull_data["file"])
    if not os.path.exists(filepath):
        print("[X] Skedari nuk u gjet.")
        return

    with open(filepath, "r", encoding="utf-8") as f:
        body = f.read()

    keyword = input("Fjala kyç per SEO: ").strip() or settings["niche"]
    seo_result = kontrollo_seo(body, keyword, artikull_data["titull"])
    keywords = gjenero_keywords(body)
    density = llogarit_dendesine_fjalesh(body)

    print(f"\n  * SEO Score: {seo_result['score']}/100")
    print(f"  * Fjalet totale: {seo_result.get('word_count', 0)}")

    if seo_result["issues"]:
        print(f"\n  [X] Probleme ({len(seo_result['issues'])}):")
        for issue in seo_result["issues"]:
            print(f"     - {issue}")
    if seo_result["suggestions"]:
        print(f"\n  [i] Sugjerime ({len(seo_result['suggestions'])}):")
        for sug in seo_result["suggestions"]:
            print(f"     - {sug}")

    print(f"\n  * Top keywords:")
    for k in keywords[:10]:
        print(f"     {k['keyword']}: {k['score']}%")

def cmd_liste():
    artikujt = lexo_artikujt()
    print(f"\n=== ARTIKUJT E GJENERUAR ===\n")
    if not artikujt["articles"]:
        print("  Nuk ka artikuj. Perdor 'gjenero' per te krijuar.")
        return
    print(f"  Niche: {artikujt.get('niche', '?')}")
    print(f"  Total: {len(artikujt['articles'])} artikuj\n")
    for i, a in enumerate(artikujt["articles"], 1):
        print(f"  {i}. [{a['type'].upper()}] {a['titull']}")
        print(f"     [file] {a['file']}")

def cmd_niches():
    print("\n=== NICHES NE DISPOZICION ===\n")
    for key, name in NICHES.items():
        print(f"  {key} - {name}")
    print()

def cmd_help():
    print(f"""
+============================================+
| AI AFFILIATE BOT v{VERSION}                   |
| Sistemi i plote per affiliate              |
+============================================+

KOMANDAT:
--------------------------------------------

  konfiguro        - Konfiguro sistemin (PayPal, niche)
  gjenero          - Gjenero artikuj + postime sociale
  programet        - Shfaq programet affiliate
  linke            - Shto linke affiliate
  statistika       - Shfaq statistikat e tracking-ut
  raport           - Gjenero raport mujor
  seo              - Analize SEO per nje artikull
  liste            - Listo artikujt e gjeneruar
  niches           - Shfaq niches ne dispozicion
  help             - Shfaq kete ndihme
  exit             - Dil nga programi

SHEMBUJ:
--------------------------------------------
  1. python main.py konfiguro
  2. python main.py gjenero
  3. python main.py raport
""")

def main():
    if len(sys.argv) > 1:
        cmd = sys.argv[1].lower()
        cmds = {
            "konfiguro": cmd_konfiguro,
            "gjenero": cmd_gjenero,
            "programet": cmd_programet,
            "linke": cmd_linke,
            "statistika": cmd_statistika,
            "raport": cmd_raport,
            "seo": cmd_seo,
            "liste": cmd_liste,
            "niches": cmd_niches,
            "help": cmd_help,
        }
        if cmd in cmds:
            cmds[cmd]()
        else:
            print(f"[X] Komande e panjohur: {cmd}")
            cmd_help()
    else:
        cmd_help()
        print("\nPerdor: python main.py <komanda>\n")
        print("Shembuj:")
        print("  python main.py konfiguro")
        print("  python main.py gjenero")
        print("  python main.py raport")

if __name__ == "__main__":
    main()
