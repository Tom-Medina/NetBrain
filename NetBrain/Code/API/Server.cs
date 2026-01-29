using NetBrain.Code.Commands;

namespace NetBrain.Code.API;

public class Server(WebApplicationBuilder builder, EndpointRegistry endpoints)
{
    private readonly WebApplication _api = builder.Build();

    public void Start()
    {
        foreach (var command in endpoints.GetAll())
        {
            if (command.Method == HttpMethod.Get)
            {
                _api.MapGet(command.Name, async (HttpRequest request) => await command.ExecuteAsync(request));
            }
            else if (command.Method == HttpMethod.Post)
            {
                _api.MapPost(command.Name, async (HttpRequest request) => await command.ExecuteAsync(request));
            }
        }

        Directory.CreateDirectory("queue");

        Console.WriteLine("Server started");
        _api.Run("http://0.0.0.0:5000");
    }
}
