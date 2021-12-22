using System.ComponentModel.DataAnnotations.Schema;

namespace Audit.Service.Domain.Entities
{
    /// <summary>
    /// Represents an audit database entity.
    /// </summary>
    [Table("audits")]
    public class Audit
    {
        /// <summary>
        /// Gets or sets the audit primary key.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? Id { get; set; }

        /// <summary>
        /// Gets or sets the feature branch details that created the audit.
        /// </summary>
        public string? Branch { get; set; }

        /// <summary>
        /// Gets or sets the data partition the audit belongs to.
        /// </summary>
        public string DataPartition { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the action associated with the audit.
        /// </summary>
        public string Action { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the subject that performed the action on the object.
        /// </summary>
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the object that had an action performed on it by the subject.
        /// </summary>
        public string Object { get; set; } = string.Empty;
    }
}