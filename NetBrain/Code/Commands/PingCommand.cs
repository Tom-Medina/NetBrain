using Microsoft.AspNetCore.Http;

namespace NetBrain.Code.Commands;

public class PingCommand : IEndpointCommand, ITelegramCommand
{
    public string Name => "/ping";
    public HttpMethod Method => HttpMethod.Get;

    public Task<IResult> ExecuteAsync(HttpRequest request) => Task.FromResult(Results.Ok("pong"));
    public Task<string> ExecuteAsync(string[] args) => Task.FromResult("pong");
}
