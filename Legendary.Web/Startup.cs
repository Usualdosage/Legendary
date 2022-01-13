using System;
using Legendary.Core.Contracts;
using Legendary.Data;
using Legendary.Data.Contracts;
using Legendary.Networking;
using Legendary.Networking.Contracts;
using Legendary.Networking.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;

namespace Legendary.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // Use this method to add services to the container.  
        public void ConfigureServices(IServiceCollection services)
        {
            // Add services to the container.
            services.AddControllersWithViews();

            // Load the configuration values for the database.
            services.Configure<DatabaseSettings>(this.Configuration.GetSection(nameof(DatabaseSettings)));

            // Load the configuration values for the server.
            services.Configure<ServerSettings>(this.Configuration.GetSection(nameof(ServerSettings)));

            // Apply necessary DI containers
            services.AddSingleton<Engine.Contracts.ILogger, Legendary.Engine.Logger>();
            services.AddSingleton<IDatabaseSettings>(sp => sp.GetRequiredService<IOptions<DatabaseSettings>>().Value);
            services.AddSingleton<Legendary.Data.Contracts.IDBConnection, Legendary.Data.MongoConnection>();
            services.AddSingleton<IDataService, Legendary.Data.DataService>();
            services.AddSingleton<IServerSettings>(sp => sp.GetRequiredService<IOptions<ServerSettings>>().Value);
            services.AddSingleton<IApiClient, Legendary.Networking.ApiClient>();

            // Configure authentication
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseCookiePolicy(new CookiePolicyOptions
            {
                MinimumSameSitePolicy = SameSiteMode.Strict,
            });

            app.UseWebSockets();
            app.UseMiddleware<Server>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
