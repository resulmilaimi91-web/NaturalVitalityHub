import os, json
from datetime import datetime
from tracking import tracker
from content_generator import lexo_artikujt

REPORTS_DIR = os.path.join(os.path.dirname(__file__), "output", "reports")

def gjenero_raport_mujor(muaji=None, viti=None):
    os.makedirs(REPORTS_DIR, exist_ok=True)
    tani = datetime.now()
    muaji = muaji or tani.month
    viti = viti or tani.year

    tracking_stats = tracker.statistikat()
    monthly = tracker.raport_mujor(muaji, viti)
    artikujt = lexo_artikujt()

    raport = {
        "raport_per": f"{muaji:02d}-{viti}",
        "data_gjenerimit": tani.isoformat(),
        "tracking": monthly,
        "total_tracking": tracking_stats,
        "permbledhje_artikujsh": artikujt,
        "sugjerime": []
    }

    if tracking_stats["total_links"] == 0:
        raport["sugjerime"].append("Nuk ka asnjë link të gjeneruar. Shto produkte dhe gjenero linke affiliate.")
    if monthly["fitime_total"] == 0:
        raport["sugjerime"].append("Nuk ka fitime këtë muaj. Rishiko strategjinë e promovimit.")
    elif monthly["fitime_total"] < 100:
        raport["sugjerime"].append("Fitimet janë të ulëta. Provo të publikosh më shumë content ose të provosh produkte me komision më të lartë.")

    # Vlerësimi i performancës
    if monthly["konvertim_rate"] > 3:
        raport["vleresimi"] = "Shkëlqyer"
    elif monthly["konvertim_rate"] > 1.5:
        raport["vleresimi"] = "Mirë"
    elif monthly["konvertim_rate"] > 0.5:
        raport["vleresimi"] = "Mesatar"
    else:
        raport["vleresimi"] = "Në fillim"

    filename = f"raport_{muaji:02d}_{viti}.json"
    filepath = os.path.join(REPORTS_DIR, filename)
    with open(filepath, "w", encoding="utf-8") as f:
        json.dump(raport, f, indent=2, ensure_ascii=False)

    return raport

def gjenero_html_raport(muaji=None, viti=None):
    raport = gjenero_raport_mujor(muaji, viti)
    tani = datetime.now()

    html = f"""<!DOCTYPE html>
<html lang="sq">
<head>
    <meta charset="UTF-8">
    <title>Raport Affiliate - {raport['raport_per']}</title>
    <style>
        body {{ font-family: -apple-system, system-ui, sans-serif; max-width: 800px; margin: auto; padding: 20px; background: #f5f5f5; }}
        .card {{ background: white; padding: 20px; margin: 15px 0; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }}
        h1, h2, h3 {{ color: #333; }}
        .stats {{ display: flex; gap: 15px; flex-wrap: wrap; }}
        .stat {{ background: #007bff; color: white; padding: 15px; border-radius: 8px; flex: 1; min-width: 120px; text-align: center; }}
        .stat.green {{ background: #28a745; }}
        .stat.orange {{ background: #ffc107; color: #333; }}
        .stat.red {{ background: #dc3545; }}
        .suggestion {{ background: #fff3cd; padding: 10px; margin: 5px 0; border-radius: 4px; border-left: 4px solid #ffc107; }}
        .table {{ width: 100%; border-collapse: collapse; }}
        .table th, .table td {{ padding: 8px; text-align: left; border-bottom: 1px solid #ddd; }}
    </style>
</head>
<body>
    <div class="card">
        <h1>📊 Raporti Mujor Affiliate</h1>
        <p><strong>Periudha:</strong> {raport['raport_per']}</p>
        <p><strong>Data:</strong> {tani.strftime('%d/%m/%Y')}</p>
        <p><strong>Vlerësimi:</strong> {raport['vleresimi']}</p>
    </div>

    <div class="card">
        <h2>Statistikat Kryesore</h2>
        <div class="stats">
            <div class="stat"><h3>{raport['tracking']['klikime']}</h3><p>Klikime</p></div>
            <div class="stat green"><h3>{raport['tracking']['konvertime']}</h3><p>Konvertime</p></div>
            <div class="stat orange"><h3>{raport['tracking']['konvertim_rate']}%</h3><p>Conversion Rate</p></div>
            <div class="stat green"><h3>${raport['tracking']['fitime_total']}</h3><p>Fitime</p></div>
        </div>
    </div>

    <div class="card">
        <h2>Statistikat Totale</h2>
        <div class="stats">
            <div class="stat"><h3>{raport['total_tracking']['total_links']}</h3><p>Linke Totale</p></div>
            <div class="stat"><h3>{raport['total_tracking']['total_clicks']}</h3><p>Klikime Totale</p></div>
            <div class="stat green"><h3>{raport['total_tracking']['total_conversions']}</h3><p>Konvertime Totale</p></div>
        </div>
    </div>

    <div class="card">
        <h2>Sugjerime</h2>
        {''.join(f'<div class="suggestion">{s}</div>' for s in raport['sugjerime'])}
    </div>

    <div class="card">
        <h2>Artikujt e Gjeneruar</h2>
        <table class="table">
            <tr><th>Titulli</th><th>Tipi</th></tr>
            {''.join(f'<tr><td>{a["titull"]}</td><td>{a["type"]}</td></tr>' for a in raport['permbledhje_artikujsh'].get('articles', []))}
        </table>
    </div>
</body>
</html>"""

    filename = f"raport_{raport['raport_per']}.html"
    filepath = os.path.join(REPORTS_DIR, filename)
    with open(filepath, "w", encoding="utf-8") as f:
        f.write(html)

    return filepath
