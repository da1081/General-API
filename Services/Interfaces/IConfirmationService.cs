using Data.Entities.Identity;

namespace Services.Interfaces
{
    public interface IConfirmationService
    {
        Task<bool> ConfirmationFailedAsync(ApplicationUser user);
        Task<string> CreateEmailConfirmationPinAsync(ApplicationUser user, string token);
        Task<string> CreatePasswordResetMailAsync(ApplicationUser user, string token);
        string CreatePasswordResetSmsAsync(ApplicationUser user, string token);
        string CreatePhoneConfirmationPinAsync(ApplicationUser user, string token);
    }
}
