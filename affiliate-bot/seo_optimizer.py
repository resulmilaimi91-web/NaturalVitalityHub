import re, math
from collections import Counter

STOP_WORDS_SQ = {
    "dhe", "ose", "por", "sepse", "ndaj", "me", "nga", "pa", "tek", "në",
    "për", "të", "së", "i", "e", "a", "që", "si", "kur", "ku", "ka",
    "janë", "ishte", "do", "mund", "duhet", "kam", "ka", "kemi", "keni",
    "kanë", "kjo", "ai", "ajo", "ata", "ato", "im", "yt", "tij", "saj",
    "tyre", "më", "të", "pas", "para", "gjatë", "përmes", "deri", "nën",
    "mbi", "midis", "ndërmjet", "disa", "shumë", "pak", "mjaft", "vetëm",
    "edhe", "gjithashtu", "madje", "pothuajse", "këtu", "atje", "aty",
    "kështu", "ashtu", "po", "jo", "mos", "nuk", "s'", "as", "asnjë",
    "gjithë", "çdo", "cilido", "çfarëdo", "kushdo"
}

def llogarit_dendesine_fjalesh(teksti):
    fjalet = re.findall(r'\b\w+\b', teksti.lower())
    total = len(fjalet)
    fjalet_filtruar = [f for f in fjalet if f not in STOP_WORDS_SQ and len(f) > 2]
    counter = Counter(fjalet_filtruar)

    dendesia = {}
    for fjala, count in counter.most_common(20):
        dendesia[fjala] = {
            "count": count,
            "density_pct": round((count / total) * 100, 2) if total > 0 else 0
        }
    return {"total_words": total, "density": dendesia}

def optimize_titull(titulli, keyword):
    if keyword.lower() not in titulli.lower():
        return f"{keyword} - {titulli}" if len(titulli) < 50 else titulli
    return titulli

def optimize_meta_description(teksti, keyword, max_len=160):
    fjalia_par = teksti.split('\n')[0] if '\n' in teksti else teksti[:max_len]
    if keyword.lower() not in fjalia_par.lower():
        return f"{keyword}. {fjalia_par}"[:max_len]
    return fjalia_par[:max_len]

def gjenero_keywords(teksti, count=10):
    fjalet = re.findall(r'\b\w+\b', teksti.lower())
    fjalet_filtruar = [f for f in fjalet if f not in STOP_WORDS_SQ and len(f) > 2]
    counter = Counter(fjalet_filtruar)
    return [{"keyword": f, "score": round(c / len(fjalet_filtruar) * 100, 2)} for f, c in counter.most_common(count)]

def kontrollo_seo(artikull_body, keyword="", titull=""):
    rezultat = {"score": 0, "issues": [], "suggestions": []}
    body_lower = artikull_body.lower()
    total_words = len(re.findall(r'\b\w+\b', artikull_body))

    if total_words < 300:
        rezultat["issues"].append(f"Artikulli ka vetëm {total_words} fjalë. Rekomandohet minimum 600 fjalë.")
    elif total_words > 2000:
        rezultat["issues"].append(f"Artikulli ka {total_words} fjalë. Mund të shkurtohet për lexueshmëri më të mirë.")

    if keyword and keyword.lower() not in artikull_body.lower():
        rezultat["issues"].append(f"Fjala kyç '{keyword}' nuk u gjet në artikull.")

    if keyword and keyword.lower() not in titull.lower():
        rezultat["suggestions"].append(f"Shto fjalën kyç '{keyword}' në titull.")

    h1_count = len(re.findall(r'^# ', artikull_body, re.MULTILINE))
    if h1_count > 1:
        rezultat["issues"].append(f"Ka {h1_count} tituj H1. Duhet të ketë vetëm 1.")
    elif h1_count == 0:
        rezultat["issues"].append("Nuk ka titull H1.")

    h2_count = len(re.findall(r'^## ', artikull_body, re.MULTILINE))
    if h2_count < 2:
        rezultat["suggestions"].append("Shto më shumë nëntituj H2 për strukturë më të mirë.")

    paragraphs = [p for p in artikull_body.split('\n\n') if len(p.strip()) > 0]
    long_paras = [p for p in paragraphs if len(p.split()) > 100]
    if long_paras:
        rezultat["suggestions"].append(f"{len(long_paras)} paragrafë janë më të gjatë se 100 fjalë. Ndaji në paragrafë më të shkurtër.")

    issues_count = len(rezultat["issues"])
    suggestions_count = len(rezultat["suggestions"])
    base_score = 100 - (issues_count * 15) - (suggestions_count * 5)
    if total_words >= 600:
        base_score += 10
    if keyword and keyword.lower() in titull.lower():
        base_score += 10
    rezultat["score"] = max(0, min(100, base_score))
    rezultat["word_count"] = total_words

    return rezultat
