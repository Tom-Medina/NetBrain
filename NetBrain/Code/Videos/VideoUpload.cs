using System;
using System.Collections.Generic;

public enum VideoStatus
{
    Queued,
    Scheduled,
    Posted,
    Failed
}

[Serializable]
public class VideoUpload
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public List<string> Platforms { get; set; } = new();
    public List<VideoVariant> Variants { get; set; } = new();
    public DateTime? ScheduledTime { get; set; }
    public VideoStatus Status { get; set; } = VideoStatus.Queued;
}

[Serializable]
public class VideoVariant
{
    public string FileName { get; set; } = "";
    public string Format { get; set; } = "";
    public byte[]? Data { get; set; }
}
