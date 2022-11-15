namespace FastPass.API;

public class Configuration
{
    public const string ConfigSection = "ContainerConfiguration";

    public string TextAnalyticsBase { get; set; } = string.Empty;
    public string TextAnalyticsKey { get; set; } = string.Empty;

}