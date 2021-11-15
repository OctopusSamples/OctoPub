using System.Collections.Generic;

namespace Audit.Service.Services
{
    /// <summary>
    /// This service extracts tenant information from a collection of accept headers.
    /// </summary>
    public interface ITenantExtractor
    {
        /// <summary>
        /// Find the tenant information from the supplied headers
        /// </summary>
        /// <param name="acceptHeader">The collection of headers containing tenant information</param>
        /// <returns>The specifically defined tenant, or the default tenant if none are defined</returns>
        string GetTenant(IEnumerable<string> acceptHeader);
    }
}