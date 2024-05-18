using Configurations.Utility;
using Grpc.Core;
using Grpc.Net.Client;
using Identification;
using Serilog;
using SuperAdmin.Service.Models.Dtos.SubscriptionDomain;
using SuperAdmin.Service.Models.Enums;
using SuperAdmin.Service.Services.Contracts;
using ILogger = Serilog.ILogger;

namespace SuperAdmin.Service;

public class SubscriptionService
{

    private readonly ILogger _logger = Log.ForContext<SubscriptionService>();
    private readonly GrpcSubscriptionService.GrpcSubscriptionServiceClient _client;

    public SubscriptionService(IConfiguration configuration)
    {
        var address = configuration.GetSection("Services").GetSection("Monolith").Value;
        if (address == null) throw new ArgumentNullException(nameof(address), "The 'address' parameter cannot be null.");

        var channel = GrpcChannel.ForAddress(address);
        _client = new GrpcSubscriptionService.GrpcSubscriptionServiceClient(channel);
    }

    public async Task<ApiResponse<List<PercentageIncrementPerPlan>>> GetPercentageIncrementPerPlan(FilterSubscription request, string? subdomain)
    {
        try
        {
            var metadata = new Metadata { { "subdomain", subdomain ?? string.Empty } };
            var requestPayload = new SubscriptionStatisticsRequest()
            {
                Application = request.Application.ToString(),
                FromDate = request.FromDate?.ToString() ?? string.Empty,
                PeriodFilter = request.PeriodFilter,
                ToDate = request.ToDate?.ToString() ?? string.Empty
            };

            var response = await _client.GetPercentageIncrementPerPlanAsync(requestPayload, headers: metadata);
            return new ApiResponse<List<PercentageIncrementPerPlan>>
            {
                Data = response.PecentageIncrement.ToList(),
                ResponseCode = "200",
                ResponseMessage = $"Companies detail fetched successfully!"
            };
        }
        catch (RpcException rpcEx) when (rpcEx.StatusCode == StatusCode.InvalidArgument)
        {
            _logger.Error($"Invalid argument error in GetPercentageIncrementPerPlan: {rpcEx.Message}");
            return new ApiResponse<List<PercentageIncrementPerPlan>>
            {
                ResponseCode = "400",
                ResponseMessage = rpcEx.Message
            };
        }
        catch (Exception ex)
        {
            _logger.Error($"Error in GetPercentageIncrementPerPlan: {ex}");
            return new ApiResponse<List<PercentageIncrementPerPlan>>
            {
                ResponseCode = "500",
                ResponseMessage = "Internal Server Error"
            };
        }
    }

    public async Task<ApiResponse<Page<SubscriptionCompanyDetail>>> GetSubscribedCompanyDetail(FilterSubscriptionCompany request, string? subdomain)
    {
        try
        {
            var metadata = new Metadata { { "subdomain", subdomain ?? string.Empty } };

            var requestPayload = new SubscriptionQueryParametersRequest()
            {
                Application = request.Application.ToString(),
                FromDate = request.FromDate?.ToString() ?? string.Empty,
                PageNumber = request.Page ?? 1,
                PageSize = request.PageSize ?? 20,
                PeriodFilter = request.PeriodFilter,
                PlanId = request.PlanId ?? string.Empty,
                SortBy = request.SortBy.ToString(),
                ToDate = request.ToDate?.ToString() ?? string.Empty
            };

            var response = await _client.GetSubscribedCompanyDetailAsync(requestPayload, headers: metadata);
            return new ApiResponse<Page<SubscriptionCompanyDetail>>
            {
                ResponseCode = "200",
                ResponseMessage = $"Companies detail fetched successfully!"
            };
        }
        catch (RpcException rpcEx) when (rpcEx.StatusCode == StatusCode.InvalidArgument)
        {
            _logger.Error($"Invalid argument error in GetSubscribedCompanyDetail: {rpcEx.Message}");
            return new ApiResponse<Page<SubscriptionCompanyDetail>>
            {
                ResponseCode = "400",
                ResponseMessage = rpcEx.Message
            };
        }
        catch (Exception ex)
        {
            _logger.Error($"Error in GetSubscribedCompanyDetail: {ex}");
            return new ApiResponse<Page<SubscriptionCompanyDetail>>
            {
                ResponseCode = "500",
                ResponseMessage = "Internal Server Error"
            };
        }
    }

    public async Task<ApiResponse<SummaryStatisticsResponse>> GetSummaryStatistics(Applications application, PaymentProviders? provider, string? subdomain)
    {
        try
        {
            var metadata = new Metadata { { "subdomain", subdomain ?? string.Empty } };
            var requestPayload = new SummaryStatisticsRequest() { Application = application.ToString() };
            var response = await _client.GetSummaryStatisticsAsync(requestPayload, headers: metadata);
            return new ApiResponse<SummaryStatisticsResponse>
            {
                Data = response,
                ResponseCode = "200",
                ResponseMessage = $"Statistics summary fetched successfully!"
            };
        }
        catch (RpcException rpcEx) when (rpcEx.StatusCode == StatusCode.InvalidArgument)
        {
            _logger.Error($"Invalid argument error in GetPercentageIncrementPerPlan: {rpcEx.Message}");
            return new ApiResponse<SummaryStatisticsResponse>
            {
                ResponseCode = "400",
                ResponseMessage = rpcEx.Message
            };
        }
        catch (Exception ex)
        {
            _logger.Error($"Error in GetPercentageIncrementPerPlan: {ex}");
            return new ApiResponse<SummaryStatisticsResponse>
            {
                ResponseCode = "500",
                ResponseMessage = "Internal Server Error"
            };
        }
    }

    public async Task<ApiResponse<List<TotalSubscriptionCountPerPlan>>> GetTotalSubscriptionsCount(FilterSubscription request, string? subdomain)
    {
        try
        {
            var metadata = new Metadata { { "subdomain", subdomain ?? string.Empty } };
            var requestPayload = new SubscriptionStatisticsRequest()
            {
                Application = request.Application.ToString(),
                FromDate = request.FromDate?.ToString() ?? string.Empty,
                PeriodFilter = request.PeriodFilter,
                ToDate = request.ToDate?.ToString() ?? string.Empty
            };

            var response = await _client.GetTotalSubscriptionsCountAsync(requestPayload, headers: metadata);
            return new ApiResponse<List<TotalSubscriptionCountPerPlan>>
            {
                Data = response.TotalSubscriptionPerPlans.ToList(),
                ResponseCode = "200",
                ResponseMessage = $"Subscription total fetched successfully!"
            };
        }
        catch (RpcException rpcEx) when (rpcEx.StatusCode == StatusCode.InvalidArgument)
        {
            _logger.Error($"Invalid argument error in GetTotalSubscriptionsCount: {rpcEx.Message}");
            return new ApiResponse<List<TotalSubscriptionCountPerPlan>>
            {
                ResponseCode = "400",
                ResponseMessage = rpcEx.Message
            };
        }
        catch (Exception ex)
        {
            _logger.Error($"Error in GetTotalSubscriptionsCount: {ex}");
            return new ApiResponse<List<TotalSubscriptionCountPerPlan>>
            {
                ResponseCode = "500",
                ResponseMessage = "Internal Server Error"
            };
        }
    }
}
