using System.Data;
using System.Drawing.Printing;
using System.IO;
using ArmepunesApp.Data;
using ArmepunesApp.Services;

namespace ArmepunesApp.Forms;

public partial class ListaFleteleshimeveForm : Form
{
    private readonly DatabaseHelper _db;
    private readonly string _perdoruesi;
    private DataTable _transData = new();
    private ComboBox cmbPrinter = null!;
    private TextBox txtKerkim = null!;
    private DataGridView dgvTrans = null!;
    private Label lblStatus = null!;

    public ListaFleteleshimeveForm(DatabaseHelper db, string perdoruesi)
    {
        _db = db;
        _perdoruesi = perdoruesi;
        InitializeComponent();
        NgarkoPrintera();
        NgarkoTransaksionet();
    }

    private void InitializeComponent()
    {
        Text = "Lista e Fleteleshimeve";
        Size = new Size(1200, 700);
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
            Text = "LISTA E TRANSAKSIONEVE / FLETELESHIMEVE",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Color.FromArgb(0, 200, 255),
            Size = new Size(700, 30),
            Location = new Point(15, 12)
        };

        var lblKerkim = new Label
        {
            Text = "Kerkim:",
            Font = new Font("Segoe UI", 9),
            ForeColor = Color.FromArgb(200, 205, 216),
            Size = new Size(60, 24),
            Location = new Point(15, 50)
        };

        txtKerkim = new TextBox
        {
            Size = new Size(300, 24),
            Location = new Point(80, 48),
            BackColor = Color.FromArgb(40, 42, 48),
            ForeColor = Color.FromArgb(200, 205, 216),
            BorderStyle = BorderStyle.FixedSingle
        };
        txtKerkim.TextChanged += (_, _) => NgarkoTransaksionet(txtKerkim.Text);

        cmbPrinter = new ComboBox
        {
            Size = new Size(220, 24),
            Location = new Point(420, 48),
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(40, 42, 48),
            ForeColor = Color.FromArgb(200, 205, 216),
            FlatStyle = FlatStyle.Flat
        };

        dgvTrans = new DataGridView
        {
            Size = new Size(1140, 470),
            Location = new Point(15, 80),
            BackgroundColor = Color.FromArgb(35, 37, 43),
            ForeColor = Color.FromArgb(200, 205, 216),
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Segoe UI", 9),
            AutoGenerateColumns = false,
            AllowUserToAddRows = false,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            ReadOnly = true,
            RowTemplate = new DataGridViewRow { Height = 26 }
        };
        dgvTrans.CellPainting += (s, e) =>
        {
            if (e.RowIndex < 0 || e.CellStyle == null) return;
            e.CellStyle.BackColor = e.RowIndex % 2 == 0 ? Color.FromArgb(35, 37, 43) : Color.FromArgb(42, 44, 50);
            e.CellStyle.SelectionBackColor = Color.FromArgb(0, 120, 215);
            e.CellStyle.SelectionForeColor = Color.White;
        };

        dgvTrans.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "Nr.", Width = 50 });
        dgvTrans.Columns.Add(new DataGridViewTextBoxColumn { Name = "Tipi", HeaderText = "Tipi", Width = 60 });
        dgvTrans.Columns.Add(new DataGridViewTextBoxColumn { Name = "DataOra", HeaderText = "Data/Ora", Width = 140 });
        dgvTrans.Columns.Add(new DataGridViewTextBoxColumn { Name = "KlientiEmri", HeaderText = "Klienti", Width = 150 });
        dgvTrans.Columns.Add(new DataGridViewTextBoxColumn { Name = "PersoneliEmri", HeaderText = "Personeli", Width = 150 });
        dgvTrans.Columns.Add(new DataGridViewTextBoxColumn { Name = "ArmaSerial", HeaderText = "Seriali", Width = 110 });
        dgvTrans.Columns.Add(new DataGridViewTextBoxColumn { Name = "Qellimi", HeaderText = "Qellimi", Width = 120 });
        dgvTrans.Columns.Add(new DataGridViewTextBoxColumn { Name = "DataOraTrim", HeaderText = "Data", Visible = false });
        dgvTrans.SelectionChanged += (_, _) => PerditesoStatusin();

        lblStatus = new Label
        {
            Text = "",
            Font = new Font("Segoe UI", 8),
            ForeColor = Color.FromArgb(120, 128, 140),
            Size = new Size(500, 20),
            Location = new Point(15, 558)
        };

        var bottomY = 585;
        var btnPrintoFleteleshim = KrijoButton("🖨 Printo Fleteleshim", 180, Color.FromArgb(155, 89, 182), new Point(15, bottomY));
        btnPrintoFleteleshim.Click += (_, _) => PrintoFleteleshimSelected();

        var btnHapPdf = KrijoButton("📂 Hap PDF", 120, Color.FromArgb(39, 174, 96), new Point(205, bottomY));
        btnHapPdf.Click += (_, _) => HapPdfSelected();

        var btnPrintoListen = KrijoButton("📋 Printo Listen", 140, Color.FromArgb(0, 140, 200), new Point(340, bottomY));
        btnPrintoListen.Click += (_, _) => PrintoListen();

        var btnGjeneroPdf = KrijoButton("🔁 Gjenero PDF", 130, Color.FromArgb(230, 126, 34), new Point(495, bottomY));
        btnGjeneroPdf.Click += (_, _) => GjeneroPdfSelected();

        var btnMbyll = KrijoButton("✕ Mbyll", 100, Color.FromArgb(120, 130, 145), new Point(640, bottomY));
        btnMbyll.Click += (_, _) => Close();

        Controls.AddRange(new Control[] { lblTitle, lblKerkim, txtKerkim, cmbPrinter, dgvTrans, lblStatus,
            btnPrintoFleteleshim, btnHapPdf, btnPrintoListen, btnGjeneroPdf, btnMbyll });
    }

    private Button KrijoButton(string text, int width, Color color, Point location)
    {
        return new Button
        {
            Text = text,
            Size = new Size(width, 36),
            Location = location,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            BackColor = color,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            UseVisualStyleBackColor = false
        };
    }

    private void NgarkoPrintera()
    {
        cmbPrinter.Items.Clear();
        foreach (string printer in PrinterSettings.InstalledPrinters)
            cmbPrinter.Items.Add(printer);
        if (cmbPrinter.Items.Count > 0)
        {
            var def = new PrinterSettings().PrinterName;
            for (int i = 0; i < cmbPrinter.Items.Count; i++)
            {
                if (cmbPrinter.Items[i]?.ToString() == def)
                { cmbPrinter.SelectedIndex = i; return; }
            }
            cmbPrinter.SelectedIndex = 0;
        }
    }

    private void NgarkoTransaksionet(string filter = "")
    {
        _transData = string.IsNullOrEmpty(filter) ? _db.MerrTransaksionet()
            : _db.KerkoTransaksionet(filter);
        dgvTrans.Rows.Clear();
        foreach (DataRow r in _transData.Rows)
        {
            var dataOra = r["DataOra"]?.ToString() ?? "";
            if (dataOra.Length >= 16) dataOra = dataOra.Replace("-", ".");
            dgvTrans.Rows.Add(
                r["Id"],
                r["Tipi"],
                dataOra,
                r["KlientiEmri"],
                r["PersoneliEmri"],
                r["ArmaSerial"],
                r["Qellimi"]);
        }
        lblStatus.Text = $"Gjithsej: {dgvTrans.Rows.Count} transaksione";
    }

    private DataRow? MerrTransaksionSelected()
    {
        if (dgvTrans.SelectedRows.Count == 0) return null;
        var id = dgvTrans.SelectedRows[0].Cells["Id"].Value;
        if (id == null) return null;
        return _db.MerrTransaksionById(Convert.ToInt32(id));
    }

    private string MerrPdfPathSelected()
    {
        if (dgvTrans.SelectedRows.Count == 0) return "";
        var id = Convert.ToInt32(dgvTrans.SelectedRows[0].Cells["Id"].Value);
        var tipi = dgvTrans.SelectedRows[0].Cells["Tipi"]?.Value?.ToString();
        var viti = DateTime.Now.Year;
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ArmepunesApp", "Fleteleshimat", viti.ToString());
        var tipStr = tipi == "Hyrje" ? "HYRJE" : "DALJE";
        return Path.Combine(dir, $"FL_{tipStr}_{viti}_{id:D6}.pdf");
    }

    private void PerditesoStatusin()
    {
        var row = MerrTransaksionSelected();
        if (row == null) { lblStatus.Text = $"Gjithsej: {dgvTrans.Rows.Count} transaksione"; return; }
        var pdfPath = MerrPdfPathSelected();
        var pdfExists = File.Exists(pdfPath);
        var pngPath = Path.ChangeExtension(pdfPath, ".png");
        var pngExists = File.Exists(pngPath);
        var status = pdfExists ? "PDF ekziston" : pngExists ? "PNG ekziston" : "Pa PDF te gjeneruar";
        lblStatus.Text = $"Gjithsej: {dgvTrans.Rows.Count} | Zgjedhur ID: {row["Id"]} | {status}";
    }

    private void PrintoFleteleshimSelected()
    {
        var row = MerrTransaksionSelected();
        if (row == null) { MessageBox.Show("Zgjidh nje transaksion.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
        using var f = new FleteleshimForm(_db, row, "TRANSAKSION");
        f.ShowDialog(this);
    }

    private void HapPdfSelected()
    {
        var path = MerrPdfPathSelected();
        if (string.IsNullOrEmpty(path)) return;
        if (File.Exists(path))
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(path) { UseShellExecute = true });
            return;
        }
        var pngPath = Path.ChangeExtension(path, ".png");
        if (File.Exists(pngPath))
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(pngPath) { UseShellExecute = true });
            return;
        }
        MessageBox.Show("As PDF as PNG nuk u gjeten per kete transaksion.\nPerdor butonin 'Gjenero PDF' per ta krijuar.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void GjeneroPdfSelected()
    {
        var row = MerrTransaksionSelected();
        if (row == null) { MessageBox.Show("Zgjidh nje transaksion.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
        var id = Convert.ToInt32(row["Id"]);
        try
        {
            var path = ExportHelper.EksportoFleteleshimAuto(_db, id, _perdoruesi);
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                MessageBox.Show($"PDF u gjenerua me sukses:\n{path}", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                PerditesoStatusin();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Gabim gjate gjenerimit te PDF:\n{ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void PrintoListen()
    {
        if (_transData.Rows.Count == 0) { MessageBox.Show("Nuk ka te dhena.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
        try
        {
            using var pd = new PrintDocument();
            pd.DefaultPageSettings.Landscape = true;
            pd.DefaultPageSettings.PaperSize = new PaperSize("A4", 827, 1169);
            pd.DefaultPageSettings.Margins = new Margins(20, 20, 20, 20);
            if (cmbPrinter.SelectedItem != null)
                pd.PrinterSettings.PrinterName = cmbPrinter.SelectedItem.ToString()!;
            pd.PrintPage += (s, e) =>
            {
                var g = e.Graphics!;
                g.PageUnit = GraphicsUnit.Display;
                var m = 20f;
                float x0 = m, y = m, w = e.PageBounds.Width - m * 2;
                float lh = 18f;

                var fontTitle = new Font("Segoe UI", 14, FontStyle.Bold);
                var fontSub = new Font("Segoe UI", 10, FontStyle.Bold);
                var fontHeader = new Font("Segoe UI", 8, FontStyle.Bold);
                var fontNormal = new Font("Segoe UI", 7.5f);
                var fontSmall = new Font("Segoe UI", 6.5f);

                g.DrawString("POLIGONI DRENI", fontTitle, Brushes.Black, x0, y); y += 24;
                g.DrawString("Qendra e Deponimit dhe Menaxhimit te Armeve", fontSub, Brushes.Black, x0, y); y += 18;
                g.DrawString("LISTA E TRANSAKSIONEVE", fontSub, Brushes.Black, x0, y); y += 20;

                var cols = new[] { "Nr.", "Tipi", "Data/Ora", "Klienti", "Personeli", "Seriali", "Qellimi" };
                var colW = new[] { 28f, 50f, 110f, 130f, 130f, 100f, 0f };
                float totalFixed = 0;
                for (int i = 0; i < colW.Length - 1; i++) totalFixed += colW[i];
                colW[colW.Length - 1] = w - totalFixed - 10;

                using var headerBg = new SolidBrush(Color.FromArgb(0, 60, 110));
                g.FillRectangle(headerBg, x0, y, w, lh + 2);
                float cx = x0;
                for (int i = 0; i < cols.Length; i++)
                {
                    g.DrawString(cols[i], fontHeader, Brushes.White, cx + 3, y + 1);
                    cx += colW[i] + 2;
                }
                y += lh + 4;

                int nr = 0;
                foreach (DataRow r in _transData.Rows)
                {
                    if (y + lh > e.PageBounds.Height - m) { e.HasMorePages = true; break; }
                    nr++;
                    var tipi = r["Tipi"]?.ToString() ?? "";
                    var dataOra = (r["DataOra"]?.ToString() ?? "").Length >= 16
                        ? (r["DataOra"]?.ToString() ?? "").Replace("-", ".")[..16] : r["DataOra"]?.ToString() ?? "";
                    var kl = r["KlientiEmri"]?.ToString() ?? "";
                    var per = r["PersoneliEmri"]?.ToString() ?? "";
                    var ser = r["ArmaSerial"]?.ToString() ?? "";
                    var qe = r["Qellimi"]?.ToString() ?? "";
                    var vals = new[] { nr.ToString(), tipi, dataOra, kl, per, ser, qe };
                    if (nr % 2 == 0)
                    {
                        using var rowBrush = new SolidBrush(Color.FromArgb(245, 247, 250));
                        g.FillRectangle(rowBrush, x0, y, w, lh);
                    }
                    using var tipBrush = new SolidBrush(tipi == "Hyrje" ? Color.FromArgb(39, 174, 96) : Color.FromArgb(192, 57, 43));
                    cx = x0;
                    for (int i = 0; i < vals.Length; i++)
                    {
                        var f = (i == 1) ? fontHeader : fontNormal;
                        var b = (i == 1) ? tipBrush : Brushes.Black;
                        g.DrawString(vals[i], f, b, cx + 3, y + 1);
                        cx += colW[i] + 2;
                    }
                    y += lh;
                }

                fontTitle.Dispose(); fontSub.Dispose(); fontHeader.Dispose(); fontNormal.Dispose(); fontSmall.Dispose();
                e.HasMorePages = false;
            };

            using var dlg = new PrintPreviewDialog
            {
                Document = pd,
                Width = 1000,
                Height = 750,
                Text = "Parashiko - Lista e Transaksioneve"
            };
            dlg.ShowDialog(this);
        }
        catch (Exception ex) { MessageBox.Show($"Gabim ne printim:\n{ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }
}
