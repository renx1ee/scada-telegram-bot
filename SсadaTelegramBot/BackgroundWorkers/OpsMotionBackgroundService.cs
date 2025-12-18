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
    private readonly BotConfigurations _botConfigurations;
    private readonly TimeSpan _pollInterval;
    private string? _lastSentJson = null;

    public OpsMotionBackgroundService(
        ILogger<OpsMotionBackgroundService> logger,
        IOptions<BotConfigurations> botOptions,
        UpdateHandler updateHandler,
        IOpcHelper opcHelper)
    {
        this._logger = logger;
        this._updateHandler = updateHandler;
        this._opcHelper = opcHelper;
        this._botConfigurations = botOptions.Value
                           ?? throw new ArgumentNullException($"Argument {_botConfigurations} is null!");
        
        this._pollInterval = TimeSpan.FromSeconds(_botConfigurations.IntervalOfUpdateOpcMotion);
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"Starting method {nameof(OpsMotionBackgroundService.ExecuteAsync)}.");
        
        _botClient = new TelegramBotClient(_botConfigurations.BotToken!);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation($"Started the loop of the {nameof(OpsMotionBackgroundService.ExecuteAsync)} method.");
            string? value = null;
            string? scadaErrorValue = null;
            
            try
            {
                value = await _opcHelper.GetValueAsync(
                    nodeId: _botConfigurations.NodeIds!.NotificationNodeId!,
                    cancellationToken: stoppingToken);

                if (string.IsNullOrEmpty(value) || value == _lastSentJson)
                {
                    await Task.Delay(_pollInterval, stoppingToken);
                    _logger.LogInformation($"the variable is equal to or matches the previous value.");
                    continue;
                }
            
                var dto = JsonSerializer.Deserialize<RequestDto>(value);

                if (dto is null)
                {
                    _logger.LogError($"Dto is null.");
                    await Task.Delay(_pollInterval, stoppingToken);
                    continue;
                }
        
                await _updateHandler.SendMessageToUsers(
                    dto: dto,
                    botClient: _botClient,
                    cancellationToken: stoppingToken
                );
                
                await _opcHelper.SendValueAsync(
                    nodeId: _botConfigurations.NodeIds!.NotificationResponseNodeId!,
                    value: dto.Id.ToString(),
                    cancellationToken: stoppingToken);

                _lastSentJson = value;
            }
            catch (ApiRequestException ex)
            {
                _logger.LogError($"Telegram API error with sending to user: {ex.Message}.");
                try
                {
                    await _opcHelper.SendValueAsync(
                        nodeId: _botConfigurations.NodeIds!.NotificationResponseNodeId!,
                        value: CreateScadaErrorJson("Telegram API error", ex.Message),
                        cancellationToken: stoppingToken);
                }
                catch (Exception e)
                {
                    _logger.LogError($"Sending Error: {e.Message}.");
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError($"Error deserializing received json: {ex.Message}.");
                try
                {
                    await _opcHelper.SendValueAsync(
                        nodeId: _botConfigurations.NodeIds!.NotificationResponseNodeId!,
                        value: CreateScadaErrorJson("Invalid JSON from opc", $"Json Serialize exception with {value}."),
                        cancellationToken: stoppingToken);
                }
                catch (Exception e)
                {
                    _logger.LogError($"Sending value error with {value}, message: {e.Message}.");
                    scadaErrorValue = CreateScadaErrorJson($"Error with {value}, message: ", ex.Message);
                }
                
                if (scadaErrorValue != null)
                {
                    await _opcHelper.SendValueAsync(
                        nodeId: _botConfigurations.NodeIds!.NotificationResponseNodeId!,
                        value: scadaErrorValue,
                        cancellationToken: stoppingToken);
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Error: {e.Message}");
            }
            
            _logger.LogInformation($"End of the while loop of the {nameof(OpsMotionBackgroundService.ExecuteAsync)} method.");
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