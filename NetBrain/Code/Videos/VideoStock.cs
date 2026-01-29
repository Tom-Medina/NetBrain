using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using NetBrain.Telegram;

public class VideoStock
{
    private readonly string _rootFolder;
    private readonly string _masterFile;
    private readonly Dictionary<string, VideoUpload> _videos = new();
    private NetBrain.Telegram.Telegram? _telegram;

    public IReadOnlyCollection<VideoUpload> Videos => _videos.Values;

    public VideoStock(string rootFolder)
    {
        _rootFolder = rootFolder;
        _masterFile = Path.Combine(_rootFolder, "master.json");

        Directory.CreateDirectory(_rootFolder);

        LoadMaster();
    }

    public void SetTelegram(NetBrain.Telegram.Telegram telegram)
    {
        _telegram = telegram;
    }

    // Charger master.json et tous les video_specs.json
    private void LoadMaster()
    {
        if (!File.Exists(_masterFile))
        {
            SaveMaster();
            return;
        }

        var masterJson = File.ReadAllText(_masterFile);
        var videoIds = JsonSerializer.Deserialize<List<string>>(masterJson) ?? new List<string>();

        foreach (var id in videoIds)
        {
            var specsFile = Path.Combine(_rootFolder, id, "video_specs.json");
            if (!File.Exists(specsFile)) continue;

            var specsJson = File.ReadAllText(specsFile);
            var video = JsonSerializer.Deserialize<VideoUpload>(specsJson);
            if (video != null) _videos[video.Id] = video;
        }
    }

    // Sauvegarder master.json
    private void SaveMaster()
    {
        var ids = _videos.Keys.ToList();
        var masterJson = JsonSerializer.Serialize(ids, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_masterFile, masterJson);
    }

    // Ajouter une nouvelle vidéo
    public void AddVideo(VideoUpload video)
    {
        var videoFolder = Path.Combine(_rootFolder, video.Id);
        Directory.CreateDirectory(videoFolder);

        foreach (var variant in video.Variants)
        {
            if (variant.Data == null) continue;

            var filePath = Path.Combine(videoFolder, variant.FileName);
            File.WriteAllBytes(filePath, variant.Data);
            variant.Data = null;
        }

        var specsFile = Path.Combine(videoFolder, "video_specs.json");
        File.WriteAllText(specsFile, JsonSerializer.Serialize(video, new JsonSerializerOptions { WriteIndented = true }));

        _videos[video.Id] = video;
        SaveMaster();

        var platforms = string.Join(", ", video.Platforms);
        var variants = string.Join(", ", video.Variants.Select(v => v.FileName));
        var scheduled = video.ScheduledTime?.ToString("yyyy-MM-dd HH:mm") ?? "Not scheduled";

        var message = $"""
            New video added

            Id: {video.Id}
            Title: {video.Title}
            Description: {video.Description}
            Platforms: {platforms}
            Variants: {variants}
            Scheduled: {scheduled}
            Status: {video.Status}
            """;

        _telegram?.NotifyAsync(message);
    }

    // Mettre à jour une vidéo existante
    public void UpdateVideo(VideoUpload video)
    {
        if (!_videos.ContainsKey(video.Id)) return;

        _videos[video.Id] = video;
        var specsFile = Path.Combine(_rootFolder, video.Id, "video_specs.json");
        File.WriteAllText(specsFile, JsonSerializer.Serialize(video, new JsonSerializerOptions { WriteIndented = true }));
    }

    // Supprimer une vidéo
    public void RemoveVideo(string id)
    {
        if (!_videos.ContainsKey(id)) return;

        _videos.Remove(id);
        var videoFolder = Path.Combine(_rootFolder, id);
        if (Directory.Exists(videoFolder))
            Directory.Delete(videoFolder, true);

        SaveMaster();
    }

    // Récupérer les vidéos prêtes à poster (Queued ou Scheduled)
    public List<VideoUpload> GetPendingVideos()
    {
        return _videos.Values
            .Where(v => v.Status == VideoStatus.Queued || v.Status == VideoStatus.Scheduled)
            .OrderBy(v => v.ScheduledTime ?? DateTime.MaxValue)
            .ToList();
    }
}
