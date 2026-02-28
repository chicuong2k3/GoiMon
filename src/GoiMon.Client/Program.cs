using GoiMon.Client;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorBlueprint.Components;
using EasyAppDev.Blazor.Store.Blazor;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services
    .AddGoiMonClient()
    .ConfigureHttpClient(client => client.BaseAddress = new Uri("http://localhost:5000/graphql"));

// Register Blazor Blueprint services (primitives, toast, dialog)
builder.Services.AddBlazorBlueprintComponents();

builder.Services.AddStoreUtilities();

await builder.Build().RunAsync();
