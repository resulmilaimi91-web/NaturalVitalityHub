namespace ArmepunesApp.Forms;

public partial class ShtoAksesorForm : Form
{
    public string Emri => txtEmri.Text.Trim();
    public int Sasia => (int)nudSasia.Value;
    public string Shenime => txtShenime.Text.Trim();

    public ShtoAksesorForm()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Text = "Shto Aksesor";
        Size = new Size(360, 200);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Color.FromArgb(30, 33, 40);
        ForeColor = Color.FromArgb(200, 205, 216);
        Font = new Font("Segoe UI", 9);

        var lblEmri = new Label { Text = "Emri:", Location = new Point(15, 15), Size = new Size(60, 22), ForeColor = Color.FromArgb(200, 205, 216) };
        txtEmri = new TextBox { Location = new Point(80, 13), Size = new Size(240, 24), BackColor = Color.FromArgb(40, 42, 48), ForeColor = Color.FromArgb(200, 205, 216), BorderStyle = BorderStyle.FixedSingle };

        var lblSasia = new Label { Text = "Sasia:", Location = new Point(15, 47), Size = new Size(60, 22), ForeColor = Color.FromArgb(200, 205, 216) };
        nudSasia = new NumericUpDown { Location = new Point(80, 45), Size = new Size(80, 24), Minimum = 1, Maximum = 999, Value = 1, BackColor = Color.FromArgb(40, 42, 48), ForeColor = Color.FromArgb(200, 205, 216) };

        var lblShenime = new Label { Text = "Shenime:", Location = new Point(15, 79), Size = new Size(60, 22), ForeColor = Color.FromArgb(200, 205, 216) };
        txtShenime = new TextBox { Location = new Point(80, 77), Size = new Size(240, 24), BackColor = Color.FromArgb(40, 42, 48), ForeColor = Color.FromArgb(200, 205, 216), BorderStyle = BorderStyle.FixedSingle };

        var btnOk = new Button { Text = "Shto", Location = new Point(140, 115), Size = new Size(90, 30), BackColor = Color.FromArgb(39, 174, 96), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
        btnOk.Click += (s, e) =>
        {
            if (string.IsNullOrWhiteSpace(txtEmri.Text)) { MessageBox.Show("Shkruaj emrin e aksesorit.", "Validim", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            DialogResult = DialogResult.OK;
            Close();
        };

        var btnAnulo = new Button { Text = "Anulo", Location = new Point(240, 115), Size = new Size(80, 30), BackColor = Color.FromArgb(60, 62, 68), ForeColor = Color.FromArgb(200, 205, 216), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
        btnAnulo.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

        Controls.Add(lblEmri); Controls.Add(txtEmri);
        Controls.Add(lblSasia); Controls.Add(nudSasia);
        Controls.Add(lblShenime); Controls.Add(txtShenime);
        Controls.Add(btnOk); Controls.Add(btnAnulo);
    }

    private TextBox txtEmri = null!;
    private NumericUpDown nudSasia = null!;
    private TextBox txtShenime = null!;
}
