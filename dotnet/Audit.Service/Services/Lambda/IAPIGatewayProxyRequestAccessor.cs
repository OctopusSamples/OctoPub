using Amazon.Lambda.APIGatewayEvents;

namespace Audit.Service.Services.Lambda
{
    /// <summary>
    /// Captures the Lambda HTTP proxy request so other services can access it.
    /// </summary>
    public interface IApiGatewayProxyRequestAccessor
    {
        /// <summary>
        /// Returns the Lambda HTTP proxy request details.
        /// </summary>
        APIGatewayProxyRequest ApiGatewayProxyRequest { get; }
    }
}