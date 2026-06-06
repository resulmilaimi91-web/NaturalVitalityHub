namespace ArmepunesApp.Forms;

partial class ShtoPersonelForm
{
    private System.ComponentModel.IContainer components = null;
    private TextBox txtEmri, txtMbiemri, txtGrada, txtNjesia, txtNrLegjitimacioni, txtTelefon;
    private Button btnRuaj, btnAnulo;
    private Label lblEmri, lblMbiemri, lblGrada, lblNjesia, lblNrLegjitimacioni, lblTelefon;
    private Panel _headerPanel;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath) ?? SystemIcons.Application; } catch { }
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(480, 350);
        Text = _personeliEkzistues != null ? "Ndrysho Personel" : "Shto Personel te Ri";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        BackColor = Color.FromArgb(35, 37, 42);

        _headerPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 50,
            BackColor = Color.FromArgb(30, 50, 70)
        };
        var headerLbl = new Label
        {
            Text = _personeliEkzistues != null ? "✏ NDRYSHO PERSONEL" : "➕ SHTO PERSONEL TE RI",
            Font = new Font("Segoe UI", 13, FontStyle.Bold),
            ForeColor = Color.White,
            Location = new Point(15, 12),
            Size = new Size(450, 30),
            TextAlign = ContentAlignment.MiddleLeft
        };
        _headerPanel.Controls.Add(headerLbl);

        int y = 68, h = 25, lx = 20, tx = 155, tw = 295, gap = 32;

        lblEmri = new Label(); lblEmri.Text = "Emri *"; lblEmri.Location = new Point(lx, y); lblEmri.Size = new Size(130, h); lblEmri.ForeColor = Color.FromArgb(180, 185, 195);
        txtEmri = new TextBox(); txtEmri.Location = new Point(tx, y); txtEmri.Size = new Size(tw, h);
        y += gap;

        lblMbiemri = new Label(); lblMbiemri.Text = "Mbiemri *"; lblMbiemri.Location = new Point(lx, y); lblMbiemri.Size = new Size(130, h); lblMbiemri.ForeColor = Color.FromArgb(180, 185, 195);
        txtMbiemri = new TextBox(); txtMbiemri.Location = new Point(tx, y); txtMbiemri.Size = new Size(tw, h);
        y += gap;

        lblGrada = new Label(); lblGrada.Text = "Grada"; lblGrada.Location = new Point(lx, y); lblGrada.Size = new Size(130, h); lblGrada.ForeColor = Color.FromArgb(180, 185, 195);
        txtGrada = new TextBox(); txtGrada.Location = new Point(tx, y); txtGrada.Size = new Size(tw, h);
        y += gap;

        lblNjesia = new Label(); lblNjesia.Text = "Njesia"; lblNjesia.Location = new Point(lx, y); lblNjesia.Size = new Size(130, h); lblNjesia.ForeColor = Color.FromArgb(180, 185, 195);
        txtNjesia = new TextBox(); txtNjesia.Location = new Point(tx, y); txtNjesia.Size = new Size(tw, h);
        y += gap;

        lblNrLegjitimacioni = new Label(); lblNrLegjitimacioni.Text = "Nr. Legjitimacioni"; lblNrLegjitimacioni.Location = new Point(lx, y); lblNrLegjitimacioni.Size = new Size(130, h); lblNrLegjitimacioni.ForeColor = Color.FromArgb(180, 185, 195);
        txtNrLegjitimacioni = new TextBox(); txtNrLegjitimacioni.Location = new Point(tx, y); txtNrLegjitimacioni.Size = new Size(tw, h);
        y += gap;

        lblTelefon = new Label(); lblTelefon.Text = "Telefon"; lblTelefon.Location = new Point(lx, y); lblTelefon.Size = new Size(130, h); lblTelefon.ForeColor = Color.FromArgb(180, 185, 195);
        txtTelefon = new TextBox(); txtTelefon.Location = new Point(tx, y); txtTelefon.Size = new Size(tw, h);
        y += 10;

        var _btnRuajColor = Color.FromArgb(39, 174, 96);
        var _btnRuajLighter = Color.FromArgb(147, 214, 175);
        btnRuaj = new Button();
        btnRuaj.Text = "💾 Ruaj";
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
        btnRuaj.Location = new Point(245, y + 15);
        btnRuaj.MouseEnter += (_, __) => btnRuaj.BackColor = _btnRuajLighter;
        btnRuaj.MouseLeave += (_, __) => btnRuaj.BackColor = _btnRuajColor;
        btnRuaj.Click += btnRuaj_Click;

        var _btnAnuloColor = Color.FromArgb(150, 160, 175);
        var _btnAnuloLighter = Color.FromArgb(202, 207, 215);
        btnAnulo = new Button();
        btnAnulo.Text = "✖ Anulo";
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
        btnAnulo.Location = new Point(365, y + 15);
        btnAnulo.MouseEnter += (_, __) => btnAnulo.BackColor = _btnAnuloLighter;
        btnAnulo.MouseLeave += (_, __) => btnAnulo.BackColor = _btnAnuloColor;
        btnAnulo.Click += btnAnulo_Click;

        Controls.AddRange(new Control[] {
            _headerPanel,
            lblEmri, txtEmri, lblMbiemri, txtMbiemri, lblGrada, txtGrada,
            lblNjesia, txtNjesia, lblNrLegjitimacioni, txtNrLegjitimacioni,
            lblTelefon, txtTelefon, btnRuaj, btnAnulo
        });

        ResumeLayout(false);
        PerformLayout();
    }
}
