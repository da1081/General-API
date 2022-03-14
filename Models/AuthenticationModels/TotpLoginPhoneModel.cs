using System.ComponentModel.DataAnnotations;

namespace Models.AuthenticationModels
{
    public class TotpLoginPhoneModel
    {
        [Required, Phone]
        public string? PhoneNumber { get; set; }

        [Required]
        public string? Pin { get; set; }
    }
}
