namespace SkadaTelegramBot_.Configurations;

public class BotConfigurations
{
    public string? BotToken { get; init; }
    public string? HostAddress { get; init; }
    public int IntervalOfUpdateServerLifeNotifier { get; init; } = 10;
    public int IntervalOfUpdateOpcMotion { get; init; } = 5;
    public int IntervalOfUpdateTelegramBot { get; init; } = 1;
    public NodeIds? NodeIds { get; set; }
    public BotMessages? BotMessages { get; set; }
}