using Microsoft.AspNetCore.Http;

namespace NetBrain.Code.Commands;

public class StockCommand(VideoStock videoStock) : IEndpointCommand, ITelegramCommand
{
    public string Name => "/stock";
    public HttpMethod Method => HttpMethod.Get;

    // Endpoint HTTP
    public Task<IResult> ExecuteAsync(HttpRequest request)
    {
        var result = videoStock.Videos.SelectMany((video, index) =>
        {
            var shortTitle = video.Title.Length > 5 ? video.Title[..5] + ".." : video.Title;
            var platforms = video.Platforms.Any() ? video.Platforms : ["none"];
            return platforms.Select(p => new { index, title = shortTitle, platform = p });
        });

        return Task.FromResult(Results.Ok(result) as IResult);
    }

    // Telegram
    public Task<string> ExecuteAsync(string[] args)
    {
        var videos = videoStock.Videos.ToList();

        if (!videos.Any())
            return Task.FromResult("No video in stock.");

        var lines = videos.SelectMany((video, index) =>
        {
            var shortTitle = video.Title.Length > 5 ? video.Title[..5] + ".." : video.Title;
            var platforms = video.Platforms.Any() ? video.Platforms : ["none"];
            return platforms.Select(p => $"{index} : {shortTitle} {p}");
        });

        return Task.FromResult(string.Join("\n", lines));
    }
}