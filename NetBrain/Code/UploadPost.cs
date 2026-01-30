using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace NetBrain.Code;

public class UploadPost
{
    private readonly HttpClient _httpClient;
    private readonly string _user;
    private readonly Reddit _reddit = new();

    public static UploadPost Init(WebApplicationBuilder webApplicationBuilder)
    {
        var apiKey = webApplicationBuilder.Configuration["UploadPost:ApiKey"] ??
                     throw new Exception("UploadPost:ApiKey not set");
        var uploadPostUser = webApplicationBuilder.Configuration["UploadPost:User"] ??
                             throw new Exception("UploadPost:User not set");
        return new UploadPost(apiKey, uploadPostUser);
    }

    private UploadPost(string apiKey, string user)
    {
        _user = user;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Apikey", apiKey);
    }

    public async Task<HttpResponseMessage> UploadVideoAsync(string videoPath, string title, string platform)
    {
        using var content = new MultipartFormDataContent();
        await using var fileStream = File.OpenRead(videoPath);

        var videoContent = new StreamContent(fileStream);
        videoContent.Headers.ContentType = new MediaTypeHeaderValue("video/mp4");

        content.Add(videoContent, "video", Path.GetFileName(videoPath));
        content.Add(new StringContent(_user), "user");
        content.Add(new StringContent(title), "title");
        content.Add(new StringContent(platform), "platform[]");
        content.Add(new StringContent(_reddit.GetNextSubreddit()), "subreddit");

        var response = await _httpClient.PostAsync("https://api.upload-post.com/api/upload", content);
        return response;
    }

    public async Task<List<PlatformStats>> GetAnalyticsAsync()
    {
        var platforms = "youtube,reddit,x";
        var response =
            await _httpClient.GetAsync($"https://api.upload-post.com/api/analytics/{_user}?platforms={platforms}");

        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception($"API error {response.StatusCode}: {json}");

        var result = new List<PlatformStats>();
        using var doc = JsonDocument.Parse(json);

        foreach (var platform in doc.RootElement.EnumerateObject())
        {
            var data = platform.Value;
            if (data.TryGetProperty("followers", out _))
            {
                result.Add(new PlatformStats
                {
                    Platform = platform.Name,
                    Followers = data.GetProperty("followers").GetInt32(),
                    Impressions = data.GetProperty("impressions").GetInt32(),
                });
            }
        }

        return result;
    }

    public class PlatformStats
    {
        public string Platform { get; set; } = "";
        public int Followers { get; set; }
        public int Impressions { get; set; }
        public int ProfileViews { get; set; }
        public int Reach { get; set; }
    }
}