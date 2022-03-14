using Data.Entities;
using Data.Entities.Identity;
using Data.Repositories;

namespace Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationContext _context;
        private GenericRepository<ApplicationUser>? applicationUserRepository;
        private GenericRepository<Sms>? smsRepository;

        public UnitOfWork(ApplicationContext context)
        {
            _context = context;
        }

        public GenericRepository<ApplicationUser> ApplicationUserRepository
        {
            get
            {
                if (applicationUserRepository is null)
                    applicationUserRepository = new GenericRepository<ApplicationUser>(_context);
                return applicationUserRepository;
            }
        }

        public GenericRepository<Sms> SmsRepository
        {
            get
            {
                if (smsRepository is null)
                    smsRepository = new GenericRepository<Sms>(_context);
                return smsRepository;
            }
        }

        /// <summary>
        /// Synchronous save - (avoid).
        /// </summary>
        /// <returns>Number of objects written(updated) to the underlying database.</returns>
        public int Save()
        {
            return _context.SaveChanges();
        }

        /// <summary>
        /// Asynchronous save.
        /// </summary>
        /// <returns>Number of objects written(updated) to the underlying database.</returns>
        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
