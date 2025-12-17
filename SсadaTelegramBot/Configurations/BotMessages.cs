namespace SkadaTelegramBot_.Configurations;

public class BotMessages
{
    public string? StartMessage { get; set; } = 
        "Вас приветствует мастер скада бот! \nОтправте /help что бы получить список доступных комманд!";
    public string? HelpMessage { get; set; } = 
        "Список всех команд: \n/start\n/help\n/me\n";
    public string? MyInformationMessage { get; set; } = 
        "Перешлите это сообщение администратору в Telegram\n";

    public string? UnknownCommand { get; set; } =
        "Команда не распознана! \nОтправте /help что бы получить список доступных комманд!";
}