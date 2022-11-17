namespace FastPass.API;

public class ConfigurationModel
{
    public const string Section = "APIConfig";
    public string TextAnalyticsBase { get; set; } = string.Empty;
    public string TextAnalyticsKey { get; set; } = string.Empty;

    public string FhirScope { get; set; } = string.Empty;
    public string FhirServerUri { get; set; } = string.Empty;
    public string Authority { get; set; } = string.Empty;
}