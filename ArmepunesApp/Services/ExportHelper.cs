using System.Data;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Drawing.Text;
using System.IO;
using ArmepunesApp.Data;

namespace ArmepunesApp.Services;

public static class ExportHelper
{
    public static string GetExportDir()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ArmepunesApp", "Exports");
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        return dir;
    }

    public static string EksportoHistorikunArmes(string serial, DatabaseHelper db)
    {
        var exportDir = GetExportDir();
        var dir = Path.Combine(exportDir, "Historiku_Armeve");
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        var fileName = $"Historiku_{serial}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
        var filePath = Path.Combine(dir, fileName);

        var trans = db.MerrTransaksionet();
        var rows = trans.AsEnumerable()
            .Where(r => (r["ArmaSerial"]?.ToString() ?? "") == serial)
            .OrderBy(r => r["DataOra"]?.ToString())
            .ToList();

        var armet = db.MerrArmet();
        var armaRow = armet.AsEnumerable().FirstOrDefault(r => (r["NumerSerial"]?.ToString() ?? "") == serial);

        var actualPath = PrintToPdf(filePath, (g, page, m) =>
        {
            using var r = new A4DocumentRenderer(g, page, m);
            r.DrawHeader("HISTORIKU I ARMES");

            if (armaRow != null)
            {
                var info = $"Seriali: {armaRow["NumerSerial"]} | Marka: {armaRow["Marka"]} | Modeli: {armaRow["Modeli"]} | Kalibri: {armaRow["Kalibri"]} | Lloji: {armaRow["Lloji"]}";
                r.G.DrawString(info, r.Small, Brushes.Black, r.X0, r.Y);
                r.Y += 20;
                if (armaRow["NrInventari"] != null && !string.IsNullOrEmpty(armaRow["NrInventari"]?.ToString()))
                {
                    r.G.DrawString($"Nr. Inventari: {armaRow["NrInventari"]}", r.Small, Brushes.Black, r.X0, r.Y);
                    r.Y += 20;
                }
            }
            r.Y += 10;

            var headers = new[] { "Nr.", "Data/Ora", "Tipi", "Klienti", "Personeli", "Qellimi" };
            float[] colW = { 28, 85, 65, 100, 100, 0 };
            float remainingW = r.W - 28 - 85 - 65 - 100 - 100;
            colW[colW.Length - 1] = remainingW;

            r.DrawTable(headers, colW, rows.Count, (i, cx) =>
            {
                var dataRow = rows[i];
                var doStr = dataRow["DataOra"]?.ToString() ?? "";
                if (doStr.Length >= 16) doStr = doStr.Replace("-", ".").Substring(0, 16);

                r.G.DrawString((i + 1).ToString() + ".", r.Normal, Brushes.Black, cx[0] + 2, r.Y + 1);
                r.G.DrawString(doStr, r.Normal, Brushes.Black, cx[1] + 2, r.Y + 1);
                r.G.DrawString(dataRow["Tipi"]?.ToString() ?? "", r.Normal, Brushes.Black, cx[2] + 2, r.Y + 1);
                r.G.DrawString(dataRow["KlientiEmri"]?.ToString() ?? "-", r.Normal, Brushes.Black, cx[3] + 2, r.Y + 1);
                r.G.DrawString(dataRow["PersoneliEmri"]?.ToString() ?? "-", r.Normal, Brushes.Black, cx[4] + 2, r.Y + 1);
                r.G.DrawString(dataRow["Qellimi"]?.ToString() ?? "-", r.Normal, Brushes.Black, cx[5] + 2, r.Y + 1);
            });

            r.Y += 20;
            r.G.DrawLine(r.LightPen, r.X0, r.Y, r.X0 + r.W, r.Y);
            r.Y += 16;
            r.G.DrawString($"Total transaksione: {rows.Count} | Gjeneruar me: {DateTime.Now:dd.MM.yyyy HH:mm:ss}", r.Footer, Brushes.Gray, r.X0, r.Y);
            r.Y += 16;
            r.G.DrawString("POLIGONI DRENI - Sistemi i Deponimit te Armeve", r.Footer, Brushes.Gray, r.X0, r.Y);
        });

        return string.IsNullOrEmpty(actualPath) ? filePath : actualPath;
    }

    public static string EksportoRaportinKlientit(string klienti, DatabaseHelper db)
    {
        var exportDir = GetExportDir();
        var emriFile = klienti.Replace(" ", "_").Replace("/", "_");
        var dir = Path.Combine(exportDir, "Raporte_Kliente");
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        var fileName = $"Raport_{emriFile}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
        var filePath = Path.Combine(dir, fileName);

        var trans = db.MerrTransaksionet();
        var rows = trans.AsEnumerable()
            .Where(r => (r["KlientiEmri"]?.ToString() ?? "").IndexOf(klienti, StringComparison.OrdinalIgnoreCase) >= 0)
            .OrderByDescending(r => r["DataOra"]?.ToString())
            .ToList();

        string[] headers;
        float[] colW;

        bool kaAksesore = rows.Any(r =>
        {
            var tid = r["Id"];
            if (tid == null || tid == DBNull.Value) return false;
            var aksDt = db.MerrAksesoretByTransaksionId(Convert.ToInt32(tid));
            return aksDt.Rows.Count > 0;
        });

        if (kaAksesore)
        {
            headers = new[] { "Nr.", "Data/Ora", "Tipi", "Seriali", "Personeli", "Qellimi", "Aksesoret" };
            colW = new float[] { 24, 80, 55, 80, 80, 85, 0 };
        }
        else
        {
            headers = new[] { "Nr.", "Data/Ora", "Tipi", "Seriali", "Personeli", "Qellimi" };
            colW = new float[] { 28, 85, 60, 90, 90, 0 };
        }

        PrintToPdf(filePath, (g, page, m) =>
        {
            using var r = new A4DocumentRenderer(g, page, m);
            float remainingW = r.W - colW.Take(colW.Length - 1).Sum() - 4;
            colW[^1] = remainingW;

            r.DrawHeader("RAPORT KLIENTI");
            r.G.DrawString($"Klienti: {klienti}", r.Sub, Brushes.DarkSlateGray, r.X0, r.Y);
            r.Y += 20;
            r.G.DrawString($"Total transaksione: {rows.Count} | Gjeneruar: {DateTime.Now:dd.MM.yyyy HH:mm:ss}", r.Small, Brushes.Gray, r.X0, r.Y);
            r.Y += 26;

            r.DrawTable(headers, colW, rows.Count, (i, cx) =>
            {
                var dataRow = rows[i];
                var doStr = dataRow["DataOra"]?.ToString() ?? "";
                if (doStr.Length >= 16) doStr = doStr.Replace("-", ".").Substring(0, 16);

                int ci = 0;
                r.G.DrawString((i + 1).ToString() + ".", r.Normal, Brushes.Black, cx[ci++] + 2, r.Y + 1);
                r.G.DrawString(doStr, r.Normal, Brushes.Black, cx[ci++] + 2, r.Y + 1);
                r.G.DrawString(dataRow["Tipi"]?.ToString() ?? "", r.Normal, Brushes.Black, cx[ci++] + 2, r.Y + 1);
                r.G.DrawString(dataRow["ArmaSerial"]?.ToString() ?? "-", r.Normal, Brushes.Black, cx[ci++] + 2, r.Y + 1);
                r.G.DrawString(dataRow["PersoneliEmri"]?.ToString() ?? "-", r.Normal, Brushes.Black, cx[ci++] + 2, r.Y + 1);
                r.G.DrawString(dataRow["Qellimi"]?.ToString() ?? "-", r.Normal, Brushes.Black, cx[ci++] + 2, r.Y + 1);

                if (kaAksesore && ci < cx.Length)
                {
                    var tid = dataRow["Id"];
                    string aksStr = "-";
                    if (tid != null && tid != DBNull.Value)
                    {
                        var aksDt = db.MerrAksesoretByTransaksionId(Convert.ToInt32(tid));
                        if (aksDt.Rows.Count > 0)
                            aksStr = string.Join(", ", aksDt.AsEnumerable().Select(ak => ak["Emri"]?.ToString()));
                    }
                    r.G.DrawString(aksStr, r.Normal, Brushes.Black, cx[ci] + 2, r.Y + 1);
                }
            });

            r.Y += 20;
            r.G.DrawLine(r.LightPen, r.X0, r.Y, r.X0 + r.W, r.Y);
            r.Y += 14;
            r.G.DrawString($"Gjeneruar nga Sistemi Deponim i Armeve - {DateTime.Now:dd.MM.yyyy HH:mm:ss}", r.Footer, Brushes.Gray, r.X0, r.Y);
        });

        return filePath;
    }

    public static string EksportoSertifikatinAkreditimitHacettepe(string marraSertifikatin, string numriSertifikatit, string data)
    {
        var exportDir = GetExportDir();
        var dir = Path.Combine(exportDir, "Sertifikate_Akreditim");
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        var fileName = $"Sertifikat_Akreditim_Hacettepe_{numriSertifikatit.Replace("/", "_")}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
        var filePath = Path.Combine(dir, fileName);

        PrintToPdf(filePath, (g, page, m) =>
        {
            using var r = new A4DocumentRenderer(g, page, m);
            r.Y = m + 40;
            r.LH = 30;

            using var titleBrush = new SolidBrush(Color.FromArgb(0, 50, 100));
            using var titleFont = new Font("Segoe UI", 24, FontStyle.Bold);
            r.G.DrawString("POLIGONI DRENI", titleFont, titleBrush, r.X0, r.Y);
            r.Y += 40;
            using var subFont = new Font("Segoe UI", 18, FontStyle.Bold);
            r.G.DrawString("SERTIFIKAT AKREDITIMI", subFont, Brushes.Black, r.X0, r.Y);
            r.Y += 50;

            r.G.DrawString("Në bazë të procedurave të akreditimit,", r.Normal, Brushes.Black, r.X0, r.Y);
            r.Y += r.LH;
            r.G.DrawString($"{marraSertifikatin}", r.Normal, Brushes.Black, r.X0, r.Y);
            r.Y += r.LH;
            r.G.DrawString("ka marrë sertifikatën e akreditimit", r.Normal, Brushes.Black, r.X0, r.Y);
            r.Y += r.LH;
            r.G.DrawString($"me numër: {numriSertifikatit}", r.Normal, Brushes.Black, r.X0, r.Y);
            r.Y += r.LH;
            r.G.DrawString($"me datë: {data}", r.Normal, Brushes.Black, r.X0, r.Y);
            r.Y += r.LH * 2;

            r.G.DrawString("Ky sertifikat konfirmon se personi/entiteti plotëson standardet e akreditimit.", r.Small, Brushes.Gray, r.X0, r.Y);
            r.Y += 20;
            r.G.DrawString("POLIGONI DRENI - Sistemi i Deponimit dhe Menaxhimit të Armëve", r.Small, Brushes.Gray, r.X0, r.Y);
            r.Y += 40;

            r.G.DrawString("_________________________", r.Normal, Brushes.Black, r.X0, r.Y);
            r.G.DrawString("Përgjegjës i Seksionit të Akreditimit", r.Small, Brushes.Black, r.X0, r.Y + 20);

            r.G.DrawString("_________________________", r.Normal, Brushes.Black, r.X0 + 300, r.Y);
            r.G.DrawString("Sekretar", r.Small, Brushes.Black, r.X0 + 300, r.Y + 20);
        });

        return filePath;
    }

    public static string EksportoFleteleshimAuto(DatabaseHelper db, int transaksioniId, string perdoruesi)
    {
        var trans = db.MerrTransaksionById(transaksioniId);
        if (trans == null) return "";

        var viti = DateTime.Now.Year;
        var exportDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ArmepunesApp", "Fleteleshimat", viti.ToString());
        if (!Directory.Exists(exportDir)) Directory.CreateDirectory(exportDir);

        var serial = trans["ArmaSerial"]?.ToString() ?? "XX";
        var tipi = trans["Tipi"]?.ToString() == "Hyrje" ? "HYRJE" : "DALJE";
        var fileName = $"FL_{tipi}_{viti}_{transaksioniId:D6}.pdf";
        var filePath = Path.Combine(exportDir, fileName);

        var arma = db.MerrArmeBySerial(serial);
        var aksesoret = db.MerrAksesoretByTransaksionId(transaksioniId);
        var municionet = db.MerrMunicionetByTransaksionId(transaksioniId);

        var actualPath = PrintToPdf(filePath, (g, page, m) =>
        {
            using var r = new A4DocumentRenderer(g, page, m);
            r.LH = 20;

            var dataOra = trans["DataOra"]?.ToString() ?? "";
            if (dataOra.Length >= 16) dataOra = dataOra.Replace("-", ".");

            var klientiEmri = trans["KlientiEmri"]?.ToString() ?? "";
            var personeliEmri = trans["PersoneliEmri"]?.ToString() ?? "";
            var qellimi = trans["Qellimi"]?.ToString() ?? "-";
            var shenime = trans["Shenime"]?.ToString() ?? "-";
            var personeliQeDorzoi = trans["PersoneliQeDorzoi"]?.ToString() ?? "";
            var personeliQeMorri = trans["PersoneliQeMorri"]?.ToString() ?? "";

            r.DrawHeader($"FLETELESHIM Nr. {viti}-{transaksioniId:D6}");
            r.G.DrawString($"Data/Ora: {dataOra}", r.Normal, Brushes.Black, r.X0, r.Y);
            r.G.DrawString($"Tipi: {tipi}", r.Normal, Brushes.Black, r.X0 + r.W - 150, r.Y);
            r.Y += 24;

            float half = (r.W - 20) / 2;
            float c2 = r.X0 + half + 20;

            r.DrawSection("1. TE DHENAT E ARMES");
            DrawField(r, "Lloji:", arma?.Lloji ?? "-", r.X0, c2);
            DrawField(r, "Kalibri:", arma?.Kalibri ?? "-", r.X0, c2);
            DrawField(r, "Marka:", arma?.Marka ?? "-", r.X0, c2);
            DrawField(r, "Nr. Serial:", serial, r.X0, c2);
            DrawField(r, "Modeli:", arma?.Modeli ?? "-", r.X0, c2);
            DrawField(r, "Nr. Inventari:", arma?.NrInventari ?? "-", r.X0, c2);
            r.Y += 4;

            r.DrawSection("2. ZYRTARI PRANUES");
            DrawField(r, "Emri:", personeliEmri, r.X0, c2);
            r.Y += 4;

            r.DrawSection("3. KLIENTI");
            DrawField(r, "Emri:", klientiEmri, r.X0, c2);
            r.Y += 4;

            r.DrawSection($"4. QELLIMI I {(tipi == "HYRJE" ? "DEPONIMIT" : "TERHEQJES")}");
            r.G.DrawString(qellimi, r.Normal, Brushes.Black, r.X0 + 4, r.Y);
            r.Y += r.LH + 4;

            r.DrawSection("5. PRANIM / DORZIM");
            DrawField(r, "Dorzoi:", string.IsNullOrEmpty(personeliQeDorzoi) ? "-" : personeliQeDorzoi, r.X0, c2);
            DrawField(r, "Morri:", string.IsNullOrEmpty(personeliQeMorri) ? "-" : personeliQeMorri, r.X0, c2);

            if (aksesoret.Rows.Count > 0)
            {
                r.Y += 4;
                r.DrawSection("6. AKSESORET");
                foreach (System.Data.DataRow a in aksesoret.Rows)
                {
                    var emri = a["Emri"]?.ToString() ?? "";
                    var sasia = a["Sasia"]?.ToString() ?? "1";
                    r.G.DrawString($"- {emri} (x{sasia})", r.Normal, Brushes.Black, r.X0 + 4, r.Y);
                    r.Y += r.LH;
                }
            }

            if (municionet.Rows.Count > 0)
            {
                r.Y += 4;
                r.DrawSection("7. MUNICIONI");
                foreach (System.Data.DataRow mRow in municionet.Rows)
                {
                    var emri = mRow["Emri"]?.ToString() ?? "";
                    var sasia = mRow["Sasia"]?.ToString() ?? "1";
                    var kalibri = mRow["Kalibri"]?.ToString() ?? "";
                    var njesia = mRow["Njesia"]?.ToString() ?? "copë";
                    var line = $"- {emri}" + (!string.IsNullOrEmpty(kalibri) ? $" ({kalibri})" : "") + $" x{sasia} {njesia}";
                    r.G.DrawString(line, r.Normal, Brushes.Black, r.X0 + 4, r.Y);
                    r.Y += r.LH;
                }
            }

            r.Y += 10;
            r.G.DrawLine(r.BorderPen, r.X0, r.Y, r.X0 + r.W, r.Y);
            r.Y += 8;
            r.G.DrawString($"Gjeneruar nga {perdoruesi} me {DateTime.Now:dd.MM.yyyy HH:mm:ss}", r.Footer, Brushes.Gray, r.X0, r.Y);
            r.Y += 16;
            r.G.DrawString("POLIGONI DRENI - Sistemi Deponim i Armeve", r.Footer, Brushes.Gray, r.X0, r.Y);
        });

        return string.IsNullOrEmpty(actualPath) ? filePath : actualPath;
    }

    private static void DrawField(A4DocumentRenderer r, string label, string value, float colX, float colX2)
    {
        r.G.DrawString(label, r.Label, Brushes.Black, colX, r.Y);
        r.G.DrawString(value, r.Normal, Brushes.Black, colX + 120, r.Y);
        r.Y += r.LH;
    }

    public static string PrintToPdf(string filePath, Action<Graphics, Rectangle, float> drawPage)
    {
        var dir = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

        string[] pdfPrinterCandidates = { "Microsoft Print to PDF", "Microsoft XPS Document Writer" };
        foreach (var candidate in pdfPrinterCandidates)
        {
            if (!System.Drawing.Printing.PrinterSettings.InstalledPrinters.Cast<string>().Any(p => p == candidate))
                continue;
            try
            {
                using var pd = new PrintDocument();
                pd.PrinterSettings.PrinterName = candidate;
                var a4 = pd.PrinterSettings.PaperSizes.Cast<PaperSize>().FirstOrDefault(p => p.Kind == PaperKind.A4);
                if (a4 != null)
                    pd.DefaultPageSettings.PaperSize = a4;
                pd.DefaultPageSettings.Landscape = false;
                pd.DefaultPageSettings.Margins = new Margins(10, 10, 10, 10);
                pd.PrinterSettings.PrintToFile = true;
                pd.PrinterSettings.PrintFileName = filePath;
                pd.PrintController = new StandardPrintController();
                pd.PrintPage += (s, e) =>
                {
                    drawPage(e.Graphics!, e.PageBounds, 15);
                    e.HasMorePages = false;
                };
                pd.Print();
                if (File.Exists(filePath) && new FileInfo(filePath).Length > 0)
                    return filePath;
            }
            catch { }
        }

        var pngPath = Path.ChangeExtension(filePath, ".png");
        try
        {
            const float dpi = 300f;
            float scale = dpi / 100f;
            int pw = (int)(8.27f * dpi);
            int ph = (int)(11.69f * dpi);

            using var bmp = new Bitmap(pw, ph);
            bmp.SetResolution(dpi, dpi);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.TextRenderingHint = TextRenderingHint.AntiAlias;
                g.Clear(Color.White);
                g.ScaleTransform(scale, scale);
                drawPage(g, new Rectangle(0, 0, 827, 1169), 15);
            }
            bmp.Save(pngPath, ImageFormat.Png);
            return pngPath;
        }
        catch { return ""; }
    }
}
