namespace SkadaTelegramBot_.DTOs;

public record GetMessageDto(IList<long> TelegramUserIds, string Message);