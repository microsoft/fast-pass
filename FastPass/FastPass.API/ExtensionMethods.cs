using Hl7.Fhir.Rest;
using Microsoft.Azure.Functions.Worker.Http;
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
    }
}
