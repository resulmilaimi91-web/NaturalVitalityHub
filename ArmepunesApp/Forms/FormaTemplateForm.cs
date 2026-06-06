using System.Data;
using System.Drawing.Printing;
using System.IO;
using ArmepunesApp.Data;

namespace ArmepunesApp.Forms;

public partial class FormaTemplateForm : Form
{
    private readonly DatabaseHelper _db;
    private DataTable _templates = new();
    private readonly ComboBox cmbLloji = null!;
    private readonly ListBox lstTemplates = null!;
    private readonly Panel pnlPreview = null!;
    private readonly Label lblEmri = null!;
    private readonly Label lblParametrat = null!;
    private readonly Button btnAktivizo = null!;

    public FormaTemplateForm(DatabaseHelper db)
    {
        _db = db;
        Size = new Size(900, 620);
        Text = "Konfigurimi i Formave A4";
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(30, 33, 40);
        Font = new Font("Segoe UI", 9);
        MinimizeBox = false;
        ShowIcon = false;
        ShowInTaskbar = false;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;

        var lblTitle = new Label
        {
            Text = "ZGJIDH FORMEN A4 PER DOKUMENTACION",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Color.FromArgb(0, 200, 255),
            Size = new Size(700, 30),
            Location = new Point(15, 12)
        };

        var lblLloji = new Label
        {
            Text = "Lloji:",
            Font = new Font("Segoe UI", 9),
            ForeColor = Color.FromArgb(200, 205, 216),
            Size = new Size(50, 24),
            Location = new Point(15, 50)
        };

        cmbLloji = new ComboBox
        {
            Size = new Size(200, 24),
            Location = new Point(70, 48),
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(40, 42, 48),
            ForeColor = Color.FromArgb(200, 205, 216),
            FlatStyle = FlatStyle.Flat
        };
        cmbLloji.Items.AddRange(new[] { "📋 Fletelejimet", "📄 Fletpranim", "📊 Raportet", "📓 Ditaret" });
        cmbLloji.SelectedIndex = 0;
        cmbLloji.SelectedIndexChanged += (_, _) => NgarkoTemplates();

        lstTemplates = new ListBox
        {
            Size = new Size(260, 280),
            Location = new Point(15, 80),
            BackColor = Color.FromArgb(35, 37, 43),
            ForeColor = Color.FromArgb(200, 205, 216),
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Segoe UI", 10),
            IntegralHeight = false
        };
        lstTemplates.SelectedIndexChanged += (_, _) => ShfaqDetajet();

        pnlPreview = new Panel
        {
            Size = new Size(570, 280),
            Location = new Point(290, 80),
            BackColor = Color.FromArgb(40, 42, 48),
            BorderStyle = BorderStyle.FixedSingle
        };
        pnlPreview.Paint += VizatoPreview;

        lblEmri = new Label
        {
            Text = "",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.FromArgb(200, 205, 216),
            Size = new Size(550, 22),
            Location = new Point(290, 370)
        };

        lblParametrat = new Label
        {
            Text = "",
            Font = new Font("Segoe UI", 8),
            ForeColor = Color.FromArgb(140, 145, 155),
            Size = new Size(550, 80),
            Location = new Point(290, 395)
        };

        btnAktivizo = new Button
        {
            Text = "✅ Aktivizo",
            Size = new Size(140, 36),
            Location = new Point(15, 370),
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = Color.FromArgb(39, 174, 96),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            UseVisualStyleBackColor = false
        };
        btnAktivizo.Click += (_, _) => Aktivizo();

        var btnKrijo = new Button
        {
            Text = "✚ Krijo Template",
            Size = new Size(140, 36),
            Location = new Point(15, 415),
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            BackColor = Color.FromArgb(52, 152, 219),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            UseVisualStyleBackColor = false
        };
        btnKrijo.Click += (_, _) => HapEditor(null);

        var btnNdrysho = new Button
        {
            Text = "✎ Ndrysho",
            Size = new Size(110, 36),
            Location = new Point(165, 415),
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            BackColor = Color.FromArgb(230, 126, 34),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            UseVisualStyleBackColor = false
        };
        btnNdrysho.Click += (_, _) => Ndrysho();

        var btnFshi = new Button
        {
            Text = "✕ Fshi",
            Size = new Size(100, 36),
            Location = new Point(15, 460),
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            BackColor = Color.FromArgb(192, 57, 43),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            UseVisualStyleBackColor = false
        };
        btnFshi.Click += (_, _) => FshiTemplate();

        var btnMbyll = new Button
        {
            Text = "✕ Mbyll",
            Size = new Size(100, 36),
            Location = new Point(750, 540),
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            BackColor = Color.FromArgb(120, 130, 145),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            UseVisualStyleBackColor = false
        };
        btnMbyll.Click += (_, _) => Close();

        Controls.AddRange(new Control[] { lblTitle, lblLloji, cmbLloji, lstTemplates, pnlPreview,
            lblEmri, lblParametrat, btnAktivizo, btnKrijo, btnNdrysho, btnFshi, btnMbyll });

        NgarkoTemplates();
    }

    private static string MerrLlojiNgaDisplay(string display)
    {
        return display switch
        {
            "📋 Fletelejimet" => "Fleteleshim",
            "📓 Ditaret" => "Ditar",
            "📊 Raportet" => "Raport",
            _ => "Fleteleshim"
        };
    }

    private void NgarkoTemplates()
    {
        var lloji = MerrLlojiNgaDisplay(cmbLloji.SelectedItem?.ToString() ?? "");
        _templates = _db.MerrTemplates();
        var filtered = _templates.Select($"Lloji = '{lloji.Replace("'", "''")}'");
        lstTemplates.Items.Clear();
        foreach (DataRow r in filtered)
        {
            var emri = r["Emri"]?.ToString() ?? "";
            var aktive = Convert.ToInt32(r["Aktive"]) == 1;
            lstTemplates.Items.Add(aktive ? $"✓ {emri}" : $"  {emri}");
        }
        if (lstTemplates.Items.Count > 0) lstTemplates.SelectedIndex = 0;
        pnlPreview.Invalidate();
    }

    private void ShfaqDetajet()
    {
        var row = MerrTemplateSelected();
        if (row == null) return;
        lblEmri.Text = row["Emri"]?.ToString() ?? "";
        var paramStr = row["Parametrat"]?.ToString() ?? "{}";
        try
        {
            var obj = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(paramStr);
            var lines = new List<string>();
            foreach (var p in obj.EnumerateObject())
                lines.Add($"{p.Name}: {p.Value}");
            lblParametrat.Text = string.Join("\n", lines);
        }
        catch { lblParametrat.Text = paramStr; }
        pnlPreview.Invalidate();
    }

    private DataRow? MerrTemplateSelected()
    {
        if (lstTemplates.SelectedIndex < 0) return null;
        var lloji = MerrLlojiNgaDisplay(cmbLloji.SelectedItem?.ToString() ?? "");
        var filtered = _templates.Select($"Lloji = '{lloji.Replace("'", "''")}'");
        if (lstTemplates.SelectedIndex < filtered.Length)
            return filtered[lstTemplates.SelectedIndex];
        return null;
    }

    private void VizatoPreview(object? s, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(Color.White);
        var row = MerrTemplateSelected();
        if (row == null) return;

        var w = pnlPreview.Width - 10;
        var x = 5f;
        var y = 5f;
        var emri = row["Emri"]?.ToString() ?? "";
        var lloji = row["Lloji"]?.ToString() ?? "";
        var paramStr = row["Parametrat"]?.ToString() ?? "{}";
        var aktive = Convert.ToInt32(row["Aktive"]) == 1;

        using var pen = new Pen(Color.FromArgb(0, 60, 110), 2);
        g.DrawRectangle(pen, x, y, w, pnlPreview.Height - 15);

        using var titleFont = new Font("Segoe UI", 12, FontStyle.Bold);
        using var subFont = new Font("Segoe UI", 8, FontStyle.Bold);
        using var normalFont = new Font("Segoe UI", 7);

        g.DrawString("POLIGONI DRENI", titleFont, Brushes.Black, x + 8, y + 6);
        g.DrawString("Qendra e Deponimit dhe Menaxhimit te Armeve", subFont, Brushes.Black, x + 8, y + 26);
        g.DrawLine(new Pen(Color.FromArgb(0, 60, 110), 1), x + 8, y + 40, x + w - 8, y + 40);

        y += 44;
        g.DrawString(lloji.ToUpper(), subFont, Brushes.Black, x + 8, y);
        y += 16;
        g.DrawString(emri, normalFont, Brushes.Black, x + 8, y);
        y += 14;

        if (aktive)
        {
            using var green = new SolidBrush(Color.FromArgb(39, 174, 96));
            g.DrawString("✓ AKTIVE", subFont, green, x + 8, y);
        }

        try
        {
            var obj = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(paramStr);
            y += 18;
            foreach (var p in obj.EnumerateObject())
            {
                g.DrawString($"• {p.Name}: {p.Value}", normalFont, Brushes.Black, x + 12, y);
                y += 14;
            }
        }
        catch { }
    }

    public void Rifresko()
    {
        NgarkoTemplates();
    }

    private void Aktivizo()
    {
        var row = MerrTemplateSelected();
        if (row == null) { MessageBox.Show("Zgjidh nje template.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        var id = Convert.ToInt32(row["Id"]);
        var lloji = row["Lloji"]?.ToString() ?? "Fleteleshim";
        var emri = row["Emri"]?.ToString() ?? "";
        if (_db.AktivizoTemplate(id, lloji))
        {
            MessageBox.Show($"Template '{emri}' u aktivizua per '{lloji}'.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
            NgarkoTemplates();
        }
        else
            MessageBox.Show("Nuk mund te aktivizohej template.", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    private void HapEditor(DataRow? existing)
    {
        var lloji = MerrLlojiNgaDisplay(cmbLloji.SelectedItem?.ToString() ?? "");
        using var editor = new TemplateEditorForm(_db, lloji, existing);
        if (editor.ShowDialog(this) == DialogResult.OK)
            NgarkoTemplates();
    }

    private void Ndrysho()
    {
        var row = MerrTemplateSelected();
        if (row == null) { MessageBox.Show("Zgjidh nje template.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        HapEditor(row);
    }

    private void FshiTemplate()
    {
        var row = MerrTemplateSelected();
        if (row == null) { MessageBox.Show("Zgjidh nje template.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        var emri = row["Emri"]?.ToString() ?? "";
        if (MessageBox.Show($"A jeni i sigurt qe doni te fshini template '{emri}'?", "Konfirmim",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        try
        {
            _db.FshiTemplate(Convert.ToInt32(row["Id"]));
            NgarkoTemplates();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Gabim: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
