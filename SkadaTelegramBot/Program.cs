using SkadaTelegramBot_.BackgroundWorkers;
using SkadaTelegramBot_.Services;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<UpdateHandler>();
builder.Services.AddHostedService<TelegramBotBackgroundService>();

builder.Services.AddSingleton<ITelegramBotClient>(new TelegramBotClient("8046621610:AAER-ci_NE92mAYbgZmMGMRTG-f9qU-eghE"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.Run();