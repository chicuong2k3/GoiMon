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

var appBaseUri = new Uri(builder.HostEnvironment.BaseAddress);
var configuredGraphqlEndpoint = builder.Configuration["GraphQL:Endpoint"]?.Trim();
var graphqlEndpoint = string.IsNullOrWhiteSpace(configuredGraphqlEndpoint)
    ? new Uri(appBaseUri, "graphql").ToString()
    : new Uri(appBaseUri, configuredGraphqlEndpoint).ToString();

var configuredGraphqlWsEndpoint = builder.Configuration["GraphQL:WebSocketEndpoint"]?.Trim();
var graphqlWebSocketEndpoint = string.IsNullOrWhiteSpace(configuredGraphqlWsEndpoint)
    ? graphqlEndpoint.Replace("https://", "wss://").Replace("http://", "ws://")
    : new Uri(appBaseUri, configuredGraphqlWsEndpoint).ToString();

if (!builder.HostEnvironment.IsDevelopment()
    && Uri.TryCreate(graphqlEndpoint, UriKind.Absolute, out var endpointUri)
    && endpointUri.IsLoopback)
{
    graphqlEndpoint = new Uri(appBaseUri, "graphql").ToString();
}

builder.Services
    .AddGoiMonClient()
    .ConfigureHttpClient(client => client.BaseAddress = new Uri(graphqlEndpoint))
    .ConfigureWebSocketClient(client => client.Uri = new Uri(graphqlWebSocketEndpoint));

// Register Blazor Blueprint services (primitives, toast, dialog)
builder.Services.AddBlazorBlueprintComponents();

// Register authentication services
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<ITokenStorageService, TokenStorageService>();
builder.Services.AddScoped<IImageUploadService, ImageUploadService>();
builder.Services.AddScoped<AuthenticationStateProvider, GoimonAuthenticationStateProvider>();

builder.Services.AddStoreUtilities();
builder.Services.AddScopedStoreWithUtilities(CategoriesUiState.Initial, (store, _) => store);
builder.Services.AddScopedStoreWithUtilities(ProductsUiState.Initial, (store, _) => store);
builder.Services.AddScopedStoreWithUtilities(CombosUiState.Initial, (store, _) => store);
builder.Services.AddScopedStoreWithUtilities(OrdersUiState.Initial, (store, _) => store);
builder.Services.AddScopedStoreWithUtilities(CheckoutUiState.Initial, (store, _) => store);
builder.Services.AddScopedStoreWithUtilities(EmployeesUiState.Initial, (store, _) => store);

await builder.Build().RunAsync();
