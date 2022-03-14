using Data.Entities.Identity;

namespace Services.Interfaces
{
    public interface ISmsService
    {
        Task SendSms(ApplicationUser user, string contentStr);
    }
}
