using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Audit.Service.Models
{
    public class Audit
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? Id { get; set; }

        public string Branch { get; set; }

        public string Tenant { get; set; }

        public string Action { get; set; }

        public string Subject { get; set; }

        public string Object { get; set; }
    }
}