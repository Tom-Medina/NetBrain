using NetBrain.Code;
using NetBrain.Code.API;
using NetBrain.Code.Clock;
using NetBrain.Code.Commands;
using NetBrain.Code.Videos;
using NetBrain.Utils;

var builder = WebApplication.CreateBuilder();

var videosPath = Path.Combine(StorageUtility.GetPersistentDataPath(), "videos");
var videoStock = new VideoStock(videosPath);
var uploadPost = UploadPost.Init(builder);
var uploadVk = UploadVk.Init(builder);
var uploadDecider = new UploadDecider(uploadPost, uploadVk);

var clock = new Clock(TimeSpan.FromSeconds(1));
clock.Start();

var pingCommand = new PingCommand();
var ipCommand = new IpCommand();
var stockCommand = new StockCommand(videoStock);
var abortCommand = new AbortCommand(videoStock);
var statsCommand = new StatsCommand(uploadPost);

var telegramCommands = new TelegramRegistry()
    .Register(pingCommand)
    .Register(ipCommand)
    .Register(stockCommand)
    .Register(abortCommand)
    .Register(statsCommand);

var telegram = new NetBrain.Telegram.Telegram(builder, telegramCommands);
videoStock.SetTelegram(telegram);

var videoUploader = new VideoUploader(uploadDecider, videoStock, telegram, videosPath);
var postScheduler = new PostScheduler(videoUploader, videoStock, telegram, clock);
var postCommand = new PostCommand(videoUploader, telegram);
var nextCommand = new NextCommand(postScheduler);
telegramCommands.Register(postCommand);
telegramCommands.Register(nextCommand);

var endpoints = new EndpointRegistry()
    .Register(pingCommand)
    .Register(ipCommand)
    .Register(new UploadCommand(videoStock))
    .Register(stockCommand)
    .Register(abortCommand)
    .Register(postCommand).Register(statsCommand)
    .Register(nextCommand);

telegram.Start();

var server = new Server(builder, endpoints);
server.Start();