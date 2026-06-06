using System.Data;
using System.Drawing.Printing;
using System.Text.Json;
using ArmepunesApp.Data;
using ArmepunesApp.Services;

namespace ArmepunesApp.Forms;

public partial class FleteleshimForm : Form
{
    private readonly DatabaseHelper _db;
    private readonly List<DataRow> _transaksionet = new();
    private readonly string _mode;
    private readonly List<PrintDoc> _printDocs = new();
    private string _templateJson = "";

    private class PrintDoc
    {
        public string? Kopja { get; set; }
        public int NrRendor { get; set; }
        public string? NrDok { get; set; }
        public string? Tipi { get; set; }
        public string? Data { get; set; }
        public string? Ora { get; set; }
        public string? Qellimi { get; set; }
        public string? Shenime { get; set; }
        public string? PersoneliQeDorzoi { get; set; }
        public string? PersoneliQeMorri { get; set; }
        public string? Municioni { get; set; }

        public string? ArmaLloji { get; set; }
        public string? ArmaMarka { get; set; }
        public string? ArmaModeli { get; set; }
        public string? ArmaKalibri { get; set; }
        public string? ArmaSerial { get; set; }
        public string? ArmaInventar { get; set; }
        public string? ArmaViti { get; set; }
        public string? ArmaStatusRi { get; set; }

        public string? PersoneliEmri { get; set; }
        public string? PersoneliGrada { get; set; }
        public string? PersoneliNjesia { get; set; }
        public string? PersoneliLegjitimi { get; set; }
        public string? PersoneliTel { get; set; }

        public string? KlientiEmri { get; set; }
        public string? KlientiNID { get; set; }
        public string? KlientiTel { get; set; }
        public string? KlientiAdresa { get; set; }
        public string? KlientiEmail { get; set; }

        public List<PrintAksesor> Aksesoret { get; set; } = new();
        public List<PrintMunicion> Municionet { get; set; } = new();
    }

    private class PrintMunicion
    {
        public int Nr { get; set; }
        public string? Emri { get; set; }
        public string? Lloji { get; set; }
        public string? Kalibri { get; set; }
        public int Sasia { get; set; }
        public string? Njesia { get; set; }
        public string? Shenime { get; set; }
    }

    private class PrintAksesor
    {
        public int Nr { get; set; }
        public string? Emri { get; set; }
        public int Sasia { get; set; }
        public string? Shenime { get; set; }
    }

    private class PrintArmepunues
    {
        public int Nr { get; set; }
        public string? Seriali { get; set; }
        public string? MarkaModel { get; set; }
        public string? Lloji { get; set; }
        public string? Kalibri { get; set; }
        public string? Inventar { get; set; }
        public string? Deponuesi { get; set; }
        public string? DataDeponimit { get; set; }
    }

    public FleteleshimForm(DatabaseHelper db, DataRow? transaksioni, string mode) : this(db, transaksioni != null ? new List<DataRow> { transaksioni } : new List<DataRow>(), mode) { }

    public FleteleshimForm(DatabaseHelper db, List<DataRow> transaksionet, string mode)
    {
        _db = db;
        _transaksionet = transaksionet ?? new List<DataRow>();
        _mode = mode;
        InitializeComponent();
        NgarkoPrintera();
        try { PrepareDocuments(); }
        catch (Exception ex)
        {
            txtPreview.Text = $"Gabim: {ex.Message}";
            ErrorHandlerService.HandleException(ex, "pergaditja e fleteleshimit", null, "print_prepare");
        }
    }

    private void NgarkoPrintera()
    {
        cmbPrinter.Items.Clear();
        foreach (string printer in PrinterSettings.InstalledPrinters)
            cmbPrinter.Items.Add(printer);
        if (cmbPrinter.Items.Count > 0)
        {
            var defaultPrinter = new PrinterSettings().PrinterName;
            for (int i = 0; i < cmbPrinter.Items.Count; i++)
            {
                if (cmbPrinter.Items[i]?.ToString() == defaultPrinter)
                { cmbPrinter.SelectedIndex = i; return; }
            }
            cmbPrinter.SelectedIndex = 0;
        }
    }

    private int _currentTransIndex;
    private DataRow? _currentRow => _currentTransIndex >= 0 && _currentTransIndex < _transaksionet.Count ? _transaksionet[_currentTransIndex] : null;

    private string? F(string emri)
    {
        try { return _currentRow?[emri]?.ToString(); } catch { return null; }
    }

    private DataRow? FindArme(string serial)
    {
        foreach (DataRow r in _db.MerrArmet().Rows)
            if ((r["NumerSerial"]?.ToString() ?? "") == serial) return r;
        return null;
    }

    private DataRow? FindPersonel(string emri)
    {
        foreach (DataRow r in _db.MerrPersonelin().Rows)
        {
            var full = $"{r["Emri"]} {r["Mbiemri"]}";
            if (full == emri) return r;
            if (r["Emri"]?.ToString()?.Equals(emri, StringComparison.OrdinalIgnoreCase) == true) return r;
        }
        return null;
    }

    private DataRow? FindKlient(string emri)
    {
        foreach (DataRow r in _db.MerrKlientet().Rows)
        {
            var full = $"{r["Emri"]} {r["Mbiemri"]}";
            if (full == emri) return r;
            if (r["Emri"]?.ToString()?.Equals(emri, StringComparison.OrdinalIgnoreCase) == true) return r;
        }
        return null;
    }

    private void PrepareDocuments()
    {
        _printDocs.Clear();

        if (_mode == "LISTE DEPONIMI")
        {
            txtPreview.Text = "LISTE DEPONIMI - Pergatit dokumentin per printim";
            return;
        }

        if (_transaksionet.Count == 0)
        {
            txtPreview.Text = "Nuk ka transaksione per printim.";
            return;
        }

        // Use first transaction to determine template
        _currentTransIndex = 0;
        var tipiStr = F("Tipi") ?? "";
        var templateLloji = tipiStr == "Hyrje" ? "Fletpranim" : "Fleteleshim";
        int nrKopje = 2;
        _templateJson = _db.MerrTemplateParametrat(_db.MerrTemplateAktivId(templateLloji));
        try
        {
            var json = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(_templateJson);
            if (json != null && json.TryGetValue("kopje", out var kopjeEl))
                nrKopje = kopjeEl.GetInt32();
        }
        catch { }
        var kopjet = nrKopje == 1 ? new[] { "KOPJE" } : nrKopje == 2 ? new[] { "ARKIVI", "KLIENTI" } : new[] { "ARKIVI", "KLIENTI", "DEPO" };

        foreach (var transRow in _transaksionet)
        {
            _currentTransIndex = _transaksionet.IndexOf(transRow);
            var transId = Convert.ToInt32(transRow["Id"]);

            foreach (var kopja in kopjet)
            {
                var d = new PrintDoc { Kopja = kopja, NrRendor = transId };

                var dataOra = (F("DataOra") ?? "").Split(' ');
                d.Data = dataOra.Length > 0 ? dataOra[0].Replace("-", ".") : "";
                d.Ora = dataOra.Length > 1 ? dataOra[1] : "";
                d.Tipi = F("Tipi") == "Hyrje" ? "DEPONIM (HYRJE)" : "TERHEQJE (DALJE)";
                var viti = DateTime.Now.Year;
                d.NrDok = $"{viti}-{F("Id")?.PadLeft(6, '0')}";
                d.Qellimi = F("Qellimi") ?? "";
                d.Shenime = F("Shenime") ?? "";
                d.PersoneliQeDorzoi = F("PersoneliQeDorzoi") ?? "";
                d.PersoneliQeMorri = F("PersoneliQeMorri") ?? "";
                d.Municioni = F("Municioni") ?? "";

                var serial = F("ArmaSerial") ?? "";
                var ar = FindArme(serial);
                if (ar != null)
                {
                    d.ArmaLloji = ar["Lloji"]?.ToString();
                    d.ArmaMarka = ar["Marka"]?.ToString();
                    d.ArmaModeli = ar["Modeli"]?.ToString();
                    d.ArmaKalibri = ar["Kalibri"]?.ToString();
                    d.ArmaSerial = serial;
                    d.ArmaInventar = ar["NrInventari"]?.ToString();
                    d.ArmaViti = ar.Table.Columns.Contains("VitiProdhimit") ? ar["VitiProdhimit"]?.ToString() : "";
                    d.ArmaStatusRi = F("Tipi") == "Dalje" ? "Tek Klienti" : "Ne Magazine";
                }

                var pemri = F("PersoneliEmri") ?? "";
                var pr = FindPersonel(pemri);
                if (pr != null)
                {
                    d.PersoneliEmri = $"{pr["Emri"]} {pr["Mbiemri"]}";
                    d.PersoneliGrada = pr["Grada"]?.ToString();
                    d.PersoneliNjesia = pr["Njesia"]?.ToString();
                    d.PersoneliLegjitimi = pr["NrLegjitimacioni"]?.ToString();
                    d.PersoneliTel = pr["Telefon"]?.ToString();
                }
                else { d.PersoneliEmri = pemri; }

                var kemri = F("KlientiEmri") ?? "";
                var kr = FindKlient(kemri);
                if (kr != null)
                {
                    d.KlientiEmri = $"{kr["Emri"]} {kr["Mbiemri"]}";
                    d.KlientiNID = kr["NrLeternjoftimit"]?.ToString();
                    d.KlientiTel = kr["Telefon"]?.ToString();
                    d.KlientiAdresa = kr["Adresa"]?.ToString();
                    d.KlientiEmail = kr["Email"]?.ToString();
                }
                else { d.KlientiEmri = kemri; }

                var akset = _db.MerrAksesoretByTransaksionId(transId);
                int n = 1;
                foreach (DataRow r in akset.Rows)
                {
                    d.Aksesoret.Add(new PrintAksesor
                    {
                        Nr = n++,
                        Emri = r["Emri"]?.ToString() ?? "",
                        Sasia = Convert.ToInt32(r["Sasia"] ?? 1),
                        Shenime = r.Table.Columns.Contains("Shenime") ? r["Shenime"]?.ToString() : ""
                    });
                }

                var municionet = _db.MerrMunicionetByTransaksionId(transId);
                int mn = 1;
                foreach (DataRow r in municionet.Rows)
                {
                    d.Municionet.Add(new PrintMunicion
                    {
                        Nr = mn++,
                        Emri = r["Emri"]?.ToString() ?? "",
                        Lloji = r["Lloji"]?.ToString(),
                        Kalibri = r["Kalibri"]?.ToString(),
                        Sasia = Convert.ToInt32(r["Sasia"] ?? 1),
                        Njesia = r["Njesia"]?.ToString() ?? "copë",
                        Shenime = r["Shenime"]?.ToString()
                    });
                }

                _printDocs.Add(d);
            }
        }

        txtPreview.Text = $"Fleteleshimi u pergatit per {_transaksionet.Count} transaksione. Kliko 'Parashiko' ose 'Printo'.";
    }

    private void btnPrinto_Click(object sender, EventArgs e)
    {
        try
        {
            _currentPrintIndex = 0;
            if (_mode == "LISTE DEPONIMI")
            {
                PrintoListenDeponimit();
                return;
            }
            var pd = KrijoPrintDocument(PrintoFleteleshimPage);
            pd.Print();
        }
        catch (Exception ex) { ErrorHandlerService.HandleException(ex, "printimi", null, "print_fleteleshim"); }
    }

    private void btnParashiko_Click(object sender, EventArgs e)
    {
        try
        {
            _currentPrintIndex = 0;
            var pd = _mode == "LISTE DEPONIMI"
                ? KrijoPrintDocument(PrintoListenDeponimitPage)
                : KrijoPrintDocument(PrintoFleteleshimPage);
            using var dlg = new PrintPreviewDialog
            {
                Document = pd,
                Width = 1000,
                Height = 750,
                Text = "Parashiko - Fleteleshim"
            };
            dlg.ShowDialog();
        }
        catch (Exception ex) { ErrorHandlerService.HandleException(ex, "parashikimi", null, "print_fleteleshim"); }
    }

    private PrintDocument KrijoPrintDocument(PrintPageEventHandler handler)
    {
        var pd = new PrintDocument();
        pd.PrintPage += handler;
        pd.DefaultPageSettings.Landscape = true;
        pd.DefaultPageSettings.PaperSize = new PaperSize("A4", A4DocumentRenderer.A4_W, A4DocumentRenderer.A4_H);
        pd.DefaultPageSettings.Margins = new Margins(20, 20, 20, 20);
        if (cmbPrinter.SelectedItem != null)
            pd.PrinterSettings.PrinterName = cmbPrinter.SelectedItem.ToString()!;
        return pd;
    }

    // ─────────────────────────────────────────────
    // LISTA E DEPONIMIT (Landscape A4)
    // ─────────────────────────────────────────────
    private void PrintoListenDeponimit()
    {
        var pd = KrijoPrintDocument(PrintoListenDeponimitPage);
        pd.Print();
    }

    private void PrintoListenDeponimitPage(object? sender, PrintPageEventArgs e)
    {
        using var r = new A4DocumentRenderer(e.Graphics!, e.PageBounds, 20, _templateJson) { LH = 22 };
        r.DrawHeader();

        var armet = _db.MerrArmetNeMagazine();
        float[] colW = { 28, 75, 110, 100, 65, 55, r.W - 28 - 75 - 110 - 100 - 65 - 55 - 68, 68 };
        var headers = new[] { "Nr.", "Seriali", "Marka", "Modeli", "Kalibri", "Inventar", "Deponuesi", "Data e Dep." };
        r.DrawTable(headers, colW, armet.Rows.Count, (i, cx) =>
        {
            var row = armet.Rows[i];
            var sn = row["NumerSerial"]?.ToString() ?? "";
            var marka = row["Marka"]?.ToString() ?? "";
            var model = row["Modeli"]?.ToString() ?? "";
            var kalib = row["Kalibri"]?.ToString() ?? "";
            var inv = row["NrInventari"]?.ToString() ?? "";

            string deponuesi = "-", dataDep = "-";
            var trans = _db.MerrTransaksionet();
            foreach (DataRow tr in trans.Rows)
            {
                if ((tr["ArmaSerial"]?.ToString() ?? "") == sn && (tr["Tipi"]?.ToString() ?? "") == "Hyrje")
                {
                    deponuesi = tr["KlientiEmri"]?.ToString() ?? "-";
                    var doStr = tr["DataOra"]?.ToString() ?? "";
                    if (doStr.Length >= 10)
                        dataDep = doStr.Substring(0, 10).Replace("-", ".");
                    break;
                }
            }

            e.Graphics!.DrawString($"{i + 1}.", r.Normal, Brushes.Black, cx[0] + 2, r.Y + 1);
            e.Graphics!.DrawString(sn, r.Normal, Brushes.Black, cx[1] + 2, r.Y + 1);
            e.Graphics!.DrawString(marka, r.Normal, Brushes.Black, cx[2] + 2, r.Y + 1);
            e.Graphics!.DrawString(model, r.Normal, Brushes.Black, cx[3] + 2, r.Y + 1);
            e.Graphics!.DrawString(kalib, r.Normal, Brushes.Black, cx[4] + 2, r.Y + 1);
            e.Graphics!.DrawString(inv, r.Normal, Brushes.Black, cx[5] + 2, r.Y + 1);
            e.Graphics!.DrawString(deponuesi, r.Normal, Brushes.Black, cx[6] + 2, r.Y + 1);
            e.Graphics!.DrawString(dataDep, r.Normal, Brushes.Black, cx[7] + 2, r.Y + 1);
        });

        armet.Dispose();

        r.Y += 18;
        var teGjitha = _db.MerrArmet();
        int neMag = 0, nePerd = 0;
        foreach (DataRow rw in teGjitha.Rows)
        {
            var s = rw["Statusi"]?.ToString();
            if (s == "Ne Magazine") neMag++;
            else if (s == "Tek Klienti") nePerd++;
        }
        teGjitha.Dispose();

        e.Graphics!.DrawString($"Gjithsej ne deponim: {armet.Rows.Count} arme", r.Sub, Brushes.Black, r.X0 + 2, r.Y);
        r.Y += 20;
        e.Graphics!.DrawString($"Ne Magazine: {neMag}    |    Ne Perdorim: {nePerd}", r.Normal, Brushes.DimGray, r.X0 + 2, r.Y);
        r.Y += 30;

        r.DrawSignatureBlock(
            new[] { r.TmpSig1.ToUpper(), r.TmpSig2.ToUpper(), r.TmpSig3.ToUpper() },
            new[] { "", "", "" },
            new[] { r.TmpSig1, r.TmpSig2, r.TmpSig3 });

        r.Y += 10;
        e.Graphics!.DrawString("Dokument i gjeneruar automatikisht nga Sistemi Deponim i Armeve", r.Footer, Brushes.Gray, r.X0 + 2, r.Y);
        r.Y += 12;
        e.Graphics!.DrawString($"Poligoni Dreni - {DateTime.Now:dd.MM.yyyy HH:mm:ss}", r.Footer, Brushes.Gray, r.X0 + 2, r.Y);

        e.HasMorePages = false;
    }

    // ─────────────────────────────────────────────
    // FLETELESHIMI (Landscape A4)
    // ─────────────────────────────────────────────
    private int _currentPrintIndex = 0;

    private void PrintoFleteleshimPage(object? sender, PrintPageEventArgs e)
    {
        int idx = _currentPrintIndex;
        if (idx >= _printDocs.Count)
        {
            e.HasMorePages = false;
            return;
        }

        var d = _printDocs[idx];
        using var r = new A4DocumentRenderer(e.Graphics!, e.PageBounds, 20, _templateJson);

        // ── HEADER ──
        r.DrawHeader();

        // ── DOCUMENT INFO BOX ──
        bool isHyrje = d.Tipi?.Contains("HYRJE") == true;
        var docTitle = isHyrje ? "FLETPRANIM" : "FLETELESHIM";
        r.DrawDocInfoBox(docTitle, d.NrDok ?? "", d.Data ?? "", d.Ora ?? "", d.Tipi ?? "");

        float c1 = r.X0 + 8;
        float c2 = r.X0 + (r.W - 24) / 2 + 20;

        // ── NR. RENDOR ──
        using var nrFont = new Font("Segoe UI", 13, FontStyle.Bold);
        using var nrBrush = new SolidBrush(Color.FromArgb(0, 70, 130));
        e.Graphics!.FillRectangle(nrBrush, r.X0, r.Y, r.W, 28);
        e.Graphics!.DrawString($"NR. RENDOR: {d.NrRendor:D5}", nrFont, Brushes.White, r.X0 + 12, r.Y + 4);
        r.Y += 32;

        // ── 1. WEAPON INFO ──
        r.DrawSection("  1.  TE DHENAT E ARMES");
        r.DrawFieldAt("Lloji:", d.ArmaLloji, c1, c2);
        e.Graphics!.DrawString("Kalibri:", r.Label, Brushes.Black, c2, r.Y);
        e.Graphics!.DrawString(d.ArmaKalibri ?? "-", r.Normal, Brushes.Black, c2 + 120, r.Y);
        r.Y += r.LH;
        r.DrawFieldAt("Marka:", d.ArmaMarka, c1, c2);
        e.Graphics!.DrawString("Nr. Serial:", r.Label, Brushes.Black, c2, r.Y);
        e.Graphics!.DrawString(d.ArmaSerial ?? "-", r.Normal, Brushes.Black, c2 + 120, r.Y);
        r.Y += r.LH;
        r.DrawFieldAt("Modeli:", d.ArmaModeli, c1, c2);
        e.Graphics!.DrawString("Nr. Inventari:", r.Label, Brushes.Black, c2, r.Y);
        e.Graphics!.DrawString(d.ArmaInventar ?? "-", r.Normal, Brushes.Black, c2 + 120, r.Y);
        r.Y += r.LH;
        r.DrawFieldAt("Viti Prodhimit:", d.ArmaViti, c1, c2);
        e.Graphics!.DrawString("Statusi i Ri:", r.Label, Brushes.Black, c2, r.Y);
        e.Graphics!.DrawString(d.ArmaStatusRi ?? "-", r.Normal, Brushes.Black, c2 + 120, r.Y);
        r.Y += r.LH + 4;

        // ── 2. PERSONNEL ──
        r.DrawSection("  2.  ZYRTARI PRANUES (PERSONELI I ARMEPUNES)");
        r.DrawFieldAt("Emri/Mbiemri:", d.PersoneliEmri, c1, c2);
        e.Graphics!.DrawString("Nr. Legjitimit:", r.Label, Brushes.Black, c2, r.Y);
        e.Graphics!.DrawString(d.PersoneliLegjitimi ?? "-", r.Normal, Brushes.Black, c2 + 120, r.Y);
        r.Y += r.LH;
        r.DrawFieldAt("Grada:", d.PersoneliGrada, c1, c2);
        e.Graphics!.DrawString("Telefon:", r.Label, Brushes.Black, c2, r.Y);
        e.Graphics!.DrawString(d.PersoneliTel ?? "-", r.Normal, Brushes.Black, c2 + 120, r.Y);
        r.Y += r.LH;
        r.DrawFieldAt("Njesia:", d.PersoneliNjesia, c1, c2);
        r.Y += r.LH + 4;

        // ── 3. CLIENT ──
        r.DrawSection("  3.  KLIENTI / PRONESI I ARMES");
        r.DrawFieldAt("Emri/Mbiemri:", d.KlientiEmri, c1, c2);
        e.Graphics!.DrawString("Telefon:", r.Label, Brushes.Black, c2, r.Y);
        e.Graphics!.DrawString(d.KlientiTel ?? "-", r.Normal, Brushes.Black, c2 + 120, r.Y);
        r.Y += r.LH;
        r.DrawFieldAt("NID:", d.KlientiNID, c1, c2);
        e.Graphics!.DrawString("Email:", r.Label, Brushes.Black, c2, r.Y);
        e.Graphics!.DrawString(d.KlientiEmail ?? "-", r.Normal, Brushes.Black, c2 + 120, r.Y);
        r.Y += r.LH;
        r.DrawFieldAt("Adresa:", d.KlientiAdresa, c1, c2);
        r.Y += r.LH + 4;

        // ── 4. PURPOSE ──
        r.DrawSection($"  4.  QELLIMI I {(isHyrje ? "DEPONIMIT" : "TERHEQJES")}");
        e.Graphics!.DrawString(d.Qellimi ?? "-", r.Normal, Brushes.Black, r.X0 + 10, r.Y);
        r.Y += r.LH + 6;

        // ── 5. ACCESSORIES ──
        r.DrawSection("  5.  AKSESORET");
        if (d.Aksesoret.Count == 0)
        {
            e.Graphics!.DrawString("Asnje", r.Small, Brushes.DimGray, r.X0 + 10, r.Y);
            r.Y += r.LH + 4;
        }
        else
        {
            float acw0 = 28f;
            float acw1 = (r.W - 40) * 0.40f;
            float acw2 = 40f;
            float acw3 = (r.W - 40) - acw0 - acw1 - acw2;
            float[] acw = { acw0, acw1, acw2, acw3 };
            float[] acx = new float[acw.Length];
            acx[0] = r.X0 + 8;
            for (int i = 1; i < acw.Length; i++)
                acx[i] = acx[i - 1] + acw[i - 1];

            float tw = acw.Sum();
            using var acHeadBrush = new SolidBrush(Color.FromArgb(200, 205, 215));
            e.Graphics!.FillRectangle(acHeadBrush, r.X0 + 8, r.Y, tw, r.LH);
            e.Graphics!.DrawString("Nr.", r.Label, Brushes.Black, acx[0] + 4, r.Y + 1);
            e.Graphics!.DrawString("Pershkrimi", r.Label, Brushes.Black, acx[1] + 4, r.Y + 1);
            e.Graphics!.DrawString("Sasia", r.Label, Brushes.Black, acx[2] + 4, r.Y + 1);
            e.Graphics!.DrawString("Shenime", r.Label, Brushes.Black, acx[3] + 4, r.Y + 1);
            r.Y += r.LH;

            bool alt = false;
            using var brAlt = new SolidBrush(Color.FromArgb(246, 248, 252));
            foreach (var a in d.Aksesoret)
            {
                if (alt) e.Graphics!.FillRectangle(brAlt, r.X0 + 8, r.Y, tw, r.LH);
                alt = !alt;
                e.Graphics!.DrawString($"{a.Nr}.", r.Normal, Brushes.Black, acx[0] + 4, r.Y + 1);
                e.Graphics!.DrawString(a.Emri ?? "", r.Normal, Brushes.Black, acx[1] + 4, r.Y + 1);
                e.Graphics!.DrawString(a.Sasia.ToString(), r.Normal, Brushes.Black, acx[2] + 4, r.Y + 1);
                e.Graphics!.DrawString(a.Shenime ?? "", r.Normal, Brushes.Black, acx[3] + 4, r.Y + 1);
                e.Graphics!.DrawLine(r.LightPen, r.X0 + 8, r.Y + r.LH, r.X0 + 8 + tw, r.Y + r.LH);
                r.Y += r.LH + 2;
            }
            e.Graphics!.DrawRectangle(r.LightPen, r.X0 + 8, r.Y - (r.LH + 2) * d.Aksesoret.Count, tw, (r.LH + 2) * d.Aksesoret.Count);
            r.Y += 4;
        }

        // ── 6. MUNICIONI ──
        r.DrawSection("  6.  MUNICIONI");
        if (d.Municionet.Count > 0)
        {
            float mw0 = 28f;
            float mw1 = (r.W - 40) * 0.30f;
            float mw2 = (r.W - 40) * 0.22f;
            float mw3 = (r.W - 40) * 0.18f;
            float mw4 = (r.W - 40) - mw0 - mw1 - mw2 - mw3;
            float[] mw = { mw0, mw1, mw2, mw3, mw4 };
            float[] mx = new float[mw.Length];
            mx[0] = r.X0 + 8;
            for (int i = 1; i < mw.Length; i++)
                mx[i] = mx[i - 1] + mw[i - 1];
            float mtw = mw.Sum();
            using var muHeadBrush = new SolidBrush(Color.FromArgb(200, 205, 215));
            e.Graphics!.FillRectangle(muHeadBrush, r.X0 + 8, r.Y, mtw, r.LH);
            e.Graphics!.DrawString("Nr.", r.Label, Brushes.Black, mx[0] + 4, r.Y + 1);
            e.Graphics!.DrawString("Emri", r.Label, Brushes.Black, mx[1] + 4, r.Y + 1);
            e.Graphics!.DrawString("Kalibri", r.Label, Brushes.Black, mx[2] + 4, r.Y + 1);
            e.Graphics!.DrawString("Sasia", r.Label, Brushes.Black, mx[3] + 4, r.Y + 1);
            e.Graphics!.DrawString("Shenime", r.Label, Brushes.Black, mx[4] + 4, r.Y + 1);
            r.Y += r.LH;
            bool malt = false;
            using var brMalt = new SolidBrush(Color.FromArgb(246, 248, 252));
            foreach (var m in d.Municionet)
            {
                if (malt) e.Graphics!.FillRectangle(brMalt, r.X0 + 8, r.Y, mtw, r.LH);
                malt = !malt;
                e.Graphics!.DrawString($"{m.Nr}.", r.Normal, Brushes.Black, mx[0] + 4, r.Y + 1);
                e.Graphics!.DrawString(m.Emri ?? "", r.Normal, Brushes.Black, mx[1] + 4, r.Y + 1);
                e.Graphics!.DrawString(m.Kalibri ?? "-", r.Normal, Brushes.Black, mx[2] + 4, r.Y + 1);
                e.Graphics!.DrawString(m.Sasia > 0 ? $"{m.Sasia} {m.Njesia}" : "-", r.Normal, Brushes.Black, mx[3] + 4, r.Y + 1);
                e.Graphics!.DrawString(m.Shenime ?? "", r.Normal, Brushes.Black, mx[4] + 4, r.Y + 1);
                e.Graphics!.DrawLine(r.LightPen, r.X0 + 8, r.Y + r.LH, r.X0 + 8 + mtw, r.Y + r.LH);
                r.Y += r.LH + 2;
            }
            e.Graphics!.DrawRectangle(r.LightPen, r.X0 + 8, r.Y - (r.LH + 2) * d.Municionet.Count, mtw, (r.LH + 2) * d.Municionet.Count);
            r.Y += 4;
        }
        else
        {
            e.Graphics!.DrawString(string.IsNullOrEmpty(d.Municioni) ? "Asnje" : d.Municioni, r.Normal, Brushes.Black, r.X0 + 10, r.Y);
            r.Y += r.LH + 6;
        }

        // ── 7. PRANIM / DORZIM ──
        r.DrawSection("  7.  PRANIM / DORZIM");
        r.Y += 2;
        e.Graphics!.DrawString("DORZUESI:", r.Label, Brushes.Black, c1, r.Y);
        e.Graphics!.DrawString(string.IsNullOrEmpty(d.PersoneliQeDorzoi) ? "-" : d.PersoneliQeDorzoi, r.Normal, Brushes.Black, c1 + 150, r.Y);
        r.Y += r.LH;
        e.Graphics!.DrawString("MARRESI:", r.Label, Brushes.Black, c1, r.Y);
        e.Graphics!.DrawString(string.IsNullOrEmpty(d.PersoneliQeMorri) ? "-" : d.PersoneliQeMorri, r.Normal, Brushes.Black, c1 + 150, r.Y);
        r.Y += r.LH;
        e.Graphics!.DrawString("ZYRTARI PRANUES:", r.Label, Brushes.Black, c1, r.Y);
        e.Graphics!.DrawString($"{d.PersoneliEmri} ({d.PersoneliGrada})", r.Normal, Brushes.Black, c1 + 150, r.Y);
        e.Graphics!.DrawString($"Legj.: {d.PersoneliLegjitimi}", r.Normal, Brushes.Black, c2, r.Y);
        r.Y += r.LH;
        e.Graphics!.DrawString("KLIENTI:", r.Label, Brushes.Black, c1, r.Y);
        e.Graphics!.DrawString(d.KlientiEmri ?? "-", r.Normal, Brushes.Black, c1 + 150, r.Y);
        e.Graphics!.DrawString($"NID: {d.KlientiNID}", r.Normal, Brushes.Black, c2, r.Y);
        r.Y += r.LH + 6;

        // ── 8. NOTES ──
        r.DrawSection("  8.  SHENIME / VEREJTJE");
        e.Graphics!.DrawString(string.IsNullOrEmpty(d.Shenime) ? "-" : d.Shenime, r.Normal, Brushes.Black, r.X0 + 10, r.Y);
        r.Y += r.LH + 8;

        // ── 9. SIGNATURES ──
        r.DrawSection("  9.  NENSHRKIMET");
        r.DrawSignatureBlock(
            new[] { r.TmpSig1.ToUpper(), r.TmpSig2.ToUpper(), r.TmpSig3.ToUpper() },
            new[] { d.PersoneliQeDorzoi ?? "", d.PersoneliQeMorri ?? "", d.PersoneliEmri ?? "" },
            new[] { r.TmpSig1, r.TmpSig2, r.TmpSig3 });

        // ── FOOTER ──
        r.DrawFooter(d.Kopja ?? "", d.ArmaSerial);

        _currentPrintIndex++;
        e.HasMorePages = _currentPrintIndex < _printDocs.Count;
    }

    private void btnMbyll_Click(object sender, EventArgs e) => Close();
}
