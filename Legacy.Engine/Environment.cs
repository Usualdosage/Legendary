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
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Legendary.Core;
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
            this.ProcessRecovery(this.connectedUser);
            await this.ProcessTime(this.connectedUser, gameHour);
            await this.ProcessWeather(this.connectedUser);
            await this.ProcessMobiles(this.connectedUser);
            await this.ProcessItemRot(this.connectedUser);
        }

        /// <summary>
        /// Restores a portion of the players health/mana/movement each tick.
        /// </summary>
        /// <param name="user">The user.</param>
        private void ProcessRecovery(UserData user)
        {
            int standardHPRecover = Constants.STANDARD_HP_RECOVERY;
            int standardManaRecover = Constants.STANDARD_MANA_RECOVERY;
            int standardMoveRecover = Constants.STANDARD_MOVE_RECOVERY;

            if (user.Character.CharacterFlags.Contains(Core.Types.CharacterFlags.Resting))
            {
                standardHPRecover *= Constants.REST_RECOVERY_MULTIPLIER;
                standardManaRecover *= Constants.REST_RECOVERY_MULTIPLIER;
                standardMoveRecover *= Constants.REST_RECOVERY_MULTIPLIER;
            }
            else if (user.Character.CharacterFlags.Contains(Core.Types.CharacterFlags.Sleeping))
            {
                standardHPRecover *= Constants.SLEEP_RECOVERY_MULTIPLIER;
                standardManaRecover *= Constants.SLEEP_RECOVERY_MULTIPLIER;
                standardMoveRecover *= Constants.SLEEP_RECOVERY_MULTIPLIER;
            }

            var moveRestore = Math.Min(user.Character.Movement.Max - user.Character.Movement.Current, standardMoveRecover);
            user.Character.Movement.Current += moveRestore;

            var manaRestore = Math.Min(user.Character.Mana.Max - user.Character.Mana.Current, standardManaRecover);
            user.Character.Mana.Current += manaRestore;

            var hitRestore = Math.Min(user.Character.Health.Max - user.Character.Health.Current, standardHPRecover);
            user.Character.Health.Current += hitRestore;
        }

        /// <summary>
        /// Iterates over all user items that may decompose and removes them if they decay.
        /// </summary>
        /// <param name="userData">The user.</param>
        /// <returns>Task.</returns>
        private async Task ProcessItemRot(UserData userData)
        {
            // Update items in inventory.
            foreach (var item in userData.Character.Inventory)
            {
                if (item.RotTimer != -1)
                {
                    item.RotTimer -= 1;

                    if (item.RotTimer <= 0)
                    {
                        await this.communicator.SendToPlayer(this.connectedUser.Connection, $"{item.ShortDescription} disintegrates.");
                    }
                }
            }

            this.connectedUser.Character.Inventory.RemoveAll(a => a.RotTimer <= 0);

            // Update items in equipment.
            foreach (var item in userData.Character.Equipment)
            {
                if (item.RotTimer != -1)
                {
                    item.RotTimer -= 1;

                    if (item.RotTimer <= 0)
                    {
                        await this.communicator.SendToPlayer(this.connectedUser.Connection, $"{item.ShortDescription} disintegrates.");
                    }
                }
            }

            this.connectedUser.Character.Equipment.RemoveAll(a => a.RotTimer <= 0);
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
