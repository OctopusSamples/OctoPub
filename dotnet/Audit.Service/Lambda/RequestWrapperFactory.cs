using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.SQSEvents;

namespace Audit.Service.Lambda
{
    /// <summary>
    /// This service supports access via HTTP and async messages. Regardless of the access type, all requests
    /// are processed as if they were RESTful JSONAPI requests.
    ///
    /// The RequestWrapper class is used to describe the intent of a request, without being tied to any particular
    /// protocol. Incoming requests are converted to a RequestWrapper, and the RequestWrapper is then used to
    /// satisfy the request.
    ///
    /// This factory class provides methods for creating RequestWrapper instances from the various inputs available
    /// from HTTP or async message requests.
    /// </summary>
    public static class RequestWrapperFactory
    {
        private static readonly int DefaultId = -1;
        private static readonly string HealthEndpoint = "/health";
        private static readonly string AuditEndpoint = "/api/audits";
        private static readonly Regex EntityCollectionRe = new Regex("^/api/audits/?$");
        private static readonly Regex SingleEntityRe = new Regex("^/api/audits/(?<id>\\d+)/?$");

        /// <summary>
        /// Convert an API Gateway proxy request to a RequestWrapper.
        /// </summary>
        /// <param name="request">The standard API Gateway proxy input.</param>
        /// <returns>The equivalent RequestWrapper</returns>
        public static RequestWrapper CreateFromHttpRequest(APIGatewayProxyRequest request)
        {
            return new RequestWrapper
            {
                Entity = GetBody(request),
                ActionType = ActionTypeFromHttpMethod(request.HttpMethod, request.Path),
                EntityType = request.Path?.StartsWith(HealthEndpoint) ?? false
                    ? EntityType.Health
                    : request.Path?.StartsWith(AuditEndpoint) ?? false
                        ? EntityType.Audit
                        : EntityType.None,
                Id = SingleEntityRe.IsMatch(request.Path ?? string.Empty)
                    ? Int32.Parse(SingleEntityRe.Match(request.Path ?? "").Groups["id"].Value)
                    : DefaultId,
                Tenant = GetTenant((request.Headers ?? new Dictionary<string, string>())
                    .Where(h => h.Key.ToLower() == Constants.AcceptHeader)
                    .Select(h => h.Value))
            };
        }

        /// <summary>
        /// Convert an SQS message request to a RequestWrapper.
        /// </summary>
        /// <param name="message">The SQS message</param>
        /// <returns>The equivalent RequestWrapper</returns>
        public static RequestWrapper CreateFromSqsMessage(SQSEvent.SQSMessage message)
        {
            return new RequestWrapper
            {
                Entity = message.Body,
                ActionType =
                    Enum.TryParse<ActionType>(GetAttribute(message.MessageAttributes, "action"), out var actionType)
                        ? actionType
                        : ActionType.None,
                EntityType =
                    Enum.TryParse<EntityType>(GetAttribute(message.MessageAttributes, "entity"), out var entity)
                        ? entity
                        : EntityType.None,
                Id = Int32.TryParse(GetAttribute(message.MessageAttributes, "id"), out var id)
                    ? id
                    : DefaultId,
                Tenant = message.Attributes?.ContainsKey("tenant") ?? false
                    ? message.Attributes["tenant"]
                    : Constants.DefaultTenant
            };
        }

        static string GetAttribute(Dictionary<string, SQSEvent.MessageAttribute> attributes, string key)
        {
            return attributes?.ContainsKey(key) ?? false ? attributes[key].StringValue ?? string.Empty : string.Empty;
        }

        /// <summary>
        /// Convert HTTP methods to their CRUD actions.
        /// </summary>
        /// <param name="method">The HTTP method</param>
        /// <returns>The equivalent CRUD action</returns>
        static ActionType ActionTypeFromHttpMethod(string method, string path)
        {
            var fixedMethod = method?.ToLower() ?? string.Empty;;
            if (fixedMethod == "get" && EntityCollectionRe.IsMatch(path))
            {
                return ActionType.ReadAll;
            }

            return fixedMethod switch
            {
                "post" => ActionType.Create,
                "delete" => ActionType.Delete,
                "patch" => ActionType.Update,
                "get" => ActionType.Read,
                _ => ActionType.None
            };
        }

        /// <summary>
        /// Extract the body of a HTTP request.
        /// </summary>
        /// <param name="request">The API Gateway proxy request.</param>
        /// <returns>The unencoded request body</returns>
        static string GetBody(APIGatewayProxyRequest request)
        {
            return request.IsBase64Encoded
                ? Encoding.UTF8.GetString(Convert.FromBase64String(request.Body))
                : request.Body;
        }

        /// <summary>
        /// Extract the tenant from the request.
        /// </summary>
        /// <param name="acceptHeader">The Http request accept header</param>
        /// <returns>The custom tenant name, or the default tenant if no specific value was provided.</returns>
        static string GetTenant(IEnumerable<string> acceptHeader)
        {
            var versions = (acceptHeader ?? Enumerable.Empty<string>())
                .SelectMany(v => v.Split(";"))
                // trim the results and make them lowercase
                .Select(v => v.Trim().ToLower())
                // find any header value segments that indicate the tenant
                .Where(v => v.StartsWith("tenant="))
                // split those values on the equals
                .Select(v => v.Split("="))
                // validate that the results have 2 elements
                .Where(v => v.Length == 2)
                .ToList();

            var appVersion = versions
                // find any header value segments that indicate the tenant
                .Where(v => v[0].Trim().StartsWith(Constants.AcceptVersionInfo + "="))
                // get the second element
                .Select(v => v[1].Trim())
                // if nothing was found, we assume we are the default tenant
                .FirstOrDefault();

            var tenantVersion = versions
                // find any header value segments that indicate the tenant
                .Where(v => v[0].Trim().StartsWith(Constants.AcceptTenantInfo + "="))
                // get the second element
                .Select(v => v[1].Trim())
                // if nothing was found, we assume we are the default tenant
                .FirstOrDefault();

            return tenantVersion ?? appVersion ?? Constants.DefaultTenant;
        }
    }
}