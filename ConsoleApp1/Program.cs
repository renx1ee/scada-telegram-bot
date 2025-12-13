using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

public class Program
{
    private static ITelegramBotClient _botClient;
    private static ReceiverOptions _receiverOptions;
    private static readonly CancellationTokenSource cts;

    private static async Task Main()
    {
        string token = "8046621610:AAER-ci_NE92mAYbgZmMGMRTG-f9qU-eghE";
        _botClient = new TelegramBotClient(token);

        var receiverOptions = new ReceiverOptions()
        {
            AllowedUpdates = new[]
            {
                UpdateType.Message
            }
            // ThrowPendingUpdates = true,
        };

        _botClient.StartReceiving(
            updateHandler: HandleUpdateAsync,
            errorHandler: HandleErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: cts.Token
        );
    }
    
    static async Task HandleUpdateAsync(
        ITelegramBotClient botClient, 
        Update update, 
        CancellationToken cancellationToken)
    {
        try
        {
            if (update.Message is not { } message)
                return;

            if (message.Text is not { } messageText)
                return;

            var chatId = message.Chat.Id;
            var userName = message.From?.FirstName ?? "Пользователь";

            Console.WriteLine($"Получено сообщение от {userName}: {messageText}");

            switch (messageText)
            {
                case "/start":
                    await botClient.SendMessage(
                        chatId: chatId,
                        text: "Привет",
                        cancellationToken: cancellationToken);
                    break;
                case "/help":
                    await botClient.SendMessage(
                        chatId: chatId,
                        text: "Список всех команд: " +
                              "/start" +
                              "/help",
                        cancellationToken: cancellationToken);
                    break;
                default:
                    await botClient.SendMessage(
                        chatId: chatId,
                        text: "Команда не распознана",
                        cancellationToken: cancellationToken);
                    break;
            }
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

    public async static Task KeyboardTest(
        ITelegramBotClient botClient,
        CancellationToken cancellationToken,
        string chatId)
    {
        var replyKeyboard = new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[] { "Button 1", "Button 2"},
            new KeyboardButton[] { "Button 3"}
        })
        {
            ResizeKeyboard = true
        };

        await botClient.SendMessage(
            chatId: chatId,
            text: "",
            replyMarkup: replyKeyboard,
            cancellationToken: cancellationToken);
    }
}