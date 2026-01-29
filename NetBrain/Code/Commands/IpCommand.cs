using Microsoft.AspNetCore.Http;
using NetBrain.Utils;

namespace NetBrain.Code.Commands;

public class IpCommand : IEndpointCommand, ITelegramCommand
{
    public string Name => "/ip";
    public HttpMethod Method => HttpMethod.Get;

    public Task<IResult> ExecuteAsync(HttpRequest request)
    {
        var ip = NetworkUtility.GetLocalIpAddress();
        return Task.FromResult(Results.Ok(ip));
    }

    public Task<string> ExecuteAsync(string[] args)
    {
        var ip = NetworkUtility.GetLocalIpAddress();
        return Task.FromResult(ip);
    }
}
