using System.Threading.Tasks;
using Audit.Service.Handler;
using Audit.Service.Lambda;
using Microsoft.AspNetCore.Mvc;

namespace Audit.Service.Controllers
{
    /// <summary>
    /// If this controller is running, return OK for any request to the /health endpoint
    /// </summary>
    [Route("/health/{*path}")]
    public class HealthController : ControllerBase
    {
        private readonly AuditHandler _auditHandler;

        public HealthController(AuditHandler auditHandler)
        {
            _auditHandler = auditHandler;
        }

        [HttpGet]
        public async Task<IActionResult> GetHealth()
        {
            var requestWrapper = await RequestWrapperFactory.CreateFromHttpRequest(Request);
            var response = _auditHandler.GetHealth(requestWrapper);
            if (response != null)
            {
                return new ActionResultConverter(response);
            }

            return NotFound();
        }
    }
}