using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using ArmepunesApp.Data;
using ArmepunesApp.Models;

namespace ArmepunesApp.Forms;

public partial class AdminForm : Form
{
    private readonly DatabaseHelper _db;
    private readonly string _perdoruesi;
    private TabControl tabControl = null!;
    private DataGridView dgvUsers = null!;
    private DataGridView dgvSettings = null!;
    private DataGridView dgvAudit = null!;

    public AdminForm(DatabaseHelper db, string perdoruesi)
    {
        _db = db;
        _perdoruesi = perdoruesi;
        InitializeComponent();
        NgarkoTabUsers();
        NgarkoTabSettings();
        NgarkoTabAudit();
        NgarkoTabDatabase();
    }

    private void InitializeComponent()
    {
        Text = "Paneli Administratorit";
        Size = new Size(1100, 680);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(30, 32, 37);
        ForeColor = Color.FromArgb(200, 205, 216);
        Font = new Font("Segoe UI", 9);
        Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath) ?? SystemIcons.Application;

        var lblTitle = new Label
        {
            Text = "ADMINISTRIMI I SISTEMIT",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = Color.FromArgb(46, 204, 113),
            Location = new Point(16, 12),
            Size = new Size(500, 32)
        };

        tabControl = new TabControl
        {
            Location = new Point(12, 52),
            Size = new Size(1060, 565),
            Font = new Font("Segoe UI", 9),
            BackColor = Color.FromArgb(35, 38, 45),
            ForeColor = Color.FromArgb(200, 205, 216)
        };

        var btnMbyll = new Button
        {
            Text = "Mbyll",
            Location = new Point(970, 620),
            Size = new Size(100, 30),
            BackColor = Color.FromArgb(80, 85, 95),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 9, FontStyle.Bold)
        };
        btnMbyll.Click += (_, _) => Close();

        Controls.Add(lblTitle);
        Controls.Add(tabControl);
        Controls.Add(btnMbyll);
    }

    // ======================== USERS TAB ========================

    private void NgarkoTabUsers()
    {
        var tab = new TabPage("  Perdoruesit  ") { BackColor = Color.FromArgb(30, 32, 37) };

        var panelBtn = new Panel { Location = new Point(8, 8), Size = new Size(1020, 36), BackColor = Color.FromArgb(35, 38, 45) };

        var btnShto = Btn("+ Shto", 100, Color.FromArgb(39, 174, 96), (_, _) => ShtoPerdorues());
        btnShto.Location = new Point(6, 3);
        var btnNdrysho = Btn("Ndrysho", 90, Color.FromArgb(52, 152, 219), (_, _) => NdryshoPerdorues());
        btnNdrysho.Location = new Point(112, 3);
        var btnFshi = Btn("Fshi", 80, Color.FromArgb(192, 57, 43), (_, _) => FshiPerdorues());
        btnFshi.Location = new Point(208, 3);
        var btnLejet = Btn("Lejet", 80, Color.FromArgb(155, 89, 182), (_, _) => HapLejet());
        btnLejet.Location = new Point(294, 3);
        var btnRifresko = Btn("Rifresko", 90, Color.FromArgb(100, 100, 110), (_, _) => RifreskoUserat());
        btnRifresko.Location = new Point(380, 3);

        panelBtn.Controls.AddRange(new Control[] { btnShto, btnNdrysho, btnFshi, btnLejet, btnRifresko });

        dgvUsers = new DataGridView
        {
            Location = new Point(8, 50),
            Size = new Size(1020, 470),
            BackgroundColor = Color.FromArgb(30, 32, 37),
            ForeColor = Color.FromArgb(200, 205, 216),
            GridColor = Color.FromArgb(50, 52, 58),
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Segoe UI", 9),
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            EnableHeadersVisualStyles = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(0, 80, 140),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            },
            ColumnHeadersHeight = 30,
            RowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(35, 38, 45),
                ForeColor = Color.FromArgb(200, 205, 216),
                SelectionBackColor = Color.FromArgb(0, 100, 160),
                SelectionForeColor = Color.White
            },
            AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(40, 42, 50) }
        };

        tab.Controls.Add(panelBtn);
        tab.Controls.Add(dgvUsers);
        tabControl.Controls.Add(tab);
        RifreskoUserat();
    }

    private void RifreskoUserat()
    {
        try
        {
            dgvUsers.DataSource = _db.MerrPerdoruesit();
            if (dgvUsers.Columns["Id"] != null) dgvUsers.Columns["Id"].Visible = false;
        }
        catch (Exception ex) { MessageBox.Show($"Gabim: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void ShtoPerdorues()
    {
        using var form = new Form
        {
            Text = "Shto Perdorues",
            Size = new Size(380, 260),
            StartPosition = FormStartPosition.CenterParent,
            BackColor = Color.FromArgb(35, 38, 45),
            ForeColor = Color.FromArgb(200, 205, 216),
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false
        };

        var lblU = new Label { Text = "Username:", Location = new Point(16, 16), Size = new Size(100, 22) };
        var txtU = new TextBox { Location = new Point(120, 14), Size = new Size(220, 24), BackColor = Color.FromArgb(50, 52, 58), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
        var lblE = new Label { Text = "Emri:", Location = new Point(16, 50), Size = new Size(100, 22) };
        var txtE = new TextBox { Location = new Point(120, 48), Size = new Size(220, 24), BackColor = Color.FromArgb(50, 52, 58), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
        var lblP = new Label { Text = "Password:", Location = new Point(16, 84), Size = new Size(100, 22) };
        var txtP = new TextBox { Location = new Point(120, 82), Size = new Size(220, 24), BackColor = Color.FromArgb(50, 52, 58), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle, UseSystemPasswordChar = true };
        var lblR = new Label { Text = "Roli:", Location = new Point(16, 118), Size = new Size(100, 22) };
        var cmbR = new ComboBox { Location = new Point(120, 116), Size = new Size(220, 24), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(50, 52, 58), ForeColor = Color.White };
        cmbR.Items.AddRange(new[] { "Admin", "User" });
        cmbR.SelectedIndex = 1;

        var btnRuaj = new Button { Text = "Ruaj", Location = new Point(120, 160), Size = new Size(100, 30), BackColor = Color.FromArgb(39, 174, 96), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
        var btnAnulo = new Button { Text = "Anulo", Location = new Point(230, 160), Size = new Size(100, 30), BackColor = Color.FromArgb(80, 85, 95), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };

        btnRuaj.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(txtU.Text) || string.IsNullOrWhiteSpace(txtP.Text))
            { MessageBox.Show("Ploteso username dhe password", "Validim", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            try
            {
                _db.ShtoPerdorues(new Perdoruesi
                {
                    Username = txtU.Text.Trim(),
                    Password = txtP.Text,
                    Emri = txtE.Text.Trim(),
                    Role = cmbR.SelectedItem?.ToString() ?? "User"
                });
                _db.RegjistroAuditLog(_perdoruesi, "Shto Perdorues", $"Username: {txtU.Text.Trim()}");
                RifreskoUserat();
                form.Close();
            }
            catch (Exception ex) { MessageBox.Show($"Gabim: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        };
        btnAnulo.Click += (_, _) => form.Close();

        form.Controls.AddRange(new Control[] { lblU, txtU, lblE, txtE, lblP, txtP, lblR, cmbR, btnRuaj, btnAnulo });
        form.ShowDialog();
    }

    private void NdryshoPerdorues()
    {
        if (dgvUsers.CurrentRow == null) { MessageBox.Show("Zgjedh nje perdorues", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        var row = dgvUsers.CurrentRow;
        int id = Convert.ToInt32(row.Cells["Id"].Value);
        string oldUser = row.Cells["Username"]?.Value?.ToString() ?? "";

        using var form = new Form
        {
            Text = "Ndrysho Perdorues",
            Size = new Size(380, 260),
            StartPosition = FormStartPosition.CenterParent,
            BackColor = Color.FromArgb(35, 38, 45),
            ForeColor = Color.FromArgb(200, 205, 216),
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false
        };

        var lblU = new Label { Text = "Username:", Location = new Point(16, 16), Size = new Size(100, 22) };
        var txtU = new TextBox { Text = row.Cells["Username"].Value?.ToString(), Location = new Point(120, 14), Size = new Size(220, 24), BackColor = Color.FromArgb(50, 52, 58), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
        var lblE = new Label { Text = "Emri:", Location = new Point(16, 50), Size = new Size(100, 22) };
        var txtE = new TextBox { Text = row.Cells["Emri"].Value?.ToString(), Location = new Point(120, 48), Size = new Size(220, 24), BackColor = Color.FromArgb(50, 52, 58), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
        var lblP = new Label { Text = "Password:", Location = new Point(16, 84), Size = new Size(100, 22) };
        var txtP = new TextBox { Location = new Point(120, 82), Size = new Size(220, 24), BackColor = Color.FromArgb(50, 52, 58), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle, UseSystemPasswordChar = true };
        var lblR = new Label { Text = "Roli:", Location = new Point(16, 118), Size = new Size(100, 22) };
        var cmbR = new ComboBox { Location = new Point(120, 116), Size = new Size(220, 24), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(50, 52, 58), ForeColor = Color.White };
        cmbR.Items.AddRange(new[] { "Admin", "User" });
        cmbR.SelectedItem = row.Cells["Role"].Value?.ToString();

        var btnRuaj = new Button { Text = "Ruaj", Location = new Point(120, 160), Size = new Size(100, 30), BackColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
        var btnAnulo = new Button { Text = "Anulo", Location = new Point(230, 160), Size = new Size(100, 30), BackColor = Color.FromArgb(80, 85, 95), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };

        btnRuaj.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(txtU.Text)) { MessageBox.Show("Username nuk mund te jete bosh", "Validim", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            try
            {
                var newPass = string.IsNullOrWhiteSpace(txtP!.Text)
                    ? row.Cells["Password"]?.Value?.ToString() ?? ""
                    : txtP.Text;
                _db.NdryshoPerdorues(new Perdoruesi
                {
                    Id = id,
                    Username = txtU!.Text.Trim(),
                    Password = newPass,
                    Emri = (txtE!.Text ?? "").Trim(),
                    Role = cmbR!.SelectedItem?.ToString() ?? "User"
                });
                _db.RegjistroAuditLog(_perdoruesi, "Ndrysho Perdorues", $"Id: {id}, Username: {txtU.Text.Trim()}");
                RifreskoUserat();
                form.Close();
            }
            catch (Exception ex) { MessageBox.Show($"Gabim: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        };
        btnAnulo.Click += (_, _) => form.Close();

        form.Controls.AddRange(new Control[] { lblU, txtU, lblE, txtE, lblP, txtP, lblR, cmbR, btnRuaj, btnAnulo });
        form.ShowDialog();
    }

    private void FshiPerdorues()
    {
        if (dgvUsers.CurrentRow == null) { MessageBox.Show("Zgjedh nje perdorues", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        var row = dgvUsers.CurrentRow;
        int id = Convert.ToInt32(row.Cells["Id"].Value);
        string username = row.Cells["Username"]?.Value?.ToString() ?? "";

        string role = row.Cells["Role"]?.Value?.ToString() ?? "";
        if (role == "Admin") { MessageBox.Show("Nuk mund te fshish nje perdorues me role Admin", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        if (MessageBox.Show($"A je i sigurt qe don te fshish '{username}'?", "Konfirmo", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

        try
        {
            _db.FshiPerdorues(id, _perdoruesi);
            RifreskoUserat();
        }
        catch (Exception ex) { MessageBox.Show($"Gabim: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void HapLejet()
    {
        if (dgvUsers.CurrentRow == null) { MessageBox.Show("Zgjedh nje perdorues", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        var row = dgvUsers.CurrentRow;
        int id = Convert.ToInt32(row.Cells["Id"].Value);
        string username = row.Cells["Username"]?.Value?.ToString() ?? "";
        using var f = new LejetForm(_db, id, username);
        f.ShowDialog(this);
    }

    // ======================== SETTINGS TAB ========================

    private void NgarkoTabSettings()
    {
        var tab = new TabPage("  Cilësimet  ") { BackColor = Color.FromArgb(30, 32, 37) };

        var panelBtn = new Panel { Location = new Point(8, 8), Size = new Size(1020, 36), BackColor = Color.FromArgb(35, 38, 45) };
        var btnRifresko = Btn("Rifresko", 90, Color.FromArgb(100, 100, 110), (_, _) => RifreskoSettings());
        btnRifresko.Location = new Point(6, 3);
        panelBtn.Controls.Add(btnRifresko);
        var btnFormaTemplates = Btn("Forma A4", 100, Color.FromArgb(155, 89, 182), (_, _) => { using var f = new FormaTemplateForm(_db); f.ShowDialog(this); RifreskoSettings(); });
        btnFormaTemplates.Location = new Point(106, 3);
        panelBtn.Controls.Add(btnFormaTemplates);

        dgvSettings = new DataGridView
        {
            Location = new Point(8, 50),
            Size = new Size(1020, 420),
            BackgroundColor = Color.FromArgb(30, 32, 37),
            ForeColor = Color.FromArgb(200, 205, 216),
            GridColor = Color.FromArgb(50, 52, 58),
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Segoe UI", 9),
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            EnableHeadersVisualStyles = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(0, 80, 140),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            },
            ColumnHeadersHeight = 30,
            RowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(35, 38, 45),
                ForeColor = Color.FromArgb(200, 205, 216),
                SelectionBackColor = Color.FromArgb(0, 100, 160),
                SelectionForeColor = Color.White
            },
            AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(40, 42, 50) }
        };
        dgvSettings.CellDoubleClick += NdryshoCilësimin;

        var lblInfo = new Label
        {
            Text = "Dykliko nje rresht per te ndryshuar vleren.",
            Font = new Font("Segoe UI", 9, FontStyle.Italic),
            ForeColor = Color.FromArgb(150, 155, 165),
            Location = new Point(12, 478),
            Size = new Size(500, 20)
        };

        tab.Controls.Add(panelBtn);
        tab.Controls.Add(dgvSettings);
        tab.Controls.Add(lblInfo);
        tabControl.Controls.Add(tab);
        RifreskoSettings();
    }

    private void RifreskoSettings()
    {
        try
        {
            dgvSettings.DataSource = _db.MerrCilësimet();
            if (dgvSettings.Columns["Key"] != null) dgvSettings.Columns["Key"].HeaderText = "Cilësimi";
            if (dgvSettings.Columns["Value"] != null) dgvSettings.Columns["Value"].HeaderText = "Vlera";
            if (dgvSettings.Columns["Description"] != null) dgvSettings.Columns["Description"].HeaderText = "Pershkrimi";
            if (dgvSettings.Columns["Category"] != null) dgvSettings.Columns["Category"].HeaderText = "Kategoria";
        }
        catch (Exception ex) { MessageBox.Show($"Gabim: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void NdryshoCilësimin(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || dgvSettings.CurrentRow == null) return;
        var row = dgvSettings.CurrentRow;
        string key = row.Cells["Key"].Value?.ToString() ?? "";
        string oldValue = row.Cells["Value"].Value?.ToString() ?? "";

        using var form = new Form
        {
            Text = $"Ndrysho: {key}",
            Size = new Size(450, 160),
            StartPosition = FormStartPosition.CenterParent,
            BackColor = Color.FromArgb(35, 38, 45),
            ForeColor = Color.FromArgb(200, 205, 216),
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false
        };

        var lbl = new Label { Text = $"Vlera per '{key}':", Location = new Point(16, 16), Size = new Size(400, 20) };
        var txt = new TextBox { Text = oldValue, Location = new Point(16, 42), Size = new Size(400, 24), BackColor = Color.FromArgb(50, 52, 58), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };

        var btnRuaj = new Button { Text = "Ruaj", Location = new Point(220, 80), Size = new Size(90, 30), BackColor = Color.FromArgb(39, 174, 96), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
        var btnAnulo = new Button { Text = "Anulo", Location = new Point(320, 80), Size = new Size(90, 30), BackColor = Color.FromArgb(80, 85, 95), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };

        btnRuaj.Click += (_, _) =>
        {
            try
            {
                _db.RuajCilësimin(key, txt.Text);
                _db.RegjistroAuditLog(_perdoruesi, "Ndrysho Cilësim", $"Key: {key} = {txt.Text}");
                RifreskoSettings();
                form.Close();
            }
            catch (Exception ex) { MessageBox.Show($"Gabim: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        };
        btnAnulo.Click += (_, _) => form.Close();

        form.Controls.AddRange(new Control[] { lbl, txt, btnRuaj, btnAnulo });
        form.ShowDialog();
    }

    // ======================== AUDIT TAB ========================

    private void NgarkoTabAudit()
    {
        var tab = new TabPage("  Historiku  ") { BackColor = Color.FromArgb(30, 32, 37) };

        var panelBtn = new Panel { Location = new Point(8, 8), Size = new Size(1020, 36), BackColor = Color.FromArgb(35, 38, 45) };

        var txtKerkim = new TextBox { Location = new Point(6, 5), Size = new Size(200, 24), BackColor = Color.FromArgb(50, 52, 58), ForeColor = Color.FromArgb(200, 205, 216), BorderStyle = BorderStyle.FixedSingle };
        var btnKerko = Btn("Kerko", 70, Color.FromArgb(0, 120, 200), (_, _) => KerkoAudit(txtKerkim.Text));
        btnKerko.Location = new Point(212, 3);
        var btnRifresko = Btn("Rifresko", 90, Color.FromArgb(100, 100, 110), (_, _) => RifreskoAudit());
        btnRifresko.Location = new Point(288, 3);
        var btnFshij = Btn("Pastro Log", 90, Color.FromArgb(192, 57, 43), (_, _) => PastroAudit());
        btnFshij.Location = new Point(384, 3);

        panelBtn.Controls.AddRange(new Control[] { txtKerkim, btnKerko, btnRifresko, btnFshij });

        dgvAudit = new DataGridView
        {
            Location = new Point(8, 50),
            Size = new Size(1020, 470),
            BackgroundColor = Color.FromArgb(30, 32, 37),
            ForeColor = Color.FromArgb(200, 205, 216),
            GridColor = Color.FromArgb(50, 52, 58),
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Segoe UI", 9),
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            EnableHeadersVisualStyles = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(0, 80, 140),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            },
            ColumnHeadersHeight = 30,
            RowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(35, 38, 45),
                ForeColor = Color.FromArgb(200, 205, 216),
                SelectionBackColor = Color.FromArgb(0, 100, 160),
                SelectionForeColor = Color.White
            },
            AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(40, 42, 50) }
        };

        tab.Controls.Add(panelBtn);
        tab.Controls.Add(dgvAudit);
        tabControl.Controls.Add(tab);
        RifreskoAudit();
    }

    private void RifreskoAudit()
    {
        try { dgvAudit.DataSource = _db.MerrAuditLog(); }
        catch (Exception ex) { MessageBox.Show($"Gabim: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void KerkoAudit(string filter)
    {
        if (string.IsNullOrWhiteSpace(filter)) { RifreskoAudit(); return; }
        try { dgvAudit.DataSource = _db.KerkoAuditLog(filter); }
        catch (Exception ex) { MessageBox.Show($"Gabim: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void PastroAudit()
    {
        if (MessageBox.Show("A je i sigurt qe don te pastrosh te gjithe historikun?", "Konfirmo", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        try
        {
            using var conn = new SQLiteConnection(DatabaseHelper.KrijoConnectionString(_db.GetDbPath()));
            conn.Open();
            using var cmd = new SQLiteCommand("DELETE FROM AuditLog", conn);
            cmd.ExecuteNonQuery();
            RifreskoAudit();
        }
        catch (Exception ex) { MessageBox.Show($"Gabim: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        _db.RegjistroAuditLog(_perdoruesi, "Pastro Audit Log", "Historiku u pastrua");
    }

    // ======================== DATABASE TAB ========================

    private void NgarkoTabDatabase()
    {
        var tab = new TabPage("  Database  ") { BackColor = Color.FromArgb(30, 32, 37) };

        var lblInfo = new Label
        {
            Text = $"Database: {_db.GetDbPath()}",
            Font = new Font("Segoe UI", 9),
            ForeColor = Color.FromArgb(160, 165, 175),
            Location = new Point(16, 16),
            Size = new Size(900, 20)
        };

        var lblSize = new Label
        {
            Text = $"Madhesia: {FormatSize(_db.MerrMadhesineDb())}",
            Font = new Font("Segoe UI", 9),
            ForeColor = Color.FromArgb(160, 165, 175),
            Location = new Point(16, 42),
            Size = new Size(300, 20)
        };

        var ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "?";
        var lblSchema = new Label
        {
            Text = $"Aplikacioni: v{ver}  |  Schema DB: v{_db.MerrSchemaVersion()}",
            Font = new Font("Segoe UI", 9),
            ForeColor = Color.FromArgb(160, 165, 175),
            Location = new Point(16, 60),
            Size = new Size(500, 20)
        };

        var btnBackup = new Button
        {
            Text = "💾 Backup Database",
            Location = new Point(16, 80),
            Size = new Size(200, 40),
            BackColor = Color.FromArgb(39, 174, 96),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };
        btnBackup.Click += (_, _) => BackupDb();

        var btnHapLocation = new Button
        {
            Text = "📂 Hap Location",
            Location = new Point(230, 80),
            Size = new Size(160, 40),
            BackColor = Color.FromArgb(52, 152, 219),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };
        btnHapLocation.Click += (_, _) =>
        {
            try { Process.Start("explorer.exe", $"/select,\"{_db.GetDbPath()}\""); }
            catch { var dir = Path.GetDirectoryName(_db.GetDbPath()); if (dir != null) Process.Start("explorer.exe", dir); }
        };

        var btnOptimizo = new Button
        {
            Text = "🧹 Optimizo (VACUUM)",
            Location = new Point(404, 80),
            Size = new Size(180, 40),
            BackColor = Color.FromArgb(155, 89, 182),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };
        btnOptimizo.Click += (_, _) => OptimizoDb();

        var lblKeshilla = new Label
        {
            Text = "Keshille: Bej backup te rregullt per te shmangur humbjen e te dhenave.",
            Font = new Font("Segoe UI", 8, FontStyle.Italic),
            ForeColor = Color.FromArgb(140, 145, 155),
            Location = new Point(16, 140),
            Size = new Size(500, 20)
        };

        var btnResetDb = new Button
        {
            Text = "🔄 Reset Database",
            Location = new Point(16, 180),
            Size = new Size(200, 40),
            BackColor = Color.FromArgb(192, 57, 43),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };
        btnResetDb.Click += (_, _) => ResetDatabase();

        var lblResetInfo = new Label
        {
            Text = "Pas resetit, perdoruesi i administratorit do te jene: admin / admin123",
            Font = new Font("Segoe UI", 8, FontStyle.Italic),
            ForeColor = Color.FromArgb(160, 165, 175),
            Location = new Point(16, 230),
            Size = new Size(400, 20)
        };

        tab.Controls.AddRange(new Control[] { lblInfo, lblSize, lblSchema, btnBackup, btnHapLocation, btnOptimizo, lblKeshilla, btnResetDb, lblResetInfo });
        tabControl.Controls.Add(tab);
    }

    private void BackupDb()
    {
        using var dlg = new SaveFileDialog
        {
            Title = "Ruaj backup-in e database",
            Filter = "SQLite database (*.sqlite)|*.sqlite|All files (*.*)|*.*",
            FileName = $"ArmepunesDB_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.sqlite"
        };
        if (dlg.ShowDialog() != DialogResult.OK) return;
        try
        {
            if (_db.BackupDb(dlg.FileName))
            {
                _db.RegjistroAuditLog(_perdoruesi, "Backup Database", $"Ne: {dlg.FileName}");
                MessageBox.Show($"Backup u ruajt me sukses!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
                MessageBox.Show($"Gabim gjate backup!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (Exception ex) { MessageBox.Show($"Gabim: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void OptimizoDb()
    {
        try
        {
            using var conn = new SQLiteConnection(DatabaseHelper.KrijoConnectionString(_db.GetDbPath()));
            conn.Open();
            using var cmd = new SQLiteCommand("VACUUM", conn);
            cmd.ExecuteNonQuery();
            _db.RegjistroAuditLog(_perdoruesi, "Optimizo Database", "VACUUM executed");
            MessageBox.Show("Database u optimizua me sukses!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) { MessageBox.Show($"Gabim: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private static string FormatSize(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        return $"{bytes / (1024.0 * 1024.0):F2} MB";
    }

    // ======================== HELPERS ========================

    private void ResetDatabase()
    {
        if (MessageBox.Show("A je i sigurt qe don te reshkonfiguroni gjitha databazen? Kete veprim do te fshije te gjithe te dhenat dhe do te ripopulloje te dhenat e fillestare.", "Konfirmo Reset Database", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            return;

        try
        {
            // Close existing connections by disposing current db helper
            // Then delete the database file
            string dbPath = _db.GetDbPath();
            
            // Delete the database file
            if (File.Exists(dbPath))
            {
                File.Delete(dbPath);
            }

            // Reinitialize the database (this will recreate tables and seed default data)
            var newDb = new DatabaseHelper(dbPath);
            
            // Refresh all tabs
            RifreskoUserat();
            RifreskoSettings();
            RifreskoAudit();
            
            MessageBox.Show("Database u reset me sukses!\nPerdoruesi i administratorit: admin / admin123", "Reset Database", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Gabim gjate resetit te databaze: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private Button Btn(string text, int w, Color c, EventHandler h)
    {
        var b = new Button
        {
            Text = text, Size = new Size(w, 30), FlatStyle = FlatStyle.Flat,
            FlatAppearance = { BorderSize = 0 },
            BackColor = c, ForeColor = Color.White,
            Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand,
            UseVisualStyleBackColor = false
        };
        b.Click += h;
        return b;
    }
}
