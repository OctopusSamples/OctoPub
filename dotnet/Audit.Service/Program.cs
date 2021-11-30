using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Audit.Service
{
    /// <summary>
    /// The entry point for the self hosted web application.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The application entry point.
        /// </summary>
        /// <param name="args">The arguments passed from the command line.</param>
        public static void Main(string[] args)
        {
            CreateHostBuilder(args)
                .Build()
                .MigrateDatabase()
                .Run();
        }

        /// <summary>
        /// Creates the host builder, exposing features like DI, logging, configuration etc.
        /// </summary>
        /// <param name="args">The main program arguments.</param>
        /// <returns>The host builder.</returns>
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
        }
    }
}