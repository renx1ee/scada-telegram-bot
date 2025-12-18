using Serilog;
using SkadaTelegramBot_.BackgroundWorkers;
using SkadaTelegramBot_.Configurations;
using SkadaTelegramBot_.Helpers;
using SkadaTelegramBot_.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<BotConfigurations>(builder.Configuration.GetSection("BotConfiguration"));

builder.Services.AddSingleton<UpdateHandler>();
builder.Services.AddSingleton<IOpcHelper, OpcHelper>(
    serviceProvider =>
    {
        var logger = serviceProvider.GetRequiredService<ILogger<OpcHelper>>();
        var hostAddress = builder.Configuration["BotConfiguration:HostAddress"];
        
        return new OpcHelper(
            logger: logger,
            endpointUrl: hostAddress!,
            configuration: Config.DefaultClient);
    });
builder.Services.AddHostedService<TelegramBotMainBackgroundService>();
builder.Services.AddHostedService<OpsMotionBackgroundService>();
builder.Services.AddHostedService<ServerLifeTimeNotifierBackgroundService>();

var loggingPathFile = builder.Configuration["Logging:PathFile"] ?? "logs/log-.txt";

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .MinimumLevel.Error()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(loggingPathFile, rollingInterval: RollingInterval.Day)
    .CreateBootstrapLogger();

builder.Host.UseSerilog();
builder.Logging.AddConsole();

if (builder.Environment.IsDevelopment())
{
    builder.Logging.SetMinimumLevel(LogLevel.Debug);
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.Run();