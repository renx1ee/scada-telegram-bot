namespace SkadaTelegramBot_.Configurations;

public class BotOptions
{
    public string? BotToken { get; init; }
    public string? HostAddress { get; init; }
    public string? NotificationNodeId { get; init; }
    public string? NotificationResponseNodeId { get; init; }
    public string? ErrorNodeId { get; init; }
    public TimeSpan IntervalOfUpdate { get; init; }
}