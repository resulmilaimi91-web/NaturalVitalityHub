using ArmepunesApp.Data;
using ArmepunesApp.Forms;

namespace ArmepunesApp;

static class Program
{
    [STAThread]
    static void Main()
    {
        Application.ThreadException += (s, e) =>
        {
            MessageBox.Show($"Gabim i papritur:\n{e.Exception.Message}\n\n{e.Exception.StackTrace}",
                "Gabim Aplikacioni", MessageBoxButtons.OK, MessageBoxIcon.Error);
        };

        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            if (e.ExceptionObject is Exception ex)
            {
                MessageBox.Show($"Gabim kritik:\n{ex.Message}\n\n{ex.StackTrace}",
                    "Gabim Kritik", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        };

        try
        {
            ApplicationConfiguration.Initialize();

            string appData = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ArmepunesApp");

            if (!Directory.Exists(appData))
                Directory.CreateDirectory(appData);

            string dbPath = Path.Combine(appData, "ArmepunesDB.sqlite");
            var db = new DatabaseHelper(dbPath);

            using var loginForm = new LoginForm(db);
            if (loginForm.ShowDialog() != DialogResult.OK)
                return;

            Application.Run(new MainForm(db, loginForm.Perdoruesi!));
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Gabim gjate nisjes: {ex.Message}\n\nStack: {ex.StackTrace}",
                "Gabim Fillestar", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
