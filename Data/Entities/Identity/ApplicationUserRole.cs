using Microsoft.AspNetCore.Identity;

namespace Data.Entities.Identity
{
    public class ApplicationUserRole : IdentityUserRole<Guid>
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public virtual ApplicationUser User { get; set; }
        public virtual ApplicationRole Role { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    }
}
