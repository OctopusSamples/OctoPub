using Amazon.Lambda.APIGatewayEvents;

namespace Audit.Service.Services.Lambda
{
    /// <summary>
    /// A simple wrapper around a APIGatewayProxyRequest instance.
    /// </summary>
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