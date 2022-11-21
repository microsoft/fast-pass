using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.Azure.Functions.Worker.Extensions.StaticWebAppAuth;

public class AppAuthenticationMiddleware
    : IFunctionsWorkerMiddleware
{
    private const string INVALID_ROLES_MESSAGE = "Invalid user role claims.";
    private const string NOT_ATHENTICATED_MESSAGE = "Request was not authenticated.";

    private readonly ILogger<AppAuthenticationMiddleware> logger;

    public AppAuthenticationMiddleware(ILoggerFactory logFactory)
    {
        logger = logFactory.CreateLogger<AppAuthenticationMiddleware>();
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
                    logger.LogWarning(INVALID_ROLES_MESSAGE);
                    await context.CreateJsonResponse(System.Net.HttpStatusCode.Unauthorized, new { Message = INVALID_ROLES_MESSAGE });
                }

                await next(context);
            }
            else
            {
                logger.LogWarning(NOT_ATHENTICATED_MESSAGE);
                await context.CreateJsonResponse(System.Net.HttpStatusCode.Unauthorized, new { Message = NOT_ATHENTICATED_MESSAGE });
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

