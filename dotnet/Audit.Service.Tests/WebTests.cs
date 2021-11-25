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
using JsonApiSerializer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace Audit.Service.Tests
{
    [TestFixture]
    public class WebTests
    {
        private static readonly DependencyInjection DependencyInjection = new DependencyInjection();

        [Test]
        public async Task TestHealth()
        {
            var serviceProvider = DependencyInjection.ConfigureServices();
            var auditHandler = serviceProvider.GetService<AuditHandler>();

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
            var serviceProvider = DependencyInjection.ConfigureServices();
            var auditHandler = serviceProvider.GetService<AuditHandler>();

            // mock the HTTP context
            var httpContext = new DefaultHttpContext();
            var controller = new AuditController(auditHandler)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = httpContext
                }
            };

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
            httpContext.Request.Headers["Accept"] = Constants.JsonApiMimeType;
            httpContext.Request.Path = "/api/audits";
            httpContext.Request.Method = "POST";
            httpContext.Request.Body = stream;
            var createResponse = await controller.Entry();
            var createEntity = JsonConvert.DeserializeObject<Models.Audit>(
                (createResponse as ContentResult).Content,
                new JsonApiSerializerSettings());

            // mock a GET request
            httpContext.Request.Headers["Accept"] = Constants.JsonApiMimeType;
            httpContext.Request.Path = "/api/audits/" + createEntity.Id;
            httpContext.Request.Method = "GET";
            httpContext.Request.Body = null;
            var getResponse = await controller.Entry();
            var getEntity = JsonConvert.DeserializeObject<Models.Audit>(
                (getResponse as ContentResult).Content,
                new JsonApiSerializerSettings());

            Assert.AreEqual(createEntity.Subject, getEntity.Subject);
        }

        [Test]
        public async Task TestCreateAndGetAll()
        {
            var serviceProvider = DependencyInjection.ConfigureServices();
            var auditHandler = serviceProvider.GetService<AuditHandler>();

            // mock the HTTP context
            var httpContext = new DefaultHttpContext();
            var controller = new AuditController(auditHandler)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = httpContext
                }
            };

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
            httpContext.Request.Headers["Accept"] = Constants.JsonApiMimeType;
            httpContext.Request.Path = "/api/audits";
            httpContext.Request.Method = "POST";
            httpContext.Request.Body = stream;
            var createResponse = await controller.Entry();
            var createEntity = JsonConvert.DeserializeObject<Models.Audit>(
                (createResponse as ContentResult).Content,
                new JsonApiSerializerSettings());

            // mock a GET request
            httpContext.Request.Headers["Accept"] = Constants.JsonApiMimeType;
            httpContext.Request.Path = "/api/audits";
            httpContext.Request.Method = "GET";
            httpContext.Request.Body = null;
            var getResponse = await controller.Entry();
            var getEntity = JsonConvert.DeserializeObject<List<Models.Audit>>(
                (getResponse as ContentResult).Content,
                new JsonApiSerializerSettings());

            Assert.IsTrue(getEntity.Any(a => a.Subject == createEntity.Subject));
        }

         [Test]
        public async Task TestCreateAndGetAllFilter()
        {
            var serviceProvider = DependencyInjection.ConfigureServices();
            var auditHandler = serviceProvider.GetService<AuditHandler>();

            // mock the HTTP context
            var httpContext = new DefaultHttpContext();
            var controller = new AuditController(auditHandler)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = httpContext
                }
            };

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
            httpContext.Request.Headers["Accept"] = Constants.JsonApiMimeType;
            httpContext.Request.Path = "/api/audits";
            httpContext.Request.Method = "POST";
            httpContext.Request.Body = stream;
            var createResponse = await controller.Entry();
            var createEntity = JsonConvert.DeserializeObject<Models.Audit>(
                (createResponse as ContentResult).Content,
                new JsonApiSerializerSettings());

            // mock a GET request
            httpContext.Request.Headers["Accept"] = Constants.JsonApiMimeType;
            httpContext.Request.Path = "/api/audits";
            httpContext.Request.Query = new QueryCollection(new Dictionary<string, StringValues>() {{"filter", "subject=='" + createEntity.Subject + "'"}});
            httpContext.Request.Method = "GET";
            httpContext.Request.Body = null;
            var getResponse = await controller.Entry();
            var getEntity = JsonConvert.DeserializeObject<List<Models.Audit>>(
                (getResponse as ContentResult).Content,
                new JsonApiSerializerSettings());

            Assert.IsTrue(getEntity.All(a => a.Subject == createEntity.Subject));
        }

        [Test]
        public async Task TestCreateAndGetAllNotFilter()
        {
            var serviceProvider = DependencyInjection.ConfigureServices();
            var auditHandler = serviceProvider.GetService<AuditHandler>();

            // mock the HTTP context
            var httpContext = new DefaultHttpContext();
            var controller = new AuditController(auditHandler)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = httpContext
                }
            };

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
            httpContext.Request.Headers["Accept"] = Constants.JsonApiMimeType;
            httpContext.Request.Path = "/api/audits";
            httpContext.Request.Method = "POST";
            httpContext.Request.Body = stream;
            var createResponse = await controller.Entry();
            var createEntity = JsonConvert.DeserializeObject<Models.Audit>(
                (createResponse as ContentResult).Content,
                new JsonApiSerializerSettings());

            // mock a GET request
            httpContext.Request.Headers["Accept"] = Constants.JsonApiMimeType;
            httpContext.Request.Path = "/api/audits";
            httpContext.Request.Query = new QueryCollection(new Dictionary<string, StringValues>() {{"filter", "subject!='" + createEntity.Subject + "'"}});
            httpContext.Request.Method = "GET";
            httpContext.Request.Body = null;
            var getResponse = await controller.Entry();
            var getEntity = JsonConvert.DeserializeObject<List<Models.Audit>>(
                (getResponse as ContentResult).Content,
                new JsonApiSerializerSettings());

            Assert.IsFalse(getEntity.Any(a => a.Subject == createEntity.Subject));
        }

        [Test]
        public async Task TestCreateAndGetAcrossPartitions()
        {
            var serviceProvider = DependencyInjection.ConfigureServices();
            var auditHandler = serviceProvider.GetService<AuditHandler>();

            // mock the HTTP context
            var httpContext = new DefaultHttpContext();
            var controller = new AuditController(auditHandler)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = httpContext
                }
            };

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
            httpContext.Request.Headers["Accept"] = Constants.JsonApiMimeType + "," + Constants.JsonApiMimeType + "; dataPartition=testing1";
            httpContext.Request.Path = "/api/audits";
            httpContext.Request.Method = "POST";
            httpContext.Request.Body = stream;
            var createResponse = await controller.Entry();
            var createEntity = JsonConvert.DeserializeObject<Models.Audit>(
                (createResponse as ContentResult).Content,
                new JsonApiSerializerSettings());

            // mock a GET request
            httpContext.Request.Headers["Accept"] = Constants.JsonApiMimeType + "," + Constants.JsonApiMimeType + "; dataPartition=testing2";
            httpContext.Request.Path = "/api/audits/" + createEntity.Id;
            httpContext.Request.Method = "GET";
            httpContext.Request.Body = null;
            var getResponse = await controller.Entry();

            Assert.AreEqual(404,  (getResponse as ContentResult).StatusCode);
        }

        [Test]
        public async Task TestCreateAndGetFromMainPartition()
        {
            var serviceProvider = DependencyInjection.ConfigureServices();
            var auditHandler = serviceProvider.GetService<AuditHandler>();

            // mock the HTTP context
            var httpContext = new DefaultHttpContext();
            var controller = new AuditController(auditHandler)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = httpContext
                }
            };

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
            httpContext.Request.Headers["Accept"] = Constants.JsonApiMimeType + "," + Constants.JsonApiMimeType + "; dataPartition=main";
            httpContext.Request.Path = "/api/audits";
            httpContext.Request.Method = "POST";
            httpContext.Request.Body = stream;
            var createResponse = await controller.Entry();
            var createEntity = JsonConvert.DeserializeObject<Models.Audit>(
                (createResponse as ContentResult).Content,
                new JsonApiSerializerSettings());

            // mock a GET request
            httpContext.Request.Headers["Accept"] = Constants.JsonApiMimeType + "," + Constants.JsonApiMimeType + "; dataPartition=testing2";
            httpContext.Request.Path = "/api/audits/" + createEntity.Id;
            httpContext.Request.Method = "GET";
            httpContext.Request.Body = null;
            var getResponse = await controller.Entry();
            var getEntity = JsonConvert.DeserializeObject<List<Models.Audit>>(
                (getResponse as ContentResult).Content,
                new JsonApiSerializerSettings());

            Assert.IsTrue(getEntity.Any(a => a.Subject == createEntity.Subject));
            Assert.AreEqual(200,  (getResponse as ContentResult).StatusCode);
        }

    }
}