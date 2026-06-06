using System.Data;
using ArmepunesApp.Forms;

namespace ArmepunesApp;

public partial class MainForm
{
    private void StilizoHeader()
    {
        var header = new Panel { Dock = DockStyle.Top, Height = 72 };
        header.Paint += (_, e) =>
        {
            using var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                header.ClientRectangle, Color.FromArgb(15, 23, 42), Color.FromArgb(30, 30, 35),
                System.Drawing.Drawing2D.LinearGradientMode.Vertical);
            e.Graphics.FillRectangle(brush, header.ClientRectangle);
            using var pen = new Pen(Color.FromArgb(30, 144, 255, 40), 2);
            e.Graphics.DrawRectangle(pen, 0, 0, header.Width - 1, header.Height - 1);
        };
        var icon = new Label { Text = "\u2694", Font = new Font("Segoe UI", 26), ForeColor = Color.Gold,
            Location = new Point(16, 10), Size = new Size(48, 48), TextAlign = ContentAlignment.MiddleCenter };
        var title = new Label { Text = "POLIGONI DRENI", Font = new Font("Segoe UI", 18, FontStyle.Bold),
            ForeColor = Color.White, Location = new Point(70, 4), Size = new Size(380, 34) };
        var ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "?";
        var subtitle = new Label { Text = $"Qendra e Deponimit dhe Menaxhimit te Armeve  |  v{ver}",
            Font = new Font("Segoe UI", 8), ForeColor = Color.FromArgb(140, 175, 210),
            Location = new Point(70, 36), Size = new Size(500, 18) };

        var userInfo = new Label
        {
            Text = $"{_perdoruesi.Emri}  |  {_perdoruesi.Role}",
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = Color.FromArgb(100, 180, 240),
            Location = new Point(ClientSize.Width - 310, 10),
            Size = new Size(280, 22),
            TextAlign = ContentAlignment.MiddleRight
        };

        var clock = new Label
        {
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.FromArgb(160, 195, 225),
            Location = new Point(ClientSize.Width - 200, 34),
            Size = new Size(170, 22),
            TextAlign = ContentAlignment.MiddleRight
        };
        var timer = new System.Windows.Forms.Timer { Interval = 1000 };
        timer.Tick += (_, _) => clock.Text = DateTime.Now.ToString("dd.MM.yyyy  HH:mm:ss");
        timer.Start();

        header.Resize += (_, _) =>
        {
            clock.Left = header.Width - 200;
            userInfo.Left = header.Width - 310;
        };

        var accentLine = new Panel { Dock = DockStyle.Bottom, Height = 2, BackColor = Color.Gold };
        header.Controls.AddRange(new Control[] { icon, title, subtitle, userInfo, clock, accentLine });
        Controls.Add(header);
        Controls.SetChildIndex(header, 0);
    }

    private void StilizoStatistikat()
    {
        _statsPanel = new Panel { Dock = DockStyle.Top, Height = 170, BackColor = Color.FromArgb(35, 38, 45), Padding = new Padding(10, 4, 10, 4) };

        var flow = new FlowLayoutPanel { Location = new Point(8, 4), Size = new Size(1000, 68), FlowDirection = FlowDirection.LeftToRight, WrapContents = false };

        _lblTotalArmet = KrijoStatCard("Total Arme", "0", Color.FromArgb(100, 140, 200), "\u2694");
        _lblNeMagazine = KrijoStatCard("Ne Magazine", "0", Color.FromArgb(46, 204, 113), "\uD83C\uDFEA");
        _lblNePerdorim = KrijoStatCard("Tek Klienti", "0", Color.FromArgb(243, 156, 18), "\uD83D\uDCCB");
        _lblKliente = KrijoStatCard("Kliente","0", Color.FromArgb(52, 152, 219), "\uD83D\uDC64");
        _lblTransaksioneSot = KrijoStatCard("Transaksione Sot", "0", Color.FromArgb(155, 89, 182), "\uD83D\uDCCA");

        flow.Controls.AddRange(new Control[] { _lblTotalArmet, _lblNeMagazine, _lblNePerdorim, _lblKliente, _lblTransaksioneSot });

        _chartPanel = new Panel { Location = new Point(8, 76), Size = new Size(700, 70), BackColor = Color.FromArgb(30, 30, 35) };
        _chartPanel.Paint += VizatoGrafikun;

        var lblGrafik = new Label();
        lblGrafik.Text = "Gjendja e Deponimit:";
        lblGrafik.Font = new Font("Segoe UI", 8, FontStyle.Bold);
        lblGrafik.ForeColor = Color.FromArgb(160, 168, 180);
        lblGrafik.Size = new Size(120, 20);
        lblGrafik.Location = new Point(720, 80);

        _lblChartPerqindje = new Label();
        _lblChartPerqindje.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        _lblChartPerqindje.ForeColor = Color.FromArgb(46, 204, 113);
        _lblChartPerqindje.Size = new Size(260, 40);
        _lblChartPerqindje.Location = new Point(720, 100);
        _lblChartPerqindje.TextAlign = ContentAlignment.TopLeft;

        _statsPanel.Controls.Add(flow);
        _statsPanel.Controls.Add(_chartPanel);
        _statsPanel.Controls.Add(lblGrafik);
        _statsPanel.Controls.Add(_lblChartPerqindje);
        Controls.Add(_statsPanel);
    }

    private Label KrijoStatCard(string titull, string vlera, Color ngjyra, string icon)
    {
        var card = new Panel { Width = 200, Height = 64, Margin = new Padding(4, 0, 4, 0), BackColor = Color.Transparent, Padding = new Padding(8) };
        card.Paint += (_, e) =>
        {
            using var bgBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
                card.ClientRectangle, Color.FromArgb(50, 53, 60), Color.FromArgb(40, 43, 50),
                System.Drawing.Drawing2D.LinearGradientMode.Vertical);
            e.Graphics.FillRectangle(bgBrush, 0, 0, card.Width, card.Height);
            using var pen = new Pen(Color.FromArgb(80, 83, 90), 1);
            e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            using var accentBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
                new Rectangle(0, 0, 8, card.Height), ngjyra, ControlPaint.Light(ngjyra, 0.3f),
                System.Drawing.Drawing2D.LinearGradientMode.Vertical);
            e.Graphics.FillRectangle(accentBrush, 0, 0, 8, card.Height);
        };
        var lblIcon = new Label { Text = icon, Font = new Font("Segoe UI", 16), Location = new Point(12, 12), Size = new Size(36, 36), TextAlign = ContentAlignment.MiddleCenter, ForeColor = ngjyra, BackColor = Color.Transparent };
        var lblValue = new Label { Text = vlera, Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = Color.White, Location = new Point(52, 4), Size = new Size(130, 32), TextAlign = ContentAlignment.MiddleLeft, BackColor = Color.Transparent };
        var lblTitle = new Label { Text = titull, Font = new Font("Segoe UI", 7), ForeColor = Color.FromArgb(160, 168, 180), Location = new Point(52, 38), Size = new Size(130, 18), TextAlign = ContentAlignment.MiddleLeft, BackColor = Color.Transparent };
        card.Controls.AddRange(new Control[] { lblIcon, lblValue, lblTitle });
        return lblValue;
    }

    private static System.Drawing.Drawing2D.GraphicsPath RoundedRect(Rectangle bounds, int radius)
    {
        var path = new System.Drawing.Drawing2D.GraphicsPath();
        int d = radius * 2;
        path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
        path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
        path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }

    private void VizatoGrafikun(object? s, PaintEventArgs e)
    {
        var total = _armetTable.Rows.Count;
        int neMagazine = total > 0 ? _armetTable.Select("Statusi = 'Ne Magazine'").Length : 0;
        int nePerdorim = total > 0 ? _armetTable.Select("Statusi = 'Tek Klienti'").Length : 0;
        int teTjere = total - neMagazine - nePerdorim;

        var panel = (Panel)s!;
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        int x = 10, y = 10, w = panel.Width - 20;
        int barHeight = 22;

        if (total == 0)
        {
            using var font = new Font("Segoe UI", 10, FontStyle.Italic);
            using var brush = new SolidBrush(Color.FromArgb(100, 105, 115));
            g.DrawString("Nuk ka te dhena per grafikon", font, brush, x + 10, y + 8);
            return;
        }

        double pctMagazine = (double)neMagazine / total;
        double pctPerdorim = (double)nePerdorim / total;
        double pctTjere = (double)teTjere / total;

        int barW = w;
        int magW = (int)(barW * pctMagazine);
        int perdW = (int)(barW * pctPerdorim);
        int tjereW = Math.Max(barW - magW - perdW, 0);

        using var lblFont = new Font("Segoe UI", 9, FontStyle.Bold);

        if (magW > 0)
        {
            using var brush = new SolidBrush(Color.FromArgb(46, 204, 113));
            using var path = RoundedRect(new Rectangle(x, y, magW, barHeight), 4);
            g.FillPath(brush, path);
            g.DrawString($"Ne Magazine: {neMagazine} ({pctMagazine * 100:F0}%)", lblFont, Brushes.White, x + 6, y + 2);
        }
        if (perdW > 0)
        {
            using var brush = new SolidBrush(Color.FromArgb(243, 156, 18));
            using var path = RoundedRect(new Rectangle(x + magW, y, perdW, barHeight), 4);
            g.FillPath(brush, path);
            g.DrawString($"Tek Klienti: {nePerdorim} ({pctPerdorim * 100:F0}%)", lblFont, Brushes.White, x + magW + 6, y + 2);
        }
        if (tjereW > 0)
        {
            using var brush = new SolidBrush(Color.FromArgb(150, 150, 160));
            using var path = RoundedRect(new Rectangle(x + magW + perdW, y, tjereW, barHeight), 4);
            g.FillPath(brush, path);
        }

        int legendY = y + barHeight + 8;
        using var linePen = new Pen(Color.FromArgb(60, 62, 68), 1);
        g.DrawLine(linePen, x, legendY, x + barW, legendY);

        using var lblLegendFont = new Font("Segoe UI", 8, FontStyle.Bold);
        using var dotMag = new SolidBrush(Color.FromArgb(46, 204, 113));
        g.FillEllipse(dotMag, x, legendY + 4, 6, 6);
        g.DrawString("Magazine", lblLegendFont, Brushes.LightGray, x + 10, legendY + 2);

        using var dotPerd = new SolidBrush(Color.FromArgb(243, 156, 18));
        g.FillEllipse(dotPerd, x + 80, legendY + 4, 6, 6);
        g.DrawString("Perdorim", lblLegendFont, Brushes.LightGray, x + 90, legendY + 2);

        using var dotTotal = new SolidBrush(Color.FromArgb(100, 140, 200));
        g.FillEllipse(dotTotal, x + 170, legendY + 4, 6, 6);
        g.DrawString($"Total: {total}", lblLegendFont, Brushes.LightGray, x + 180, legendY + 2);

        _lblChartPerqindje.Text = $"Ne Magazine: {neMagazine} ({pctMagazine * 100:F0}%)\nTek Klienti: {nePerdorim} ({pctPerdorim * 100:F0}%)";
    }

    private void PerditesoStatistikat()
    {
        _lblTotalArmet.Text = _armetTable.Rows.Count.ToString();
        int neMag = _armetTable.Rows.Count > 0 ? _armetTable.Select("Statusi = 'Ne Magazine'").Length : 0;
        int nePerd = _armetTable.Rows.Count > 0 ? _armetTable.Select("Statusi = 'Tek Klienti'").Length : 0;
        _lblNeMagazine.Text = neMag.ToString();
        _lblNePerdorim.Text = nePerd.ToString();
        _lblKliente.Text = _klientetTable.Rows.Count.ToString();
        _lblTransaksioneSot.Text = _transaksionetTable.Columns.Contains("DataOra")
            ? _transaksionetTable.Select($"DataOra LIKE '{DateTime.Now:yyyy-MM-dd}%'").Length.ToString() : "0";
        _chartPanel.Invalidate();
    }

    private void StilizoRibbon()
    {
        var ribbon = new Panel { Dock = DockStyle.Top, Height = 50 };
        ribbon.Paint += (_, e) =>
        {
            using var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                ribbon.ClientRectangle, Color.FromArgb(30, 30, 36), Color.FromArgb(20, 20, 25),
                System.Drawing.Drawing2D.LinearGradientMode.Vertical);
            e.Graphics.FillRectangle(brush, ribbon.ClientRectangle);
        };
        var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
        flow.Padding = new Padding(0);

        Panel GroupAccent(Color c)
        {
            var p = new Panel { Width = 14, Height = 48, BackColor = Color.Transparent };
            p.Paint += (_, e) =>
            {
                using var brush = new SolidBrush(Color.FromArgb(70, c.R, c.G, c.B));
                e.Graphics.FillRectangle(brush, 4, 8, 6, 32);
            };
            return p;
        }

        var toolTip = new ToolTip { AutoPopDelay = 5000, InitialDelay = 300, ReshowDelay = 100, ShowAlways = true };

        Button BtnBig(string text, int w, Color c, EventHandler handler, string tooltip = "")
        {
            var b = new Button();
            b.Text = text; b.Size = new Size(w, 46); b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 1; b.FlatAppearance.BorderColor = ControlPaint.Light(c, 0.2f);
            b.BackColor = c; b.ForeColor = Color.White;
            b.Font = new Font("Segoe UI", 10, FontStyle.Bold); b.Cursor = Cursors.Hand;
            b.TextAlign = ContentAlignment.MiddleCenter; b.UseVisualStyleBackColor = false;
            b.Margin = new Padding(2, 0, 2, 0);
            b.MouseEnter += (_, _) => { b.FlatAppearance.BorderColor = Color.White; b.BackColor = ControlPaint.Light(c, 0.3f); };
            b.MouseLeave += (_, _) => { b.FlatAppearance.BorderColor = ControlPaint.Light(c, 0.3f); b.BackColor = c; };
            b.Click += handler;
            if (!string.IsNullOrEmpty(tooltip)) toolTip.SetToolTip(b, tooltip);
            return b;
        }

        var green = Color.FromArgb(39, 174, 96);
        var darkGreen = Color.FromArgb(0, 160, 80);
        var red = Color.FromArgb(200, 60, 40);
        var purple = Color.FromArgb(155, 89, 182);
        var orange = Color.FromArgb(230, 126, 34);
        var teal = Color.FromArgb(0, 160, 200);
        var gray = Color.FromArgb(120, 130, 145);
        var violet = Color.FromArgb(128, 100, 180);

        flow.Controls.Add(GroupAccent(Color.Gold));
        flow.Controls.Add(BtnBig("\uD83C\uDFE0 BALLINA", 100, Color.FromArgb(60, 70, 90), (_, _) => tabControl.SelectedTab = tabBallina, "Kthehu ne faqen kryesore (Ballina)"));

        flow.Controls.Add(GroupAccent(darkGreen));
        flow.Controls.Add(BtnBig("\uD83D\uDCE5 HYRJE", 110, darkGreen, btnRegjistroHyrje_Click, "Regjistro nje hyrje (deponim) te ri"));
        flow.Controls.Add(BtnBig("\uD83D\uDCE4 DALJE", 110, red, btnRegjistroDalje_Click, "Regjistro nje dalje (terheqje) te re"));

        flow.Controls.Add(GroupAccent(green));
        flow.Controls.Add(BtnBig("\uD83D\uDC64 PERSONEL", 105, green, btnShtoPersonel_Click, "Shto nje personel te ri"));
        flow.Controls.Add(BtnBig("\uD83D\uDC65 KLIENT", 100, green, btnShtoKlient_Click, "Shto nje klient te ri"));
        flow.Controls.Add(BtnBig("\u2694 ARME", 95, green, btnShtoArme_Click, "Shto nje arme te re ne sistem"));
        flow.Controls.Add(BtnBig("\u2B06 BATCH", 90, Color.FromArgb(155, 89, 182), btnBatchArme_Click, "Regjistro arme ne shumice ose importo nga Excel"));

        flow.Controls.Add(GroupAccent(purple));
        flow.Controls.Add(BtnBig("\uD83D\uDDA8 FLETELESHIM", 120, purple, btnPrintoFleteleshim_Click, "Printo fleteleshim per nje transaksion"));
        flow.Controls.Add(BtnBig("\uD83D\uDCC4 FLETELESHIMET", 130, purple, btnListaFleteleshimeve_Click, "Shiko listen e te gjitha fleteleshimeve"));
        flow.Controls.Add(BtnBig("\uD83D\uDCCB LISTA DEPON.", 120, purple, btnListeDeponimi_Click, "Gjenero listen e deponimeve"));
        flow.Controls.Add(BtnBig("\uD83D\uDCD0 FORMAT A4", 105, purple, btnFormaTemplates_Click, "Konfiguro format A4 per dokumentacion"));

        flow.Controls.Add(GroupAccent(orange));
        flow.Controls.Add(BtnBig("\uD83D\uDCCA RAPORTO", 110, orange, btnRaporto_Click, "Gjenero raporte te ndryshme"));
        flow.Controls.Add(BtnBig("\uD83D\uDCCB LISTAT", 100, Color.FromArgb(230, 126, 34), btnRaporto_Click, "Shiko listen e armeve, klienteve dhe personelit"));

        flow.Controls.Add(GroupAccent(teal));
        flow.Controls.Add(BtnBig("\uD83D\uDCE4 EKSPORTO", 105, teal, btnEksporto_Click, "Eksporto te dhenat ne Excel/PDF"));

        flow.Controls.Add(GroupAccent(gray));
        flow.Controls.Add(BtnBig("\uD83D\uDDD1 FSHI TRANS.", 110, gray, btnFshiTransaksion_Click, "Fshi nje transaksion ekzistues"));

        if (_perdoruesi.Role == "Admin")
        {
            flow.Controls.Add(GroupAccent(violet));
            flow.Controls.Add(BtnBig("\u2699 ADMIN", 95, violet, (s, e) => { using var f = new AdminForm(_db, _perdoruesi.Username); f.ShowDialog(); NgarkoBallina(); }, "Paneli i administratorit"));
            flow.Controls.Add(BtnBig("\uD83D\uDD04 AZHORNO", 110, purple, btnUpdateAplikacionit_Click, "Kontrollo per azhornime te aplikacionit"));
        }

        ribbon.Controls.Add(flow);
        Controls.Add(ribbon);

        // Remove any stray sidebar-type buttons from tabs (keep our action buttons)
        foreach (TabPage tab in tabControl.TabPages)
        {
            if (tab == tabArmet || tab == tabPersoneli || tab == tabKlientet) continue;
            var toRemove = new List<Control>();
            foreach (Control c in tab.Controls)
                if (c is Button)
                    toRemove.Add(c);
            foreach (var c in toRemove)
                tab.Controls.Remove(c);
        }
    }

    private void StilizoTabControl()
    {
        tabControl.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        tabControl.Padding = new Point(24, 8);
        tabControl.BackColor = Color.FromArgb(40, 42, 48);
        tabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
        tabControl.DrawItem += (_, e) =>
        {
            using var bgBrush = new SolidBrush(Color.FromArgb(35, 37, 42));
            e.Graphics.FillRectangle(bgBrush, e.Bounds);
            var selected = e.Index == tabControl.SelectedIndex;
            var textColor = selected ? Color.Gold : Color.FromArgb(160, 168, 180);
            using var textFont = new Font("Segoe UI", 10, FontStyle.Bold);
            var text = tabControl.TabPages[e.Index].Text;
            var textSize = e.Graphics.MeasureString(text, textFont);
            var textX = e.Bounds.X + (e.Bounds.Width - (int)textSize.Width) / 2;
            var textY = e.Bounds.Y + (e.Bounds.Height - (int)textSize.Height) / 2;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            e.Graphics.DrawString(text, textFont, Brushes.White, textX, textY);
            if (selected)
            {
                using var goldPen = new Pen(Color.Gold, 3);
                e.Graphics.DrawLine(goldPen, e.Bounds.X + 8, e.Bounds.Bottom - 3, e.Bounds.Right - 8, e.Bounds.Bottom - 3);
            }
        };
        foreach (TabPage tab in tabControl.TabPages)
        { tab.BackColor = Color.FromArgb(30, 32, 38); tab.ForeColor = Color.FromArgb(200, 205, 215); }
    }

    private void StilizoDataGridViews()
    {
        StilizoGrid(dgvArmet, Color.FromArgb(45, 48, 55));
        dgvArmet.CellFormatting += DgvArmet_CellFormatting;
        dgvArmet.DataBindingComplete += (_, _) => FshihKolona(dgvArmet, new[] { "Id" });
        LidhKontekstDgv(dgvArmet, btnNdryshoArme_Click, btnFshiArme_Click);
        dgvArmet.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) btnNdryshoArme_Click(s, e); };

        StilizoGrid(dgvPersoneli, Color.FromArgb(45, 48, 55));
        dgvPersoneli.DataBindingComplete += (_, _) => FshihKolona(dgvPersoneli, new[] { "Id" });
        LidhKontekstDgv(dgvPersoneli, btnNdryshoPersonel_Click, btnFshiPersonel_Click);
        dgvPersoneli.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) btnNdryshoPersonel_Click(s, e); };

        StilizoGrid(dgvKlientet, Color.FromArgb(45, 48, 55));
        dgvKlientet.DataBindingComplete += (_, _) => FshihKolona(dgvKlientet, new[] { "Id" });
        LidhKontekstDgv(dgvKlientet, btnNdryshoKlient_Click, btnFshiKlient_Click);
        dgvKlientet.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) btnNdryshoKlient_Click(s, e); };

        StilizoGrid(dgvTransaksionet, Color.FromArgb(48, 45, 50));
        dgvTransaksionet.CellFormatting += DgvTransaksionet_CellFormatting;
        dgvTransaksionet.DataBindingComplete += (_, _) =>
        {
            FshihKolona(dgvTransaksionet, new[] { "Id", "ArmaId", "PersoneliId", "KlientiId" });
            VendosTituj(dgvTransaksionet, new Dictionary<string, string>
            {
                { "ArmaSerial", "Seriali" }, { "PersoneliEmri", "Stafi" }, { "KlientiEmri", "Klienti" },
                { "Tipi", "Tipi" }, { "DataOra", "Data/Ora" }, { "Qellimi", "Qellimi" },
                { "PersoneliQeDorzoi", "Dorzoi" }, { "PersoneliQeMorri", "Morri" }, { "Shenime", "Shenime" }
            });
            if (dgvTransaksionet.Columns["DataOra"] != null)
                dgvTransaksionet.Columns["DataOra"].MinimumWidth = 130;
        };
        LidhKontekstTransaksion(dgvTransaksionet);

        StilizoGrid(dgvGjendjaDeponimit, Color.FromArgb(42, 50, 48));
        dgvGjendjaDeponimit.DataBindingComplete += (_, _) =>
        {
            FshihKolona(dgvGjendjaDeponimit, new[] { "ArmaId" });
            VendosTituj(dgvGjendjaDeponimit, new Dictionary<string, string>
            {
                { "Seriali", "Seriali" }, { "Marka", "Marka" }, { "Modeli", "Modeli" },
                { "Lloji", "Lloji" }, { "Kalibri", "Kalibri" }, { "NrInventari", "Nr. Inventari" },
                { "Klienti", "Klienti" }, { "Stafi", "Stafi" }, { "DataHyrjes", "Data Hyrjes" },
                { "Qellimi", "Qellimi" }, { "Statusi", "Statusi" }
            });
        };

        if (_perdoruesi.Role == "Admin")
        {
            StilizoGrid(dgvPerdoruesit, Color.FromArgb(45, 48, 55));
            dgvPerdoruesit.DataBindingComplete += (_, _) => FshihKolona(dgvPerdoruesit, new[] { "Id", "Password" });
        }
    }

    private void StilizoGrid(DataGridView dgv, Color altColor)
    {
        dgv.BackgroundColor = Color.FromArgb(35, 37, 42);
        dgv.BorderStyle = BorderStyle.None;
        dgv.GridColor = Color.FromArgb(55, 58, 64);
        dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(40, 43, 50);
        dgv.DefaultCellStyle.Font = new Font("Segoe UI", 9);
        dgv.DefaultCellStyle.ForeColor = Color.FromArgb(210, 215, 225);
        dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(41, 128, 185);
        dgv.DefaultCellStyle.SelectionForeColor = Color.White;
        dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(20, 50, 75);
        dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(200, 225, 250);
        dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        dgv.ColumnHeadersHeight = 34; dgv.EnableHeadersVisualStyles = false;
        dgv.RowTemplate.Height = 32; dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgv.RowHeadersVisible = false; dgv.AllowUserToAddRows = false; dgv.ReadOnly = true;
    }

    private void LidhKontekstDgv(DataGridView dgv, EventHandler editHandler, EventHandler fshiHandler)
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add("Ndrysho", null, editHandler);
        menu.Items.Add("Fshi", null, fshiHandler);
        menu.ForeColor = Color.Black;
        dgv.ContextMenuStrip = menu;
    }

    private void LidhKontekstTransaksion(DataGridView dgv)
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add("Fshi Transaksion", null, (s, e) =>
        {
            if (!KaLeje("Fshi Transaksion"))
            { MessageBox.Show("Nuk keni leje.", "Ndaluar", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (!KerkoAutorizim("Fshi Transaksion"))
                return;
            if (dgv.SelectedRows.Count == 0) return;
            var idObj = dgv.SelectedRows[0].Cells["Id"]?.Value;
            if (idObj == null) return;
            int id = Convert.ToInt32(idObj);
            if (MessageBox.Show("Jeni te sigurt qe doni te fshini kete transaksion?",
                "Konfirmim", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            if (_db.FshiTransaksion(id, _perdoruesi.Username))
            {
                NgarkoTransaksionet(txtKerkimTransaksion.Text);
                NgarkoBallina();
            }
            else
                MessageBox.Show("Transaksioni nuk mund te fshihet.", "Gabim",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
        });
        menu.ForeColor = Color.Black;
        dgv.ContextMenuStrip = menu;
    }

    private void LidhKontekstBallina(DataGridView dgv, string tipi)
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add("Fshi Transaksion", null, (s, e) =>
        {
            if (!KaLeje("Fshi Transaksion"))
            { MessageBox.Show("Nuk keni leje.", "Ndaluar", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (!KerkoAutorizim("Fshi Transaksion"))
                return;
            if (dgv.SelectedRows.Count == 0) return;
            var serial = dgv.SelectedRows[0].Cells["Nr. Serial"]?.Value?.ToString() ?? "";
            var data = dgv.SelectedRows[0].Cells["Data"]?.Value?.ToString() ?? "";
            if (string.IsNullOrEmpty(serial) || string.IsNullOrEmpty(data)) return;
            foreach (DataRow row in _transaksionetTable.Rows)
            {
                var rowSerial = row["ArmaSerial"]?.ToString() ?? "";
                var rowData = row["DataOra"]?.ToString() ?? "";
                if (rowData.Length >= 10) rowData = rowData.Substring(0, 10).Replace("-", ".");
                var rowTipi = row["Tipi"]?.ToString() ?? "";
                if (rowSerial == serial && rowData == data && rowTipi == tipi)
                {
                    int id = Convert.ToInt32(row["Id"]);
                    if (MessageBox.Show("Jeni te sigurt qe doni te fshini kete transaksion?",
                        "Konfirmim", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                        return;
                    if (_db.FshiTransaksion(id, _perdoruesi.Username))
                    {
                        NgarkoTransaksionet(txtKerkimTransaksion.Text);
                        NgarkoBallina();
                    }
                    else
                        MessageBox.Show("Transaksioni nuk mund te fshihet.", "Gabim",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            MessageBox.Show("Transaksioni nuk u gjet.", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        });
        menu.ForeColor = Color.Black;
        dgv.ContextMenuStrip = menu;
    }

    private void StilizoSearchBoxes()
    {
        foreach (var tb in new[] { txtKerkimArme, txtKerkimTransaksion })
        {
            if (tb == null) continue;
            tb.BackColor = Color.FromArgb(55, 57, 63);
            tb.ForeColor = Color.FromArgb(200, 205, 215);
            tb.BorderStyle = BorderStyle.FixedSingle;
        }
    }

    private void FshihKolona(DataGridView dgv, string[] kolona)
    { foreach (var c in kolona) if (dgv.Columns.Contains(c)) dgv.Columns[c].Visible = false; }

    private void VendosTituj(DataGridView dgv, Dictionary<string, string> tituj)
    { foreach (var kvp in tituj) if (dgv.Columns.Contains(kvp.Key)) dgv.Columns[kvp.Key].HeaderText = kvp.Value; }

    private void DgvArmet_CellFormatting(object? s, DataGridViewCellFormattingEventArgs e)
    {
        if (dgvArmet.Columns[e.ColumnIndex].Name == "Statusi" && e.Value != null && e.CellStyle != null)
        {
            e.CellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            e.CellStyle.ForeColor = e.Value.ToString() == "Ne Magazine" ? Color.FromArgb(39, 174, 96)
                : e.Value.ToString() == "Tek Klienti" ? Color.FromArgb(230, 126, 34) : Color.FromArgb(192, 57, 43);
            e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }
    }

    private void DgvTransaksionet_CellFormatting(object? s, DataGridViewCellFormattingEventArgs e)
    {
        if (e.Value == null || e.CellStyle == null) return;
        if (dgvTransaksionet.Columns[e.ColumnIndex].Name == "Tipi")
        {
            e.CellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            if (e.Value.ToString() == "Hyrje")
            { e.CellStyle.ForeColor = Color.FromArgb(39, 174, 96); e.CellStyle.SelectionBackColor = Color.FromArgb(39, 174, 96); e.CellStyle.SelectionForeColor = Color.White; }
            else
            { e.CellStyle.ForeColor = Color.FromArgb(192, 57, 43); e.CellStyle.SelectionBackColor = Color.FromArgb(192, 57, 43); e.CellStyle.SelectionForeColor = Color.White; }
            e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }
        else if (dgvTransaksionet.Columns[e.ColumnIndex].Name == "DataOra" && e.Value is string dataOra && dataOra.Length >= 16)
        {
            e.Value = dataOra.Replace("-", ".").Substring(0, 16);
            e.FormattingApplied = true;
        }
    }

    private void StilizoPersoneliTab()
    {
        dgvPersoneli.Size = new Size(1135, 330);
        dgvPersoneli.SelectionChanged += (_, _) => NgarkoPersoneliDetail();

        _personeliDetailPanel = new Panel();
        _personeliDetailPanel.Size = new Size(1135, 240);
        _personeliDetailPanel.Location = new Point(12, 385);
        _personeliDetailPanel.BackColor = Color.FromArgb(35, 38, 45);
        _personeliDetailPanel.BorderStyle = BorderStyle.FixedSingle;

        lblPersoneliDetaje = new Label();
        lblPersoneliDetaje.Text = "\uD83D\uDC64 Zgjidh nje personel per te pare detajet";
        lblPersoneliDetaje.Font = new Font("Segoe UI", 10, FontStyle.Italic);
        lblPersoneliDetaje.ForeColor = Color.FromArgb(120, 128, 140);
        lblPersoneliDetaje.Size = new Size(500, 22);
        lblPersoneliDetaje.Location = new Point(15, 10);

        var lblTransTitle = new Label();
        lblTransTitle.Text = "\uD83D\uDCCB Historiku i transaksioneve (Hyrje/Dalje):";
        lblTransTitle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        lblTransTitle.ForeColor = Color.FromArgb(0, 200, 255);
        lblTransTitle.Size = new Size(500, 20);
        lblTransTitle.Location = new Point(15, 35);

        dgvPersoneliTrans = new DataGridView();
        dgvPersoneliTrans.Size = new Size(1095, 180);
        dgvPersoneliTrans.Location = new Point(15, 55);
        dgvPersoneliTrans.BackgroundColor = Color.FromArgb(30, 32, 37);
        dgvPersoneliTrans.ForeColor = Color.FromArgb(200, 205, 216);
        dgvPersoneliTrans.GridColor = Color.FromArgb(50, 52, 58);
        dgvPersoneliTrans.BorderStyle = BorderStyle.None;
        dgvPersoneliTrans.Font = new Font("Segoe UI", 9);
        dgvPersoneliTrans.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvPersoneliTrans.ReadOnly = true;
        dgvPersoneliTrans.AllowUserToAddRows = false;
        dgvPersoneliTrans.RowHeadersVisible = false;
        dgvPersoneliTrans.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvPersoneliTrans.EnableHeadersVisualStyles = false;
        dgvPersoneliTrans.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 70, 120);
        dgvPersoneliTrans.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgvPersoneliTrans.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        dgvPersoneliTrans.ColumnHeadersHeight = 26;
        dgvPersoneliTrans.RowsDefaultCellStyle.BackColor = Color.FromArgb(35, 38, 45);
        dgvPersoneliTrans.RowsDefaultCellStyle.ForeColor = Color.FromArgb(200, 205, 216);
        dgvPersoneliTrans.RowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 100, 160);
        dgvPersoneliTrans.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(40, 42, 50);

        _personeliDetailPanel.Controls.Add(lblPersoneliDetaje);
        _personeliDetailPanel.Controls.Add(lblTransTitle);
        _personeliDetailPanel.Controls.Add(dgvPersoneliTrans);
        tabPersoneli.Controls.Add(_personeliDetailPanel);
    }

    private Button BtnTab(string text, int w, Color c, EventHandler handler)
    {
        var b = new Button();
        b.Text = text; b.Size = new Size(w, 30); b.FlatStyle = FlatStyle.Flat;
        b.FlatAppearance.BorderSize = 0;
        b.BackColor = c; b.ForeColor = Color.White;
        b.Font = new Font("Segoe UI", 9, FontStyle.Bold); b.Cursor = Cursors.Hand;
        b.UseVisualStyleBackColor = false;
        b.Click += handler;
        return b;
    }
}
