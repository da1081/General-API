using Data.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Services.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthenticationService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<bool> IsUserLockedoutAsync(ApplicationUser user)
        {
            var now = DateTimeOffset.UtcNow;

            // User is new, 'LockedOutEnd'-property has not yet been set.
            if (user.LockoutEnd is null)
            {
                user.LockoutEnd = now;
                await _userManager.SetLockoutEndDateAsync(user, now);
            }

            // Lockout is dissabled but should be enabled becuase of the LockoutEnd datetime.
            if (!user.LockoutEnabled && user.LockoutEnd >= now)
            {
                user.LockoutEnabled = true;
                await _userManager.SetLockoutEnabledAsync(user, true);
            }

            // If user is already lockedout.
            if (user.LockoutEnabled)
            {
                // Check if user should continue to be locked out.
                if (user.LockoutEnd <= now)
                {
                    user.LockoutEnabled = false;
                    await _userManager.SetLockoutEnabledAsync(user, false);
                }
                else
                {
                    return true;
                }
            }
            return false;
        }

        public async Task<JwtSecurityToken> CreateJwtSecurityTokenAsync(ApplicationUser user, string issuer, string audience, string secret)
        {
            // Create identity claims.
            List<Claim> identityClaims = new()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            IList<string> userRoles = await _userManager.GetRolesAsync(user);
            foreach (string roleName in userRoles)
                identityClaims.Add(new Claim(ClaimTypes.Role, roleName));

            // Create token.
            JwtSecurityToken rawToken = new(
                issuer: issuer,
                audience: audience,
                expires: DateTime.Now.AddHours(3),
                claims: identityClaims,
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                    SecurityAlgorithms.HmacSha512Signature));

            return rawToken;
        }
    }
}
