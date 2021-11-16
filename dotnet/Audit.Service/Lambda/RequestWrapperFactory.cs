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
    /// A factory class providing methods for creating RequestWrapper instances from the various inputs available
    /// from HTTP or async message requests.
    /// </summary>
    public static class RequestWrapperFactory
    {
        private static readonly int DefaultId = -1;
        private static readonly string HealthEndpoint = "/health";
        private static readonly Regex EntityCollectionRe = new Regex("^/api/audits/?$");
        private static readonly Regex SingleEntityRe = new Regex("^/api/audits/(?<id>\\d+)/?$");

        public static RequestWrapper CreateFromHttpRequest(APIGatewayProxyRequest request)
        {
            return new RequestWrapper
            {
                Entity = GetBody(request),
                ActionType = ActionTypeFromHttpMethod(request.HttpMethod),
                EntityType = request.Path?.StartsWith(HealthEndpoint) ?? false
                    ? EntityType.Health
                    : SingleEntityRe.IsMatch(request.Path ?? "")
                        ? EntityType.Individual : EntityType.Collection,
                Id = SingleEntityRe.IsMatch(request.Path ?? "")
                    ? Int32.Parse(SingleEntityRe.Match(request.Path ?? "").Groups["id"].Value)
                    : DefaultId,
                Tenant = GetTenant((request.Headers ?? new Dictionary<string,string>())
                    .Where(h => h.Key.ToLower() == Constants.AcceptHeader)
                    .Select(h => h.Value))
            };
        }

        public static RequestWrapper CreateFromSqsMessage(SQSEvent.SQSMessage message)
        {
            return new RequestWrapper
            {
                Entity = message.Body,
                ActionType = Enum.TryParse<ActionType>(message.Attributes?["action"], out var actionType) ? actionType : ActionType.Create,
                EntityType = Enum.TryParse<EntityType>(message.Attributes?["entity"], out var entity) ? entity : EntityType.Individual,
                Id = Int32.TryParse(message.Attributes?.ContainsKey("id") ?? false ? message.Attributes["id"] : "", out var id) ? id: DefaultId,
                Tenant = message.Attributes?.ContainsKey("tenant") ?? false ? message.Attributes["tenant"] : Constants.DefaultTenant
            };
        }

        static ActionType ActionTypeFromHttpMethod(string method)
        {
            return (method?.ToLower() ?? "") switch
            {
                "post" => ActionType.Create,
                "delete" => ActionType.Delete,
                "patch" => ActionType.Update,
                _ => ActionType.Read
            };
        }

        static string GetBody(APIGatewayProxyRequest request)
        {
            return request.IsBase64Encoded
                ? Encoding.UTF8.GetString(Convert.FromBase64String(request.Body))
                : request.Body;
        }

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

            var appVersion =  versions
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