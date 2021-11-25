using Amazon.Lambda.APIGatewayEvents;
using Microsoft.AspNetCore.Mvc;

namespace Audit.Service.Controllers
{
    public class ActionResultConverter : ContentResult
    {
        public ActionResultConverter(APIGatewayProxyResponse apiGatewayProxyResponse)
        {
            StatusCode = apiGatewayProxyResponse.StatusCode;
            ContentType = Constants.JsonApiMimeType;
            Content = apiGatewayProxyResponse.Body;
        }
    }
}