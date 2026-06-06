using ArmepunesApp.Data;
using ArmepunesApp.Models;
using ArmepunesApp.Services;

namespace ArmepunesApp.Forms;

public partial class ShtoPersonelForm : Form
{
    private readonly DatabaseHelper _db;
    private readonly Personeli? _personeliEkzistues;

    public ShtoPersonelForm(DatabaseHelper db, Personeli? personeli)
    {
        _db = db;
        _personeliEkzistues = personeli;
        InitializeComponent();
        if (personeli != null) MbushFushat(personeli);
    }

    private void MbushFushat(Personeli p)
    {
        txtEmri.Text = p.Emri;
        txtMbiemri.Text = p.Mbiemri;
        txtGrada.Text = p.Grada;
        txtNjesia.Text = p.Njesia;
        txtNrLegjitimacioni.Text = p.NrLegjitimacioni;
        txtTelefon.Text = p.Telefon;
    }

    private void btnRuaj_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtEmri.Text) || string.IsNullOrWhiteSpace(txtMbiemri.Text))
        {
            MessageBox.Show("Emri dhe Mbiemri jane fushat e detyrueshme!", "Gabim",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // Enhanced validation for personnel operations
        string emri = txtEmri.Text.Trim();
        string mbiemri = txtMbiemri.Text.Trim();
        string grada = txtGrada.Text.Trim();
        string nesia = txtNjesia.Text.Trim();
        string nrLegjitimacioni = txtNrLegjitimacioni.Text.Trim();
        string telefon = txtTelefon.Text.Trim();
        
        // Validate required fields
        if (string.IsNullOrWhiteSpace(emri))
        {
            MessageBox.Show("Emri i personelit është i detyrueshëm!", "Gabim i validimit",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        
        if (string.IsNullOrWhiteSpace(mbiemri))
        {
            MessageBox.Show("Mbiemri i personelit është i detyrueshëm!", "Gabim i validimit",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        
        if (string.IsNullOrWhiteSpace(grada))
        {
            MessageBox.Show("Grada e personelit është e detyrueshme!", "Gabim i validimit",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        
        if (string.IsNullOrWhiteSpace(nesia))
        {
            MessageBox.Show("Njesia e personelit është i detyrueshëm!", "Gabim i validimit",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        
        if (!string.IsNullOrWhiteSpace(nrLegjitimacioni))
        {
            // Existing DB constraint handles uniqueness
        }
        
        var p = new Personeli
        {
            Emri = emri,
            Mbiemri = mbiemri,
            Grada = grada,
            Njesia = nesia,
            NrLegjitimacioni = nrLegjitimacioni,
            Telefon = telefon
        };

        try
        {
            if (_personeliEkzistues != null)
            {
                p.Id = _personeliEkzistues.Id;
                _db.NdryshoPersonel(p);
            }
            else
            {
                _db.ShtoPersonel(p);
            }
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            ErrorHandlerService.HandleException(ex, "shtoj/ndrysho personel", null, "shto_personel");
        }
    }

    private void btnAnulo_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }
}
