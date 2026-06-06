using ArmepunesApp.Data;

namespace ArmepunesApp.Forms;

public partial class LejetForm : Form
{
    private readonly DatabaseHelper _db;
    private readonly int _userId;
    private readonly string _username;
    private readonly CheckedListBox _clb;

    public LejetForm(DatabaseHelper db, int userId, string username)
    {
        _db = db;
        _userId = userId;
        _username = username;
        Size = new Size(420, 500);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(35, 38, 45);
        ForeColor = Color.FromArgb(200, 205, 216);
        Font = new Font("Segoe UI", 9);
        Text = $"Lejet - {username}";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        var lbl = new Label
        {
            Text = $"Menaxho lejet per: {username}",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = Color.FromArgb(46, 204, 113),
            Location = new Point(16, 14),
            Size = new Size(380, 26)
        };

        var lblInfo = new Label
        {
            Text = "Zgjedh lejet qe do ti jepen ketij perdoruesi:",
            Location = new Point(16, 44),
            Size = new Size(380, 18),
            ForeColor = Color.FromArgb(160, 168, 180)
        };

        _clb = new CheckedListBox
        {
            Location = new Point(16, 66),
            Size = new Size(370, 330),
            BackColor = Color.FromArgb(30, 32, 37),
            ForeColor = Color.FromArgb(200, 205, 216),
            Font = new Font("Segoe UI", 10),
            CheckOnClick = true,
            BorderStyle = BorderStyle.FixedSingle
        };

        var existing = _db.MerrLejet(userId);
        foreach (var p in _db.MerrTeGjithaLejet())
            _clb.Items.Add(p, existing.Contains(p));

        var btnRuaj = new Button
        {
            Text = "Ruaj",
            Location = new Point(200, 410),
            Size = new Size(90, 32),
            BackColor = Color.FromArgb(39, 174, 96),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 9, FontStyle.Bold)
        };
        btnRuaj.Click += (_, _) => { Ruaj(); DialogResult = DialogResult.OK; Close(); };

        var btnAnulo = new Button
        {
            Text = "Anulo",
            Location = new Point(300, 410),
            Size = new Size(90, 32),
            BackColor = Color.FromArgb(80, 85, 95),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 9, FontStyle.Bold)
        };
        btnAnulo.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };

        Controls.Add(lbl);
        Controls.Add(lblInfo);
        Controls.Add(_clb);
        Controls.Add(btnRuaj);
        Controls.Add(btnAnulo);
    }

    private void Ruaj()
    {
        var allPerms = _db.MerrTeGjithaLejet();
        for (int i = 0; i < allPerms.Length; i++)
        {
            if (_clb.GetItemChecked(i))
                _db.ShtoLeje(_userId, allPerms[i]);
            else
                _db.LargoLeje(_userId, allPerms[i]);
        }
    }
}
