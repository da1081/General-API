using Data.Entities.Identity;

namespace Services.Interfaces
{
    public interface IPasswordService
    {
        Task<string> CreateEmailTemporaryPasswordPinAsync(ApplicationUser user, string token);
        string CreatePhoneTemporaryPasswordPinAsync(ApplicationUser user, string token);
    }
}
