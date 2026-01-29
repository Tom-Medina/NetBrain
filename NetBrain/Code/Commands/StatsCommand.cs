using Microsoft.AspNetCore.Http;
using System.Net.Http.Json;

namespace NetBrain.Code.Commands;

public class StatsCommand : IEndpointCommand, ITelegramCommand
{
    private readonly UploadPost _uploadPost;

    public StatsCommand(UploadPost uploadPost)
    {
        _uploadPost = uploadPost;
    }

    public string Name => "/stats";
    public HttpMethod Method => HttpMethod.Get;

    // HTTP endpoint
    public async Task<IResult> ExecuteAsync(HttpRequest request)
    {
        var stats = await _uploadPost.GetAnalyticsAsync();
        return Results.Ok(stats);
    }

    // Telegram
    public async Task<string> ExecuteAsync(string[] args)
    {
        var stats = await _uploadPost.GetAnalyticsAsync();

        if (!stats.Any())
            return "No analytics available.";

        var lines = stats.Select(platform =>
            $"""
             Platform: {platform.Platform}
             Followers: {platform.Followers}
             Impressions: {platform.Impressions}
             Profile Views: {platform.ProfileViews}
             Reach: {platform.Reach}
             """);

        return string.Join("\n\n", lines);
    }
}