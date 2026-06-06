using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;

namespace ArmepunesApp.Data;

public class UpdateInfo
{
    public string Version { get; set; } = "1.0.0.0";
    public string DownloadUrl { get; set; } = "";
    public string Changelog { get; set; } = "";
    public string FileName { get; set; } = "ArmepunesApp.exe";
    public string Checksum { get; set; } = "";
}

public class UpdateHelper
{
    private readonly string? _updateUrl;

    public UpdateHelper(string? updateUrl)
    {
        _updateUrl = updateUrl;
    }

    public Version GetCurrentVersion()
    {
        var ver = Assembly.GetExecutingAssembly().GetName().Version;
        return ver ?? new Version(1, 0, 0, 0);
    }

    public string? CheckLocalUpdate(out Version? localVersion)
    {
        localVersion = null;
        try
        {
            var appDir = Path.GetDirectoryName(Application.ExecutablePath) ?? ".";
            var localExe = Path.Combine(appDir, "ArmepunesApp.exe.new");
            if (!File.Exists(localExe))
            {
                var publishDir = Path.Combine(appDir, "publish");
                localExe = Path.Combine(publishDir, "ArmepunesApp.exe");
                if (!File.Exists(localExe))
                    return null;
            }

            var ver = FileVersionInfo.GetVersionInfo(localExe);
            if (!string.IsNullOrEmpty(ver.FileVersion))
                localVersion = new Version(ver.FileVersion);
            else
                localVersion = new Version(2, 0, 0, 0);

            return localExe;
        }
        catch
        {
            return null;
        }
    }

    public async Task<UpdateInfo?> CheckForUpdatesAsync()
    {
        if (string.IsNullOrWhiteSpace(_updateUrl))
            return null;

        if (!_updateUrl.StartsWith("http://") && !_updateUrl.StartsWith("https://"))
        {
            try
            {
                if (!File.Exists(_updateUrl)) return null;
                var json = File.ReadAllText(_updateUrl);
                return JsonSerializer.Deserialize<UpdateInfo>(json);
            }
            catch { return null; }
        }

        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            var json = await http.GetStringAsync(_updateUrl);
            return JsonSerializer.Deserialize<UpdateInfo>(json);
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> DownloadUpdateAsync(UpdateInfo info, string destPath, IProgress<int> progress)
    {
        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
            using var response = await http.GetAsync(info.DownloadUrl, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            var total = response.Content.Headers.ContentLength ?? -1;
            using var stream = await response.Content.ReadAsStreamAsync();
            using var fileStream = new FileStream(destPath + ".tmp", FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

            var buffer = new byte[8192];
            long read = 0;
            int bytesRead;
            while ((bytesRead = await stream.ReadAsync(buffer)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                read += bytesRead;
                if (total > 0)
                    progress?.Report((int)(read * 100 / total));
            }

            await fileStream.FlushAsync();
            fileStream.Close();

            if (File.Exists(destPath + ".tmp"))
                return true;
            return false;
        }
        catch
        {
            if (File.Exists(destPath + ".tmp"))
                try { File.Delete(destPath + ".tmp"); } catch { }
            return false;
        }
    }

    public static void SwapExe(string newExePath, string currentExePath)
    {
        var scriptPath = Path.Combine(Path.GetTempPath(), "update_armepunes.bat");
        var scriptContent = $@"@echo off
timeout /t 2 /nobreak >nul
:retry
ren ""{currentExePath}"" ""{Path.GetFileName(currentExePath)}.old"" 2>nul
copy /y ""{newExePath}"" ""{currentExePath}"" >nul
if exist ""{currentExePath}"" (
    del ""{newExePath}"" >nul 2>nul
    del ""{currentExePath}.old"" >nul 2>nul
    start """" ""{currentExePath}""
    exit /b 0
)
timeout /t 1 /nobreak >nul
goto retry
";
        File.WriteAllText(scriptPath, scriptContent);
        Process.Start(new ProcessStartInfo
        {
            FileName = scriptPath,
            WindowStyle = ProcessWindowStyle.Hidden,
            CreateNoWindow = true,
            UseShellExecute = false
        });
    }
}
