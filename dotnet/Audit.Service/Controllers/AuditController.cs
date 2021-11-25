using System.Threading.Tasks;
using Audit.Service.Handler;
using Audit.Service.Lambda;
using Microsoft.AspNetCore.Mvc;

namespace Audit.Service.Controllers
{
    [Route("/api/audits")]
    public class AuditController : ControllerBase
    {
        private readonly AuditHandler _auditHandler;

        public AuditController(AuditHandler auditHandler)
        {
            _auditHandler = auditHandler;
        }

        [HttpGet]
        [HttpGet("{id}")]
        [HttpPost]
        public async Task<IActionResult> Entry()
        {
            var requestWrapper = await RequestWrapperFactory.CreateFromHttpRequest(Request);
            var response = _auditHandler.GetAll(requestWrapper)
                           ?? _auditHandler.GetOne(requestWrapper)
                           ?? _auditHandler.CreateOne(requestWrapper);
            if (response != null)
            {
                return new ActionResultConverter(response);
            }

            return NotFound();
        }
    }
}