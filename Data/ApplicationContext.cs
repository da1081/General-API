using Data.Entities;
using Data.Entities.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Data
{
    // Migration creation and DB-Update Notes:
    // Refrence: https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/?tabs=dotnet-core-cli
    //      1.  Add-Migration InitialCreate
    //      2.  Update-Database

    public class ApplicationContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        public DbSet<Sms> Sms { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public ApplicationContext()
        {
        }

        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
        }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>(b =>
            {
                // Each User can have many UserClaims
                b.HasMany(e => e.Claims)
                    .WithOne()
                    .HasForeignKey(uc => uc.UserId)
                    .IsRequired();

                // Each User can have many UserLogins
                b.HasMany(e => e.Logins)
                    .WithOne()
                    .HasForeignKey(ul => ul.UserId)
                    .IsRequired();

                // Each User can have many UserTokens
                b.HasMany(e => e.Tokens)
                    .WithOne()
                    .HasForeignKey(ut => ut.UserId)
                    .IsRequired();

                // Each User can have many entries in the UserRole join table
                b.HasMany(e => e.UserRoles)
                    .WithOne(e => e.User)
                    .HasForeignKey(ur => ur.UserId)
                    .IsRequired();
            });

            modelBuilder.Entity<ApplicationRole>(b =>
            {
                // Each Role can have many entries in the UserRole join table
                b.HasMany(e => e.UserRoles)
                    .WithOne(e => e.Role)
                    .HasForeignKey(ur => ur.RoleId)
                    .IsRequired();
            });
        }

        public override int SaveChanges()
        {
            ApplyDateAuditDetails();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            ApplyDateAuditDetails();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void ApplyDateAuditDetails()
        {
            var now = DateTime.UtcNow;
            foreach (var changedEntity in ChangeTracker.Entries())
            {
                if (changedEntity.Entity.GetType().IsSubclassOf(typeof(BaseEntity)))
                {
                    switch (changedEntity.State)
                    {
                        case EntityState.Added:
                            changedEntity.Property("AddedDate").CurrentValue = now;
                            changedEntity.Property("ModifiedDate").CurrentValue = now;
                            break;
                        case EntityState.Modified:
                            changedEntity.Property("ModifiedDate").CurrentValue = now;
                            break;
                    }
                }
            }
        }
    }
}
