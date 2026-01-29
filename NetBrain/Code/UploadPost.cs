using System.Net.Http.Headers;

namespace NetBrain.Code;

public class UploadPost
{
    private readonly HttpClient _httpClient;
    private readonly string _user;

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

        var response = await _httpClient.PostAsync("https://api.upload-post.com/api/upload", content);
        return response;
    }
}