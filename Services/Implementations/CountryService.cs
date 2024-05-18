using Configurations.Utility;
using SuperAdmin.Service.Database;
using SuperAdmin.Service.Models.Dtos.CountryDomain;
using SuperAdmin.Service.Services.Contracts;

namespace SuperAdmin.Service.Services.Implementations
{
    public class CountryService : ICountryService
    {
        private readonly AppDbContext _appDbContext;

        public CountryService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        /// <summary>
        /// This method gets countries
        /// </summary>
        /// <param name="country"></param>
        /// <returns></returns>
        public async Task<ApiResponse<IEnumerable<CountryDto>>> GetCountries(string? country)
        {
            IQueryable<CountryDto> records;

            if (!string.IsNullOrWhiteSpace(country))
            {
                records = _appDbContext.Country.Where(x => x.Name.Contains(country)).Select(x => new CountryDto
                {
                    Id = x.Id,
                    Country = x.Name
                }).OrderBy(x => x.Country);
            }
            else
            {
                records = _appDbContext.Country.Select(x => new CountryDto
                {
                    Id = x.Id,
                    Country = x.Name
                }).OrderBy(x => x.Country);
            }

            return new ApiResponse<IEnumerable<CountryDto>>
            {
                Data = records,
                ResponseCode = "200"
            };
        }
    }
}
