using System.Text.Json;
using Microsoft.Extensions.Options;
using SkadaTelegramBot_.Configurations;
using SkadaTelegramBot_.DTOs;
using SkadaTelegramBot_.Helpers;
using SkadaTelegramBot_.Services;
using Telegram.Bot;
using Telegram.Bot.Exceptions;

namespace SkadaTelegramBot_.BackgroundWorkers;

public class OpsMotionBackgroundService : BackgroundService
{
    private readonly ILogger<OpsMotionBackgroundService> _logger;
    private TelegramBotClient _botClient;
    private readonly UpdateHandler _updateHandler;
    private readonly IOpcHelper _opcHelper;
    private readonly BotOptions _botOptions;
    private readonly TimeSpan _pollInterval = TimeSpan.FromSeconds(5); // TODO: из конфига
    private string? _lastSentJson = null;

    public OpsMotionBackgroundService(
        ILogger<OpsMotionBackgroundService> logger,
        IOptions<BotOptions> botOptions,
        UpdateHandler updateHandler,
        IOpcHelper opcHelper)
    {
        this._logger = logger;
        this._updateHandler = updateHandler;
        this._opcHelper = opcHelper;
        this._botOptions = botOptions.Value
                           ?? throw new ArgumentNullException($"Argument {_botOptions} is null!");
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _botClient = new TelegramBotClient(_botOptions.BotToken!);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            string? value = null;
            string? scadaErrorValue = null;
            
            try
            {
                value = await _opcHelper.GetValueAsync(
                    nodeId: _botOptions.NotificationNodeId!,
                    cancellationToken: stoppingToken);

                if (string.IsNullOrEmpty(value) || value == _lastSentJson)
                {
                    await Task.Delay(_pollInterval, stoppingToken);
                    continue;
                }
            
                var dto = JsonSerializer.Deserialize<RequestDto>(value);

                if (dto is null)
                    continue;
        
                await _updateHandler.SendMessageToUsers(
                    dto: dto,
                    botClient: _botClient,
                    cancellationToken: stoppingToken
                );
                
                await _opcHelper.SendValueAsync(
                    nodeId: _botOptions.NotificationResponseNodeId!,
                    value: dto.Id.ToString(),
                    cancellationToken: stoppingToken);

                _lastSentJson = value;
            }
            catch (ApiRequestException ex)
            {
                Console.WriteLine($"Telegram API error with sending to user: {ex.Message}");
                try
                {
                    await _opcHelper.SendValueAsync(
                        nodeId: _botOptions.NotificationResponseNodeId!,
                        value: CreateScadaErrorJson("Telegram API error", ex.Message),
                        cancellationToken: stoppingToken);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error deserializing received json: {ex.Message}");
                try
                {
                    await _opcHelper.SendValueAsync(
                        nodeId: _botOptions.NotificationResponseNodeId!,
                        value: CreateScadaErrorJson("Invalid JSON from opc", $"Json Serialize exception with {value}"),
                        cancellationToken: stoppingToken);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    scadaErrorValue = CreateScadaErrorJson($"Error with {value}, message: ", ex.Message);
                }
                
                if (scadaErrorValue != null)
                {
                    await _opcHelper.SendValueAsync(
                        nodeId: _botOptions.NotificationResponseNodeId!,
                        value: scadaErrorValue,
                        cancellationToken: stoppingToken);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
            }
            
            await Task.Delay(_pollInterval, stoppingToken);
        }
    }

    private string CreateScadaErrorJson(string errorType, string message, string? details = null)
    {
        var errorObj = new
        {
            ErrorType = errorType,
            Message = message,
            Details = details,
            Timestamp = DateTime.UtcNow
        };
        return JsonSerializer.Serialize(errorObj);
    }
}

// сделать переменную, в котторой будет обновляться время, что бы скада знала, что наш сервер жив
// {
//     "msgId": 1,
//     "sysId": "SystemName", // number type
//     "dvcId": "DeviceId", // string
//     "msgText": "Привет, мир!",
//     "tgIds": [1398791366, 8114073508]
// }
// ошибки
//      Использовать OPC UA Subscriptions (подписка на изменения) — идеальный вариант.
//      Или хотя бы увеличить интервал (например, 5–10 сек) + backoff при ошибках.