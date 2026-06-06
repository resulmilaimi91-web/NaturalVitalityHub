using System.Text;
using System.Text.Json;
using SocialBot.Models;

namespace SocialBot.Services;

public class ContentModerator
{
    private readonly HttpClient _http;
    private readonly ILogger<ContentModerator>? _logger;
    private static readonly string[] ProhibitedPatterns =
    {
        "like if", "share if", "comment if", "tag someone who",
        "ignore if", "scroll past", "only real fans",
        "share this post", "must watch", "you won't believe",
        "this is not a drill", "breaking:", "urgent:",
        "send this to", "copy and paste",
        "follow for follow", "like for like",
        "click here", "link in bio", "buy now",
        "limited time offer", "act now"
    };

    public ContentModerator(ILogger<ContentModerator>? logger = null)
    {
        _http = new HttpClient();
        _logger = logger;
    }

    public ModerationResult Analyze(string caption, List<string>? hashtags = null)
    {
        var issues = new List<string>();
        var lower = caption.ToLowerInvariant();

        // Check engagement bait patterns
        foreach (var pattern in ProhibitedPatterns)
        {
            if (lower.Contains(pattern))
                issues.Add($"Perdor 'engagement bait': '{pattern}'");
        }

        // Check excessive hashtags
        if (hashtags != null && hashtags.Count > 15)
            issues.Add($"Hashtags te shumta ({hashtags.Count}). Max 10 rekomandohet.");

        // Check ALL CAPS (shouting)
        var words = caption.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var upperWords = words.Count(w => w.Length > 2 && w.All(char.IsUpper));
        if (upperWords > words.Length * 0.3 && words.Length > 5)
            issues.Add("Perdorim i tepert i shkronjave kapitale (shouting)");

        // Check URL shorteners
        if (lower.Contains("bit.ly") || lower.Contains("tinyurl") || lower.Contains("goo.gl"))
            issues.Add("Perdor URL shorteners qe mund te konsiderohen spam");

        // Check excessive emojis
        var emojiCount = caption.Count(c => c > 0x1F600 && c <= 0x1F9FF || c == '\u2764' || c == '\u2600');
        if (emojiCount > 10)
            issues.Add($"Perdorim i tepert i emojive ({emojiCount}). Max 5-7 rekomandohet.");

        return new ModerationResult
        {
            IsClean = issues.Count == 0,
            Issues = issues,
            Score = Math.Max(0, 100 - issues.Count * 25)
        };
    }

    public async Task<ModerationResult> ModerateWithAI(string caption, string? openAiKey = null)
    {
        if (string.IsNullOrEmpty(openAiKey))
            return Analyze(caption);

        try
        {
            var prompt = $@"You are a Meta Community Guidelines compliance checker. 
Analyze this social media post and check for violations:

Post: ""{caption}""

Check for:
1. Hate speech or discrimination
2. Violence or harmful content
3. Misinformation or false claims
4. Engagement bait
5. Spam or deceptive practices
6. Copyright violations
7. Prohibited regulated goods

Return JSON ONLY:
{{
    ""isCompliant"": true/false,
    ""issues"": [""issue1"", ""issue2""],
    ""riskLevel"": ""low""|""medium""|""high"",
    ""suggestion"": ""how to fix""
}}";

            var body = new
            {
                model = "gpt-4o-mini",
                messages = new[]
                {
                    new { role = "system", content = "You are a Meta content moderation expert." },
                    new { role = "user", content = prompt }
                },
                temperature = 0.1,
                response_format = new { type = "json_object" }
            };

            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            _http.DefaultRequestHeaders.Authorization = new("Bearer", openAiKey);

            var resp = await _http.PostAsync("https://api.openai.com/v1/chat/completions", content);
            var respBody = await resp.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(respBody);
            var text = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
            using var result = JsonDocument.Parse(text!);

            var issues = new List<string>();
            foreach (var issue in result.RootElement.GetProperty("issues").EnumerateArray())
                issues.Add(issue.GetString() ?? "");

            return new ModerationResult
            {
                IsClean = result.RootElement.GetProperty("isCompliant").GetBoolean(),
                Issues = issues,
                RiskLevel = result.RootElement.GetProperty("riskLevel").GetString() ?? "low",
                Suggestion = result.RootElement.TryGetProperty("suggestion", out var s) ? s.GetString() : null,
                Score = result.RootElement.GetProperty("isCompliant").GetBoolean() ? 100 : 30
            };
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "AI moderation fallback to local rules");
            return Analyze(caption);
        }
    }
}

public class ModerationResult
{
    public bool IsClean { get; set; }
    public List<string> Issues { get; set; } = new();
    public string RiskLevel { get; set; } = "low";
    public string? Suggestion { get; set; }
    public int Score { get; set; } = 100;
}
