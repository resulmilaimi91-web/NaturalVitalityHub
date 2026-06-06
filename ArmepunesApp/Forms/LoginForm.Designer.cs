namespace ArmepunesApp.Forms;

partial class LoginForm
{
    private System.ComponentModel.IContainer components = null;
    private TextBox txtUsername, txtPassword;
    private Button btnKycu, btnDil;
    private CheckBox chkRemember;
    private Panel _headerPanel;
    private Label lblTitle, lblSubtitle, lblUser, lblPass;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath) ?? SystemIcons.Application; } catch { }
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(440, 360);
        Text = "Kyçu - POLIGONI DRENI";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Color.FromArgb(30, 33, 40);

        // ── Header ──
        _headerPanel = new Panel { Dock = DockStyle.Top, Height = 100, BackColor = Color.FromArgb(15, 30, 50) };
        lblTitle = new Label
        {
            Text = "POLIGONI DRENI",
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            ForeColor = Color.White,
            Location = new Point(20, 14),
            Size = new Size(400, 34)
        };
        lblSubtitle = new Label
        {
            Text = "Qendra e Deponimit dhe Menaxhimit te Armeve",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(140, 175, 210),
            Location = new Point(20, 48),
            Size = new Size(400, 22)
        };
        var icon = new Label
        {
            Text = "⚔",
            Font = new Font("Segoe UI", 28),
            ForeColor = Color.Gold,
            Location = new Point(370, 16),
            Size = new Size(50, 50),
            TextAlign = ContentAlignment.MiddleCenter
        };
        var accentLine = new Panel { Dock = DockStyle.Bottom, Height = 3, BackColor = Color.Gold };
        _headerPanel.Controls.AddRange(new Control[] { lblTitle, lblSubtitle, icon, accentLine });

        // ── Form fields ──
        int lx = 40, tx = 150, tw = 250, h = 28, yStart = 120;

        lblUser = new Label
        {
            Text = "Përdorues:", Font = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = Color.FromArgb(180, 185, 195),
            Location = new Point(lx, yStart), Size = new Size(100, h)
        };
        txtUsername = new TextBox
        {
            Location = new Point(tx, yStart - 2), Size = new Size(tw, h),
            Font = new Font("Segoe UI", 10),
            BackColor = Color.FromArgb(45, 48, 55),
            ForeColor = Color.FromArgb(220, 225, 235),
            BorderStyle = BorderStyle.FixedSingle
        };

        lblPass = new Label
        {
            Text = "Fjalëkalimi:", Font = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = Color.FromArgb(180, 185, 195),
            Location = new Point(lx, yStart + 42), Size = new Size(100, h)
        };
        txtPassword = new TextBox
        {
            Location = new Point(tx, yStart + 40), Size = new Size(tw, h),
            PasswordChar = '*', Font = new Font("Segoe UI", 10),
            BackColor = Color.FromArgb(45, 48, 55),
            ForeColor = Color.FromArgb(220, 225, 235),
            BorderStyle = BorderStyle.FixedSingle
        };
        txtPassword.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) btnKycu_Click(s, e); };

        // ── Buttons ──
        var _btnKycuColor = Color.FromArgb(0, 80, 140);
        var _btnKycuHover = Color.FromArgb(0, 110, 180);
        btnKycu = new Button
        {
            Text = "Kyçu", FlatStyle = FlatStyle.Flat,
            FlatAppearance = { BorderSize = 0 },
            BackColor = _btnKycuColor, ForeColor = Color.White,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Cursor = Cursors.Hand,
            Size = new Size(120, 36),
            Location = new Point(150, yStart + 86)
        };
        btnKycu.MouseEnter += (_, _) => btnKycu.BackColor = _btnKycuHover;
        btnKycu.MouseLeave += (_, _) => btnKycu.BackColor = _btnKycuColor;
        btnKycu.Click += btnKycu_Click;

        btnDil = new Button
        {
            Text = "Dil", FlatStyle = FlatStyle.Flat,
            FlatAppearance = { BorderSize = 1, BorderColor = Color.FromArgb(100, 105, 115) },
            BackColor = Color.Transparent, ForeColor = Color.FromArgb(160, 165, 175),
            Font = new Font("Segoe UI", 10),
            Cursor = Cursors.Hand,
            Size = new Size(120, 36),
            Location = new Point(280, yStart + 86)
        };
        btnDil.MouseEnter += (_, _) => btnDil.BackColor = Color.FromArgb(55, 58, 65);
        btnDil.MouseLeave += (_, _) => btnDil.BackColor = Color.Transparent;
        btnDil.Click += btnDil_Click;

        // ── Remember me ──
        chkRemember = new CheckBox
        {
            Text = "Më mbaj mend",
            Font = new Font("Segoe UI", 9),
            ForeColor = Color.FromArgb(160, 165, 175),
            BackColor = Color.Transparent,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(240, 24),
            Location = new Point(150, yStart + 130),
            Padding = new Padding(4, 0, 0, 0)
        };

        // ── Footer ──
        var footer = new Label
        {
            Text = "© Poligoni Dreni — Sistem Deponimi Armesh",
            Font = new Font("Segoe UI", 7, FontStyle.Italic),
            ForeColor = Color.FromArgb(80, 85, 95),
            Location = new Point(0, yStart + 165),
            Size = new Size(440, 18),
            TextAlign = ContentAlignment.MiddleCenter
        };

        Controls.AddRange(new Control[] { _headerPanel, lblUser, txtUsername, lblPass, txtPassword, btnKycu, btnDil, chkRemember, footer });
        ResumeLayout(false); PerformLayout();
    }
}
