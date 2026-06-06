using ArmepunesApp.Data;
using ArmepunesApp.Models;
using ArmepunesApp.Services;

namespace ArmepunesApp.Forms;

public partial class ShtoArmeForm : Form
{
    private readonly DatabaseHelper _db;
    private readonly Arma? _armaEkzistuese;

    public ShtoArmeForm(DatabaseHelper db, Arma? arma)
    {
        _db = db;
        _armaEkzistuese = arma;
        InitializeComponent();
        if (arma != null) MbushFushat(arma);
    }

    private void MbushFushat(Arma a)
    {
        txtNumerSerial.Text = a.NumerSerial ?? "";
        txtLloji.Text = a.Lloji ?? "";
        txtMarka.Text = a.Marka ?? "";
        txtModeli.Text = a.Modeli ?? "";
        txtKalibri.Text = a.Kalibri ?? "";
        txtVendlindja.Text = a.Vendlindja ?? "";
        cmbStatusi.SelectedItem = a.Statusi ?? "Ne Magazine";
        txtShenime.Text = a.Shenime ?? "";
        if (!string.IsNullOrWhiteSpace(a.DataRegjistrimit))
        {
            if (DateTime.TryParse(a.DataRegjistrimit, out var dt))
                dtpDataRegjistrimit.Value = dt;
        }
        txtNrInventari.Text = a.NrInventari ?? "";
    }

    private void btnRuaj_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtNumerSerial.Text) ||
            string.IsNullOrWhiteSpace(txtLloji.Text))
        {
            MessageBox.Show("Numer Serial dhe Lloji jane fushat e detyrueshme!", "Gabim",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // Enhanced validation for weapon operations
        string numerSerial = txtNumerSerial.Text.Trim();
        string lloji = txtLloji.Text.Trim();
        string marka = txtMarka.Text.Trim();
        string modeli = txtModeli.Text.Trim();
        string kalibri = txtKalibri.Text.Trim();
        string vendlindja = txtVendlindja.Text.Trim();
        string shenime = txtShenime.Text.Trim();
        string nrInventari = txtNrInventari.Text.Trim();
        
        // Validate required fields
        if (string.IsNullOrWhiteSpace(numerSerial))
        {
            MessageBox.Show("Numer Serial i armas është i detyrueshëm!", "Gabim i validimit",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        
        if (string.IsNullOrWhiteSpace(lloji))
        {
            MessageBox.Show("Lloji i armas është i detyrueshëm!", "Gabim i validimit",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        
        if (string.IsNullOrWhiteSpace(marka))
        {
            MessageBox.Show("Marka e armas është e detyrueshme!", "Gabim i validimit",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        
        if (string.IsNullOrWhiteSpace(modeli))
        {
            MessageBox.Show("Modeli i armas është i detyrueshëm!", "Gabim i validimit",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        
        if (string.IsNullOrWhiteSpace(kalibri))
        {
            MessageBox.Show("Kalibri i armas është i detyrueshëm!", "Gabim i validimit",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        
        // Validate date format
        if (!DateTime.TryParse(dtpDataRegjistrimit.Value.ToString("yyyy-MM-dd"), out _))
        {
            MessageBox.Show("Formati i datës registreimit është i pavlefshëm!", "Gabim i validimit",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        
        // Check for duplicate serial number (additional check beyond UNIQUE constraint)
        Arma? existingArma = null;
        if (_armaEkzistuese == null) // Adding new weapon
        {
            existingArma = _db.MerrArmeBySerial(numerSerial);
        }
        else // Updating existing weapon - allow same serial if it's the same weapon
        {
            if (_armaEkzistuese.NumerSerial != numerSerial)
                existingArma = _db.MerrArmeBySerial(numerSerial);
        }
        
        if (existingArma != null)
        {
            MessageBox.Show($"Arme me numer serial '{numerSerial}' ekziston tashmë!\n" +
                           $"ID: {existingArma.Id}, Lloji: {existingArma.Lloji}", 
                           "Gabim i validimit", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        
        var arma = new Arma
        {
            NumerSerial = numerSerial,
            Lloji = lloji,
            Marka = marka,
            Modeli = modeli,
            Kalibri = kalibri,
            Vendlindja = vendlindja,
            Statusi = cmbStatusi.SelectedItem?.ToString() ?? "Ne Magazine",
            Shenime = shenime,
            DataRegjistrimit = dtpDataRegjistrimit.Value.ToString("yyyy-MM-dd"),
            NrInventari = nrInventari
        };

        try
        {
            if (_armaEkzistuese != null)
            {
                arma.Id = _armaEkzistuese.Id;
                _db.NdryshoArme(arma);
            }
            else
            {
                _db.ShtoArme(arma);
            }
            DialogResult = DialogResult.OK;
            Close();
            }
            catch (Exception ex)
            {
                ErrorHandlerService.HandleException(ex, "shtoj/ndrysho arme", null, "shto_arme");
            }
    }

    private void btnAnulo_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }
}
