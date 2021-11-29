using System.Threading.Tasks;
using Audit.Service.Handler;
using Audit.Service.Lambda;
using Microsoft.AspNetCore.Mvc;

namespace Audit.Service.Controllers
{
    [Route("/api/audits")]
    public class AuditController : ControllerBase
    {
        private readonly AuditHandler auditHandler;

        public AuditController(AuditHandler auditHandler)
        {
            this.auditHandler = auditHandler;
        }

        [HttpGet]
        [HttpGet("{id}")]
        [HttpPost]
        public async Task<IActionResult> Entry()
        {
            var requestWrapper = await RequestWrapperFactory.CreateFromHttpRequest(Request);
            var response = auditHandler.GetAll(requestWrapper)
                           ?? auditHandler.GetOne(requestWrapper)
                           ?? auditHandler.CreateOne(requestWrapper);
            if (response != null) return new ActionResultConverter(response);

            return NotFound();
        }
    }
}