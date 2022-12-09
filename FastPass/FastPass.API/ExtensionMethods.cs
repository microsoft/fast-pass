using Azure.AI.TextAnalytics;
using FastPass.Models;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Serialization;
using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;
using System.Net;

namespace FastPass.API
{
    public static class ExtensionMethods
    {
        public static async Task<string> GetBodyString(this HttpRequestData req)
        {
            string bodyString;

            using (var sr = new StreamReader(req.Body))
            {
                bodyString = await sr.ReadToEndAsync();
            }

            return bodyString;
        }

        public static async Task<HttpResponseData> CreateResponseAsync(this HttpRequestData req, HttpStatusCode responseCode, string body)
        {
            var resp = req.CreateResponse(responseCode);

            await resp.WriteStringAsync(body);

            if (responseCode.IsSuccessful())
                resp.Headers.Add("Content-Type", "application/json");

            return resp;
        }

        public static Models.TextAnalyticsResult ToTextAnalyticsResult(this AnalyzeHealthcareEntitiesResult r, FhirJsonParser parser = null)
        {
            parser ??= new FhirJsonParser();

            var entities = r.Entities.Select(e => new Entity
            {
                NormalizedText = e?.NormalizedText,
                Text = e?.Text,
                ConfidenceScore = e.ConfidenceScore,
                Offset = e.Offset,
                Length = e.Length,
                Assertion = new Assertion
                {
                    Association = e?.Assertion?.Association?.ToString(),
                    Certainty = e?.Assertion?.Certainty?.ToString(),
                    Conditionality = e?.Assertion?.Conditionality?.ToString()
                },
                Links = e.DataSources.Select(ds => new Link
                {
                    Id = ds?.EntityId,
                    DataSource = ds?.Name
                }).ToList(),
                Category = e?.Category.ToString()
            }).ToList();

            var json = JsonConvert.SerializeObject(r.FhirBundle);
            
            return new Models.TextAnalyticsResult
            {
                Entities = entities,
                FhirBundle = json
            };
        }
    }
}
