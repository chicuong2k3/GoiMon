using GoiMon.Portal.Components;
using GoiMon.Portal.Features.Authentication;
using BlazorBlueprint.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddBlazorBlueprintComponents();

// GraphQL auth client
var graphqlEndpoint = builder.Configuration["GraphQL:Endpoint"] ?? "http://localhost:5000/graphql";
builder.Services.AddHttpClient<GraphQLAuthClient>(client =>
{
    client.BaseAddress = new Uri(graphqlEndpoint);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
