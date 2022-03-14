using System.ComponentModel.DataAnnotations;

namespace Models.SingleDataModels
{
    public class PhoneNumberModel
    {
        [Required]
        [DataType(DataType.PhoneNumber)]
        public string? PhoneNumber { get; set; }
    }
}
