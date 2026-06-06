using ArmepunesApp.Data;
using ArmepunesApp.Models;
using ArmepunesApp.Services;

namespace ArmepunesApp.Forms;

public partial class ShtoPerdoruesForm : Form
{
    private readonly DatabaseHelper _db;
    private readonly Perdoruesi? _perdoruesiEkzistues;

    public ShtoPerdoruesForm(DatabaseHelper db, Perdoruesi? perdoruesi)
    {
        _db = db;
        _perdoruesiEkzistues = perdoruesi;
        InitializeComponent();
        if (perdoruesi != null) MbushFushat(perdoruesi);
    }

    private void MbushFushat(Perdoruesi p)
    {
        txtUsername.Text = p.Username;
        txtPassword.Text = p.Password;
        txtEmri.Text = p.Emri;
        cmbRole.Text = p.Role;
    }

    private void btnRuaj_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
        {
            MessageBox.Show("Username dhe Password jane te detyrueshme!", "Gabim",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var p = new Perdoruesi
        {
            Username = txtUsername.Text.Trim(),
            Password = txtPassword.Text.Trim(),
            Emri = txtEmri.Text.Trim(),
            Role = cmbRole.Text.Trim()
        };

        try
        {
            if (_perdoruesiEkzistues != null)
            {
                p.Id = _perdoruesiEkzistues.Id;
                _db.NdryshoPerdorues(p);
            }
            else
            {
                _db.ShtoPerdorues(p);
            }
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            ErrorHandlerService.HandleException(ex, "shtoj/ndrysho perdorues", null, "shto_perdorues");
        }
    }

    private void btnAnulo_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }
}
