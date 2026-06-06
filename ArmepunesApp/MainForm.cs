using System.Data;
using ArmepunesApp.Data;
using ArmepunesApp.Forms;
using ArmepunesApp.Models;
using ArmepunesApp.Services;

namespace ArmepunesApp;

public partial class MainForm : Form
{
    private readonly DatabaseHelper _db;
    private readonly Perdoruesi _perdoruesi;
    private DataTable _armetTable = new();
    private DataTable _personeliTable = new();
    private DataTable _klientetTable = new();
    private DataTable _transaksionetTable = new();
    private DataTable _perdoruesitTable = new();

    private Label _lblTotalArmet = null!;
    private Label _lblNeMagazine = null!;
    private Label _lblNePerdorim = null!;
    private Label _lblTransaksioneSot = null!;
    private Label _lblKliente = null!;
    private Panel _statsPanel = null!;
    private System.Windows.Forms.Timer _refreshTimer = null!;
    private Panel _personeliDetailPanel = null!;
    private DataGridView dgvPersoneliTrans = null!;
    private Label lblPersoneliDetaje = null!;
    private Panel _chartPanel = null!;
    private Label _lblChartPerqindje = null!;

    private static readonly List<Color> _chartColors = new()
    {
        Color.FromArgb(46, 204, 113), Color.FromArgb(231, 76, 60), Color.FromArgb(52, 152, 219),
        Color.FromArgb(155, 89, 182), Color.FromArgb(243, 156, 18), Color.FromArgb(26, 188, 156),
        Color.FromArgb(230, 126, 34), Color.FromArgb(142, 68, 173)
    };
    private DataGridView dgvBallinaHyrje = null!;
    private DataGridView dgvBallinaDalje = null!;
    private Label lblBallinaHyrjeCount = null!;
    private Label lblBallinaDaljeCount = null!;

    private StatusStrip _statusStrip = null!;
    private ToolStripStatusLabel _statusLabelUser = null!;
    private ToolStripStatusLabel _statusLabelRefresh = null!;
    private ToolStripStatusLabel _statusLabelDb = null!;

    public MainForm(DatabaseHelper db, Perdoruesi perdoruesi)
    {
        _db = db;
        _perdoruesi = perdoruesi;
        InitializeComponent();
        this.WindowState = FormWindowState.Maximized;
        StilizoFormularin();
        NgarkoArmet();
        NgarkoPersonelin();
        NgarkoKlientet();
        NgarkoTransaksionet();
        NgarkoGjendjenDeponimit();
        NgarkoBallina();
        if (_perdoruesi.Role == "Admin")
            NgarkoPerdoruesit();

        _refreshTimer = new System.Windows.Forms.Timer { Interval = 5000 };
        _refreshTimer.Tick += (_, _) =>
        {
            NgarkoArmet(txtKerkimArme.Text);
            NgarkoTransaksionet(txtKerkimTransaksion.Text);
            NgarkoGjendjenDeponimit();
            NgarkoBallina();
            if (tabControl.SelectedTab == tabHistoriku)
                NgarkoHistorikun(txtKerkimHistoriku.Text);
            _statusLabelRefresh.Text = $"\u2705 U rifreskua: {DateTime.Now:HH:mm:ss}";
        };
        _refreshTimer.Start();

        this.Shown += (_, _) =>
        {
            PerditesoStatistikat();
            NgarkoBallina();
        };

        this.Resize += (_, _) =>
        {
            if (tabControl.SelectedTab == tabBallina)
                NgarkoBallina();
        };
    }

    private void StilizoFormularin()
    {
        try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath) ?? SystemIcons.Application; } catch { }
        BackColor = Color.FromArgb(30, 30, 35);
        Text = $"DEPONIM I ARMEVE - Kycur: {_perdoruesi.Username} ({_perdoruesi.Role})";
        StilizoHeader();
        StilizoStatistikat();
        StilizoRibbon();
        StilizoTabControl();
        StilizoDataGridViews();
        StilizoSearchBoxes();
        StilizoPersoneliTab();

        this.KeyPreview = true;
        this.KeyDown += (s, ke) =>
        {
            if (ke.KeyCode == Keys.F5)
            {
                NgarkoArmet(txtKerkimArme.Text);
                NgarkoPersonelin();
                NgarkoKlientet();
                NgarkoTransaksionet(txtKerkimTransaksion.Text);
                NgarkoGjendjenDeponimit();
                NgarkoBallina();
                if (_perdoruesi.Role == "Admin")
                    NgarkoPerdoruesit();
                if (tabControl.SelectedTab == tabHistoriku)
                    NgarkoHistorikun(txtKerkimHistoriku.Text);
                _statusLabelRefresh.Text = $"\u2705 U rifreskua: {DateTime.Now:HH:mm:ss}";
                ke.SuppressKeyPress = true;
            }
        };

        if (_perdoruesi.Role != "Admin")
            tabControl.TabPages.Remove(tabAdmin);

        StilizoStatusStrip();
    }

    private bool KaLeje(string permission)
    {
        return _perdoruesi.Role == "Admin" || _db.KaLeje(_perdoruesi.Id, permission);
    }

    private bool KerkoAutorizim(string veprimi)
    {
        using var dlg = new PasswordDialog(_db, veprimi);
        return dlg.ShowDialog(this) == DialogResult.OK && dlg.Autorizuar;
    }

    // ============== BALLINA (Dashboard) ==============
    public void NgarkoBallina()
    {
        var tabW = Math.Max(tabBallina.ClientSize.Width, 800);
        int gap = 20;
        int gridW = (tabW - gap) / 2;
        int gridH = 280;
        int bottomY = gridH + 47;
        int rightX = gridW + gap;

        if (tabBallina.Controls.Count == 0)
        {
            tabBallina.SuspendLayout();

            var lblHyrje = new Label();
            lblHyrje.Text = "Deponimet e Fundit";
            lblHyrje.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            lblHyrje.ForeColor = Color.FromArgb(46, 204, 113);
            lblHyrje.Name = "lblHyrje";

            dgvBallinaHyrje = new DataGridView();
            dgvBallinaHyrje.Name = "dgvHyrje";
            StilizoDgvBallina(dgvBallinaHyrje);
            dgvBallinaHyrje.CellDoubleClick += BallinaHyrje_CellDoubleClick;
            LidhKontekstBallina(dgvBallinaHyrje, "Hyrje");

            var lblDalje = new Label();
            lblDalje.Text = "Terheqjet e Fundit";
            lblDalje.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            lblDalje.ForeColor = Color.FromArgb(231, 76, 60);
            lblDalje.Name = "lblDalje";

            dgvBallinaDalje = new DataGridView();
            dgvBallinaDalje.Name = "dgvDalje";
            StilizoDgvBallina(dgvBallinaDalje);
            dgvBallinaDalje.CellDoubleClick += BallinaDalje_CellDoubleClick;
            LidhKontekstBallina(dgvBallinaDalje, "Dalje");

            lblBallinaHyrjeCount = new Label();
            lblBallinaHyrjeCount.Font = new Font("Segoe UI", 8, FontStyle.Italic);
            lblBallinaHyrjeCount.ForeColor = Color.FromArgb(160, 168, 180);
            lblBallinaHyrjeCount.Name = "lblHyrjeCount";

            lblBallinaDaljeCount = new Label();
            lblBallinaDaljeCount.Font = new Font("Segoe UI", 8, FontStyle.Italic);
            lblBallinaDaljeCount.ForeColor = Color.FromArgb(160, 168, 180);
            lblBallinaDaljeCount.Name = "lblDaljeCount";

            var gjendjaPanel = new Panel();
            gjendjaPanel.Name = "gjendjaPanel";
            gjendjaPanel.BackColor = Color.FromArgb(35, 38, 45);
            gjendjaPanel.BorderStyle = BorderStyle.FixedSingle;
            gjendjaPanel.Paint += (_, e) =>
            {
                var gp = e.Graphics;
                gp.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                gp.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                int totalArmetP = 0, neMagazineP = 0, nePerdorimP = 0;
                try
                {
                    totalArmetP = _armetTable.Rows.Count;
                    neMagazineP = _armetTable.Select("Statusi = 'Ne Magazine'").Length;
                    nePerdorimP = _armetTable.Select("Statusi = 'Tek Klienti'").Length;
                }
                catch { System.Diagnostics.Debug.WriteLine("Gabim ne leximin e statistikave per pie chart"); }
                if (totalArmetP == 0) return;
                var pieRect = new Rectangle(gjendjaPanel.Width - 140, 10, 90, 90);
                float magPct = (float)neMagazineP / totalArmetP;
                float perdPct = (float)nePerdorimP / totalArmetP;
                float tjerePct = 1f - magPct - perdPct;
                float startAngle = -90f;
                using var magBrush = new SolidBrush(Color.FromArgb(46, 204, 113));
                using var perdBrush = new SolidBrush(Color.FromArgb(243, 156, 18));
                using var tjereBrush = new SolidBrush(Color.FromArgb(150, 150, 160));
                if (magPct > 0f) { float sweep = magPct * 360f; gp.FillPie(magBrush, pieRect, startAngle, sweep); startAngle += sweep; }
                if (perdPct > 0f) { float sweep = perdPct * 360f; gp.FillPie(perdBrush, pieRect, startAngle, sweep); startAngle += sweep; }
                if (tjerePct > 0f) gp.FillPie(tjereBrush, pieRect, startAngle, tjerePct * 360f);
                using var outlinePen = new Pen(Color.FromArgb(60, 62, 68), 1);
                gp.DrawEllipse(outlinePen, pieRect);
            };

            var lblGjendjaTitle = new Label();
            lblGjendjaTitle.Text = "PASQYRA E DEPONIMIT";
            lblGjendjaTitle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            lblGjendjaTitle.ForeColor = Color.FromArgb(200, 205, 215);
            lblGjendjaTitle.Location = new Point(15, 10);
            lblGjendjaTitle.Size = new Size(400, 26);
            lblGjendjaTitle.Name = "lblGjendjaTitle";

            var lblTot = new Label();
            lblTot.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            lblTot.ForeColor = Color.FromArgb(100, 140, 200);
            lblTot.Name = "lblTot";
            lblTot.Size = new Size(200, 28);

            var lblMag = new Label();
            lblMag.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            lblMag.ForeColor = Color.FromArgb(46, 204, 113);
            lblMag.Name = "lblMag";
            lblMag.Size = new Size(200, 28);

            var lblPerd = new Label();
            lblPerd.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            lblPerd.ForeColor = Color.FromArgb(243, 156, 18);
            lblPerd.Name = "lblPerd";
            lblPerd.Size = new Size(200, 28);

            var lblWeaponTypesTitle = new Label();
            lblWeaponTypesTitle.Text = "Shp\u00ebrndarja e arm\u00ebve sipas llojit";
            lblWeaponTypesTitle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblWeaponTypesTitle.ForeColor = Color.FromArgb(200, 205, 215);
            lblWeaponTypesTitle.Name = "lblWeaponTypesTitle";

            var weaponTypesPanel = new Panel();
            weaponTypesPanel.Name = "weaponTypesPanel";
            weaponTypesPanel.BackColor = Color.FromArgb(30, 30, 35);
            weaponTypesPanel.BorderStyle = BorderStyle.FixedSingle;
            weaponTypesPanel.Paint += VizatoGrafikunLlojeve;

            gjendjaPanel.Controls.Add(lblGjendjaTitle);
            gjendjaPanel.Controls.Add(lblTot);
            gjendjaPanel.Controls.Add(lblMag);
            gjendjaPanel.Controls.Add(lblPerd);

            tabBallina.Controls.AddRange(new Control[] {
                lblHyrje, dgvBallinaHyrje, lblBallinaHyrjeCount,
                lblDalje, dgvBallinaDalje, lblBallinaDaljeCount,
                gjendjaPanel, lblWeaponTypesTitle, weaponTypesPanel
            });

            tabBallina.ResumeLayout(false);
            tabBallina.PerformLayout();
        }

        tabBallina.SuspendLayout();
        var ctlHyrje = tabBallina.Controls["lblHyrje"];
        var ctlDgvHyrje = tabBallina.Controls["dgvHyrje"];
        var ctlHyrjeCount = tabBallina.Controls["lblHyrjeCount"];
        var ctlDalje = tabBallina.Controls["lblDalje"];
        var ctlDgvDalje = tabBallina.Controls["dgvDalje"];
        var ctlDaljeCount = tabBallina.Controls["lblDaljeCount"];
        var ctlGjendja = tabBallina.Controls["gjendjaPanel"];
        var ctlWeaponTitle = tabBallina.Controls["lblWeaponTypesTitle"];
        var ctlWeaponPanel = tabBallina.Controls["weaponTypesPanel"];
        if (ctlHyrje != null) ((Label)ctlHyrje).SetBounds(0, 10, gridW, 26);
        if (ctlDgvHyrje != null) ctlDgvHyrje.SetBounds(0, 42, gridW, gridH);
        if (ctlHyrjeCount != null) ((Label)ctlHyrjeCount).SetBounds(0, bottomY, gridW, 18);
        if (ctlDalje != null) ((Label)ctlDalje).SetBounds(rightX, 10, gridW, 26);
        if (ctlDgvDalje != null) ctlDgvDalje.SetBounds(rightX, 42, gridW, gridH);
        if (ctlDaljeCount != null) ((Label)ctlDaljeCount).SetBounds(rightX, bottomY, gridW, 18);
        if (ctlGjendja != null) ctlGjendja.SetBounds(0, bottomY + 25, tabW, 100);
        if (ctlWeaponTitle != null) ((Label)ctlWeaponTitle).SetBounds(0, bottomY + 135, tabW, 20);
        if (ctlWeaponPanel != null) ctlWeaponPanel.SetBounds(0, bottomY + 155, tabW, 220);

        if (ctlGjendja != null)
        {
            var gjPanel = (Panel)ctlGjendja;
            var ctlGjLblTot = gjPanel.Controls["lblTot"];
            var ctlGjLblMag = gjPanel.Controls["lblMag"];
            var ctlGjLblPerd = gjPanel.Controls["lblPerd"];
            if (ctlGjLblTot != null) ((Label)ctlGjLblTot).SetBounds(30, 45, 200, 22);
            if (ctlGjLblMag != null) ((Label)ctlGjLblMag).SetBounds(230, 45, 200, 22);
            if (ctlGjLblPerd != null) ((Label)ctlGjLblPerd).SetBounds(430, 45, 200, 22);
        }
        tabBallina.ResumeLayout(false);
        tabBallina.PerformLayout();

        int totalArmet = 0, neMagazine = 0, nePerdorim = 0;
        try
        {
            totalArmet = _armetTable.Rows.Count;
            neMagazine = _armetTable.Select("Statusi = 'Ne Magazine'").Length;
            nePerdorim = _armetTable.Select("Statusi = 'Tek Klienti'").Length;
        }
        catch { System.Diagnostics.Debug.WriteLine("Gabim ne leximin e statistikave per Ballina"); }

        var gjendjaPanel2 = tabBallina.Controls["gjendjaPanel"] as Panel;
        if (gjendjaPanel2 != null)
        {
            var ctlTot = gjendjaPanel2.Controls["lblTot"] as Label;
            var ctlMag = gjendjaPanel2.Controls["lblMag"] as Label;
            var ctlPerd = gjendjaPanel2.Controls["lblPerd"] as Label;
            if (ctlTot != null) ctlTot.Text = $"Total Arme: {totalArmet}";
            if (ctlMag != null) ctlMag.Text = $"Ne Magazine: {neMagazine}";
            if (ctlPerd != null) ctlPerd.Text = $"Tek Klienti: {nePerdorim}";
        }

        try
        {
            var hyrjeDt = new DataTable();
            hyrjeDt.Columns.Add("Data", typeof(string));
            hyrjeDt.Columns.Add("Nr. Serial", typeof(string));
            hyrjeDt.Columns.Add("Klienti", typeof(string));
            var daljeDt = new DataTable();
            daljeDt.Columns.Add("Data", typeof(string));
            daljeDt.Columns.Add("Nr. Serial", typeof(string));
            daljeDt.Columns.Add("Klienti", typeof(string));
            int nrH = 0, nrD = 0, maxBallina = 15;
            foreach (DataRow row in _transaksionetTable.Rows)
            {
                string tipi = Convert.ToString(row["Tipi"]) ?? "";
                string dataOra = Convert.ToString(row["DataOra"]) ?? "";
                string data = dataOra.Length >= 10 ? dataOra.Substring(0, 10).Replace("-", ".") : dataOra;
                string armaSerial = Convert.ToString(row["ArmaSerial"]) ?? "";
                string klienti = Convert.ToString(row["KlientiEmri"]) ?? "";
                if (tipi == "Hyrje" && nrH < maxBallina) { hyrjeDt.Rows.Add(data, armaSerial, klienti); nrH++; }
                else if (tipi == "Dalje" && nrD < maxBallina) { daljeDt.Rows.Add(data, armaSerial, klienti); nrD++; }
                if (nrH >= maxBallina && nrD >= maxBallina) break;
            }
            dgvBallinaHyrje.DataSource = hyrjeDt;
            dgvBallinaDalje.DataSource = daljeDt;
            if (dgvBallinaHyrje.Columns.Count > 0)
            {
                dgvBallinaHyrje.Columns["Data"].FillWeight = 20;
                dgvBallinaHyrje.Columns["Nr. Serial"].FillWeight = 45;
                dgvBallinaHyrje.Columns["Klienti"].FillWeight = 35;
            }
            if (dgvBallinaDalje.Columns.Count > 0)
            {
                dgvBallinaDalje.Columns["Data"].FillWeight = 20;
                dgvBallinaDalje.Columns["Nr. Serial"].FillWeight = 45;
                dgvBallinaDalje.Columns["Klienti"].FillWeight = 35;
            }
            lblBallinaHyrjeCount.Text = $"Deponime: {nrH} \u2014 Duke treguar me te fundit";
            lblBallinaDaljeCount.Text = $"Terheqje: {nrD} \u2014 Duke treguar me te fundit";
        }
        catch { System.Diagnostics.Debug.WriteLine("Gabim ne ngarkimin e Ballina data"); }
    }

    private void BallinaHyrje_CellDoubleClick(object? s, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0) return;
        var serial = dgvBallinaHyrje.Rows[e.RowIndex].Cells["Nr. Serial"]?.Value?.ToString();
        if (!string.IsNullOrEmpty(serial))
            new DitariArmeForm(_db, serial, _perdoruesi.Username).ShowDialog();
    }

    private void BallinaDalje_CellDoubleClick(object? s, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0) return;
        var serial = dgvBallinaDalje.Rows[e.RowIndex].Cells["Nr. Serial"]?.Value?.ToString();
        if (!string.IsNullOrEmpty(serial))
            new DitariArmeForm(_db, serial, _perdoruesi.Username).ShowDialog();
    }

    // ============== WEAPON TYPES PIE CHART ==============
    private void VizatoGrafikunLlojeve(object? s, PaintEventArgs e)
    {
        var panel = (Panel)s!;
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        var weaponTypes = new Dictionary<string, int>();
        try
        {
            foreach (DataRow row in _armetTable.Rows)
            {
                string lloji = Convert.ToString(row["Lloji"]) ?? "Unknown";
                if (weaponTypes.ContainsKey(lloji)) weaponTypes[lloji]++;
                else weaponTypes[lloji] = 1;
            }
        }
        catch { System.Diagnostics.Debug.WriteLine("Gabim ne leximin e llojeve te armeve"); }

        if (weaponTypes.Count == 0)
        {
            using var font = new Font("Segoe UI", 9, FontStyle.Italic);
            using var brush = new SolidBrush(Color.FromArgb(100, 105, 115));
            g.DrawString("Nuk ka te dhena per llojet e armave", font, brush, 10, 10);
            return;
        }

        int total = weaponTypes.Values.Sum();
        float startAngle = -90f;
        int diameter = Math.Min(panel.Width, panel.Height) - 20;
        int x = (panel.Width - diameter) / 2;
        int y = (panel.Height - diameter) / 2;
        var pieRect = new Rectangle(x, y, diameter, diameter);

        int colorIndex = 0;
        foreach (var kvp in weaponTypes)
        {
            float sweep = (float)kvp.Value / total * 360f;
            using var brush = new SolidBrush(_chartColors[colorIndex % _chartColors.Count]);
            g.FillPie(brush, pieRect, startAngle, sweep);
            string label = $"{kvp.Key}: {kvp.Value} ({(float)kvp.Value / total * 100:F0}%)";
            using var font = new Font("Segoe UI", 8);
            using var textBrush = new SolidBrush(Color.White);
            float labelAngle = startAngle + sweep / 2;
            float labelRadius = diameter / 2 + 20;
            float labelX = x + diameter / 2 + (float)Math.Cos(labelAngle * Math.PI / 180) * labelRadius - 40;
            float labelY = y + diameter / 2 + (float)Math.Sin(labelAngle * Math.PI / 180) * labelRadius - 10;
            g.DrawString(label, font, textBrush, labelX, labelY);
            startAngle += sweep;
            colorIndex++;
        }
        using var outlinePen = new Pen(Color.FromArgb(60, 62, 68), 1);
        g.DrawEllipse(outlinePen, pieRect);
    }

    private void StilizoDgvBallina(DataGridView dgv)
    {
        dgv.BackgroundColor = Color.FromArgb(30, 32, 37);
        dgv.ForeColor = Color.FromArgb(200, 205, 216);
        dgv.GridColor = Color.FromArgb(50, 52, 58);
        dgv.BorderStyle = BorderStyle.FixedSingle;
        dgv.Font = new Font("Segoe UI", 9);
        dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgv.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
        dgv.ReadOnly = true;
        dgv.AllowUserToAddRows = false;
        dgv.AllowUserToDeleteRows = false;
        dgv.RowHeadersVisible = false;
        dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgv.EnableHeadersVisualStyles = false;
        dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 80, 140);
        dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        dgv.ColumnHeadersHeight = 30;
        dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        dgv.RowsDefaultCellStyle.BackColor = Color.FromArgb(35, 38, 45);
        dgv.RowsDefaultCellStyle.ForeColor = Color.FromArgb(200, 205, 216);
        dgv.RowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 100, 160);
        dgv.RowsDefaultCellStyle.SelectionForeColor = Color.White;
        dgv.RowsDefaultCellStyle.Padding = new Padding(4, 2, 4, 2);
        dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(40, 42, 50);
        dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
    }

    // ============== STATUS STRIP ==============
    private void StilizoStatusStrip()
    {
        _statusStrip = new StatusStrip
        {
            BackColor = Color.FromArgb(25, 27, 32),
            ForeColor = Color.FromArgb(180, 185, 195),
            Font = new Font("Segoe UI", 9),
            SizingGrip = false,
            Padding = new Padding(8, 2, 8, 2),
            ShowItemToolTips = true
        };

        _statusLabelUser = new ToolStripStatusLabel
        {
            Text = $"\uD83D\uDC64 {_perdoruesi.Username} ({_perdoruesi.Role})",
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = Color.FromArgb(100, 180, 255),
            BorderSides = ToolStripStatusLabelBorderSides.Right,
            BorderStyle = Border3DStyle.Etched,
            Padding = new Padding(4, 0, 12, 0)
        };

        _statusLabelRefresh = new ToolStripStatusLabel
        {
            Text = "Shtyp F5 per te rifreskuar",
            Font = new Font("Segoe UI", 9),
            ForeColor = Color.FromArgb(160, 165, 175),
            BorderSides = ToolStripStatusLabelBorderSides.Right,
            BorderStyle = Border3DStyle.Etched,
            Padding = new Padding(8, 0, 12, 0)
        };

        _statusLabelDb = new ToolStripStatusLabel
        {
            Text = $"DB Schema: v{_db.MerrSchemaVersion()}",
            Font = new Font("Segoe UI", 9),
            ForeColor = Color.FromArgb(140, 145, 155),
            Spring = true,
            TextAlign = ContentAlignment.MiddleRight
        };

        var btnAutoRefresh = new ToolStripButton
        {
            Text = "Auto",
            Font = new Font("Segoe UI", 8, FontStyle.Bold),
            ForeColor = Color.FromArgb(46, 204, 113),
            DisplayStyle = ToolStripItemDisplayStyle.Text,
            Padding = new Padding(4, 0, 4, 0),
            ToolTipText = "Aktiv/Caktivizo rifreskimin automatik"
        };
        btnAutoRefresh.Click += (_, _) =>
        {
            _refreshTimer.Enabled = !_refreshTimer.Enabled;
            btnAutoRefresh.ForeColor = _refreshTimer.Enabled
                ? Color.FromArgb(46, 204, 113)
                : Color.FromArgb(160, 80, 60);
            _statusLabelRefresh.Text = _refreshTimer.Enabled
                ? "Auto-Refresh ON"
                : "Auto-Refresh OFF";
        };

        _statusStrip.Items.Add(_statusLabelUser);
        _statusStrip.Items.Add(_statusLabelRefresh);
        _statusStrip.Items.Add(btnAutoRefresh);
        _statusStrip.Items.Add(_statusLabelDb);
        Controls.Add(_statusStrip);
    }

    // ============== MBYLLJE E SIGURT ==============
    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (e.CloseReason == CloseReason.UserClosing)
        {
            var result = MessageBox.Show("Jeni te sigurt qe doni te dilni nga aplikacioni?",
                "Konfirmim", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes)
                e.Cancel = true;
        }
        _refreshTimer?.Stop();
        base.OnFormClosing(e);
    }
}
