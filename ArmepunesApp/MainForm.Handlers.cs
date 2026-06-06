using System.Data;
using ArmepunesApp.Data;
using ArmepunesApp.Forms;

namespace ArmepunesApp;

public partial class MainForm
{
    private void btnRegjistroHyrje_Click(object? sender, EventArgs e)
    {
        if (!KaLeje("Regjistro Transaksion"))
        { MessageBox.Show("Nuk keni leje.", "Ndaluar", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        using var f = new RegjistroWizardForm(_db, "Hyrje", _perdoruesi.Username);
        if (f.ShowDialog(this) == DialogResult.OK)
        {
            NgarkoArmet(txtKerkimArme.Text);
            NgarkoTransaksionet(txtKerkimTransaksion.Text);
            NgarkoGjendjenDeponimit();
            NgarkoBallina();
        }
    }

    private void btnRegjistroDalje_Click(object? sender, EventArgs e)
    {
        if (!KaLeje("Regjistro Transaksion"))
        { MessageBox.Show("Nuk keni leje.", "Ndaluar", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        using var f = new RegjistroWizardForm(_db, "Dalje", _perdoruesi.Username);
        if (f.ShowDialog(this) == DialogResult.OK)
        {
            NgarkoArmet(txtKerkimArme.Text);
            NgarkoTransaksionet(txtKerkimTransaksion.Text);
            NgarkoGjendjenDeponimit();
            NgarkoBallina();
        }
    }

    private void btnShtoPersonel_Click(object? sender, EventArgs e)
    {
        if (!KaLeje("Shto/Ndrysho Personel"))
        { MessageBox.Show("Nuk keni leje.", "Ndaluar", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        using var f = new ShtoPersonelForm(_db, null);
        if (f.ShowDialog(this) == DialogResult.OK)
            NgarkoPersonelin();
    }

    private void btnShtoKlient_Click(object? sender, EventArgs e)
    {
        if (!KaLeje("Shto/Ndrysho Klient"))
        { MessageBox.Show("Nuk keni leje.", "Ndaluar", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        using var f = new ShtoKlientForm(_db, null);
        if (f.ShowDialog(this) == DialogResult.OK)
            NgarkoKlientet();
    }

    private void btnShtoArme_Click(object? sender, EventArgs e)
    {
        if (!KaLeje("Shto/Ndrysho Arme"))
        { MessageBox.Show("Nuk keni leje.", "Ndaluar", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        using var f = new ShtoArmeForm(_db, null);
        if (f.ShowDialog(this) == DialogResult.OK)
        {
            NgarkoArmet(txtKerkimArme.Text);
            NgarkoGjendjenDeponimit();
        }
    }

    private void btnBatchArme_Click(object? sender, EventArgs e)
    {
        if (!KaLeje("Shto/Ndrysho Arme"))
        { MessageBox.Show("Nuk keni leje.", "Ndaluar", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        using var f = new Forms.BatchArmeForm(_db);
        if (f.ShowDialog(this) == DialogResult.OK)
        {
            NgarkoArmet(txtKerkimArme.Text);
            NgarkoGjendjenDeponimit();
        }
    }

    private void btnPrintoFleteleshim_Click(object? sender, EventArgs e)
    {
        if (!KaLeje("Printo Fleteleshim"))
        { MessageBox.Show("Nuk keni leje.", "Ndaluar", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        using var f = new ListaFleteleshimeveForm(_db, _perdoruesi.Username);
        f.ShowDialog(this);
    }

    private void btnListaFleteleshimeve_Click(object? sender, EventArgs e)
    {
        if (!KaLeje("Listo Fleteleshime"))
        { MessageBox.Show("Nuk keni leje.", "Ndaluar", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        using var f = new ListaFleteleshimeveForm(_db, _perdoruesi.Username);
        f.ShowDialog(this);
    }

    private void btnListeDeponimi_Click(object? sender, EventArgs e)
    {
        if (!KaLeje("Liste Deponimi"))
        { MessageBox.Show("Nuk keni leje.", "Ndaluar", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        tabControl.SelectedTab = tabGjendja;
        NgarkoGjendjenDeponimit();
    }

    private void btnFormaTemplates_Click(object? sender, EventArgs e)
    {
        if (!KaLeje("Forma A4 Template"))
        { MessageBox.Show("Nuk keni leje.", "Ndaluar", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        using var f = new FormaTemplateForm(_db);
        f.ShowDialog(this);
    }

    private void btnRaporto_Click(object? sender, EventArgs e)
    {
        if (!KaLeje("Raporto"))
        { MessageBox.Show("Nuk keni leje.", "Ndaluar", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        using var f = new RaportoForm(_db, _perdoruesi.Role == "Admin");
        f.ShowDialog(this);
    }

    private void btnEksporto_Click(object? sender, EventArgs e)
    {
        if (!KaLeje("Eksporto"))
        { MessageBox.Show("Nuk keni leje.", "Ndaluar", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        using var f = new ExportRaportForm(_db);
        f.ShowDialog(this);
    }

    private void btnFshiTransaksion_Click(object? sender, EventArgs e)
    {
        if (!KaLeje("Fshi Transaksion"))
        { MessageBox.Show("Nuk keni leje.", "Ndaluar", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        if (!KerkoAutorizim("Fshi Transaksion"))
            return;

        if (dgvTransaksionet.SelectedRows.Count == 0)
        {
            MessageBox.Show("Zgjidh nje transaksion per te fshire.", "Info",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var idObj = dgvTransaksionet.SelectedRows[0].Cells["Id"]?.Value;
        if (idObj == null) return;
        int id = Convert.ToInt32(idObj);

        if (MessageBox.Show("Jeni te sigurt qe doni te fshini kete transaksion?",
            "Konfirmim", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        if (_db.FshiTransaksion(id, _perdoruesi.Username))
        {
            NgarkoTransaksionet(txtKerkimTransaksion.Text);
            NgarkoBallina();
        }
        else
            MessageBox.Show("Transaksioni nuk mund te fshihet.", "Gabim",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    private void btnUpdateAplikacionit_Click(object? sender, EventArgs e)
    {
        if (!KaLeje("Update Aplikacionit"))
        { MessageBox.Show("Nuk keni leje.", "Ndaluar", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        var updateUrl = _db.MerrCilësimin("update_url");
        var updater = new UpdateHelper(updateUrl);
        using var f = new UpdateForm(updater);
        f.ShowDialog(this);
    }

    // ============== NDRYSHO / FSHI nga DataGridViews ==============

    private void btnNdryshoArme_Click(object? sender, EventArgs e)
    {
        if (!KaLeje("Shto/Ndrysho Arme"))
        { MessageBox.Show("Nuk keni leje.", "Ndaluar", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        if (dgvArmet.SelectedRows.Count == 0)
        {
            MessageBox.Show("Zgjidh nje arme nga lista.", "Info",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        var idObj = dgvArmet.SelectedRows[0].Cells["Id"]?.Value;
        if (idObj == null) return;
        var arma = _db.MerrArmeById(Convert.ToInt32(idObj));
        if (arma == null) return;

        using var f = new ShtoArmeForm(_db, arma);
        if (f.ShowDialog(this) == DialogResult.OK)
        {
            NgarkoArmet(txtKerkimArme.Text);
            NgarkoGjendjenDeponimit();
        }
    }

    private void btnFshiArme_Click(object? sender, EventArgs e)
    {
        if (!KaLeje("Fshi Arme"))
        { MessageBox.Show("Nuk keni leje.", "Ndaluar", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        if (!KerkoAutorizim("Fshi Arme"))
            return;

        if (dgvArmet.SelectedRows.Count == 0)
        {
            MessageBox.Show("Zgjidh nje arme per te fshire.", "Info",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        var idObj = dgvArmet.SelectedRows[0].Cells["Id"]?.Value;
        if (idObj == null) return;
        int id = Convert.ToInt32(idObj);

        if (MessageBox.Show("Jeni te sigurt qe doni te fshini kete arme?",
            "Konfirmim", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        if (_db.FshiArme(id, _perdoruesi.Username))
        {
            NgarkoArmet(txtKerkimArme.Text);
            NgarkoGjendjenDeponimit();
            NgarkoBallina();
            NgarkoTransaksionet(txtKerkimTransaksion.Text);
        }
        else
            MessageBox.Show("Arma nuk mund te fshihet.", "Gabim",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    private void btnNdryshoPersonel_Click(object? sender, EventArgs e)
    {
        if (!KaLeje("Shto/Ndrysho Personel"))
        { MessageBox.Show("Nuk keni leje.", "Ndaluar", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        if (dgvPersoneli.SelectedRows.Count == 0)
        {
            MessageBox.Show("Zgjidh nje personel nga lista.", "Info",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        var idObj = dgvPersoneli.SelectedRows[0].Cells["Id"]?.Value;
        if (idObj == null) return;

        var personeli = new Models.Personeli { Id = Convert.ToInt32(idObj) };
        var dt = _db.MerrPersonelin();
        foreach (DataRow row in dt.Rows)
        {
            if (Convert.ToInt32(row["Id"]) == personeli.Id)
            {
                personeli.Emri = row["Emri"]?.ToString() ?? "";
                personeli.Mbiemri = row["Mbiemri"]?.ToString() ?? "";
                personeli.Grada = row["Grada"]?.ToString() ?? "";
                personeli.Njesia = row["Njesia"]?.ToString() ?? "";
                personeli.NrLegjitimacioni = row["NrLegjitimacioni"]?.ToString() ?? "";
                personeli.Telefon = row["Telefon"]?.ToString() ?? "";
                break;
            }
        }

        using var f = new ShtoPersonelForm(_db, personeli);
        if (f.ShowDialog(this) == DialogResult.OK)
            NgarkoPersonelin();
    }

    private void btnFshiPersonel_Click(object? sender, EventArgs e)
    {
        if (!KaLeje("Fshi Personel"))
        { MessageBox.Show("Nuk keni leje.", "Ndaluar", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        if (!KerkoAutorizim("Fshi Personel"))
            return;

        if (dgvPersoneli.SelectedRows.Count == 0)
        {
            MessageBox.Show("Zgjidh nje personel per te fshire.", "Info",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        var idObj = dgvPersoneli.SelectedRows[0].Cells["Id"]?.Value;
        if (idObj == null) return;
        int id = Convert.ToInt32(idObj);

        if (MessageBox.Show("Jeni te sigurt qe doni te fshini kete personel?",
            "Konfirmim", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        if (_db.FshiPersonel(id, _perdoruesi.Username))
            NgarkoPersonelin();
        else
            MessageBox.Show("Personeli nuk mund te fshihet.", "Gabim",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    private void btnNdryshoKlient_Click(object? sender, EventArgs e)
    {
        if (!KaLeje("Shto/Ndrysho Klient"))
        { MessageBox.Show("Nuk keni leje.", "Ndaluar", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        if (dgvKlientet.SelectedRows.Count == 0)
        {
            MessageBox.Show("Zgjidh nje klient nga lista.", "Info",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        var idObj = dgvKlientet.SelectedRows[0].Cells["Id"]?.Value;
        if (idObj == null) return;

        var klienti = new Models.Klienti { Id = Convert.ToInt32(idObj) };
        var dt = _db.MerrKlientet();
        foreach (DataRow row in dt.Rows)
        {
            if (Convert.ToInt32(row["Id"]) == klienti.Id)
            {
                klienti.Emri = row["Emri"]?.ToString() ?? "";
                klienti.Mbiemri = row["Mbiemri"]?.ToString() ?? "";
                klienti.Adresa = row["Adresa"]?.ToString() ?? "";
                klienti.Telefon = row["Telefon"]?.ToString() ?? "";
                klienti.Email = row["Email"]?.ToString() ?? "";
                klienti.NrLeternjoftimit = row["NrLeternjoftimit"]?.ToString() ?? "";
                klienti.Shenime = row["Shenime"]?.ToString() ?? "";
                break;
            }
        }

        using var f = new ShtoKlientForm(_db, klienti);
        if (f.ShowDialog(this) == DialogResult.OK)
            NgarkoKlientet();
    }

    private void btnFshiKlient_Click(object? sender, EventArgs e)
    {
        if (!KaLeje("Fshi Klient"))
        { MessageBox.Show("Nuk keni leje.", "Ndaluar", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        if (!KerkoAutorizim("Fshi Klient"))
            return;

        if (dgvKlientet.SelectedRows.Count == 0)
        {
            MessageBox.Show("Zgjidh nje klient per te fshire.", "Info",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        var idObj = dgvKlientet.SelectedRows[0].Cells["Id"]?.Value;
        if (idObj == null) return;
        int id = Convert.ToInt32(idObj);

        if (MessageBox.Show("Jeni te sigurt qe doni te fshini kete klient?",
            "Konfirmim", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        if (_db.FshiKlient(id, _perdoruesi.Username))
            NgarkoKlientet();
        else
            MessageBox.Show("Klienti nuk mund te fshihet.", "Gabim",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    // ============== KERKIM ==============

    private void txtKerkimArme_TextChanged(object? sender, EventArgs e)
    {
        NgarkoArmet(txtKerkimArme.Text);
    }

    private void txtKerkimTransaksion_TextChanged(object? sender, EventArgs e)
    {
        NgarkoTransaksionet(txtKerkimTransaksion.Text);
    }

    private void txtKerkimHistoriku_TextChanged(object? sender, EventArgs e)
    {
        NgarkoHistorikun(txtKerkimHistoriku.Text);
    }
}
