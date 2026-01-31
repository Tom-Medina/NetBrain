using NetBrain.Code.Utils;
using NetBrain.Code.Videos;

namespace NetBrain.Code.Clock;

public class PostScheduler : IClockListener
{
    private readonly VideoUploader _videoUploader;
    private readonly VideoStock _videoStock;
    private readonly NetBrain.Telegram.Telegram _telegram;
    private readonly JsonStorage<SchedulerState> _storage;
    private readonly Random _random = new();
    private SchedulerState _state;
    private bool _uploading;

    public PostScheduler(VideoUploader videoUploader, VideoStock videoStock,
        NetBrain.Telegram.Telegram telegram, global::Clock clock)
    {
        _videoUploader = videoUploader;
        _videoStock = videoStock;
        _telegram = telegram;
        _storage = new JsonStorage<SchedulerState>("scheduler_state.json");
        _state = _storage.Load();
        clock.AddListener(this);
    }

    public void OnTick(DateTime now)
    {
        var today = now.ToString("yyyy-MM-dd");

        if (_state.LastDate != today)   
        {
            _state.LastDate = today;
            _state.PostedToday.Clear();
            _state.DailyOffsets.Clear();
            GenerateDailyOffsets();
            _storage.Save(_state);
        }

        if (_uploading)
            return;

        var nowTime = TimeOnly.FromDateTime(now);

        foreach (var slot in BestTime.GetTimeline())
        {
            var key = $"{today}_{slot.Platform}_{slot.Region}";
            if (_state.PostedToday.Contains(key))
                continue;

            var offset = _state.DailyOffsets.GetValueOrDefault(slot.Platform, 15);
            var postTime = slot.Time.AddMinutes(-offset);

            if (nowTime < postTime)
                continue;

            var isExactMinute = nowTime.Hour == postTime.Hour && nowTime.Minute == postTime.Minute;

            if (!isExactMinute)
            {
                _state.PostedToday.Add(key);
                _storage.Save(_state);
                continue;
            }

            var video = FindVideoForPlatform(slot.Platform);
            if (video == null)
            {
                _state.PostedToday.Add(key);
                _storage.Save(_state);
                _telegram.NotifyAsync($"[Scheduler] No video in stock for {slot.Platform} ({slot.Region})");
                continue;
            }

            _state.PostedToday.Add(key);
            _storage.Save(_state);

            _uploading = true;
            Task.Run(async () =>
            {
                try
                {
                    var result = await _videoUploader.PostSinglePlatformAsync(video, slot.Platform);
                    _telegram.NotifyAsync($"[Scheduler] {result}");
                }
                catch (Exception ex)
                {
                    _telegram.NotifyAsync($"[Scheduler] Error uploading {video.Title} to {slot.Platform}: {ex.Message}");
                }
                finally
                {
                    _uploading = false;
                }
            });

            return;
        }
    }

    private VideoUpload? FindVideoForPlatform(string platform)
    {
        return _videoStock.GetPendingVideos()
            .FirstOrDefault(v => v.Platforms.Any(p => p.Equals(platform, StringComparison.OrdinalIgnoreCase)));
    }

    public (string Platform, Region Region, TimeOnly PostTime)? GetNextPost(int skip = 0)
    {
        var now = DateTime.Now;
        var today = now.ToString("yyyy-MM-dd");
        var nowTime = TimeOnly.FromDateTime(now);
        var found = 0;

        foreach (var slot in BestTime.GetTimeline())
        {
            var key = $"{today}_{slot.Platform}_{slot.Region}";
            if (_state.PostedToday.Contains(key))
                continue;

            var offset = _state.DailyOffsets.GetValueOrDefault(slot.Platform, 15);
            var postTime = slot.Time.AddMinutes(-offset);

            if (nowTime >= postTime)
                continue;

            if (found < skip)
            {
                found++;
                continue;
            }

            return (slot.Platform, slot.Region, postTime);
        }

        return null;
    }

    private void GenerateDailyOffsets()
    {
        foreach (var slot in BestTime.GetTimeline())
            _state.DailyOffsets.TryAdd(slot.Platform, _random.Next(10, 21));
    }
}
