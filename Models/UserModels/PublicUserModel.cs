
using Data.Entities.Identity;

namespace Models.UserModels
{
    public class PublicUserModel
    {
        public string? Username { get; set; }

        public PublicUserModel()
        {

        }

        public PublicUserModel(ApplicationUser user)
        {
            Username = user.UserName;
        }
    }
}
