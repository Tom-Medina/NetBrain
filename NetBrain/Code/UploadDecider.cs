namespace NetBrain.Code;

public class UploadDecider
{
    private readonly UploadPost _uploadPost;
    private readonly UploadVk _uploadVk;

    public UploadDecider(UploadPost uploadPost, UploadVk uploadVk)
    {
        _uploadPost = uploadPost;
        _uploadVk = uploadVk;
    }

    public async Task<HttpResponseMessage> UploadVideoAsync(string videoPath, string title, string description, string platform)
    {
        if (platform.Equals("vk", StringComparison.OrdinalIgnoreCase))
            return await _uploadVk.UploadVideoAsync(videoPath, title, description);

        return await _uploadPost.UploadVideoAsync(videoPath, title, platform);
    }
}
