using System.ComponentModel.DataAnnotations;

namespace Models.SingleDataModels
{
    public class EmailModel
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
    }
}
