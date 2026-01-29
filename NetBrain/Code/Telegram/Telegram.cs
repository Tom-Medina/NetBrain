using NetBrain.Code.Commands;
using NetBrain.Utils;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace NetBrain.Telegram;

public class Telegram
{
    private readonly TelegramBotClient _bot;
    private readonly long _adminChatId;
    private readonly TelegramRegistry _commands;

    public Telegram(WebApplicationBuilder builder, TelegramRegistry commands)
    {
        var telegramToken = builder.Configuration["Telegram:ApiKey"]
                            ?? throw new Exception("Telegram:Token not set");

        _adminChatId = long.Parse(
            builder.Configuration["Telegram:User"]
            ?? throw new Exception("Telegram:AdminChatId not set")
        );

        _bot = new TelegramBotClient(telegramToken);
        _commands = commands;
    }

    public void Start()
    {
        _bot.StartReceiving(
            HandleUpdate,
            HandleError
        );

        Console.WriteLine("Telegram bot started");
        NotifyAsync($"Server started | ip: {NetworkUtility.GetLocalIpAddress()}:5000");
    }

    private async Task HandleUpdate(ITelegramBotClient bot, Update update, CancellationToken ct)
    {
        if (update.Type != UpdateType.Message)
            return;

        var msg = update.Message;
        if (msg?.Text == null)
            return;

        if (msg.Chat.Id != _adminChatId)
            return;

        var parts = msg.Text.Split(' ');
        var commandName = parts[0];
        var args = parts.Skip(1).ToArray();

        var command = _commands.Get(commandName);
        if (command == null)
            return;

        var result = await command.ExecuteAsync(args);
        await bot.SendMessage(msg.Chat.Id, result, cancellationToken: ct);
    }

    private Task HandleError(ITelegramBotClient bot, Exception ex, CancellationToken ct)
    {
        Console.WriteLine($"Telegram error: {ex.Message}");
        return Task.CompletedTask;
    }

    public void NotifyAsync(string message)
    {
        _bot.SendMessage(_adminChatId, message);
    }
}
