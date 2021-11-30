using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Audit.Service.Handler;
using Audit.Service.Lambda;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Audit.Service.Controllers
{
    /// <summary>
    /// The HTTP controller exposing audit resources.
    /// </summary>
    [Route("/api/audits")]
    public class AuditController : ControllerBase
    {
        private readonly AuditHandler auditHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuditController"/> class.
        /// </summary>
        /// <param name="auditHandler">The AuditHandler used to process requests.</param>
        public AuditController(AuditHandler auditHandler)
        {
            this.auditHandler = auditHandler;
        }

        /// <summary>
        /// Gets a single resource matching the supplied ID.
        /// </summary>
        /// <returns>The matching resource.</returns>
        [HttpGet]
        [HttpGet("{id}")]
        [HttpPost]
        public async Task<IActionResult> Entry()
        {
            if (!CheckAcceptHeader())
            {
                return new ContentResult()
                {
                    StatusCode = 406
                };
            }

            var requestWrapper = await RequestWrapperFactory.CreateFromHttpRequest(Request);
            var response = auditHandler.GetAll(requestWrapper)
                           ?? auditHandler.GetOne(requestWrapper)
                           ?? auditHandler.CreateOne(requestWrapper);
            if (response != null) return new ActionResultConverter(response);

            return NotFound();
        }

        private bool CheckAcceptHeader()
        {
            var acceptHeaders = (Request.Headers ?? new HeaderDictionary())
                .Where(h => h.Key.ToLower() == Constants.AcceptHeader)
                .SelectMany(h => h.Value);

            return acceptHeaders
                .Where(h => h != null)
                .SelectMany(h => h.Split(","))
                .Select(h => h.Trim())
                .Where(h => h.StartsWith(Constants.JsonApiMimeType))
                .Any(h => h == Constants.JsonApiMimeType);
        }
    }
}