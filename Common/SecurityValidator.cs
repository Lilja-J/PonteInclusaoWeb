using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace PonteInclusaoWeb.Common;

public static class SecurityValidator
{
    private const int MaxCityLength = 100;
    private const int MaxDisabilityLength = 100;

    // Permite letras (incluindo acentuadas), números, espaços, hífens, pontos e apóstrofos
    private static readonly Regex SafeCityRegex = new(
        @"^[\p{L}0-9\s\.\-']+$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant,
        TimeSpan.FromMilliseconds(100)
    );

    // Permite letras, números, espaços, parênteses e hífens
    private static readonly Regex SafeDisabilityRegex = new(
        @"^[\p{L}0-9\s\.\-\(\)]+$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant,
        TimeSpan.FromMilliseconds(100)
    );

    public static bool TrySanitizeCity(string? input, out string sanitized)
    {
        sanitized = string.Empty;
        if (string.IsNullOrWhiteSpace(input))
            return false;

        var trimmed = input.Trim();
        if (trimmed.Length > MaxCityLength)
            return false;

        // Rejeita caracteres de controle
        if (trimmed.Any(char.IsControl))
            return false;

        try
        {
            if (!SafeCityRegex.IsMatch(trimmed))
                return false;
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }

        sanitized = trimmed;
        return true;
    }

    public static bool TrySanitizeDisability(string? input, out string sanitized)
    {
        sanitized = string.Empty;
        if (string.IsNullOrWhiteSpace(input))
            return false;

        var trimmed = input.Trim();
        if (trimmed.Length > MaxDisabilityLength)
            return false;

        if (trimmed.Any(char.IsControl))
            return false;

        try
        {
            if (!SafeDisabilityRegex.IsMatch(trimmed))
                return false;
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }

        sanitized = trimmed;
        return true;
    }

    public static bool IsValidCoordinate(double lat, double lng)
    {
        if (double.IsNaN(lat) || double.IsInfinity(lat)) return false;
        if (double.IsNaN(lng) || double.IsInfinity(lng)) return false;

        return lat >= -90.0 && lat <= 90.0 && lng >= -180.0 && lng <= 180.0;
    }
}
