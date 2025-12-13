namespace SkadaTelegramBot_.DTOs;

public class RequestDto
{
    public string Message { get; set; }
    public IList<long> TelegramIds { get; set; }
}