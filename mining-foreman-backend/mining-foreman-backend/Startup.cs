using System;
using EVEStandard;
using EVEStandard.Enumerations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using IWebHostEnvironment = Microsoft.AspNetCore.Hosting.IWebHostEnvironment;

namespace mining_foreman_backend {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public static IConfiguration Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            // Initialize the client
            var esiClient = new EVEStandardAPI(
                "EVEStandard", // User agent
                DataSource.Tranquility, // Server [Tranquility/Singularity]
                TimeSpan.FromSeconds(30), // Timeout
                Configuration["SSOCallbackUrl"],
                Configuration["ClientId"],
                Configuration["SecretKey"]);

            // Register with DI container
            services.AddSingleton<EVEStandardAPI>(esiClient);
            services.AddSingleton<IHostedService, ESIService>();
            // Add cookie authentication and set the login url
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options => {
                    options.LoginPath = "/Auth/Login";
                    options.Cookie.HttpOnly = false;
                });
            services.AddDistributedMemoryCache();
            services.AddSession();
            services.AddCors();
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            app.UseSession();
            //app.UseStaticFiles();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseCors(builder =>
                builder
                    .WithOrigins("http://localhost")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
            );

            app.UseAuthentication();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}