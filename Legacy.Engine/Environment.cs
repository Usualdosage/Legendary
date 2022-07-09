// <copyright file="Environment.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Engine
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Legendary.Core.Contracts;
    using Legendary.Core.Models;
    using Legendary.Engine.Contracts;

    /// <summary>
    /// Represents the user environment.
    /// </summary>
    public class Environment : IEnvironment
    {
        private readonly ICommunicator communicator;
        private readonly IRandom random;
        private readonly UserData connectedUser;

        /// <summary>
        /// Initializes a new instance of the <see cref="Environment"/> class.
        /// </summary>
        /// <param name="communicator">The communicator.</param>
        /// <param name="random">The random number generator.</param>
        /// <param name="connectedUser">The connected user.</param>
        public Environment(ICommunicator communicator, IRandom random, UserData connectedUser)
        {
            this.communicator = communicator;
            this.random = random;
            this.connectedUser = connectedUser;
        }

        /// <summary>
        /// Processes all environment changes that are relevant to the connected user.
        /// </summary>
        /// <param name="gameTicks">The game ticks.</param>
        /// <param name="gameHour">The game hour.</param>
        /// <returns>Task.</returns>
        public async Task ProcessEnvironmentChanges(int gameTicks, int gameHour)
        {
            await this.ProcessTime(this.connectedUser, gameHour);
            await this.ProcessWeather(this.connectedUser);
            await this.ProcessMobiles(this.connectedUser);
        }

        private async Task ProcessMobiles(UserData userData)
        {
            await this.communicator.CheckMobCommunication(userData.Character, userData.Character.Location, "stands here");
        }

        private async Task ProcessTime(UserData userData, int gameHour)
        {
            // TODO: Is the player inside?
            var room = userData.Character.Location;

            if (gameHour == 6)
            {
                await this.communicator.SendToPlayer(this.connectedUser.Connection, "The sun rises in the east.");
            }
            else if (gameHour == 19)
            {
                await this.communicator.SendToPlayer(this.connectedUser.Connection, "The sun sets in the west.");
            }
            else
            {
                return;
            }
        }

        /// <summary>
        /// Processes messages about the weather each hour to the user.
        /// </summary>
        /// <returns>Task.</returns>
        private async Task ProcessWeather(UserData userData)
        {
            var room = userData.Character.Location;

            // TODO: Fix room flags, then check to see if the weather can be seen. Based on terrain.
            var weather = this.random.Next(1, 8);

            switch (weather)
            {
                default:
                    break;
                case 1:
                    await this.communicator.SendToPlayer(this.connectedUser.Connection, "The stars in space seem to swirl around.");
                    break;
                case 2:
                    await this.communicator.SendToPlayer(this.connectedUser.Connection, "A comet flies by.");
                    break;
                case 3:
                    await this.communicator.SendToPlayer(this.connectedUser.Connection, "Somewhere in the distance, a star goes supernova.");
                    break;
                case 4:
                    await this.communicator.SendToPlayer(this.connectedUser.Connection, "The bleakness of vast space stretches all around you.");
                    break;
                case 5:
                    await this.communicator.SendToPlayer(this.connectedUser.Connection, "A cloud of primordial dust floats past you.");
                    break;
            }
        }
    }
}
