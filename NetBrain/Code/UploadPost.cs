using System.Net.Http.Headers;

public class UploadPost
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _user;

    public UploadPost(string apiKey, string user)
    {
        _apiKey = apiKey;
        _user = user;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Apikey", _apiKey);
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

        var response = await _httpClient.PostAsync("https://api.upload-post.com/api/upload", content);
        return response;
    }
}
