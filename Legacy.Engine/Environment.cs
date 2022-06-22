// <copyright file="Environment.cs" company="Legendary">
//  Copyright © 2021-2022 Legendary
//  All rights are reserved. Reproduction or transmission in whole or
//  in part, in any form or by any means, electronic, mechanical or
//  otherwise, is prohibited without the prior written consent of
//  the copyright owner.
// </copyright>

namespace Legendary.Engine
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Legendary.Engine.Contracts;
    using Legendary.Engine.Models;

    public class Environment : IEnvironment
	{
		private readonly ICommunicator communicator;
        private readonly IRandom random;
        private readonly KeyValuePair<string, UserData> connectedUser;

        public Environment(ICommunicator communicator, IRandom random, KeyValuePair<string, UserData> connectedUser)
		{
			this.communicator = communicator;
            this.random = random;
            this.connectedUser = connectedUser;
		}

        /// <summary>
        /// Processes all environment changes that are relevant to the connected user.
        /// </summary>
        /// <returns></returns>
        public async Task ProcessEnvironmentChanges(int gameTicks, int gameHour)
        {
            await ProcessTime(gameHour);
            await ProcessWeather();
        }

        private async Task ProcessTime(int gameHour)
        {
            if (gameHour == 6)
            {
                await this.communicator.SendToPlayer(connectedUser.Value.Connection, "The sun rises in the east.");
            }
            else if (gameHour == 19)
            {
                await this.communicator.SendToPlayer(connectedUser.Value.Connection, "The sun sets in the west.");
            }
            else
            {
                return;
            }
        }

        /// <summary>
        /// Processes messages about the weather each hour to the user.
        /// </summary>
        /// <returns></returns>
        private async Task ProcessWeather()
        {
            // TODO: Check what sort of room/area the player is in, and generate weather based on that.

            var weather = this.random.Next(1, 8);

            switch (weather)
            {
                default:
                    break;
                case 1:
                    await this.communicator.SendToPlayer(connectedUser.Value.Connection, "The stars in space seem to swirl around.");
                    break;
                case 2:
                    await this.communicator.SendToPlayer(connectedUser.Value.Connection, "A comet flies by.");
                    break;
                case 3:
                    await this.communicator.SendToPlayer(connectedUser.Value.Connection, "Somewhere in the distance, a star goes supernova.");
                    break;
                case 4:
                    await this.communicator.SendToPlayer(connectedUser.Value.Connection, "The bleakness of vast space stretches all around you.");
                    break;
                case 5:
                    await this.communicator.SendToPlayer(connectedUser.Value.Connection, "A cloud of primordial dust floats past you.");
                    break;
            }
        }
    }
}

