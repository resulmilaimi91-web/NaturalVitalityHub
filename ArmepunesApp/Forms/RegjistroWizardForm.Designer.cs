namespace ArmepunesApp.Forms;

partial class RegjistroWizardForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        // ─── Top step chain ───
        panelSteps = new Panel();
        panelSteps.Size = new Size(800, 52);
        panelSteps.Location = new Point(0, 0);
        panelSteps.BackColor = Color.FromArgb(25, 28, 33);
        panelSteps.Padding = new Padding(0, 0, 0, 0);

        // ─── Header ───
        var panelHeader = new Panel();
        panelHeader.Size = new Size(800, 80);
        panelHeader.Location = new Point(0, 52);
        panelHeader.BackColor = Color.FromArgb(28, 30, 35);

        var lblHeader = new Label();
        lblHeader.Text = $"REGJISTRO TRANSAKSION - {(_tipi == "Hyrje" ? "HYRJE (DEPONIM)" : "DALJE (TERHEQJE)")}";
        lblHeader.Font = new Font("Segoe UI", 13, FontStyle.Bold);
        lblHeader.ForeColor = Color.FromArgb(0, 200, 255);
        lblHeader.Size = new Size(580, 26);
        lblHeader.Location = new Point(15, 8);

        lblStepTitle = new Label();
        lblStepTitle.Text = "";
        lblStepTitle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
        lblStepTitle.ForeColor = Color.FromArgb(200, 205, 216);
        lblStepTitle.Size = new Size(400, 22);
        lblStepTitle.Location = new Point(15, 36);

        lblStepDescription = new Label();
        lblStepDescription.Text = "";
        lblStepDescription.Font = new Font("Segoe UI", 9);
        lblStepDescription.ForeColor = Color.FromArgb(120, 125, 135);
        lblStepDescription.Size = new Size(500, 18);
        lblStepDescription.Location = new Point(15, 56);

        panelHeader.Controls.Add(lblHeader);
        panelHeader.Controls.Add(lblStepTitle);
        panelHeader.Controls.Add(lblStepDescription);

        // ─── Content area ───
        panelContent = new Panel();
        panelContent.Size = new Size(750, 550);
        panelContent.Location = new Point(25, 132);
        panelContent.AutoScroll = true;
        panelContent.BackColor = Color.FromArgb(30, 30, 35);

        // ─── Bottom navigation ───
        var panelNav = new Panel();
        panelNav.Size = new Size(800, 50);
        panelNav.Location = new Point(0, 686);
        panelNav.BackColor = Color.FromArgb(28, 30, 35);

        btnBack = new Button();
        btnBack.Text = "◄ Prapa";
        btnBack.Size = new Size(110, 34);
        btnBack.Location = new Point(15, 8);
        btnBack.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        btnBack.BackColor = Color.FromArgb(60, 62, 68);
        btnBack.ForeColor = Color.FromArgb(200, 205, 216);
        btnBack.FlatStyle = FlatStyle.Flat;
        btnBack.FlatAppearance.BorderColor = Color.FromArgb(80, 82, 88);
        btnBack.Cursor = Cursors.Hand;
        btnBack.Click += btnBack_Click;

        btnNext = new Button();
        btnNext.Text = "Para ►";
        btnNext.Size = new Size(130, 34);
        btnNext.Location = new Point(520, 8);
        btnNext.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        btnNext.BackColor = Color.FromArgb(0, 140, 200);
        btnNext.ForeColor = Color.White;
        btnNext.FlatStyle = FlatStyle.Flat;
        btnNext.FlatAppearance.BorderColor = Color.FromArgb(0, 160, 220);
        btnNext.Cursor = Cursors.Hand;
        btnNext.Click += btnNext_Click;

        btnCancel = new Button();
        btnCancel.Text = "Anulo";
        btnCancel.Size = new Size(100, 34);
        btnCancel.Location = new Point(680, 8);
        btnCancel.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        btnCancel.BackColor = Color.FromArgb(200, 60, 60);
        btnCancel.ForeColor = Color.White;
        btnCancel.FlatStyle = FlatStyle.Flat;
        btnCancel.FlatAppearance.BorderColor = Color.FromArgb(220, 80, 80);
        btnCancel.Cursor = Cursors.Hand;
        btnCancel.Click += btnCancel_Click;

        panelNav.Controls.Add(btnBack);
        panelNav.Controls.Add(btnNext);
        panelNav.Controls.Add(btnCancel);

        // ─── Form ───
        BackColor = Color.FromArgb(30, 30, 35);
        ForeColor = Color.FromArgb(200, 205, 216);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(800, 740);
        Text = $"Regjistro Transaksion - {(_tipi == "Hyrje" ? "Deponim" : "Terheqje")}";

        Controls.Add(panelSteps);
        Controls.Add(panelHeader);
        Controls.Add(panelContent);
        Controls.Add(panelNav);

        ResumeLayout(false);
        PerformLayout();
    }
}
