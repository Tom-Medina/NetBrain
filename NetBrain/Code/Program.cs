using NetBrain.Code;
using NetBrain.Code.API;
using NetBrain.Code.Commands;

var builder = WebApplication.CreateBuilder();

var uploadPost = UploadPost.Init(builder);

var pingCommand = new PingCommand();
var ipCommand = new IpCommand();

var endpoints = new EndpointRegistry()
    .Register(pingCommand)
    .Register(ipCommand)
    .Register(new UploadCommand(uploadPost));

var telegramCommands = new TelegramRegistry()
    .Register(pingCommand)
    .Register(ipCommand);

var telegram = new NetBrain.Telegram.Telegram(builder, telegramCommands);
telegram.Start();

var server = new Server(builder, endpoints);
server.Start();
