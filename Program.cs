using PonteInclusaoWeb.Components;
using PonteInclusaoWeb.Services;

var builder = WebApplication.CreateBuilder(args);

// ADICIONE ESTAS DUAS LINHAS PARA CONFIGURAR O LOGGING
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Nossos serviços customizados
builder.Services.AddSingleton<IMapService, GoogleMapsService>();
builder.Services.AddHttpClient();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();