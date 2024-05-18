using Configurations.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SuperAdmin.Service.Database;
using SuperAdmin.Service.Database.Entities;
using SuperAdmin.Service.Extensions;
using SuperAdmin.Service.Helpers;
using SuperAdmin.Service.Models.Dtos.RoleDomain;
using SuperAdmin.Service.Services.Contracts;

namespace SuperAdmin.Service.Services.Implementations
{
    public class RoleService : IRoleService
    {
        private readonly RoleManager<Role> _roleManager;
        private readonly AppDbContext _appDbContext;

        public RoleService(RoleManager<Role> roleManager, AppDbContext appDbContext)
        {
            _roleManager = roleManager;
            _appDbContext = appDbContext;
        }

        /// <summary>
        /// This method creates a new role and assigns permissions 
        /// </summary>
        /// <param name="model"></param>
        public async Task<ApiResponse<RoleDto>> CreateRole(CreateRole model)
        {
            if (string.IsNullOrWhiteSpace(model.RoleName) || model.PermissionIds.IsNullOrEmpty())
            {
                return new ApiResponse<RoleDto>
                {
                    ResponseCode = "400",
                    ResponseMessage = "Please provide required parameters"
                };
            }

            bool roleExists = await _roleManager.RoleExistsAsync(model.RoleName);
            if (roleExists)
            {
                return new ApiResponse<RoleDto>
                {
                    ResponseCode = "400",
                    ResponseMessage = $"Role name '{model.RoleName}' already exists"
                };
            }

            var permissions = _appDbContext.Permissions.Where(x => model.PermissionIds.Contains(x.Id));
            if (!permissions.Any())
            {
                return new ApiResponse<RoleDto>
                {
                    ResponseCode = "400",
                    ResponseMessage = "Role cannot be created without an assigned permission."
                };
            }

            Role role = new Role
            {
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                Name = model.RoleName,
                NormalizedName = model.RoleName.ToUpper(),
            };

            var result = await _roleManager.CreateAsync(role);
            if (!result.Succeeded)
            {
                return new ApiResponse<RoleDto>
                {
                    ResponseCode = "400",
                    ResponseMessage = string.Join(".", result.Errors.Select(x => x.Description))
                };
            }

            foreach (var permission in permissions)
            {
                _appDbContext.RolePermissions.Add(new RolePermission
                {
                    PermissionId = permission.Id,
                    RoleId = role.Id
                });
            }

            await _appDbContext.SaveChangesAsync();
            return new ApiResponse<RoleDto>
            {
                Data = new RoleDto
                {
                    Id = role.Id,
                    Name = role.Name,
                    DateCreatedInEpochMilliseconds = role.CreatedAt.ToEpochTimestampInMilliseconds()
                },
                ResponseCode = "200",
                ResponseMessage = $"Created {role.Name} role successfully"
            };
        }

        /// <summary>
        /// This method deletes a role
        /// </summary>
        /// <param name="id"></param>
        public async Task<ApiResponse<dynamic>> DeleteRole(string id)
        {
            var role = await _appDbContext.Roles.FindAsync(id);

            if (role == null)
            {
                return new ApiResponse<dynamic>
                {
                    ResponseCode = "404",
                    ResponseMessage = "Role does not exist"
                };
            }

            if (role.Name == HelperConstants.SUPER_ADMIN_ROLE || role.Name == HelperConstants.SUSPENSION_ROLE)
            {
                return new ApiResponse<dynamic>
                {
                    ResponseCode = "409",
                    ResponseMessage = $"{role.Name} cannot be deleted"
                };
            }

            role.IsDeleted = true;
            var result = await _roleManager.UpdateAsync(role);
            if (!result.Succeeded)
            {
                return new ApiResponse<dynamic>
                {
                    ResponseCode = "400",
                    ResponseMessage = string.Join(".", result.Errors.Select(x => x.Description))
                };
            }

            return new ApiResponse<dynamic>
            {
                ResponseCode = "200",
                ResponseMessage = "Deleted role successfully"
            };
        }

        /// <summary>
        /// This method gets all roles
        /// </summary>
        /// <returns></returns>
        public async Task<ApiResponse<IEnumerable<RoleDto>>> GetRoles()
        {
            var roles = await _appDbContext.Roles.Where(x => x.Name != HelperConstants.SUSPENSION_ROLE && !x.IsDeleted)
                                                        .Select(x => new RoleDto
                                                        {
                                                            Id = x.Id,
                                                            Name = x.Name!,
                                                            DateCreatedInEpochMilliseconds = x.CreatedAt.ToEpochTimestampInMilliseconds()
                                                        })
                                                        .ToListAsync();

            return new ApiResponse<IEnumerable<RoleDto>>
            {
                Data = roles,
                ResponseCode = "200"
            };
        }

        /// <summary>
        /// This method gets permissions assigned to a role
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public async Task<ApiResponse<RolePermissionDto>> GetPermissionsForARole(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role is null || role.IsDeleted)
            {
                return new ApiResponse<RolePermissionDto>
                {
                    ResponseCode = "404",
                    ResponseMessage = "Role not found"
                };
            }

            var permissions = await _appDbContext.RolePermissions.Where(x => x.RoleId == roleId).Include(x => x.Permission)
                                                                                    .Select(x => x.Permission.Description).ToArrayAsync();

            var rolePermissions = new RolePermissionDto
            {
                RoleName = role.Name!,
                Permissions = permissions
            };

            return new ApiResponse<RolePermissionDto>
            {
                ResponseCode = "200",
                Data = rolePermissions
            };
        }
    }
}
