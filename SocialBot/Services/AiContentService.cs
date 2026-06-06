using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using SocialBot.Models;

namespace SocialBot.Services;

public class AiContentService
{
    private readonly HttpClient _http;
    private readonly ILogger<AiContentService>? _logger;

    public AiContentService(ILogger<AiContentService>? logger = null)
    {
        _http = new HttpClient();
        _http.Timeout = TimeSpan.FromSeconds(60);
        _logger = logger;
    }

    public async Task<AiSuggestion> GeneratePostContent(string? imageUrl = null, string? topic = null, string? tone = "professional")
    {
        var cfg = AiSettings.Load();
        var prompt = BuildPrompt(topic, tone);

        // If imageUrl is a local file path, convert to base64
        string? processedImageUrl = imageUrl;
        if (imageUrl != null && File.Exists(imageUrl))
        {
            var base64 = Convert.ToBase64String(await File.ReadAllBytesAsync(imageUrl));
            var ext = Path.GetExtension(imageUrl).ToLowerInvariant();
            var mime = ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                _ => "image/jpeg"
            };
            processedImageUrl = $"data:{mime};base64,{base64}";
        }

        if (cfg.Provider == "openai" || (!string.IsNullOrEmpty(cfg.OpenAiKey) && cfg.Provider != "gemini"))
            return await CallOpenAi(cfg, prompt, processedImageUrl);
        else if (cfg.Provider == "gemini" && !string.IsNullOrEmpty(cfg.GeminiKey))
            return await CallGemini(cfg, prompt, processedImageUrl);
        else if (cfg.Provider == "ollama")
            return await CallOllama(cfg, prompt);
        else
            // Default fallback - if no API keys configured, return a basic suggestion
            return new AiSuggestion
            {
                Caption = "Ju lutem konfiguroni nje provider AI (OpenAI, Gemini, ose Ollama) ne Settings > AI Configuration",
                Hashtags = new List<string> { "#socialbot", "#metamanager" },
                EngagementTip = "Konfiguro AI Settings per gjenerim automatik"
            };
    }

    private string BuildPrompt(string? topic, string? tone)
    {
        var toneDesc = tone switch
        {
            "professional" => "profesional dhe zyrtar",
            "casual" => "te relaksuar dhe miqesor",
            "humorous" => "humoristik dhe argetues",
            "inspirational" => "inspirues dhe motivues",
            "promotional" => "promovues dhe bindes",
            _ => "profesional"
        };

        return $@"You are a social media content creator for Facebook and Instagram fanpages. 
Your task is to create engaging, platform-optimized content that drives engagement (likes, comments, shares) 
while strictly following Meta's Community Guidelines to avoid penalties.

Create content with a {toneDesc} tone.
{(topic != null ? $"Topic/theme: {topic}" : "Based on the image provided, create relevant content.")}

IMPORTANT RULES for Meta compliance:
- NO prohibited content: hate speech, violence, discrimination, false news, spam
- NO engagement bait (like 'LIKE if you agree' or 'SHARE this')
- NO misleading claims or clickbait
- Keep hashtags relevant (max 8-10 hashtags, do not overstuff)
- Natural language, not robotic
- Include a call-to-action that complies with Meta policies (e.g., ask a question, invite opinions)

Return ONLY valid JSON in this exact format (no markdown, no code blocks):
{{
    ""caption"": ""the main post text (2-4 sentences, max 300 chars)"",
    ""hashtags"": [""#tag1"", ""#tag2"", ""#tag3""],
    ""bestTime"": ""best posting time suggestion"",
    ""engagementTip"": ""one tip to improve engagement for this post""
}}";
    }

    private async Task<AiSuggestion> CallOpenAi(AiSettings cfg, string prompt, string? imageUrl)
    {
        try
        {
            var messages = new List<object>
            {
                new { role = "system", content = "You are an expert social media content creator for Meta platforms." }
            };

            if (!string.IsNullOrEmpty(imageUrl))
            {
                messages.Add(new
                {
                    role = "user",
                    content = new object[]
                    {
                        new { type = "text", text = prompt },
                        new { type = "image_url", image_url = new { url = imageUrl, detail = "low" } }
                    }
                });
            }
            else
            {
                messages.Add(new { role = "user", content = prompt });
            }

            var body = new
            {
                model = cfg.OpenAiModel ?? "gpt-4o-mini",
                messages = messages,
                temperature = 0.7,
                max_tokens = 600,
                response_format = new { type = "json_object" }
            };

            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", cfg.OpenAiKey);

            var resp = await _http.PostAsync("https://api.openai.com/v1/chat/completions", content);
            var respBody = await resp.Content.ReadAsStringAsync();
            _logger?.LogInformation("OpenAI response: {Body}", respBody);

            using var doc = JsonDocument.Parse(respBody);
            var text = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
            return ParseSuggestion(text);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "OpenAI call failed");
            return new AiSuggestion { Caption = $"Gabim AI: {ex.Message}", Hashtags = new() };
        }
    }

    private async Task<AiSuggestion> CallGemini(AiSettings cfg, string prompt, string? imageUrl)
    {
        try
        {
            var contents = new List<object>();

            if (!string.IsNullOrEmpty(imageUrl))
            {
                var imageData = await DownloadImageAsBase64(imageUrl);
                if (imageData != null)
                {
                    contents.Add(new
                    {
                        parts = new object[]
                        {
                            new { text = prompt },
                            new { inline_data = new { mime_type = imageData.MimeType, data = imageData.Base64 } }
                        }
                    });
                }
            }

            if (contents.Count == 0)
            {
                contents.Add(new { parts = new[] { new { text = prompt } } });
            }

            var body = new
            {
                contents = contents,
                generationConfig = new
                {
                    temperature = 0.7,
                    maxOutputTokens = 600,
                    response_mime_type = "application/json"
                }
            };

            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{cfg.GeminiModel ?? "gemini-2.0-flash"}:generateContent?key={cfg.GeminiKey}";
            var resp = await _http.PostAsync(url, content);
            var respBody = await resp.Content.ReadAsStringAsync();
            _logger?.LogInformation("Gemini response: {Body}", respBody);

            using var doc = JsonDocument.Parse(respBody);
            var text = doc.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();
            return ParseSuggestion(text);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Gemini call failed");
            return new AiSuggestion { Caption = $"Gabim AI: {ex.Message}", Hashtags = new() };
        }
    }

    private async Task<AiSuggestion> CallOllama(AiSettings cfg, string prompt)
    {
        try
        {
            var body = new
            {
                model = cfg.OllamaModel ?? "llama3",
                stream = false,
                format = "json",
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var endpoint = cfg.OllamaEndpoint?.TrimEnd('/') ?? "http://localhost:11434";
            var resp = await _http.PostAsync($"{endpoint}/api/chat", content);
            var respBody = await resp.Content.ReadAsStringAsync();
            _logger?.LogInformation("Ollama response: {Body}", respBody);

            using var doc = JsonDocument.Parse(respBody);
            var text = doc.RootElement.GetProperty("message").GetProperty("content").GetString();
            return ParseSuggestion(text);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Ollama call failed");
            return new AiSuggestion { Caption = $"Gabim AI: {ex.Message}", Hashtags = new() };
        }
    }

    private AiSuggestion ParseSuggestion(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return new AiSuggestion { Caption = "Nuk u krijua content", Hashtags = new() };

        try
        {
            // Clean markdown code blocks if present
            if (text.StartsWith("```"))
            {
                var start = text.IndexOf('\n') + 1;
                var end = text.LastIndexOf("```");
                if (start > 0 && end > start)
                    text = text[start..end].Trim();
            }

            using var doc = JsonDocument.Parse(text);
            var root = doc.RootElement;

            var hashtags = new List<string>();
            if (root.TryGetProperty("hashtags", out var tags))
            {
                foreach (var tag in tags.EnumerateArray())
                {
                    var h = tag.GetString() ?? "";
                    if (!string.IsNullOrEmpty(h) && !hashtags.Contains(h))
                        hashtags.Add(h);
                }
            }

            return new AiSuggestion
            {
                Caption = root.TryGetProperty("caption", out var cap) ? cap.GetString() ?? "" : "",
                Hashtags = hashtags,
                BestTime = root.TryGetProperty("bestTime", out var bt) ? bt.GetString() : null,
                EngagementTip = root.TryGetProperty("engagementTip", out var et) ? et.GetString() : null
            };
        }
        catch (JsonException ex)
        {
            _logger?.LogWarning(ex, "Failed to parse AI response as JSON. Raw: {Text}", text);
            // Fallback: treat entire response as caption
            return new AiSuggestion { Caption = text.Trim(), Hashtags = new() };
        }
    }

    private async Task<ImageData?> DownloadImageAsBase64(string url)
    {
        try
        {
            // Handle data URLs (base64 already encoded)
            if (url.StartsWith("data:"))
            {
                var parts = url.Substring(5).Split(';');
                var mime = parts[0];
                var base64 = parts.Length > 1 ? url.Substring(url.IndexOf(',') + 1) : "";
                return new ImageData { Base64 = base64, MimeType = mime };
            }

            var resp = await _http.GetAsync(url);
            if (!resp.IsSuccessStatusCode) return null;
            var bytes = await resp.Content.ReadAsByteArrayAsync();
            var mime2 = resp.Content.Headers.ContentType?.MediaType ?? "image/jpeg";
            return new ImageData
            {
                Base64 = Convert.ToBase64String(bytes),
                MimeType = mime2
            };
        }
        catch { return null; }
    }

    private class ImageData
    {
        public string Base64 { get; set; } = "";
        public string MimeType { get; set; } = "image/jpeg";
    }
}

public class AiSuggestion
{
    public string Caption { get; set; } = "";
    public List<string> Hashtags { get; set; } = new();
    public string? BestTime { get; set; }
    public string? EngagementTip { get; set; }
    public string HashtagString => string.Join(" ", Hashtags);
}
