namespace ArmepunesApp.Forms;

partial class FleteleshimForm
{
    private System.ComponentModel.IContainer components = null;
    private Label lblTitle, lblPrinter;
    private ComboBox cmbPrinter;
    private TextBox txtPreview;
    private Button btnPrinto, btnParashiko, btnMbyll, btnRefreshPrinter;
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
        ClientSize = new Size(850, 750);
        Text = "Fleteleshim";
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(35, 37, 42);

        _headerPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 55,
            BackColor = Color.FromArgb(30, 50, 70)
        };

        lblTitle = new Label();
        lblTitle.Text = "📄 FLETELESHIM";
        lblTitle.Font = new Font("Segoe UI", 14, FontStyle.Bold);
        lblTitle.ForeColor = Color.White;
        lblTitle.Location = new Point(15, 12);
        lblTitle.Size = new Size(750, 32);
        lblTitle.TextAlign = ContentAlignment.MiddleLeft;
        _headerPanel.Controls.Add(lblTitle);

        txtPreview = new TextBox();
        txtPreview.Font = new Font("Courier New", 10);
        txtPreview.Location = new Point(12, 65);
        txtPreview.Size = new Size(825, 620);
        txtPreview.Multiline = true;
        txtPreview.ReadOnly = true;
        txtPreview.BackColor = Color.FromArgb(40, 42, 48);
        txtPreview.ForeColor = Color.FromArgb(200, 205, 216);
        txtPreview.ScrollBars = ScrollBars.Vertical;
        txtPreview.BorderStyle = BorderStyle.FixedSingle;

        var _btnPrintoColor = Color.FromArgb(52, 152, 219);
        var _btnPrintoLighter = Color.FromArgb(153, 203, 237);
        // Printer selection
        lblPrinter = new Label();
        lblPrinter.Text = "Printeri:";
        lblPrinter.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        lblPrinter.ForeColor = Color.FromArgb(200, 205, 216);
        lblPrinter.Location = new Point(260, 703);
        lblPrinter.Size = new Size(60, 28);
        lblPrinter.TextAlign = ContentAlignment.MiddleLeft;

        cmbPrinter = new ComboBox();
        cmbPrinter.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbPrinter.Font = new Font("Segoe UI", 9);
        cmbPrinter.BackColor = Color.FromArgb(55, 57, 63);
        cmbPrinter.ForeColor = Color.FromArgb(200, 205, 216);
        cmbPrinter.Location = new Point(320, 703);
        cmbPrinter.Size = new Size(280, 28);

        btnRefreshPrinter = new Button();
        btnRefreshPrinter.Text = "🔄";
        btnRefreshPrinter.FlatStyle = FlatStyle.Flat;
        btnRefreshPrinter.BackColor = Color.FromArgb(60, 62, 68);
        btnRefreshPrinter.ForeColor = Color.White;
        btnRefreshPrinter.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        btnRefreshPrinter.Cursor = Cursors.Hand;
        btnRefreshPrinter.Size = new Size(30, 28);
        btnRefreshPrinter.Location = new Point(605, 703);
        btnRefreshPrinter.Click += (_, __) => NgarkoPrintera();
        btnRefreshPrinter.FlatAppearance.BorderSize = 1;

        btnPrinto = new Button();
        btnPrinto.Text = "🖨 Printo";
        btnPrinto.FlatStyle = FlatStyle.Flat;
        btnPrinto.FlatAppearance.BorderSize = 2;
        btnPrinto.FlatAppearance.BorderColor = _btnPrintoLighter;
        btnPrinto.BackColor = _btnPrintoColor;
        btnPrinto.ForeColor = Color.White;
        btnPrinto.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        btnPrinto.Cursor = Cursors.Hand;
        btnPrinto.Size = new Size(110, 36);
        btnPrinto.TextAlign = ContentAlignment.MiddleCenter;
        btnPrinto.UseVisualStyleBackColor = false;
        btnPrinto.Location = new Point(12, 700);
        btnPrinto.MouseEnter += (_, __) => btnPrinto.BackColor = _btnPrintoLighter;
        btnPrinto.MouseLeave += (_, __) => btnPrinto.BackColor = _btnPrintoColor;
        btnPrinto.Click += btnPrinto_Click;

        var _btnParashikoColor = Color.FromArgb(46, 134, 102);
        var _btnParashikoLighter = Color.FromArgb(150, 194, 178);
        btnParashiko = new Button();
        btnParashiko.Text = "👁 Parashiko";
        btnParashiko.FlatStyle = FlatStyle.Flat;
        btnParashiko.FlatAppearance.BorderSize = 2;
        btnParashiko.FlatAppearance.BorderColor = _btnParashikoLighter;
        btnParashiko.BackColor = _btnParashikoColor;
        btnParashiko.ForeColor = Color.White;
        btnParashiko.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        btnParashiko.Cursor = Cursors.Hand;
        btnParashiko.Size = new Size(110, 36);
        btnParashiko.TextAlign = ContentAlignment.MiddleCenter;
        btnParashiko.UseVisualStyleBackColor = false;
        btnParashiko.Location = new Point(132, 700);
        btnParashiko.MouseEnter += (_, __) => btnParashiko.BackColor = _btnParashikoLighter;
        btnParashiko.MouseLeave += (_, __) => btnParashiko.BackColor = _btnParashikoColor;
        btnParashiko.Click += btnParashiko_Click;

        var _btnMbyllColor = Color.FromArgb(150, 160, 175);
        var _btnMbyllLighter = Color.FromArgb(202, 207, 215);
        btnMbyll = new Button();
        btnMbyll.Text = "✖ Mbyll";
        btnMbyll.FlatStyle = FlatStyle.Flat;
        btnMbyll.FlatAppearance.BorderSize = 2;
        btnMbyll.FlatAppearance.BorderColor = _btnMbyllLighter;
        btnMbyll.BackColor = _btnMbyllColor;
        btnMbyll.ForeColor = Color.White;
        btnMbyll.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        btnMbyll.Cursor = Cursors.Hand;
        btnMbyll.Size = new Size(110, 36);
        btnMbyll.TextAlign = ContentAlignment.MiddleCenter;
        btnMbyll.UseVisualStyleBackColor = false;
        btnMbyll.Location = new Point(735, 700);
        btnMbyll.MouseEnter += (_, __) => btnMbyll.BackColor = _btnMbyllLighter;
        btnMbyll.MouseLeave += (_, __) => btnMbyll.BackColor = _btnMbyllColor;
        btnMbyll.Click += btnMbyll_Click;

        Controls.AddRange(new Control[] { _headerPanel, lblTitle, txtPreview, btnPrinto, btnParashiko, btnMbyll, lblPrinter, cmbPrinter, btnRefreshPrinter });
        ResumeLayout(false);
        PerformLayout();
    }
}
