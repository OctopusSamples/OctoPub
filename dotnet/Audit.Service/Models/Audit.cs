using System.ComponentModel.DataAnnotations.Schema;

namespace Audit.Service.Models
{
    [Table("audits")]
    public class Audit
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? Id { get; set; }

        public string? Branch { get; set; }

        public string DataPartition { get; set; } = string.Empty;

        public string Action { get; set; } = string.Empty;

        public string Subject { get; set; } = string.Empty;

        public string Object { get; set; } = string.Empty;
    }
}