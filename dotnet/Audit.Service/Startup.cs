using Audit.Service.Repositories.InMemory;
using Audit.Service.Services;
using Audit.Service.Services.InMemory;
using Audit.Service.Services.Web;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace Audit.Service
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "audit_service", Version = "v1" });
            });

            services.AddDbContext<Db>(opt => opt.UseInMemoryDatabase("audit"));
            services.AddScoped<ICreateService<Models.Audit, int>, AuditCreateService>();
            services.AddScoped<IGetAllService<Models.Audit, int>, AuditGetAllService>();
            services.AddScoped<IGetByIdService<Models.Audit, int>, AuditGetByIdService>();
            services.AddScoped<ITenantParser, WebWebTenantParser>();

            services.AddJsonApi<Db>(opts =>
            {
                opts.Namespace = "api";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "audit_service v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseJsonApi();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}