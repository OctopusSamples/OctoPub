namespace Audit.Service.Services
{
    /// <summary>
    /// Extract tenant information from the request.
    /// </summary>
    public interface ITenantParser
    {
        /// <summary>
        /// Extract the tenant information from the request
        /// </summary>
        /// <returns>The specifically defined tenant, or the default tenant if none we defined</returns>
        string GetTenant();
    }
}