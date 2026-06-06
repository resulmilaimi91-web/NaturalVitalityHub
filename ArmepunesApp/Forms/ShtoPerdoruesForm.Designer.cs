namespace ArmepunesApp.Forms;

partial class ShtoPerdoruesForm
{
    private System.ComponentModel.IContainer components = null;
    private TextBox txtUsername, txtPassword, txtEmri;
    private ComboBox cmbRole;
    private Button btnRuaj, btnAnulo;
    private Label lblUsername, lblPassword, lblEmri, lblRole;
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
        ClientSize = new Size(450, 290);
        Text = _perdoruesiEkzistues != null ? "Ndrysho Perdorues" : "Shto Perdorues";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        BackColor = Color.FromArgb(35, 37, 42);

        _headerPanel = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.FromArgb(30, 50, 70) };
        var headerLbl = new Label { Text = _perdoruesiEkzistues != null ? "✏ NDRYSHO PERDORUES" : "➕ SHTO PERDORUES",
            Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = Color.White,
            Location = new Point(15, 12), Size = new Size(400, 30), TextAlign = ContentAlignment.MiddleLeft };
        _headerPanel.Controls.Add(headerLbl);

        int y = 68, h = 25, lx = 20, tx = 130, tw = 280, gap = 32;

        lblUsername = new Label(); lblUsername.Text = "Username *"; lblUsername.Location = new Point(lx, y); lblUsername.Size = new Size(105, h); lblUsername.ForeColor = Color.FromArgb(180, 185, 195);
        txtUsername = new TextBox(); txtUsername.Location = new Point(tx, y); txtUsername.Size = new Size(tw, h); y += gap;

        lblPassword = new Label(); lblPassword.Text = "Password *"; lblPassword.Location = new Point(lx, y); lblPassword.Size = new Size(105, h); lblPassword.ForeColor = Color.FromArgb(180, 185, 195);
        txtPassword = new TextBox(); txtPassword.Location = new Point(tx, y); txtPassword.Size = new Size(tw, h); y += gap;

        lblEmri = new Label(); lblEmri.Text = "Emri"; lblEmri.Location = new Point(lx, y); lblEmri.Size = new Size(105, h); lblEmri.ForeColor = Color.FromArgb(180, 185, 195);
        txtEmri = new TextBox(); txtEmri.Location = new Point(tx, y); txtEmri.Size = new Size(tw, h); y += gap;

        lblRole = new Label(); lblRole.Text = "Roli"; lblRole.Location = new Point(lx, y); lblRole.Size = new Size(105, h); lblRole.ForeColor = Color.FromArgb(180, 185, 195);
        cmbRole = new ComboBox(); cmbRole.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbRole.Items.AddRange(new object[] { "Admin", "User" }); cmbRole.SelectedIndex = 1;
        cmbRole.Location = new Point(tx, y); cmbRole.Size = new Size(tw, h); y += 45;

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
        btnRuaj.Location = new Point(220, y);
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
        btnAnulo.Location = new Point(335, y);
        btnAnulo.MouseEnter += (_, __) => btnAnulo.BackColor = _btnAnuloLighter;
        btnAnulo.MouseLeave += (_, __) => btnAnulo.BackColor = _btnAnuloColor;
        btnAnulo.Click += btnAnulo_Click;

        Controls.AddRange(new Control[] { _headerPanel, lblUsername, txtUsername, lblPassword, txtPassword,
            lblEmri, txtEmri, lblRole, cmbRole, btnRuaj, btnAnulo });
        ResumeLayout(false); PerformLayout();
    }
}
