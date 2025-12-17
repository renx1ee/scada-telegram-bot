using System.Globalization;
using Microsoft.Extensions.Options;
using SkadaTelegramBot_.Configurations;
using SkadaTelegramBot_.Helpers;

namespace SkadaTelegramBot_.BackgroundWorkers;

public class ServerLifeTimeNotifierBackgroundService : BackgroundService
{
    private readonly ILogger<ServerLifeTimeNotifierBackgroundService> _logger;
    private readonly IOpcHelper _opcHelper;
    private readonly BotConfigurations _botConfigurations;
    private readonly TimeSpan _pollInterval;

    public ServerLifeTimeNotifierBackgroundService(
        ILogger<ServerLifeTimeNotifierBackgroundService> logger,
        IOptions<BotConfigurations> botOptions,
        IOpcHelper opcHelper)
    {
        this._logger = logger;
        this._opcHelper = opcHelper;
        this._botConfigurations = botOptions.Value
                           ?? throw new ArgumentNullException($"Argument {_botConfigurations} is null!");
        
        this._pollInterval = TimeSpan.FromSeconds(_botConfigurations.IntervalOfUpdateServerLifeNotifier);
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"Starting method {nameof(ServerLifeTimeNotifierBackgroundService.ExecuteAsync)}.");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation($"Started the loop of the {nameof(ServerLifeTimeNotifierBackgroundService.ExecuteAsync)} method.");
            
            var value = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
                .ToString(CultureInfo.InvariantCulture);
            
            try
            {
                await _opcHelper.SendValueAsync(
                    nodeId: _botConfigurations.NodeIds!.ServerIsLifeNotificationNodeId!,
                    value: value,
                    cancellationToken: stoppingToken);
            }
            catch (Exception e)
            {
                _logger.LogError($"Exception with sending the value to the variable ({value})" + e.Message);
            }
            
            _logger.LogInformation($"End of the while loop of the {nameof(ServerLifeTimeNotifierBackgroundService.ExecuteAsync)} method.");
            
            await Task.Delay(_pollInterval, stoppingToken);
        }
    }
}