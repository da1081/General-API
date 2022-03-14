using System.ComponentModel.DataAnnotations;
using Validators;

namespace Models.AuthenticationModels
{
    public class LoginModel
    {
        [IdentityUsernameValidation]
        [Required(ErrorMessage = "Username is required")]
        public string? Username { get; set; }

        [IdentityPasswordValidation]
        [Required(ErrorMessage = "Password is required")]
        public string? Password { get; set; }
    }
}
