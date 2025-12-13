using System.Text.Json;
using Renx1ee.OPCControl;
using SkadaTelegramBot_.DTOs;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bots.Http;

namespace SkadaTelegramBot_.Services;

public class TelegramBotService
{
    public async Task CheckAndSendToUsers(
        IServiceProvider serviceProvider,
        TelegramBotClient botClient,
        string nodeId,
        CancellationToken cancellationToken)
    {
        using (var scope = serviceProvider.CreateScope())
        {
            try
            {
                var updateHandler = scope.ServiceProvider.GetService<UpdateHandler>()!;
                var opcHelper = scope.ServiceProvider.GetService<OpcHelper>()!;

                var value = await opcHelper.GetValueAsync(
                    nodeId: nodeId,
                    cancellationToken: cancellationToken);

                if (string.IsNullOrEmpty(value))
                    return;
                
                var deserializeResult = JsonSerializer.Deserialize<RequestDto>(value);

                if (deserializeResult is null)
                    return;
            
                await updateHandler.SendMessageToUsers(
                    dto: deserializeResult,
                    botClient: botClient,
                    cancellationToken: cancellationToken
                );
            }
            catch (ApiRequestException ex)
            {
                Console.WriteLine($"Ошибка при отправке пользователю: {ex.Message}");
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Ошибка при десериализации полученного json: {ex.Message}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Ошибка: {e.Message}");
            }
        }
    }
}