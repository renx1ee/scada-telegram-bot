using System.Text;
using Microsoft.Extensions.Options;
using SkadaTelegramBot_.Configurations;
using SkadaTelegramBot_.DTOs;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace SkadaTelegramBot_.Services;

public class UpdateHandler
{
    private readonly ILogger<UpdateHandler> _logger;
    private readonly BotConfigurations _botConfigurations;

    public UpdateHandler(
        ILogger<UpdateHandler> logger,
        IOptions<BotConfigurations> botOptions)
    {
        this._logger = logger;
        this._botConfigurations = botOptions.Value
                                  ?? throw new ArgumentNullException($"Argument {_botConfigurations} is null!");
    }
    
    public async Task HandleUpdateAsync(
        ITelegramBotClient botClient,
        Update update, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Starting method {nameof(UpdateHandler.HandleUpdateAsync)}");
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
            _logger.LogError($"Error message: {e.Message}");
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
        var userName = message.From?.FirstName ?? "User";

        _logger.LogInformation($"Recieved the message from {userName}: {messageText}");

        switch (messageText.Split(' ')[0])
        {
            case "/start":
                await botClient.SendMessage(
                    chatId: chatId,
                    text: _botConfigurations.BotMessages.StartMessage,
                    cancellationToken: cancellationToken);
                break;
            case "/help":
                await botClient.SendMessage(
                    chatId: chatId,
                    text: _botConfigurations.BotMessages.HelpMessage,
                    cancellationToken: cancellationToken);
                break;
            
            case "/me":
                var id = message.From.Id;
                var firstName = message.From.FirstName;
                var lastName = message.From.LastName ?? string.Empty;
                var username = message.From.Username ?? string.Empty;

                StringBuilder text = new StringBuilder();

                text.Append(
                    _botConfigurations.BotMessages.MyInformationMessage +
                    "\nДанные об вашем аккаунте Telegram\n" +
                    $"id: {id}\n" +
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

    public async Task SendMessageToUsers(
        RequestDto dto,
        ITelegramBotClient botClient,
        CancellationToken cancellationToken)
    {
        try
        {
            foreach (var user in dto.TelegramIds)
            {
                await botClient.SendMessage(
                    chatId: user,
                    text: dto.Message,
                    cancellationToken: cancellationToken);
            
                _logger.LogInformation($"The message sent to user: {user}");
            }
        }
        catch (ApiRequestException e)
        {
            _logger.LogInformation($"Error with sending the message" + e.Message);
        }
    }
    
    private async Task OnCallbackQueryReceived(
        ITelegramBotClient botClient,
        CallbackQuery callbackQuery, 
        CancellationToken cancellationToken)
    {
        // TODO:
    }

    private async Task OnInlineQueryReceived(
        ITelegramBotClient botClient,
        InlineQuery inlineQuery,
        CancellationToken cancellationToken)
    {
        // TODO: 
    }

    private Task OnUnknownUpdate(
        ITelegramBotClient botClient,
        Update update,
        CancellationToken cancellationToken)
    {
        // TODO:
        return Task.CompletedTask;
    }
    
}