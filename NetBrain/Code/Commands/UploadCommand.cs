using Microsoft.AspNetCore.Http;

namespace NetBrain.Code.Commands;

public class UploadCommand(UploadPost uploadPost) : IEndpointCommand
{
    public string Name => "/upload";
    public HttpMethod Method => HttpMethod.Post;

    public async Task<IResult> ExecuteAsync(HttpRequest request)
    {
        var form = await request.ReadFormAsync();

        var file = form.Files["video"];
        if (file == null) return Results.BadRequest("No video provided");

        var text = form["text"];
        var platform = form["platform"];

        var savePath = Path.Combine("queue", file.FileName);

        await using (var stream = File.Create(savePath))
        {
            await file.CopyToAsync(stream);
        }

        Console.WriteLine($"Received video: {file.FileName}, text: {text}, platform: {platform}");

        var response = await uploadPost.UploadVideoAsync(savePath, text!, platform!);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            return Results.Problem($"Upload failed: {responseBody}", statusCode: (int)response.StatusCode);

        Console.WriteLine("Upload complete");
        return Results.Ok(new { status = "ok", path = savePath, uploadPost = responseBody });
    }
}
