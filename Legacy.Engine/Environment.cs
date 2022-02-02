// <copyright file="Environment.cs" company="Legendary">
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
    using Legendary.Engine.Contracts;

    /// <summary>
    /// Handles environmental information like sunset, weather, etc.
    /// </summary>
    public class Environment
    {
        private readonly ILogger logger;
        private readonly ICommunicator communicator;
        private readonly IProcessor processor;
        private readonly IRandom random;

        /// <summary>
        /// Initializes a new instance of the <see cref="Environment"/> class.
        /// </summary>
        /// <param name="logger">The ILogger.</param>
        /// <param name="communicator">The ICommunicator.</param>
        /// <param name="processor">The IProcessor.</param>
        /// <param name="random">The random number generator.</param>
        public Environment(ILogger logger, ICommunicator communicator, IProcessor processor, IRandom random)
        {
            this.logger = logger;
            this.communicator = communicator;
            this.random = random;
            this.processor = processor;
        }

        /// <summary>
        /// Sends messages about environmental changes to the users (each "hour"). Restores players each tick.
        /// </summary>
        /// <param name="gameHour">The current game hour.</param>
        public async Task ProcessChanges(int gameHour)
        {
            await this.RestoreUsers();

            if (gameHour == 6)
            {
                await this.communicator.SendGlobal("The sun rises in the east.");
            }
            else if (gameHour == 18)
            {
                await this.communicator.SendGlobal("The sun sets in the west.");
            }

            var weather = this.random.Next(1, 5);

            switch (weather)
            {
                default:
                    break;
                case 1:
                    await this.communicator.SendGlobal("The stars in space seem to swirl around.");
                    break;
                case 2:
                    await this.communicator.SendGlobal("A comet flies by.");
                    break;
                case 3:
                    await this.communicator.SendGlobal("Somewhere in the distance, a star goes supernova.");
                    break;
                case 4:
                    await this.communicator.SendGlobal("The bleakness of vast space stretches all around you.");
                    break;
                case 5:
                    await this.communicator.SendGlobal("A cloud of primordial dust floats past you.");
                    break;
            }
        }

        /// <summary>
        /// Each tick, restores various attributes of each user.
        /// </summary>
        private async Task RestoreUsers()
        {
            if (Communicator.Users != null)
            {
                foreach (var user in Communicator.Users)
                {
                    // TODO: If user is resting, or sleeping, restore more/faster

                    var moveRestore = Math.Min(user.Value.Character.Movement.Max - user.Value.Character.Movement.Current, 20);
                    user.Value.Character.Movement.Current += moveRestore;

                    // TODO: Implement spell effects wearing off (e.g. poison)

                    // Update the player info box
                    await this.processor.ShowPlayerInfo(user.Value);
                }
            }
        }
    }
}



