using FastPass.API.Services;
using FastPass.API.TextAnalyticsModels;
using FastPass.API;
using FastPass.Models;
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
    private readonly IFirelyService _fhirService;
    private readonly ILogger _logger;   

    public TextAnalyticsServiceProxyFunction(
        IOptions<ConfigurationModel> config,
        ILoggerFactory loggerFactory,
        HttpClient client, 
        JsonSerializerSettings jsonSettings, 
        IFirelyService fhirService)
    {
        _client = client;
        _textAnalyticsKey = config.Value.TextAnalyticsKey;
        _jsonsettings = jsonSettings;
        _fhirService = fhirService;
        _logger = loggerFactory.CreateLogger<TextAnalyticsServiceProxyFunction>();
    }

    [Function("TestingFetchPatient")]
    public async Task<HttpResponseData> RunTestPatientAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequestData req)
    {
        try
        {
            var patients = await _fhirService.GetPatientsAsync();
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(patients.FirstOrDefault());

            return response;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    [Function("TextAnalyticsServiceProxy")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestData req)
    {
        string bodyString;
        using (var sr = new StreamReader(req.Body))
        {
            bodyString = await sr.ReadToEndAsync();
        }

        var proxyRequest = JsonConvert.DeserializeObject<TextAnalyticsProxyRequest>(bodyString);
        var documentId = string.IsNullOrWhiteSpace(proxyRequest.Id) ? Guid.NewGuid().ToString() : proxyRequest.Id;

        try
        {
            HttpResponseMessage result;
            using (var request = new HttpRequestMessage(HttpMethod.Post, "/language/analyze-text/jobs?api-version=2022-05-15-preview"))
            {
                request.Headers.TryAddWithoutValidation(SUBSCRIPTION_HEADER_NAME, _textAnalyticsKey);
                request.Content = new StringContent(JsonConvert.SerializeObject(new TextAnalyticsRequest(proxyRequest.TextToAnalyze, language: proxyRequest.Language, documentId: documentId), _jsonsettings));
                request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                _logger.Log(LogLevel.Trace, $"Calling TextAnalytics for documentId {documentId}");
                result = await _client.SendAsync(request);
            }

            if (result == null || !result.IsSuccessStatusCode || !result.Headers.Contains(OPERATION_LOCATION_HEADER))
            {
                var message = $"TextAnalytics (docId: {documentId}) call failed with {result.StatusCode}.";
                _logger.Log(LogLevel.Warning, message);

                var err = req.CreateResponse(result.StatusCode);
                err.WriteString(message);
                return err;
            }

            var callbackLocation = new Uri(result.Headers.GetValues(OPERATION_LOCATION_HEADER).FirstOrDefault());

            _logger.Log(LogLevel.Warning, $"TextAnalytics (docId: {documentId}) call succeeded with a callback location of {callbackLocation}.");

            string requestStatus;
            TextAnalyticsResponse responseObj;
            do
            {
                Thread.Sleep(_requestDelay);

                using (var request = new HttpRequestMessage(HttpMethod.Get, callbackLocation.PathAndQuery))
                {
                    request.Headers.TryAddWithoutValidation(SUBSCRIPTION_HEADER_NAME, _textAnalyticsKey);
                    result = await _client.SendAsync(request);
                }

                var strResponse = await result.Content.ReadAsStringAsync();

                responseObj = JsonConvert.DeserializeObject<TextAnalyticsResponse>(strResponse);
                requestStatus = responseObj.Status;
                _logger.Log(LogLevel.Warning, $"Checked TextAnalytics job for (docId: {documentId}) current status is {requestStatus}.");

            } while (requestStatus == RUNNING_STATUS);

            _logger.Log(LogLevel.Warning, $"TextAnalytics (docId: {documentId}) completed successfully, returning FhirBundle.");

            var resp = req.CreateResponse(HttpStatusCode.OK);
            await resp.WriteAsJsonAsync(responseObj.Tasks?.Items?.First()?.Results?.Documents?.First()?.FhirBundle);
            return resp;
        }
        catch (Exception ex)
        {
            var msg = $"TextAnalytics (docId: {documentId}) exception caught. Detail: {ex}";
            _logger.Log(LogLevel.Error, msg);

            var resp = req.CreateResponse(HttpStatusCode.BadRequest);
            resp.WriteString(msg);
            return resp;
        }

    }
}
