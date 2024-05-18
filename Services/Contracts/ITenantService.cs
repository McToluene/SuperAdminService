using Configurations.Utility;

namespace SuperAdmin.Service.Services.Contracts
{
	public interface ITenantService
	{
        Task<ApiResponse<List<GetTenantResponse>>> GetTenants();
        Task<ApiResponse<GetTenantResponse>> GetTenant(string id);
    }
}

