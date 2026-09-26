using System;
using System.Globalization;
using System.Threading;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PonteInclusaoWeb.Common;
using PonteInclusaoWeb.Components;
using PonteInclusaoWeb.Services;

var builder = WebApplication.CreateBuilder(args);

// OWASP A05: Remover cabeçalho Server do Kestrel
builder.WebHost.ConfigureKestrel(options =>
{
    options.AddServerHeader = false;
});

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// OWASP A04: Configuração de HttpClient com timeout estrito
builder.Services.AddHttpClient("GoogleMaps", client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
    client.DefaultRequestHeaders.Add("User-Agent", "PonteInclusaoWeb/1.0");
});

builder.Services.AddSingleton<IMapService, GoogleMapsService>();

// OWASP A02: Proteção de cookies antiforgery
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

// OWASP A02: Configuração de HSTS para ambientes de produção
builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
});

// OWASP A04: Rate Limiting para mitigar DoS e esgotamento de cotas de APIs
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsync(
            "{\"error\":\"Muitas requisições. Por favor, aguarde antes de tentar novamente.\"}",
            cancellationToken: token);
    };

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: clientIp,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            });
    });

    options.AddFixedWindowLimiter("MapEndpointPolicy", opt =>
    {
        opt.PermitLimit = 20;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

// OWASP A05: Security Headers Middleware
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "SAMEORIGIN");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Append("Permissions-Policy", "camera=(), microphone=(), geolocation=(), payment=()");
    context.Response.Headers.Append(
        "Content-Security-Policy",
        "default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; font-src 'self' https://fonts.gstatic.com data:; img-src 'self' data: blob: https://maps.googleapis.com; connect-src 'self' wss: ws:; frame-ancestors 'self'; object-src 'none'; base-uri 'self';"
    );

    context.Response.Headers.Remove("Server");
    context.Response.Headers.Remove("X-Powered-By");

    await next();
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseRateLimiter();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// OWASP A01 & A10: Endpoint seguro com validação de coordenadas, rate limit e cache
app.MapGet("/map-image", async (
    double lat,
    double lng,
    IConfiguration config,
    IHttpClientFactory httpFactory,
    ILoggerFactory loggerFactory,
    HttpContext httpContext,
    CancellationToken cancellationToken) =>
{
    var logger = loggerFactory.CreateLogger("MapImageEndpoint");

    // Validação estrita de coordenadas para prevenir abusos / SSRF
    if (!SecurityValidator.IsValidCoordinate(lat, lng))
    {
        return Results.BadRequest(new { error = "Parâmetros de coordenadas geográficas inválidos." });
    }

    var apiKey = config["GoogleMaps:ApiKey"];
    if (string.IsNullOrWhiteSpace(apiKey) || apiKey.Contains("SUA_CHAVE"))
    {
        logger.LogWarning("Tentativa de acesso a /map-image sem GoogleMaps:ApiKey configurada no ambiente.");
        return Results.Problem(
            statusCode: StatusCodes.Status503ServiceUnavailable,
            title: "Serviço indisponível",
            detail: "O serviço de mapas não está disponível no momento."
        );
    }

    // Formatação invariante para evitar bugs com vírgula decimal em pt-BR
    var latStr = lat.ToString(CultureInfo.InvariantCulture);
    var lngStr = lng.ToString(CultureInfo.InvariantCulture);
    var imageUrl = $"https://maps.googleapis.com/maps/api/staticmap?center={latStr},{lngStr}&zoom=16&size=400x120&markers=color:purple%7C{latStr},{lngStr}&key={apiKey}";

    try
    {
        var httpClient = httpFactory.CreateClient("GoogleMaps");
        var imageBytes = await httpClient.GetByteArrayAsync(imageUrl, cancellationToken);

        // Cache no cliente e CDN para diminuir requisições e custo
        httpContext.Response.Headers.CacheControl = "public, max-age=86400";
        return Results.Bytes(imageBytes, "image/png");
    }
    catch (OperationCanceledException)
    {
        return Results.StatusCode(StatusCodes.Status499ClientClosedRequest);
    }
    catch (Exception ex)
    {
        // Log seguro sem vazar a URL nem a chave
        logger.LogError(ex, "Erro ao obter imagem estática do mapa para coordenadas lat={Lat}, lng={Lng}", lat, lng);
        return Results.NotFound();
    }
}).RequireRateLimiting("MapEndpointPolicy");

app.Run();
