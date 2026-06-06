using SocialBot.Models;
using SocialBot.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddWindowsService();
builder.Services.AddSingleton<MetaGraphService>();
builder.Services.AddSingleton<PostScheduler>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<PostScheduler>());
builder.Services.AddSingleton<AiContentService>();
builder.Services.AddSingleton<ContentModerator>();

// Increase max body size for file uploads
builder.WebHost.ConfigureKestrel(o => o.Limits.MaxRequestBodySize = 50 * 1024 * 1024);

var app = builder.Build();
app.UseStaticFiles();

var meta = app.Services.GetRequiredService<MetaGraphService>();
var scheduler = app.Services.GetRequiredService<PostScheduler>();
var ai = app.Services.GetRequiredService<AiContentService>();
var moderator = app.Services.GetRequiredService<ContentModerator>();

// --- Auth APIs ---
app.MapGet("/api/auth/url", (string appId, string redirectUri) =>
{
    var state = Guid.NewGuid().ToString("N");
    var url = meta.GetOAuthUrl(appId, redirectUri, state);
    return Results.Ok(new { url, state });
});

app.MapPost("/api/auth/token", async (string code, string redirectUri) =>
{
    var cfg = MetaConfig.Load();
    if (string.IsNullOrEmpty(cfg.AppId) || string.IsNullOrEmpty(cfg.AppSecret))
        return Results.BadRequest(new { error = "Konfiguro App ID dhe App Secret ne Settings" });
    var token = await meta.ExchangeCodeForToken(cfg.AppId, cfg.AppSecret, code, redirectUri);
    if (token == null)
        return Results.BadRequest(new { error = "Kembimi i token deshtoi" });
    cfg.UserAccessToken = token;
    cfg.Save();
    return Results.Ok(new { token });
});

app.MapGet("/auth/callback", async (HttpRequest request, string? code, string? error) =>
{
    if (!string.IsNullOrEmpty(error))
        return Results.Redirect($"/?error={Uri.EscapeDataString(error)}");
    if (!string.IsNullOrEmpty(code))
    {
        var cfg = MetaConfig.Load();
        var redirectUri = $"{request.Scheme}://{request.Host}/auth/callback";
        var token = await meta.ExchangeCodeForToken(cfg.AppId, cfg.AppSecret, code, redirectUri);
        if (token != null)
        {
            cfg.UserAccessToken = token;
            cfg.Save();
            return Results.Redirect("/?login=success");
        }
        return Results.Redirect("/?error=token_exchange_failed");
    }
    return Results.Redirect("/?error=Pa_kode_autorizimi");
});

app.MapPost("/api/auth/set-credentials", (string appId, string appSecret) =>
{
    var cfg = MetaConfig.Load();
    cfg.AppId = appId;
    cfg.AppSecret = appSecret;
    cfg.Save();
    return Results.Ok(new { message = "Kredencialet u ruajten" });
});

// --- Pages APIs ---
app.MapGet("/api/pages", () =>
{
    var cfg = MetaConfig.Load();
    if (string.IsNullOrEmpty(cfg.UserAccessToken))
        return Results.Ok(new { pages = new List<FanPage>(), error = "Nuk je i lidhur. Shko te Settings > Lidhu me Facebook." });
    var (pages, error) = meta.GetUserPages(cfg.UserAccessToken).Result;
    if (error != null)
        return Results.Ok(new { pages, error });

    // Check if user needs to add product or switch to Live
    if (pages.Count == 0)
        return Results.Ok(new { pages, error = "Nuk u gjet asnje faqe. Shkaqe te mundshme:\n1. App-i yt eshte ne 'Development Mode' - ndrroje ne 'Live' (Settings > Basic > App Mode)\n2. Nuk ke shtuar produktin 'Facebook Login' tek app-i yt\n3. Nuk ke asnje faqe (Fanpage) ne kete llogari" });

        return Results.Ok(new { pages, error = (string?)null });
});

app.MapGet("/api/debug", async () =>
{
    var cfg = MetaConfig.Load();
    var debugInfo = new Dictionary<string, object?>
    {
        ["hasAppId"] = !string.IsNullOrEmpty(cfg.AppId),
        ["hasAppSecret"] = !string.IsNullOrEmpty(cfg.AppSecret),
        ["hasToken"] = !string.IsNullOrEmpty(cfg.UserAccessToken),
        ["tokenPreview"] = cfg.UserAccessToken?.Length > 20 ? cfg.UserAccessToken.Substring(0, 20) + "..." : null,
        ["pageId"] = cfg.PageId,
        ["instagramId"] = cfg.InstagramId,
        ["savedPages"] = cfg.Pages.Count
    };

    if (!string.IsNullOrEmpty(cfg.UserAccessToken))
    {
        try
        {
            var http = new HttpClient();
            var debugUrl = $"https://graph.facebook.com/v21.0/debug_token?input_token={cfg.UserAccessToken}&access_token={cfg.UserAccessToken}";
            var resp = await http.GetStringAsync(debugUrl);
            debugInfo["tokenDebug"] = resp;
        }
        catch (Exception ex)
        {
            debugInfo["tokenDebugError"] = ex.Message;
        }

        try
        {
            var http2 = new HttpClient();
            var meUrl = $"https://graph.facebook.com/v21.0/me?access_token={cfg.UserAccessToken}&fields=id,name,email";
            var resp2 = await http2.GetStringAsync(meUrl);
            debugInfo["me"] = resp2;
        }
        catch (Exception ex)
        {
            debugInfo["meError"] = ex.Message;
        }

        try
        {
            var http3 = new HttpClient();
            var pagesUrl = $"https://graph.facebook.com/v21.0/me/accounts?access_token={cfg.UserAccessToken}&fields=id,name";
            var resp3 = await http3.GetStringAsync(pagesUrl);
            debugInfo["pagesRaw"] = resp3;
        }
        catch (Exception ex)
        {
            debugInfo["pagesError"] = ex.Message;
        }
    }

    return Results.Ok(debugInfo);
});

app.MapPost("/api/pages/select", async (string pageId, string pageToken, string? pageName) =>
{
    var cfg = MetaConfig.Load();
    cfg.PageId = pageId;
    cfg.PageAccessToken = pageToken;

    if (!cfg.Pages.Any(p => p.Id == pageId))
    {
        cfg.Pages.Add(new SavedPage
        {
            Id = pageId,
            Name = pageName ?? pageId,
            AccessToken = pageToken
        });
    }

    var instaId = await meta.GetInstagramId(pageId, pageToken);
    cfg.InstagramId = instaId;
    cfg.Save();
    return Results.Ok(new { message = "Faqja u zgjodh", instagramId = instaId });
});

// --- Posts APIs ---
app.MapPost("/api/posts/publish", async (string? pageId, string? pageToken, string message, string? mediaUrl, string? mediaType, string? scheduleTime) =>
{
    var cfg = MetaConfig.Load();
    var pid = pageId ?? cfg.PageId;
    var tok = pageToken ?? cfg.PageAccessToken;
    if (string.IsNullOrEmpty(pid) || string.IsNullOrEmpty(tok))
        return Results.BadRequest(new { error = "Zgjidh nje faqe fillimisht" });

    DateTime? scheduled = null;
    if (!string.IsNullOrEmpty(scheduleTime) && DateTime.TryParse(scheduleTime, out var st))
        scheduled = st.ToUniversalTime();

    string? result;
    if (!string.IsNullOrEmpty(mediaUrl) && mediaType == "video")
        result = await meta.PublishVideo(pid, tok, mediaUrl, message, message);
    else if (!string.IsNullOrEmpty(mediaUrl))
        result = await meta.PublishPhoto(pid, tok, mediaUrl, message);
    else
        result = await meta.PublishPost(pid, tok, message, null, null, scheduled);

    if (result != null && result.StartsWith("Gabim"))
        return Results.BadRequest(new { error = result });

    if (scheduled.HasValue)
    {
        var posts = scheduler.LoadScheduled();
        posts.Add(new PostItem
        {
            Id = result ?? "",
            PageId = pid,
            Message = message,
            MediaUrl = mediaUrl ?? "",
            ScheduledTime = scheduled,
            Status = "scheduled"
        });
        scheduler.SaveScheduled(posts);
    }

    return Results.Ok(new { id = result, message = scheduled.HasValue ? "Postimi u skedulua" : "Postimi u publikua" });
});

app.MapGet("/api/posts", (string? pageId, string? pageToken) =>
{
    var cfg = MetaConfig.Load();
    var pid = pageId ?? cfg.PageId;
    var tok = pageToken ?? cfg.PageAccessToken;
    if (string.IsNullOrEmpty(pid) || string.IsNullOrEmpty(tok))
        return Results.Ok(new List<PostItem>());

    var posts = meta.GetPagePosts(pid, tok).Result;
    var scheduled = scheduler.LoadScheduled().Where(p => p.PageId == pid && p.Status == "scheduled").ToList();
    posts.AddRange(scheduled);
    return Results.Ok(posts.OrderByDescending(p => p.CreatedTime ?? p.ScheduledTime ?? DateTime.MinValue));
});

app.MapGet("/api/posts/scheduled", () =>
{
    var posts = scheduler.LoadScheduled().Where(p => p.Status == "scheduled").ToList();
    return Results.Ok(posts.OrderBy(p => p.ScheduledTime));
});

app.MapDelete("/api/posts/{postId}", async (string postId, string? pageToken) =>
{
    var cfg = MetaConfig.Load();
    var tok = pageToken ?? cfg.PageAccessToken;
    if (string.IsNullOrEmpty(tok))
        return Results.BadRequest(new { error = "Pa token" });
    var ok = await meta.DeletePost(tok, postId);
    return Results.Ok(new { deleted = ok });
});

// --- Comments APIs ---
app.MapGet("/api/comments", (string postId, string? pageId, string? pageToken) =>
{
    var cfg = MetaConfig.Load();
    var pid = pageId ?? cfg.PageId;
    var tok = pageToken ?? cfg.PageAccessToken;
    if (string.IsNullOrEmpty(pid) || string.IsNullOrEmpty(tok))
        return Results.Ok(new List<CommentItem>());
    var comments = meta.GetPostComments(pid, tok, postId).Result;
    return Results.Ok(comments);
});

app.MapPost("/api/comments/reply", async (string commentId, string message, string? pageToken) =>
{
    var cfg = MetaConfig.Load();
    var tok = pageToken ?? cfg.PageAccessToken;
    if (string.IsNullOrEmpty(tok))
        return Results.BadRequest(new { error = "Pa token" });
    var result = await meta.ReplyToComment(tok, commentId, message);
    if (result != null && result.StartsWith("Gabim"))
        return Results.BadRequest(new { error = result });
    return Results.Ok(new { id = result, message = "Komenti u pergjigj" });
});

// --- Instagram APIs ---
app.MapPost("/api/instagram/publish", async (string? instagramId, string? pageToken, string mediaUrl, string caption, string mediaType) =>
{
    var cfg = MetaConfig.Load();
    var igId = instagramId ?? cfg.InstagramId;
    var tok = pageToken ?? cfg.PageAccessToken;
    if (string.IsNullOrEmpty(igId) || string.IsNullOrEmpty(tok))
        return Results.BadRequest(new { error = "Zgjidh nje faqe Instagram fillimisht" });
    var result = await meta.PublishInstagramPost(igId, tok, mediaUrl, caption, mediaType);
    if (result != null && result.StartsWith("Gabim"))
        return Results.BadRequest(new { error = result });
    return Results.Ok(new { id = result });
});

// --- Config API ---
app.MapGet("/api/config", () =>
{
    var cfg = MetaConfig.Load();
    return Results.Ok(new
    {
        cfg.AppId,
        hasToken = !string.IsNullOrEmpty(cfg.UserAccessToken),
        cfg.PageId,
        cfg.InstagramId,
        pages = cfg.Pages.Select(p => new { p.Id, p.Name })
    });
});

// --- AI Content APIs ---
app.MapPost("/api/ai/generate", async (string? imageUrl, string? topic, string? tone) =>
{
    var result = await ai.GeneratePostContent(imageUrl, topic, tone);
    return Results.Ok(result);
});

app.MapPost("/api/ai/generate-with-image", async (HttpRequest request) =>
{
    if (!request.HasFormContentType || request.Form.Files.Count == 0)
        return Results.BadRequest(new { error = "Ngarko nje imazh" });

    var file = request.Form.Files[0];
    if (file.Length > 10 * 1024 * 1024)
        return Results.BadRequest(new { error = "Imazhi max 10MB" });

    var topic = request.Form["topic"].FirstOrDefault();
    var tone = request.Form["tone"].FirstOrDefault() ?? "professional";

    // Upload to temporary storage and get URL
    var uploadsDir = Path.Combine(Path.GetTempPath(), "SocialBotUploads");
    if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);
    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
    var filePath = Path.Combine(uploadsDir, fileName);
    using (var stream = File.Create(filePath))
        await file.CopyToAsync(stream);

    // For AI analysis, we need a publicly accessible URL or base64
    // We'll serve it from a local endpoint
    var imageUrl = $"{request.Scheme}://{request.Host}/uploads/{fileName}";

    // Register static file serving for uploads
    // Use base64 for AI since it might not have public access
    var result = await ai.GeneratePostContent(filePath, topic, tone);

    return Results.Ok(new { result, imageUrl });
});

app.MapPost("/api/ai/moderate", async (string caption, bool? useAi) =>
{
    if (useAi == true)
    {
        var cfg = AiSettings.Load();
        var result = await moderator.ModerateWithAI(caption, cfg.OpenAiKey);
        return Results.Ok(result);
    }
    var localResult = moderator.Analyze(caption);
    return Results.Ok(localResult);
});

app.MapPost("/api/ai/settings", (string provider, string? openAiKey, string? geminiKey, string? ollamaEndpoint, bool? moderationEnabled) =>
{
    var cfg = AiSettings.Load();
    cfg.Provider = provider;
    if (openAiKey != null) cfg.OpenAiKey = openAiKey;
    if (geminiKey != null) cfg.GeminiKey = geminiKey;
    if (ollamaEndpoint != null) cfg.OllamaEndpoint = ollamaEndpoint;
    if (moderationEnabled.HasValue) cfg.ModerationEnabled = moderationEnabled.Value;
    cfg.Save();
    return Results.Ok(new { message = "Konfigurimi AI u ruajt" });
});

app.MapGet("/api/ai/settings", () =>
{
    var cfg = AiSettings.Load();
    return Results.Ok(new
    {
        cfg.Provider,
        hasOpenAi = !string.IsNullOrEmpty(cfg.OpenAiKey),
        hasGemini = !string.IsNullOrEmpty(cfg.GeminiKey),
        cfg.OllamaEndpoint,
        cfg.ModerationEnabled,
        cfg.OpenAiModel,
        cfg.GeminiModel,
        cfg.OllamaModel
    });
});

// Serve uploaded files
app.MapGet("/uploads/{fileName}", (string fileName) =>
{
    var uploadsDir = Path.Combine(Path.GetTempPath(), "SocialBotUploads");
    var filePath = Path.Combine(uploadsDir, fileName);
    if (!File.Exists(filePath))
        return Results.NotFound();
    return Results.File(filePath);
});

// Fallback to index.html
app.MapFallbackToFile("index.html");

var urls = string.Join(", ", app.Urls);
Console.WriteLine(@"");
Console.WriteLine(@"  ╔══════════════════════════════════════╗");
Console.WriteLine(@"  ║       SocialBot - Meta Manager      ║");
Console.WriteLine(@"  ║        Fanpage & Instagram Bot      ║");
Console.WriteLine(@"  ╠══════════════════════════════════════╣");
Console.WriteLine(@"  ║  Web UI: {0,-29}║", app.Urls.FirstOrDefault() ?? "http://localhost:5000");
Console.WriteLine(@"  ╚══════════════════════════════════════╝");
Console.WriteLine(@"");

app.Run();
