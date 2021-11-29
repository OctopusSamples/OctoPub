using System;
using System.Reflection;
using Audit.Service.Handler;
using Audit.Service.Repositories;
using Audit.Service.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
                    var folder = Environment.SpecialFolder.LocalApplicationData;
                    var path = Environment.GetFolderPath(folder);
                    var dbPath = $"{path}{System.IO.Path.DirectorySeparatorChar}audits.db";
                    opt.UseSqlite($"Data Source={dbPath}");
                }
                else
                {
                    opt.UseMySql(
                        Configuration.GetConnectionString("MySqlDatabase"),
                        new MySqlServerVersion(Constants.MySqlVersion),
                        x => x.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name));
                }
            });
            services.AddScoped<AuditHandler>();
            services.AddScoped<AuditCreateService>();
            services.AddScoped<AuditGetAllService>();
            services.AddScoped<AuditGetByIdService>();
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

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}