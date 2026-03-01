using GoiMon.Client;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using BlazorBlueprint.Components;
using EasyAppDev.Blazor.Store.Blazor;
using Blazored.LocalStorage;
using GoiMon.Client.State;
using GoiMon.Client.Features.Authentication.Services;
using GoiMon.Client.Features.ImageUpload.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

var graphqlEndpoint = builder.Configuration["GraphQL:Endpoint"];
if (string.IsNullOrWhiteSpace(graphqlEndpoint))
{
    graphqlEndpoint = builder.HostEnvironment.IsDevelopment()
        ? "http://localhost:5000/graphql"
        : new Uri(new Uri(builder.HostEnvironment.BaseAddress), "graphql").ToString();
}

builder.Services
    .AddGoiMonClient()
    .ConfigureHttpClient(client => client.BaseAddress = new Uri(graphqlEndpoint));

// Register Blazor Blueprint services (primitives, toast, dialog)
builder.Services.AddBlazorBlueprintComponents();

// Register authentication services
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<ITokenStorageService, TokenStorageService>();
builder.Services.AddScoped<IImageUploadService, ImageUploadService>();
builder.Services.AddScoped<AuthenticationStateProvider, GoimonAuthenticationStateProvider>();

builder.Services.AddStoreUtilities();
builder.Services.AddScopedStoreWithUtilities(UiCacheState.Initial, (store, _) => store);

await builder.Build().RunAsync();
