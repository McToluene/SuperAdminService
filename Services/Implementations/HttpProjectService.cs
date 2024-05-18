using System.Text.Json;
using Configurations.Utility;
using Identification;
using Serilog;
using SuperAdmin.Service.Models.Dtos.ProjectDomain;
using SuperAdmin.Service.Services.Contracts;
using ILogger = Serilog.ILogger;

namespace SuperAdmin.Service.Services.Implementations
{
    public class HttpProjectService : IProjectService, IDisposable
    {
        private readonly ILogger _logger = Log.ForContext<HttpProjectService>();
        private readonly HttpClient _httpClient;

        public HttpProjectService(IConfiguration configuration)
        {
            var baseAddress = configuration.GetSection("Services")?.GetSection("Monolith")?.Value;
            if (baseAddress == null)
                throw new ArgumentNullException(nameof(baseAddress), "The 'baseAddress' parameter cannot be null.");

            _httpClient = new HttpClient { BaseAddress = new Uri(baseAddress) };
        }

        public async Task<ApiResponse<Page<CompanyDetailDto>>> GetProjectCompanyDetail(CompanyProjectFilter filter, int? pageSize, int? pageNumber, string? subdomain)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("subdomain", subdomain);

                var queryParams = $"?filter={filter}&pageSize={pageSize}&pageNumber={pageNumber}";
                var response = await _httpClient.GetAsync("/api/project/getprojectcompanydetail" + queryParams);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse<Page<CompanyDetailDto>>>(responseBody);

                    return new ApiResponse<Page<CompanyDetailDto>>
                    {
                        Data = result?.Data,
                        ResponseCode = result?.ResponseCode,
                        ResponseMessage = result?.ResponseMessage
                    };
                }
                else
                {
                    var statusCode = (int)response.StatusCode;
                    var errorMessage = await response.Content.ReadAsStringAsync();

                    var errorApiResponse = JsonSerializer.Deserialize<ApiResponse<Page<CompanyDetailDto>>>(errorMessage)
                                           ?? new ApiResponse<Page<CompanyDetailDto>>
                                           {
                                               ResponseCode = statusCode.ToString(),
                                               ResponseMessage = "Failed to fetch data"
                                           };

                    _logger.Error($"Error in GetProjectCompanyDetail: {errorMessage}");

                    return errorApiResponse;
                }
            }
            catch (HttpRequestException httpEx)
            {
                _logger.Error($"Error in GetProjectCompanyDetail: {httpEx.Message}");
                return new ApiResponse<Page<CompanyDetailDto>>
                {
                    ResponseCode = httpEx.StatusCode.ToString(),
                    ResponseMessage = "Internal Server Error"
                };
            }
            catch (Exception ex)
            {
                _logger.Error($"Error in GetProjectCompanyDetail: {ex.Message}");
                return new ApiResponse<Page<CompanyDetailDto>>
                {
                    ResponseCode = "500",
                    ResponseMessage = "Failed to fetch data"
                };
            }
        }

        public async Task<ApiResponse<List<ProjectCount>>> GetProjectCount(string? subdomain)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("subdomain", subdomain);
                var response = await _httpClient.GetAsync("/api/project/getprojectcount");
                var result = await HandleApiResponse<List<ProjectCount>>(response);

                return result;
            }
            catch (HttpRequestException httpEx)
            {
                _logger.Error($"Error in GetProjectCount: {httpEx.Message}");
                return new ApiResponse<List<ProjectCount>>
                {
                    ResponseCode = httpEx.StatusCode.ToString(),
                    ResponseMessage = "Internal Server Error"
                };
            }
            catch (Exception ex)
            {
                _logger.Error($"Error in GetProjectCount: {ex.Message}");
                return new ApiResponse<List<ProjectCount>>
                {
                    ResponseCode = "500",
                    ResponseMessage = "Failed to fetch data"
                };
            }
        }

        public async Task<ApiResponse<List<TopPerformingCompany>>> GetTopPerformingCompaniesPercentage(string? subdomain)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("subdomain", subdomain);
                var response = await _httpClient.GetAsync("/api/project/gettopperformingcompaniespercentage");
                var result = await HandleApiResponse<List<TopPerformingCompany>>(response);

                return result;
            }
            catch (HttpRequestException httpEx)
            {
                _logger.Error($"Error in GetTopPerformingCompaniesPercentage: {httpEx.Message}");
                return new ApiResponse<List<TopPerformingCompany>>
                {
                    ResponseCode = httpEx.StatusCode.ToString(),
                    ResponseMessage = "Internal Server Error"
                };
            }
            catch (Exception ex)
            {
                _logger.Error($"Error in GetTopPerformingCompaniesPercentage: {ex.Message}");
                return new ApiResponse<List<TopPerformingCompany>>
                {
                    ResponseCode = "500",
                    ResponseMessage = "Failed to fetch data"
                };
            }
        }

        private async Task<ApiResponse<T>> HandleApiResponse<T>(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResponse<T?>>(responseBody);

                return new ApiResponse<T>
                {
                    Data = result.Data,
                    ResponseCode = result?.ResponseCode,
                    ResponseMessage = result?.ResponseMessage
                };
            }
            else
            {
                var statusCode = (int)response.StatusCode;
                var errorMessage = await response.Content.ReadAsStringAsync();

                var errorApiResponse = JsonSerializer.Deserialize<ApiResponse<T>>(errorMessage)
                                       ?? new ApiResponse<T>
                                       {
                                           ResponseCode = statusCode.ToString(),
                                           ResponseMessage = "Failed to fetch data"
                                       };

                _logger.Error($"Error in HandleApiResponse: {errorMessage}");

                return errorApiResponse;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) => _httpClient.Dispose();
    }
}
