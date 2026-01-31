using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace NetBrain.Code;

public class UploadVk
{
    private readonly HttpClient _httpClient;
    private readonly string _accessToken;
    private readonly string _ownerId;
    private readonly string _groupId;

    public static UploadVk Init(WebApplicationBuilder webApplicationBuilder)
    {
        var accessToken = webApplicationBuilder.Configuration["VK:AccessToken"]
                          ?? throw new Exception("VK:AccessToken not set");
        var ownerId = webApplicationBuilder.Configuration["VK:OwnerId"]
                      ?? throw new Exception("VK:OwnerId not set");
        var groupId = webApplicationBuilder.Configuration["VK:GroupId"]
                      ?? throw new Exception("VK:GroupId not set");
        return new UploadVk(accessToken, ownerId, groupId);
    }

    private UploadVk(string accessToken, string ownerId, string groupId)
    {
        _accessToken = accessToken;
        _ownerId = ownerId;
        _groupId = groupId;
        _httpClient = new HttpClient();
    }

    /// <summary>
    /// Upload une vidéo sur VK et la publie sur le mur / Clips
    /// </summary>
    public async Task<HttpResponseMessage> UploadVideoAsync(string videoPath, string title, string description)
    {
        // Étape 1 : obtenir l'upload_url
        var saveUrl = $"https://api.vk.com/method/video.save?access_token={_accessToken}" +
                      $"&group_id={_groupId}" +
                      $"&name={Uri.EscapeDataString(title)}" +
                      $"&description={Uri.EscapeDataString(description)}" +
                      $"&wallpost=1" +
                      "&v=5.131";

        var saveResponse = await _httpClient.PostAsync(saveUrl, null);
        saveResponse.EnsureSuccessStatusCode();
        var saveJson = await saveResponse.Content.ReadAsStringAsync();
        using var saveDoc = JsonDocument.Parse(saveJson);

        if (!saveDoc.RootElement.TryGetProperty("response", out var responseElement) ||
            !responseElement.TryGetProperty("upload_url", out var uploadUrlElement))
            throw new Exception($"VK video.save failed: {saveJson}");

        var uploadUrl = uploadUrlElement.GetString() ?? throw new Exception("upload_url not found");

        // Étape 2 : uploader la vidéo sur upload_url
        using var content = new MultipartFormDataContent();
        await using var fileStream = File.OpenRead(videoPath);
        var videoContent = new StreamContent(fileStream);
        videoContent.Headers.ContentType = new MediaTypeHeaderValue("video/mp4");
        content.Add(videoContent, "video_file", Path.GetFileName(videoPath));

        var uploadResponse = await _httpClient.PostAsync(uploadUrl, content);
        return uploadResponse;
    }

    /// <summary>
    /// Récupère quelques stats de base du mur / compte
    /// </summary>
    public async Task<AccountStats> GetStatsAsync()
    {
        var url =
            $"https://api.vk.com/method/users.get?user_ids={_ownerId}&fields=followers_count&access_token={_accessToken}&v=5.131";
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var user = doc.RootElement.GetProperty("response")[0];

        return new AccountStats
        {
            Followers = user.GetProperty("followers_count").GetInt32()
        };
    }

    public class AccountStats
    {
        public int Followers { get; set; }
    }
}