using Microsoft.AspNetCore.Http;

namespace NetBrain.Code.Commands;

public class UploadCommand(VideoStock videoStock) : IEndpointCommand
{
    public string Name => "/upload";
    public HttpMethod Method => HttpMethod.Post;

    public async Task<IResult> ExecuteAsync(HttpRequest request)
    {
        var form = await request.ReadFormAsync();

        var file = form.Files["video"];
        if (file == null) return Results.BadRequest("No video provided");

        var title = form["title"];
        var description = form["description"];
        var platforms = form["platforms"].ToString().Split(',').Select(p => p.Trim()).ToList();

        var videoUpdate = new VideoUpload
        {
            Title = title,
            Description = description,
            Platforms = platforms
        };

        // Ajouter le variant correspondant au fichier reçu
        await using (var memoryStream = new MemoryStream())
        {
            await file.CopyToAsync(memoryStream);
            videoUpdate.Variants.Add(new VideoVariant
            {
                FileName = file.FileName,
                Format = "original",
                Data = memoryStream.ToArray() // On stocke les bytes dans le variant pour VideoStock
            });
        }

        // Ajouter la vidéo au stock, VideoStock se charge de tout le stockage physique via StorageUtility
        videoStock.AddVideo(videoUpdate);

        Console.WriteLine($"Video '{videoUpdate.Title}' added to stock with ID {videoUpdate.Id}");

        return Results.Ok(new
        {
            status = "ok",
            id = videoUpdate.Id
        });
    }
}