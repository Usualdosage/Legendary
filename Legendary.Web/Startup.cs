// <copyright file="Startup.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Legendary.Web
{
    using Legendary.Core.Contracts;
    using Legendary.Data;
    using Legendary.Data.Contracts;
    using Legendary.Engine;
    using Legendary.Engine.Contracts;
    using Legendary.Engine.Models;
    using Legendary.Engine.Models.Skills;
    using Legendary.Networking;
    using Legendary.Networking.Contracts;
    using Legendary.Networking.Models;
    using Legendary.Web.Contracts;
    using Legendary.Web.Models;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using MongoDB.Bson.Serialization;

    /// <summary>
    /// Main entry point.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">IConfiguration object.</param>
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        /// <summary>
        /// Gets the IConfiguration interface.
        /// </summary>
        public IConfiguration Configuration { get; private set; }

        /// <summary>
        /// Adds services to the dependency injection container.
        /// </summary>
        /// <param name="services">IServiceCollection.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            // Add services to the container.
            services.AddControllersWithViews();

            // Load the configuration values for the database.
            services.Configure<DatabaseSettings>(this.Configuration.GetSection(nameof(DatabaseSettings)));

            // Load the configuration values for the server.
            services.Configure<ServerSettings>(this.Configuration.GetSection(nameof(ServerSettings)));

            // Load the configuration values for the build.
            services.Configure<BuildSettings>(this.Configuration.GetSection(nameof(BuildSettings)));

            // Apply necessary DI containers for fully independent services.
            services.AddSingleton<ILogger, Logger>();
            services.AddSingleton<IDatabaseSettings>(sp => sp.GetRequiredService<IOptions<DatabaseSettings>>().Value);
            services.AddSingleton<IDBConnection, MongoConnection>();
            services.AddTransient<IDataService, DataService>();
            services.AddSingleton<IServerSettings>(sp => sp.GetRequiredService<IOptions<ServerSettings>>().Value);
            services.AddSingleton<IApiClient, ApiClient>();
            services.AddSingleton<IBuildSettings>(sp => sp.GetRequiredService<IOptions<BuildSettings>>().Value);

            // Load the world.
            services.AddSingleton<IWorld>(sp => sp.GetRequiredService<IDataService>().LoadWorld());

            // Initialize the engine.
            services.AddSingleton<IEngine, Engine>();

            // Configure authentication.
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie();

            // Register skills and spells.
            this.ConfigureSkills();
            this.ConfigureSpells();
        }

        /// <summary>
        /// Registers all of the skills to a class map so Mongo knows how to save them since they all derive from a root class.
        /// </summary>
        public void ConfigureSkills()
        {
            BsonClassMap.RegisterClassMap<Skill>(cm =>
            {
                cm.AutoMap();
                cm.SetIsRootClass(true);
            });

            BsonClassMap.RegisterClassMap<Recall>();
        }

        /// <summary>
        /// Registers all of the spells to a class map so Mongo knows how to save them since they all derive from a root class.
        /// </summary>
        public void ConfigureSpells()
        {
            BsonClassMap.RegisterClassMap<Spell>(cm =>
            {
                cm.AutoMap();
                cm.SetIsRootClass(true);
            });
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="env">The hosting environment.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDeveloperExceptionPage();

            // app.UseExceptionHandler("/Home/Error");

            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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

            app.UseWebSockets();

            // Start up the comms to handle websocket requests.
            app.UseMiddleware<Communicator>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Login}");
            });
        }
    }
}
