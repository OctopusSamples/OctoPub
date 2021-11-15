using Amazon.Lambda.APIGatewayEvents;

namespace audit_service.Services.Lambda
{
    public interface IApiGatewayProxyRequestAccessor
    {
        APIGatewayProxyRequest ApiGatewayProxyRequest { get; }
    }
}