using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.SQSEvents;
using Microsoft.AspNetCore.Http;

namespace Audit.Service.Application.Lambda
{
    /// <summary>
    ///     This service supports access via HTTP and async messages. Regardless of the access type, all requests
    ///     are processed as if they were RESTful JSONAPI requests.
    ///     The RequestWrapper class is used to describe the intent of a request, without being tied to any particular
    ///     protocol. Incoming requests are converted to a RequestWrapper, and the RequestWrapper is then used to
    ///     satisfy the request.
    ///     This factory class provides methods for creating RequestWrapper instances from the various inputs available
    ///     from HTTP or async message requests.
    /// </summary>
    public static class RequestWrapperFactory
    {
        private static readonly int DefaultId = -1;
        private static readonly Regex HealthEndpointRe = new Regex("^/health/audits/(GET|POST|[A-Za-z0-9]+/GET)/?$");
        private static readonly string AuditEndpoint = "/api/audits";
        private static readonly Regex EntityCollectionRe = new Regex("^/api/audits/?$");
        private static readonly Regex SingleEntityRe = new Regex("^/api/audits/(?<id>\\d+)/?$");

        /// <summary>
        /// Create a request wrapper from a JSONAPI HTTP request.
        /// </summary>
        /// <param name="request">The HTTP request.</param>
        /// <returns>The wrapper that contains the request context.</returns>
        public static async Task<RequestWrapper> CreateFromHttpRequest(HttpRequest request)
        {
            return new RequestWrapper
            {
                Entity = request.Body != null ? await new StreamReader(request.Body).ReadToEndAsync() : string.Empty,
                ActionType = ActionTypeFromHttpMethod(request.Method, request.Path),
                EntityType = HealthEndpointRe.IsMatch(request.Path)
                    ? EntityType.Health
                    : request.Path.Value?.StartsWith(AuditEndpoint) ?? false
                        ? EntityType.Audit
                        : EntityType.None,
                Id = SingleEntityRe.IsMatch(request.Path.Value ?? string.Empty)
                    ? int.Parse(SingleEntityRe.Match(request.Path.Value ?? string.Empty).Groups["id"].Value)
                    : DefaultId,
                DataPartition = GetDataPartition((request.Headers ?? new HeaderDictionary())
                    .Where(h => h.Key.ToLower() == Constants.AcceptHeader)
                    .SelectMany(h => h.Value)),
                Filter = (request.Query ?? new QueryCollection())
                    .Where(h => h.Key.ToLower() == Constants.FilterQuery)
                    .SelectMany(h => h.Value)
                    .FirstOrDefault() ?? string.Empty
            };
        }

        /// <summary>
        ///     Convert an API Gateway proxy request to a RequestWrapper.
        /// </summary>
        /// <param name="request">The standard API Gateway proxy input.</param>
        /// <returns>The equivalent RequestWrapper.</returns>
        public static RequestWrapper CreateFromHttpRequest(APIGatewayProxyRequest request)
        {
            return new RequestWrapper
            {
                Entity = GetBody(request),
                ActionType = ActionTypeFromHttpMethod(request.HttpMethod, request.Path),
                EntityType = HealthEndpointRe.IsMatch(request.Path)
                    ? EntityType.Health
                    : request.Path?.StartsWith(AuditEndpoint) ?? false
                        ? EntityType.Audit
                        : EntityType.None,
                Id = SingleEntityRe.IsMatch(request.Path ?? string.Empty)
                    ? int.Parse(SingleEntityRe.Match(request.Path ?? string.Empty).Groups["id"].Value)
                    : DefaultId,
                DataPartition = GetDataPartition((request.MultiValueHeaders ?? new Dictionary<string, IList<string>>())
                    .Where(h => h.Key.ToLower() == Constants.AcceptHeader)
                    .SelectMany(h => h.Value)
                    .Union((request.Headers ?? new Dictionary<string, string>())
                        .Where(h => h.Key.ToLower() == Constants.AcceptHeader)
                        .Select(h => h.Value))),
                Filter = (request.MultiValueQueryStringParameters ?? new Dictionary<string, IList<string>>())
                    .Where(h => h.Key.ToLower() == Constants.FilterQuery)
                    .SelectMany(h => h.Value)
                    .Union((request.QueryStringParameters ?? new Dictionary<string, string>())
                        .Where(h => h.Key.ToLower() == Constants.FilterQuery)
                        .Select(h => h.Value))
                    .FirstOrDefault() ?? string.Empty
            };
        }

        /// <summary>
        ///     Convert an SQS message request to a RequestWrapper.
        /// </summary>
        /// <param name="message">The SQS message.</param>
        /// <returns>The equivalent RequestWrapper.</returns>
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
                Id = int.TryParse(GetAttribute(message.MessageAttributes, "id"), out var id)
                    ? id
                    : DefaultId,
                DataPartition = message.MessageAttributes?.ContainsKey("dataPartition") ?? false
                    ? GetDataPartition(message.MessageAttributes["dataPartition"].StringValue.Split(","))
                    : Constants.DefaultPartition,
                Filter = message.MessageAttributes?.ContainsKey(Constants.FilterQuery) ?? false
                    ? message.MessageAttributes[Constants.FilterQuery].StringValue
                    : string.Empty
            };
        }

        /// <summary>
        ///     Extract the tenant from the request.
        /// </summary>
        /// <param name="acceptHeader">The Http request accept header.</param>
        /// <returns>The custom tenant name, or the default tenant if no specific value was provided.</returns>
        public static string GetDataPartition(IEnumerable<string> acceptHeader)
        {
            return (acceptHeader ?? Enumerable.Empty<string>())
                /* Ignore null values */
                .Where(v => v != null)
                /* Split the headers on the comma for multi value headers */
                .SelectMany(v => v.Split(","))
                /* Split the headers on the semi colon */
                .SelectMany(v => v.Split(";"))
                /* trim the results and make them lowercase */
                .Select(v => v.Trim().ToLower())
                /* split those values on the equals */
                .Select(v => v.Split("="))
                /* validate that the results have 2 elements */
                .Where(v => v.Length == 2)
                /* We are interested in results that match the data partition setting */
                .Where(v => v[0].Trim().Equals(Constants.AcceptPartitionInfo, StringComparison.OrdinalIgnoreCase))
                /* get the second element */
                .Select(v => v[1].Trim())
                /* if nothing was found, we assume we are the default tenant */
                .FirstOrDefault() ?? Constants.DefaultPartition;
        }

        private static string GetAttribute(Dictionary<string, SQSEvent.MessageAttribute> attributes, string key)
        {
            return attributes?.ContainsKey(key) ?? false ? attributes[key].StringValue ?? string.Empty : string.Empty;
        }

        /// <summary>
        ///     Convert HTTP methods to their CRUD actions.
        /// </summary>
        /// <param name="method">The HTTP method.</param>
        /// <param name="path">The HTTP path.</param>
        /// <returns>The equivalent CRUD action.</returns>
        private static ActionType ActionTypeFromHttpMethod(string method, string path)
        {
            var fixedMethod = method?.ToLower() ?? string.Empty;
            if (fixedMethod == "get" && EntityCollectionRe.IsMatch(path)) return ActionType.ReadAll;

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
        ///     Extract the body of a HTTP request.
        /// </summary>
        /// <param name="request">The API Gateway proxy request.</param>
        /// <returns>The unencoded request body.</returns>
        private static string GetBody(APIGatewayProxyRequest request)
        {
            return request.IsBase64Encoded
                ? Encoding.UTF8.GetString(Convert.FromBase64String(request.Body))
                : request.Body;
        }
    }
}