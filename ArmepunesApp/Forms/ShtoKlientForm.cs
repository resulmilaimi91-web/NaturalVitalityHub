using ArmepunesApp.Data;
using ArmepunesApp.Models;
using ArmepunesApp.Services;

namespace ArmepunesApp.Forms;

public partial class ShtoKlientForm : Form
{
    private readonly DatabaseHelper _db;
    private readonly Klienti? _klientiEkzistues;
    public int KlientiId { get; private set; }

    public ShtoKlientForm(DatabaseHelper db, Klienti? klienti)
    {
        _db = db;
        _klientiEkzistues = klienti;
        InitializeComponent();
        if (klienti != null) MbushFushat(klienti);
    }

    private void MbushFushat(Klienti k)
    {
        txtEmri.Text = k.Emri ?? "";
        txtMbiemri.Text = k.Mbiemri ?? "";
        txtAdresa.Text = k.Adresa ?? "";
        txtTelefon.Text = k.Telefon ?? "";
        txtEmail.Text = k.Email ?? "";
        txtNrLeternjoftimit.Text = k.NrLeternjoftimit ?? "";
        txtShenime.Text = k.Shenime ?? "";
    }

    private void btnRuaj_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtEmri.Text) || string.IsNullOrWhiteSpace(txtMbiemri.Text))
        {
            MessageBox.Show("Emri dhe Mbiemri jane te detyrueshme!", "Gabim",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var nid = txtNrLeternjoftimit.Text.Trim();
        if (!string.IsNullOrWhiteSpace(nid))
        {
            var ekzNid = _db.MerrKlientByNID(nid);
            if (ekzNid != null)
            {
                var r = ekzNid.Rows[0];
                if (MessageBox.Show($"Ky numer letërnjoftimi ekziston per:\n{r["Emri"]} {r["Mbiemri"]}\n\nDeshironi ta perdorni kete klient?", "Klienti ekziston",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    KlientiId = Convert.ToInt32(r["Id"]);
                    DialogResult = DialogResult.OK;
                    Close();
                    return;
                }
            }
        }

        var ekzEmri = _db.MerrKlientByEmriMbiemri(txtEmri.Text.Trim(), txtMbiemri.Text.Trim());
        if (ekzEmri != null && _klientiEkzistues == null)
        {
            var r = ekzEmri.Rows[0];
            if (r["Id"] != null && !string.IsNullOrWhiteSpace(r["NrLeternjoftimit"]?.ToString()))
            {
                if (MessageBox.Show($"Klienti '{txtEmri.Text.Trim()} {txtMbiemri.Text.Trim()}' ekziston me Nr.Leternjoftimit: {r["NrLeternjoftimit"]}\n\nDeshironi ta perdorni ate?", "Klienti ekziston",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    KlientiId = Convert.ToInt32(r["Id"]);
                    DialogResult = DialogResult.OK;
                    Close();
                    return;
                }
            }
        }

        var klienti = new Klienti
        {
            Emri = txtEmri.Text.Trim(),
            Mbiemri = txtMbiemri.Text.Trim(),
            Adresa = txtAdresa.Text.Trim(),
            Telefon = txtTelefon.Text.Trim(),
            Email = txtEmail.Text.Trim(),
            NrLeternjoftimit = nid,
            Shenime = txtShenime.Text.Trim()
        };

        try
        {
            if (_klientiEkzistues != null)
            {
                klienti.Id = _klientiEkzistues.Id;
                _db.NdryshoKlient(klienti);
            }
            else
            {
                _db.ShtoKlient(klienti);
            }
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            ErrorHandlerService.HandleException(ex, "shtoj/ndrysho klient", null, "shto_klient");
        }
    }

    private void btnAnulo_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }
}
