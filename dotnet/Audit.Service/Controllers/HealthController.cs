﻿using Microsoft.AspNetCore.Mvc;

namespace Audit.Service.Controllers
{
    /// <summary>
    /// If this controller is running, return OK for any request to the /health endpoint
    /// </summary>
    [Route("/health/{*path}")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetHealth()
        {
            return Ok("{\"message\": \"OK\"}");
        }
    }
}