using Amazon.Lambda.APIGatewayEvents;
using Microsoft.AspNetCore.Mvc;

namespace Audit.Service.Controllers
{
    /// <summary>
    /// A class that builds a new ContentResult object from a APIGatewayProxyResponse.
    /// </summary>
    public class ActionResultConverter : ContentResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActionResultConverter"/> class.
        /// Extracts the values from APIGatewayProxyResponse and places them into this instance.
        /// </summary>
        /// <param name="apiGatewayProxyResponse">The Lambda return object.</param>
        public ActionResultConverter(APIGatewayProxyResponse apiGatewayProxyResponse)
        {
            StatusCode = apiGatewayProxyResponse.StatusCode;
            ContentType = Constants.JsonApiMimeType;
            Content = apiGatewayProxyResponse.Body;
        }
    }
}