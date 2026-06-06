using System.Data;
using System.Drawing.Printing;
using ArmepunesApp.Data;
using ArmepunesApp.Models;

namespace ArmepunesApp.Forms;

public partial class DitariArmeForm : Form
{
    private readonly DatabaseHelper _db;
    private readonly string _serial;
    private readonly string _perdoruesi;
    private DataGridView dgv = null!;
    private DataTable? _data;
    private ComboBox cmbPrinter = null!;

    // Template parameters
    private string _tmpHeaderTitle = "POLIGONI DRENI";
    private string _tmpHeaderSub = "Qendra e Deponimit dhe Menaxhimit te Armeve";
    private string _tmpHeaderAddr = "Prishtine, Republika e Kosoves";
    private string _tmpSig1 = "DOREZUESI";
    private string _tmpSig2 = "MARRESI";
    private string _tmpSig3 = "PERGJEGJESI I DEPOS";
    private string _tmpFooter = "Dokument i gjeneruar nga Sistemi Deponim i Armeve";
    private Color _tmpHeaderColor = Color.FromArgb(0, 70, 130);
    private Color _tmpTextColor = Color.Black;
    private int _tmpFontTitle = 16;
    private int _tmpFontNormal = 9;
    private float _tmpYOffset = 0;

    public DitariArmeForm(DatabaseHelper db, string serial, string perdoruesi = "")
    {
        _db = db;
        _serial = serial;
        _perdoruesi = perdoruesi;
        InitializeComponent();
        NgarkoTemplateParams();
        NgarkoPrintera();
        NgarkoTeDhenat();
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

    private void InitializeComponent()
    {
        Text = $"Ditari i Armes - {_serial}";
        Size = new Size(800, 520);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(30, 33, 40);
        ForeColor = Color.FromArgb(200, 205, 216);
        Font = new Font("Segoe UI", 9);

        var lblTitle = new Label
        {
            Text = $"Ditari i Armes: {_serial}",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Color.FromArgb(46, 204, 113),
            Location = new Point(15, 15),
            Size = new Size(500, 30)
        };

        dgv = new DataGridView();
        dgv.Location = new Point(15, 55);
        dgv.Size = new Size(755, 390);
        StilizoDgv(dgv);

        var lblPrinter = new Label
        {
            Text = "Printeri:",
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = Color.FromArgb(160, 165, 175),
            Location = new Point(15, 452),
            Size = new Size(55, 24)
        };

        cmbPrinter = new ComboBox
        {
            Location = new Point(72, 450),
            Size = new Size(280, 24),
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(45, 48, 55),
            ForeColor = Color.FromArgb(200, 205, 216),
            Font = new Font("Segoe UI", 9)
        };

        var btnParashiko = new Button
        {
            Text = "Parashiko",
            Location = new Point(480, 448),
            Size = new Size(90, 30),
            BackColor = Color.FromArgb(60, 62, 68),
            ForeColor = Color.FromArgb(200, 205, 216),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnParashiko.Click += (s, e) => ParashikoDitarin();

        var btnPrinto = new Button
        {
            Text = "Printo",
            Location = new Point(580, 448),
            Size = new Size(90, 30),
            BackColor = Color.FromArgb(0, 80, 140),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 9, FontStyle.Bold)
        };
        btnPrinto.Click += (s, e) => PrintoDitarin();

        var btnKarlo = new Button
        {
            Text = "Karlo Tipin",
            Location = new Point(360, 448),
            Size = new Size(90, 30),
            BackColor = Color.FromArgb(155, 89, 182),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 9, FontStyle.Bold)
        };
        btnKarlo.Click += (s, e) => KarloTipin();

        var btnFshi = new Button
        {
            Text = "Fshi",
            Location = new Point(460, 448),
            Size = new Size(70, 30),
            BackColor = Color.FromArgb(192, 57, 43),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 9, FontStyle.Bold)
        };
        btnFshi.Click += (s, e) => FshiTransaksionin();

        var btnMbyll = new Button
        {
            Text = "Mbyll",
            Location = new Point(680, 448),
            Size = new Size(90, 30),
            BackColor = Color.FromArgb(60, 62, 68),
            ForeColor = Color.FromArgb(200, 205, 216),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnMbyll.Click += (s, e) => Close();

        Controls.Add(lblTitle);
        Controls.Add(dgv);
        Controls.Add(lblPrinter);
        Controls.Add(cmbPrinter);
        Controls.Add(btnParashiko);
        Controls.Add(btnPrinto);
        Controls.Add(btnKarlo);
        Controls.Add(btnFshi);
        Controls.Add(btnMbyll);
    }

    private void StilizoDgv(DataGridView dgv)
    {
        dgv.BackgroundColor = Color.FromArgb(30, 32, 37);
        dgv.ForeColor = Color.FromArgb(200, 205, 216);
        dgv.GridColor = Color.FromArgb(50, 52, 58);
        dgv.BorderStyle = BorderStyle.FixedSingle;
        dgv.Font = new Font("Segoe UI", 9);
        dgv.ReadOnly = true;
        dgv.AllowUserToAddRows = false;
        dgv.AllowUserToDeleteRows = false;
        dgv.RowHeadersVisible = false;
        dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgv.EnableHeadersVisualStyles = false;
        dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 80, 140);
        dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        dgv.ColumnHeadersHeight = 28;
        dgv.RowsDefaultCellStyle.BackColor = Color.FromArgb(35, 38, 45);
        dgv.RowsDefaultCellStyle.ForeColor = Color.FromArgb(200, 205, 216);
        dgv.RowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 100, 160);
        dgv.RowsDefaultCellStyle.SelectionForeColor = Color.White;
        dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(40, 42, 50);
    }

    private void NgarkoTeDhenat()
    {
        try
        {
            _data = _db.MerrTransaksionetBySerial(_serial);
            if (_data == null || _data.Rows.Count == 0)
            {
                Text = $"Ditari i armes - {_serial} (pa transaksione)";
                return;
            }

            var display = new DataTable();
            display.Columns.Add("Nr", typeof(int));
            display.Columns.Add("Tipi");
            display.Columns.Add("Data/Ora");
            display.Columns.Add("Klienti");
            display.Columns.Add("Personeli");
            display.Columns.Add("Qellimi");

            int nr = 1;
            foreach (DataRow r in _data.Rows)
            {
                var dataOra = r["DataOra"]?.ToString() ?? "";
                var data = dataOra.Length >= 16 ? dataOra.Replace("-", ".") : dataOra;
                display.Rows.Add(
                    nr++,
                    r["Tipi"]?.ToString() == "Hyrje" ? "HYRJE" : "DALJE",
                    data,
                    r["KlientiEmri"] ?? "-",
                    r["PersoneliEmri"] ?? "-",
                    r["Qellimi"] ?? "-"
                );
            }

            dgv.DataSource = display;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            if (dgv.Columns["Nr"] != null) dgv.Columns["Nr"].Width = 40;
            if (dgv.Columns["Tipi"] != null) dgv.Columns["Tipi"].Width = 65;
            if (dgv.Columns["Data/Ora"] != null) dgv.Columns["Data/Ora"].Width = 140;
            if (dgv.Columns["Klienti"] != null) dgv.Columns["Klienti"].Width = 170;
            if (dgv.Columns["Personeli"] != null) dgv.Columns["Personeli"].Width = 170;
            if (dgv.Columns["Qellimi"] != null) dgv.Columns["Qellimi"].Width = dgv.Width - 40 - 65 - 140 - 170 - 170 - 30;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Gabim gjate ngarkimit te ditarit:\n{ex.Message}", "Gabim",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void NgarkoTemplateParams()
    {
        try
        {
            int tid = _db.MerrTemplateAktivId("Ditar");
            if (tid == 0) return;
            var p = _db.MerrTemplateParametrat(tid);
            if (string.IsNullOrEmpty(p) || p == "{}") return;
            var json = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, System.Text.Json.JsonElement>>(p);
            if (json == null) return;
            if (json.TryGetValue("txtHeaderTitle", out var v)) _tmpHeaderTitle = v.GetString() ?? _tmpHeaderTitle;
            if (json.TryGetValue("txtHeaderSub", out v)) _tmpHeaderSub = v.GetString() ?? _tmpHeaderSub;
            if (json.TryGetValue("txtHeaderAddr", out v)) _tmpHeaderAddr = v.GetString() ?? _tmpHeaderAddr;
            if (json.TryGetValue("txtSig1", out v)) _tmpSig1 = v.GetString() ?? _tmpSig1;
            if (json.TryGetValue("txtSig2", out v)) _tmpSig2 = v.GetString() ?? _tmpSig2;
            if (json.TryGetValue("txtSig3", out v)) _tmpSig3 = v.GetString() ?? _tmpSig3;
            if (json.TryGetValue("txtFooter", out v)) _tmpFooter = v.GetString() ?? _tmpFooter;
            if (json.TryGetValue("headerColor", out var c)) _tmpHeaderColor = Color.FromArgb(c.GetInt32());
            if (json.TryGetValue("textColor", out c)) _tmpTextColor = Color.FromArgb(c.GetInt32());
            if (json.TryGetValue("fontTitle", out var f)) _tmpFontTitle = f.GetInt32();
            if (json.TryGetValue("fontNormal", out f)) _tmpFontNormal = f.GetInt32();
            if (json.TryGetValue("yOffset", out var y)) _tmpYOffset = (float)y.GetDouble();
        }
        catch { }
    }

    private PrintDocument KrijoPrintDocument()
    {
        var pd = new PrintDocument();
        pd.PrintPage += PrintoFaqen;
        pd.DefaultPageSettings.Landscape = false;
        pd.DefaultPageSettings.PaperSize = new PaperSize("A4", 827, 1169);
        // Margins from template are read via template params; fallback to 25
        pd.DefaultPageSettings.Margins = new Margins(25, 25, 25, 25);
        if (cmbPrinter.SelectedItem != null)
            pd.PrinterSettings.PrinterName = cmbPrinter.SelectedItem.ToString()!;
        return pd;
    }

    private void ParashikoDitarin()
    {
        if (_data == null || _data.Rows.Count == 0)
        {
            MessageBox.Show("Nuk ka te dhena per te paraqitur.", "Info",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        try
        {
            using var dlg = new PrintPreviewDialog
            {
                Document = KrijoPrintDocument(),
                Width = 900,
                Height = 700,
                Text = "Parashiko - Ditari i Armes"
            };
            dlg.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Gabim ne parashikim:\n{ex.Message}", "Gabim",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void PrintoDitarin()
    {
        if (_data == null || _data.Rows.Count == 0)
        {
            MessageBox.Show("Nuk ka te dhena per te printuar.", "Info",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        try
        {
            KrijoPrintDocument().Print();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Gabim ne printim:\n{ex.Message}", "Gabim",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void PrintoFaqen(object? sender, PrintPageEventArgs e)
    {
        var g = e.Graphics!;
        var page = e.PageBounds;
        float m = 25;
        float x0 = m;
        float y = m + _tmpYOffset / 25.4f * 100;
        float w = page.Width - m * 2;
        float usableH = page.Height - m * 2;

        if (_data == null) { e.HasMorePages = false; return; }

        var fontTitle = new Font("Segoe UI", _tmpFontTitle, FontStyle.Bold);
        var fontSub = new Font("Segoe UI", Math.Max(10, _tmpFontTitle - 4), FontStyle.Bold);
        var fontNormal = new Font("Segoe UI", _tmpFontNormal);
        var fontHeader = new Font("Segoe UI", _tmpFontNormal, FontStyle.Bold);
        var fontSmall = new Font("Segoe UI", Math.Max(7, _tmpFontNormal - 1));
        float lh = Math.Max(16, _tmpFontNormal + 10);

        using var textBrush = new SolidBrush(_tmpTextColor);
        var penLight = new Pen(Color.FromArgb(160, 165, 175), 0.5f);
        var brDark = new SolidBrush(_tmpHeaderColor);

        // Header
        g.DrawString(_tmpHeaderTitle, fontTitle, brDark, x0, y);
        y += 24;
        g.DrawString(_tmpHeaderSub, fontSub, textBrush, x0, y);
        y += 16;
        g.DrawString(_tmpHeaderAddr, fontNormal, textBrush, x0, y);
        y += fontNormal.Height + 2;
        using var bdrPen = new Pen(_tmpHeaderColor, 2);
        g.DrawLine(bdrPen, x0, y, x0 + w, y);
        y += 10;

        // Document title
        g.DrawString($"DITARI I ARMES — {_serial}", fontSub, textBrush, x0, y);
        g.DrawString($"Printuar: {DateTime.Now:dd.MM.yyyy HH:mm}", fontSmall, Brushes.Gray, x0 + w - 140, y);
        y += 24;

        // Table header
        float[] colW = { 28, 55, 110, 120, 120, w - 28 - 55 - 110 - 120 - 120 };
        float[] colX = new float[colW.Length];
        colX[0] = x0;
        for (int i = 1; i < colW.Length; i++)
            colX[i] = colX[i - 1] + colW[i - 1];
        float tableW = colW.Sum();

        string[] headers = { "Nr.", "Tipi", "Data/Ora", "Klienti", "Personeli", "Qellimi" };
        using var headBrush = new SolidBrush(Color.FromArgb(
            Math.Max(0, _tmpHeaderColor.R - 20),
            Math.Max(0, _tmpHeaderColor.G - 20),
            Math.Min(255, _tmpHeaderColor.B + 10)));
        g.FillRectangle(headBrush, x0, y, tableW, lh + 4);
        for (int i = 0; i < headers.Length; i++)
            g.DrawString(headers[i], fontHeader, Brushes.White, colX[i] + 3, y + 2);
        y += lh + 4;

        // Data rows
        bool alt = false;
        int nr = 1;
        using var brAlt = new SolidBrush(Color.FromArgb(240, 243, 248));
        foreach (DataRow r in _data.Rows)
        {
            if (y + lh > usableH + m - 30) break;
            if (alt) g.FillRectangle(brAlt, x0, y, tableW, lh);
            alt = !alt;

            var dataOra = r["DataOra"]?.ToString() ?? "";
            var data = dataOra.Length >= 16 ? dataOra.Replace("-", ".") : dataOra;
            var tipi = r["Tipi"]?.ToString() == "Hyrje" ? "HYRJE" : "DALJE";

            g.DrawString($"{nr++}.", fontNormal, Brushes.Black, colX[0] + 3, y + 1);
            g.DrawString(tipi, fontNormal, Brushes.Black, colX[1] + 3, y + 1);
            g.DrawString(data, fontNormal, Brushes.Black, colX[2] + 3, y + 1);
            g.DrawString(r["KlientiEmri"]?.ToString() ?? "-", fontNormal, Brushes.Black, colX[3] + 3, y + 1);
            g.DrawString(r["PersoneliEmri"]?.ToString() ?? "-", fontNormal, Brushes.Black, colX[4] + 3, y + 1);
            g.DrawString(r["Qellimi"]?.ToString() ?? "-", fontNormal, Brushes.Black, colX[5] + 3, y + 1);
            g.DrawLine(penLight, x0, y + lh - 1, x0 + tableW, y + lh - 1);
            y += lh;
        }

        // Footer
        y = Math.Max(y, m + 60);
        using var footPen = new Pen(_tmpHeaderColor, 1);
        g.DrawLine(footPen, x0, y + 20, x0 + w, y + 20);
        y += 28;

        float sigW = (tableW - 40) / 3;
        string[] sigLabels = { _tmpSig1, _tmpSig2, _tmpSig3 };
        for (int i = 0; i < 3; i++)
        {
            g.DrawRectangle(penLight, x0 + 10 + i * (sigW + 20), y, sigW, 50);
            g.DrawString(sigLabels[i], fontHeader, textBrush, x0 + 15 + i * (sigW + 20), y + 4);
            g.DrawString("Nenshkrimi / Data", fontSmall, Brushes.Gray, x0 + 15 + i * (sigW + 20), y + 30);
        }
        y += 58;

        g.DrawString($"{_tmpFooter} - {DateTime.Now:dd.MM.yyyy HH:mm:ss}",
            fontSmall, Brushes.Gray, x0, y);

        brDark.Dispose();
        penLight.Dispose();
        fontTitle.Dispose(); fontSub.Dispose(); fontNormal.Dispose();
        fontHeader.Dispose(); fontSmall.Dispose();

        e.HasMorePages = false;
    }

    private bool KerkoAutorizim(string veprimi)
    {
        using var dlg = new PasswordDialog(_db, veprimi);
        return dlg.ShowDialog(this) == DialogResult.OK && dlg.Autorizuar;
    }

    private int? MerrIdTransaksionitTeSelektuar()
    {
        if (dgv.SelectedRows.Count == 0 || _data == null) return null;
        int rowIdx = dgv.SelectedRows[0].Index;
        if (rowIdx < 0 || rowIdx >= _data.Rows.Count) return null;
        var idObj = _data.Rows[rowIdx]["Id"];
        return idObj != null ? Convert.ToInt32(idObj) : null;
    }

    private void KarloTipin()
    {
        var id = MerrIdTransaksionitTeSelektuar();
        if (id == null)
        { MessageBox.Show("Zgjidh nje transaksion nga lista.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }

        if (!KerkoAutorizim("Ndrysho Tip Transaksioni"))
            return;

        if (_data == null) return;
        string tipiAktual = "";
        foreach (DataRow r in _data.Rows)
        {
            if (Convert.ToInt32(r["Id"]) == id.Value)
            { tipiAktual = r["Tipi"]?.ToString() ?? ""; break; }
        }
        if (string.IsNullOrEmpty(tipiAktual)) return;

        string tipiRi = tipiAktual == "Hyrje" ? "Dalje" : "Hyrje";
        string pershkrim = tipiAktual == "Hyrje" ? "Hyrje → Dalje" : "Dalje → Hyrje";

        if (MessageBox.Show($"A jeni te sigurt qe doni ta ndryshoni kete transaksion?\n{pershkrim}",
            "Konfirmo Ndryshim", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        if (_db.NdryshoTipinTransaksionit(id.Value, tipiRi, _perdoruesi))
        {
            NgarkoTeDhenat();
            MessageBox.Show($"Transaksioni u ndryshua me sukses: {pershkrim}", "Sukses",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        else
            MessageBox.Show("Transaksioni nuk mund te ndryshohet.", "Gabim",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    private void FshiTransaksionin()
    {
        var id = MerrIdTransaksionitTeSelektuar();
        if (id == null)
        { MessageBox.Show("Zgjidh nje transaksion nga lista.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }

        if (!KerkoAutorizim("Fshi Transaksion"))
            return;

        if (MessageBox.Show("Jeni te sigurt qe doni te fshini kete transaksion?",
            "Konfirmim", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        if (_db.FshiTransaksion(id.Value, _perdoruesi))
        {
            NgarkoTeDhenat();
            MessageBox.Show("Transaksioni u fshi me sukses.", "Sukses",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        else
            MessageBox.Show("Transaksioni nuk mund te fshihet.", "Gabim",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
