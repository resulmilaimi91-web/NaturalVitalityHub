namespace SocialBot.Models;

public class CommentItem
{
    public string Id { get; set; } = "";
    public string PostId { get; set; } = "";
    public string PageId { get; set; } = "";
    public string FromName { get; set; } = "";
    public string FromId { get; set; } = "";
    public string Message { get; set; } = "";
    public DateTime? CreatedTime { get; set; }
    public int LikeCount { get; set; }
    public bool IsReplied { get; set; }
    public string ReplyMessage { get; set; } = "";
}
