using Microsoft.AspNetCore.Http;

namespace NetBrain.Code.Commands;

public interface IEndpointCommand
{
    string Name { get; }
    HttpMethod Method { get; }
    Task<IResult> ExecuteAsync(HttpRequest request);
}

public interface ITelegramCommand
{
    string Name { get; }
    Task<string> ExecuteAsync(string[] args);
}

public class EndpointRegistry
{
    private readonly Dictionary<string, IEndpointCommand> _commands = new();

    public EndpointRegistry Register(IEndpointCommand command)
    {
        _commands[command.Name] = command;
        return this;
    }

    public IEndpointCommand? Get(string name) => _commands.GetValueOrDefault(name);
    public IEnumerable<IEndpointCommand> GetAll() => _commands.Values;
}

public class TelegramRegistry
{
    private readonly Dictionary<string, ITelegramCommand> _commands = new();

    public TelegramRegistry Register(ITelegramCommand command)
    {
        _commands[command.Name] = command;
        return this;
    }

    public ITelegramCommand? Get(string name) => _commands.GetValueOrDefault(name);
    public IEnumerable<ITelegramCommand> GetAll() => _commands.Values;
}
