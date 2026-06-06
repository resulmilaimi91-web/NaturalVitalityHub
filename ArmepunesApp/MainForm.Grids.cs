using System.Data;
using ArmepunesApp.Services;

namespace ArmepunesApp;

public partial class MainForm
{
    private void NgarkoArmet(string filter = "")
    {
        var dt = string.IsNullOrEmpty(filter) ? _db.MerrArmet() : _db.KerkoArmet(filter);
        _armetTable = dt;
        dgvArmet.DataSource = dt;
        lblArmetCount.Text = $"Gjithsej: {dt.Rows.Count} arme";
        PerditesoStatistikat();
    }

    private void NgarkoPersonelin(string filter = "")
    {
        var dt = _db.MerrPersonelin();
        _personeliTable = dt;
        dgvPersoneli.DataSource = dt;
        lblPersoneliCount.Text = $"Gjithsej: {dt.Rows.Count} personel";
    }

    private void NgarkoKlientet(string filter = "")
    {
        var dt = _db.MerrKlientet();
        _klientetTable = dt;
        dgvKlientet.DataSource = dt;
        lblKlientetCount.Text = $"Gjithsej: {dt.Rows.Count} kliente";
    }

    private void NgarkoTransaksionet(string filter = "")
    {
        var dt = string.IsNullOrEmpty(filter) ? _db.MerrTransaksionet() : _db.KerkoTransaksionet(filter);
        _transaksionetTable = dt;
        dgvTransaksionet.DataSource = dt;
        lblTransaksionetCount.Text = $"Gjithsej: {dt.Rows.Count} transaksione";
    }

    private void NgarkoGjendjenDeponimit()
    {
        var dt = _db.MerrGjendjenDeponimit();
        dgvGjendjaDeponimit.DataSource = dt;
        lblGjendjaCount.Text = $"Gjithsej: {dt.Rows.Count} arme ne deponim";
    }

    private void NgarkoHistorikun(string filter)
    {
        var dt = string.IsNullOrEmpty(filter) ? _db.MerrAuditLog() : _db.KerkoAuditLog(filter);
        dgvHistoriku.DataSource = dt;
    }

    private void NgarkoPerdoruesit()
    {
        var dt = _db.MerrPerdoruesit();
        _perdoruesitTable = dt;
        dgvPerdoruesit.DataSource = dt;
    }

    private void NgarkoPersoneliDetail()
    {
        if (dgvPersoneli.SelectedRows.Count == 0)
        {
            lblPersoneliDetaje.Text = "\uD83D\uDC64 Zgjidh nje personel per te pare detajet";
            dgvPersoneliTrans.DataSource = null;
            return;
        }

        var row = dgvPersoneli.SelectedRows[0];
        if (row.Cells["Id"]?.Value == null) return;
        var idObj = row.Cells?["Id"]?.Value;
        if (idObj == null) return;

        int personeliId = Convert.ToInt32(idObj);
        var emri = row.Cells?["Emri"]?.Value?.ToString() ?? "";
        var mbiemri = row.Cells?["Mbiemri"]?.Value?.ToString() ?? "";
        var grada = row.Cells?["Grada"]?.Value?.ToString() ?? "";
        var njesia = row.Cells?["Njesia"]?.Value?.ToString() ?? "";

        lblPersoneliDetaje.Text = $"\uD83D\uDC64 {emri} {mbiemri}  |  Grada: {grada}  |  Njesia: {njesia}";

        var transDt = new DataTable();
        transDt.Columns.Add("Data", typeof(string));
        transDt.Columns.Add("Tipi", typeof(string));
        transDt.Columns.Add("Armata", typeof(string));
        transDt.Columns.Add("Klienti", typeof(string));
        transDt.Columns.Add("Qellimi", typeof(string));

        try
        {
            foreach (DataRow r in _transaksionetTable.Rows)
            {
                if (Convert.ToInt32(r["PersoneliId"]) != personeliId) continue;
                var dataOra = r["DataOra"]?.ToString() ?? "";
                var data = dataOra.Length >= 10 ? dataOra.Replace("-", ".").Substring(0, 10) : dataOra;
                transDt.Rows.Add(
                    data,
                    r["Tipi"],
                    r["ArmaSerial"],
                    r["KlientiEmri"],
                    r["Qellimi"]
                );
            }
        }
        catch { System.Diagnostics.Debug.WriteLine("Gabim ne ngarkimin e detajeve te personelit"); }

        dgvPersoneliTrans.DataSource = transDt;
    }
}
