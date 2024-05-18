using System.Net;
using System.Text.Json;
using Configurations.Utility;
using Identification;
using Serilog;
using SuperAdmin.Service.Database.Enums;
using SuperAdmin.Service.Models.Dtos.SubscriptionDomain;
using SuperAdmin.Service.Models.Enums;
using SuperAdmin.Service.Services.Contracts;
using ILogger = Serilog.ILogger;

namespace SuperAdmin.Service.Services.Implementations
{
    public class HttpSubscriptionService : ISubscriptionService, IDisposable
    {
        private readonly ILogger _logger = Log.ForContext<HttpSubscriptionService>();
        private readonly HttpClient _httpClient;
        private readonly ITicketCategoryService _ticketCategoryService;
        private readonly ITicketService _ticketService;

        public HttpSubscriptionService(IConfiguration configuration, ITicketCategoryService ticketCategoryService, ITicketService ticketService)
        {
            var baseAddress = configuration.GetSection("Services").GetSection("Monolith").Value;
            if (baseAddress == null)
                throw new ArgumentNullException(nameof(baseAddress), "The 'baseAddress' parameter cannot be null.");

            _httpClient = new HttpClient { BaseAddress = new Uri(baseAddress) };
            _ticketCategoryService = ticketCategoryService;
            _ticketService = ticketService;
        }

        public async Task<ApiResponse<Page<SubscriptionCompanyDetail>>> GetSubscribedCompanyDetail(FilterSubscriptionCompany request, string? subdomain)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("subdomain", subdomain);

                var requestUri = $"/api/subscription/subscribed-company?" +
                                 $"application={request.Application}&fromDate={request.FromDate}&" +
                                 $"pageNumber={request.Page}&pageSize={request.PageSize}&" +
                                 $"periodFilter={request.PeriodFilter}&planId={request.PlanId}&" +
                                 $"sortBy={request.SortBy}&toDate={request.ToDate}&" +
                                 $"provider={request.Provider}";

                var response = await _httpClient.GetAsync(requestUri);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse<Page<SubscriptionCompanyDetail>>>(responseBody);

                    return new ApiResponse<Page<SubscriptionCompanyDetail>>
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

                    var errorApiResponse = JsonSerializer.Deserialize<ApiResponse<Page<SubscriptionCompanyDetail>>>(errorMessage)
                                           ?? new ApiResponse<Page<SubscriptionCompanyDetail>>
                                           {
                                               ResponseCode = statusCode.ToString(),
                                               ResponseMessage = "Failed to fetch data"
                                           };

                    _logger.Error($"Error in GetSubscribedCompanyDetail: {errorMessage}");

                    return errorApiResponse;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error in GetSubscribedCompanyDetail: {ex}");
                return new ApiResponse<Page<SubscriptionCompanyDetail>>
                {
                    ResponseCode = "500",
                    ResponseMessage = "Failed to fetch data"
                };
            }
        }

        public async Task<ApiResponse<ApplicationStatistic>> GetSummaryStatistics(Applications application, PaymentProviders? provider, string? subdomain)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("subdomain", subdomain);

                var requestUri = $"/api/subscription/statistic?" +
                                 $"application={application}&" +
                                 $"paymentProvider={provider}";

                var response = await _httpClient.GetAsync(requestUri);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse<SummaryStatisticsResponse>>(responseBody);

                    var id = await _ticketCategoryService.GetCategoryByApplication(application);
                    int count = await _ticketService.TicketCountByCategory(id.ToString(), TicketStatus.PENDING);

                    return new ApiResponse<ApplicationStatistic>
                    {
                        Data = new ApplicationStatistic
                        {
                            TotalActive = result?.Data?.TotalActive,
                            TotalCompanies = result?.Data?.TotalCompanies,
                            PendingRequest = count,
                            TotalRevenue = result?.Data?.TotalRevenue,
                            TotalSubscriptions = result?.Data?.TotalSubscriptions
                        },
                        ResponseCode = result?.ResponseCode,
                        ResponseMessage = result?.ResponseMessage
                    };
                }
                else
                {
                    var statusCode = (int)response.StatusCode;
                    var errorMessage = await response.Content.ReadAsStringAsync();

                    return JsonSerializer.Deserialize<ApiResponse<ApplicationStatistic>>(errorMessage)
                           ?? new ApiResponse<ApplicationStatistic>
                           {
                               ResponseCode = statusCode.ToString(),
                               ResponseMessage = "Failed to fetch data"
                           };
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error in GetSummaryStatistics: {ex}");
                return new ApiResponse<ApplicationStatistic>
                {
                    ResponseCode = "500",
                    ResponseMessage = "Failed to fetch data"
                };
            }
        }

        public async Task<ApiResponse<List<TotalSubscriptionCountPerPlan>>> GetTotalSubscriptionsCount(FilterSubscription request, string? subdomain)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("subdomain", subdomain);

                var requestUri = $"/api/subscription/total-count?" +
                                 $"application={request.Application}&fromDate={request.FromDate}&" +
                                 $"periodFilter={request.PeriodFilter}&toDate={request.ToDate}&" +
                                 $"paymentProvider={request.Provider}";

                var response = await _httpClient.GetAsync(requestUri);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse<List<TotalSubscriptionCountPerPlan>>>(responseBody);

                    return new ApiResponse<List<TotalSubscriptionCountPerPlan>>
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

                    var errorApiResponse = JsonSerializer.Deserialize<ApiResponse<List<TotalSubscriptionCountPerPlan>>>(errorMessage)
                                           ?? new ApiResponse<List<TotalSubscriptionCountPerPlan>>
                                           {
                                               ResponseCode = statusCode.ToString(),
                                               ResponseMessage = "Failed to fetch data"
                                           };

                    _logger.Error($"Error in GetTotalSubscriptionsCount: {errorMessage}");

                    return errorApiResponse;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error in GetTotalSubscriptionsCount: {ex}");
                return new ApiResponse<List<TotalSubscriptionCountPerPlan>>
                {
                    ResponseCode = "500",
                    ResponseMessage = "Failed to fetch data"
                };
            }
        }

        public async Task<ApiResponse<List<PercentageIncrementPerPlanDto>>> GetPercentageIncrementPerPlan(FilterSubscription request, string? subdomain)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("subdomain", subdomain);

                var requestUri = $"/api/subscription/percentage-increment?" +
                                 $"application={request.Application}&fromDate={request.FromDate}&" +
                                 $"periodFilter={request.PeriodFilter}&toDate={request.ToDate}&" +
                                 $"paymentProvider={request.Provider}";

                var response = await _httpClient.GetAsync(requestUri);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse<List<PercentageIncrementPerPlanDto>>>(responseBody);

                    return new ApiResponse<List<PercentageIncrementPerPlanDto>>
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

                    var errorApiResponse = JsonSerializer.Deserialize<ApiResponse<List<PercentageIncrementPerPlanDto>>>(errorMessage)
                                           ?? new ApiResponse<List<PercentageIncrementPerPlanDto>>
                                           {
                                               ResponseCode = statusCode.ToString(),
                                               ResponseMessage = "Failed to fetch data"
                                           };

                    _logger.Error($"Error in GetPercentageIncrementPerPlan: {errorMessage}");

                    return errorApiResponse;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error in GetPercentageIncrementPerPlan: {ex}");
                return new ApiResponse<List<PercentageIncrementPerPlanDto>>
                {
                    ResponseCode = "500",
                    ResponseMessage = "Failed to fetch data"
                };
            }
        }

        public async Task<ApiResponse<Page<TenantSubscription>>> GetTenantsSubscriptionHistory(FilterSubscriptionCompany request, string? subdomain)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("subdomain", subdomain);

                var requestUri = $"/api/subscription/tenants/history?" +
                                 $"application={request.Application}&fromDate={request.FromDate}&" +
                                 $"pageNumber={request.Page}&pageSize={request.PageSize}&" +
                                 $"periodFilter={request.PeriodFilter}&planId={request.PlanId}&" +
                                 $"sortBy={request.SortBy}&toDate={request.ToDate}&" +
                                 $"provider={request.Provider}";

                var response = await _httpClient.GetAsync(requestUri);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse<Page<TenantSubscription>>>(responseBody);

                    return new ApiResponse<Page<TenantSubscription>>
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

                    var errorApiResponse = JsonSerializer.Deserialize<ApiResponse<Page<TenantSubscription>>>(errorMessage)
                                           ?? new ApiResponse<Page<TenantSubscription>>
                                           {
                                               ResponseCode = statusCode.ToString(),
                                               ResponseMessage = "Failed to fetch data"
                                           };

                    _logger.Error($"Error in GetTenantsSubscriptionHistory: {errorMessage}");

                    return errorApiResponse;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error in GetTenantsSubscriptionHistory: {ex}");
                return new ApiResponse<Page<TenantSubscription>>
                {
                    ResponseCode = "500",
                    ResponseMessage = "Failed to fetch data"
                };
            }
        }

        public async Task<ApiResponse<Page<TenantSubscriptionDetail>>> GetTenantSubscriptionHistory(string tenantId, FilterSubscriptionCompany request, string? subdomain)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("subdomain", subdomain);

                var requestUri = $"/api/subscription/tenant/history?" +
                                 $"tenantId={tenantId}&" +
                                 $"application={request.Application}&fromDate={request.FromDate}&" +
                                 $"pageNumber={request.Page}&pageSize={request.PageSize}&" +
                                 $"periodFilter={request.PeriodFilter}&planId={request.PlanId}&" +
                                 $"sortBy={request.SortBy}&toDate={request.ToDate}&" +
                                 $"provider={request.Provider}";

                var response = await _httpClient.GetAsync(requestUri);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse<Page<TenantSubscriptionDetail>>>(responseBody);

                    return new ApiResponse<Page<TenantSubscriptionDetail>>
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

                    var errorApiResponse = JsonSerializer.Deserialize<ApiResponse<Page<TenantSubscriptionDetail>>>(errorMessage)
                                           ?? new ApiResponse<Page<TenantSubscriptionDetail>>
                                           {
                                               ResponseCode = statusCode.ToString(),
                                               ResponseMessage = "Failed to fetch data"
                                           };

                    _logger.Error($"Error in GetTenantSubscriptionHistory: {errorMessage}");

                    return errorApiResponse;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error in GetTenantSubscriptionHistory: {ex}");
                return new ApiResponse<Page<TenantSubscriptionDetail>>
                {
                    ResponseCode = "500",
                    ResponseMessage = "Failed to fetch data"
                };
            }
        }

        public async Task<ApiResponse<List<ProviderRevenue>>> GetProviderRevenue(Applications application, TimePeriodFilter periodFilter, DateTime? fromDate, DateTime? toDate, string? subdomain)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("subdomain", subdomain);

                var requestUri = $"/api/subscription/revenue?" +
                                 $"application={application}&fromDate={fromDate}&" +
                                 $"periodFilter={periodFilter}&toDate={toDate}&";

                var response = await _httpClient.GetAsync(requestUri);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse<List<ProviderRevenue>>>(responseBody);

                    return new ApiResponse<List<ProviderRevenue>>
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

                    var errorApiResponse = JsonSerializer.Deserialize<ApiResponse<List<ProviderRevenue>>>(errorMessage)
                                           ?? new ApiResponse<List<ProviderRevenue>>
                                           {
                                               ResponseCode = statusCode.ToString(),
                                               ResponseMessage = "Failed to fetch data"
                                           };

                    _logger.Error($"Error in GetProviderRevenue: {errorMessage}");

                    return errorApiResponse;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error in GetProviderRevenue: {ex}");
                return new ApiResponse<List<ProviderRevenue>>
                {
                    ResponseCode = "500",
                    ResponseMessage = "Failed to fetch data"
                };
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
