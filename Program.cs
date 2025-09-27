using PonteInclusaoWeb.Components;
using PonteInclusaoWeb.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient();
builder.Services.AddSingleton<IMapService, GoogleMapsService>();

var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapGet("/map-image", async (double lat, double lng, IConfiguration config, IHttpClientFactory httpFactory) =>
{
    var apiKey = config["GoogleMaps:ApiKey"];
    if (string.IsNullOrEmpty(apiKey))
    {
        return Results.BadRequest("API Key não configurada.");
    }

    var imageUrl = $"https://maps.googleapis.com/maps/api/staticmap?center={lat},{lng}&zoom=16&size=400x120&markers=color:purple%7C{lat},{lng}&key={apiKey}";

    try
    {
        var httpClient = httpFactory.CreateClient();
        var imageBytes = await httpClient.GetByteArrayAsync(imageUrl);
        return Results.Bytes(imageBytes, "image/png");
    }
    catch { return Results.NotFound(); }
});

app.Run();