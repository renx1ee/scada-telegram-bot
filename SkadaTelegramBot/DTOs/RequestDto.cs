namespace SkadaTelegramBot_.DTOs;

public class RequestDto
{
    public Guid Id { get; set; }
    public string Message { get; set; }
    public IList<long> TelegramIds { get; set; }
}