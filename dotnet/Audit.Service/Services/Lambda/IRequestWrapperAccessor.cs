using Amazon.Lambda.APIGatewayEvents;
using Audit.Service.Lambda;

namespace Audit.Service.Services.Lambda
{
    /// <summary>
    /// Captures the Lambda HTTP proxy request so other services can access it.
    /// </summary>
    public interface IRequestWrapperAccessor
    {
        /// <summary>
        /// Returns the Lambda HTTP proxy request details.
        /// </summary>
        RequestWrapper RequestWrapper { get; set; }
    }
}