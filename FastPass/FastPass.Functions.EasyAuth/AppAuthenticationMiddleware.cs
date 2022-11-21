using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.Azure.Functions.Worker.Extensions.StaticWebAppAuth;

public class AppAuthenticationMiddleware
    : IFunctionsWorkerMiddleware
{
    private readonly ILogger<AppAuthenticationMiddleware> logger;

    public AppAuthenticationMiddleware(ILogger<AppAuthenticationMiddleware> logger)
    {
        this.logger = logger;
    }
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {

        if (!context.IsHttpTriggerFunction())
        {
            await next(context);
        }
        else
        {
            var headers = context.BindingContext.BindingData["Headers"]?.ToString();
            var httpHeaders = JsonSerializer.Deserialize<HttpHeaders>(headers!);
            var principal = new ClientPrincipal();
            
            if (!string.IsNullOrEmpty(httpHeaders?.ClientPrincipal))
            {
                //Validation logic for your token.
                var decoded = Convert.FromBase64String(httpHeaders.ClientPrincipal);
                var json = Encoding.UTF8.GetString(decoded);
                principal = JsonSerializer.Deserialize<ClientPrincipal>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                principal!.UserRoles = principal.UserRoles.Except(new string[] { "anonymous" }, StringComparer.CurrentCultureIgnoreCase);

                if (!principal.UserRoles?.Any() ?? true)
                {
                    await context.CreateJsonResponse(System.Net.HttpStatusCode.Unauthorized, new { Message = "Invalid user role claims." });
                }

                await next(context);
            }
            else
            {
                await context.CreateJsonResponse(System.Net.HttpStatusCode.Unauthorized, new { Message = "Request was not authenticated." });
            }
        }        
    }

    private class HttpHeaders
    {
        public string Authorization { get; set; } = default!;

        [JsonPropertyName("x-ms-client-principal")]
        public string ClientPrincipal { get; set; } = default!;
    }
}

