using Microsoft.Extensions.Configuration;
using PonteInclusaoWeb.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using System;
using System.Diagnostics;
using System.Linq;

namespace PonteInclusaoWeb.Services;

public class GoogleMapsService : IMapService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private const string BaseUrl = "https://maps.googleapis.com/maps/api/place/textsearch/json";

    public GoogleMapsService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<List<Place>> SearchSchoolsAsync(string city, string disabilityType)
    {
        string fields = "place_id,name,formatted_address,geometry,rating,formatted_phone_number,website,url";
        string searchQuery = $"escolas com suporte para {disabilityType} em {city}";
        return await ProcessSearchRequest(searchQuery, fields);
    }

    public async Task<Place?> SearchCityHallAsync(string city)
    {
        string fields = "place_id,name,formatted_address,geometry,formatted_phone_number,website,url";
        string searchQuery = $"secretaria de educação de {city}";
        var results = await ProcessSearchRequest(searchQuery, fields);
        return results.FirstOrDefault();
    }

    private async Task<List<Place>> ProcessSearchRequest(string searchQuery, string fields)
    {
        // A CHAVE DA API É LIDA DO User Secrets AQUI, E SÓ AQUI.
        var apiKey = _configuration["GoogleMaps:ApiKey"];
        if (string.IsNullOrEmpty(apiKey) || apiKey.Contains("SUA_CHAVE"))
            throw new InvalidOperationException("A chave da API do Google Maps não está configurada corretamente.");

        try
        {
            string encodedQuery = HttpUtility.UrlEncode(searchQuery);
            string requestUrl = $"{BaseUrl}?query={encodedQuery}&fields={fields}&key={apiKey}&language=pt-BR";

            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.GetAsync(requestUrl);
            if (!response.IsSuccessStatusCode) return new List<Place>();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var places = new List<Place>();
            var googleResponse = JsonSerializer.Deserialize<GooglePlaceResponse>(jsonResponse);

            if (googleResponse?.Results != null)
            {
                foreach (var result in googleResponse.Results)
                {
                    var place = new Place
                    {
                        Name = result.Name,
                        Address = result.FormattedAddress,
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
        catch (Exception ex)
        {
            Debug.WriteLine($"Ocorreu um erro em GoogleMapsService: {ex.Message}");
            return new List<Place>();
        }
    }
}