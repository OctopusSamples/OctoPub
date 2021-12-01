using System;
using System.IO;
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
    /// <summary>
    /// The class used to initialize the host builder.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">The configuration interface exposed by the host builder.</param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// Gets the configuration interface.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        ///  This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">The DI services.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddDbContext<Db>(opt =>
            {
                if (bool.Parse((ReadOnlySpan<char>)Configuration.GetSection("Database:UseInMemory").Value))
                {
                    var dbPath = $"{Path.GetTempPath()}{Path.DirectorySeparatorChar}audits.db";
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

        /// <summary>
        ///  This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="env">The host environment.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}