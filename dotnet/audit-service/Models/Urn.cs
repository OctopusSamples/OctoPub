using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace audit_service.Models
{
    public class Urn : Identifiable<string>
    {
        [Attr] public string Tenant { get; set; }
    }
}