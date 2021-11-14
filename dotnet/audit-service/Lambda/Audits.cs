using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

namespace audit_service.Lambda
{
    public class Audits
    {
        [LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
        public APIGatewayProxyResponse AuditsApi(APIGatewayProxyRequest request, ILambdaContext context)
        {
            return new APIGatewayProxyResponse
            {
                Body = "OK",
                StatusCode = 200
            };
        }
    }
}