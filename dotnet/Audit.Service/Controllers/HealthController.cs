using System.Threading.Tasks;
using Audit.Service.Handler;
using Audit.Service.Lambda;
using Microsoft.AspNetCore.Mvc;

namespace Audit.Service.Controllers
{
    /// <summary>
    /// If this controller is running, return OK for any request to the /health endpoint.
    /// </summary>
    [Route("/health/{*path}")]
    public class HealthController : ControllerBase
    {
        private readonly AuditHandler auditHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="HealthController"/> class.
        /// </summary>
        /// <param name="auditHandler">The AuditHandler used to process requests.</param>
        public HealthController(AuditHandler auditHandler)
        {
            this.auditHandler = auditHandler;
        }

        /// <summary>
        /// Get the health of the requested endpoint.
        /// </summary>
        /// <returns>OK if the endpoint is known, and Missing if it is unknown.</returns>
        [HttpGet]
        public async Task<IActionResult> GetHealth()
        {
            var requestWrapper = await RequestWrapperFactory.CreateFromHttpRequest(Request);
            var response = auditHandler.GetHealth(requestWrapper);
            if (response != null) return new ActionResultConverter(response);

            return NotFound();
        }
    }
}