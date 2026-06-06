using System.Text.Json;
using SocialBot.Models;

namespace SocialBot.Services;

public class PostScheduler : BackgroundService
{
    private readonly ILogger<PostScheduler> _logger;
    private readonly string _scheduleFile;
    private readonly MetaGraphService _meta;

    public PostScheduler(ILogger<PostScheduler> logger)
    {
        _logger = logger;
        _meta = new MetaGraphService();
        _scheduleFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SocialBot", "scheduled_posts.json");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PostScheduler started");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessScheduledPosts();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Scheduler error");
            }
            await Task.Delay(30_000, stoppingToken);
        }
    }

    private async Task ProcessScheduledPosts()
    {
        var posts = LoadScheduled();
        var now = DateTime.UtcNow;
        var due = posts.Where(p => p.Status == "scheduled" && p.ScheduledTime <= now).ToList();

        foreach (var post in due)
        {
            var config = MetaConfig.Load();
            var token = config.Pages.FirstOrDefault(p => p.Id == post.PageId)?.AccessToken ?? config.PageAccessToken;
            if (string.IsNullOrEmpty(token)) continue;

            string? result;
            if (!string.IsNullOrEmpty(post.MediaUrl))
                result = await _meta.PublishPhoto(post.PageId, token!, post.MediaUrl, post.Message);
            else
                result = await _meta.PublishPost(post.PageId, token!, post.Message);

            post.Status = result != null && !result.StartsWith("Gabim") ? "published" : "failed";
            if (result != null && !result.StartsWith("Gabim"))
                post.Id = result;

            _logger.LogInformation("Scheduled post {Status}: {Id}", post.Status, post.Id);
        }

        if (due.Count > 0) SaveScheduled(posts);
    }

    public List<PostItem> LoadScheduled()
    {
        try
        {
            if (File.Exists(_scheduleFile))
                return JsonSerializer.Deserialize<List<PostItem>>(File.ReadAllText(_scheduleFile)) ?? new();
        }
        catch { }
        return new();
    }

    public void SaveScheduled(List<PostItem> posts)
    {
        var dir = Path.GetDirectoryName(_scheduleFile);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        File.WriteAllText(_scheduleFile, JsonSerializer.Serialize(posts, new JsonSerializerOptions { WriteIndented = true }));
    }
}
