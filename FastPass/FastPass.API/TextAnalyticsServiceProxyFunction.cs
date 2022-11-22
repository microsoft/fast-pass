using FastPass.API;
using FastPass.API.TextAnalyticsModels;
using FastPass.Models;
using Hl7.Fhir.Rest;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;

namespace FastPass.Api;

public class TextAnalyticsServiceProxyFunction
{
    private const string OPERATION_LOCATION_HEADER = "operation-location";
    private const string SUBSCRIPTION_HEADER_NAME = "Ocp-Apim-Subscription-Key";
    private const string RUNNING_STATUS = "running";
    private static readonly TimeSpan _requestDelay = TimeSpan.FromSeconds(2);
    private static string _textAnalyticsKey;
    private static HttpClient _client;
    private static JsonSerializerSettings _jsonsettings;
    private readonly ILogger _logger;

    public TextAnalyticsServiceProxyFunction(
        IOptions<ConfigurationModel> config,
        ILoggerFactory loggerFactory,
        HttpClient client,
        JsonSerializerSettings jsonSettings)
    {
        _client = client;
        _textAnalyticsKey = config.Value.TextAnalyticsKey;
        _jsonsettings = jsonSettings;
        _logger = loggerFactory.CreateLogger<TextAnalyticsServiceProxyFunction>();
    }

    [Function("TextAnalyticsServiceProxy")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestData req)
    {
        using var sr = new StreamReader(req.Body);

        var bodyString = await sr.ReadToEndAsync();

        var proxyRequest = JsonConvert.DeserializeObject<TextAnalyticsProxyRequest>(bodyString);

        var documentId = string.IsNullOrWhiteSpace(proxyRequest.Id) ? Guid.NewGuid().ToString() : proxyRequest.Id;

        try
        {
            HttpResponseMessage result;

            using var jobStartReq = new HttpRequestMessage(HttpMethod.Post, "/language/analyze-text/jobs?api-version=2022-05-15-preview");

            jobStartReq.Headers.TryAddWithoutValidation(SUBSCRIPTION_HEADER_NAME, _textAnalyticsKey);

            jobStartReq.Content = new StringContent(JsonConvert.SerializeObject(new TextAnalyticsRequest(proxyRequest.TextToAnalyze, language: proxyRequest.Language, documentId: documentId), _jsonsettings));

            jobStartReq.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            _logger.LogInformation("Calling TextAnalytics for documentId {documentId}", documentId);

            result = await _client.SendAsync(jobStartReq);

            if (!(result?.IsSuccessStatusCode ?? false) || !result.Headers.Contains(OPERATION_LOCATION_HEADER))
            {
                var message = $"TextAnalytics (docId: {documentId}) call failed with {result.StatusCode}.";

                _logger.LogWarning(message);

                return await CreateResponseAsync(req, result.StatusCode, message);
            }

            var callbackLocation = new Uri(result.Headers.GetValues(OPERATION_LOCATION_HEADER).FirstOrDefault());

            _logger.LogInformation("TextAnalytics (docId: {documentId}) call succeeded with a callback location of {callbackLocation}.", documentId, callbackLocation);

            string requestStatus;

            TextAnalyticsResponse responseObj;

            do
            {
                Thread.Sleep(_requestDelay);

                using var jobStatusReq = new HttpRequestMessage(HttpMethod.Get, callbackLocation.PathAndQuery);

                jobStatusReq.Headers.TryAddWithoutValidation(SUBSCRIPTION_HEADER_NAME, _textAnalyticsKey);

                result = await _client.SendAsync(jobStatusReq);

                var strResponse = await result.Content.ReadAsStringAsync();

                responseObj = JsonConvert.DeserializeObject<TextAnalyticsResponse>(strResponse);

                requestStatus = responseObj.Status;

                _logger.LogInformation("Checked TextAnalytics job for (docId: {documentId}) current status is {requestStatus}.", documentId, requestStatus);

            } while (requestStatus == RUNNING_STATUS);

            _logger.LogInformation("TextAnalytics (docId: {documentId}) completed successfully, returning FhirBundle.", documentId);

            var fhirBundle = responseObj.Tasks?.Items?.FirstOrDefault()?.Results?.Documents?.FirstOrDefault()?.FhirBundle.ToString();

            return await CreateResponseAsync(req, HttpStatusCode.OK, fhirBundle);
        }
        catch (Exception ex)
        {
            var msg = $"TextAnalytics (docId: {documentId}) exception caught. Detail: {ex}";

            _logger.LogError(msg);

            return await CreateResponseAsync(req, HttpStatusCode.BadRequest, msg);
        }
    }


    private async Task<HttpResponseData> CreateResponseAsync(HttpRequestData req, HttpStatusCode responseCode, string body)
    {
        var resp = req.CreateResponse(HttpStatusCode.BadRequest);

        await resp.WriteStringAsync(body);

        if (responseCode.IsSuccessful())
            resp.Headers.Add("Content-Type", "application/json");

        return resp;
    }
}
