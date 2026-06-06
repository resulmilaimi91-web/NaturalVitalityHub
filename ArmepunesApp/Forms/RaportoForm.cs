using System.Data;
using System.Drawing.Printing;
using ArmepunesApp.Data;

namespace ArmepunesApp.Forms;

public partial class RaportoForm : Form
{
    private readonly DatabaseHelper _db;
    private readonly bool _isAdmin;
    private DataTable _armet = null!;
    private DataTable _transaksionet = null!;
    private DataTable _personeli = null!;
    private DataTable _klientet = null!;
    private ComboBox cmbFilterTipi = null!;
    private DateTimePicker dtpPrej = null!;
    private DateTimePicker dtpDeri = null!;

    // Template
    private string _tmpHeaderTitle = "POLIGONI DRENI";
    private string _tmpHeaderSub = "Qendra e Deponimit dhe Menaxhimit te Armeve";
    private string _tmpHeaderAddr = "Prishtine, Republika e Kosoves";
    private string _tmpSig1 = "Pergjegjesi i Depos";
    private string _tmpSig2 = "Personeli Pranues";
    private string _tmpSig3 = "Drejtuesi";
    private string _tmpFooter = "Dokument i gjeneruar nga Sistemi Deponim i Armeve";
    private Color _tmpHeaderColor = Color.FromArgb(0, 70, 130);
    private Color _tmpTextColor = Color.Black;
    private int _tmpFontTitle = 18;
    private int _tmpFontNormal = 10;

    public RaportoForm(DatabaseHelper db, bool isAdmin = false)
    {
        _db = db;
        _isAdmin = isAdmin;
        InitializeComponent();
        MbushPrinterat();
        try { LoadData(); }
        catch { }
    }

    private void MbushPrinterat()
    {
        cmbPrinter.Items.Clear();
        foreach (string printer in System.Drawing.Printing.PrinterSettings.InstalledPrinters)
            cmbPrinter.Items.Add(printer);
        if (cmbPrinter.Items.Count > 0)
        {
            string defaultPrinter = new System.Drawing.Printing.PrinterSettings().PrinterName;
            for (int i = 0; i < cmbPrinter.Items.Count; i++)
                if (cmbPrinter.Items[i]?.ToString() == defaultPrinter)
                { cmbPrinter.SelectedIndex = i; return; }
            cmbPrinter.SelectedIndex = 0;
        }
    }

    private void LoadData()
    {
        _armet = _db.MerrArmet() ?? new DataTable();
        _transaksionet = _db.MerrTransaksionet() ?? new DataTable();
        _personeli = _db.MerrPersonelin() ?? new DataTable();
        _klientet = _db.MerrKlientet() ?? new DataTable();
        NgarkoTemplate();
        NgarkoStokun();
        NgarkoDaljet();
        NgarkoArmetTekKlienti();
        NgarkoListenArmet();
        NgarkoListenKliente();
        NgarkoListenPersonel();
    }

    private void NgarkoTemplate()
    {
        try
        {
            int tid = _db.MerrTemplateAktivId("Raport");
            if (tid == 0) return;
            var p = _db.MerrTemplateParametrat(tid);
            if (string.IsNullOrEmpty(p) || p == "{}") return;
            var json = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, System.Text.Json.JsonElement>>(p);
            if (json == null) return;
            if (json.TryGetValue("txtHeaderTitle", out var v)) _tmpHeaderTitle = v.GetString() ?? _tmpHeaderTitle;
            if (json.TryGetValue("txtHeaderSub", out v)) _tmpHeaderSub = v.GetString() ?? _tmpHeaderSub;
            if (json.TryGetValue("txtHeaderAddr", out v)) _tmpHeaderAddr = v.GetString() ?? _tmpHeaderAddr;
            if (json.TryGetValue("txtSig1", out v)) _tmpSig1 = v.GetString() ?? _tmpSig1;
            if (json.TryGetValue("txtSig2", out v)) _tmpSig2 = v.GetString() ?? _tmpSig2;
            if (json.TryGetValue("txtSig3", out v)) _tmpSig3 = v.GetString() ?? _tmpSig3;
            if (json.TryGetValue("txtFooter", out v)) _tmpFooter = v.GetString() ?? _tmpFooter;
            if (json.TryGetValue("headerColor", out var c)) _tmpHeaderColor = Color.FromArgb(c.GetInt32());
            if (json.TryGetValue("textColor", out c)) _tmpTextColor = Color.FromArgb(c.GetInt32());
            if (json.TryGetValue("fontTitle", out var f)) _tmpFontTitle = f.GetInt32();
            if (json.TryGetValue("fontNormal", out f)) _tmpFontNormal = f.GetInt32();
        }
        catch { }
    }

    private void Filtro()
    {
        LoadData();
    }

    private void NgarkoArmetTekKlienti()
    {
        var dt = new DataTable();
        dt.Columns.Add("Nr", typeof(int));
        dt.Columns.Add("Seriali");
        dt.Columns.Add("Marka");
        dt.Columns.Add("Modeli");
        dt.Columns.Add("Kalibri");
        dt.Columns.Add("Lloji");
        dt.Columns.Add("Klienti");
        dt.Columns.Add("DataDaljes");
        dt.Columns.Add("Qellimi");

        try
        {
            var data = _db.MerrArmetTekKlienti() ?? new DataTable();
            int nr = 1;
            foreach (DataRow r in data.Rows)
                dt.Rows.Add(nr++, r["Seriali"], r["Marka"], r["Modeli"], r["Kalibri"], r["Lloji"],
                    r["Klienti"],
                    r["DataDaljes"] != DBNull.Value && r["DataDaljes"] != null
                        ? r["DataDaljes"].ToString()!.Replace("-", ".").Substring(0, 10)
                        : "-",
                    r["Qellimi"] ?? "-");
        }
        catch (Exception ex) { MessageBox.Show($"Gabim: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        dgvArmetKlient.DataSource = dt;
        if (dgvArmetKlient.Columns["Nr"] != null) dgvArmetKlient.Columns["Nr"].Width = 40;
    }

    private void NgarkoListenArmet()
    {
        var dt = new DataTable();
        dt.Columns.Add("Id", typeof(int));
        dt.Columns.Add("Nr", typeof(int));
        dt.Columns.Add("Seriali");
        dt.Columns.Add("Marka");
        dt.Columns.Add("Modeli");
        dt.Columns.Add("Kalibri");
        dt.Columns.Add("Lloji");
        dt.Columns.Add("NrInventari");
        dt.Columns.Add("Statusi");

        try
        {
            var data = _db.MerrArmet() ?? new DataTable();
            int nr = 1;
            foreach (DataRow r in data.Rows)
                dt.Rows.Add(r["Id"], nr++, r["NumerSerial"], r["Marka"], r["Modeli"], r["Kalibri"], r["Lloji"],
                    r["NrInventari"], r["Statusi"]);
        }
        catch (Exception ex) { MessageBox.Show($"Gabim: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        dgvListaArmet.DataSource = dt;
        if (dgvListaArmet.Columns["Id"] != null) dgvListaArmet.Columns["Id"].Visible = false;
        if (dgvListaArmet.Columns["Nr"] != null) dgvListaArmet.Columns["Nr"].Width = 40;
        LidhKontekstArmet();
    }

    private void NgarkoListenKliente()
    {
        var dt = new DataTable();
        dt.Columns.Add("Id", typeof(int));
        dt.Columns.Add("Nr", typeof(int));
        dt.Columns.Add("Emri");
        dt.Columns.Add("Mbiemri");
        dt.Columns.Add("Adresa");
        dt.Columns.Add("Telefon");
        dt.Columns.Add("Email");
        dt.Columns.Add("NrLeternjoftimit");

        try
        {
            var data = _db.MerrKlientet() ?? new DataTable();
            int nr = 1;
            foreach (DataRow r in data.Rows)
                dt.Rows.Add(r["Id"], nr++, r["Emri"], r["Mbiemri"], r["Adresa"] ?? "-", r["Telefon"] ?? "-",
                    r["Email"] ?? "-", r["NrLeternjoftimit"] ?? "-");
        }
        catch (Exception ex) { MessageBox.Show($"Gabim: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        dgvListaKliente.DataSource = dt;
        if (dgvListaKliente.Columns["Id"] != null) dgvListaKliente.Columns["Id"].Visible = false;
        if (dgvListaKliente.Columns["Nr"] != null) dgvListaKliente.Columns["Nr"].Width = 40;
        LidhKontekstKlient();
    }

    private void LidhKontekstArmet()
    {
        var menu = new ContextMenuStrip();
        if (_isAdmin)
            menu.Items.Add("Fshi Arme", null, (s, e) =>
        {
            if (dgvListaArmet.SelectedRows.Count == 0) return;
            var idObj = dgvListaArmet.SelectedRows[0].Cells["Id"]?.Value;
            if (idObj == null) return;
            int id = Convert.ToInt32(idObj);
            var serial = dgvListaArmet.SelectedRows[0].Cells["Seriali"]?.Value?.ToString() ?? "";
            if (MessageBox.Show($"A je i sigurt qe don te fshish armen me serial: {serial}?",
                "Konfirmim", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            if (_db.FshiArme(id))
            {
                NgarkoListenArmet();
                NgarkoStokun();
                NgarkoArmetTekKlienti();
            }
            else
                MessageBox.Show("Arma nuk mund te fshihet.", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
        });
        menu.ForeColor = Color.Black;
        dgvListaArmet.ContextMenuStrip = menu;
    }

    private void LidhKontekstKlient()
    {
        var menu = new ContextMenuStrip();
        if (_isAdmin)
            menu.Items.Add("Fshi Klient", null, (s, e) =>
        {
            if (dgvListaKliente.SelectedRows.Count == 0) return;
            var idObj = dgvListaKliente.SelectedRows[0].Cells["Id"]?.Value;
            if (idObj == null) return;
            int id = Convert.ToInt32(idObj);
            var emri = dgvListaKliente.SelectedRows[0].Cells["Emri"]?.Value?.ToString() ?? "";
            var mbiemri = dgvListaKliente.SelectedRows[0].Cells["Mbiemri"]?.Value?.ToString() ?? "";
            if (MessageBox.Show($"A je i sigurt qe don te fshish klientin: {emri} {mbiemri}?",
                "Konfirmim", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            if (_db.FshiKlient(id))
            {
                NgarkoListenKliente();
            }
            else
                MessageBox.Show("Klienti nuk mund te fshihet.", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
        });
        menu.ForeColor = Color.Black;
        dgvListaKliente.ContextMenuStrip = menu;
    }

    private void LidhKontekstPersonel()
    {
        var menu = new ContextMenuStrip();
        if (_isAdmin)
            menu.Items.Add("Fshi Personel", null, (s, e) =>
        {
            if (dgvListaPersonel.SelectedRows.Count == 0) return;
            var idObj = dgvListaPersonel.SelectedRows[0].Cells["Id"]?.Value;
            if (idObj == null) return;
            int id = Convert.ToInt32(idObj);
            var emri = dgvListaPersonel.SelectedRows[0].Cells["Emri"]?.Value?.ToString() ?? "";
            var mbiemri = dgvListaPersonel.SelectedRows[0].Cells["Mbiemri"]?.Value?.ToString() ?? "";
            if (MessageBox.Show($"A je i sigurt qe don te fshish personelin: {emri} {mbiemri}?\nKjo do te fshije edhe transaksionet e lidhura me te.",
                "Konfirmim", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            if (_db.FshiPersonel(id))
            {
                NgarkoListenPersonel();
            }
            else
                MessageBox.Show("Personeli nuk mund te fshihet.", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
        });
        menu.ForeColor = Color.Black;
        dgvListaPersonel.ContextMenuStrip = menu;
    }

    private void NgarkoListenPersonel()
    {
        var dt = new DataTable();
        dt.Columns.Add("Id", typeof(int));
        dt.Columns.Add("Nr", typeof(int));
        dt.Columns.Add("Emri");
        dt.Columns.Add("Mbiemri");
        dt.Columns.Add("Grada");
        dt.Columns.Add("Njesia");
        dt.Columns.Add("NrLegjitimacioni");
        dt.Columns.Add("Telefon");

        try
        {
            var data = _db.MerrPersonelin() ?? new DataTable();
            int nr = 1;
            foreach (DataRow r in data.Rows)
                dt.Rows.Add(r["Id"], nr++, r["Emri"], r["Mbiemri"], r["Grada"], r["Njesia"] ?? "-",
                    r["NrLegjitimacioni"] ?? "-", r["Telefon"] ?? "-");
        }
        catch (Exception ex) { MessageBox.Show($"Gabim: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        dgvListaPersonel.DataSource = dt;
        if (dgvListaPersonel.Columns["Id"] != null) dgvListaPersonel.Columns["Id"].Visible = false;
        if (dgvListaPersonel.Columns["Nr"] != null) dgvListaPersonel.Columns["Nr"].Width = 40;
        LidhKontekstPersonel();
    }

    private void PrintoListaPersonel(bool preview)
    {
        try
        {
            var pd = new PrintDocument();
            pd.PrintPage += (s, e) => PrintoReport(e, dgvListaPersonel, "LISTA E PERSONELIT",
                new[] { "Nr.", "Emri", "Mbiemri", "Grada", "Njesia", "Nr.Legjit.", "Telefon" },
                new[] { 28f, 100f, 100f, 90f, 120f, 100f, 0f });
            pd.DefaultPageSettings.Landscape = true;
            pd.DefaultPageSettings.PaperSize = new PaperSize("A4", 827, 1169);
            pd.DefaultPageSettings.Margins = new Margins(30, 30, 30, 30);
            if (cmbPrinter.SelectedItem != null)
                pd.PrinterSettings.PrinterName = cmbPrinter.SelectedItem.ToString()!;
            if (preview)
            {
                using var dlg = new PrintPreviewDialog { Document = pd, Width = 1000, Height = 750, Text = "Parashiko - Lista e Personelit" };
                dlg.ShowDialog();
            }
            else pd.Print();
        }
        catch (Exception ex) { MessageBox.Show($"Gabim: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void NgarkoStokun()
    {
        var dt = new DataTable();
        dt.Columns.Add("Nr", typeof(int));
        dt.Columns.Add("Seriali");
        dt.Columns.Add("Marka");
        dt.Columns.Add("Modeli");
        dt.Columns.Add("Kalibri");
        dt.Columns.Add("Lloji");
        dt.Columns.Add("Inventar");
        dt.Columns.Add("Deponuesi");
        dt.Columns.Add("DataDeponimit");

        try
        {
            var prej = dtpPrej.Value.Date;
            var deri = dtpDeri.Value.Date.AddDays(1);

            var neMag = _armet.Select("Statusi = 'Ne Magazine'");
            int nr = 1;
            foreach (DataRow r in neMag)
            {
                var sn = r["NumerSerial"]?.ToString() ?? "";
                DataRow? depRow = null;
                try { depRow = GjejTransaksioninFundit(sn, "Hyrje"); } catch { }

                if (depRow != null)
                {
                    var dStr = depRow["DataOra"]?.ToString() ?? "";
                    if (!string.IsNullOrEmpty(dStr) && dStr.Length >= 10)
                    {
                        if (DateTime.TryParse(dStr.Substring(0, 10), out var d))
                        {
                            if (d < prej || d >= deri) continue;
                        }
                    }
                }

                dt.Rows.Add(
                    nr++,
                    sn,
                    r["Marka"],
                    r["Modeli"],
                    r["Kalibri"],
                    r["Lloji"],
                    r["NrInventari"],
                    depRow != null ? depRow["KlientiEmri"] : "-",
                    depRow != null ? depRow["DataOra"]?.ToString()?.Replace("-", ".")?.Substring(0, 10) : "-"
                );
            }
        }
        catch (Exception ex) { MessageBox.Show($"Gabim ne ngarkimin e stokut: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        dgvStoku.DataSource = dt;
        if (dgvStoku.Columns["Nr"] != null) dgvStoku.Columns["Nr"].Width = 40;
    }

    private void NgarkoDaljet()
    {
        var dt = new DataTable();
        dt.Columns.Add("Nr", typeof(int));
        dt.Columns.Add("Seriali");
        dt.Columns.Add("Marka/Modeli");
        dt.Columns.Add("Kalibri");
        dt.Columns.Add("Klienti");
        dt.Columns.Add("Personeli Pranues");
        dt.Columns.Add("DataDaljes");
        dt.Columns.Add("Ora");
        dt.Columns.Add("Qellimi");

        try
        {
            var tipiFilter = cmbFilterTipi.SelectedItem?.ToString();
            var prej = dtpPrej.Value.Date;
            var deri = dtpDeri.Value.Date.AddDays(1);

            var daljet = _transaksionet.Select("Tipi = 'Dalje'", "Id DESC");
            int nr = 1;
            foreach (DataRow r in daljet)
            {
                if (tipiFilter != "Te gjitha" && !string.IsNullOrEmpty(tipiFilter) && (r["Tipi"]?.ToString() ?? "") != tipiFilter)
                    continue;

                var dStr = r["DataOra"]?.ToString() ?? "";
                if (!string.IsNullOrEmpty(dStr) && dStr.Length >= 10)
                {
                    if (DateTime.TryParse(dStr.Substring(0, 10), out var d))
                    {
                        if (d < prej || d >= deri) continue;
                    }
                }

                var serial = r["ArmaSerial"]?.ToString() ?? "";
                var arma = GjejArmen(serial);
                var mm = arma != null ? $"{arma["Marka"]} {arma["Modeli"]}" : serial;

                dt.Rows.Add(
                    nr++,
                    serial,
                    mm,
                    arma?["Kalibri"] ?? "-",
                    r["KlientiEmri"] ?? "-",
                    r["PersoneliEmri"] ?? "-",
                    r["DataOra"]?.ToString()?.Replace("-", ".")?.Substring(0, 10) ?? "-",
                    r["DataOra"]?.ToString()?.Length >= 16 ? r["DataOra"]?.ToString()?.Substring(11, 5) : "-",
                    r["Qellimi"] ?? "-"
                );
            }
        }
        catch (Exception ex) { MessageBox.Show($"Gabim ne ngarkimin e daljeve: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        dgvDaljet.DataSource = dt;
        if (dgvDaljet.Columns["Nr"] != null) dgvDaljet.Columns["Nr"].Width = 40;
    }

    private DataRow? GjejTransaksioninFundit(string serial, string tipi)
    {
        DataRow? found = null;
        try
        {
            foreach (DataRow r in _transaksionet.Rows)
            {
                if ((r["ArmaSerial"]?.ToString() ?? "") == serial && (r["Tipi"]?.ToString() ?? "") == tipi)
                {
                    if (found == null) found = r;
                    else
                    {
                        var d1 = r["DataOra"]?.ToString() ?? "";
                        var d2 = found["DataOra"]?.ToString() ?? "";
                        if (string.Compare(d1, d2) > 0) found = r;
                    }
                }
            }
        }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Gabim ne GjejTransaksioninFundit: {ex.Message}"); }
        return found;
    }

    private DataRow? GjejArmen(string serial)
    {
        try
        {
            foreach (DataRow r in _armet.Rows)
                if ((r["NumerSerial"]?.ToString() ?? "") == serial) return r;
        }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Gabim ne GjejArmen: {ex.Message}"); }
        return null;
    }

    private void btnKerko_Click(object? sender, EventArgs e)
    {
        try
        {
            var serial = txtKerkimSerial.Text.Trim();
            if (serial == "" || serial == "Kerkim nga Nr. Serik...")
            {
                MessageBox.Show("Shkruaj nje numer serik per te kerkuar.", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var dt = new DataTable();
            dt.Columns.Add("Nr", typeof(int));
            dt.Columns.Add("Tipi");
            dt.Columns.Add("DataOra");
            dt.Columns.Add("Klienti");
            dt.Columns.Add("Personeli");
            dt.Columns.Add("Qellimi");
            dt.Columns.Add("Shenime");

            int nr = 1;
            foreach (DataRow r in _transaksionet.Rows)
            {
                if ((r["ArmaSerial"]?.ToString() ?? "").ToUpper().Contains(serial.ToUpper()))
                {
                    var dataOra = r["DataOra"]?.ToString() ?? "";
                    var data = dataOra.Length >= 10 ? dataOra.Replace("-", ".").Substring(0, 10) : "";
                    var ora = dataOra.Length >= 16 ? dataOra.Substring(11, 5) : "";

                    dt.Rows.Add(
                        nr++,
                        r["Tipi"]?.ToString() == "Hyrje" ? "HYRJE" : "DALJE",
                        $"{data} {ora}",
                        r["KlientiEmri"] ?? "-",
                        r["PersoneliEmri"] ?? "-",
                        r["Qellimi"] ?? "-",
                        r["Shenime"] ?? "-"
                    );
                }
            }

            if (dt.Rows.Count == 0)
                MessageBox.Show($"Nuk u gjet asnje transaksion per serialin: {serial}", "Rezultat",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

            dgvAnalitik.DataSource = dt;
            if (dgvAnalitik.Columns["Nr"] != null) dgvAnalitik.Columns["Nr"].Width = 40;
            tabReports.SelectedTab = tabAnalitik;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Gabim gjate kerkimit:\n{ex.Message}", "Gabim",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void PrintoStokun(bool preview)
    {
        try
        {
            var pd = new PrintDocument();
            pd.PrintPage += (s, e) => PrintoReport(e, dgvStoku, "GJENDJA E STOKUT - DEPONIM I ARMEVE",
                new[] { "Nr.", "Seriali", "Marka", "Modeli", "Kalibri", "Lloji", "Inventar", "Deponuesi", "Data Dep." },
                new[] { 28f, 80f, 110f, 100f, 65f, 60f, 60f, 100f, 0f });
            pd.DefaultPageSettings.Landscape = true;
            pd.DefaultPageSettings.PaperSize = new PaperSize("A4", 827, 1169);
            pd.DefaultPageSettings.Margins = new Margins(30, 30, 30, 30);
            if (cmbPrinter.SelectedItem != null)
                pd.PrinterSettings.PrinterName = cmbPrinter.SelectedItem.ToString()!;
            if (preview)
            {
                using var dlg = new PrintPreviewDialog
                {
                    Document = pd,
                    Width = 1000,
                    Height = 750,
                    Text = "Parashiko - Gjendja e Stokut"
                };
                dlg.ShowDialog();
            }
            else { pd.Print(); }
        }
        catch (Exception ex) { MessageBox.Show($"Gabim ne printim:\n{ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void PrintoDaljet(bool preview)
    {
        try
        {
            var pd = new PrintDocument();
            pd.PrintPage += (s, e) => PrintoReport(e, dgvDaljet, "LISTA E DALJEVE NGA DEPO",
                new[] { "Nr.", "Seriali", "Marka/Modeli", "Kalibri", "Klienti", "Personeli", "Data", "Ora", "Qellimi" },
                new[] { 28f, 80f, 110f, 65f, 110f, 100f, 60f, 40f, 0f });
            pd.DefaultPageSettings.Landscape = true;
            pd.DefaultPageSettings.PaperSize = new PaperSize("A4", 827, 1169);
            pd.DefaultPageSettings.Margins = new Margins(30, 30, 30, 30);
            if (cmbPrinter.SelectedItem != null)
                pd.PrinterSettings.PrinterName = cmbPrinter.SelectedItem.ToString()!;
            if (preview)
            {
                using var dlg = new PrintPreviewDialog
                {
                    Document = pd,
                    Width = 1000,
                    Height = 750,
                    Text = "Parashiko - Lista e Daljeve"
                };
                dlg.ShowDialog();
            }
            else { pd.Print(); }
        }
        catch (Exception ex) { MessageBox.Show($"Gabim ne printim:\n{ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void PrintoArmetKlient(bool preview)
    {
        try
        {
            var pd = new PrintDocument();
            pd.PrintPage += (s, e) => PrintoReport(e, dgvArmetKlient, "ARMET TEK KLIENTI",
                new[] { "Nr.", "Seriali", "Marka", "Modeli", "Kalibri", "Lloji", "Klienti", "Data Daljes", "Qellimi" },
                new[] { 28f, 80f, 100f, 90f, 60f, 60f, 100f, 70f, 0f });
            pd.DefaultPageSettings.Landscape = true;
            pd.DefaultPageSettings.PaperSize = new PaperSize("A4", 827, 1169);
            pd.DefaultPageSettings.Margins = new Margins(30, 30, 30, 30);
            if (cmbPrinter.SelectedItem != null)
                pd.PrinterSettings.PrinterName = cmbPrinter.SelectedItem.ToString()!;
            if (preview)
            {
                using var dlg = new PrintPreviewDialog { Document = pd, Width = 1000, Height = 750, Text = "Parashiko - Armet Tek Klienti" };
                dlg.ShowDialog();
            }
            else pd.Print();
        }
        catch (Exception ex) { MessageBox.Show($"Gabim: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void PrintoListaArmet(bool preview)
    {
        try
        {
            var pd = new PrintDocument();
            pd.PrintPage += (s, e) => PrintoReport(e, dgvListaArmet, "LISTA E ARMEVE - INVENTARI",
                new[] { "Nr.", "Seriali", "Marka", "Modeli", "Kalibri", "Lloji", "Nr.Inventar", "Statusi" },
                new[] { 28f, 80f, 110f, 100f, 65f, 70f, 90f, 0f });
            pd.DefaultPageSettings.Landscape = true;
            pd.DefaultPageSettings.PaperSize = new PaperSize("A4", 827, 1169);
            pd.DefaultPageSettings.Margins = new Margins(30, 30, 30, 30);
            if (cmbPrinter.SelectedItem != null)
                pd.PrinterSettings.PrinterName = cmbPrinter.SelectedItem.ToString()!;
            if (preview)
            {
                using var dlg = new PrintPreviewDialog { Document = pd, Width = 1000, Height = 750, Text = "Parashiko - Lista e Armeve" };
                dlg.ShowDialog();
            }
            else pd.Print();
        }
        catch (Exception ex) { MessageBox.Show($"Gabim: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void PrintoListaKliente(bool preview)
    {
        try
        {
            var pd = new PrintDocument();
            pd.PrintPage += (s, e) => PrintoReport(e, dgvListaKliente, "LISTA E KLIENTEVE",
                new[] { "Nr.", "Emri", "Mbiemri", "Adresa", "Telefon", "Email", "Nr.Leternjoftimit" },
                new[] { 28f, 100f, 100f, 140f, 100f, 120f, 0f });
            pd.DefaultPageSettings.Landscape = true;
            pd.DefaultPageSettings.PaperSize = new PaperSize("A4", 827, 1169);
            pd.DefaultPageSettings.Margins = new Margins(30, 30, 30, 30);
            if (cmbPrinter.SelectedItem != null)
                pd.PrinterSettings.PrinterName = cmbPrinter.SelectedItem.ToString()!;
            if (preview)
            {
                using var dlg = new PrintPreviewDialog { Document = pd, Width = 1000, Height = 750, Text = "Parashiko - Lista e Klienteve" };
                dlg.ShowDialog();
            }
            else pd.Print();
        }
        catch (Exception ex) { MessageBox.Show($"Gabim: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void PrintoAnalitik(bool preview)
    {
        try
        {
            var pd = new PrintDocument();
            pd.PrintPage += (s, e) => PrintoReport(e, dgvAnalitik, "RAPORT ANALITIK - HISTORIKU I TRANSAKSIONEVE",
                new[] { "Nr.", "Tipi", "Data/Ora", "Klienti", "Personeli", "Qellimi", "Shenime" },
                new[] { 28f, 60f, 100f, 100f, 100f, 110f, 0f });
            pd.DefaultPageSettings.Landscape = true;
            pd.DefaultPageSettings.PaperSize = new PaperSize("A4", 827, 1169);
            pd.DefaultPageSettings.Margins = new Margins(30, 30, 30, 30);
            if (cmbPrinter.SelectedItem != null)
                pd.PrinterSettings.PrinterName = cmbPrinter.SelectedItem.ToString()!;
            if (preview)
            {
                using var dlg = new PrintPreviewDialog
                {
                    Document = pd,
                    Width = 1000,
                    Height = 750,
                    Text = "Parashiko - Raport Analitik"
                };
                dlg.ShowDialog();
            }
            else { pd.Print(); }
        }
        catch (Exception ex) { MessageBox.Show($"Gabim ne printim:\n{ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void PrintoReport(PrintPageEventArgs e, DataGridView dgv, string titull, string[] headers, float[] colWidths)
    {
        var g = e.Graphics!;
        var page = e.PageBounds;
        float m = 35;
        float x0 = m;
        float y = m + 5;
        float w = page.Width - m * 2;
        float lh = 20;
        float usableH = page.Height - m * 2;

        float sumFixed = 0;
        for (int i = 0; i < colWidths.Length - 1; i++) sumFixed += colWidths[i];
        colWidths[^1] = w - 4 - sumFixed;

        using var fontTitle = new Font("Segoe UI", Math.Max(10, _tmpFontTitle), FontStyle.Bold);
        using var fontSub = new Font("Segoe UI", Math.Max(8, _tmpFontTitle - 6), FontStyle.Bold);
        using var fontHeader = new Font("Segoe UI", Math.Max(8, _tmpFontNormal), FontStyle.Bold);
        using var fontNormal = new Font("Segoe UI", Math.Max(7, _tmpFontNormal - 1));
        using var fontSmall = new Font("Segoe UI", Math.Max(6, _tmpFontNormal - 2));
        using var brHeader = new SolidBrush(_tmpHeaderColor);
        using var brText = new SolidBrush(_tmpTextColor);
        using var brAlt = new SolidBrush(Color.FromArgb(240, 244, 250));
        using var penBorder = new Pen(_tmpHeaderColor, 1.5f);

        // ── HEADER ──
        g.DrawString(_tmpHeaderTitle, fontTitle, brHeader, x0 + 2, y);
        y += fontTitle.Height + 4;
        g.DrawString(_tmpHeaderSub, fontSub, brText, x0 + 2, y);
        y += fontSub.Height + 2;
        g.DrawString(_tmpHeaderAddr, fontNormal, brText, x0 + 2, y);
        y += fontNormal.Height + 4;
        using var topPen = new Pen(_tmpHeaderColor, 2);
        g.DrawLine(topPen, x0, y, x0 + w, y);
        y += 10;
        g.DrawString(titull, fontSub, brText, x0 + 2, y);
        g.DrawString($"Data: {DateTime.Now:dd.MM.yyyy}  Ora: {DateTime.Now:HH:mm}", fontNormal, brText, x0 + w - 150, y);
        y += fontSub.Height + 6;

        // ── TABLE ──
        float[] colX = new float[colWidths.Length];
        colX[0] = x0 + 2;
        for (int i = 1; i < colWidths.Length; i++)
            colX[i] = colX[i - 1] + colWidths[i - 1];
        float tableW = colWidths.Sum();

        g.FillRectangle(brHeader, x0 + 2, y, tableW, lh + 2);
        for (int i = 0; i < headers.Length; i++)
            g.DrawString(headers[i], fontHeader, Brushes.White, colX[i] + 4, y + 2);
        y += lh + 2;

        bool alt = false;
        var rows = dgv.Rows.Cast<DataGridViewRow>().ToArray();
        int printed = 0;
        foreach (var row in rows)
        {
            if (y + lh > usableH + m - 40) break;
            if (alt) g.FillRectangle(brAlt, x0 + 2, y, tableW, lh);
            alt = !alt;
            for (int i = 0; i < Math.Min(headers.Length, row.Cells.Count); i++)
            {
                var val = row.Cells[i].Value?.ToString() ?? "";
                g.DrawString(val, fontNormal, brText, colX[i] + 4, y + 1);
            }
            using var lightPen = new Pen(Color.FromArgb(180, 190, 200), 0.5f);
            g.DrawLine(lightPen, x0 + 2, y + lh, x0 + tableW, y + lh);
            y += lh + 2;
            printed++;
        }

        g.DrawRectangle(penBorder, x0 + 2, y - (lh + 2) * printed, tableW, (lh + 2) * printed);

        y += 12;
        g.DrawString($"Gjithsej: {rows.Length} rreshta  |  Printuar: {printed}", fontSub, brText, x0 + 2, y);
        using var sumPen = new Pen(_tmpHeaderColor, 1);
        g.DrawLine(sumPen, x0, y + 20, x0 + w, y + 20);
        y += 28;

        // ── SIGNATURES ──
        float sigW = (tableW - 60) / 3;
        float[] sigX = { x0 + 10, x0 + sigW + 30, x0 + sigW * 2 + 50 };
        string[] sigLabels = { _tmpSig1.ToUpper(), _tmpSig2.ToUpper(), _tmpSig3.ToUpper() };
        for (int i = 0; i < 3; i++)
        {
            using var sigPen = new Pen(Color.FromArgb(80, 80, 80), 1);
            g.DrawRectangle(sigPen, sigX[i], y, sigW, 50);
            g.DrawString(sigLabels[i], fontSmall, Brushes.DimGray, sigX[i] + 5, y + 4);
            g.DrawLine(sigPen, sigX[i] + 5, y + 32, sigX[i] + sigW - 5, y + 32);
            g.DrawString("Nenshkrimi / Data", fontSmall, Brushes.Gray, sigX[i] + 5, y + 34);
        }
        y += 58;

        // ── FOOTER ──
        using var footPen = new Pen(_tmpHeaderColor, 2);
        g.DrawLine(footPen, x0, y, x0 + w, y);
        y += 6;
        g.DrawString(_tmpFooter, fontSmall, Brushes.Gray, x0 + 2, y);
        g.DrawString($"{DateTime.Now:dd.MM.yyyy HH:mm:ss}", fontSmall, Brushes.Gray, x0 + w - 80, y);
    }
}
