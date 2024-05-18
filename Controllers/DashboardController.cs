using System.ComponentModel.DataAnnotations;
using Configurations.Utility;
using Identification;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SuperAdmin.Service.Helpers;
using SuperAdmin.Service.Models.Dtos.ProjectDomain;
using SuperAdmin.Service.Models.Dtos.SubscriptionDomain;
using SuperAdmin.Service.Models.Enums;
using SuperAdmin.Service.Services.Contracts;

namespace SuperAdmin.Service.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
// [Authorize(Roles = HelperConstants.SUPER_ADMIN_ROLE, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class DashboardController : BaseController
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly IProjectService _projectService;
    public DashboardController(ISubscriptionService subscriptionService, IProjectService projectService)
    {
        _subscriptionService = subscriptionService;
        _projectService = projectService;
    }

    /// <summary>
    /// This endpoint gets subscription summary
    /// </summary>
    /// <param name="application"></param>
    /// <returns></returns>
    [HttpGet("subscription/summary")]
    [ProducesResponseType(typeof(ApiResponse<ApplicationStatistic>), 200)]
    public async Task<IActionResult> GetSummaryStatistics([FromQuery][Required] Applications application, [FromQuery] PaymentProviders? provider)
    {
        // Extract subdomain from HTTP request headers
        var subdomain = HttpContext.Request.Headers["subdomain"];

        var response = await _subscriptionService.GetSummaryStatistics(application, provider, subdomain);
        return ParseResponse(response);
    }

    /// <summary>
    /// This endpoint gets subscription summary
    /// </summary>
    /// <param name="application"></param>
    /// <returns></returns>
    [HttpGet("subscription/revenue")]
    [ProducesResponseType(typeof(ApiResponse<List<ProviderRevenue>>), 200)]
    public async Task<IActionResult> GetProviderRevenue([FromQuery][Required] Applications application, [FromQuery][Required] TimePeriodFilter periodFilter, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        // Extract subdomain from HTTP request headers
        var subdomain = HttpContext.Request.Headers["subdomain"];

        var response = await _subscriptionService.GetProviderRevenue(application, periodFilter, fromDate, toDate, subdomain);
        return ParseResponse(response);
    }

    /// <summary>
    /// This endpoint gets subscription plan percentage increment
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpGet("subscription/percentage")]
    [ProducesResponseType(typeof(ApiResponse<List<PercentageIncrementPerPlanDto>>), 200)]
    public async Task<IActionResult> GetPercentageIncrementPerPlan([FromQuery] FilterSubscription model)
    {
        // Extract subdomain from HTTP request headers
        var subdomain = HttpContext.Request.Headers["subdomain"];

        var response = await _subscriptionService.GetPercentageIncrementPerPlan(model, subdomain);
        return ParseResponse(response);
    }

    /// <summary>
    /// This endpoint gets subscription plans count
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpGet("subscription/count")]
    [ProducesResponseType(typeof(ApiResponse<List<TotalSubscriptionCountPerPlan>>), 200)]
    public async Task<IActionResult> GetTotalSubscriptionsCount([FromQuery] FilterSubscription model)
    {
        // Extract subdomain from HTTP request headers
        var subdomain = HttpContext.Request.Headers["subdomain"];

        var response = await _subscriptionService.GetTotalSubscriptionsCount(model, subdomain);
        return ParseResponse(response);
    }

    /// <summary>
    /// This endpoint get subscription company details
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpGet("subscription/company")]
    [ProducesResponseType(typeof(ApiResponse<Page<SubscriptionCompanyDetail>>), 200)]
    public async Task<IActionResult> GetSubscribedCompanyDetail([FromQuery] FilterSubscriptionCompany model)
    {
        // Extract subdomain from HTTP request headers
        var subdomain = HttpContext.Request.Headers["subdomain"];

        var response = await _subscriptionService.GetSubscribedCompanyDetail(model, subdomain);
        return ParseResponse(response);
    }

    /// <summary>
    /// This endpoint get subscription company details
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpGet("subscription/company/history")]
    [ProducesResponseType(typeof(ApiResponse<Page<TenantSubscriptionDetail>>), 200)]
    public async Task<IActionResult> GetTenantsSubscriptionHistory([FromQuery] FilterSubscriptionCompany model)
    {
        // Extract subdomain from HTTP request headers
        var subdomain = HttpContext.Request.Headers["subdomain"];

        var response = await _subscriptionService.GetTenantsSubscriptionHistory(model, subdomain);
        return ParseResponse(response);
    }

    /// <summary>
    /// This endpoint get subscription company details
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpGet("subscription/company/{tenantId}/history")]
    [ProducesResponseType(typeof(ApiResponse<Page<TenantSubscriptionDetail>>), 200)]
    public async Task<IActionResult> GetTenantSubscriptionHistory([FromRoute] string tenantId, [FromQuery] FilterSubscriptionCompany model)
    {
        // Extract subdomain from HTTP request headers
        var subdomain = HttpContext.Request.Headers["subdomain"];

        var response = await _subscriptionService.GetTenantSubscriptionHistory(tenantId, model, subdomain);
        return ParseResponse(response);
    }

    /// <summary>
    /// This endpoint get subscription company details
    /// </summary>
    /// <returns></returns>
    [HttpGet("project/count")]
    [ProducesResponseType(typeof(ApiResponse<List<ProjectCount>>), 200)]
    public async Task<IActionResult> GetProjectCount()
    {
        // Extract subdomain from HTTP request headers
        var subdomain = HttpContext.Request.Headers["subdomain"];

        var response = await _projectService.GetProjectCount(subdomain);
        return ParseResponse(response);
    }


    /// <summary>
    /// This endpoint get subscription company details
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpGet("project/performance")]
    [ProducesResponseType(typeof(ApiResponse<List<TopPerformingCompany>>), 200)]
    public async Task<IActionResult> GetTopPerformingCompaniesPercentage()
    {
        // Extract subdomain from HTTP request headers
        var subdomain = HttpContext.Request.Headers["subdomain"];

        var response = await _projectService.GetTopPerformingCompaniesPercentage(subdomain);
        return ParseResponse(response);
    }

    /// <summary>
    /// This endpoint get subscription company details
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpGet("project/company")]
    [ProducesResponseType(typeof(ApiResponse<PagedProjectCompanyDetail>), 200)]
    public async Task<IActionResult> GetProjectCompanyDetail([FromQuery][Required] FilterProject model)
    {
        // Extract subdomain from HTTP request headers
        var subdomain = HttpContext.Request.Headers["subdomain"];

        var response = await _projectService.GetProjectCompanyDetail(model.Filter, model.PageSize, model.Page, subdomain);
        return ParseResponse(response);
    }
}
