using FastPass.API;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;

[assembly: FunctionsStartup(typeof(Startup))]
namespace FastPass.API;

public class Startup : FunctionsStartup
{
    public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
    {
        FunctionsHostBuilderContext context = builder.GetContext();

        builder.ConfigurationBuilder
            .SetBasePath(context.ApplicationRootPath)
            .AddEnvironmentVariables()            ;

    }

    public override void Configure(IFunctionsHostBuilder builder)
    {
        var config = new Configuration
        {
            TextAnalyticsBase = Environment.GetEnvironmentVariable("TextAnalyticsBase"),
            TextAnalyticsKey = Environment.GetEnvironmentVariable("TextAnalyticsKey"),
        };

        builder.Services.AddSingleton(config);
        builder.Services.AddSingleton(new HttpClient
        {
            BaseAddress = new Uri(config.TextAnalyticsBase)
        });
    }
}