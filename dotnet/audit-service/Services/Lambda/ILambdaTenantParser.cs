using Amazon.Lambda.APIGatewayEvents;

namespace audit_service.Services.Lambda
{
    public interface ILambdaTenantParser
    {
        string GetTenant(APIGatewayProxyRequest request);
    }
}