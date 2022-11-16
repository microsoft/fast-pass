using FastPass.API.TextAnalyticsModels;
using FastPass.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace FastPass.API
{
    public class TextAnalyticsServiceProxyFunction
    {
        private static HttpClient _client;
        private static string OPERATION_LOCATION_HEADER = "operation-location";
        private static string SUBSCRIPTION_HEADER_NAME = "Ocp-Apim-Subscription-Key";
        private static string RUNNING_STATUS = "running";
        private static TimeSpan REQUEST_DELAY = TimeSpan.FromSeconds(5);
        private static string _textAnalyticsKey;

        public TextAnalyticsServiceProxyFunction(Configuration config, HttpClient client)
        {
            _client = client;
            _textAnalyticsKey = config.TextAnalyticsKey;
        }


        [FunctionName("TestAnalyticsServiceProxy")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req, ILogger log)
        {
            // Check if user is authenticated
            var cp = StaticWebAppsAuth.Parse(req);
            if (!cp.Identity.IsAuthenticated)
                return new UnauthorizedResult();

            string bodyString;
            using(var sr = new StreamReader(req.Body))
            {
                bodyString = await sr.ReadToEndAsync();
            }

            // TODO Move to startup
            var jsonSettings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                },
                Formatting = Formatting.Indented
            };

            var proxyRequest = JsonConvert.DeserializeObject<TextAnalyticsProxyRequest>(bodyString);

            HttpResponseMessage result;
            using (var request = new HttpRequestMessage(HttpMethod.Post, "/language/analyze-text/jobs?api-version=2022-05-15-preview"))
            {
                request.Headers.TryAddWithoutValidation(SUBSCRIPTION_HEADER_NAME, _textAnalyticsKey);
                request.Content = new StringContent(JsonConvert.SerializeObject(new TextAnalyticsRequest(proxyRequest.TextToAnalize, documentId: String.IsNullOrWhiteSpace(proxyRequest.Id) ? Guid.NewGuid().ToString() : proxyRequest.Id), jsonSettings));
                request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                result = await _client.SendAsync(request);
            }

            if(result == null || !result.IsSuccessStatusCode || !result.Headers.Contains(OPERATION_LOCATION_HEADER))
                return new StatusCodeResult((int) result.StatusCode);

            var callbackLocation = new Uri(result.Headers.GetValues(OPERATION_LOCATION_HEADER).FirstOrDefault());
            
            string requestStatus;
            TextAnalyticsResponse responseObj;
            do
            {
                Thread.Sleep(REQUEST_DELAY);

                using (var request = new HttpRequestMessage(HttpMethod.Get, callbackLocation.PathAndQuery))
                {
                    request.Headers.TryAddWithoutValidation(SUBSCRIPTION_HEADER_NAME, _textAnalyticsKey);
                    result = await _client.SendAsync(request);
                }

                responseObj = JsonConvert.DeserializeObject<TextAnalyticsResponse>(await result.Content.ReadAsStringAsync());
                requestStatus = responseObj.Status;
            } while (requestStatus == RUNNING_STATUS);

            return new OkObjectResult(responseObj.Tasks?.Items?.First()?.Results?.Documents?.First()?.FhirBundle);
        }
    }
}
