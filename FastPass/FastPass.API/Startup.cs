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
            .AddEnvironmentVariables();

    }

    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddOptions<ConfigurationModel>()
            .Configure<IConfiguration>((settings, configuration) =>
            {
                configuration.GetSection(ConfigurationModel.Section).Bind(settings);
            });

        builder.Services.AddScoped(c =>
        {
            var configSvc = c.GetService<IConfiguration>();
            ConfigurationModel model = new ConfigurationModel();
            configSvc.GetSection(ConfigurationModel.Section).Bind(model);
            return new HttpClient
            {
                BaseAddress = new Uri(model.TextAnalyticsBase)
            };
        });
    }
}