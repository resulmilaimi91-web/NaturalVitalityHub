using System.Data;
using ArmepunesApp.Data;
using ArmepunesApp.Services;

namespace ArmepunesApp.Forms;

public partial class ExportRaportForm : Form
{
    private readonly DatabaseHelper _db;
    private ComboBox cmbArmet = null!;
    private ComboBox cmbKlientet = null!;
    private ComboBox cmbKlientetBackup = null!;
    private TabControl tabExport = null!;
    private Label lblStatus = null!;

    public ExportRaportForm(DatabaseHelper db)
    {
        _db = db;
        Text = "📤 Eksporto Raporte";
        Size = new Size(650, 500);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(30, 30, 35);
        Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath) ?? SystemIcons.Application;

        var header = new Label
        {
            Text = "  📤 EKSPORTIM RAPORTESH",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = Color.White,
            Dock = DockStyle.Top,
            Height = 50,
            Padding = new Padding(12, 10, 0, 0),
            BackColor = Color.FromArgb(18, 35, 55)
        };
        Controls.Add(header);

        tabExport = new TabControl { Dock = DockStyle.Fill, Padding = new Point(16, 8) };
        tabExport.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        tabExport.BackColor = Color.FromArgb(50, 52, 58);

        tabExport.TabPages.Add(BuildHistorikuArmesTab());
        tabExport.TabPages.Add(BuildRaportKlientitTab());
        tabExport.TabPages.Add(BuildBackupKlientTab());

        var statusPanel = new Panel { Dock = DockStyle.Bottom, Height = 40, BackColor = Color.FromArgb(35, 38, 45), Padding = new Padding(10, 8, 10, 8) };
        lblStatus = new Label
        {
            Text = "Gati per eksportim",
            Font = new Font("Segoe UI", 9, FontStyle.Italic),
            ForeColor = Color.FromArgb(160, 168, 180),
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        };
        statusPanel.Controls.Add(lblStatus);

        Controls.Add(tabExport);
        Controls.Add(statusPanel);
    }

    private TabPage BuildHistorikuArmesTab()
    {
        var tab = new TabPage("  🔫 Historiku i Armes  ");
        tab.BackColor = Color.FromArgb(38, 40, 48);
        tab.Padding = new Padding(20);

        var lbl = new Label
        {
            Text = "Zgjidh nje arme per te pare historikun e plote te hyrje/daljeve:",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(200, 205, 215),
            Location = new Point(20, 20),
            Size = new Size(500, 24)
        };

        cmbArmet = new ComboBox
        {
            Location = new Point(20, 55),
            Size = new Size(400, 28),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Segoe UI", 10),
            BackColor = Color.FromArgb(40, 42, 48),
            ForeColor = Color.FromArgb(200, 205, 216),
            FlatStyle = FlatStyle.Flat
        };

        var armet = _db.MerrArmet();
        foreach (DataRow r in armet.Rows)
        {
            var sn = r["NumerSerial"]?.ToString() ?? "";
            var marka = r["Marka"]?.ToString() ?? "";
            var model = r["Modeli"]?.ToString() ?? "";
            cmbArmet.Items.Add($"{sn} - {marka} {model}");
        }
        if (cmbArmet.Items.Count > 0) cmbArmet.SelectedIndex = 0;

        var btnEksporto = new Button
        {
            Text = "📄 Gjenero PDF",
            Location = new Point(20, 100),
            Size = new Size(200, 44),
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            BackColor = Color.FromArgb(0, 140, 200),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnEksporto.Click += BtnEksportoHistorikun_Click;
        btnEksporto.MouseEnter += (_, _) => btnEksporto.BackColor = Color.FromArgb(30, 160, 220);
        btnEksporto.MouseLeave += (_, _) => btnEksporto.BackColor = Color.FromArgb(0, 140, 200);

        var btnHapDir = new Button
        {
            Text = "📂 Hap Dosjen",
            Location = new Point(240, 100),
            Size = new Size(140, 44),
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = Color.FromArgb(60, 62, 68),
            ForeColor = Color.FromArgb(200, 205, 216),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnHapDir.Click += (_, _) =>
        {
            var dir = Path.Combine(ExportHelper.GetExportDir(), "Historiku_Armeve");
            if (Directory.Exists(dir)) System.Diagnostics.Process.Start("explorer.exe", dir);
            else MessageBox.Show("Nuk ka dosje eksporti ende.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        };

        tab.Controls.AddRange(new Control[] { lbl, cmbArmet, btnEksporto, btnHapDir });
        return tab;
    }

    private TabPage BuildRaportKlientitTab()
    {
        var tab = new TabPage("  👥 Raport Klienti  ");
        tab.BackColor = Color.FromArgb(38, 40, 48);
        tab.Padding = new Padding(20);

        var lbl = new Label
        {
            Text = "Zgjidh nje klient per te pare te gjitha transaksionet:",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(200, 205, 215),
            Location = new Point(20, 20),
            Size = new Size(500, 24)
        };

        cmbKlientet = new ComboBox
        {
            Location = new Point(20, 55),
            Size = new Size(400, 28),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Segoe UI", 10),
            BackColor = Color.FromArgb(40, 42, 48),
            ForeColor = Color.FromArgb(200, 205, 216),
            FlatStyle = FlatStyle.Flat
        };

        var klientet = _db.MerrKlientet();
        foreach (DataRow r in klientet.Rows)
        {
            var emri = $"{r["Emri"]} {r["Mbiemri"]}";
            cmbKlientet.Items.Add(emri);
        }
        if (cmbKlientet.Items.Count > 0) cmbKlientet.SelectedIndex = 0;

        var btnEksporto = new Button
        {
            Text = "📄 Gjenero PDF",
            Location = new Point(20, 100),
            Size = new Size(200, 44),
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            BackColor = Color.FromArgb(39, 174, 96),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnEksporto.Click += BtnEksportoKlientin_Click;
        btnEksporto.MouseEnter += (_, _) => btnEksporto.BackColor = Color.FromArgb(60, 200, 120);
        btnEksporto.MouseLeave += (_, _) => btnEksporto.BackColor = Color.FromArgb(39, 174, 96);

        var btnHapDir = new Button
        {
            Text = "📂 Hap Dosjen",
            Location = new Point(240, 100),
            Size = new Size(140, 44),
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = Color.FromArgb(60, 62, 68),
            ForeColor = Color.FromArgb(200, 205, 216),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnHapDir.Click += (_, _) =>
        {
            var dir = Path.Combine(ExportHelper.GetExportDir(), "Raporte_Kliente");
            if (Directory.Exists(dir)) System.Diagnostics.Process.Start("explorer.exe", dir);
            else MessageBox.Show("Nuk ka dosje eksporti ende.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        };

        tab.Controls.AddRange(new Control[] { lbl, cmbKlientet, btnEksporto, btnHapDir });
        return tab;
    }

    private TabPage BuildBackupKlientTab()
    {
        var tab = new TabPage("  💾 Backup Klienti  ");
        tab.BackColor = Color.FromArgb(38, 40, 48);
        tab.Padding = new Padding(20);

        var lbl = new Label
        {
            Text = "Zgjidh nje klient per te kriju databaze te veçante me te gjitha te dhenat e tij:",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(200, 205, 215),
            Location = new Point(20, 20),
            Size = new Size(500, 24)
        };

        cmbKlientetBackup = new ComboBox
        {
            Location = new Point(20, 55),
            Size = new Size(400, 28),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Segoe UI", 10),
            BackColor = Color.FromArgb(40, 42, 48),
            ForeColor = Color.FromArgb(200, 205, 216),
            FlatStyle = FlatStyle.Flat
        };

        var klientet = _db.MerrKlientet();
        foreach (DataRow r in klientet.Rows)
        {
            var emri = $"{r["Emri"]} {r["Mbiemri"]}";
            cmbKlientetBackup.Items.Add(emri);
        }
        if (cmbKlientetBackup.Items.Count > 0) cmbKlientetBackup.SelectedIndex = 0;

        var btnBackup = new Button
        {
            Text = "💾 Krijo Backup DB",
            Location = new Point(20, 100),
            Size = new Size(200, 44),
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            BackColor = Color.FromArgb(0, 120, 215),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnBackup.Click += BtnBackupKlienti_Click;
        btnBackup.MouseEnter += (_, _) => btnBackup.BackColor = Color.FromArgb(30, 140, 235);
        btnBackup.MouseLeave += (_, _) => btnBackup.BackColor = Color.FromArgb(0, 120, 215);

        var btnHapDir = new Button
        {
            Text = "📂 Hap Dosjen Backup",
            Location = new Point(240, 100),
            Size = new Size(140, 44),
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = Color.FromArgb(60, 62, 68),
            ForeColor = Color.FromArgb(200, 205, 216),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnHapDir.Click += (_, _) =>
        {
            var klienti = cmbKlientetBackup.SelectedItem?.ToString() ?? "";
            var dir = Path.Combine(BackupHelper.GetBackupDir(), "Klientet", BackupHelper.SanitizeFileName(klienti));
            if (Directory.Exists(dir)) System.Diagnostics.Process.Start("explorer.exe", dir);
            else MessageBox.Show("Nuk ka dosje backup per kete klient ende.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        };

        var btnBackupFull = new Button
        {
            Text = "💾 Backup i Plotë DB",
            Location = new Point(20, 150),
            Size = new Size(220, 44),
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = Color.FromArgb(39, 174, 96),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnBackupFull.Click += BtnBackupFull_Click;
        btnBackupFull.MouseEnter += (_, _) => btnBackupFull.BackColor = Color.FromArgb(60, 200, 120);
        btnBackupFull.MouseLeave += (_, _) => btnBackupFull.BackColor = Color.FromArgb(39, 174, 96);

        tab.Controls.AddRange(new Control[] { lbl, cmbKlientetBackup, btnBackup, btnHapDir, btnBackupFull });
        return tab;
    }

    private void BtnEksportoHistorikun_Click(object? sender, EventArgs e)
    {
        if (cmbArmet.SelectedItem == null) return;
        var serial = cmbArmet.SelectedItem.ToString()?.Split(" - ")[0] ?? "";
        lblStatus.Text = "⏳ Duke gjeneruar PDF...";
        lblStatus.ForeColor = Color.FromArgb(243, 156, 18);
        Refresh();

        try
        {
            var path = ExportHelper.EksportoHistorikunArmes(serial, _db);
            lblStatus.Text = $"✅ PDF u gjenerua: {path}";
            lblStatus.ForeColor = Color.FromArgb(46, 204, 113);
            var result = MessageBox.Show($"PDF u ruajt ne:\n{path}\n\nDeshironi ta hapni?", "Sukses",
                MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (result == DialogResult.Yes)
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(path) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            lblStatus.Text = "❌ Gabim gjate gjenerimit!";
            lblStatus.ForeColor = Color.FromArgb(192, 57, 43);
            MessageBox.Show($"Gabim: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnEksportoKlientin_Click(object? sender, EventArgs e)
    {
        if (cmbKlientet.SelectedItem == null) return;
        var klienti = cmbKlientet.SelectedItem.ToString() ?? "";
        lblStatus.Text = "⏳ Duke gjeneruar PDF...";
        lblStatus.ForeColor = Color.FromArgb(243, 156, 18);
        Refresh();

        try
        {
            var path = ExportHelper.EksportoRaportinKlientit(klienti, _db);
            lblStatus.Text = $"✅ PDF u gjenerua: {path}";
            lblStatus.ForeColor = Color.FromArgb(46, 204, 113);
            var result = MessageBox.Show($"PDF u ruajt ne:\n{path}\n\nDeshironi ta hapni?", "Sukses",
                MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (result == DialogResult.Yes)
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(path) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            lblStatus.Text = "❌ Gabim gjate gjenerimit!";
            lblStatus.ForeColor = Color.FromArgb(192, 57, 43);
            MessageBox.Show($"Gabim: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnBackupKlienti_Click(object? sender, EventArgs e)
    {
        if (cmbKlientetBackup.SelectedItem == null) return;
        var klienti = cmbKlientetBackup.SelectedItem.ToString() ?? "";
        lblStatus.Text = "⏳ Duke krijuar backup per klientin...";
        lblStatus.ForeColor = Color.FromArgb(243, 156, 18);
        Refresh();

        try
        {
            int klientiId = 0;
            var klientet = _db.MerrKlientet();
            foreach (DataRow r in klientet.Rows)
            {
                var emri = $"{r["Emri"]} {r["Mbiemri"]}";
                if (emri.Equals(klienti, StringComparison.OrdinalIgnoreCase))
                {
                    klientiId = Convert.ToInt32(r["Id"]);
                    break;
                }
            }

            var path = BackupHelper.EksportoKlientNeDb(klienti, klientiId, _db);
            lblStatus.Text = $"✅ Backup u krijua: {path}";
            lblStatus.ForeColor = Color.FromArgb(46, 204, 113);
            var result = MessageBox.Show($"Backup u ruajt ne:\n{path}\n\nDeshironi ta hapni dosjen?", "Sukses",
                MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (result == DialogResult.Yes)
            {
                var dir = Path.GetDirectoryName(path);
                if (dir != null) System.Diagnostics.Process.Start("explorer.exe", dir);
            }
        }
        catch (Exception ex)
        {
            lblStatus.Text = "❌ Gabim gjate backup!";
            lblStatus.ForeColor = Color.FromArgb(192, 57, 43);
            MessageBox.Show($"Gabim: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnBackupFull_Click(object? sender, EventArgs e)
    {
        lblStatus.Text = "⏳ Duke krijuar backup te plote...";
        lblStatus.ForeColor = Color.FromArgb(243, 156, 18);
        Refresh();

        try
        {
            var mainDbPath = _db.GetDbPath();
            var path = BackupHelper.KrijoBackupFull(_db, mainDbPath);
            lblStatus.Text = $"✅ Backup i plote u krijua: {path}";
            lblStatus.ForeColor = Color.FromArgb(46, 204, 113);
            var result = MessageBox.Show($"Backup u ruajt ne:\n{path}\n\nDeshironi ta hapni dosjen?", "Sukses",
                MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (result == DialogResult.Yes)
            {
                var dir = Path.GetDirectoryName(path);
                if (dir != null) System.Diagnostics.Process.Start("explorer.exe", dir);
            }
        }
        catch (Exception ex)
        {
            lblStatus.Text = "❌ Gabim gjate backup!";
            lblStatus.ForeColor = Color.FromArgb(192, 57, 43);
            MessageBox.Show($"Gabim: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);
    }
}
