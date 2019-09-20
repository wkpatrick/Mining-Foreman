using System;
using EVEStandard;
using EVEStandard.Enumerations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace mining_foreman_backend {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public static IConfiguration Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            // Add cookie authentication and set the login url
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options => {
                    options.LoginPath = "/Auth/Login";
                    options.Cookie.HttpOnly = false;
                });

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
            services.AddSession();
            services.AddCors();
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(builder =>
                builder
                    .WithOrigins("http://localhost")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
            );


            app.UseSession();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseMvc();
        }
    }
}