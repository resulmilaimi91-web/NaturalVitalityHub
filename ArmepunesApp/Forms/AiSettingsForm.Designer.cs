namespace ArmepunesApp.Forms;

partial class AiSettingsForm
{
    private System.ComponentModel.IContainer components = null;
    private Button btnSave;
    private Button btnCancel;
    private TextBox txtEndpoint;
    private TextBox txtModel;
    private Label lblEndpoint;
    private Label lblModel;
    private Label lblInfo;
    private LinkLabel lblTest;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.Text = "Cilësimet e AI";
        this.Size = new Size(480, 260);
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.BackColor = Color.FromArgb(30, 32, 37);
        this.ForeColor = Color.FromArgb(200, 205, 216);

        lblInfo = new Label();
        lblInfo.Text = "Konfiguro lidhjen me AI (Ollama) per gjenerimin automatik te fleteleshimeve.";
        lblInfo.Font = new Font("Segoe UI", 9);
        lblInfo.Size = new Size(440, 36);
        lblInfo.Location = new Point(20, 15);
        lblInfo.ForeColor = Color.FromArgb(150, 155, 165);

        lblEndpoint = new Label();
        lblEndpoint.Text = "Ollama Endpoint:";
        lblEndpoint.Font = new Font("Segoe UI", 9);
        lblEndpoint.Size = new Size(120, 22);
        lblEndpoint.Location = new Point(20, 65);

        txtEndpoint = new TextBox();
        txtEndpoint.Font = new Font("Segoe UI", 9);
        txtEndpoint.Size = new Size(310, 24);
        txtEndpoint.Location = new Point(140, 63);
        txtEndpoint.BackColor = Color.FromArgb(40, 42, 48);
        txtEndpoint.ForeColor = Color.FromArgb(200, 205, 216);
        txtEndpoint.BorderStyle = BorderStyle.FixedSingle;

        lblModel = new Label();
        lblModel.Text = "Modeli:";
        lblModel.Font = new Font("Segoe UI", 9);
        lblModel.Size = new Size(120, 22);
        lblModel.Location = new Point(20, 100);

        txtModel = new TextBox();
        txtModel.Font = new Font("Segoe UI", 9);
        txtModel.Size = new Size(310, 24);
        txtModel.Location = new Point(140, 98);
        txtModel.BackColor = Color.FromArgb(40, 42, 48);
        txtModel.ForeColor = Color.FromArgb(200, 205, 216);
        txtModel.BorderStyle = BorderStyle.FixedSingle;

        lblTest = new LinkLabel();
        lblTest.Text = "Testo lidhjen";
        lblTest.Font = new Font("Segoe UI", 9);
        lblTest.Size = new Size(100, 22);
        lblTest.Location = new Point(460, 65);
        lblTest.ForeColor = Color.FromArgb(0, 200, 255);
        lblTest.LinkColor = Color.FromArgb(0, 200, 255);
        lblTest.ActiveLinkColor = Color.White;
        lblTest.Click += btnTest_Click;

        btnSave = new Button();
        btnSave.Text = "Ruaj";
        btnSave.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        btnSave.Size = new Size(100, 36);
        btnSave.Location = new Point(260, 170);
        btnSave.BackColor = Color.FromArgb(39, 174, 96);
        btnSave.ForeColor = Color.White;
        btnSave.FlatStyle = FlatStyle.Flat;
        btnSave.Cursor = Cursors.Hand;
        btnSave.Click += btnSave_Click;

        btnCancel = new Button();
        btnCancel.Text = "Anulo";
        btnCancel.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        btnCancel.Size = new Size(100, 36);
        btnCancel.Location = new Point(140, 170);
        btnCancel.BackColor = Color.FromArgb(60, 62, 68);
        btnCancel.ForeColor = Color.FromArgb(200, 205, 216);
        btnCancel.FlatStyle = FlatStyle.Flat;
        btnCancel.Cursor = Cursors.Hand;
        btnCancel.Click += (s, e) => this.Close();

        this.Controls.Add(lblInfo);
        this.Controls.Add(lblEndpoint);
        this.Controls.Add(txtEndpoint);
        this.Controls.Add(lblModel);
        this.Controls.Add(txtModel);
        this.Controls.Add(lblTest);
        this.Controls.Add(btnSave);
        this.Controls.Add(btnCancel);
    }
}
