using System;
using Audit.Service.Sqs;
using CommandLine;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Audit.Service
{
    /// <summary>
    /// The entry point for the self hosted web application.
    /// </summary>
    public static class Program
    {
        private static readonly SqsListener SqsListener = new SqsListener();

        /// <summary>
        /// The application entry point.
        /// </summary>
        /// <param name="args">The arguments passed from the command line.</param>
        public static void Main(string[] args)
        {
            var parser = new Parser(with => with.IgnoreUnknownArguments = true);
            parser.ParseArguments<Options>(args)
                .WithParsed<Options>(o =>
                {
                    AppMode fixedMode = Enum.TryParse<AppMode>(o.Mode, out var mode) ? mode : AppMode.Web;
                    switch (fixedMode)
                    {
                        case AppMode.Sqs:
                            SqsListener.ListenSqs(o).GetAwaiter().GetResult();
                            break;
                        default:
                            CreateHostBuilder(args)
                                .Build()
                                .MigrateDatabase()
                                .Run();
                            break;
                    }
                });
        }

        /// <summary>
        /// Creates the host builder, exposing features like DI, logging, configuration etc.
        /// </summary>
        /// <param name="args">The main program arguments.</param>
        /// <returns>The host builder.</returns>
        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
        }
    }
}