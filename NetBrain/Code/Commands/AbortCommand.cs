using Microsoft.AspNetCore.Http;

namespace NetBrain.Code.Commands;

public class AbortCommand(VideoStock videoStock) : IEndpointCommand, ITelegramCommand
{
    public string Name => "/abort";
    public HttpMethod Method => HttpMethod.Delete;

    public Task<IResult> ExecuteAsync(HttpRequest request)
    {
        if (!int.TryParse(request.Query["index"], out var index))
            return Task.FromResult(Results.BadRequest("Missing or invalid 'index' parameter.") as IResult);

        var result = AbortByIndex(index);
        return Task.FromResult(Results.Ok(result) as IResult);
    }

    public Task<string> ExecuteAsync(string[] args)
    {
        if (args.Length == 0 || !int.TryParse(args[0], out var index))
            return Task.FromResult("Usage: /abort {index}");

        return Task.FromResult(AbortByIndex(index));
    }

    private string AbortByIndex(int index)
    {
        var videos = videoStock.Videos.Reverse().Take(5).ToList();

        if (index < 0 || index >= videos.Count)
            return $"Invalid index {index}. Use /stock to see available videos.";

        var video = videos[index];
        videoStock.RemoveVideo(video.Id);

        return $"Video '{video.Title}' has been removed from stock.";
    }
}
