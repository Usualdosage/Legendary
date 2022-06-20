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
    using System.Threading.Tasks;
    using Legendary.Core.Contracts;
    using Legendary.Data.Contracts;
    using Legendary.Engine.Contracts;
    using Legendary.Engine.Models;
    using Legendary.Engine.Types;

    /// <summary>
    /// The main entry point of the Legendary app engine.
    /// </summary>
    public class Engine : IEngine
    {
        private readonly ILogger logger;
        private readonly IWorld world;
        private IRandom? random;
        private int gameTicks = 0;
        private int gameHour = 0;
        private int saveGame = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="Engine"/> class.
        /// </summary>
        public Engine(ILogger logger, IWorld world)
        {
            this.logger = logger;
            this.world = world;

            Task.Run(async () => await this.Initialize());
        }

        /// <inheritdoc/>
        public event EventHandler? VioTick;

        /// <inheritdoc/>
        public event EventHandler? Tick;

        public async Task Initialize()
        {
            this.logger.Info("Populating the world with mobiles and items..");
            await this.world.Populate();

            // Wait for the webserver to load before loading the images
            var contentPopulationTimer = new System.Threading.Timer(
                async t =>
                {
                    this.logger.Info("Loading room images...(this may take a while)");
                    await this.world.LoadRoomImages();
                    this.logger.Info("Room images loaded.");
                },
                null,
                10000,
                System.Threading.Timeout.Infinite);

            this.logger.Info("Initializing randomness...");
            this.random = new Random();

            this.logger.Info("Starting main loop...");
            var timer = new System.Threading.Timer(
                async
                t =>
                {                    
                    this.gameTicks++;

                    this.OnVioTick(this, new EngineEventArgs(this.gameTicks, this.gameHour));
                    this.logger.Info("Combat tick.");

                    // One "hour" game time, or 30 seconds.
                    if (this.gameTicks == 30)
                    {
                        this.gameHour++;
                        if (this.gameHour == 23)
                        {
                            this.gameHour = 0;
                        }

                        this.gameTicks = 0;

                        this.OnVioTick(this, new EngineEventArgs(this.gameTicks, this.gameHour));
                        this.logger.Info("Hour tick.");
                  
                    }

                    // Save all players every 60 seconds.
                    this.saveGame++;
                    if (this.saveGame == 60)
                    {
                        this.saveGame = 0;
                        this.logger.Info("Saving players...");
                        await this.world.SaveAllCharacters();
                    }
                },
                null,
                1000,
                1000);
        }

        /// <summary>
        /// Raises the InputReceived event.
        /// </summary>
        /// <param name="sender">The sender of the message (userdata).</param>
        /// <param name="e">CommunicationEventArgs.</param>
        protected virtual void OnTick(object sender, EngineEventArgs e)
        {
            EventHandler? handler = this.Tick;
            handler?.Invoke(sender, e);
        }

        /// <summary>
        /// Raises the InputReceived event.
        /// </summary>
        /// <param name="sender">The sender of the message (userdata).</param>
        /// <param name="e">CommunicationEventArgs.</param>
        protected virtual void OnVioTick(object sender, EngineEventArgs e)
        {
            EventHandler? handler = this.VioTick;
            handler?.Invoke(sender, e);
        }
    }
}



