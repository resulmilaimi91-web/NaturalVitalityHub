using ArmepunesApp.Data;

namespace ArmepunesApp.Forms;

public partial class UpdateForm : Form
{
    private readonly UpdateHelper _updater;

    public UpdateForm(UpdateHelper updater)
    {
        _updater = updater;
        InitializeComponent();
        _ = KontrolloUpdate();
    }

    private async Task KontrolloUpdate()
    {
        lblStatus.Text = "Duke kontrolluar per azhornime...";
        btnUpdate.Enabled = false;

        var currentVer = _updater.GetCurrentVersion();

        // First check local update
        var localPath = _updater.CheckLocalUpdate(out var localVer);
        if (localPath != null && localVer != null && localVer > currentVer)
        {
            lblVersion.Text = $"Versioni aktual: {currentVer}  |  Lokal: {localVer}";
            lblStatus.Text = $"Azhornim lokal i gjetur: {localVer}";
            lblChangelog.Text = "Azhornim nga dosja lokale (publish).";
            btnUpdate.Text = $"📥 Azhorno ne {localVer} (Lokal)";
            btnUpdate.Enabled = true;
            Tag = localPath;
            return;
        }

        // Fall back to remote check
        var info = await _updater.CheckForUpdatesAsync();

        if (info == null)
        {
            lblStatus.Text = "Nuk ka azhornim lokal dhe nuk mund te kontaktohet serveri.";
            lblVersion.Text = $"Versioni aktual: {currentVer}";
            return;
        }

        var remoteVer = Version.TryParse(info.Version, out var v) ? v : currentVer;
        lblVersion.Text = $"Versioni aktual: {currentVer}  |  Remote: {remoteVer}";
        lblChangelog.Text = string.IsNullOrWhiteSpace(info.Changelog) ? "Pa detaje" : info.Changelog;

        if (remoteVer > currentVer)
        {
            lblStatus.Text = $"Azhornim remote i disponueshem: {info.Version}";
            btnUpdate.Text = $"📥 Azhorno ne {info.Version}";
            btnUpdate.Enabled = true;
            Tag = info;
        }
        else
        {
            lblStatus.Text = "Keni versionin me te fundit!";
        }
    }

    private async void btnUpdate_Click(object sender, EventArgs e)
    {
        if (MessageBox.Show("Azhornimi do te mbyll aplikacionin. Vazhdo?", "Konfirmim",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        btnUpdate.Enabled = false;
        progressBar.Style = ProgressBarStyle.Marquee;
        progressBar.Value = 0;
        lblStatus.Text = "Duke pergatitur azhornimin...";

        var destPath = Application.ExecutablePath;

        if (Tag is string localPath && File.Exists(localPath))
        {
            lblStatus.Text = "Duke azhornuar nga fajlli lokal...";
            await Task.Delay(500);
            UpdateHelper.SwapExe(localPath, destPath);
            Application.Exit();
            return;
        }

        if (Tag is UpdateInfo info)
        {
            lblStatus.Text = "Duke shkarkuar azhornimin...";
            progressBar.Style = ProgressBarStyle.Blocks;
            progressBar.Value = 0;

            var tmpPath = destPath + ".tmp";
            var progress = new Progress<int>(p =>
            {
                progressBar.Value = Math.Min(p, 100);
                lblStatus.Text = $"Duke shkarkuar... {p}%";
            });

            var result = await _updater.DownloadUpdateAsync(info, destPath, progress);
            if (result && File.Exists(tmpPath))
            {
                lblStatus.Text = "Azhornimi u shkarkua. Aplikacioni do te riniset...";
                UpdateHelper.SwapExe(tmpPath, destPath);
                Application.Exit();
            }
            else
            {
                lblStatus.Text = "Gabim gjate shkarkimit.";
                btnUpdate.Enabled = true;
            }
        }
    }

    private void btnMbyll_Click(object sender, EventArgs e) => Close();
}
