namespace ArmepunesApp;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private TabControl tabControl;
    private TabPage tabBallina;
    private TabPage tabArmet;
    private TabPage tabPersoneli;
    private TabPage tabKlientet;
    private TabPage tabTransaksionet;
    private TabPage tabGjendja;
    private TabPage tabHistoriku;
    private TabPage tabAdmin;

    private DataGridView dgvArmet;
    private TextBox txtKerkimArme;
    private Label lblArmetCount;
    private Label label1;
    private Button btnShtoArme;
    private Button btnNdryshoArme;
    private Button btnFshiArme;
    private Button btnBatchArme;

    private DataGridView dgvPersoneli;
    private Label lblPersoneliCount;
    private Button btnShtoPersonel;
    private Button btnNdryshoPersonel;
    private Button btnFshiPersonel;

    private DataGridView dgvTransaksionet;
    private TextBox txtKerkimTransaksion;
    private Label lblTransaksionetCount;
    private Label label2;

    private DataGridView dgvKlientet;
    private Label lblKlientetCount;
    private Button btnShtoKlient;
    private Button btnNdryshoKlient;
    private Button btnFshiKlient;

    private DataGridView dgvGjendjaDeponimit;
    private Label lblGjendjaCount;

    private DataGridView dgvPerdoruesit;
    private DataGridView dgvHistoriku = null!;
    private TextBox txtKerkimHistoriku = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        tabControl = new TabControl();
        tabBallina = new TabPage();
        tabPersoneli = new TabPage();
        tabKlientet = new TabPage();
        tabArmet = new TabPage();
        tabTransaksionet = new TabPage();
        tabGjendja = new TabPage();
        tabHistoriku = new TabPage();
        tabAdmin = new TabPage();
        dgvArmet = new DataGridView();
        dgvPersoneli = new DataGridView();
        dgvTransaksionet = new DataGridView();
        SuspendLayout();

        // MainForm
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1200, 750);
        MinimumSize = new Size(950, 550);
        Text = "DEPONIM I ARMEVE";
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(30, 30, 35);

        // tabControl
        tabControl.Dock = DockStyle.Fill;
        tabControl.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        tabControl.Padding = new Point(20, 8);
        tabControl.BackColor = Color.FromArgb(50, 52, 58);
        tabControl.Controls.Add(tabBallina);
        tabControl.Controls.Add(tabPersoneli);
        tabControl.Controls.Add(tabKlientet);
        tabControl.Controls.Add(tabArmet);
        tabControl.Controls.Add(tabTransaksionet);
        tabControl.Controls.Add(tabGjendja);
        tabControl.Controls.Add(tabHistoriku);
        tabControl.Controls.Add(tabAdmin);

        // ========== TAB BALLINA ==========
        tabBallina.Text = "  Ballina  ";
        tabBallina.Padding = new Padding(12);
        tabBallina.BackColor = Color.FromArgb(30, 30, 35);
        tabBallina.AutoScroll = true;

        // ========== TAB ARMET ==========
        tabArmet.Text = "  Armet  ";
        tabArmet.Padding = new Padding(12);

        label1 = new Label(); label1.Text = "Kerko:"; label1.Font = new Font("Segoe UI", 10);
        label1.Location = new Point(310, 12); label1.Size = new Size(50, 28); label1.ForeColor = Color.FromArgb(180, 185, 195);

        txtKerkimArme = new TextBox();
        txtKerkimArme.Location = new Point(360, 12);
        txtKerkimArme.Size = new Size(250, 23);
        txtKerkimArme.TextChanged += txtKerkimArme_TextChanged;

        lblArmetCount = new Label();
        lblArmetCount.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        lblArmetCount.ForeColor = Color.FromArgb(180, 185, 195);
        lblArmetCount.Location = new Point(12, 42);
        lblArmetCount.Size = new Size(400, 22);

        btnShtoArme = BtnTab("+ Shto Arme", 90, Color.FromArgb(46, 204, 113), btnShtoArme_Click);
        btnShtoArme.Location = new Point(12, 8);
        btnNdryshoArme = BtnTab("Ndrysho", 85, Color.FromArgb(52, 152, 219), btnNdryshoArme_Click);
        btnNdryshoArme.Location = new Point(107, 8);
        btnFshiArme = BtnTab("Fshi", 70, Color.FromArgb(231, 76, 60), btnFshiArme_Click);
        btnFshiArme.Location = new Point(197, 8);
        btnBatchArme = BtnTab("⬆ Batch", 78, Color.FromArgb(155, 89, 182), btnBatchArme_Click);
        btnBatchArme.Location = new Point(272, 8);

        dgvArmet.Location = new Point(12, 65);
        dgvArmet.Size = new Size(1135, 575);

        tabArmet.Controls.AddRange(new Control[] {
            label1, txtKerkimArme, lblArmetCount, dgvArmet, btnShtoArme, btnNdryshoArme, btnFshiArme, btnBatchArme
        });

        // ========== TAB PERSONELI ==========
        tabPersoneli.Text = "  Personeli  ";
        tabPersoneli.Padding = new Padding(12);

        lblPersoneliCount = new Label();
        lblPersoneliCount.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        lblPersoneliCount.ForeColor = Color.FromArgb(180, 185, 195);
        lblPersoneliCount.Location = new Point(12, 42);
        lblPersoneliCount.Size = new Size(400, 22);

        btnShtoPersonel = BtnTab("+ Shto Personel", 105, Color.FromArgb(46, 204, 113), btnShtoPersonel_Click);
        btnShtoPersonel.Location = new Point(12, 8);
        btnNdryshoPersonel = BtnTab("Ndrysho", 85, Color.FromArgb(52, 152, 219), btnNdryshoPersonel_Click);
        btnNdryshoPersonel.Location = new Point(122, 8);
        btnFshiPersonel = BtnTab("Fshi", 70, Color.FromArgb(231, 76, 60), btnFshiPersonel_Click);
        btnFshiPersonel.Location = new Point(212, 8);

        dgvPersoneli.Location = new Point(12, 65);
        dgvPersoneli.Size = new Size(1135, 575);

        tabPersoneli.Controls.AddRange(new Control[] {
            lblPersoneliCount, dgvPersoneli, btnShtoPersonel, btnNdryshoPersonel, btnFshiPersonel
        });

        // ========== TAB KLIENTET ==========
        tabKlientet.Text = "  Klientet  ";
        tabKlientet.Padding = new Padding(12);

        lblKlientetCount = new Label();
        lblKlientetCount.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        lblKlientetCount.ForeColor = Color.FromArgb(180, 185, 195);
        lblKlientetCount.Location = new Point(12, 42);
        lblKlientetCount.Size = new Size(400, 22);

        btnShtoKlient = BtnTab("+ Shto Klient", 95, Color.FromArgb(46, 204, 113), btnShtoKlient_Click);
        btnShtoKlient.Location = new Point(12, 8);
        btnNdryshoKlient = BtnTab("Ndrysho", 85, Color.FromArgb(52, 152, 219), btnNdryshoKlient_Click);
        btnNdryshoKlient.Location = new Point(112, 8);
        btnFshiKlient = BtnTab("Fshi", 70, Color.FromArgb(231, 76, 60), btnFshiKlient_Click);
        btnFshiKlient.Location = new Point(202, 8);

        dgvKlientet = new DataGridView();
        dgvKlientet.Location = new Point(12, 65);
        dgvKlientet.Size = new Size(1135, 575);

        tabKlientet.Controls.AddRange(new Control[] {
            lblKlientetCount, dgvKlientet, btnShtoKlient, btnNdryshoKlient, btnFshiKlient
        });

        // ========== TAB TRANSAKSIONET ==========
        tabTransaksionet.Text = "  Transaksionet  ";
        tabTransaksionet.Padding = new Padding(12);

        label2 = new Label(); label2.Text = "Kerko:"; label2.Font = new Font("Segoe UI", 10);
        label2.Location = new Point(12, 12); label2.Size = new Size(50, 28); label2.ForeColor = Color.FromArgb(180, 185, 195);

        txtKerkimTransaksion = new TextBox();
        txtKerkimTransaksion.Location = new Point(62, 12);
        txtKerkimTransaksion.Size = new Size(250, 23);
        txtKerkimTransaksion.TextChanged += txtKerkimTransaksion_TextChanged;

        lblTransaksionetCount = new Label();
        lblTransaksionetCount.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        lblTransaksionetCount.ForeColor = Color.FromArgb(180, 185, 195);
        lblTransaksionetCount.Location = new Point(12, 42);
        lblTransaksionetCount.Size = new Size(400, 22);

        dgvTransaksionet.Location = new Point(12, 65);
        dgvTransaksionet.Size = new Size(1135, 575);

        tabTransaksionet.Controls.AddRange(new Control[] {
            label2, txtKerkimTransaksion, lblTransaksionetCount, dgvTransaksionet
        });

        // ========== TAB GJENDJA DEPONIMIT ==========
        tabGjendja.Text = "  Gjendja e Deponimit  ";
        tabGjendja.Padding = new Padding(12);

        var lblGjendjaTitle = new Label();
        lblGjendjaTitle.Text = "Armet aktualisht ne deponim (Magazine)";
        lblGjendjaTitle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
        lblGjendjaTitle.ForeColor = Color.FromArgb(200, 205, 215);
        lblGjendjaTitle.Location = new Point(12, 12);
        lblGjendjaTitle.Size = new Size(500, 28);

        lblGjendjaCount = new Label();
        lblGjendjaCount.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        lblGjendjaCount.ForeColor = Color.FromArgb(180, 185, 195);
        lblGjendjaCount.Location = new Point(520, 14);
        lblGjendjaCount.Size = new Size(400, 22);

        dgvGjendjaDeponimit = new DataGridView();
        dgvGjendjaDeponimit.Location = new Point(12, 50);
        dgvGjendjaDeponimit.Size = new Size(1135, 590);

        tabGjendja.Controls.AddRange(new Control[] {
            lblGjendjaTitle, lblGjendjaCount, dgvGjendjaDeponimit
        });

        // ========== TAB HISTORIKU ==========
        tabHistoriku.Text = "  Historiku  ";
        tabHistoriku.Padding = new Padding(12);

        var lblHistoriku = new Label();
        lblHistoriku.Text = "Historiku i veprimeve ne sistem";
        lblHistoriku.Font = new Font("Segoe UI", 11, FontStyle.Bold);
        lblHistoriku.ForeColor = Color.FromArgb(200, 205, 215);
        lblHistoriku.Location = new Point(12, 12);
        lblHistoriku.Size = new Size(500, 28);

        var lblKerkoHistoriku = new Label(); lblKerkoHistoriku.Text = "Kerko:"; lblKerkoHistoriku.Font = new Font("Segoe UI", 10);
        lblKerkoHistoriku.Location = new Point(520, 12); lblKerkoHistoriku.Size = new Size(50, 28); lblKerkoHistoriku.ForeColor = Color.FromArgb(180, 185, 195);

        txtKerkimHistoriku = new TextBox();
        txtKerkimHistoriku.Location = new Point(570, 12);
        txtKerkimHistoriku.Size = new Size(250, 23);
        txtKerkimHistoriku.TextChanged += txtKerkimHistoriku_TextChanged;

        dgvHistoriku = new DataGridView();
        dgvHistoriku.Location = new Point(12, 45);
        dgvHistoriku.Size = new Size(1135, 595);

        tabHistoriku.Controls.AddRange(new Control[] {
            lblHistoriku, lblKerkoHistoriku, txtKerkimHistoriku, dgvHistoriku
        });

        tabControl.SelectedIndexChanged += (s, e) =>
        {
            var tab = tabControl.SelectedTab;
            if (tab == tabHistoriku)
                NgarkoHistorikun(txtKerkimHistoriku.Text);
            else if (tab == tabBallina)
                NgarkoBallina();
            else if (tab == tabGjendja)
                NgarkoGjendjenDeponimit();
            else if (tab == tabPersoneli && dgvPersoneli.SelectedRows.Count > 0)
                NgarkoPersoneliDetail();
        };

        // ========== TAB ADMIN ==========
        tabAdmin.Text = "  Administrimi  ";
        tabAdmin.Padding = new Padding(12);

        dgvPerdoruesit = new DataGridView();
        dgvPerdoruesit.Location = new Point(12, 50);
        dgvPerdoruesit.Size = new Size(1135, 280);

        var lblAdminInfo = new Label();
        lblAdminInfo.Text = "Menaxhimi i perdoruesve te sistemit";
        lblAdminInfo.Font = new Font("Segoe UI", 9, FontStyle.Italic);
        lblAdminInfo.ForeColor = Color.FromArgb(180, 185, 195);
        lblAdminInfo.Location = new Point(12, 340);
        lblAdminInfo.Size = new Size(500, 22);

        tabAdmin.Controls.AddRange(new Control[] {
            dgvPerdoruesit, lblAdminInfo
        });

        Controls.Add(tabControl);
        ResumeLayout(false);
    }
}
