﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime.Tree;
using Audit.Service.Controllers;
using Audit.Service.Handler;
using Audit.Service.Lambda;
using Audit.Service.Repositories;
using JsonApiSerializer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace Audit.Service.Tests
{
    [TestFixture]
    public class WebTests
    {
        private static readonly ServiceProvider ServiceProvider = new DependencyInjection().ConfigureServices();

        private AuditController CreateAuditController()
        {
            var auditHandler = ServiceProvider.GetService<AuditHandler>();

            // mock the HTTP context
            var httpContext = new DefaultHttpContext();
            return new AuditController(auditHandler)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = httpContext
                }
            };
        }

        private void PopulateHttpContext(ControllerBase controller, String path, String method, Stream? body = null)
        {
            controller.HttpContext.Request.Headers["Accept"] = Constants.JsonApiMimeType;
            controller.HttpContext.Request.Path = path;
            controller.HttpContext.Request.Method = method;
            controller.HttpContext.Request.Body = body;
        }

        private async Task<Models.Audit> CallControllerAndGetAudit(AuditController controller)
        {
            var createResponse = await controller.Entry();
            return JsonConvert.DeserializeObject<Models.Audit>(
                (createResponse as ContentResult).Content,
                new JsonApiSerializerSettings());
        }
        
        private async Task<(List<Models.Audit>, ContentResult)> CallControllerAndGetAudits(AuditController controller)
        {
            var createResponse = await controller.Entry() as ContentResult;
            return (JsonConvert.DeserializeObject<List<Models.Audit>>(
                createResponse.Content,
                new JsonApiSerializerSettings()), createResponse);
        }

        [Test]
        public async Task TestHealth()
        {
            var auditHandler = ServiceProvider.GetService<AuditHandler>();

            // mock the HTTP context
            var httpContext = new DefaultHttpContext();
            var controller = new HealthController(auditHandler)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = httpContext
                }
            };

            // mock a GET request
            httpContext.Request.Path = "/health/audits/GET";
            httpContext.Request.Method = "GET";
            httpContext.Request.Body = null;
            var getResponse = await controller.GetHealth();
            Assert.AreEqual(200, (getResponse as ContentResult).StatusCode);
        }

        [Test]
        public async Task TestCreateAndGet()
        {
            var controller = CreateAuditController();

            var guid = Guid.NewGuid();

            // create a stream for the post request
            await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(
                new Models.Audit
                {
                    Action = "test1",
                    Object = "test2",
                    Subject = guid.ToString()
                }, new JsonApiSerializerSettings())));

            // mock a POST request
            PopulateHttpContext(controller, "/api/audits", "POST", stream);
            var createEntity = await CallControllerAndGetAudit(controller);

            Assert.AreEqual("main", createEntity.DataPartition);

            // mock a GET request
            PopulateHttpContext(controller, "/api/audits/" + createEntity.Id, "GET");
            var getEntity = await CallControllerAndGetAudit(controller);

            Assert.AreEqual(createEntity.Subject, getEntity.Subject);
        }

        [Test]
        public async Task TestCreateAndGetAll()
        {
            var controller = CreateAuditController();

            var guid = Guid.NewGuid();

            // create a stream for the post request
            await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(
                new Models.Audit
                {
                    Action = "test1",
                    Object = "test2",
                    Subject = guid.ToString()
                }, new JsonApiSerializerSettings())));

            // mock a POST request
            PopulateHttpContext(controller, "/api/audits", "POST", stream);
            var createEntity = await CallControllerAndGetAudit(controller);

            Assert.AreEqual("main", createEntity.DataPartition);

            // mock a GET request
            PopulateHttpContext(controller, "/api/audits", "GET");
            var (getEntity, _) = await CallControllerAndGetAudits(controller);

            Assert.IsTrue(getEntity.Any(a => a.Subject == createEntity.Subject));
        }

         [Test]
        public async Task TestCreateAndGetAllFilter()
        {
            var controller = CreateAuditController();

            var guid = Guid.NewGuid();

            // create a stream for the post request
            await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(
                new Models.Audit
                {
                    Action = "test1",
                    Object = "test2",
                    Subject = guid.ToString()
                }, new JsonApiSerializerSettings())));

            // mock a POST request
            PopulateHttpContext(controller, "/api/audits", "POST", stream);
            var createEntity = await CallControllerAndGetAudit(controller);

            Assert.AreEqual("main", createEntity.DataPartition);

            // mock a GET request
            PopulateHttpContext(controller, "/api/audits", "GET");
            controller.HttpContext.Request.Query = new QueryCollection(new Dictionary<string, StringValues>()
                {{"filter", "subject=='" + createEntity.Subject + "'"}});
            var (getEntity, _) = await CallControllerAndGetAudits(controller);

            Assert.IsTrue(getEntity.All(a => a.Subject == createEntity.Subject));

        }

        [Test]
        public async Task TestCreateAndGetAllNotFilter()
        {
            var controller = CreateAuditController();

            var guid = Guid.NewGuid();

            // create a stream for the post request
            await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(
                new Models.Audit
                {
                    Action = "test1",
                    Object = "test2",
                    Subject = guid.ToString()
                }, new JsonApiSerializerSettings())));

            // mock a POST request
            PopulateHttpContext(controller, "/api/audits", "POST", stream);
            var createEntity = await CallControllerAndGetAudit(controller);

            Assert.AreEqual("main", createEntity.DataPartition);

            // mock a GET request
            PopulateHttpContext(controller, "/api/audits", "GET");
            controller.HttpContext.Request.Query = new QueryCollection(new Dictionary<string, StringValues>()
                {{"filter", "subject!='" + createEntity.Subject + "'"}});
            
            var (getEntity, _) = await CallControllerAndGetAudits(controller);

            Assert.IsFalse(getEntity.Any(a => a.Subject == createEntity.Subject));
        }

        [Test]
        public async Task TestCreateAndGetAcrossPartitions()
        {
            var controller = CreateAuditController();

            var guid = Guid.NewGuid();

            // create a stream for the post request
            await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(
                new Models.Audit
                {
                    Action = "test1",
                    Object = "test2",
                    Subject = guid.ToString()
                }, new JsonApiSerializerSettings())));

            // mock a POST request
            PopulateHttpContext(controller, "/api/audits", "POST", stream);
            controller.HttpContext.Request.Headers["Accept"] += 
                "," + Constants.JsonApiMimeType + "; dataPartition=testing1";
            var createEntity = await CallControllerAndGetAudit(controller);

            Assert.AreEqual("testing1", createEntity.DataPartition);

            // mock a GET request
            PopulateHttpContext(controller, "/api/audits/" + createEntity.Id, "GET");
            controller.HttpContext.Request.Headers["Accept"] += 
                "," + Constants.JsonApiMimeType + "; dataPartition=testing2";
            
            var getResponse = await controller.Entry();

            Assert.AreEqual(404,  (getResponse as ContentResult).StatusCode);
        }

        [Test]
        public async Task TestCreateAndGetFromMainPartition()
        {
            var controller = CreateAuditController();

            var guid = Guid.NewGuid();

            // create a stream for the post request
            await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(
                new Models.Audit
                {
                    Action = "test1",
                    Object = "test2",
                    Subject = guid.ToString()
                }, new JsonApiSerializerSettings())));

            // mock a POST request
            PopulateHttpContext(controller, "/api/audits", "POST", stream);
            var createEntity = await CallControllerAndGetAudit(controller);

            Assert.AreEqual("main", createEntity.DataPartition);

            // mock a GET request
            PopulateHttpContext(controller, "/api/audits/" + createEntity.Id, "GET");
            var (getEntity, getResponse) = await CallControllerAndGetAudits(controller);

            Assert.IsTrue(getEntity.Any(a => a.Subject == createEntity.Subject));
            Assert.AreEqual(200,  (getResponse as ContentResult).StatusCode);
        }

    }
}