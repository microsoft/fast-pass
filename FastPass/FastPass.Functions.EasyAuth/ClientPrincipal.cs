namespace Microsoft.Azure.Functions.Worker.Extensions.StaticWebAppAuth;

internal class ClientPrincipal
{
    public string IdentityProvider { get; set; } = default!;
    public string UserId { get; set; } = default!;
    public string UserDetails { get; set; } = default!;
    public IEnumerable<string> UserRoles { get; set; } = default!;
}
