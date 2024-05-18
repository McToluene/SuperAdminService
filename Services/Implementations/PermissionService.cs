using Configurations.Utility;
using Microsoft.EntityFrameworkCore;
using SuperAdmin.Service.Database;
using SuperAdmin.Service.Extensions;
using SuperAdmin.Service.Models.Dtos.PermissionDomain;
using SuperAdmin.Service.Services.Contracts;

namespace SuperAdmin.Service.Services.Implementations
{
    public class PermissionService : IPermissionService
    {
        private readonly AppDbContext _appDbContext;
        
        public PermissionService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        /// <summary>
        /// This method gets all role permissions
        /// </summary>
        /// <returns></returns>
        public async Task<ApiResponse<IEnumerable<PermissionDto>>> GetPermissions()
        {
            var permissions = await _appDbContext.Permissions.Select(x => new PermissionDto
                                                                                                        {
                                                                                                            Id = x.Id,
                                                                                                            Category = x.Category,
                                                                                                            Description = x.Description,
                                                                                                            Type = x.Type,
                                                                                                            CreatedAt = x.CreatedAt.ToEpochTimestampInMilliseconds(),
                                                                                                        }).ToListAsync();

            return new ApiResponse<IEnumerable<PermissionDto>>
            {
                Data = permissions,
                ResponseCode = "200"
            };
        }
    }
}
