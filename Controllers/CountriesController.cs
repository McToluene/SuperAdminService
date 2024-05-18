using Configurations.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SuperAdmin.Service.Models.Dtos.CountryDomain;
using SuperAdmin.Service.Services.Contracts;

namespace SuperAdmin.Service.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class CountriesController : BaseController
    {
        private readonly ICountryService _countryService;

        public CountriesController(ICountryService countryService)
        {
            _countryService = countryService;
        }

        /// <summary>
        /// This endpoint gets all countries
        /// </summary>
        /// <param name="country"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<CountryDto>>), 200)]
        public async Task<IActionResult> GetCountries([FromQuery] string? country)
        {
            var response = await _countryService.GetCountries(country);
            return ParseResponse(response);
        }
    }
}
