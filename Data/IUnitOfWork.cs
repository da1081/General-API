using Data.Entities;
using Data.Entities.Identity;
using Data.Repositories;

namespace Data
{
    public interface IUnitOfWork
    {
        GenericRepository<Sms> SmsRepository { get; }
        GenericRepository<ApplicationUser> ApplicationUserRepository { get; }

        int Save();
        Task<int> SaveAsync();
    }
}
