namespace SocialBot.Models;

public class MetaConfig
{
    public string AppId { get; set; } = "";
    public string AppSecret { get; set; } = "";
    public string UserAccessToken { get; set; } = "";
    public string? PageId { get; set; }
    public string? PageAccessToken { get; set; }
    public string? InstagramId { get; set; }
    public long TokenExpiresAt { get; set; }
    public List<SavedPage> Pages { get; set; } = new();

    public static string FilePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "SocialBot", "meta_config.json");

    public void Save()
    {
        var dir = Path.GetDirectoryName(FilePath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        File.WriteAllText(FilePath,
            System.Text.Json.JsonSerializer.Serialize(this, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
    }

    public static MetaConfig Load()
    {
        try
        {
            if (File.Exists(FilePath))
                return System.Text.Json.JsonSerializer.Deserialize<MetaConfig>(File.ReadAllText(FilePath)) ?? new();
        }
        catch { }
        return new();
    }
}

public class SavedPage
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string AccessToken { get; set; } = "";
    public bool IsInstagram { get; set; }
}
