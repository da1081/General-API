using Microsoft.AspNetCore.Identity;

namespace Data.Entities.Identity
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public int ConfirmationFailedCount { get; set; }

        public ICollection<Sms>? Smses { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public virtual ICollection<IdentityUserClaim<Guid>> Claims { get; set; }
        public virtual ICollection<IdentityUserLogin<Guid>> Logins { get; set; }
        public virtual ICollection<IdentityUserToken<Guid>> Tokens { get; set; }
        public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    }
}
