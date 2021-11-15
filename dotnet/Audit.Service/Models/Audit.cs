using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Audit.Service.Models
{
    public class Audit : Identifiable<int>
    {
        [Attr] public string Tenant { get; set; }

        [Attr] public string Action { get; set; }

        [HasOne] public string Subject { get; set; }

        [HasOne] public string Object { get; set; }
    }
}