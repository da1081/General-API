using Data.Entities.Identity;
using System.IdentityModel.Tokens.Jwt;

namespace Services.Interfaces
{
    public interface IAuthenticationService
    {
        Task<JwtSecurityToken> CreateJwtSecurityTokenAsync(ApplicationUser user, string issuer, string audience, string secret);
        Task<bool> IsUserLockedoutAsync(ApplicationUser user);
    }
}
