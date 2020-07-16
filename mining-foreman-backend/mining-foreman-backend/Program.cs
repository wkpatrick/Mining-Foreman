using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace mining_foreman_backend {
    public class Program {
        public static void Main(string[] args) {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File("mining-foreman.log")
                .CreateLogger();
            try {
                Log.Information("Starting mining-foreman");
                CreateHostBuilder(args)
                    .Build().Run();
            }
            catch (Exception ex) {
                Log.Fatal(ex, "Application start-up failed");
            }
            finally {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder => {
                    webBuilder.UseUrls("http://localhost:5500");
                    webBuilder.UseStartup<Startup>();
                });
    }
}