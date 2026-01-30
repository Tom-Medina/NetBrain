using Microsoft.AspNetCore.Http;
using NetBrain.Code.Clock;

namespace NetBrain.Code.Commands;

public class NextCommand(PostScheduler scheduler) : IEndpointCommand, ITelegramCommand
{
    public string Name => "/next";
    public HttpMethod Method => HttpMethod.Get;

    public Task<IResult> ExecuteAsync(HttpRequest request)
    {
        var next = scheduler.GetNextPost();
        var slot = next ?? GetSlotFromTimeline(0);
        var (platform, region, postTime) = slot;
        var usTime = postTime.AddHours(-6);

        return Task.FromResult(Results.Ok(new
        {
            platform,
            region = region.ToString(),
            frTime = postTime.ToString("HH:mm"),
            usTime = usTime.ToString("HH:mm")
        }) as IResult);
    }

    public Task<string> ExecuteAsync(string[] args)
    {
        var skip = args.Length > 0 && int.TryParse(args[0], out var s) ? s : 0;
        var next = scheduler.GetNextPost(skip);
        var slot = next ?? GetSlotFromTimeline(skip);
        var (platform, region, postTime) = slot;
        var usTime = postTime.AddHours(-6);

        return Task.FromResult($"Next: {platform} for {region}\n{postTime:HH:mm} FR | {usTime:HH:mm} US");
    }

    private static (string Platform, Region Region, TimeOnly PostTime) GetSlotFromTimeline(int skip)
    {
        var timeline = BestTime.GetTimeline();
        var slot = timeline[skip % timeline.Count];
        return (slot.Platform, slot.Region, slot.Time);
    }
}
