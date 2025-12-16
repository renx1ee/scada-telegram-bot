using Renx1ee.OPCControl;
using Renx1ee.OPCControl.Configurations;
using SkadaTelegramBot_.BackgroundWorkers;
using SkadaTelegramBot_.Configurations;
using SkadaTelegramBot_.Services;

var builder = WebApplication.CreateBuilder(args);

var serverHostAddress = builder.Configuration.GetValue<string>("BotConfiguration:HostAddress")!;

builder.Services.Configure<BotOptions>(builder.Configuration.GetSection("BotConfiguration"));

builder.Services.AddSingleton<UpdateHandler>();
builder.Services.AddSingleton<OpcHelper>(serviceProvider => 
    new OpcHelper(
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