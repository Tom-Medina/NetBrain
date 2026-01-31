namespace NetBrain.Code.Videos;

public class VideoUploader
{
    private readonly UploadDecider _uploadDecider;
    private readonly VideoStock _videoStock;
    private readonly Telegram.Telegram _telegram;
    private readonly string _videosPath;

    public VideoUploader(UploadDecider uploadDecider, VideoStock videoStock, Telegram.Telegram telegram, string videosPath)
    {
        _uploadDecider = uploadDecider;
        _videoStock = videoStock;
        _telegram = telegram;
        _videosPath = videosPath;
    }

    public async Task<string> PostByIndexAsync(int index)
    {
        var videos = GetVideoByIndex();

        if (index < 0 || index >= videos.Count)
            return $"Invalid index {index}. Use /stock to see available videos.";

        var video = videos[index];
        var variant = video.Variants.FirstOrDefault();

        if (variant == null)
            return $"No video file found for '{video.Title}'.";

        var videoPath = Path.Combine(_videosPath, video.Id, variant.FileName);

        if (!File.Exists(videoPath))
            return $"Video file not found: {variant.FileName}";

        var platformsToRemove = new List<string>();
        var errors = new List<string>();

        foreach (var platform in video.Platforms.ToList())
        {
            var response = await _uploadDecider.UploadVideoAsync(videoPath, video.Title, video.Description, platform);

            if (response.IsSuccessStatusCode)
            {
                platformsToRemove.Add(platform);
                _telegram.NotifyAsync($"Video uploaded: {video.Title} → {platform}");
            }
            else
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                errors.Add($"{platform}: {response.StatusCode} - {errorBody}");
            }
        }

        foreach (var platform in platformsToRemove)
            video.Platforms.Remove(platform);

        if (video.Platforms.Count == 0)
        {
            _videoStock.RemoveVideo(video.Id);
            return $"All platforms uploaded for '{video.Title}'. Video removed from stock.";
        }

        video.Status = VideoStatus.Failed;
        _videoStock.UpdateVideo(video);

        return
            $"Partial upload for '{video.Title}'.\nSuccess: {string.Join(", ", platformsToRemove)}\nFailed: {string.Join(", ", errors)}";
    }

    public async Task<string> PostSinglePlatformAsync(VideoUpload video, string platform)
    {
        var variant = video.Variants.FirstOrDefault();

        if (variant == null)
            return $"No video file found for '{video.Title}'.";

        var videoPath = Path.Combine(_videosPath, video.Id, variant.FileName);

        if (!File.Exists(videoPath))
            return $"Video file not found: {variant.FileName}";

        var response = await _uploadDecider.UploadVideoAsync(videoPath, video.Title, video.Description, platform);

        if (response.IsSuccessStatusCode)
        {
            video.Platforms.Remove(platform);

            if (video.Platforms.Count == 0)
            {
                _videoStock.RemoveVideo(video.Id);
                return $"Uploaded '{video.Title}' → {platform}. All platforms done, video removed.";
            }

            _videoStock.UpdateVideo(video);
            return $"Uploaded '{video.Title}' → {platform}. Remaining: {string.Join(", ", video.Platforms)}";
        }

        var errorBody = await response.Content.ReadAsStringAsync();
        video.Status = VideoStatus.Failed;
        _videoStock.UpdateVideo(video);
        return $"Failed '{video.Title}' → {platform}: {response.StatusCode} - {errorBody}";
    }

    public List<VideoUpload> GetVideoByIndex()
    {
        var videos = _videoStock.Videos.Reverse().Take(5).ToList();
        return videos;
    }
}