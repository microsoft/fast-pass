using Azure.AI.TextAnalytics;
using Azure.Identity;
using FastPass.Models;
using Hl7.Fhir.Rest;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;

namespace FastPass.API.Functions;

public class TextAnalyticsServiceProxyFunction
{
    private static TextAnalyticsClient _client;
    private readonly ILogger _logger;
    private readonly ConfigurationModel _settings;

    public TextAnalyticsServiceProxyFunction(
        IOptions<ConfigurationModel> options,
        ILoggerFactory loggerFactory,
        TextAnalyticsClient client)
    {
        _client = client;
        _logger = loggerFactory.CreateLogger<TextAnalyticsServiceProxyFunction>();
        _settings = options.Value;
    }

    [Function("TextAnalyticsServiceProxy")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestData req)
    {

        using var sr = new StreamReader(req.Body);
        var bodyString = await sr.ReadToEndAsync();

        try
        {
            // configure TextAnalytics options to include FhirBundle in results
            var options = new AnalyzeHealthcareEntitiesOptions
            {
                FhirVersion = WellKnownFhirVersion.V4_0_1
            };
            var healthOps = await _client.StartAnalyzeHealthcareEntitiesAsync(new[] { bodyString }, options: options);

            await healthOps.WaitForCompletionAsync();

            if (!healthOps.HasValue)
            {
                var msg = $"No values returned from Text Analytics.";
                _logger.LogWarning(msg);
                return await req.CreateResponseAsync(HttpStatusCode.BadRequest, msg);
            }

            var results = healthOps.GetValues().SelectMany(p => p.Select(r => r));

            return await req.CreateResponseAsync(HttpStatusCode.OK, JsonConvert.SerializeObject(results));

        }
        catch (Exception ex)
        {
            var msg = $"TextAnalytics exception caught. Detail: {ex}";
            _logger.LogError(msg);

            return await req.CreateResponseAsync(HttpStatusCode.BadRequest, msg);
        }
    }
}
