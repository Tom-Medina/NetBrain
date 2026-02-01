using Microsoft.AspNetCore.Http;
using NetBrain.Code.Clock;

namespace NetBrain.Code.Commands;

public class UntilCommand(VideoStock videoStock) : IEndpointCommand, ITelegramCommand
{
    public string Name => "/until";
    public HttpMethod Method => HttpMethod.Get;

    public Task<IResult> ExecuteAsync(HttpRequest request)
    {
        var (lastPost, postCount, days) = Simulate();

        if (lastPost == null)
            return Task.FromResult(Results.Ok(new { message = "Stock is empty." }) as IResult);

        return Task.FromResult(Results.Ok(new
        {
            until = lastPost.Value.ToString("dd/MM HH:mm"),
            posts = postCount,
            days
        }) as IResult);
    }

    public Task<string> ExecuteAsync(string[] args)
    {
        var (lastPost, postCount, days) = Simulate();

        if (lastPost == null)
            return Task.FromResult("Stock is empty.");

        return Task.FromResult($"Stock until {lastPost.Value:dd/MM} at {lastPost.Value:HH:mm}\n{postCount} posts left · {days} days");
    }

    private (DateTime? LastPost, int PostCount, int Days) Simulate()
    {
        var timeline = BestTime.GetTimeline();
        var pending = videoStock.GetPendingVideos();

        if (pending.Count == 0)
            return (null, 0, 0);

        var stock = pending.Select(v => new HashSet<string>(v.Platforms, StringComparer.OrdinalIgnoreCase)).ToList();
        var now = DateTime.Now;
        var today = now.Date;
        var nowTime = TimeOnly.FromDateTime(now);

        DateTime? lastPost = null;
        var postCount = 0;
        var maxDays = 365;

        for (var day = 0; day < maxDays; day++)
        {
            var currentDate = today.AddDays(day);

            foreach (var slot in timeline)
            {
                if (day == 0 && slot.Time <= nowTime)
                    continue;

                var videoIndex = stock.FindIndex(platforms => platforms.Contains(slot.Platform));
                if (videoIndex == -1)
                    continue;

                postCount++;
                lastPost = currentDate.Add(slot.Time.ToTimeSpan());

                stock[videoIndex].Remove(slot.Platform);
                if (stock[videoIndex].Count == 0)
                    stock.RemoveAt(videoIndex);
            }

            if (stock.Count == 0)
                break;
        }

        var days = lastPost.HasValue ? (int)Math.Ceiling((lastPost.Value - now).TotalDays) : 0;
        return (lastPost, postCount, days);
    }
}
