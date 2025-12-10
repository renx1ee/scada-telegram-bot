using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SkadaTelegramBot_.Services;

public class UpdateHandler
{
    public async Task HandleUpdateAsync(
        ITelegramBotClient botClient,
        Update update, 
        CancellationToken cancellationToken)
    {
        try
        {
            var handler = update.Type switch
            {
                UpdateType.Message => OnMessageReceived(botClient, update.Message, cancellationToken),
                UpdateType.CallbackQuery => OnCallbackQueryReceived(botClient, update.CallbackQuery, cancellationToken),
                UpdateType.InlineQuery => OnInlineQueryReceived(botClient, update.InlineQuery, cancellationToken),
                _ => OnUnknownUpdate(botClient, update, cancellationToken)
            };
            
            await handler;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private async Task OnMessageReceived(
        ITelegramBotClient botClient,
        Message message, 
        CancellationToken cancellationToken)
    {
        if (message.Text is not { } messageText)
            return;

        var chatId = message.Chat.Id;
        var userName = message.From?.FirstName ?? "Пользователь";

        Console.WriteLine($"Получено сообщение от {userName}: {messageText}");

        switch (messageText.Split(' ')[0])
        {
            case "/start":
                await botClient.SendMessage(
                    chatId: chatId,
                    text: "Вас приветствует мастер скада бот! \n " +
                          "Отправте \\help что бы получить список доступных комманд!",
                    cancellationToken: cancellationToken);
                break;
            case "/help":
                await botClient.SendMessage(
                    chatId: chatId,
                    text: "Список всех команд: \n" +
                          "/start\n" +
                          "/help\n" +
                          "/me\n",
                    cancellationToken: cancellationToken);
                break;
            
            case "/me":
                var id = message.From.Id;
                var firstName = message.From.FirstName;
                var lastName = message.From.LastName ?? string.Empty;
                var username = message.From.Username ?? string.Empty;
                var isPremium = message.From.IsPremium;
                var languageCode = message.From.LanguageCode ?? string.Empty;

                StringBuilder text = new StringBuilder();

                text.Append(
                    "Данные об вашем аккаунте telegram: \n" +
                    $"id: {id} \n" +
                    $"is premium: {isPremium}\n" +
                    $"first name: {firstName} \n"
                );
                
                if (!string.IsNullOrEmpty(lastName))
                {
                    text.Append($"last name: {lastName} \n");
                }
                
                if (!string.IsNullOrEmpty(username))
                {
                    text.Append($"user name: {username} \n");
                }
                
                if (!string.IsNullOrEmpty(languageCode))
                {
                    text.Append($"language code: {languageCode} \n");
                }
                
                await botClient.SendMessage(
                    chatId: chatId,
                    text: text.ToString(),
                    cancellationToken: cancellationToken);
                break;
            
            default:
                await botClient.SendMessage(
                    chatId: chatId,
                    text: "Команда не распознана! \n" + 
                          "Отправте /help что бы получить список доступных комманд!",
                    cancellationToken: cancellationToken);
                break;
        }
    }
    
    private async Task OnCallbackQueryReceived(
        ITelegramBotClient botClient,
        CallbackQuery callbackQuery, 
        CancellationToken cancellationToken)
    {
            
    }

    private async Task OnInlineQueryReceived(
        ITelegramBotClient botClient,
        InlineQuery inlineQuery,
        CancellationToken cancellationToken)
    {
        
    }

    private Task OnUnknownUpdate(
        ITelegramBotClient botClient,
        Update update,
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}