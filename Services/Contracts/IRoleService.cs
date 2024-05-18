using Configurations.Utility;
using SuperAdmin.Service.Models.Dtos.RoleDomain;

namespace SuperAdmin.Service.Services.Contracts
{
    public interface IRoleService
    {
        Task<ApiResponse<IEnumerable<RoleDto>>> GetRoles();
        Task<ApiResponse<RoleDto>> CreateRole(CreateRole model);
        Task<ApiResponse<dynamic>> DeleteRole(string id);
        Task<ApiResponse<RolePermissionDto>> GetPermissionsForARole(string roleId);
    }
}
