using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SocialBot.Models;

namespace SocialBot.Services;

public class MetaGraphService
{
    private readonly HttpClient _http;
    private readonly string _apiVersion = "v21.0";
    private const string BaseUrl = "https://graph.facebook.com";

    public MetaGraphService()
    {
        _http = new HttpClient();
        _http.Timeout = TimeSpan.FromSeconds(30);
        _http.DefaultRequestHeaders.UserAgent.ParseAdd("SocialBot/1.0");
    }

    public string GetOAuthUrl(string appId, string redirectUri, string state)
    {
        var scopes = string.Join(",", new[]
        {
            "pages_show_list",
            "pages_read_engagement",
            "pages_manage_posts",
            "pages_manage_engagement",
            "pages_manage_metadata",
            "instagram_basic",
            "instagram_content_publish",
            "instagram_manage_comments",
            "instagram_manage_messages",
            "business_management",
            "public_profile"
        });
        return $"{BaseUrl}/{_apiVersion}/dialog/oauth?client_id={appId}&redirect_uri={Uri.EscapeDataString(redirectUri)}&scope={Uri.EscapeDataString(scopes)}&state={state}&response_type=code";
    }

    public async Task<string?> ExchangeCodeForToken(string appId, string appSecret, string code, string redirectUri)
    {
        var url = $"{BaseUrl}/{_apiVersion}/oauth/access_token?client_id={appId}&client_secret={appSecret}&redirect_uri={Uri.EscapeDataString(redirectUri)}&code={code}";
        var resp = await _http.GetStringAsync(url);
        var json = JObject.Parse(resp);
        return json["access_token"]?.ToString();
    }

    public async Task<string?> DebugToken(string token)
    {
        var url = $"{BaseUrl}/{_apiVersion}/debug_token?input_token={token}&access_token={token}";
        var resp = await _http.GetStringAsync(url);
        var json = JObject.Parse(resp);
        var data = json["data"];
        if (data?["is_valid"]?.Value<bool>() == true)
            return data["expires_at"]?.Value<long>().ToString();
        return null;
    }

    public async Task<(List<FanPage> Pages, string? Error)> GetUserPages(string userToken)
    {
        try
        {
            var pages = new List<FanPage>();
            var url = $"{BaseUrl}/{_apiVersion}/me/accounts?access_token={userToken}&fields=id,name,category,followers_count,picture";
            var resp = await _http.GetStringAsync(url);
            var json = JObject.Parse(resp);

            if (json["error"] != null)
            {
                var errMsg = json["error"]?["message"]?.ToString() ?? "";
                var errCode = json["error"]?["code"]?.Value<int>() ?? 0;
                return (pages, errCode == 190 ? "Token i skaduar ose i pavlefshem. Riautorizu." : $"Gabim API: {errMsg}");
            }

            foreach (var item in json["data"] ?? new JArray())
            {
                pages.Add(new FanPage
                {
                    Id = item["id"]?.ToString() ?? "",
                    Name = item["name"]?.ToString() ?? "",
                    AccessToken = item["access_token"]?.ToString() ?? "",
                    Category = item["category"]?.ToString() ?? "",
                    FollowerCount = item["followers_count"]?.Value<int>() ?? 0,
                    PictureUrl = item["picture"]?["data"]?["url"]?.ToString() ?? ""
                });
            }
            return (pages, null);
        }
        catch (HttpRequestException ex)
        {
            return (new List<FanPage>(), $"Gabim rrjeti: {ex.Message}");
        }
        catch (Exception ex)
        {
            return (new List<FanPage>(), $"Gabim: {ex.Message}");
        }
    }

    public async Task<string?> GetInstagramId(string pageId, string pageToken)
    {
        try
        {
            var url = $"{BaseUrl}/{_apiVersion}/{pageId}?fields=instagram_business_account&access_token={pageToken}";
            var resp = await _http.GetStringAsync(url);
            var json = JObject.Parse(resp);
            return json["instagram_business_account"]?["id"]?.ToString();
        }
        catch { return null; }
    }

    public async Task<string?> PublishPost(string pageId, string pageToken, string message, string? mediaUrl = null, string? mediaType = null, DateTime? scheduledTime = null)
    {
        try
        {
            var payload = new Dictionary<string, string>
            {
                ["message"] = message,
                ["access_token"] = pageToken
            };

            if (!string.IsNullOrEmpty(mediaUrl))
            {
                if (mediaType == "video")
                    payload["attached_media"] = $"[{{\"media_fbid\":\"{mediaUrl}\"}}]";
                else
                    payload["url"] = mediaUrl;
            }

            if (scheduledTime.HasValue && scheduledTime.Value > DateTime.UtcNow)
            {
                var unixTs = new DateTimeOffset(scheduledTime.Value).ToUnixTimeSeconds();
                payload["published"] = "false";
                payload["scheduled_publish_time"] = unixTs.ToString();
            }

            var content = new FormUrlEncodedContent(payload);
            var url = $"{BaseUrl}/{_apiVersion}/{pageId}/feed";
            var resp = await _http.PostAsync(url, content);
            var respBody = await resp.Content.ReadAsStringAsync();
            var json = JObject.Parse(respBody);

            if (json["error"] != null)
                return $"Gabim: {json["error"]?["message"]}";
            return json["id"]?.ToString();
        }
        catch (Exception ex)
        {
            return $"Gabim: {ex.Message}";
        }
    }

    public async Task<string?> PublishPhoto(string pageId, string pageToken, string imageUrl, string message)
    {
        try
        {
            var payload = new Dictionary<string, string>
            {
                ["url"] = imageUrl,
                ["message"] = message,
                ["access_token"] = pageToken
            };
            var content = new FormUrlEncodedContent(payload);
            var url = $"{BaseUrl}/{_apiVersion}/{pageId}/photos";
            var resp = await _http.PostAsync(url, content);
            var respBody = await resp.Content.ReadAsStringAsync();
            var json = JObject.Parse(respBody);
            if (json["error"] != null)
                return $"Gabim: {json["error"]?["message"]}";
            return json["id"]?.ToString();
        }
        catch (Exception ex)
        {
            return $"Gabim: {ex.Message}";
        }
    }

    public async Task<string?> PublishVideo(string pageId, string pageToken, string videoUrl, string title, string description)
    {
        try
        {
            var payload = new Dictionary<string, string>
            {
                ["file_url"] = videoUrl,
                ["title"] = title,
                ["description"] = description,
                ["access_token"] = pageToken
            };
            var content = new FormUrlEncodedContent(payload);
            var url = $"{BaseUrl}/{_apiVersion}/{pageId}/videos";
            var resp = await _http.PostAsync(url, content);
            var respBody = await resp.Content.ReadAsStringAsync();
            var json = JObject.Parse(respBody);
            if (json["error"] != null)
                return $"Gabim: {json["error"]?["message"]}";
            return json["id"]?.ToString();
        }
        catch (Exception ex)
        {
            return $"Gabim: {ex.Message}";
        }
    }

    public async Task<string?> PublishInstagramPost(string instagramId, string pageToken, string mediaUrl, string caption, string mediaType = "IMAGE")
    {
        try
        {
            var payload = new Dictionary<string, string>
            {
                ["media_type"] = mediaType,
                [mediaType == "VIDEO" ? "video_url" : "image_url"] = mediaUrl,
                ["caption"] = caption,
                ["access_token"] = pageToken
            };
            var content = new FormUrlEncodedContent(payload);

            var createUrl = $"{BaseUrl}/{_apiVersion}/{instagramId}/media";
            var resp = await _http.PostAsync(createUrl, content);
            var respBody = await resp.Content.ReadAsStringAsync();
            var json = JObject.Parse(respBody);
            var creationId = json["id"]?.ToString();
            if (json["error"] != null || string.IsNullOrEmpty(creationId))
                return $"Gabim: {json["error"]?["message"]}";

            await Task.Delay(5000);

            var publishPayload = new Dictionary<string, string>
            {
                ["creation_id"] = creationId,
                ["access_token"] = pageToken
            };
            var pubContent = new FormUrlEncodedContent(publishPayload);
            var pubUrl = $"{BaseUrl}/{_apiVersion}/{instagramId}/media_publish";
            var pubResp = await _http.PostAsync(pubUrl, pubContent);
            var pubBody = await pubResp.Content.ReadAsStringAsync();
            var pubJson = JObject.Parse(pubBody);
            if (pubJson["error"] != null)
                return $"Gabim: {pubJson["error"]?["message"]}";
            return pubJson["id"]?.ToString();
        }
        catch (Exception ex)
        {
            return $"Gabim: {ex.Message}";
        }
    }

    public async Task<List<PostItem>> GetPagePosts(string pageId, string pageToken, int limit = 25)
    {
        var posts = new List<PostItem>();
        try
        {
            var url = $"{BaseUrl}/{_apiVersion}/{pageId}/posts?access_token={pageToken}&fields=id,message,created_time,permalink_url,shares,comments.limit(0).summary(true),likes.limit(0).summary(true)&limit={limit}";
            var resp = await _http.GetStringAsync(url);
            var json = JObject.Parse(resp);
            foreach (var item in json["data"] ?? new JArray())
            {
                posts.Add(new PostItem
                {
                    Id = item["id"]?.ToString() ?? "",
                    PageId = pageId,
                    Message = item["message"]?.ToString() ?? "(pa tekst)",
                    CreatedTime = DateTime.TryParse(item["created_time"]?.ToString(), out var dt) ? dt : null,
                    LikesCount = item["likes"]?["summary"]?["total_count"]?.Value<int>() ?? 0,
                    CommentsCount = item["comments"]?["summary"]?["total_count"]?.Value<int>() ?? 0,
                    SharesCount = item["shares"]?["count"]?.Value<int>() ?? 0,
                    Status = "published"
                });
            }
        }
        catch { }
        return posts;
    }

    public async Task<List<CommentItem>> GetPostComments(string pageId, string pageToken, string postId)
    {
        var comments = new List<CommentItem>();
        try
        {
            var url = $"{BaseUrl}/{_apiVersion}/{postId}/comments?access_token={pageToken}&fields=id,from,message,created_time,like_count,comments.limit(0).summary(true)&order=reverse_chronological";
            var resp = await _http.GetStringAsync(url);
            var json = JObject.Parse(resp);
            foreach (var item in json["data"] ?? new JArray())
            {
                comments.Add(new CommentItem
                {
                    Id = item["id"]?.ToString() ?? "",
                    PostId = postId,
                    PageId = pageId,
                    FromName = item["from"]?["name"]?.ToString() ?? "Anonim",
                    FromId = item["from"]?["id"]?.ToString() ?? "",
                    Message = item["message"]?.ToString() ?? "",
                    CreatedTime = DateTime.TryParse(item["created_time"]?.ToString(), out var dt) ? dt : null,
                    LikeCount = item["like_count"]?.Value<int>() ?? 0
                });
            }
        }
        catch { }
        return comments;
    }

    public async Task<string?> ReplyToComment(string pageToken, string commentId, string message)
    {
        try
        {
            var payload = new Dictionary<string, string>
            {
                ["message"] = message,
                ["access_token"] = pageToken
            };
            var content = new FormUrlEncodedContent(payload);
            var url = $"{BaseUrl}/{_apiVersion}/{commentId}/comments";
            var resp = await _http.PostAsync(url, content);
            var respBody = await resp.Content.ReadAsStringAsync();
            var json = JObject.Parse(respBody);
            if (json["error"] != null)
                return $"Gabim: {json["error"]?["message"]}";
            return json["id"]?.ToString();
        }
        catch (Exception ex)
        {
            return $"Gabim: {ex.Message}";
        }
    }

    public async Task<bool> DeletePost(string pageToken, string postId)
    {
        try
        {
            var url = $"{BaseUrl}/{_apiVersion}/{postId}?access_token={pageToken}";
            var resp = await _http.DeleteAsync(url);
            return resp.IsSuccessStatusCode;
        }
        catch { return false; }
    }
}
