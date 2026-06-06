namespace ArmepunesApp.Forms;

partial class PasswordDialog
{
    private System.ComponentModel.IContainer components = null;
    private Label lblVeprimi;
    private TextBox txtUsername;
    private TextBox txtPassword;
    private Button btnAutorizo;
    private Button btnAnulo;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        lblVeprimi = new Label();
        txtUsername = new TextBox();
        txtPassword = new TextBox();
        btnAutorizo = new Button();
        btnAnulo = new Button();

        SuspendLayout();

        lblVeprimi.Text = "";
        lblVeprimi.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        lblVeprimi.ForeColor = Color.FromArgb(200, 205, 216);
        lblVeprimi.Size = new Size(350, 24);
        lblVeprimi.Location = new Point(20, 15);

        var lblUser = new Label();
        lblUser.Text = "Username:";
        lblUser.Font = new Font("Segoe UI", 9);
        lblUser.ForeColor = Color.FromArgb(200, 205, 216);
        lblUser.Size = new Size(100, 22);
        lblUser.Location = new Point(20, 50);

        txtUsername.Font = new Font("Segoe UI", 10);
        txtUsername.Size = new Size(250, 26);
        txtUsername.Location = new Point(20, 75);
        txtUsername.BackColor = Color.FromArgb(40, 42, 48);
        txtUsername.ForeColor = Color.FromArgb(200, 205, 216);
        txtUsername.BorderStyle = BorderStyle.FixedSingle;

        var lblPass = new Label();
        lblPass.Text = "Password:";
        lblPass.Font = new Font("Segoe UI", 9);
        lblPass.ForeColor = Color.FromArgb(200, 205, 216);
        lblPass.Size = new Size(100, 22);
        lblPass.Location = new Point(20, 110);

        txtPassword.Font = new Font("Segoe UI", 10);
        txtPassword.Size = new Size(250, 26);
        txtPassword.Location = new Point(20, 135);
        txtPassword.BackColor = Color.FromArgb(40, 42, 48);
        txtPassword.ForeColor = Color.FromArgb(200, 205, 216);
        txtPassword.BorderStyle = BorderStyle.FixedSingle;
        txtPassword.UseSystemPasswordChar = true;

        btnAutorizo.Text = "Autorizo";
        btnAutorizo.Size = new Size(120, 38);
        btnAutorizo.Location = new Point(20, 180);
        btnAutorizo.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        btnAutorizo.BackColor = Color.FromArgb(0, 140, 200);
        btnAutorizo.ForeColor = Color.White;
        btnAutorizo.FlatStyle = FlatStyle.Flat;
        btnAutorizo.FlatAppearance.BorderColor = Color.FromArgb(0, 160, 220);
        btnAutorizo.Cursor = Cursors.Hand;
        btnAutorizo.Click += btnAutorizo_Click;

        btnAnulo.Text = "Anulo";
        btnAnulo.Size = new Size(120, 38);
        btnAnulo.Location = new Point(150, 180);
        btnAnulo.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        btnAnulo.BackColor = Color.FromArgb(60, 62, 68);
        btnAnulo.ForeColor = Color.FromArgb(200, 205, 216);
        btnAnulo.FlatStyle = FlatStyle.Flat;
        btnAnulo.FlatAppearance.BorderColor = Color.FromArgb(80, 82, 88);
        btnAnulo.Cursor = Cursors.Hand;
        btnAnulo.Click += btnAnulo_Click;

        BackColor = Color.FromArgb(30, 30, 35);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(300, 240);
        Text = "Autorizim Admin";
        Controls.Add(lblVeprimi);
        Controls.Add(lblUser);
        Controls.Add(txtUsername);
        Controls.Add(lblPass);
        Controls.Add(txtPassword);
        Controls.Add(btnAutorizo);
        Controls.Add(btnAnulo);
        ResumeLayout(false);
        PerformLayout();
    }
}
