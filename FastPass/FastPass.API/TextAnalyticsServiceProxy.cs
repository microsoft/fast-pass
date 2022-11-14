using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using FastPass.Models;
using ResourceType = Hl7.Fhir.Model.ResourceType;
using System.Collections.Generic;
using System.Net.Http;
using FastPass.API.TextAnalyticsModels;

namespace FastPass.API
{
    public static class TextAnalyticsServiceProxy
    {
        private static HttpClient _client = new HttpClient();

        [FunctionName("TestAnalyticsServiceProxy")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string bodyString;
            using(var sr = new StreamReader(req.Body))
            {
                bodyString = await sr.ReadToEndAsync();
            }

            var proxyRequest = JsonConvert.DeserializeObject<TextAnalyticsProxyRequest>(bodyString);

            var textRequest = new TextAnalyticsRequest();

            // TODO: Move stuff to configs
            _client.BaseAddress = new Uri("https://yoururl.cognitiveservices.azure.com");
            var result = await _client.PostAsJsonAsync<TextAnalyticsRequest>("/language/analyze-text/jobs?api-version=2022-05-15-preview", textRequest);



            return new OkResult();
        }
    }
}
