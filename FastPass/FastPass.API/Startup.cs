using FastPass.API;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

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
        // TODO: Borrowed from elsewhere, remove what's not necessary

        var containerConfig = new ContainerConfiguration
        {
            ClientId = Environment.GetEnvironmentVariable("ContainerConfiguration:ClientId"),
            ClientSecret = Environment.GetEnvironmentVariable("ContainerConfiguration:ClientSecret"),
            TenantId = Environment.GetEnvironmentVariable("ContainerConfiguration:TenantId"),
            SubscriptionId = Environment.GetEnvironmentVariable("ContainerConfiguration:SubscriptionId"),
            ResourceGroupName = Environment.GetEnvironmentVariable("ContainerConfiguration:ResourceGroupName"),
            StorageName = Environment.GetEnvironmentVariable("ContainerConfiguration:StorageName"),
            StorageKey = Environment.GetEnvironmentVariable("ContainerConfiguration:StorageKey"),
            BatchRegion = Environment.GetEnvironmentVariable("ContainerConfiguration:BatchRegion"),
            BatchAccountName = Environment.GetEnvironmentVariable("ContainerConfiguration:BatchAccountName"),
            BatchKey = Environment.GetEnvironmentVariable("ContainerConfiguration:BatchKey")
        };

        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings:DefaultConnection");

        builder.Services.AddSingleton(containerConfig);

        //builder.Services.AddScoped(options =>
        //{
        //    var tokenProvider = new ClientSecretCredential(containerConfig.TenantId, containerConfig.ClientId, containerConfig.ClientSecret);
        //    var creds = new TokenCredentials(tokenProvider.GetToken(new TokenRequestContext(new[] { "https://management.azure.com/.default" })).Token);

        //    return new ContainerInstanceManagementClient(creds)
        //    {
        //        SubscriptionId = containerConfig.SubscriptionId
        //    };
        //});
    }
}