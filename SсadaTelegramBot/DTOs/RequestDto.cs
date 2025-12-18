namespace SkadaTelegramBot_.DTOs;

public class RequestDto
{
    public int Id { get; set; }
    public string Message { get; set; } = String.Empty;
    public IList<long> TelegramIds { get; set; }
    public uint? SystemId { get; set; }
    public string? DeviceId { get; set; }
}