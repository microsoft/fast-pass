using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

namespace FastPass.API
{
    public static class ExtensionMethods
    {
        public static async Task<string> GetBodyString(this HttpRequest req)
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
