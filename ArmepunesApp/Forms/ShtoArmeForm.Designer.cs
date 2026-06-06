namespace ArmepunesApp.Forms;

partial class ShtoArmeForm
{
    private System.ComponentModel.IContainer components = null;
    private TextBox txtNumerSerial, txtLloji, txtMarka, txtModeli, txtKalibri;
    private TextBox txtVendlindja, txtShenime, txtNrInventari;
    private ComboBox cmbStatusi;
    private DateTimePicker dtpDataRegjistrimit;
    private Button btnRuaj, btnAnulo;
    private Label lblNumerSerial, lblLloji, lblMarka, lblModeli, lblKalibri;
    private Label lblVendlindja, lblStatusi, lblShenime, lblDataRegjistrimit, lblNrInventari;
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
        ClientSize = new Size(500, 520);
        Text = _armaEkzistuese != null ? "Ndrysho Arme" : "Shto Arme te Re";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        BackColor = Color.FromArgb(35, 37, 42);

        // Header
        _headerPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 50,
            BackColor = Color.FromArgb(30, 50, 70)
        };
        var headerLbl = new Label
        {
            Text = _armaEkzistuese != null ? "✏ NDRYSHO ARME" : "➕ SHTO ARME TE RE",
            Font = new Font("Segoe UI", 13, FontStyle.Bold),
            ForeColor = Color.White,
            Location = new Point(15, 12),
            Size = new Size(450, 30),
            TextAlign = ContentAlignment.MiddleLeft
        };
        _headerPanel.Controls.Add(headerLbl);

        int y = 68, h = 25, lx = 20, tx = 155, tw = 315, gap = 32;

        // Controls
        lblNumerSerial = new Label(); lblNumerSerial.Text = "Numer Serial *"; lblNumerSerial.Location = new Point(lx, y); lblNumerSerial.Size = new Size(130, h); lblNumerSerial.ForeColor = Color.FromArgb(180, 185, 195);
        txtNumerSerial = new TextBox(); txtNumerSerial.Location = new Point(tx, y); txtNumerSerial.Size = new Size(tw, h);
        y += gap;

        lblLloji = new Label(); lblLloji.Text = "Lloji *"; lblLloji.Location = new Point(lx, y); lblLloji.Size = new Size(130, h); lblLloji.ForeColor = Color.FromArgb(180, 185, 195);
        txtLloji = new TextBox(); txtLloji.Location = new Point(tx, y); txtLloji.Size = new Size(tw, h);
        y += gap;

        lblMarka = new Label(); lblMarka.Text = "Marka"; lblMarka.Location = new Point(lx, y); lblMarka.Size = new Size(130, h); lblMarka.ForeColor = Color.FromArgb(180, 185, 195);
        txtMarka = new TextBox(); txtMarka.Location = new Point(tx, y); txtMarka.Size = new Size(tw, h);
        y += gap;

        lblModeli = new Label(); lblModeli.Text = "Modeli"; lblModeli.Location = new Point(lx, y); lblModeli.Size = new Size(130, h); lblModeli.ForeColor = Color.FromArgb(180, 185, 195);
        txtModeli = new TextBox(); txtModeli.Location = new Point(tx, y); txtModeli.Size = new Size(tw, h);
        y += gap;

        lblKalibri = new Label(); lblKalibri.Text = "Kalibri"; lblKalibri.Location = new Point(lx, y); lblKalibri.Size = new Size(130, h); lblKalibri.ForeColor = Color.FromArgb(180, 185, 195);
        txtKalibri = new TextBox(); txtKalibri.Location = new Point(tx, y); txtKalibri.Size = new Size(tw, h);
        y += gap;

        lblVendlindja = new Label(); lblVendlindja.Text = "Vendlindja"; lblVendlindja.Location = new Point(lx, y); lblVendlindja.Size = new Size(130, h); lblVendlindja.ForeColor = Color.FromArgb(180, 185, 195);
        txtVendlindja = new TextBox(); txtVendlindja.Location = new Point(tx, y); txtVendlindja.Size = new Size(tw, h);
        y += gap;

        lblNrInventari = new Label(); lblNrInventari.Text = "Nr. Inventari"; lblNrInventari.Location = new Point(lx, y); lblNrInventari.Size = new Size(130, h); lblNrInventari.ForeColor = Color.FromArgb(180, 185, 195);
        txtNrInventari = new TextBox(); txtNrInventari.Location = new Point(tx, y); txtNrInventari.Size = new Size(tw, h);
        y += gap;

        lblStatusi = new Label(); lblStatusi.Text = "Statusi"; lblStatusi.Location = new Point(lx, y); lblStatusi.Size = new Size(130, h); lblStatusi.ForeColor = Color.FromArgb(180, 185, 195);
        cmbStatusi = new ComboBox(); cmbStatusi.Items.AddRange(new[] { "Ne Magazine", "Tek Klienti", "Jashte Sherbimit" }); cmbStatusi.SelectedIndex = 0; cmbStatusi.DropDownStyle = ComboBoxStyle.DropDownList; cmbStatusi.Location = new Point(tx, y); cmbStatusi.Size = new Size(tw, h);
        y += gap;

        lblDataRegjistrimit = new Label(); lblDataRegjistrimit.Text = "Data Regjistrimit"; lblDataRegjistrimit.Location = new Point(lx, y); lblDataRegjistrimit.Size = new Size(130, h); lblDataRegjistrimit.ForeColor = Color.FromArgb(180, 185, 195);
        dtpDataRegjistrimit = new DateTimePicker(); dtpDataRegjistrimit.Format = DateTimePickerFormat.Short; dtpDataRegjistrimit.Location = new Point(tx, y); dtpDataRegjistrimit.Size = new Size(tw, h);
        y += gap;

        lblShenime = new Label(); lblShenime.Text = "Shenime"; lblShenime.Location = new Point(lx, y); lblShenime.Size = new Size(130, h); lblShenime.ForeColor = Color.FromArgb(180, 185, 195);
        txtShenime = new TextBox(); txtShenime.Location = new Point(tx, y); txtShenime.Size = new Size(tw, 55); txtShenime.Multiline = true;
        y += 70;

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
        btnRuaj.Location = new Point(275, y);
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
        btnAnulo.Location = new Point(385, y);
        btnAnulo.MouseEnter += (_, __) => btnAnulo.BackColor = _btnAnuloLighter;
        btnAnulo.MouseLeave += (_, __) => btnAnulo.BackColor = _btnAnuloColor;
        btnAnulo.Click += btnAnulo_Click;

        Controls.AddRange(new Control[] {
            _headerPanel,
            lblNumerSerial, txtNumerSerial, lblLloji, txtLloji, lblMarka, txtMarka,
            lblModeli, txtModeli, lblKalibri, txtKalibri, lblVendlindja, txtVendlindja,
            lblNrInventari, txtNrInventari, lblStatusi, cmbStatusi,
            lblDataRegjistrimit, dtpDataRegjistrimit, lblShenime, txtShenime,
            btnRuaj, btnAnulo
        });

        ResumeLayout(false);
        PerformLayout();
    }
}
