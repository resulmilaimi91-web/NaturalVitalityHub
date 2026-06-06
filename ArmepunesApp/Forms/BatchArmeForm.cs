using System.Data;
using ArmepunesApp.Data;
using ArmepunesApp.Models;
using ClosedXML.Excel;

namespace ArmepunesApp.Forms;

public partial class BatchArmeForm : Form
{
    private DatabaseHelper _db;
    private DataTable _dt;
    private DataGridView dgv;
    private Label lblStatus;

    public BatchArmeForm(DatabaseHelper db)
    {
        _db = db;
        InitializeForm();
        _dt = KrijoDataTabelen();
        dgv.DataSource = _dt;
    }

    private void InitializeForm()
    {
        Text = "Regjistro Arme ne Shumice (Batch Import/Export)";
        Size = new Size(1200, 700);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(30, 33, 40);
        Font = new Font("Segoe UI", 9);
        MinimizeBox = false;
        ShowIcon = false;
        ShowInTaskbar = false;

        var lblTitle = new Label
        {
            Text = "REGJISTRO ARME NE SHUMICE",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Color.FromArgb(0, 200, 255),
            Size = new Size(600, 30),
            Location = new Point(15, 12)
        };

        lblStatus = new Label
        {
            Text = "Gati.",
            ForeColor = Color.FromArgb(140, 145, 155),
            Size = new Size(900, 20),
            Location = new Point(15, 44)
        };

        int bx = 15, by = 68, bw = 130, gap = 6;

        var btnShtoRresht = Btn("+ Shto Rresht", bw, Color.FromArgb(46, 204, 113), bx, by);
        btnShtoRresht.Click += (_, _) => ShtoRresht();

        var btnFshiRresht = Btn("✕ Fshi Rresht", bw, Color.FromArgb(231, 76, 60), bx + (bw + gap), by);
        btnFshiRresht.Click += (_, _) => FshiRresht();

        var btnImporto = Btn("\uD83D\uDCC2 Importo Excel", bw + 20, Color.FromArgb(52, 152, 219), bx + (bw + gap) * 2, by);
        btnImporto.Click += (_, _) => ImportoExcel();

        var btnExporto = Btn("\uD83D\uDCC4 Exporto Excel", bw + 20, Color.FromArgb(155, 89, 182), bx + (bw + gap) * 3, by);
        btnExporto.Click += (_, _) => ExportoExcel();

        var btnExpDepo = Btn("\uD83D\uDCE6 Deponim", bw + 10, Color.FromArgb(0, 150, 136), bx + (bw + gap) * 4, by);
        btnExpDepo.Click += (_, _) => ExportoDeponim();

        var btnRuaj = Btn("\u2714 Ruaj te Gjitha", bw + 10, Color.FromArgb(39, 174, 96), bx + (bw + gap) * 5, by);
        btnRuaj.Click += (_, _) => RuajTeGjitha();

        var btnMbyll = Btn("✕ Mbyll", 90, Color.FromArgb(120, 130, 145), bx + (bw + gap) * 6, by);
        btnMbyll.Click += (_, _) => Close();

        dgv = new DataGridView
        {
            Location = new Point(15, 110),
            Size = new Size(1140, 520),
            BackgroundColor = Color.FromArgb(35, 37, 43),
            ForeColor = Color.FromArgb(200, 205, 216),
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Segoe UI", 9),
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            RowHeadersVisible = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            ColumnHeadersHeight = 30,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            GridColor = Color.FromArgb(55, 58, 65)
        };

        Controls.AddRange(new Control[] {
            lblTitle, lblStatus,
            btnShtoRresht, btnFshiRresht, btnImporto, btnExporto, btnExpDepo, btnRuaj, btnMbyll,
            dgv
        });
    }

    private static Button Btn(string text, int w, Color c, int x, int y)
    {
        return new Button
        {
            Text = text, Size = new Size(w, 34), Location = new Point(x, y),
            FlatStyle = FlatStyle.Flat, BackColor = c, ForeColor = Color.White,
            Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand,
            UseVisualStyleBackColor = false
        };
    }

    private static DataTable KrijoDataTabelen()
    {
        var dt = new DataTable();
        dt.Columns.Add("NumerSerial", typeof(string));
        dt.Columns.Add("Lloji", typeof(string));
        dt.Columns.Add("Marka", typeof(string));
        dt.Columns.Add("Modeli", typeof(string));
        dt.Columns.Add("Kalibri", typeof(string));
        dt.Columns.Add("Vendlindja", typeof(string));
        dt.Columns.Add("NrInventari", typeof(string));
        dt.Columns.Add("Statusi", typeof(string));
        dt.Columns.Add("Shenime", typeof(string));
        return dt;
    }

    private void VendosTitujt()
    {
        foreach (DataGridViewColumn col in dgv.Columns)
        {
            switch (col.Name)
            {
                case "NumerSerial": col.HeaderText = "Seriali*"; col.Width = 140; break;
                case "Lloji": col.HeaderText = "Lloji*"; col.Width = 80; break;
                case "Marka": col.HeaderText = "Marka*"; col.Width = 100; break;
                case "Modeli": col.HeaderText = "Modeli*"; col.Width = 100; break;
                case "Kalibri": col.HeaderText = "Kalibri*"; col.Width = 80; break;
                case "Vendlindja": col.HeaderText = "Vendlindja"; col.Width = 100; break;
                case "NrInventari": col.HeaderText = "Nr Inventari"; col.Width = 100; break;
                case "Statusi": col.HeaderText = "Statusi"; col.Width = 90; break;
                case "Shenime": col.HeaderText = "Shenime"; col.Width = 200; break;
            }
            col.DefaultCellStyle.BackColor = Color.FromArgb(40, 42, 48);
        }
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        VendosTitujt();
    }

    private void ShtoRresht()
    {
        try
        {
            var row = _dt.NewRow();
            row["NumerSerial"] = "";
            row["Lloji"] = "";
            row["Marka"] = "";
            row["Modeli"] = "";
            row["Kalibri"] = "";
            row["Vendlindja"] = "";
            row["NrInventari"] = "";
            row["Statusi"] = "Ne Magazine";
            row["Shenime"] = "";
            _dt.Rows.Add(row);
            lblStatus.Text = $"Rreshti u shtua. Gjithsej: {_dt.Rows.Count}";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Gabim: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void FshiRresht()
    {
        try
        {
            if (dgv.SelectedRows.Count == 0) return;
            if (dgv.SelectedRows[0].Index < 0 || dgv.SelectedRows[0].Index >= _dt.Rows.Count) return;
            var list = new List<int>();
            foreach (DataGridViewRow r in dgv.SelectedRows)
                if (r.Index >= 0 && r.Index < _dt.Rows.Count)
                    list.Add(r.Index);
            list.Sort((a, b) => b.CompareTo(a));
            foreach (int idx in list)
                _dt.Rows.RemoveAt(idx);
            lblStatus.Text = $"Rreshti u fshi. Gjithsej: {_dt.Rows.Count}";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Gabim: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ImportoExcel()
    {
        using var dlg = new OpenFileDialog
        {
            Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*",
            Title = "Zgjidh nje fajll Excel per import"
        };
        if (dlg.ShowDialog() != DialogResult.OK) return;

        try
        {
            using var wb = new XLWorkbook(dlg.FileName);
            var ws = wb.Worksheet(1);
            var range = ws.RangeUsed();
            if (range == null) throw new Exception("Fajlli eshte bosh.");

            var rows = range.Rows().ToList();
            if (rows.Count < 2) throw new Exception("Nuk ka te dhena (duhet header + minimum 1 rresht).");

            var colMap = new Dictionary<string, int>();
            int maxCol = range.ColumnCount();
            for (int ci = 1; ci <= maxCol; ci++)
            {
                var hdr = ws.Cell(1, ci).GetString().Trim().ToLower();
                if (!string.IsNullOrEmpty(hdr))
                    colMap[hdr] = ci;
            }

            if (colMap.Count == 0)
                throw new Exception("Nuk u gjet asnje header ne rreshtin e pare.");

            string[] cols = { "NumerSerial", "Lloji", "Marka", "Modeli", "Kalibri", "Vendlindja", "NrInventari", "Statusi", "Shenime" };

            int imported = 0;
            for (int ri = 2; ri <= rows.Count; ri++)
            {
                bool hasData = false;
                var row = _dt.NewRow();
                row["Statusi"] = "Ne Magazine";
                foreach (var col in cols)
                {
                    if (colMap.TryGetValue(col.ToLower(), out int ci))
                    {
                        var val = ws.Cell(ri, ci).GetString().Trim();
                        row[col] = val;
                        if (!string.IsNullOrEmpty(val)) hasData = true;
                    }
                }
                if (hasData)
                {
                    _dt.Rows.Add(row);
                    imported++;
                }
            }

            lblStatus.Text = $"Importoi {imported} arme. Gjithsej: {_dt.Rows.Count}";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Gabim ne import: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ExportoExcel()
    {
        if (_dt.Rows.Count == 0)
        { MessageBox.Show("Nuk ka te dhena per te eksportuar.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

        using var dlg = new SaveFileDialog
        {
            Filter = "Excel files (*.xlsx)|*.xlsx",
            Title = "Ruaj fajllin Excel",
            FileName = $"Armet_{DateTime.Now:yyyyMMdd_HHmm}.xlsx"
        };
        if (dlg.ShowDialog() != DialogResult.OK) return;

        try
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Armet");
            string[] headers = { "NumerSerial", "Lloji", "Marka", "Modeli", "Kalibri", "Vendlindja", "NrInventari", "Statusi", "Shenime" };

            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cell(1, i + 1).Value = headers[i];
                ws.Cell(1, i + 1).Style.Font.Bold = true;
                ws.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.FromArgb(0, 70, 130);
                ws.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;
            }

            for (int ri = 0; ri < _dt.Rows.Count; ri++)
            {
                var dr = _dt.Rows[ri];
                for (int ci = 0; ci < headers.Length; ci++)
                    ws.Cell(ri + 2, ci + 1).Value = (dr[headers[ci]] as string) ?? "";
            }

            ws.Columns().AdjustToContents();
            wb.SaveAs(dlg.FileName);
            lblStatus.Text = $"Eksportoi {_dt.Rows.Count} arme ne {dlg.FileName}";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Gabim ne export: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ExportoDeponim()
    {
        try
        {
            using var dlg = new SaveFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx",
                Title = "Ruaj formatin e deponimit per Excel",
                FileName = $"Template_Deponim_{DateTime.Now:yyyyMMdd_HHmm}.xlsx"
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Deponim");

            string[] headers = {
                "NumerSerial*", "Lloji*", "Marka*", "Modeli*", "Kalibri*",
                "Vendlindja", "NrInventari",
                "Klienti", "Stafi", "DataHyrjes", "Qellimi", "Shenime"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cell(1, i + 1).Value = headers[i];
                ws.Cell(1, i + 1).Style.Font.Bold = true;
                ws.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.FromArgb(0, 70, 130);
                ws.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;
                ws.Cell(1, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            // Add current depot data as example rows
            var depot = _db.MerrGjendjenDeponimit();
            for (int ri = 0; ri < Math.Min(depot.Rows.Count, 5); ri++)
            {
                var dr = depot.Rows[ri];
                ws.Cell(ri + 2, 1).Value = (dr["Seriali"] as string) ?? "";
                ws.Cell(ri + 2, 2).Value = (dr["Lloji"] as string) ?? "";
                ws.Cell(ri + 2, 3).Value = (dr["Marka"] as string) ?? "";
                ws.Cell(ri + 2, 4).Value = (dr["Modeli"] as string) ?? "";
                ws.Cell(ri + 2, 5).Value = (dr["Kalibri"] as string) ?? "";
                ws.Cell(ri + 2, 6).Value = "";
                ws.Cell(ri + 2, 7).Value = (dr["NrInventari"] as string) ?? "";
                ws.Cell(ri + 2, 8).Value = (dr["Klienti"] as string) ?? "";
                ws.Cell(ri + 2, 9).Value = (dr["Stafi"] as string) ?? "";
                ws.Cell(ri + 2, 10).Value = (dr["DataHyrjes"] as string ?? "").Length >= 10
                    ? (dr["DataHyrjes"] as string ?? "").Substring(0, 10) : "";
                ws.Cell(ri + 2, 11).Value = (dr["Qellimi"] as string) ?? "";
                ws.Cell(ri + 2, 12).Value = "";
            }

            ws.Columns().AdjustToContents();

            // Add instructions sheet
            var inst = wb.Worksheets.Add("Udhëzime");
            inst.Cell(1, 1).Value = "UDHEZIME PER MBUSHJEN E TEMPLATE";
            inst.Cell(1, 1).Style.Font.Bold = true;
            inst.Cell(1, 1).Style.Font.FontSize = 14;
            inst.Cell(3, 1).Value = "1. Kolonat me * jane te detyrueshme (NumerSerial, Lloji, Marka, Modeli, Kalibri).";
            inst.Cell(4, 1).Value = "2. NumerSerial duhet te jete unik (nuk guxon te perseritet ne sistem).";
            inst.Cell(5, 1).Value = "3. Klienti - shkruaj emrin dhe mbiemrin e klientit qe deponon armen.";
            inst.Cell(6, 1).Value = "4. Stafi - shkruaj emrin dhe mbiemrin e stafit qe pranon armen.";
            inst.Cell(7, 1).Value = "5. DataHyrjes - format: yyyy-MM-dd (psh. 2026-05-28).";
            inst.Cell(8, 1).Value = "6. Rreshtat shembull jane te dhenat ekzistuese ne depo - zevendesoji ose fshiji.";
            inst.Cell(9, 1).Value = "7. Pas mbushjes, perdor butonin '⬇ Importo Deponim' per te ngarkuar ne sistem.";
            inst.Columns().AdjustToContents();

            wb.SaveAs(dlg.FileName);
            MessageBox.Show($"Template i deponimit u ruajt ne {dlg.FileName}\n\nMbusheni dhe perdorni 'Importo Deponim' per ta ngarkuar.",
                "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Gabim: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ImportoDeponim()
    {
        using var dlg = new OpenFileDialog
        {
            Filter = "Excel files (*.xlsx)|*.xlsx",
            Title = "Zgjidh fajllin Excel te deponimit"
        };
        if (dlg.ShowDialog() != DialogResult.OK) return;

        try
        {
            using var wb = new XLWorkbook(dlg.FileName);
            var ws = wb.Worksheet("Deponim") ?? wb.Worksheet(1);

            var range = ws.RangeUsed();
            if (range == null) throw new Exception("Fajlli eshte bosh.");

            var colMap = new Dictionary<string, int>();
            int maxCol = range.ColumnCount();
            for (int ci = 1; ci <= maxCol; ci++)
            {
                var hdr = ws.Cell(1, ci).GetString().Trim().Replace("*", "").ToLower();
                if (!string.IsNullOrEmpty(hdr))
                    colMap[hdr] = ci;
            }

            var rows = range.Rows().ToList();
            if (rows.Count < 2) throw new Exception("Duhet header + minimum 1 rresht.");

            int imported = 0, skipped = 0;
            var errors = new List<string>();
            var serialet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var personeliDt = _db.MerrPersonelin();

            for (int ri = 2; ri <= rows.Count; ri++)
            {
                string serial = "", lloji = "", marka = "", modeli = "", kalibri = "";

                serial = (MerrCel(ws, ri, colMap, "numerSerial") ?? "").Trim();
                lloji = (MerrCel(ws, ri, colMap, "lloji") ?? "").Trim();
                marka = (MerrCel(ws, ri, colMap, "marka") ?? "").Trim();
                modeli = (MerrCel(ws, ri, colMap, "modeli") ?? "").Trim();
                kalibri = (MerrCel(ws, ri, colMap, "kalibri") ?? "").Trim();

                if (string.IsNullOrWhiteSpace(serial) && string.IsNullOrWhiteSpace(lloji)
                    && string.IsNullOrWhiteSpace(marka)) continue;

                if (string.IsNullOrWhiteSpace(serial)) { errors.Add($"Rreshti {ri}: Seriali bosh."); skipped++; continue; }
                if (string.IsNullOrWhiteSpace(lloji)) { errors.Add($"Rreshti {ri} ({serial}): Lloji bosh."); skipped++; continue; }
                if (string.IsNullOrWhiteSpace(marka)) { errors.Add($"Rreshti {ri} ({serial}): Marka bosh."); skipped++; continue; }
                if (string.IsNullOrWhiteSpace(modeli)) { errors.Add($"Rreshti {ri} ({serial}): Modeli bosh."); skipped++; continue; }
                if (string.IsNullOrWhiteSpace(kalibri)) { errors.Add($"Rreshti {ri} ({serial}): Kalibri bosh."); skipped++; continue; }

                if (!serialet.Add(serial)) { errors.Add($"Rreshti {ri}: Seriali '{serial}' dyfish."); skipped++; continue; }
                if (_db.MerrArmeBySerial(serial) != null) { errors.Add($"Rreshti {ri}: Seriali '{serial}' ekziston."); skipped++; continue; }

                var vendlindja = (MerrCel(ws, ri, colMap, "vendlindja") ?? "").Trim();
                var nrInv = (MerrCel(ws, ri, colMap, "nrInventari") ?? "").Trim();
                var klientiEmri = (MerrCel(ws, ri, colMap, "klienti") ?? "").Trim();
                var stafiEmri = (MerrCel(ws, ri, colMap, "stafi") ?? "").Trim();
                var dataHyrjes = (MerrCel(ws, ri, colMap, "dataHyrjes") ?? "").Trim();
                var qellimi = (MerrCel(ws, ri, colMap, "qellimi") ?? "").Trim();
                var shenime = (MerrCel(ws, ri, colMap, "shenime") ?? "").Trim();

                try
                {
                    var arma = new Arma
                    {
                        NumerSerial = serial, Lloji = lloji, Marka = marka,
                        Modeli = modeli, Kalibri = kalibri,
                        Vendlindja = vendlindja, NrInventari = nrInv,
                        DataRegjistrimit = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        Statusi = "Ne Magazine", Shenime = shenime
                    };
                    if (!_db.ShtoArme(arma))
                    { errors.Add($"Rreshti {ri}: Nuk u ruajt arma '{serial}'."); skipped++; continue; }

                    var insertedArma = _db.MerrArmeBySerial(serial);
                    int armaId = insertedArma?.Id ?? 0;
                    if (armaId == 0)
                    { errors.Add($"Rreshti {ri}: Nuk u gjet ID per armen '{serial}'."); skipped++; continue; }

                    int klientiId = 0;
                    if (!string.IsNullOrEmpty(klientiEmri))
                    {
                        var parts = klientiEmri.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 2)
                        {
                            var kl = _db.MerrKlientByEmriMbiemri(parts[0], string.Join(" ", parts.Skip(1)));
                            if (kl != null && kl.Rows.Count > 0) klientiId = Convert.ToInt32(kl.Rows[0]["Id"]);
                        }
                    }

                    int personeliId = 0;
                    if (!string.IsNullOrEmpty(stafiEmri))
                    {
                        foreach (DataRow pr in personeliDt.Rows)
                        {
                            var fullName = $"{pr["Emri"]} {pr["Mbiemri"]}".ToLower();
                            if (fullName.Contains(stafiEmri.ToLower()))
                            { personeliId = Convert.ToInt32(pr["Id"]); break; }
                        }
                    }

                    string dataOra = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    if (!string.IsNullOrEmpty(dataHyrjes) && DateTime.TryParse(dataHyrjes, out var dh))
                        dataOra = dh.ToString("yyyy-MM-dd HH:mm:ss");

                    var trans = new Models.Transaksioni
                    {
                        ArmaId = armaId,
                        Tipi = "Hyrje",
                        DataOra = dataOra,
                        KlientiId = klientiId,
                        PersoneliId = personeliId,
                        Qellimi = string.IsNullOrEmpty(qellimi) ? "Deponim ne sistem" : qellimi,
                        Shenime = shenime,
                        PersoneliQeDorzoi = stafiEmri,
                        PersoneliQeMorri = ""
                    };
                    _db.RegjistroTransaksion(trans);

                    imported++;
                }
                catch (Exception ex)
                { errors.Add($"Rreshti {ri} ({serial}): {ex.Message}"); skipped++; }
            }

            string msg = $"U importuan {imported} arme ne deponim.";
            if (skipped > 0) msg += $"\nU kapërcyen: {skipped}";
            if (errors.Count > 0)
            {
                msg += "\n\nGabime:\n" + string.Join("\n", errors.Take(15));
                if (errors.Count > 15) msg += $"\n...dhe {errors.Count - 15} te tjera";
                MessageBox.Show(msg, "Rezultati", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
                MessageBox.Show(msg, "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Gabim: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static string? MerrCel(IXLWorksheet ws, int row, Dictionary<string, int> map, string key)
    {
        if (map.TryGetValue(key, out int ci))
            return ws.Cell(row, ci).GetString();
        return null;
    }

    private void RuajTeGjitha()
    {
        if (_dt == null || _dt.Rows.Count == 0)
        { MessageBox.Show("Nuk ka rreshta per te ruajtur.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

        var errors = new List<string>();
        var serialet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < _dt.Rows.Count; i++)
        {
            var row = _dt.Rows[i];
            string serial = "", lloji = "", marka = "", modeli = "", kalibri = "";

            try
            {
                serial = (row["NumerSerial"] as string ?? "").Trim();
                lloji = (row["Lloji"] as string ?? "").Trim();
                marka = (row["Marka"] as string ?? "").Trim();
                modeli = (row["Modeli"] as string ?? "").Trim();
                kalibri = (row["Kalibri"] as string ?? "").Trim();
            }
            catch { errors.Add($"Rreshti {i + 1}: Problem me leximin e rreshtit."); continue; }

            if (string.IsNullOrWhiteSpace(serial)) { errors.Add($"Rreshti {i + 1}: Seriali bosh."); continue; }
            if (string.IsNullOrWhiteSpace(lloji)) { errors.Add($"Rreshti {i + 1}: Lloji bosh."); continue; }
            if (string.IsNullOrWhiteSpace(marka)) { errors.Add($"Rreshti {i + 1}: Marka bosh."); continue; }
            if (string.IsNullOrWhiteSpace(modeli)) { errors.Add($"Rreshti {i + 1}: Modeli bosh."); continue; }
            if (string.IsNullOrWhiteSpace(kalibri)) { errors.Add($"Rreshti {i + 1}: Kalibri bosh."); continue; }

            if (!serialet.Add(serial)) { errors.Add($"Rreshti {i + 1}: Seriali '{serial}' dyfish ne liste."); continue; }

            try
            {
                var existing = _db.MerrArmeBySerial(serial);
                if (existing != null) { errors.Add($"Rreshti {i + 1}: Seriali '{serial}' ekziston."); continue; }
            }
            catch { errors.Add($"Rreshti {i + 1}: Gabim DB per serialin '{serial}'."); continue; }
        }

        if (errors.Count > 0)
        {
            var msg = $"Gjithsej {errors.Count} gabime:\n" + string.Join("\n", errors.Take(15));
            if (errors.Count > 15) msg += $"\n...dhe {errors.Count - 15} te tjera";
            MessageBox.Show(msg, "Gabime Validimi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            lblStatus.Text = $"{errors.Count} gabime. Rregullo dhe provo perseri.";
            return;
        }

        int saved = 0;
        var saveErrors = new List<string>();
        for (int i = 0; i < _dt.Rows.Count; i++)
        {
            var row = _dt.Rows[i];
            try
            {
                var arma = new Arma
                {
                    NumerSerial = (row["NumerSerial"] as string ?? "").Trim(),
                    Lloji = (row["Lloji"] as string ?? "").Trim(),
                    Marka = (row["Marka"] as string ?? "").Trim(),
                    Modeli = (row["Modeli"] as string ?? "").Trim(),
                    Kalibri = (row["Kalibri"] as string ?? "").Trim(),
                    Vendlindja = (row["Vendlindja"] as string ?? "").Trim(),
                    NrInventari = (row["NrInventari"] as string ?? "").Trim(),
                    Statusi = (row["Statusi"] as string ?? "Ne Magazine").Trim(),
                    Shenime = (row["Shenime"] as string ?? "").Trim()
                };
                if (_db.ShtoArme(arma))
                    saved++;
                else
                    saveErrors.Add($"Rreshti {i + 1}: Seriali '{arma.NumerSerial}' nuk u ruajt.");
            }
            catch (Exception ex) { saveErrors.Add($"Rreshti {i + 1}: {ex.Message}"); }
        }

        string msg2 = $"U ruajten {saved} nga {_dt.Rows.Count} arme.";
        if (saveErrors.Count > 0)
        {
            msg2 += $"\n{string.Join("\n", saveErrors.Take(10))}";
            if (saveErrors.Count > 10) msg2 += $"\n...dhe {saveErrors.Count - 10} te tjera";
            MessageBox.Show(msg2, "Rezultati", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        else
        {
            MessageBox.Show(msg2, "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
            dgv.DataSource = null;
            _dt = KrijoDataTabelen();
            dgv.DataSource = _dt;
            VendosTitujt();
        }
        lblStatus.Text = msg2;
    }
}
