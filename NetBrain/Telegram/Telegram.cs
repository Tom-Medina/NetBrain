using NetBrain.Utils;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace NetBrain.Telegram;

public class Telegram
{
    private readonly TelegramBotClient _bot;
    private readonly long _adminChatId;

    public Telegram(string token, long adminChatId)
    {
        _bot = new TelegramBotClient(token);
        _adminChatId = adminChatId;
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
        {
            return;
        }

        var msg = update.Message;
        if (msg?.Text == null)
            return;

        if (msg.Chat.Id != _adminChatId)
            return;

        if (msg.Text == "/ping")
        {
            await bot.SendMessage(
                msg.Chat.Id,
                "Pong",
                cancellationToken: ct
            );
        }

        if (msg.Text == "/ip")
        {
            await bot.SendMessage(msg.Chat.Id, NetworkUtility.GetLocalIpAddress(), cancellationToken: ct);
        }
    }

    private Task HandleError(ITelegramBotClient bot,
        Exception ex,
        CancellationToken ct)
    {
        Console.WriteLine($"Telegram error: {ex.Message}");
        return Task.CompletedTask;
    }

    public Task NotifyAsync(string message)
    {
        return _bot.SendMessage(_adminChatId, message);
    }
}