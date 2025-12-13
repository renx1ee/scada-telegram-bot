using System.Text.Json;
using Microsoft.Extensions.Options;
using Renx1ee.OPCControl;
using SkadaTelegramBot_.Configurations;
using SkadaTelegramBot_.DTOs;
using SkadaTelegramBot_.Services;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SkadaTelegramBot_.BackgroundWorkers;

public class TelegramBotMainBackgroundService : BackgroundService
{
    private readonly ILogger<TelegramBotMainBackgroundService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly BotConfiguration _botConfiguration;
    private TelegramBotClient _botClient;
    private readonly TimeSpan _pollInterval = TimeSpan.FromSeconds(1);

    public TelegramBotMainBackgroundService(
        ILogger<TelegramBotMainBackgroundService> logger,
        IServiceProvider serviceProvider,
        IOptions<BotConfiguration> botOptions)
    {
        this._logger = logger;
        this._serviceProvider = serviceProvider;
        this._botConfiguration = botOptions.Value;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        string token = "8046621610:AAER-ci_NE92mAYbgZmMGMRTG-f9qU-eghE";
        string nodeId = "";
        _botClient = new TelegramBotClient(token);

        var receiverOptions = new ReceiverOptions()
        {
            AllowedUpdates = new[]
            {
                UpdateType.Message
            },
            DropPendingUpdates = false
        };

        _botClient.StartReceiving(
            updateHandler: HandleUpdateAsync,
            errorHandler: HandleErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: stoppingToken
        );
        
        await WaitForStopSignal(stoppingToken);
    }

    private async Task HandleUpdateAsync(
        ITelegramBotClient botClient, 
        Update update, 
        CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var updateHandler = scope.ServiceProvider.GetService<UpdateHandler>()!;
            
            await updateHandler.HandleUpdateAsync(botClient, update, cancellationToken);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    private static Task HandleErrorAsync(
        ITelegramBotClient botClient,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException => 
                $"Telegram bot API error: " +
                $"[{apiRequestException.ErrorCode}]" +
                $"[{apiRequestException.Message}]",
            _ => exception.ToString()
        };
        Console.WriteLine(errorMessage);
        return Task.CompletedTask;
    }

    private async Task WaitForStopSignal(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(_pollInterval, stoppingToken);
        }
    }
}