using FastPass.API.Services;
using FastPass.API;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using Microsoft.Extensions.DependencyInjection;
using Hl7.Fhir.Rest;


var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults((ctx, builder) =>
    {                
        builder.AddAppAuthentication(() =>
        {
            // function must return a boolean which will determine if middleware is used or not
            return string.IsNullOrEmpty(ctx.Configuration["IsDevelopment"]) 
                ? true 
                : !bool.Parse(ctx.Configuration["IsDevelopment"]);
        });
    })
    .ConfigureServices((ctx, services) =>
    {
        services.AddOptions<ConfigurationModel>()
            .Configure<IConfiguration>((settings, configuration) =>
            {
                configuration.GetSection(ConfigurationModel.Section).Bind(settings);
            });

        services.AddScoped<IFirelyService>(c =>
        {
            var configSvc = c.GetService<IConfiguration>();

            var model = new ConfigurationModel();

            configSvc.GetSection(ConfigurationModel.Section).Bind(model);

            var handler = new AuthorizationHandler
            {
                Scopes = new string[] { model.FhirScope },
                TenantId = model.TenantId,
                ClientId = model.ClientId,
                ClientSecret = model.ClientSecret
            };

            var fhirClient = new FhirClient(new Uri(model.FhirServerUri), new FhirClientSettings
            {
                PreferredFormat = ResourceFormat.Json,
                PreferredReturn = Prefer.ReturnMinimal
            }, handler);

            return new FirelyService(fhirClient);
        });

        services.AddScoped(c =>
        {
            var configSvc = c.GetService<IConfiguration>();
            var model = new ConfigurationModel();
            configSvc.GetSection(ConfigurationModel.Section).Bind(model);
            return new HttpClient
            {
                BaseAddress = new Uri(model.TextAnalyticsBase)
            };
        });

        services.AddSingleton(new JsonSerializerSettings()
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            },
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore
        });
    })
    .Build();

host.Run();
