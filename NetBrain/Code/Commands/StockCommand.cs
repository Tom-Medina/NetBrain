using Microsoft.AspNetCore.Http;

namespace NetBrain.Code.Commands;

public class StockCommand(VideoStock videoStock) : IEndpointCommand, ITelegramCommand
{
    public string Name => "/stock";
    public HttpMethod Method => HttpMethod.Get;

    // Endpoint HTTP
    public Task<IResult> ExecuteAsync(HttpRequest request)
    {
        var videos = videoStock.Videos.Reverse().Take(5);

        var result = videos.Select((video, index) => new
        {
            index,
            title = video.Title,
            file = video.Variants.FirstOrDefault()?.FileName ?? "no-file"
        });

        return Task.FromResult(Results.Ok(result) as IResult);
    }

    // Telegram
    public Task<string> ExecuteAsync(string[] args)
    {
        var videos = videoStock.Videos.Reverse().Take(5).ToList();

        if (!videos.Any())
            return Task.FromResult("No video in stock.");

        var lines = videos.Select((video, index) =>
        {
            var platforms = video.Platforms.Any() ? string.Join(", ", video.Platforms) : "none";
            var variants = video.Variants.Any()
                ? string.Join(", ", video.Variants.Select(v => $"{v.FileName} ({v.Format})"))
                : "none";
            var scheduled = video.ScheduledTime?.ToString("yyyy-MM-dd HH:mm") ?? "not scheduled";

            return $"""
                [{index}] {video.Title}
                Id: {video.Id}
                Description: {video.Description}
                Platforms: {platforms}
                Variants: {variants}
                Scheduled: {scheduled}
                Status: {video.Status}
                """;
        });

        return Task.FromResult(string.Join("\n\n", lines));
    }
}