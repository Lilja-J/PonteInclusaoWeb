using Microsoft.Extensions.Configuration;
using PonteInclusaoWeb.Models; // Note o namespace correto
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using System;
using System.Diagnostics;

namespace PonteInclusaoWeb.Services; // Note o namespace correto

public class GoogleMapsService : IMapService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    // A VARIÁVEL QUE FALTAVA ESTÁ AQUI:
    private const string BaseUrl = "https://maps.googleapis.com/maps/api/place/textsearch/json";

    public GoogleMapsService(IConfiguration configuration, HttpClient httpClient)
    {
        _apiKey = Environment.GetEnvironmentVariable("GOOGLE_MAPS_API_KEY")!;
        if (string.IsNullOrEmpty(_apiKey))
            throw new InvalidOperationException("API Key do Google Maps não configurada na variável de ambiente GOOGLE_MAPS_API_KEY.");
        _httpClient = httpClient;
    }

    public async Task<List<Place>> SearchSchoolsAsync(string city, string disabilityType)
    {
        try
        {
            string searchQuery = $"escolas com suporte para {disabilityType} em {city}";
            string encodedQuery = HttpUtility.UrlEncode(searchQuery);
            string requestUrl = $"{BaseUrl}?query={encodedQuery}&key={_apiKey}&language=pt-BR";

            var response = await _httpClient.GetAsync(requestUrl);

            if (!response.IsSuccessStatusCode) return new List<Place>();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var places = new List<Place>();
            var googleResponse = JsonSerializer.Deserialize<GooglePlaceResponse>(jsonResponse);

            if (googleResponse?.Results != null)
            {
                foreach (var result in googleResponse.Results)
                {
                    string name = result.Name.ToLower();
                    if (name.Contains("universidade") || name.Contains("faculdade"))
                        continue;

                    var place = new Place
                    {
                        Name = result.Name,
                        Address = result.FormattedAddress,
                        Coordinates = new Coordinates(
                            result.Geometry?.Location?.Lat ?? 0.0,
                            result.Geometry?.Location?.Lng ?? 0.0
                        ),
                        Rating = result.Rating ?? 0.0,
                        IsPermanentlyClosed = result.BusinessStatus == "CLOSED_PERMANENTLY"
                    };

                    if (!place.IsPermanentlyClosed)
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