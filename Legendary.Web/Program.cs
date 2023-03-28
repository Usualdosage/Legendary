// <copyright file="Program.cs" company="Legendary™">
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
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;

    /// <summary>
    /// Defines the entry point for the web application.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The main entry point of the application.
        /// </summary>
        /// <param name="args">Command line args.</param>
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        /// <summary>
        /// Creates the host builder and initializes the startup.
        /// </summary>
        /// <param name="args">Command line args.</param>
        /// <returns>IHostBuilder.</returns>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureAppConfiguration(config =>
                    {
                        // Get the connection string from an environment variable.
                        string connectionString = Environment.GetEnvironmentVariable("AZUREAPPSETTINGSCONNECTIONSTRING") ??
                            throw new Exception("Unable to load app config from Azure App Settings.");

                        // Load configuration from Azure App Configuration
                        config.AddAzureAppConfiguration(connectionString);
                    });

                    webBuilder.UseStartup<Startup>();
                });
    }
}
