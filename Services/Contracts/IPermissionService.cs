using Configurations.Utility;
using SuperAdmin.Service.Models.Dtos.PermissionDomain;

namespace SuperAdmin.Service.Services.Contracts
{
    public interface IPermissionService
    {
        Task<ApiResponse<IEnumerable<PermissionDto>>> GetPermissions();
    }
}
