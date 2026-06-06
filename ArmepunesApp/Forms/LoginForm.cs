using System.Text.Json;
using ArmepunesApp.Data;
using ArmepunesApp.Models;

namespace ArmepunesApp.Forms;

public partial class LoginForm : Form
{
    private readonly DatabaseHelper _db;
    private static readonly string _configPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ArmepunesApp", "login_config.json");
    public Perdoruesi? Perdoruesi { get; private set; }

    public LoginForm(DatabaseHelper db)
    {
        _db = db;
        InitializeComponent();
        NgarkoRememberMe();
    }

    private void NgarkoRememberMe()
    {
        try
        {
            if (!File.Exists(_configPath)) return;
            var json = File.ReadAllText(_configPath);
            var data = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            if (data != null && data.TryGetValue("username", out var u) && !string.IsNullOrEmpty(u))
            {
                txtUsername.Text = u;
                if (data.TryGetValue("password", out var p) && !string.IsNullOrEmpty(p))
                    txtPassword.Text = p;
                chkRemember.Checked = true;
            }
        }
        catch { }
    }

    private void RuajRememberMe()
    {
        try
        {
            var dir = Path.GetDirectoryName(_configPath)!;
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            var data = new Dictionary<string, string>
            {
                ["username"] = txtUsername.Text.Trim(),
                ["password"] = txtPassword.Text.Trim()
            };
            File.WriteAllText(_configPath, JsonSerializer.Serialize(data));
        }
        catch { }
    }

    private void LargoRememberMe()
    {
        try { if (File.Exists(_configPath)) File.Delete(_configPath); } catch { }
    }

    private void btnKycu_Click(object sender, EventArgs e)
    {
        var user = txtUsername.Text.Trim();
        var pass = txtPassword.Text.Trim();

        if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
        {
            MessageBox.Show("Plotesoni username dhe password!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_db.VerifikoPerdoruesin(user, pass, out var perdoruesi) && perdoruesi != null)
        {
            Perdoruesi = perdoruesi;
            if (chkRemember.Checked) RuajRememberMe();
            else LargoRememberMe();
            DialogResult = DialogResult.OK;
            Close();
        }
        else
        {
            MessageBox.Show("Username ose password i gabuar!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            txtPassword.Clear();
            txtPassword.Focus();
        }
    }

    private void btnDil_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }
}
