using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PonteInclusaoWeb.Common;
using PonteInclusaoWeb.Models;

namespace PonteInclusaoWeb.Services;

public class GoogleMapsService : IMapService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GoogleMapsService> _logger;
    private const string BaseUrl = "https://maps.googleapis.com/maps/api/place/textsearch/json";

    public GoogleMapsService(
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        ILogger<GoogleMapsService> logger)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<List<Place>> SearchSchoolsAsync(string city, string disabilityType, CancellationToken cancellationToken = default)
    {
        // OWASP A03: Validação estrita de entrada
        if (!SecurityValidator.TrySanitizeCity(city, out var safeCity))
        {
            _logger.LogWarning("Parâmetro de cidade inválido ou rejeitado pelo validador de segurança.");
            return new List<Place>();
        }

        if (!SecurityValidator.TrySanitizeDisability(disabilityType, out var safeDisability))
        {
            _logger.LogWarning("Parâmetro de tipo de deficiência inválido ou rejeitado pelo validador de segurança.");
            return new List<Place>();
        }

        const string fields = "place_id,name,formatted_address,geometry,rating,formatted_phone_number,website,url";
        string searchQuery = $"escolas com suporte para {safeDisability} em {safeCity}";
        return await ProcessSearchRequest(searchQuery, fields, cancellationToken);
    }

    public async Task<Place?> SearchCityHallAsync(string city, CancellationToken cancellationToken = default)
    {
        if (!SecurityValidator.TrySanitizeCity(city, out var safeCity))
        {
            _logger.LogWarning("Parâmetro de cidade inválido para busca de órgão público.");
            return null;
        }

        const string fields = "place_id,name,formatted_address,geometry,formatted_phone_number,website,url";
        string searchQuery = $"secretaria de educação de {safeCity}";
        var results = await ProcessSearchRequest(searchQuery, fields, cancellationToken);
        return results.FirstOrDefault();
    }

    private async Task<List<Place>> ProcessSearchRequest(string searchQuery, string fields, CancellationToken cancellationToken)
    {
        // OWASP A05: Chave obtida via IConfiguration (User Secrets em dev, variáveis de ambiente ou Secret Manager em prod)
        var apiKey = _configuration["GoogleMaps:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey) || apiKey.Contains("SUA_CHAVE"))
        {
            _logger.LogWarning("Chave de API do Google Maps não configurada ou com valor padrão de placeholder.");
            return new List<Place>();
        }

        try
        {
            string encodedQuery = HttpUtility.UrlEncode(searchQuery);
            // Montagem estrita de URL; note que nunca logamos requestUrl com a chave
            string requestUrl = $"{BaseUrl}?query={encodedQuery}&fields={fields}&key={apiKey}&language=pt-BR";

            var httpClient = _httpClientFactory.CreateClient("GoogleMaps");
            using var response = await httpClient.GetAsync(requestUrl, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Resposta da API de locais retornou status code HTTP {StatusCode}", (int)response.StatusCode);
                return new List<Place>();
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var googleResponse = await JsonSerializer.DeserializeAsync<GooglePlaceResponse>(stream, cancellationToken: cancellationToken);

            var places = new List<Place>();
            if (googleResponse?.Results != null)
            {
                foreach (var result in googleResponse.Results)
                {
                    var place = new Place
                    {
                        Name = result.Name ?? string.Empty,
                        Address = result.FormattedAddress ?? string.Empty,
                        Coordinates = new Coordinates(
                            result.Geometry?.Location?.Lat ?? 0.0,
                            result.Geometry?.Location?.Lng ?? 0.0
                        ),
                        Rating = result.Rating ?? 0.0
                    };
                    places.Add(place);
                }
            }

            return places;
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Busca de escolas cancelada pela solicitação do cliente.");
            return new List<Place>();
        }
        catch (Exception ex)
        {
            // OWASP A09: Log seguro sem vazar a URL nem a API Key
            _logger.LogError(ex, "Erro ao processar consulta na API de locais do Google.");
            return new List<Place>();
        }
    }
}
