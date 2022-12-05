using Azure.AI.TextAnalytics;
using Hl7.Fhir.Rest;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;

namespace FastPass.API.Functions;

public class TextAnalyticsServiceProxyFunction
{
    private static TextAnalyticsClient _client;
    private readonly ILogger _logger;

    public TextAnalyticsServiceProxyFunction(
        ILoggerFactory loggerFactory,
        TextAnalyticsClient client)
    {
        _client = client;
        _logger = loggerFactory.CreateLogger<TextAnalyticsServiceProxyFunction>();
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
                return await CreateResponseAsync(req, HttpStatusCode.BadRequest, msg);
            }

            var results = healthOps.GetValues();
            var fhirResults = results.SelectMany(p => p.Select(d => d.FhirBundle)).FirstOrDefault();
            return await CreateResponseAsync(req, HttpStatusCode.OK, JsonConvert.SerializeObject(fhirResults));

        }
        catch (Exception ex)
        {
            var msg = $"TextAnalytics exception caught. Detail: {ex}";
            _logger.LogError(msg);

            return await CreateResponseAsync(req, HttpStatusCode.BadRequest, msg);
        }
    }


    private async Task<HttpResponseData> CreateResponseAsync(HttpRequestData req, HttpStatusCode responseCode, string body)
    {
        var resp = req.CreateResponse(responseCode);

        await resp.WriteStringAsync(body);

        if (responseCode.IsSuccessful())
            resp.Headers.Add("Content-Type", "application/json");

        return resp;
    }
}
