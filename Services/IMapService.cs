using PonteInclusaoWeb.Models; 
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PonteInclusaoWeb.Services
{
    public interface IMapService
    {
        Task<List<Place>> SearchSchoolsAsync(string city, string disabilityType);
    }
}