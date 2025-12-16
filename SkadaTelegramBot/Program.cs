using SkadaTelegramBot_.BackgroundWorkers;
using SkadaTelegramBot_.Configurations;
using SkadaTelegramBot_.Helpers;
using SkadaTelegramBot_.Services;

var builder = WebApplication.CreateBuilder(args);

var serverHostAddress = builder.Configuration["BotConfiguration:HostAddress"]!;

builder.Services.Configure<BotOptions>(builder.Configuration.GetSection("BotConfiguration"));

builder.Services.AddSingleton<UpdateHandler>();
builder.Services.AddSingleton<IOpcHelper, OpcHelper>(
    serviceProvider => new OpcHelper(
        endpointUrl: serverHostAddress,
        configuration: Config.DefaultClient));
builder.Services.AddHostedService<TelegramBotMainBackgroundService>();
builder.Services.AddHostedService<OpsMotionBackgroundService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.Run();