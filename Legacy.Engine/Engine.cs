// <copyright file="Engine.cs" company="Legendary">
//  Copyright © 2021-2022 Legendary
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

        /// <inheritdoc/>
        public event EventHandler? EngineUpdate;

        /// <inheritdoc/>
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

            this.logger.Info("Starting main loop...");
            var timer = new System.Threading.Timer(
                async
                t =>
                {                    
                    this.gameTicks++;

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

                        this.OnTick(this, new EngineEventArgs(this.gameTicks, this.gameHour, null));
                  
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
        /// Raises the Tick event.
        /// </summary>
        /// <param name="sender">The sender of the message (userdata).</param>
        /// <param name="e">CommunicationEventArgs.</param>
        protected virtual void OnTick(object sender, EngineEventArgs e)
        {
            this.RestoreUsers();
            EventHandler? handler = this.Tick;
            handler?.Invoke(sender, e);
        }

        /// <summary>
        /// Raises the VioTick event.
        /// </summary>
        /// <param name="sender">The sender of the message (userdata).</param>
        /// <param name="e">CommunicationEventArgs.</param>
        protected virtual void OnVioTick(object sender, EngineEventArgs e)
        {
            EventHandler? handler = this.VioTick;
            handler?.Invoke(sender, e);
        }

        /// <summary>
        /// Raises the EngineUpdate event.
        /// </summary>
        /// <param name="sender">The sender of the message (userdata).</param>
        /// <param name="e">CommunicationEventArgs.</param>
        protected virtual void OnEngineUpdate(object sender, EngineEventArgs e)
        {
            EventHandler? handler = this.EngineUpdate;
            handler?.Invoke(sender, e);
        }

        /// <summary>
        /// Each tick, restores various attributes of each user.
        /// </summary>
        private void RestoreUsers()
        {
            if (Communicator.Users != null)
            {
                foreach (var user in Communicator.Users)
                {
                    // TODO: If user is resting, or sleeping, restore more/faster.

                    var moveRestore = Math.Min(user.Value.Character.Movement.Max - user.Value.Character.Movement.Current, 20);
                    user.Value.Character.Movement.Current += moveRestore;

                    // This will update on tick, by the communicator.

                    // TODO: Implement spell effects wearing off (e.g. poison)                    
                }
            }
        }
    }
}



