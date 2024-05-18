using Configurations.Utility;
using SuperAdmin.Service.Database.Entities;
using SuperAdmin.Service.Models.Dtos;
using SuperAdmin.Service.Models.Dtos.UserDomain;

namespace SuperAdmin.Service.Services.Contracts
{
    public interface IUserService
    {
        Task<ApiResponse<PaginationResult<UserDto>>> GetUsers(FilterUserBy filterBy);
        Task<ApiResponse<dynamic>> CreateUser(CreateUser model);
        Task<ApiResponse<dynamic>> UpdateUser(string userId, UpdateUser model);
        Task<ApiResponse<dynamic>> ResetUserPassword(string userId, string password);
        Task<ApiResponse<dynamic>> SuspendUsers(SuspendUser model);
        Task<ApiResponse<dynamic>> ReactivateSuspendedUsers(ReactivateUser model);
        Task<ApiResponse<dynamic>> DeleteUser(string id);
        Task<ApiResponse<PaginationResult<UserDto>>> GetDeletedUsers(FilterUserBy filterBy);
        Task<ApiResponse<PaginationResult<UserDto>>> GetSuspendedUsers(FilterUserBy filterBy);
        Task<User?> GetUser(string id);
    }
}
