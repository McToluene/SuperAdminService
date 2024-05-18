using SuperAdmin.Service.Database.Entities;
using SuperAdmin.Service.Models.Dtos.AuthDomain;

namespace SuperAdmin.Service.Extensions
{
    public static class Mapper
    {
        public static AuthenticatedUser ToAuthenticatedUser(this User user, string token, IList<string> roles)
        {
            return new AuthenticatedUser
            {
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Roles = roles.ToArray(),
                Token = token,
                UserId = user.Id
            };
        }
    }
}
