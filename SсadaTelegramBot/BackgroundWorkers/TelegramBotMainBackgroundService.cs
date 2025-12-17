using Microsoft.Extensions.Options;
using SkadaTelegramBot_.Configurations;
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
    private readonly BotConfigurations _botConfigurations;
    private TelegramBotClient _botClient;
    private readonly TimeSpan _pollInterval;

    public TelegramBotMainBackgroundService(
        ILogger<TelegramBotMainBackgroundService> logger,
        IServiceProvider serviceProvider,
        IOptions<BotConfigurations> botOptions)
    {
        this._logger = logger;
        this._serviceProvider = serviceProvider;
        this._botConfigurations = botOptions.Value 
                           ?? throw new ArgumentNullException($"Argument {_botConfigurations} is null!");
        
        this._pollInterval = TimeSpan.FromSeconds(_botConfigurations.IntervalOfUpdateTelegramBot);
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"Starting method {nameof(TelegramBotMainBackgroundService.ExecuteAsync)}.");
        
        _botClient = new TelegramBotClient(_botConfigurations.BotToken!);

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
            _logger.LogError($"Error with {nameof(HandleUpdateAsync)}, error message: {e.Message}.");
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
        return Task.CompletedTask;
    }

    private async Task WaitForStopSignal(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation($"Started the loop of the {nameof(TelegramBotMainBackgroundService.ExecuteAsync)} method.");
            await Task.Delay(_pollInterval, stoppingToken);
        }
    }
}