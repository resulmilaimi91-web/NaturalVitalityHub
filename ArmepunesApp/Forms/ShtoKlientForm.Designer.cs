namespace ArmepunesApp.Forms;

partial class ShtoKlientForm
{
    private System.ComponentModel.IContainer components = null;
    private TextBox txtEmri, txtMbiemri, txtAdresa, txtTelefon, txtEmail, txtNrLeternjoftimit, txtShenime;
    private Button btnRuaj, btnAnulo;
    private Label lblEmri, lblMbiemri, lblAdresa, lblTelefon, lblEmail, lblNrLeternjoftimit, lblShenime;
    private Panel _headerPanel;

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
        ClientSize = new Size(500, 400);
        Text = _klientiEkzistues != null ? "Ndrysho Klient" : "Shto Klient te Ri";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        BackColor = Color.FromArgb(35, 37, 42);

        _headerPanel = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.FromArgb(30, 50, 70) };
        var headerLbl = new Label { Text = _klientiEkzistues != null ? "✏ NDRYSHO KLIENT" : "➕ SHTO KLIENT TE RI",
            Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = Color.White,
            Location = new Point(15, 12), Size = new Size(450, 30), TextAlign = ContentAlignment.MiddleLeft };
        _headerPanel.Controls.Add(headerLbl);

        int y = 68, h = 25, lx = 20, tx = 150, tw = 320, gap = 30;

        lblEmri = new Label(); lblEmri.Text = "Emri *"; lblEmri.Location = new Point(lx, y); lblEmri.Size = new Size(125, h); lblEmri.ForeColor = Color.FromArgb(180, 185, 195);
        txtEmri = new TextBox(); txtEmri.Location = new Point(tx, y); txtEmri.Size = new Size(tw, h); y += gap;

        lblMbiemri = new Label(); lblMbiemri.Text = "Mbiemri *"; lblMbiemri.Location = new Point(lx, y); lblMbiemri.Size = new Size(125, h); lblMbiemri.ForeColor = Color.FromArgb(180, 185, 195);
        txtMbiemri = new TextBox(); txtMbiemri.Location = new Point(tx, y); txtMbiemri.Size = new Size(tw, h); y += gap;

        lblAdresa = new Label(); lblAdresa.Text = "Adresa"; lblAdresa.Location = new Point(lx, y); lblAdresa.Size = new Size(125, h); lblAdresa.ForeColor = Color.FromArgb(180, 185, 195);
        txtAdresa = new TextBox(); txtAdresa.Location = new Point(tx, y); txtAdresa.Size = new Size(tw, h); y += gap;

        lblTelefon = new Label(); lblTelefon.Text = "Telefon"; lblTelefon.Location = new Point(lx, y); lblTelefon.Size = new Size(125, h); lblTelefon.ForeColor = Color.FromArgb(180, 185, 195);
        txtTelefon = new TextBox(); txtTelefon.Location = new Point(tx, y); txtTelefon.Size = new Size(tw, h); y += gap;

        lblEmail = new Label(); lblEmail.Text = "Email"; lblEmail.Location = new Point(lx, y); lblEmail.Size = new Size(125, h); lblEmail.ForeColor = Color.FromArgb(180, 185, 195);
        txtEmail = new TextBox(); txtEmail.Location = new Point(tx, y); txtEmail.Size = new Size(tw, h); y += gap;

        lblNrLeternjoftimit = new Label(); lblNrLeternjoftimit.Text = "Nr. Leternjoftimit"; lblNrLeternjoftimit.Location = new Point(lx, y); lblNrLeternjoftimit.Size = new Size(125, h); lblNrLeternjoftimit.ForeColor = Color.FromArgb(180, 185, 195);
        txtNrLeternjoftimit = new TextBox(); txtNrLeternjoftimit.Location = new Point(tx, y); txtNrLeternjoftimit.Size = new Size(tw, h); y += gap;

        lblShenime = new Label(); lblShenime.Text = "Shenime"; lblShenime.Location = new Point(lx, y); lblShenime.Size = new Size(125, h); lblShenime.ForeColor = Color.FromArgb(180, 185, 195);
        txtShenime = new TextBox(); txtShenime.Location = new Point(tx, y); txtShenime.Size = new Size(tw, 50); txtShenime.Multiline = true; y += 60;

        var _btnRuajColor = Color.FromArgb(39, 174, 96);
        var _btnRuajLighter = Color.FromArgb(147, 214, 175);
        btnRuaj = new Button(); btnRuaj.Text = "💾 Ruaj";
        btnRuaj.FlatStyle = FlatStyle.Flat;
        btnRuaj.FlatAppearance.BorderSize = 2;
        btnRuaj.FlatAppearance.BorderColor = _btnRuajLighter;
        btnRuaj.BackColor = _btnRuajColor;
        btnRuaj.ForeColor = Color.White;
        btnRuaj.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        btnRuaj.Cursor = Cursors.Hand;
        btnRuaj.Size = new Size(110, 36);
        btnRuaj.TextAlign = ContentAlignment.MiddleCenter;
        btnRuaj.UseVisualStyleBackColor = false;
        btnRuaj.Location = new Point(270, y);
        btnRuaj.MouseEnter += (_, __) => btnRuaj.BackColor = _btnRuajLighter;
        btnRuaj.MouseLeave += (_, __) => btnRuaj.BackColor = _btnRuajColor;
        btnRuaj.Click += btnRuaj_Click;

        var _btnAnuloColor = Color.FromArgb(150, 160, 175);
        var _btnAnuloLighter = Color.FromArgb(202, 207, 215);
        btnAnulo = new Button(); btnAnulo.Text = "✖ Anulo";
        btnAnulo.FlatStyle = FlatStyle.Flat;
        btnAnulo.FlatAppearance.BorderSize = 2;
        btnAnulo.FlatAppearance.BorderColor = _btnAnuloLighter;
        btnAnulo.BackColor = _btnAnuloColor;
        btnAnulo.ForeColor = Color.White;
        btnAnulo.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        btnAnulo.Cursor = Cursors.Hand;
        btnAnulo.Size = new Size(110, 36);
        btnAnulo.TextAlign = ContentAlignment.MiddleCenter;
        btnAnulo.UseVisualStyleBackColor = false;
        btnAnulo.Location = new Point(385, y);
        btnAnulo.MouseEnter += (_, __) => btnAnulo.BackColor = _btnAnuloLighter;
        btnAnulo.MouseLeave += (_, __) => btnAnulo.BackColor = _btnAnuloColor;
        btnAnulo.Click += btnAnulo_Click;

        Controls.AddRange(new Control[] { _headerPanel, lblEmri, txtEmri, lblMbiemri, txtMbiemri,
            lblAdresa, txtAdresa, lblTelefon, txtTelefon, lblEmail, txtEmail,
            lblNrLeternjoftimit, txtNrLeternjoftimit, lblShenime, txtShenime, btnRuaj, btnAnulo });
        ResumeLayout(false); PerformLayout();
    }
}
