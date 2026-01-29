using Microsoft.AspNetCore.Http;
using NetBrain.Code.Videos;

namespace NetBrain.Code.Commands;

public class PostCommand(VideoUploader videoUploader) : IEndpointCommand, ITelegramCommand
{
    public string Name => "/post";
    public HttpMethod Method => HttpMethod.Post;

    public async Task<IResult> ExecuteAsync(HttpRequest request)
    {
        if (!int.TryParse(request.Query["index"], out var index))
            return Results.BadRequest("Missing or invalid 'index' parameter.");

        var result = await videoUploader.PostByIndexAsync(index);
        return Results.Ok(result);
    }

    public async Task<string> ExecuteAsync(string[] args)
    {
        if (args.Length == 0 || !int.TryParse(args[0], out var index))
            return "Usage: /post {index}";

        return await videoUploader.PostByIndexAsync(index);
    }
}
