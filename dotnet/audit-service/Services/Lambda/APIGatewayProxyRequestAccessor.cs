using Amazon.Lambda.APIGatewayEvents;

namespace audit_service.Services.Lambda
{
    public class ApiGatewayProxyRequestAccessor : IApiGatewayProxyRequestAccessor
    {
        private APIGatewayProxyRequest _apiGatewayProxyRequest;

        public ApiGatewayProxyRequestAccessor(APIGatewayProxyRequest apiGatewayProxyRequest)
        {
            _apiGatewayProxyRequest = apiGatewayProxyRequest;
        }

        public APIGatewayProxyRequest ApiGatewayProxyRequest => _apiGatewayProxyRequest;
    }
}