using System;
using Amazon.Lambda.APIGatewayEvents;

namespace Audit.Service.Services
{
    /// <summary>
    /// Defines a service for building common JSONAPI responses.
    /// </summary>
    public interface IResponseBuilder
    {
        /// <summary>
        /// Build a server side error.
        /// </summary>
        /// <param name="ex">The exception that was thrown.</param>
        /// <returns>A 500 response.</returns>
        APIGatewayProxyResponse BuildError(Exception ex);

        /// <summary>
        /// Build a client side error.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <returns>A 401 response.</returns>
        APIGatewayProxyResponse BuildClientError(string message);

        /// <summary>
        /// Builds a resource not found error.
        /// </summary>
        /// <returns>The 404 response.</returns>
        APIGatewayProxyResponse BuildNotFound();
    }
}