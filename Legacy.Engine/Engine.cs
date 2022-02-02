// <copyright file="Engine.cs" company="Legendary">
//  Copyright © 2021 Legendary
//  All rights are reserved. Reproduction or transmission in whole or
//  in part, in any form or by any means, electronic, mechanical or
//  otherwise, is prohibited without the prior written consent of
//  the copyright owner.
// </copyright>

namespace Legendary.Engine
{
    using System;
    using System.Net.WebSockets;
    using System.Threading.Tasks;
    using Legendary.Core.Contracts;
    using Legendary.Data.Contracts;
    using Legendary.Engine.Contracts;
    using Legendary.Engine.Models;
    using Legendary.Engine.Types;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// The main entry point of the Legendary app engine.
    /// </summary>
    public class Engine : IEngine
    {
        private readonly RequestDelegate requestDelegate;
        private readonly IDBConnection connection;
        private readonly ILogger logger;
        private readonly IDataService dataService;
        private readonly IApiClient apiClient;
        private ICommunicator? communicator;
        private IRandom? random;
        private Processor? processor;
        private Environment? environment;
        private int gameTicks = 0;
        private int gameHour = 0;
        private int saveGame = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="Engine"/> class.
        /// </summary>
        /// <param name="requestDelegate">The request delegate.</param>
        /// <param name="connection">The database connection.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="dataService">The data service.</param>
        /// <param name="apiClient">The api client.</param>
        public Engine(RequestDelegate requestDelegate, ILogger logger, IDBConnection connection, IDataService dataService, IApiClient apiClient)
        {
            this.requestDelegate = requestDelegate ?? throw new ArgumentNullException(nameof(requestDelegate));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.connection = connection ?? throw new ArgumentException(null, nameof(connection));
            this.dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
            this.apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        }

        /// <summary>
        /// Gets the world.
        /// </summary>
        public IWorld? World { get; internal set; }

        /// <inheritdoc/>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            this.Terminate();
        }

        /// <summary>
        /// Invokes the communicator when a new socket is opened.
        /// </summary>
        /// <param name="context">The HttpContext.</param>
        /// <returns>Task.</returns>
        public async Task Invoke(HttpContext context)
        {
            try
            {
                if (this.communicator != null)
                {
                    await this.communicator.Invoke(context);
                }
            }
            catch (WebSocketException)
            {
                await this.logger.Warn("A player has disconnected.");
            }
            catch (TaskCanceledException)
            {
                await this.logger.Warn("A player has refreshed the page.");
            }
            catch (Exception exc)
            {
                await this.logger.Error(exc);
            }
        }

        /// <summary>
        /// Terminates the engine.
        /// </summary>
        public void Terminate()
        {
            this.logger.Warn("Terminate message received. Stopping engine.");
            this.connection.Dispose();
            this.communicator?.Dispose();
            this.processor?.Dispose();
            this.World?.Dispose();
            GC.Collect();
            System.Environment.Exit(1);
        }

        /// <inheritdoc/>
        public async Task Start()
        {
            Console.Clear();

            await this.logger.Info("Starting Legendary engine...");

            try
            {
                // Configure and test the DB connection.
                this.TestConnection();

                // Load objects.
                await this.logger.Info("Loading world from database...");

                var world = this.dataService.LoadWorld();

                if (world == null)
                {
                    await this.logger.Error("Unable to load world from database!");
                    throw new Exception("Unable to load world from database!");
                }
                else
                {
                    this.World = world;
                    await this.logger.Info($"World loaded, with {world.Areas.Count} area(s) and {world.GetAllCharacters().Count} character(s).");

                    await this.logger.Info("Populating the world with mobiles and items..");
                    await this.World.Populate();

                    // Wait for the webserver to load before loading the images
                    var contentPopulationTimer = new System.Threading.Timer(
                        async t =>
                        {
                            await this.logger.Info("Loading room images...(this may take a while)");
                            await this.World.LoadRoomImages();
                            await this.logger.Info("Room images loaded.");
                        },
                        null,
                        10000,
                        System.Threading.Timeout.Infinite);

                    await this.logger.Info("Initializing randomness...");
                    this.random = new Random();

                    await this.logger.Info("Starting communication services...");
                    this.communicator = new Communicator(this.requestDelegate, this.logger, this.apiClient, this.World);
                    this.communicator.InputReceived += this.Communicator_InputReceived;
                    this.processor = new Processor(this.logger, this.communicator);

                    await this.logger.Info("Seeding environment...");
                    this.environment = new Environment(this.logger, this.communicator, this.processor, this.random);

                    // Start the main timer.
                    await this.logger.Info("Starting main loop...");
                    var timer = new System.Threading.Timer(
                        async
                        t =>
                        {
                            // TO-DO: Main timer implementation goes here.
                            this.gameTicks++;

                            // One "hour" game time, or 30 seconds.
                            if (this.gameTicks == 30)
                            {
                                this.gameHour++;
                                if (this.gameHour == 23)
                                {
                                    this.gameHour = 0;
                                }

                                this.gameTicks = 0;
                                await this.environment.ProcessChanges(this.gameHour);
                            }

                            // Save all players every 60 seconds.
                            this.saveGame++;
                            if (this.saveGame == 60)
                            {
                                this.saveGame = 0;
                                await this.logger.Info("Saving players...");
                                await this.World.SaveAllCharacters();
                            }
                        },
                        null,
                        1000,
                        1000);

                    // Open for websocket connections.
                    await this.logger.Info("Legendary engine has started. Starting webserver...");
                }
            }
            catch (Exception exc)
            {
                await this.logger.Error(exc);
                this.Terminate();
                await this.Start();
            }
        }

        /// <summary>
        /// Tests the connection to the database (and connects if able to).
        /// </summary>
        public void TestConnection()
        {
            this.logger.Info("Testing database connection...");

            try
            {
                if (this.connection.TestConnection())
                {
                    this.logger.Info("Connection to database succeeded.");
                }
                else
                {
                    this.logger.Error("Unable to connect. Please check connection string and database name.");
                    this.Terminate();
                }
            }
            catch (Exception exc)
            {
                this.logger.Error(exc);
                this.Terminate();
            }
        }

        /// <summary>
        /// Method is called when messages are received from a connected socket.
        /// </summary>
        /// <param name="sender">Websocket.</param>
        /// <param name="e">CommunicationEventArgs.</param>
        private void Communicator_InputReceived(object? sender, EventArgs e)
        {
            if (sender == null || e == null)
            {
                return;
            }

            var user = (UserData)sender;
            var commEventArgs = (CommunicationEventArgs)e;
            this.processor?.ProcessMessage(user, commEventArgs.Message).Wait();
        }
    }
}



