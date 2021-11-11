using Microsoft.AspNetCore.Mvc;

namespace audit_service.Controllers
{
    /// <summary>
    /// If this controller is running, return OK for any request to the /health endpoint
    /// </summary>
    [Route("/health/{*path}")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        [HttpDelete]
        [HttpPatch]
        [HttpPost]
        [HttpPut]
        public IActionResult GetHealth()
        {
            return Ok("{\"message\": \"OK\"}");
        }
    }
}