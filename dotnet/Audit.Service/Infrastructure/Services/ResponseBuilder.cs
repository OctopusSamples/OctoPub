using System;
using Amazon.Lambda.APIGatewayEvents;

namespace Audit.Service.Infrastructure.Services
{
    /// <summary>
    /// A service used to build responses.
    /// </summary>
    public class ResponseBuilder : IResponseBuilder
    {
        /// <summary>
        /// Builds a response for server side errors.
        /// </summary>
        /// <param name="ex">The exception that was thrown.</param>
        /// <returns>The response.</returns>
        public APIGatewayProxyResponse BuildError(Exception ex)
        {
            return new APIGatewayProxyResponse
            {
                Body = System.Text.Json.JsonSerializer.Serialize(new
                {
                    errors = new[]
                    {
                        new { code = ex.GetType().Name }
                    }
                }),
                StatusCode = 500
            };
        }

        /// <summary>
        /// Builds a response for client side errors.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <returns>The response.</returns>
        public APIGatewayProxyResponse BuildClientError(string message)
        {
            return new APIGatewayProxyResponse
            {
                Body = System.Text.Json.JsonSerializer.Serialize(new
                {
                    errors = new[]
                    {
                        new { title = message }
                    }
                }),
                StatusCode = 401
            };
        }

        /// <summary>
        /// Returns a 404 not found response.
        /// </summary>
        /// <returns>The 404 response.</returns>
        public APIGatewayProxyResponse BuildNotFound()
        {
            return new APIGatewayProxyResponse
            {
                Body = System.Text.Json.JsonSerializer.Serialize(new
                {
                    errors = new[]
                    {
                        new { title = "Resource was not found" }
                    }
                }),
                StatusCode = 404
            };
        }
    }
}