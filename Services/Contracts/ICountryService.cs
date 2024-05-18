using Configurations.Utility;
using SuperAdmin.Service.Models.Dtos.CountryDomain;

namespace SuperAdmin.Service.Services.Contracts
{
    public interface ICountryService
    {
        Task<ApiResponse<IEnumerable<CountryDto>>> GetCountries(string? country);
    }
}
