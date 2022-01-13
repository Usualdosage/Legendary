// <copyright file="Environment.cs" company="Legendary">
//  Copyright © 2021 Legendary
//  All rights are reserved. Reproduction or transmission in whole or
//  in part, in any form or by any means, electronic, mechanical or
//  otherwise, is prohibited without the prior written consent of
//  the copyright owner.
// </copyright>

namespace Legendary.Engine
{
    using Legendary.Engine.Contracts;

    /// <summary>
    /// Handles environmental information like sunset, weather, etc.
    /// </summary>
    public class Environment
    {
        private readonly ILogger logger;
        private readonly ICommunicator communicator;
        private readonly IRandom random;

        /// <summary>
        /// Initializes a new instance of the <see cref="Environment"/> class.
        /// </summary>
        /// <param name="logger">The ILogger.</param>
        /// <param name="communicator">The ICommunicator.</param>
        /// <param name="random">The random number generator.</param>
        public Environment(ILogger logger, ICommunicator communicator, IRandom random)
        {
            this.logger = logger;
            this.communicator = communicator;
            this.random = random;
        }

        /// <summary>
        /// Sends messages about environmental changes to the users (each "hour").
        /// </summary>
        /// <param name="gameHour">The current game hour.</param>
        public void ProcessChanges(int gameHour)
        {
            if (gameHour == 6)
            {
                this.communicator.SendGlobal("The sun rises in the east.");
            }
            else if (gameHour == 18)
            {
                this.communicator.SendGlobal("The sun sets in the west.");
            }

            var weather = this.random.Next(1, 10);

            switch (weather)
            {
                default:
                    break;
                case 1:
                    this.communicator.SendGlobal("The stars in space seem to swirl around.");
                    break;
                case 2:
                    this.communicator.SendGlobal("A comet flies by.");
                    break;
                case 3:
                    this.communicator.SendGlobal("Somewhere in the distance, a star goes supernova.");
                    break;
                case 4:
                    this.communicator.SendGlobal("The bleakness of vast space stretches all around you.");
                    break;
                case 5:
                    this.communicator.SendGlobal("A cloud of primordial dust floats past you.");
                    break;
            }
        }
    }
}



