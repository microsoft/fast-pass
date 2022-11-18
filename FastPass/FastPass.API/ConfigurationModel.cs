namespace FastPass.Api;

public class ConfigurationModel
{
    public const string Section = "APIConfig";
    public string TextAnalyticsBase { get; set; } = string.Empty;
    public string TextAnalyticsKey { get; set; } = string.Empty;

    public string FhirScope { get; set; } = string.Empty;
    public string FhirServerUri { get; set; } = string.Empty;
    public string Authority { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;

}