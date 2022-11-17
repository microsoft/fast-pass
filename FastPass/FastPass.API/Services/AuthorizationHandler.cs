using Azure.Core;
using Azure.Identity;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace FastPass.API.Services;

public class AuthorizationHandler : HttpClientHandler
{
    private static DefaultAzureCredential _credentials = new DefaultAzureCredential();
    internal string[] Scopes;

    public AuthenticationHeaderValue Authorization { get; set; }
    protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var ctx = new TokenRequestContext(Scopes);

        var token = await _credentials.GetTokenAsync(ctx);

        Console.Write(token.ToString());

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);

        return await base.SendAsync(request, cancellationToken);
    }
}
