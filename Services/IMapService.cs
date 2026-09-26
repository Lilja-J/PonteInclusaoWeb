using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PonteInclusaoWeb.Models;

namespace PonteInclusaoWeb.Services;

public interface IMapService
{
    Task<List<Place>> SearchSchoolsAsync(string city, string disabilityType, CancellationToken cancellationToken = default);
    Task<Place?> SearchCityHallAsync(string city, CancellationToken cancellationToken = default);
}
