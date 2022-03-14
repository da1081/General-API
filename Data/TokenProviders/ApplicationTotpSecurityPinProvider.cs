using Data.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace Data.TokenProviders
{
    // To Generate Pin: _userManager.GenerateUserTokenAsync(user, PinTokenProvider.ProviderName, "personal-pin-provider");
    // To Validate Pin: _userManager.ValidateUserTokenAsync(user, pin);
    public class SecurityPinProvider : ApplicationTotpSecurityPinProvider<ApplicationUser>
    {
        public static readonly string TotpProvider = "TotpSecurityPinProvider";
        public static readonly string EmailProvider = "EmailSecurityPinProvider";
        public static readonly string PhoneProvider = "PhoneSecurityPinProvider";
        public static readonly string PasswordResetProvider = "PasswordResetSecurityPinProvider";
    }

    public class ApplicationTotpSecurityPinProvider<TUser> : TotpSecurityStampBasedTokenProvider<TUser> where TUser : class
    {
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override async Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<TUser> manager, TUser user) => false;
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

        public override async Task<string> GenerateAsync(string purpose, UserManager<TUser> manager, TUser user)
        {
            var token = new SecurityToken(await manager.CreateSecurityTokenAsync(user));
            var modifier = await GetUserModifierAsync(purpose, manager, user);
            var code = Rfc6238AuthenticationService.GenerateCode(token, modifier, 8);
            return $"{code:00000000}";
        }

        public override async Task<bool> ValidateAsync(string purpose, string token, UserManager<TUser> manager, TUser user)
        {
            if (!Int32.TryParse(token, out int code))
                return false;
            var securityToken = new SecurityToken(await manager.CreateSecurityTokenAsync(user));
            var modifier = await GetUserModifierAsync(purpose, manager, user);
            var valid = Rfc6238AuthenticationService.ValidateCode(securityToken, code, modifier, token.Length);
            return valid;
        }

        public override Task<string> GetUserModifierAsync(string purpose, UserManager<TUser> manager, TUser user)
        {
            return base.GetUserModifierAsync(purpose, manager, user);
        }
    }
}
