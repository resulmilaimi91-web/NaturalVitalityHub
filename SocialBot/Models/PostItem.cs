namespace SocialBot.Models;

public class PostItem
{
    public string Id { get; set; } = "";
    public string PageId { get; set; } = "";
    public string Message { get; set; } = "";
    public string MediaUrl { get; set; } = "";
    public string MediaType { get; set; } = ""; // photo, video, link
    public DateTime? CreatedTime { get; set; }
    public DateTime? ScheduledTime { get; set; }
    public string Status { get; set; } = "draft"; // draft, scheduled, published, failed
    public int LikesCount { get; set; }
    public int CommentsCount { get; set; }
    public int SharesCount { get; set; }
}
