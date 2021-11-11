using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace audit_service.Models
{
    public class Audit : Identifiable<int>
    {
        [Attr] public string Tenant { get; set; }

        [Attr] public string Action { get; set; }

        [HasOne] public Urn Subject { get; set; }

        [HasOne] public Urn Object { get; set; }
    }
}