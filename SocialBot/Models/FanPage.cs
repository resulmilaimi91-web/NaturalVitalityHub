namespace SocialBot.Models;

public class FanPage
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string AccessToken { get; set; } = "";
    public string Category { get; set; } = "";
    public int FollowerCount { get; set; }
    public bool IsInstagram { get; set; }
    public string PictureUrl { get; set; } = "";
}
