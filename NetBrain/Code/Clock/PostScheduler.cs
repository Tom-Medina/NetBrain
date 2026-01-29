using System;
using System.Collections.Generic;
using NetBrain.Code.Clock;

public class PostScheduler : IClockListener
{
    private readonly NetBrain.Telegram.Telegram _telegram;
    private readonly HashSet<(string platform, int hour, int minute)> _notifiedThisHour = new();
    private int _lastHour = -1;

    private readonly Dictionary<int, List<string>> _schedule = new()
    {
        { 0, new List<string> { "Instagram", "TikTok", "YouTube" } },
        { 1, new List<string> { "Reddit", "X", "Bluesky" } },
        { 2, new List<string> { "Pinterest", "Facebook" } },
        { 3, new List<string> { "Instagram", "Reddit" } },
        { 5, new List<string> { "TikTok", "X", "Pinterest" } },
        { 8, new List<string> { "YouTube", "Bluesky", "Facebook" } },
        { 13, new List<string> { "Instagram", "TikTok", "Reddit" } },
        { 21, new List<string> { "YouTube", "X", "Pinterest" } },
        { 34, new List<string> { "Bluesky", "Facebook" } },
        { 55, new List<string> { "Instagram", "YouTube", "X", "Facebook" } },
    };

    public PostScheduler(NetBrain.Telegram.Telegram telegram, Clock clock)
    {
        _telegram = telegram;
        clock.AddListener(this);
    }

    public void OnTick(DateTime now)
    {
        var currentHour = now.Hour;
        var currentMinute = now.Minute;

        if (currentHour != _lastHour)
        {
            _lastHour = currentHour;
            _notifiedThisHour.Clear();
        }

        if (!_schedule.TryGetValue(currentMinute, out var platforms))
            return;

        foreach (var platform in platforms)
        {
            var key = (platform, currentHour, currentMinute);
            if (_notifiedThisHour.Contains(key))
                continue;

            _notifiedThisHour.Add(key);
            var text = $"[Scheduler] Post on {platform} at {currentHour:D2}:{currentMinute:D2}";
            Console.WriteLine(text);
            _telegram.NotifyAsync(text);
        }
    }
}
