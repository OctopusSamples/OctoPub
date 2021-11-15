using Amazon.Lambda.APIGatewayEvents;

namespace Audit.Service.Services.Lambda
{
    public interface IApiGatewayProxyRequestAccessor
    {
        APIGatewayProxyRequest ApiGatewayProxyRequest { get; }
    }
}