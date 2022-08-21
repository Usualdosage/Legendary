// <copyright file="Engine.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine
{
    using System;
    using System.Threading.Tasks;
    using Legendary.Core;
    using Legendary.Core.Contracts;
    using Legendary.Engine.Contracts;
    using Legendary.Engine.Models;

    /// <summary>
    /// The main entry point of the Legendary app engine.
    /// </summary>
    public class Engine : IEngine
    {
        private readonly ILogger logger;
        private readonly IWorld world;
        private readonly IEnvironment environment;
        private int gameTicks = 0;
        private int gameHour = 0;

        /// <summary>
        /// Master game timer.
        /// </summary>
        private System.Threading.Timer? timer;

        /// <summary>
        /// Initializes a new instance of the <see cref="Engine"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="world">The world.</param>
        /// <param name="environment">The environment.</param>
        public Engine(ILogger logger, IWorld world, IEnvironment environment)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.world = world ?? throw new ArgumentNullException(nameof(world));
            this.environment = environment ?? throw new ArgumentNullException(nameof(environment));

            Task.Run(async () => await this.Initialize());
        }

        /// <inheritdoc/>
        public event EventHandler? VioTick;

        /// <inheritdoc/>
        public event EventHandler? Tick;

        /// <inheritdoc/>
        public event EventHandler? EngineUpdate;

        /// <inheritdoc/>
        public async Task Initialize()
        {
            try
            {
                this.logger.Info("Legendary is starting up...", null);

                this.logger.Info("Loading the world from the database...", null);

                await this.world.LoadWorld();

                this.logger.Info("Updating the game metrics...", null);

                await this.world.UpdateGameMetrics(null);

                this.logger.Info("Populating the world with mobiles and items...", null);

                await this.world.CleanupWorld();
                this.world.Populate();

                this.logger.Info("Creating weather...", null);
                this.environment.GenerateWeather();

                this.logger.Info("Starting main loop...", null);
                this.StartGameLoop();
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, null);
            }
        }

        /// <summary>
        /// Starts the main game loop.
        /// </summary>
        public void StartGameLoop()
        {
            this.logger.Info("Waiting for connections...", null);

            this.timer = new System.Threading.Timer(
                async t =>
                {
                    try
                    {
                        this.gameTicks++;

                        // Fires every 2 seconds to calculate combat.
                        this.OnVioTick(this, new EngineEventArgs(this.gameTicks, this.gameHour, null));

                        // One "hour" game time, or 30 seconds.
                        if (this.gameTicks == Constants.TICK)
                        {
                            this.logger.Debug("TICK.", null);

                            this.gameHour++;

                            // Repopulate an area with mobiles 4x per day.
                            if (this.gameHour % 6 == 0)
                            {
                                this.logger.Debug("Repopulating areas...", null);
                                foreach (var area in this.world.Areas)
                                {
                                    this.world.RepopulateMobiles(area);
                                }
                            }

                            if (this.gameHour == 24)
                            {
                                this.gameHour = 0;
                            }

                            this.gameTicks = 0;

                            var metrics = await this.world.UpdateGameMetrics(null);

                            // Raise the event to any listeners (e.g. Communicator).
                            this.OnTick(this, new EngineEventArgs(this.gameTicks, metrics.CurrentHour, null));
                        }
                    }
                    catch (Exception ex)
                    {
                        this.logger.Error(ex.ToString(), null);

                        await this.world.UpdateGameMetrics(ex);

                        // If we hit an exception, we need to restart the timer.
                        this.StartGameLoop();
                    }
                },
                null,
                Constants.VIOTICK,
                Constants.VIOTICK);
        }

        /// <summary>
        /// Raises the Tick event.
        /// </summary>
        /// <param name="sender">The sender of the message (userdata).</param>
        /// <param name="e">CommunicationEventArgs.</param>
        protected virtual void OnTick(object sender, EngineEventArgs e)
        {
            try
            {
                EventHandler? handler = this.Tick;
                handler?.Invoke(sender, e);
            }
            catch (Exception exc)
            {
                this.logger.Error(exc, null);
            }
        }

        /// <summary>
        /// Raises the VioTick event.
        /// </summary>
        /// <param name="sender">The sender of the message (userdata).</param>
        /// <param name="e">CommunicationEventArgs.</param>
        protected virtual void OnVioTick(object sender, EngineEventArgs e)
        {
            try
            {
                EventHandler? handler = this.VioTick;
                handler?.Invoke(sender, e);
            }
            catch (Exception exc)
            {
                this.logger.Error(exc, null);
            }
        }

        /// <summary>
        /// Raises the EngineUpdate event.
        /// </summary>
        /// <param name="sender">The sender of the message (userdata).</param>
        /// <param name="e">CommunicationEventArgs.</param>
        protected virtual void OnEngineUpdate(object sender, EngineEventArgs e)
        {
            try
            {
                EventHandler? handler = this.EngineUpdate;
                handler?.Invoke(sender, e);
            }
            catch (Exception exc)
            {
                this.logger.Error(exc, null);
            }
        }
    }
}
