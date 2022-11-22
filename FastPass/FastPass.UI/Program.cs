using AzureStaticWebApps.Blazor.Authentication;
using Blazored.SessionStorage;
using FastPass.UI;
using FastPass.UI.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var baseUrl = builder.HostEnvironment.BaseAddress;

builder.Services.AddScoped(sp => {
    return new HttpClient { BaseAddress = new Uri(baseUrl) };
});
builder.Services.AddScoped<ApiService>();

builder.Services.AddBlazoredSessionStorage();
builder.Services.AddStaticWebAppsAuthentication();

await builder.Build().RunAsync();
