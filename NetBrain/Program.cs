using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder();

var apiKey = builder.Configuration["UploadPost:ApiKey"] ?? throw new Exception("UploadPost:ApiKey not set");
var uploadPostUser = builder.Configuration["UploadPost:User"] ?? throw new Exception("UploadPost:User not set");
var uploadPost = new UploadPost(apiKey, uploadPostUser);

var app = builder.Build();

app.MapGet("/ping", () =>
{
    Console.WriteLine("Received ping");
    return Results.Ok("pong");
});

Directory.CreateDirectory("queue");

app.MapPost("/upload", async (HttpRequest request) =>
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
});


Console.WriteLine("Server started");
app.Run("http://0.0.0.0:5000");