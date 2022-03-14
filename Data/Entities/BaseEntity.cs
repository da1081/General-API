using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class BaseEntity
    {
        [Key, Required, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual Guid Id { get; set; }

        [Timestamp]
        public virtual byte[]? RowVersion { get; set; }

        public virtual DateTime AddedDate { get; set; }

        public virtual DateTime ModifiedDate { get; set; }
    }
}
