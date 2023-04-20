// <copyright file="Startup.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Web
{
    using System;
    using System.Linq;
    using Legendary.Core.Contracts;
    using Legendary.Data;
    using Legendary.Data.Contracts;
    using Legendary.Engine;
    using Legendary.Engine.Contracts;
    using Legendary.Networking;
    using Legendary.Networking.Models;
    using Legendary.Web.Contracts;
    using Legendary.Web.Models;
    using Legendary.Web.Processors;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Main entry point.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">IConfiguration object.</param>
        /// <param name="webHostEnvironment">The hosting environment.</param>
        public Startup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            this.Configuration = configuration;
            this.WebHostEnvironment = webHostEnvironment;
        }

        /// <summary>
        /// Gets the IConfiguration interface.
        /// </summary>
        public IConfiguration Configuration { get; private set; }

        /// <summary>
        /// Gets the IHostingEnvironment.
        /// </summary>
        public IWebHostEnvironment WebHostEnvironment { get; private set; }

        /// <summary>
        /// Adds services to the dependency injection container.
        /// </summary>
        /// <param name="services">IServiceCollection.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            // Add services to the container.
            services.AddControllersWithViews();

            // Load the configuration values.
            services.Configure<ServerSettings>(this.Configuration.GetSection("Legendary:ServerSettings"));
            services.Configure<BuildSettings>(this.Configuration.GetSection("Legendary:BuildSettings"));

            // Apply necessary DI containers for fully independent services.
            services.AddSingleton<ILogger, Logger>();
            services.AddSingleton<IRandom, Legendary.Engine.Random>();
            services.AddSingleton<IServerSettings>(sp => sp.GetRequiredService<IOptions<ServerSettings>>().Value);
            services.AddSingleton<IDBConnection, MongoConnection>();
            services.AddTransient<IDataService, DataService>();
            services.AddSingleton<IApiClient, ApiClient>();
            services.AddSingleton<IBuildSettings>(sp => sp.GetRequiredService<IOptions<BuildSettings>>().Value);
            services.AddSingleton<IMailService, MailService>();
            services.AddSingleton<ICompanionProcessor, CompanionProcessor>();

            // Configure authentication.
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Home/Login";
                    options.LogoutPath = "/Home/Login";
                });

            // Ensure we pass the HTTP Context to the required classes.
            services.AddHttpContextAccessor();

            var accessor = services.Last(s => s.ServiceType == typeof(IHttpContextAccessor)).ImplementationInstance;

            if (accessor != null)
            {
                services.AddSingleton((IHttpContextAccessor)accessor);
            }
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="env">The hosting environment.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDeveloperExceptionPage();

            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseCookiePolicy(new CookiePolicyOptions
            {
                MinimumSameSitePolicy = SameSiteMode.Strict,
            });

            // https://stackoverflow.com/questions/40502921/net-websockets-forcibly-closed-despite-keep-alive-and-activity-on-the-connectio
            System.Net.ServicePointManager.MaxServicePointIdleTime = int.MaxValue;
            app.UseWebSockets(new WebSocketOptions() { KeepAliveInterval = TimeSpan.FromSeconds(90) });

            // Start up the comms to handle websocket requests.
            app.UseMiddleware<Communicator>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}");
            });
        }
    }
}
