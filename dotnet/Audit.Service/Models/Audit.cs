namespace Audit.Service.Models
{
    public class Audit
    {
        public int Id { get; set; }

        public string Tenant { get; set; }

        public string Action { get; set; }

        public string Subject { get; set; }

        public string Object { get; set; }
    }
}