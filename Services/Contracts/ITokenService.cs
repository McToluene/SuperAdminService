using SuperAdmin.Service.Database.Entities;

namespace SuperAdmin.Service.Services.Contracts
{
    public interface ITokenService
    {
        Task<(bool isSuccessful, string token)> GenerateToken(User user);
    }
}
