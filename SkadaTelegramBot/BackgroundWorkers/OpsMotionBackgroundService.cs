using System.Text.Json;
using Microsoft.Extensions.Options;
using Renx1ee.OPCControl;
using SkadaTelegramBot_.Configurations;
using SkadaTelegramBot_.DTOs;
using SkadaTelegramBot_.Services;
using Telegram.Bot;
using Telegram.Bot.Exceptions;

namespace SkadaTelegramBot_.BackgroundWorkers;

public class OpsMotionBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private TelegramBotClient _botClient;
    private readonly TimeSpan _pollInterval = TimeSpan.FromSeconds(1);

    public OpsMotionBackgroundService(
        ILogger<TelegramBotMainBackgroundService> logger,
        IServiceProvider serviceProvider,
        IOptions<BotConfiguration> botOptions)
    {
        this._serviceProvider = serviceProvider;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                try
                {
                    var updateHandler = scope.ServiceProvider.GetService<UpdateHandler>()!;
                    var opcHelper = scope.ServiceProvider.GetService<OpcHelper>()!;

                    var value = await opcHelper.GetValueAsync(
                        nodeId: "",
                        cancellationToken: stoppingToken);

                    if (string.IsNullOrEmpty(value))
                        return;
                
                    var deserializeResult = JsonSerializer.Deserialize<RequestDto>(value);

                    if (deserializeResult is null)
                        return;
            
                    await updateHandler.SendMessageToUsers(
                        dto: deserializeResult,
                        botClient: _botClient,
                        cancellationToken: stoppingToken
                    );
                }
                catch (ApiRequestException ex)
                {
                    Console.WriteLine($"Error sending to user: {ex.Message}");
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"Error deserializing received json: {ex.Message}");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error: {e.Message}");
                }
            }
            
            await Task.Delay(_pollInterval, stoppingToken);
        }
    }
}