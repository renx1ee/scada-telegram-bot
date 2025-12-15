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
    private readonly BotOptions _botOptions;
    private TelegramBotClient _botClient;
    private readonly TimeSpan _pollInterval = TimeSpan.FromSeconds(0, 1, 5);

    public TelegramBotMainBackgroundService(
        ILogger<TelegramBotMainBackgroundService> logger,
        IServiceProvider serviceProvider,
        IOptions<BotOptions> botOptions)
    {
        this._logger = logger;
        this._serviceProvider = serviceProvider;
        this._botOptions = botOptions.Value 
                           ?? throw new ArgumentNullException($"Argument {_botOptions} is null!");
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _botClient = new TelegramBotClient(_botOptions.BotToken!);

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