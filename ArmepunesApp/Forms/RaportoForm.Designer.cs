namespace ArmepunesApp.Forms;

partial class RaportoForm
{
    private System.ComponentModel.IContainer components = null;
    private TabControl tabReports;
    private TabPage tabStoku;
    private TabPage tabDaljet;
    private TabPage tabAnalitik;
    private TabPage tabArmetKlient;
    private TabPage tabListaArmet;
    private TabPage tabListaKliente;
    private TabPage tabListaPersonel;
    private DataGridView dgvStoku;
    private DataGridView dgvDaljet;
    private DataGridView dgvAnalitik;
    private DataGridView dgvArmetKlient;
    private DataGridView dgvListaArmet;
    private DataGridView dgvListaKliente;
    private DataGridView dgvListaPersonel;
    private TextBox txtKerkimSerial;
    private Button btnKerko;
    private Button btnPrintStoku;
    private Button btnPrintDaljet;
    private Button btnPrintAnalitik;
    private Button btnMbyll;
    private ComboBox cmbPrinter;
    private Panel panelSearch;
    private Panel panelFooter;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.Text = "Raporte - Deponim i Armeve";
        this.Size = new Size(1200, 750);
        this.StartPosition = FormStartPosition.CenterParent;
        this.BackColor = Color.FromArgb(30, 32, 37);
        this.ForeColor = Color.FromArgb(200, 205, 216);
        this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath) ?? SystemIcons.Application;
        this.Font = new Font("Segoe UI", 9);

        // Search Panel
        panelSearch = new Panel();
        panelSearch.Size = new Size(1180, 55);
        panelSearch.Location = new Point(10, 10);
        panelSearch.BackColor = Color.FromArgb(35, 38, 45);

        txtKerkimSerial = new TextBox();
        txtKerkimSerial.Size = new Size(260, 26);
        txtKerkimSerial.Location = new Point(15, 15);
        txtKerkimSerial.BackColor = Color.FromArgb(40, 42, 48);
        txtKerkimSerial.ForeColor = Color.FromArgb(200, 205, 216);
        txtKerkimSerial.BorderStyle = BorderStyle.FixedSingle;
        txtKerkimSerial.Font = new Font("Segoe UI", 10);
        txtKerkimSerial.Text = "Kerkim nga Nr. Serik...";
        txtKerkimSerial.Enter += (s, e) => { if (txtKerkimSerial.Text == "Kerkim nga Nr. Serik...") txtKerkimSerial.Text = ""; };
        txtKerkimSerial.Leave += (s, e) => { if (txtKerkimSerial.Text == "") txtKerkimSerial.Text = "Kerkim nga Nr. Serik..."; };

        btnKerko = new Button();
        btnKerko.Text = "Kerko";
        btnKerko.Size = new Size(90, 28);
        btnKerko.Location = new Point(285, 14);
        btnKerko.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        btnKerko.BackColor = Color.FromArgb(0, 120, 200);
        btnKerko.ForeColor = Color.White;
        btnKerko.FlatStyle = FlatStyle.Flat;
        btnKerko.Cursor = Cursors.Hand;
        btnKerko.Click += btnKerko_Click;

        var lblFilter = new Label();
        lblFilter.Text = "Tipi:"; lblFilter.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        lblFilter.ForeColor = Color.FromArgb(160, 168, 180);
        lblFilter.Location = new Point(390, 16); lblFilter.Size = new Size(40, 24);

        cmbFilterTipi = new ComboBox();
        cmbFilterTipi.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbFilterTipi.Items.AddRange(new object[] { "Te gjitha", "Hyrje", "Dalje" });
        cmbFilterTipi.SelectedIndex = 0;
        cmbFilterTipi.Location = new Point(430, 14);
        cmbFilterTipi.Size = new Size(100, 26);
        cmbFilterTipi.BackColor = Color.FromArgb(40, 42, 48);
        cmbFilterTipi.ForeColor = Color.FromArgb(200, 205, 216);
        cmbFilterTipi.FlatStyle = FlatStyle.Flat;
        cmbFilterTipi.Font = new Font("Segoe UI", 9);

        var lblPrej = new Label();
        lblPrej.Text = "Prej:"; lblPrej.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        lblPrej.ForeColor = Color.FromArgb(160, 168, 180);
        lblPrej.Location = new Point(545, 16); lblPrej.Size = new Size(35, 24);

        dtpPrej = new DateTimePicker();
        dtpPrej.Format = DateTimePickerFormat.Short;
        dtpPrej.Value = DateTime.Now.AddMonths(-1);
        dtpPrej.Location = new Point(580, 14);
        dtpPrej.Size = new Size(110, 26);
        dtpPrej.BackColor = Color.FromArgb(40, 42, 48);
        dtpPrej.ForeColor = Color.FromArgb(200, 205, 216);
        dtpPrej.Font = new Font("Segoe UI", 9);

        var lblDeri = new Label();
        lblDeri.Text = "Deri:"; lblDeri.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        lblDeri.ForeColor = Color.FromArgb(160, 168, 180);
        lblDeri.Location = new Point(700, 16); lblDeri.Size = new Size(35, 24);

        dtpDeri = new DateTimePicker();
        dtpDeri.Format = DateTimePickerFormat.Short;
        dtpDeri.Value = DateTime.Now;
        dtpDeri.Location = new Point(735, 14);
        dtpDeri.Size = new Size(110, 26);
        dtpDeri.BackColor = Color.FromArgb(40, 42, 48);
        dtpDeri.ForeColor = Color.FromArgb(200, 205, 216);
        dtpDeri.Font = new Font("Segoe UI", 9);

        var btnFiltro = new Button();
        btnFiltro.Text = "Filtro";
        btnFiltro.Size = new Size(80, 28);
        btnFiltro.Location = new Point(860, 14);
        btnFiltro.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        btnFiltro.BackColor = Color.FromArgb(46, 204, 113);
        btnFiltro.ForeColor = Color.White;
        btnFiltro.FlatStyle = FlatStyle.Flat;
        btnFiltro.Cursor = Cursors.Hand;
        btnFiltro.Click += (_, _) => { try { NgarkoStokun(); NgarkoDaljet(); NgarkoArmetTekKlienti(); NgarkoListenArmet(); NgarkoListenKliente(); NgarkoListenPersonel(); } catch { } };

        panelSearch.Controls.Add(txtKerkimSerial);
        panelSearch.Controls.Add(btnKerko);
        panelSearch.Controls.Add(lblFilter);
        panelSearch.Controls.Add(cmbFilterTipi);
        panelSearch.Controls.Add(lblPrej);
        panelSearch.Controls.Add(dtpPrej);
        panelSearch.Controls.Add(lblDeri);
        panelSearch.Controls.Add(dtpDeri);
        panelSearch.Controls.Add(btnFiltro);

        // Tab Control
        tabReports = new TabControl();
        tabReports.Size = new Size(1180, 600);
        tabReports.Location = new Point(10, 70);
        tabReports.Font = new Font("Segoe UI", 9);
        tabReports.BackColor = Color.FromArgb(35, 38, 45);
        tabReports.ForeColor = Color.FromArgb(200, 205, 216);
        tabReports.Multiline = true;
        tabReports.SizeMode = TabSizeMode.Normal;

        // Tab Stoku
        tabStoku = new TabPage("Gjendja e Stokut");
        tabStoku.BackColor = Color.FromArgb(30, 32, 37);
        dgvStoku = KrijoDGV();
        dgvStoku.Dock = DockStyle.Fill;
        tabStoku.Controls.Add(dgvStoku);

        // Tab Daljet
        tabDaljet = new TabPage("Lista e Daljeve");
        tabDaljet.BackColor = Color.FromArgb(30, 32, 37);
        dgvDaljet = KrijoDGV();
        dgvDaljet.Dock = DockStyle.Fill;
        tabDaljet.Controls.Add(dgvDaljet);

        // Tab Analitik
        tabAnalitik = new TabPage("Raport Analitik");
        tabAnalitik.BackColor = Color.FromArgb(30, 32, 37);
        dgvAnalitik = KrijoDGV();
        dgvAnalitik.Dock = DockStyle.Fill;
        tabAnalitik.Controls.Add(dgvAnalitik);

        // Tab Armet Tek Klienti
        tabArmetKlient = new TabPage("Armet Tek Klienti");
        tabArmetKlient.BackColor = Color.FromArgb(30, 32, 37);
        dgvArmetKlient = KrijoDGV();
        dgvArmetKlient.Dock = DockStyle.Fill;
        tabArmetKlient.Controls.Add(dgvArmetKlient);

        // Tab Lista e Armeve
        tabListaArmet = new TabPage("Lista e Armeve");
        tabListaArmet.BackColor = Color.FromArgb(30, 32, 37);
        dgvListaArmet = KrijoDGV();
        dgvListaArmet.Dock = DockStyle.Fill;
        tabListaArmet.Controls.Add(dgvListaArmet);

        // Tab Lista e Klienteve
        tabListaKliente = new TabPage("Lista e Klienteve");
        tabListaKliente.BackColor = Color.FromArgb(30, 32, 37);
        dgvListaKliente = KrijoDGV();
        dgvListaKliente.Dock = DockStyle.Fill;
        tabListaKliente.Controls.Add(dgvListaKliente);

        // Tab Lista e Personelit
        tabListaPersonel = new TabPage("Lista e Personelit");
        tabListaPersonel.BackColor = Color.FromArgb(30, 32, 37);
        dgvListaPersonel = KrijoDGV();
        dgvListaPersonel.Dock = DockStyle.Fill;
        tabListaPersonel.Controls.Add(dgvListaPersonel);

        tabReports.Controls.Add(tabStoku);
        tabReports.Controls.Add(tabDaljet);
        tabReports.Controls.Add(tabAnalitik);
        tabReports.Controls.Add(tabArmetKlient);
        tabReports.Controls.Add(tabListaArmet);
        tabReports.Controls.Add(tabListaKliente);
        tabReports.Controls.Add(tabListaPersonel);

        // Footer panel
        panelFooter = new Panel();
        panelFooter.Size = new Size(1180, 45);
        panelFooter.Location = new Point(10, 675);
        panelFooter.BackColor = Color.FromArgb(35, 38, 45);

        var lblPrinter = new Label();
        lblPrinter.Text = "Printer:";
        lblPrinter.Font = new Font("Segoe UI", 9);
        lblPrinter.ForeColor = Color.FromArgb(160, 168, 180);
        lblPrinter.Location = new Point(10, 12);
        lblPrinter.Size = new Size(55, 22);

        cmbPrinter = new ComboBox();
        cmbPrinter.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbPrinter.Location = new Point(65, 10);
        cmbPrinter.Size = new Size(200, 26);
        cmbPrinter.BackColor = Color.FromArgb(40, 42, 48);
        cmbPrinter.ForeColor = Color.FromArgb(200, 205, 216);
        cmbPrinter.FlatStyle = FlatStyle.Flat;
        cmbPrinter.Font = new Font("Segoe UI", 9);

        btnPrintStoku = Btn("Stoku", 70, Color.FromArgb(52, 152, 219), (s, e) => PrintoStokun(true));
        btnPrintStoku.Location = new Point(275, 8);

        btnPrintDaljet = Btn("Daljet", 70, Color.FromArgb(155, 89, 182), (s, e) => PrintoDaljet(true));
        btnPrintDaljet.Location = new Point(350, 8);

        btnPrintAnalitik = Btn("Analitik", 75, Color.FromArgb(46, 204, 113), (s, e) => PrintoAnalitik(true));
        btnPrintAnalitik.Location = new Point(430, 8);

        var btnArmetKlient = Btn("Armet Klient", 95, Color.FromArgb(230, 126, 34), (s, e) => PrintoArmetKlient(true));
        btnArmetKlient.Location = new Point(510, 8);

        var btnListaArmet = Btn("Lista Arme", 80, Color.FromArgb(26, 188, 156), (s, e) => PrintoListaArmet(true));
        btnListaArmet.Location = new Point(610, 8);

        var btnListaKliente = Btn("Klientet", 75, Color.FromArgb(155, 89, 182), (s, e) => PrintoListaKliente(true));
        btnListaKliente.Location = new Point(695, 8);

        var btnListaPersonel = Btn("Personeli", 80, Color.FromArgb(52, 152, 219), (s, e) => PrintoListaPersonel(true));
        btnListaPersonel.Location = new Point(780, 8);

        btnMbyll = Btn("Mbyll", 100, Color.FromArgb(80, 85, 95), (s, e) => Close());
        btnMbyll.Location = new Point(870, 8);

        panelFooter.Controls.Add(lblPrinter);
        panelFooter.Controls.Add(cmbPrinter);
        panelFooter.Controls.Add(btnPrintStoku);
        panelFooter.Controls.Add(btnPrintDaljet);
        panelFooter.Controls.Add(btnPrintAnalitik);
        panelFooter.Controls.Add(btnArmetKlient);
        panelFooter.Controls.Add(btnListaArmet);
        panelFooter.Controls.Add(btnListaKliente);
        panelFooter.Controls.Add(btnListaPersonel);
        panelFooter.Controls.Add(btnMbyll);

        Controls.Add(panelSearch);
        Controls.Add(tabReports);
        Controls.Add(panelFooter);
    }

    private DataGridView KrijoDGV()
    {
        var dgv = new DataGridView();
        dgv.BackgroundColor = Color.FromArgb(30, 32, 37);
        dgv.ForeColor = Color.FromArgb(200, 205, 216);
        dgv.GridColor = Color.FromArgb(50, 52, 58);
        dgv.BorderStyle = BorderStyle.None;
        dgv.Font = new Font("Segoe UI", 9);
        dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
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
        dgv.RowsDefaultCellStyle.BackColor = Color.FromArgb(35, 38, 45);
        dgv.RowsDefaultCellStyle.ForeColor = Color.FromArgb(200, 205, 216);
        dgv.RowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 100, 160);
        dgv.RowsDefaultCellStyle.SelectionForeColor = Color.White;
        dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(40, 42, 50);
        return dgv;
    }

    private Button Btn(string text, int w, Color c, EventHandler h)
    {
        var b = new Button();
        b.Text = text; b.Size = new Size(w, 30); b.FlatStyle = FlatStyle.Flat;
        b.FlatAppearance.BorderSize = 0;
        b.BackColor = c; b.ForeColor = Color.White;
        b.Font = new Font("Segoe UI", 9, FontStyle.Bold); b.Cursor = Cursors.Hand;
        b.UseVisualStyleBackColor = false;
        b.Click += h;
        return b;
    }
}
