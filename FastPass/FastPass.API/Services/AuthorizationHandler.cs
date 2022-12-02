using Azure.Core;
using Azure.Identity;
using System.Net.Http.Headers;

namespace FastPass.API.Services;

public class AuthorizationHandler : HttpClientHandler
{
    internal string[] Scopes;
    internal string TenantId;
    internal string ClientId;
    internal string ClientSecret;

    public AuthenticationHeaderValue Authorization { get; set; }
    protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var ctx = new TokenRequestContext(Scopes);
        var clientSecretCredential = new ClientSecretCredential(TenantId, ClientId, ClientSecret);
        var ccToken = await clientSecretCredential.GetTokenAsync(ctx, cancellationToken);

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ccToken.Token);

        return await base.SendAsync(request, cancellationToken);
    }
}
