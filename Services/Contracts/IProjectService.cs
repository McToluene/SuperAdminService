using Configurations.Utility;
using Identification;
using SuperAdmin.Service.Models.Dtos.ProjectDomain;


namespace SuperAdmin.Service.Services.Contracts;

public interface IProjectService
{
    Task<ApiResponse<List<ProjectCount>>> GetProjectCount(string? subdomain);
    Task<ApiResponse<List<TopPerformingCompany>>> GetTopPerformingCompaniesPercentage(string? subdomain);
    Task<ApiResponse<Page<CompanyDetailDto>>> GetProjectCompanyDetail(CompanyProjectFilter filter, int? pageSize, int? pageNumber, string? subdomain);
}