using System.ComponentModel.DataAnnotations;

namespace Models.AuthenticationModels
{
    public class TotpLoginMailModel
    {
        [Required, EmailAddress]
        public string? Email { get; set; }

        [Required]
        public string? Pin { get; set; }
    }
}
