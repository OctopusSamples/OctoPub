using System;
using Audit.Service.Models;
using Audit.Service.Repositories.InMemory;
using Audit.Service.Services.InMemory;
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
            services.AddDbContext<Db>(opt =>
            {
                if (Boolean.Parse((ReadOnlySpan<char>)Configuration.GetSection("Database:UseInMemory").Value))
                {
                    opt.UseInMemoryDatabase("audit");
                }
                else
                {
                    opt.UseMySQL(Configuration.GetConnectionString("MySqlDatabase"));
                }
            });
            services.AddScoped<AuditCreateService>();
            services.AddScoped<AuditCreateService>();
            services.AddScoped<AuditCreateService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}