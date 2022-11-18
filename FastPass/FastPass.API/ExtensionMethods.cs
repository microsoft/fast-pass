using Microsoft.Azure.Functions.Worker.Http;

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
    }
}
