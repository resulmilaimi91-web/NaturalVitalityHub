using System.Data;
using System.IO;
using ArmepunesApp.Data;
using ArmepunesApp.Models;
using ArmepunesApp.Services;

namespace ArmepunesApp.Forms;

public partial class RegjistroWizardForm : Form
{
    private readonly DatabaseHelper _db;
    private readonly string _tipi;
    private int _currentStep = 1;
    private const int TotalSteps = 5;

    // Data collected across steps
    private int _selectedPersoneliId;
    private string _selectedPersoneliText = "";
    private int _selectedKlientiId;
    private string _selectedKlientiText = "";
    private readonly List<ArmaSelection> _selectedArmet = new();
    private string _qellimi = "";
    private string _dorzoi = "";
    private string _morri = "";
    private string _shenime = "";
    private string _dataOraRegjistrimit = "";
    private string _numriRadhordhes = "";
    private string _pdfPath = "";

    private class ArmaSelection
    {
        public int ArmaId { get; set; }
        public string ArmaText { get; set; } = "";
        public List<Aksesori> Aksesoret { get; set; } = new();
    }

    // Step controls
    private ListBox listPersoneli = null!;
    private ListBox listKlientet = null!;
    private ListBox listArmet = null!;
    private TextBox txtQellimiStep = null!;
    private TextBox txtDorzoiStep = null!;
    private TextBox txtMorriStep = null!;
    private TextBox txtShenimeStep = null!;
    private TextBox txtMunicionStep = null!;
    private DataGridView dgvAksesoret = null!;
    private Label lblStepTitle = null!;
    private Label lblStepDescription = null!;
    private Button btnBack = null!;
    private Button btnNext = null!;
    private Button btnCancel = null!;
    private Panel panelSteps = null!;
    private Panel panelContent = null!;
    private readonly List<Label> _stepLabels = new();
    private readonly string _perdoruesi = "";
    private readonly List<int> _selectedTransaksionIds = new();

    public RegjistroWizardForm(DatabaseHelper db, string tipi, string perdoruesi = "")
    {
        _db = db;
        _tipi = tipi;
        _perdoruesi = perdoruesi;
        InitializeComponent();
        BuildStepIndicators();
        ShowStep(1);
    }

    private void BuildStepIndicators()
    {
        var steps = new[] { "Punonjesi", "Klienti", "Arma", "Konfirmo", "Printo" };
        int stepW = 800 / steps.Length;
        for (int i = 0; i < steps.Length; i++)
        {
            var container = new Panel();
            container.Size = new Size(stepW, 52);
            container.Location = new Point(i * stepW, 0);
            container.BackColor = Color.FromArgb(25, 28, 33);

            var lbl = new Label();
            lbl.Text = $"{i + 1}. {steps[i]}";
            lbl.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            lbl.Size = new Size(stepW - 10, 52);
            lbl.Location = new Point(5, 0);
            lbl.TextAlign = ContentAlignment.MiddleCenter;
            lbl.ForeColor = Color.FromArgb(100, 105, 115);
            lbl.BackColor = Color.Transparent;

            var bar = new Panel();
            bar.Size = new Size(stepW - 10, 4);
            bar.Location = new Point(5, 46);
            bar.BackColor = Color.FromArgb(40, 42, 48);
            bar.Tag = i;

            container.Controls.Add(lbl);
            container.Controls.Add(bar);
            panelSteps.Controls.Add(container);
            _stepLabels.Add(lbl);
        }
    }

    private void ShowStep(int step)
    {
        _currentStep = step;
        UpdateStepIndicators();
        panelContent.Controls.Clear();

        switch (step)
        {
            case 1: BuildStepPunonjesi(); break;
            case 2: BuildStepKlienti(); break;
            case 3: BuildStepArma(); break;
            case 4: BuildStepKonfirmo(); break;
            case 5: BuildStepPrinto(); break;
        }

        btnBack.Visible = step > 1 && step < 5;
        btnNext.Text = step < 3 ? "Para ►" : step == 3 ? "Vazhdo ►" : step == 4 ? "✓ Ruaj dhe vazhdo" : "► Printo";
        btnNext.Visible = step < 4 || step == 4 || step == 5;
        btnCancel.Text = step == 5 ? "Mbyll" : "Anulo";

        // Ensure scrollbar appears if content overflows
        int maxBottom = 0;
        foreach (Control c in panelContent.Controls)
            maxBottom = Math.Max(maxBottom, c.Bounds.Bottom + 10);
        panelContent.AutoScrollMinSize = new Size(panelContent.ClientSize.Width - 5, Math.Max(maxBottom, panelContent.ClientSize.Height + 1));
    }

    private void UpdateStepIndicators()
    {
        for (int i = 0; i < _stepLabels.Count; i++)
        {
            int stepNum = i + 1;
            bool isCurrent = stepNum == _currentStep;
            bool isDone = stepNum < _currentStep;

            _stepLabels[i].ForeColor = isCurrent
                ? Color.FromArgb(0, 200, 255)
                : isDone ? Color.FromArgb(100, 200, 100) : Color.FromArgb(100, 105, 115);

            _stepLabels[i].Font = new Font("Segoe UI", 10, isCurrent ? FontStyle.Bold : FontStyle.Regular);

            // bar is the second control in the container (index 1)
            var container = (Panel)panelSteps.Controls[i];
            var bar = (Panel)container.Controls[1];
            bar.BackColor = isCurrent
                ? Color.FromArgb(0, 200, 255)
                : isDone ? Color.FromArgb(100, 200, 100) : Color.FromArgb(40, 42, 48);
        }
    }

    private void BuildStepPunonjesi()
    {
        lblStepTitle.Text = "Zgjedh Punonjesin";
        lblStepDescription.Text = "Zgjedh punonjesin/zyrtarin qe po kryen kete transaksion";

        var lbl = new Label();
        lbl.Text = "Punonjesit e disponueshem:";
        lbl.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        lbl.ForeColor = Color.FromArgb(200, 205, 216);
        lbl.Size = new Size(400, 20);
        lbl.Location = new Point(20, 10);

        listPersoneli = new ListBox();
        listPersoneli.Size = new Size(400, 220);
        listPersoneli.Location = new Point(20, 35);
        listPersoneli.BackColor = Color.FromArgb(40, 42, 48);
        listPersoneli.ForeColor = Color.FromArgb(200, 205, 216);
        listPersoneli.Font = new Font("Segoe UI", 10);
        listPersoneli.BorderStyle = BorderStyle.FixedSingle;
        listPersoneli.SelectedIndexChanged += (s, e) => btnNext.Enabled = listPersoneli.SelectedItem != null;

        var dt = _db.MerrPersonelin();
        foreach (DataRow row in dt.Rows)
        {
            listPersoneli.Items.Add(new ComboboxItem
            {
                Text = $"{row["Emri"]} {row["Mbiemri"]} - {row["Grada"]}",
                Value = Convert.ToInt32(row["Id"])
            });
        }
        if (listPersoneli.Items.Count > 0) listPersoneli.SelectedIndex = 0;

        btnNext.Enabled = listPersoneli.Items.Count > 0;

        panelContent.Controls.Add(lbl);
        panelContent.Controls.Add(listPersoneli);

        if (listPersoneli.Items.Count == 0)
        {
            var warn = new Label();
            warn.Text = "⚠ Nuk ka punonjes te regjistruar! Shtoni nje me poshte.";
            warn.Font = new Font("Segoe UI", 9);
            warn.ForeColor = Color.FromArgb(255, 150, 50);
            warn.Size = new Size(400, 30);
            warn.Location = new Point(20, 265);
            panelContent.Controls.Add(warn);
        }

        var btnShtoPunonjes = new Button();
        btnShtoPunonjes.Text = "+ Shto Punonjes te ri";
        btnShtoPunonjes.Size = new Size(200, 36);
        btnShtoPunonjes.Location = new Point(20, 290);
        btnShtoPunonjes.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        btnShtoPunonjes.BackColor = Color.FromArgb(39, 174, 96);
        btnShtoPunonjes.ForeColor = Color.White;
        btnShtoPunonjes.FlatStyle = FlatStyle.Flat;
        btnShtoPunonjes.Cursor = Cursors.Hand;
        btnShtoPunonjes.Click += (s, e) =>
        {
            using var f = new ShtoPersonelForm(_db, null);
            if (f.ShowDialog(this) == DialogResult.OK)
            {
                // Reload personeli list
                listPersoneli.Items.Clear();
                var dt = _db.MerrPersonelin();
                foreach (DataRow row in dt.Rows)
                {
                    listPersoneli.Items.Add(new ComboboxItem
                    {
                        Text = $"{row["Emri"]} {row["Mbiemri"]} - {row["Grada"]}",
                        Value = Convert.ToInt32(row["Id"])
                    });
                }
                if (listPersoneli.Items.Count > 0)
                    listPersoneli.SelectedIndex = listPersoneli.Items.Count - 1;
                btnNext.Enabled = listPersoneli.Items.Count > 0;
            }
        };

        panelContent.Controls.Add(btnShtoPunonjes);
    }

    private void BuildStepKlienti()
    {
        lblStepTitle.Text = "Zgjedh Klientin";
        lblStepDescription.Text = "Zgjedh klientin ose shto nje te ri";

        var lbl = new Label();
        lbl.Text = "Klientet e disponueshem:";
        lbl.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        lbl.ForeColor = Color.FromArgb(200, 205, 216);
        lbl.Size = new Size(400, 20);
        lbl.Location = new Point(20, 10);

        listKlientet = new ListBox();
        listKlientet.Size = new Size(400, 195);
        listKlientet.Location = new Point(20, 35);
        listKlientet.BackColor = Color.FromArgb(40, 42, 48);
        listKlientet.ForeColor = Color.FromArgb(200, 205, 216);
        listKlientet.Font = new Font("Segoe UI", 10);
        listKlientet.BorderStyle = BorderStyle.FixedSingle;
        listKlientet.SelectedIndexChanged += (s, e) => btnNext.Enabled = listKlientet.SelectedItem != null;

        NgarkoListKlientet();

        btnNext.Enabled = listKlientet.Items.Count > 0;

        var btnShtoKlient = new Button();
        btnShtoKlient.Text = "+ Shto Klient te ri";
        btnShtoKlient.Size = new Size(200, 36);
        btnShtoKlient.Location = new Point(20, 240);
        btnShtoKlient.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        btnShtoKlient.BackColor = Color.FromArgb(39, 174, 96);
        btnShtoKlient.ForeColor = Color.White;
        btnShtoKlient.FlatStyle = FlatStyle.Flat;
        btnShtoKlient.Cursor = Cursors.Hand;
        btnShtoKlient.Click += (s, e) =>
        {
            using var f = new ShtoKlientForm(_db, null);
            if (f.ShowDialog(this) == DialogResult.OK)
            {
                NgarkoListKlientet();
                if (f.KlientiId > 0)
                {
                    for (int i = 0; i < listKlientet.Items.Count; i++)
                    {
                        if (((ComboboxItem)listKlientet.Items[i]).Value == f.KlientiId)
                        { listKlientet.SelectedIndex = i; break; }
                    }
                }
                else if (listKlientet.Items.Count > 0)
                    listKlientet.SelectedIndex = listKlientet.Items.Count - 1;
                btnNext.Enabled = true;
            }
        };

        panelContent.Controls.Add(lbl);
        panelContent.Controls.Add(listKlientet);
        panelContent.Controls.Add(btnShtoKlient);
    }

    private void NgarkoListKlientet()
    {
        listKlientet.Items.Clear();
        var dt = _db.MerrKlientet();
        foreach (DataRow row in dt.Rows)
        {
            listKlientet.Items.Add(new ComboboxItem
            {
                Text = $"{row["Emri"]} {row["Mbiemri"]} - {row["NrLeternjoftimit"]}",
                Value = Convert.ToInt32(row["Id"])
            });
        }
    }

    private ListBox listArmetSelektuara = null!;

    private void BuildStepArma()
    {
        lblStepTitle.Text = "Zgjedh Armët";
        lblStepDescription.Text = _tipi == "Dalje"
            ? "Selekto armët që po tërhiqen nga depo"
            : "Shto armët që po deponohen (mund të shtosh 2-3 armë)";

        int y = 8;

        // ── Panel: Weapons available for selection ──
        var lblArmetNeLire = new Label();
        lblArmetNeLire.Text = _tipi == "Dalje" ? "Armët në magazinë (të klientit):" : "Armët në dispozicion:";
        lblArmetNeLire.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        lblArmetNeLire.ForeColor = Color.FromArgb(200, 205, 216);
        lblArmetNeLire.Size = new Size(400, 20);
        lblArmetNeLire.Location = new Point(20, y);

        listArmet = new ListBox();
        listArmet.Size = new Size(300, 160);
        listArmet.Location = new Point(20, y + 22);
        listArmet.BackColor = Color.FromArgb(40, 42, 48);
        listArmet.ForeColor = Color.FromArgb(200, 205, 216);
        listArmet.Font = new Font("Segoe UI", 10);
        listArmet.BorderStyle = BorderStyle.FixedSingle;
        NgarkoListArmetDisponueshme();

        var btnShtoNeListe = new Button();
        btnShtoNeListe.Text = ">> Shto";
        btnShtoNeListe.Size = new Size(70, 28);
        btnShtoNeListe.Location = new Point(330, y + 50);
        btnShtoNeListe.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        btnShtoNeListe.BackColor = Color.FromArgb(39, 174, 96);
        btnShtoNeListe.ForeColor = Color.White;
        btnShtoNeListe.FlatStyle = FlatStyle.Flat;
        btnShtoNeListe.Cursor = Cursors.Hand;
        btnShtoNeListe.Click += (s, e) =>
        {
            if (listArmet.SelectedItem == null) return;
            var item = (ComboboxItem)listArmet.SelectedItem;
            if (_selectedArmet.Any(a => a.ArmaId == item.Value))
            {
                MessageBox.Show("Kjo armë është shtuar tashmë!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            _selectedArmet.Add(new ArmaSelection { ArmaId = item.Value, ArmaText = item.Text });
            if (_tipi == "Dalje")
            {
                var aksDt = _db.MerrAksesoretFunditHyrjePerArmen(item.Value);
                foreach (System.Data.DataRow ar in aksDt.Rows)
                    _selectedArmet.Last().Aksesoret.Add(new Aksesori
                    {
                        Emri = ar["Emri"]?.ToString() ?? "",
                        Sasia = Convert.ToInt32(ar["Sasia"] ?? 1),
                        Shenime = ar["Shenime"]?.ToString() ?? ""
                    });
            }
            RefreshArmaSelektuara();
            btnNext.Enabled = _selectedArmet.Count > 0;
        };

        // ── Panel: Selected weapons ──
        var lblSelektuara = new Label();
        lblSelektuara.Text = "Armët e selektuara:";
        lblSelektuara.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        lblSelektuara.ForeColor = Color.FromArgb(0, 200, 255);
        lblSelektuara.Size = new Size(350, 20);
        lblSelektuara.Location = new Point(410, y);

        listArmetSelektuara = new ListBox();
        listArmetSelektuara.Size = new Size(350, 160);
        listArmetSelektuara.Location = new Point(410, y + 22);
        listArmetSelektuara.BackColor = Color.FromArgb(35, 37, 43);
        listArmetSelektuara.ForeColor = Color.FromArgb(200, 205, 216);
        listArmetSelektuara.Font = new Font("Segoe UI", 10);
        listArmetSelektuara.BorderStyle = BorderStyle.FixedSingle;
        listArmetSelektuara.DisplayMember = "Text";
        RefreshArmaSelektuara();

        // ── Buttons row ──
        y += 190;

        var btnHiq = new Button();
        btnHiq.Text = "✕ Hiq";
        btnHiq.Size = new Size(80, 30);
        btnHiq.Location = new Point(410, y);
        btnHiq.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        btnHiq.BackColor = Color.FromArgb(192, 57, 43);
        btnHiq.ForeColor = Color.White;
        btnHiq.FlatStyle = FlatStyle.Flat;
        btnHiq.Cursor = Cursors.Hand;
        btnHiq.Click += (s, e) =>
        {
            if (listArmetSelektuara.SelectedIndex < 0) return;
            _selectedArmet.RemoveAt(listArmetSelektuara.SelectedIndex);
            RefreshArmaSelektuara();
            btnNext.Enabled = _selectedArmet.Count > 0;
        };

        var btnShtoArmeRe = new Button();
        btnShtoArmeRe.Text = "+ Shto Armë të re";
        btnShtoArmeRe.Size = new Size(160, 30);
        btnShtoArmeRe.Location = new Point(500, y);
        btnShtoArmeRe.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        btnShtoArmeRe.BackColor = Color.FromArgb(39, 174, 96);
        btnShtoArmeRe.ForeColor = Color.White;
        btnShtoArmeRe.FlatStyle = FlatStyle.Flat;
        btnShtoArmeRe.Cursor = Cursors.Hand;
        btnShtoArmeRe.Click += (s, e) =>
        {
            using var f = new ShtoArmeForm(_db, null);
            if (f.ShowDialog(this) == DialogResult.OK)
                NgarkoListArmetDisponueshme();
        };

        btnNext.Enabled = _selectedArmet.Count > 0;
        btnNext.Text = "Vazhdo ►";

        panelContent.Controls.AddRange(new Control[] {
            lblArmetNeLire, listArmet, btnShtoNeListe,
            lblSelektuara, listArmetSelektuara, btnHiq, btnShtoArmeRe
        });
    }

    private void RefreshArmaSelektuara()
    {
        listArmetSelektuara.Items.Clear();
        foreach (var a in _selectedArmet)
        {
            int aksCount = a.Aksesoret.Count;
            string aksInfo = aksCount > 0 ? $" [aksesorë: {aksCount}]" : "";
            listArmetSelektuara.Items.Add(new ComboboxItem
            {
                Text = $"{a.ArmaText}{aksInfo}",
                Value = a.ArmaId
            });
        }
    }

    private void NgarkoListArmetDisponueshme()
    {
        listArmet.Items.Clear();
        var armet = _db.MerrArmet();

        if (_tipi == "Dalje")
        {
            foreach (DataRow row in armet.Rows)
            {
                if (row["Statusi"]?.ToString() != "Ne Magazine") continue;
                int armaId = Convert.ToInt32(row["Id"]);
                int klientiIdFundit = _db.MerrKlientiIdFunditHyrjePerArmen(armaId);
                if (klientiIdFundit == _selectedKlientiId || _selectedKlientiId == 0)
                {
                    listArmet.Items.Add(new ComboboxItem
                    {
                        Text = $"{row["NumerSerial"]} - {row["Marka"]} {row["Modeli"]} ({row["Lloji"]})",
                        Value = armaId
                    });
                }
            }
        }
        else
        {
            foreach (DataRow row in armet.Rows)
            {
                if (row == null) continue;
                listArmet.Items.Add(new ComboboxItem
                {
                    Text = $"{row["NumerSerial"]} - {row["Marka"]} {row["Modeli"]} ({row["Lloji"]})",
                    Value = Convert.ToInt32(row["Id"])
                });
            }
        }
    }

    private DateTimePicker dtpDataPranimit = null!;
    private int _currentArmaIndex;

    private void BuildStepKonfirmo()
    {
        lblStepTitle.Text = "Konfirmo dhe Aksesoret";
        lblStepDescription.Text = "Shqyrto të dhënat dhe shto aksesorët për secilën armë";

        int y = 8;

        // ── Row 1: Data + Nr regjistri ──
        var lblData = new Label();
        lblData.Text = "Data e Pranimit:";
        lblData.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        lblData.ForeColor = Color.FromArgb(0, 200, 255);
        lblData.Size = new Size(110, 22);
        lblData.Location = new Point(20, y);

        dtpDataPranimit = new DateTimePicker();
        dtpDataPranimit.Format = DateTimePickerFormat.Custom;
        dtpDataPranimit.CustomFormat = "dd.MM.yyyy  HH:mm";
        dtpDataPranimit.Value = DateTime.Now;
        dtpDataPranimit.Size = new Size(180, 24);
        dtpDataPranimit.Location = new Point(130, y);
        dtpDataPranimit.BackColor = Color.FromArgb(40, 42, 48);
        dtpDataPranimit.ForeColor = Color.FromArgb(200, 205, 216);
        dtpDataPranimit.Font = new Font("Segoe UI", 10, FontStyle.Bold);

        _dataOraRegjistrimit = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        var allTrans = _db.MerrTransaksionet();
        _numriRadhordhes = $"Regj. Nr. {(allTrans.Rows.Count + 1):D6}";
        var lblNr = new Label();
        lblNr.Text = _numriRadhordhes;
        lblNr.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        lblNr.ForeColor = Color.FromArgb(100, 200, 100);
        lblNr.Size = new Size(200, 22);
        lblNr.Location = new Point(330, y);
        lblNr.TextAlign = ContentAlignment.MiddleLeft;

        panelContent.Controls.Add(lblData);
        panelContent.Controls.Add(dtpDataPranimit);
        panelContent.Controls.Add(lblNr);

        // ── Row 2: Summary compact ──
        y += 28;
        var summaryBack = new Panel();
        summaryBack.Size = new Size(700, 60);
        summaryBack.Location = new Point(15, y);
        summaryBack.BackColor = Color.FromArgb(35, 37, 43);
        summaryBack.BorderStyle = BorderStyle.FixedSingle;

        string armetSummary = string.Join("\n", _selectedArmet.Select((a, i) => $"  {i + 1}. {a.ArmaText}"));
        var summaryText = $"{_selectedPersoneliText}  |  {_selectedKlientiText}  |  {(_tipi == "Hyrje" ? "Deponim" : "Terheqje")}\n{armetSummary}";
        var liSum = new Label();
        liSum.Text = summaryText;
        liSum.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        liSum.ForeColor = Color.FromArgb(200, 205, 216);
        liSum.Size = new Size(680, 50);
        liSum.Location = new Point(10, 5);
        liSum.TextAlign = ContentAlignment.MiddleLeft;
        summaryBack.Controls.Add(liSum);
        panelContent.Controls.Add(summaryBack);

        // ── Row 3: Qellimi + Dorzoi ──
        y += 65;
        var lblQellimi = new Label();
        lblQellimi.Text = "Qellimi:";
        lblQellimi.Font = new Font("Segoe UI", 9);
        lblQellimi.ForeColor = Color.FromArgb(200, 205, 216);
        lblQellimi.Size = new Size(60, 22);
        lblQellimi.Location = new Point(20, y);

        txtQellimiStep = new TextBox();
        txtQellimiStep.Size = new Size(250, 24);
        txtQellimiStep.Location = new Point(85, y);
        txtQellimiStep.BackColor = Color.FromArgb(40, 42, 48);
        txtQellimiStep.ForeColor = Color.FromArgb(200, 205, 216);
        txtQellimiStep.BorderStyle = BorderStyle.FixedSingle;
        txtQellimiStep.Font = new Font("Segoe UI", 9);
        txtQellimiStep.MaxLength = 200;

        var lblDorzoi = new Label();
        lblDorzoi.Text = "Dorzoi:";
        lblDorzoi.Font = new Font("Segoe UI", 9);
        lblDorzoi.ForeColor = Color.FromArgb(200, 205, 216);
        lblDorzoi.Size = new Size(50, 22);
        lblDorzoi.Location = new Point(370, y);

        txtDorzoiStep = new TextBox();
        txtDorzoiStep.Size = new Size(200, 24);
        txtDorzoiStep.Location = new Point(420, y);
        txtDorzoiStep.BackColor = Color.FromArgb(40, 42, 48);
        txtDorzoiStep.ForeColor = Color.FromArgb(200, 205, 216);
        txtDorzoiStep.BorderStyle = BorderStyle.FixedSingle;
        txtDorzoiStep.Font = new Font("Segoe UI", 9);
        txtDorzoiStep.MaxLength = 200;

        panelContent.Controls.Add(lblQellimi);
        panelContent.Controls.Add(txtQellimiStep);
        panelContent.Controls.Add(lblDorzoi);
        panelContent.Controls.Add(txtDorzoiStep);

        // ── Row 4: Morri + Shenime ──
        y += 28;
        var lblMorri = new Label();
        lblMorri.Text = "Morri:";
        lblMorri.Font = new Font("Segoe UI", 9);
        lblMorri.ForeColor = Color.FromArgb(200, 205, 216);
        lblMorri.Size = new Size(50, 22);
        lblMorri.Location = new Point(20, y);

        txtMorriStep = new TextBox();
        txtMorriStep.Size = new Size(200, 24);
        txtMorriStep.Location = new Point(85, y);
        txtMorriStep.BackColor = Color.FromArgb(40, 42, 48);
        txtMorriStep.ForeColor = Color.FromArgb(200, 205, 216);
        txtMorriStep.BorderStyle = BorderStyle.FixedSingle;
        txtMorriStep.Font = new Font("Segoe UI", 9);
        txtMorriStep.MaxLength = 200;

        var lblShenime = new Label();
        lblShenime.Text = "Shenime:";
        lblShenime.Font = new Font("Segoe UI", 9);
        lblShenime.ForeColor = Color.FromArgb(200, 205, 216);
        lblShenime.Size = new Size(60, 22);
        lblShenime.Location = new Point(300, y);

        txtShenimeStep = new TextBox();
        txtShenimeStep.Size = new Size(380, 24);
        txtShenimeStep.Location = new Point(360, y);
        txtShenimeStep.BackColor = Color.FromArgb(40, 42, 48);
        txtShenimeStep.ForeColor = Color.FromArgb(200, 205, 216);
        txtShenimeStep.BorderStyle = BorderStyle.FixedSingle;
        txtShenimeStep.Font = new Font("Segoe UI", 9);
        txtShenimeStep.MaxLength = 500;

        panelContent.Controls.Add(lblMorri);
        panelContent.Controls.Add(txtMorriStep);
        panelContent.Controls.Add(lblShenime);
        panelContent.Controls.Add(txtShenimeStep);

        // ── Row 4b: Municioni ──
        y += 30;
        var lblMunicion = new Label();
        lblMunicion.Text = "Municioni:";
        lblMunicion.Font = new Font("Segoe UI", 9);
        lblMunicion.ForeColor = Color.FromArgb(200, 205, 216);
        lblMunicion.Size = new Size(70, 22);
        lblMunicion.Location = new Point(20, y);

        txtMunicionStep = new TextBox();
        txtMunicionStep.Size = new Size(400, 24);
        txtMunicionStep.Location = new Point(95, y);
        txtMunicionStep.BackColor = Color.FromArgb(40, 42, 48);
        txtMunicionStep.ForeColor = Color.FromArgb(200, 205, 216);
        txtMunicionStep.BorderStyle = BorderStyle.FixedSingle;
        txtMunicionStep.Font = new Font("Segoe UI", 9);
        txtMunicionStep.MaxLength = 500;
        txtMunicionStep.PlaceholderText = "p.sh.: 9mm Parabellum, 50 copë";

        panelContent.Controls.Add(lblMunicion);
        panelContent.Controls.Add(txtMunicionStep);

        // ── Auto-populate fields from selections ──
        string personeliEmri = _selectedPersoneliText.Contains(" - ")
            ? _selectedPersoneliText[.._selectedPersoneliText.IndexOf(" - ")]
            : _selectedPersoneliText;
        string klientiEmri = _selectedKlientiText.Contains(" - ")
            ? _selectedKlientiText[.._selectedKlientiText.IndexOf(" - ")]
            : _selectedKlientiText;
        txtDorzoiStep.Text = _tipi == "Hyrje" ? klientiEmri : personeliEmri;
        txtMorriStep.Text = _tipi == "Hyrje" ? personeliEmri : klientiEmri;
        txtQellimiStep.Text = _tipi == "Hyrje" ? "Deponim" : "Terheqje";

        // ── Row 5: Weapon selector + Accessories ──
        y += 30;
        _currentArmaIndex = 0;

        var aksPanel = new Panel();
        aksPanel.Size = new Size(700, 210);
        aksPanel.Location = new Point(15, y);
        aksPanel.BackColor = Color.FromArgb(30, 33, 40);
        aksPanel.BorderStyle = BorderStyle.FixedSingle;

        // Weapon tabs (ComboBox to switch between weapons)
        var lblArmaSelect = new Label();
        lblArmaSelect.Text = "Aktual:";
        lblArmaSelect.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        lblArmaSelect.ForeColor = Color.FromArgb(0, 200, 255);
        lblArmaSelect.Size = new Size(50, 22);
        lblArmaSelect.Location = new Point(5, 4);

        var cmbArma = new ComboBox();
        cmbArma.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbArma.BackColor = Color.FromArgb(40, 42, 48);
        cmbArma.ForeColor = Color.FromArgb(200, 205, 216);
        cmbArma.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        cmbArma.Size = new Size(620, 22);
        cmbArma.Location = new Point(55, 3);
        for (int i = 0; i < _selectedArmet.Count; i++)
            cmbArma.Items.Add($"{i + 1}. {_selectedArmet[i].ArmaText}");
        if (cmbArma.Items.Count > 0) cmbArma.SelectedIndex = 0;
        cmbArma.SelectedIndexChanged += (s, _) =>
        {
            // Save current accessories
            RuajAksesoretAktual();
            // Switch to new weapon
            _currentArmaIndex = cmbArma.SelectedIndex;
            NgarkoAksesoretPerArmen(_currentArmaIndex);
        };
        aksPanel.Controls.Add(lblArmaSelect);
        aksPanel.Controls.Add(cmbArma);

        // Quick-add preset buttons (compact row)
        var akset = new[] { "Magazine", "Holster", "Lugë pastrimi", "Kuti origjinale",
            "Certifikate", "Manual", "Celës sigurie", "Optikë", "Fener", "Rreshter",
            "Rrip", "Kapak", "Çantë", "Komplet pastrimi" };
        var flpPresets = new FlowLayoutPanel();
        flpPresets.Size = new Size(688, 26);
        flpPresets.Location = new Point(5, 28);
        flpPresets.BackColor = Color.FromArgb(35, 37, 43);
        flpPresets.Padding = new Padding(2, 0, 0, 0);
        flpPresets.AutoScroll = true;
        flpPresets.BorderStyle = BorderStyle.None;
        foreach (var a in akset)
        {
            var btn = new Button();
            btn.Text = a;
            btn.Font = new Font("Segoe UI", 7);
            btn.ForeColor = Color.FromArgb(200, 205, 216);
            btn.BackColor = Color.FromArgb(50, 52, 58);
            btn.FlatStyle = FlatStyle.Flat;
            btn.Size = new Size(95, 22);
            btn.Margin = new Padding(2);
            btn.Cursor = Cursors.Hand;
            btn.Click += (_, _) => ShtoAksesorNeGrid(a, 1, "");
            flpPresets.Controls.Add(btn);
        }
        aksPanel.Controls.Add(flpPresets);

        // DataGridView
        dgvAksesoret = new DataGridView();
        dgvAksesoret.Size = new Size(688, 115);
        dgvAksesoret.Location = new Point(5, 56);
        dgvAksesoret.BackgroundColor = Color.FromArgb(35, 37, 43);
        dgvAksesoret.ForeColor = Color.FromArgb(200, 205, 216);
        dgvAksesoret.BorderStyle = BorderStyle.None;
        dgvAksesoret.Font = new Font("Segoe UI", 9);
        dgvAksesoret.AutoGenerateColumns = false;
        dgvAksesoret.AllowUserToAddRows = false;
        dgvAksesoret.RowHeadersVisible = false;
        dgvAksesoret.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvAksesoret.MultiSelect = false;
        dgvAksesoret.ColumnHeadersHeight = 24;
        dgvAksesoret.Columns.Add(new DataGridViewTextBoxColumn { Name = "Emri", HeaderText = "Emri", Width = 340 });
        dgvAksesoret.Columns.Add(new DataGridViewTextBoxColumn { Name = "Sasia", HeaderText = "Sasia", Width = 80 });
        dgvAksesoret.Columns.Add(new DataGridViewTextBoxColumn { Name = "Shenime", HeaderText = "Shenime", Width = 250 });
        dgvAksesoret.CellPainting += (sender, e) =>
        {
            if (e.RowIndex < 0 || e.CellStyle == null) return;
            e.CellStyle.BackColor = e.RowIndex % 2 == 0 ? Color.FromArgb(35, 37, 43) : Color.FromArgb(42, 44, 50);
            e.CellStyle.SelectionBackColor = Color.FromArgb(0, 120, 215);
            e.CellStyle.SelectionForeColor = Color.White;
        };
        aksPanel.Controls.Add(dgvAksesoret);

        // Load accessories for first weapon
        NgarkoAksesoretPerArmen(0);

        // Kyq / Hiq buttons
        var btnKyq = new Button();
        btnKyq.Text = "➕ KYQ";
        btnKyq.Size = new Size(90, 26);
        btnKyq.Location = new Point(5, 176);
        btnKyq.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        btnKyq.BackColor = Color.FromArgb(39, 174, 96);
        btnKyq.ForeColor = Color.White;
        btnKyq.FlatStyle = FlatStyle.Flat;
        btnKyq.Cursor = Cursors.Hand;
        btnKyq.Click += (_, _) =>
        {
            using var f = new ShtoAksesorForm();
            if (f.ShowDialog(this) == DialogResult.OK)
                ShtoAksesorNeGrid(f.Emri, f.Sasia, f.Shenime);
        };

        var btnHiq = new Button();
        btnHiq.Text = "✕ HIQR";
        btnHiq.Size = new Size(90, 26);
        btnHiq.Location = new Point(100, 176);
        btnHiq.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        btnHiq.BackColor = Color.FromArgb(192, 57, 43);
        btnHiq.ForeColor = Color.White;
        btnHiq.FlatStyle = FlatStyle.Flat;
        btnHiq.Cursor = Cursors.Hand;
        btnHiq.Click += (_, _) =>
        {
            if (dgvAksesoret.CurrentRow != null && !dgvAksesoret.CurrentRow.IsNewRow)
                dgvAksesoret.Rows.Remove(dgvAksesoret.CurrentRow);
        };

        aksPanel.Controls.Add(btnKyq);
        aksPanel.Controls.Add(btnHiq);
        panelContent.Controls.Add(aksPanel);

        // ── KONFIRMO button at very bottom ──
        y += 215;
        var btnKonfirmo = new Button();
        btnKonfirmo.Text = "✓ KONFIRMO TRANSAKSIONET";
        btnKonfirmo.Size = new Size(300, 44);
        btnKonfirmo.Location = new Point(20, y);
        btnKonfirmo.Font = new Font("Segoe UI", 12, FontStyle.Bold);
        btnKonfirmo.BackColor = Color.FromArgb(39, 174, 96);
        btnKonfirmo.ForeColor = Color.White;
        btnKonfirmo.FlatStyle = FlatStyle.Flat;
        btnKonfirmo.Cursor = Cursors.Hand;
        btnKonfirmo.Click += BtnKonfirmo_Click;

        var btnAnulo = new Button();
        btnAnulo.Text = "✕ Anulo (kthehu)";
        btnAnulo.Size = new Size(160, 44);
        btnAnulo.Location = new Point(340, y);
        btnAnulo.Font = new Font("Segoe UI", 12, FontStyle.Bold);
        btnAnulo.BackColor = Color.FromArgb(192, 57, 43);
        btnAnulo.ForeColor = Color.White;
        btnAnulo.FlatStyle = FlatStyle.Flat;
        btnAnulo.Cursor = Cursors.Hand;
        btnAnulo.Click += (s, e) => ShowStep(3);

        panelContent.Controls.Add(btnKonfirmo);
        panelContent.Controls.Add(btnAnulo);
    }

    private void RuajAksesoretAktual()
    {
        if (_currentArmaIndex < 0 || _currentArmaIndex >= _selectedArmet.Count) return;
        _selectedArmet[_currentArmaIndex].Aksesoret.Clear();
        foreach (DataGridViewRow row in dgvAksesoret.Rows)
        {
            if (row.IsNewRow) continue;
            var emri = row.Cells["Emri"].Value?.ToString() ?? "";
            if (string.IsNullOrWhiteSpace(emri)) continue;
            int sasia = 1;
            int.TryParse(row.Cells["Sasia"].Value?.ToString(), out sasia);
            _selectedArmet[_currentArmaIndex].Aksesoret.Add(new Aksesori
            {
                Emri = emri,
                Sasia = sasia,
                Shenime = row.Cells["Shenime"].Value?.ToString() ?? ""
            });
        }
    }

    private void NgarkoAksesoretPerArmen(int index)
    {
        dgvAksesoret.Rows.Clear();
        if (index < 0 || index >= _selectedArmet.Count) return;
        foreach (var aks in _selectedArmet[index].Aksesoret)
            dgvAksesoret.Rows.Add(aks.Emri, aks.Sasia, aks.Shenime);
    }

    private void BuildStepPrinto()
    {
        lblStepTitle.Text = "Transaksioni u Regjistrua!";
        lblStepDescription.Text = "Te dhenat jane ruajtur automatikisht. Printo fletelejimin.";

        var lblSukses = new Label();
        lblSukses.Text = "✓ Transaksioni u regjistrua me sukses!";
        lblSukses.Font = new Font("Segoe UI", 14, FontStyle.Bold);
        lblSukses.ForeColor = Color.FromArgb(100, 200, 100);
        lblSukses.Size = new Size(500, 30);
        lblSukses.Location = new Point(40, 20);
        panelContent.Controls.Add(lblSukses);

        var lblDetaje = new Label();
        string armetList = string.Join("\n", _selectedArmet.Select((a, i) => $"  Arma {i + 1}: {a.ArmaText}"));
        lblDetaje.Text = $"{_numriRadhordhes}\n{_dataOraRegjistrimit}\n{_selectedPersoneliText}\n{_selectedKlientiText}\n{armetList}";
        lblDetaje.Font = new Font("Segoe UI", 10);
        lblDetaje.ForeColor = Color.FromArgb(200, 205, 216);
        lblDetaje.Size = new Size(400, 100);
        lblDetaje.Location = new Point(40, 60);
        panelContent.Controls.Add(lblDetaje);

        var lblPdfPath = new Label();
        if (string.IsNullOrEmpty(_pdfPath) && _selectedTransaksionIds.Count > 0)
        {
            var pdfDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ArmepunesApp", "Fleteleshimat", DateTime.Now.Year.ToString());
            var pdfFile = $"FL_{( _tipi== "Hyrje" ? "HYRJE" : "DALJE")}_{DateTime.Now.Year}_{_selectedTransaksionIds[0]:D6}.pdf";
            _pdfPath = Path.Combine(pdfDir, pdfFile);
        }
        lblPdfPath.Text = $"PDF: {_pdfPath}";
        lblPdfPath.Font = new Font("Segoe UI", 7);
        lblPdfPath.ForeColor = Color.FromArgb(140, 145, 155);
        lblPdfPath.Size = new Size(600, 18);
        lblPdfPath.Location = new Point(40, 155);
        panelContent.Controls.Add(lblPdfPath);

        var btnPrinto = new Button();
        btnPrinto.Text = "🖨 Printo Fleteleshimin";
        btnPrinto.Size = new Size(190, 38);
        btnPrinto.Location = new Point(40, 175);
        btnPrinto.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        btnPrinto.BackColor = Color.FromArgb(0, 140, 200);
        btnPrinto.ForeColor = Color.White;
        btnPrinto.FlatStyle = FlatStyle.Flat;
        btnPrinto.Cursor = Cursors.Hand;
        btnPrinto.Click += (s, e) => PrintoFleteleshimin();
        panelContent.Controls.Add(btnPrinto);

        var btnHapPdf = new Button();
        btnHapPdf.Text = "📂 Hap PDF";
        btnHapPdf.Size = new Size(110, 38);
        btnHapPdf.Location = new Point(240, 175);
        btnHapPdf.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        btnHapPdf.BackColor = Color.FromArgb(39, 174, 96);
        btnHapPdf.ForeColor = Color.White;
        btnHapPdf.FlatStyle = FlatStyle.Flat;
        btnHapPdf.Cursor = Cursors.Hand;
        btnHapPdf.Click += (_, _) =>
        {
            var path = _pdfPath;
            if (!File.Exists(path))
            {
                var pngPath = Path.ChangeExtension(path, ".png");
                if (File.Exists(pngPath)) path = pngPath;
                else { MessageBox.Show("As PDF as PNG nuk u gjeten.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            }
            try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(path) { UseShellExecute = true }); }
            catch (Exception ex) { MessageBox.Show($"Nuk mundi te hapet dokumenti:\n{ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        };
        panelContent.Controls.Add(btnHapPdf);

        var btnMbyll = new Button();
        btnMbyll.Text = "Mbyll";
        btnMbyll.Size = new Size(100, 38);
        btnMbyll.Location = new Point(360, 175);
        btnMbyll.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        btnMbyll.BackColor = Color.FromArgb(60, 62, 68);
        btnMbyll.ForeColor = Color.FromArgb(200, 205, 216);
        btnMbyll.FlatStyle = FlatStyle.Flat;
        btnMbyll.Cursor = Cursors.Hand;
        btnMbyll.Click += (s, e) => { DialogResult = DialogResult.OK; Close(); };
        panelContent.Controls.Add(btnMbyll);
    }

    private void BtnKonfirmo_Click(object? sender, EventArgs e)
    {
        _qellimi = txtQellimiStep.Text.Trim();
        _dorzoi = txtDorzoiStep.Text.Trim();
        _morri = txtMorriStep.Text.Trim();
        _shenime = txtShenimeStep.Text.Trim();
        var municioni = txtMunicionStep.Text.Trim();

        if (string.IsNullOrWhiteSpace(_qellimi))
        {
            MessageBox.Show("Ju lutem plotesoni qellimin e transaksionit.", "Kerkese", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(_dorzoi))
        {
            MessageBox.Show("Ju lutem plotesoni dorezuesin.", "Kerkese", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(_morri))
        {
            MessageBox.Show("Ju lutem plotesoni marresin.", "Kerkese", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // Save current weapon's accessories before confirming
        RuajAksesoretAktual();

        _dataOraRegjistrimit = dtpDataPranimit.Value.ToString("yyyy-MM-dd HH:mm:ss");

        _selectedTransaksionIds.Clear();
        try
        {
            foreach (var arma in _selectedArmet)
            {
                var transaksioni = new Transaksioni
                {
                    ArmaId = arma.ArmaId,
                    PersoneliId = _selectedPersoneliId,
                    KlientiId = _selectedKlientiId,
                    Tipi = _tipi,
                    DataOra = _dataOraRegjistrimit,
                    Qellimi = _qellimi,
                    PersoneliQeDorzoi = _dorzoi,
                    PersoneliQeMorri = _morri,
                    Shenime = _shenime,
                    Municioni = municioni,
                    Aksesoret = arma.Aksesoret
                };

                int id = _db.RegjistroTransaksion(transaksioni, _perdoruesi);
                _selectedTransaksionIds.Add(id);
            }

            // Export PDF for the first transaction
            if (_selectedTransaksionIds.Count > 0)
            {
                try { _pdfPath = ExportHelper.EksportoFleteleshimAuto(_db, _selectedTransaksionIds[0], _perdoruesi); }
                catch { _pdfPath = ""; }
            }

            ShowStep(5);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Gabim gjate regjistrimit:\n{ex.Message}", "Gabim",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ShtoAksesorNeGrid(string emri, int sasia, string shenime)
    {
        foreach (DataGridViewRow row in dgvAksesoret.Rows)
        {
            if (row.IsNewRow) continue;
            if ((row.Cells["Emri"].Value?.ToString() ?? "").Equals(emri, StringComparison.OrdinalIgnoreCase))
            {
                var existing = 1;
                int.TryParse(row.Cells["Sasia"].Value?.ToString(), out existing);
                row.Cells["Sasia"].Value = existing + sasia;
                return;
            }
        }
        dgvAksesoret.Rows.Add(emri, sasia, shenime);
    }

    private void BtnShtoAksesor_Click(object? sender, EventArgs e)
    {
        var txtEmri = new TextBox { BackColor = Color.FromArgb(40, 42, 48), ForeColor = Color.FromArgb(200, 205, 216), BorderStyle = BorderStyle.FixedSingle };
        var nudSasia = new NumericUpDown { Minimum = 1, Maximum = 999, Value = 1, BackColor = Color.FromArgb(40, 42, 48), ForeColor = Color.FromArgb(200, 205, 216) };
        var txtShenime = new TextBox { BackColor = Color.FromArgb(40, 42, 48), ForeColor = Color.FromArgb(200, 205, 216), BorderStyle = BorderStyle.FixedSingle };

        var form = new Form();
        form.Text = "Shto Aksesor";
        form.Size = new Size(360, 220);
        form.StartPosition = FormStartPosition.CenterParent;
        form.FormBorderStyle = FormBorderStyle.FixedDialog;
        form.MaximizeBox = false;
        form.MinimizeBox = false;
        form.BackColor = Color.FromArgb(30, 33, 40);
        form.ForeColor = Color.FromArgb(200, 205, 216);
        form.Font = new Font("Segoe UI", 9);

        var lbl1 = new Label { Text = "Emri:", Location = new Point(15, 15), Size = new Size(60, 22), ForeColor = Color.FromArgb(200, 205, 216) };
        txtEmri.Location = new Point(80, 12); txtEmri.Size = new Size(240, 24);

        var lbl2 = new Label { Text = "Sasia:", Location = new Point(15, 48), Size = new Size(60, 22), ForeColor = Color.FromArgb(200, 205, 216) };
        nudSasia.Location = new Point(80, 45); nudSasia.Size = new Size(80, 24);

        var lbl3 = new Label { Text = "Shenime:", Location = new Point(15, 80), Size = new Size(60, 22), ForeColor = Color.FromArgb(200, 205, 216) };
        txtShenime.Location = new Point(80, 77); txtShenime.Size = new Size(240, 24);

        var btnOk = new Button { Text = "OK", Location = new Point(80, 120), Size = new Size(100, 32), BackColor = Color.FromArgb(0, 120, 215), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.OK };
        var btnCancel = new Button { Text = "Anulo", Location = new Point(190, 120), Size = new Size(100, 32), BackColor = Color.FromArgb(60, 62, 68), ForeColor = Color.FromArgb(200, 205, 216), FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.Cancel };

        form.Controls.AddRange(new Control[] { lbl1, txtEmri, lbl2, nudSasia, lbl3, txtShenime, btnOk, btnCancel });
        form.AcceptButton = btnOk;
        form.CancelButton = btnCancel;

        if (form.ShowDialog(this) == DialogResult.OK)
        {
            var emri = txtEmri.Text.Trim();
            if (!string.IsNullOrWhiteSpace(emri))
                ShtoAksesorNeGrid(emri, (int)nudSasia.Value, txtShenime.Text.Trim());
        }
    }

    private void PrintoFleteleshimin()
    {
        if (_selectedTransaksionIds.Count == 0)
        {
            MessageBox.Show("Transaksionet nuk u regjistruan si duhet.", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var rows = new List<DataRow>();
        foreach (int tid in _selectedTransaksionIds)
        {
            var row = _db.MerrTransaksionById(tid);
            if (row != null) rows.Add(row);
        }

        if (rows.Count > 0)
        {
            var fleteleshim = new FleteleshimForm(_db, rows, "TRANSAKSION");
            fleteleshim.ShowDialog();
        }
        else
        {
            MessageBox.Show("Transaksionet u ruajten por nuk u gjeten per printim.", "Info",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    public ComboboxItem GetSelectedItem(ListBox list)
    {
        return (ComboboxItem)(list.SelectedItem ?? throw new InvalidOperationException("No item selected"));
    }

    private void btnNext_Click(object sender, EventArgs e)
    {
        if (_currentStep == 1)
        {
            if (listPersoneli.SelectedItem != null)
            {
                var item = GetSelectedItem(listPersoneli);
                _selectedPersoneliId = item.Value;
                _selectedPersoneliText = item.Text;
            }
            else { _selectedPersoneliId = 0; _selectedPersoneliText = "Ne pritje"; }
            ShowStep(2);
        }
        else if (_currentStep == 2)
        {
            if (listKlientet.SelectedItem != null)
            {
                var item = GetSelectedItem(listKlientet);
                _selectedKlientiId = item.Value;
                _selectedKlientiText = item.Text;
            }
            else { _selectedKlientiId = 0; _selectedKlientiText = "Ne pritje"; }
            ShowStep(3);
        }
        else if (_currentStep == 3)
        {
            ShowStep(4);
        }
        else if (_currentStep == 4)
        {
            BtnKonfirmo_Click(null, EventArgs.Empty);
        }
        else if (_currentStep == 5)
        {
            PrintoFleteleshimin();
        }
    }

    private void btnBack_Click(object sender, EventArgs e)
    {
        if (_currentStep > 1)
            ShowStep(_currentStep - 1);
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        if (_currentStep == 5)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
        else
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}

public class ComboboxItem
{
    public string Text { get; set; } = "";
    public int Value { get; set; }
    public override string ToString() => Text;
}
