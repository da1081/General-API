using System.ComponentModel.DataAnnotations;
using Validators;

namespace Models.AuthenticationModels
{
    public class RegisterModel
    {
        [IdentityUsernameValidation]
        [Required(ErrorMessage = "Username is required")]
        public string? Username { get; set; }

        [EmailAddress]
        [Required(ErrorMessage = "Email is required")]
        public string? Email { get; set; }

        [IdentityPasswordValidation]
        [Required(ErrorMessage = "Password is required")]
        public string? Password { get; set; }
    }
}
