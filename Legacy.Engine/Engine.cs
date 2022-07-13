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
    using Legendary.Core.Types;
    using Legendary.Engine.Contracts;
    using Legendary.Engine.Models;

    /// <summary>
    /// The main entry point of the Legendary app engine.
    /// </summary>
    public class Engine : IEngine
    {
        private readonly ILogger logger;
        private readonly IWorld world;
        private int gameTicks = 0;
        private int gameHour = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="Engine"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="world">The world.</param>
        public Engine(ILogger logger, IWorld world)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.world = world ?? throw new ArgumentNullException(nameof(world));

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
            this.logger.Info("Populating the world with mobiles and items..");

            await this.world.Populate();

            this.logger.Info("Starting main loop...");

            var timer = new System.Threading.Timer(
                t =>
                {
                    try
                    {
                        this.gameTicks++;

                        // Fires each second to calculate combat.
                        this.OnVioTick(this, new EngineEventArgs(this.gameTicks, this.gameHour, null));

                        // One "hour" game time, or 30 seconds.
                        if (this.gameTicks == 30)
                        {
                            this.gameHour++;
                            if (this.gameHour == 23)
                            {
                                this.gameHour = 0;
                            }

                            this.gameTicks = 0;

                            // Raise the event to any listeners (e.g. Communicator).
                            this.OnTick(this, new EngineEventArgs(this.gameTicks, this.gameHour, null));
                        }
                    }
                    catch (Exception ex)
                    {
                        this.logger.Debug(ex.ToString());
                    }
                },
                null,
                2000,
                2000);
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
                this.logger.Error(exc);
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
                this.logger.Error(exc);
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
                this.logger.Error(exc);
            }
        }
    }
}
