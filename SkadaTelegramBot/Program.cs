using Renx1ee.OPCControl;
using Renx1ee.OPCControl.Configurations;
using SkadaTelegramBot_.BackgroundWorkers;
using SkadaTelegramBot_.Services;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<TelegramBotService>();
builder.Services.AddScoped<UpdateHandler>();
builder.Services.AddScoped<OpcHelper>(serviceProvider => 
    new OpcHelper(
        endpointUrl: "", 
        configuration: Config.DefaultClient));
builder.Services.AddHostedService<TelegramBotMainBackgroundService>();
builder.Services.AddHostedService<OpsMotionBackgroundService>();

builder.Services.AddSingleton<ITelegramBotClient>(new TelegramBotClient("8046621610:AAER-ci_NE92mAYbgZmMGMRTG-f9qU-eghE"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.Run();