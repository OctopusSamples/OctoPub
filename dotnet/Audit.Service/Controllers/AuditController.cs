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
        public async Task<IActionResult> GetAll()
        {
            var requestWrapper = await RequestWrapperFactory.CreateFromHttpRequest(Request);
            var response = _auditHandler.GetAll(requestWrapper);
            if (response != null)
            {
                return new ActionResultConverter(response);
            }
            return NotFound();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOne()
        {
            var requestWrapper = await RequestWrapperFactory.CreateFromHttpRequest(Request);
            var response = _auditHandler.GetOne(requestWrapper);
            if (response != null)
            {
                return new ActionResultConverter(response);
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> CreateOne()
        {
            var requestWrapper = await RequestWrapperFactory.CreateFromHttpRequest(Request);
            var response = _auditHandler.CreateOne(requestWrapper);
            if (response != null)
            {
                return new ActionResultConverter(response);
            }
            return NotFound();
        }
    }
}