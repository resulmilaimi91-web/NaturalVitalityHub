namespace ArmepunesApp.Forms;

partial class UpdateForm
{
    private System.ComponentModel.IContainer components = null;
    private Label lblStatus, lblVersion, lblChangelog;
    private ProgressBar progressBar;
    private Button btnUpdate, btnMbyll;
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
        ClientSize = new Size(480, 280);
        Text = "Azhornim i Aplikacionit";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        BackColor = Color.FromArgb(35, 37, 42);

        _headerPanel = new Panel { Dock = DockStyle.Top, Height = 55, BackColor = Color.FromArgb(30, 50, 70) };
        var headerLbl = new Label { Text = "🔄 AZHORNIM I APLIKACIONIT",
            Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = Color.White,
            Location = new Point(15, 14), Size = new Size(450, 30), TextAlign = ContentAlignment.MiddleLeft };
        _headerPanel.Controls.Add(headerLbl);

        lblStatus = new Label { Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.FromArgb(52, 73, 94), Location = new Point(20, 72), Size = new Size(440, 22),
            Text = "Duke kontrolluar..." };

        lblVersion = new Label { Font = new Font("Segoe UI", 9),
            ForeColor = Color.FromArgb(160, 165, 175), Location = new Point(20, 98), Size = new Size(440, 20) };

        lblChangelog = new Label { Font = new Font("Segoe UI", 9),
            ForeColor = Color.FromArgb(180, 185, 195), Location = new Point(20, 125), Size = new Size(440, 40) };

        progressBar = new ProgressBar { Location = new Point(20, 175), Size = new Size(440, 22), Style = ProgressBarStyle.Marquee, MarqueeAnimationSpeed = 30 };

        var _btnUpdateColor = Color.FromArgb(39, 174, 96);
        var _btnUpdateLighter = Color.FromArgb(147, 214, 175);
        btnUpdate = new Button(); btnUpdate.Text = "📥 Azhorno";
        btnUpdate.FlatStyle = FlatStyle.Flat;
        btnUpdate.FlatAppearance.BorderSize = 2;
        btnUpdate.FlatAppearance.BorderColor = _btnUpdateLighter;
        btnUpdate.BackColor = _btnUpdateColor;
        btnUpdate.ForeColor = Color.White;
        btnUpdate.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        btnUpdate.Cursor = Cursors.Hand;
        btnUpdate.Size = new Size(110, 36);
        btnUpdate.TextAlign = ContentAlignment.MiddleCenter;
        btnUpdate.UseVisualStyleBackColor = false;
        btnUpdate.Location = new Point(255, 220);
        btnUpdate.Enabled = false;
        btnUpdate.MouseEnter += (_, __) => btnUpdate.BackColor = _btnUpdateLighter;
        btnUpdate.MouseLeave += (_, __) => btnUpdate.BackColor = _btnUpdateColor;
        btnUpdate.Click += btnUpdate_Click;

        var _btnMbyllColor = Color.FromArgb(150, 160, 175);
        var _btnMbyllLighter = Color.FromArgb(202, 207, 215);
        btnMbyll = new Button(); btnMbyll.Text = "✖ Mbyll";
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
        btnMbyll.Location = new Point(365, 220);
        btnMbyll.MouseEnter += (_, __) => btnMbyll.BackColor = _btnMbyllLighter;
        btnMbyll.MouseLeave += (_, __) => btnMbyll.BackColor = _btnMbyllColor;
        btnMbyll.Click += btnMbyll_Click;

        Controls.AddRange(new Control[] { _headerPanel, lblStatus, lblVersion, lblChangelog, progressBar, btnUpdate, btnMbyll });
        ResumeLayout(false); PerformLayout();
    }
}
