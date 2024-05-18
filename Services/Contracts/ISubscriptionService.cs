using Configurations.Utility;
using Identification;
using SuperAdmin.Service.Models.Dtos.SubscriptionDomain;
using SuperAdmin.Service.Models.Enums;

namespace SuperAdmin.Service.Services.Contracts;

public interface ISubscriptionService
{
    Task<ApiResponse<List<PercentageIncrementPerPlanDto>>> GetPercentageIncrementPerPlan(FilterSubscription request, string? subdomain);
    Task<ApiResponse<Page<SubscriptionCompanyDetail>>> GetSubscribedCompanyDetail(FilterSubscriptionCompany request, string? subdomain);
    Task<ApiResponse<Page<TenantSubscription>>> GetTenantsSubscriptionHistory(FilterSubscriptionCompany request, string? subdomain);
    Task<ApiResponse<Page<TenantSubscriptionDetail>>> GetTenantSubscriptionHistory(string tenantId, FilterSubscriptionCompany request, string? subdomain);
    Task<ApiResponse<ApplicationStatistic>> GetSummaryStatistics(Applications application, PaymentProviders? provider, string? subdomain);
    Task<ApiResponse<List<ProviderRevenue>>> GetProviderRevenue(Applications application, TimePeriodFilter periodFilter, DateTime? fromDate, DateTime? toDate, string? subdomain);
    Task<ApiResponse<List<TotalSubscriptionCountPerPlan>>> GetTotalSubscriptionsCount(FilterSubscription request, string? subdomain);
}

