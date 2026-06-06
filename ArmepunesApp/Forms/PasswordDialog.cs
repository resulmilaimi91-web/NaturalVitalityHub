using ArmepunesApp.Data;

namespace ArmepunesApp.Forms;

public partial class PasswordDialog : Form
{
    private readonly DatabaseHelper _db;
    public bool Autorizuar { get; private set; }

    public PasswordDialog(DatabaseHelper db, string veprimi)
    {
        _db = db;
        InitializeComponent();
        lblVeprimi.Text = $"Kerkohet autorizim per: {veprimi}";
    }

    private void btnAutorizo_Click(object sender, EventArgs e)
    {
        if (_db.VerifikoPerdoruesin(txtUsername.Text.Trim(), txtPassword.Text.Trim(), out var perdoruesi)
            && perdoruesi != null && perdoruesi.Role == "Admin")
        {
            Autorizuar = true;
            DialogResult = DialogResult.OK;
            Close();
        }
        else
        {
            MessageBox.Show("Vetem admin mund te autorizoje! Username ose password i gabuar.",
                "Autorizim i deshtuar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtPassword.Clear();
        }
    }

    private void btnAnulo_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }
}
