
using Data.Entities.Identity;

namespace Models.UserModels
{
    public class PrivateUserModel
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string? Phone { get; set; }
        public bool PhoneConfirmed { get; set; }

        public PrivateUserModel(string username, string email)
        {
            Username = username;
            Email = email;
        }

        public PrivateUserModel(ApplicationUser user)
        {
            Id = user.Id;
            Username = user.UserName;
            Email = user.Email;
            EmailConfirmed = user.EmailConfirmed;
            Phone = user.PhoneNumber;
            PhoneConfirmed = user.PhoneNumberConfirmed;
        }
    }
}
