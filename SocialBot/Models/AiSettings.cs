using System.Text.Json;

namespace SocialBot.Models;

public class AiSettings
{
    public string Provider { get; set; } = "openai";
    public string OpenAiKey { get; set; } = "";
    public string OpenAiModel { get; set; } = "gpt-4o-mini";
    public string GeminiKey { get; set; } = "";
    public string GeminiModel { get; set; } = "gemini-2.0-flash";
    public string OllamaEndpoint { get; set; } = "http://localhost:11434";
    public string OllamaModel { get; set; } = "llama3.2-vision";
    public bool ModerationEnabled { get; set; } = true;

    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "SocialBot", "ai_settings.json");

    public void Save()
    {
        var dir = Path.GetDirectoryName(SettingsPath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        File.WriteAllText(SettingsPath,
            JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
    }

    public static AiSettings Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
                return JsonSerializer.Deserialize<AiSettings>(File.ReadAllText(SettingsPath)) ?? new();
        }
        catch { }
        return new();
    }
}
