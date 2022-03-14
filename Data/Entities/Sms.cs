using Data.Entities.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class Sms : BaseEntity
    {
        public string? Sid { get; set; }

        public string? Status { get; set; }

        public string? ToNumber { get; set; }
        public string? Price { get; set; }
        public string? PriceUnit { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
        public string? MessagingServiceSid { get; set; }
        public string? AccountSid { get; set; }
        public string? FromNumber { get; set; }

        // User ref.
        [ForeignKey("ApplicationUser")]
        public Guid ApplicationUserId { get; set; }
        public ApplicationUser? ApplicationUser { get; set; }
    }
}
