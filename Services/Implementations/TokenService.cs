using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SuperAdmin.Service.Database.Entities;
using SuperAdmin.Service.Extensions;
using SuperAdmin.Service.Models.Configurations;
using SuperAdmin.Service.Services.Contracts;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SuperAdmin.Service.Services.Implementations
{
    public class TokenService : ITokenService
    {
        private readonly Jwt _jwtSettings;
        private readonly UserManager<User> _userManager;

        public TokenService(
            IOptions<Jwt> jwtSettings,
            UserManager<User> userManager)
        {
            _jwtSettings = jwtSettings.Value;
            _userManager = userManager;
        }

        public async Task<(bool isSuccessful, string token)> GenerateToken(User user)
        {
            try
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var claims = await GetClaims(user);

                var securityToken = new JwtSecurityToken(
                    issuer: _jwtSettings.Issuer,
                    audience: _jwtSettings.Audience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes),
                    signingCredentials: credentials
                    );

                string token = new JwtSecurityTokenHandler().WriteToken(securityToken);
                return (true, token);
            }
            catch (Exception)
            {
                return (false, string.Empty);
            }
        }

        /// <summary>
        /// This method gets all user's identity claims
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private async Task<IList<Claim>> GetClaims(User user)
        {
            var identityOptions = new IdentityOptions();
            List<Claim> claims = new()
            {
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToEpochTimestampInSeconds().ToString()),
                new Claim(identityOptions.ClaimsIdentity.UserIdClaimType, user.Id),
                new Claim(identityOptions.ClaimsIdentity.UserNameClaimType, user.UserName!),
            };

            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var userRole in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            return claims;
        }
    }
}
