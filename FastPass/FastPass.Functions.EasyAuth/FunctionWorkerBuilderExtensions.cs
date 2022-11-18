using Microsoft.Azure.Functions.Worker.Extensions.StaticWebAppAuth;
using Microsoft.Azure.Functions.Worker;

namespace Microsoft.Extensions.Hosting;

public static class FunctionWorkerBuilderExtensions
{
    public static IFunctionsWorkerApplicationBuilder AddAppAuthentication(this IFunctionsWorkerApplicationBuilder builder)
    {
        builder.UseMiddleware<AppAuthenticationMiddleware>();

        return builder;
    }

    public static IFunctionsWorkerApplicationBuilder AddAppAuthentication(this IFunctionsWorkerApplicationBuilder builder, Func<bool> isDev)
    {
        builder.UseWhen<AppAuthenticationMiddleware>(context =>
        {
            return isDev.Invoke();
        });

        return builder;
    }
}
