using Microsoft.AspNetCore.Identity;

namespace Data.Entities.Identity
{
    public class ApplicationRole : IdentityRole<Guid>
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public ApplicationRole() : base()
        {
        }

        public ApplicationRole(string roleName) : base(roleName)
        {
        }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }
    }
}
