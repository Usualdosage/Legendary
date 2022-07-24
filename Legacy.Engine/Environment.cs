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
    using System.Threading;
    using System.Threading.Tasks;
    using Legendary.Core;
    using Legendary.Core.Contracts;
    using Legendary.Core.Models;
    using Legendary.Engine.Extensions;

    /// <summary>
    /// Represents the user environment.
    /// </summary>
    public class Environment : IEnvironment
    {
        private readonly ICommunicator communicator;
        private readonly IRandom random;
        private readonly Combat combat;
        private readonly UserData connectedUser;

        /// <summary>
        /// Initializes a new instance of the <see cref="Environment"/> class.
        /// </summary>
        /// <param name="communicator">The communicator.</param>
        /// <param name="random">The random number generator.</param>
        /// <param name="connectedUser">The connected user.</param>
        /// <param name="combat">The combat engine.</param>
        public Environment(ICommunicator communicator, IRandom random, UserData connectedUser, Combat combat)
        {
            this.communicator = communicator;
            this.random = random;
            this.connectedUser = connectedUser;
            this.combat = combat;
        }

        /// <summary>
        /// Processes all environment changes that are relevant to the connected user.
        /// </summary>
        /// <param name="gameTicks">The game ticks.</param>
        /// <param name="gameHour">The game hour.</param>
        public void ProcessEnvironmentChanges(int gameTicks, int gameHour)
        {
            List<Task> tasks = new List<Task>();

            tasks.Add(this.ProcessRecovery(this.connectedUser));
            tasks.Add(this.ProcessTime(this.connectedUser, gameHour));
            tasks.Add(this.ProcessWeather(this.connectedUser));
            tasks.Add(this.ProcessMobiles(this.connectedUser));
            tasks.Add(this.ProcessItemRot(this.connectedUser));
            tasks.Add(this.ProcessAffects(this.connectedUser));

            Task.WaitAll(tasks.ToArray());
        }

        private async Task ProcessRecovery(UserData user)
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

            user.Character.Hunger.Current += 1;
            user.Character.Thirst.Current += 1;

            // TODO Cumulative effects, and add damage as these increase.
            if (user.Character.Hunger.Current >= user.Character.Hunger.Max)
            {
                await this.communicator.SendToPlayer(user.Connection, $"You are hungry.");
            }

            if (user.Character.Thirst.Current >= user.Character.Thirst.Max)
            {
                await this.communicator.SendToPlayer(user.Connection, $"You are thirsty.");
            }
        }

        private async Task ProcessAffects(UserData user)
        {
            foreach (var effect in user.Character.AffectedBy)
            {
                if (effect != null)
                {
                    effect.Duration -= 1;

                    if (effect.Duration < 0)
                    {
                        await this.communicator.SendToPlayer(user.Connection, $"The {effect.Name} effect wears off.");
                    }
                    else
                    {
                        if (effect.Effector != null && effect.Action != null)
                        {
                            await effect.Action.OnTick(user.Character, effect);
                        }
                    }
                }
            }

            user.Character.AffectedBy.RemoveAll(e => e.Duration < 0);
        }

        /// <summary>
        /// Iterates over all user items that may decompose and removes them if they decay.
        /// </summary>
        /// <param name="userData">The user.</param>
        /// <returns>Task.</returns>
        private async Task ProcessItemRot(UserData userData)
        {
            foreach (var item in userData.Character.Inventory)
            {
                if (item.RotTimer == -1)
                {
                    continue;
                }
                else
                {
                    item.RotTimer -= 1;

                    if (item.RotTimer == 0)
                    {
                        if (item.ItemType == Core.Types.ItemType.Spring)
                        {
                            await this.communicator.SendToRoom(null, userData.Character.Location, string.Empty, $"{item.Name.FirstCharToUpper()} dries up.");
                        }
                        else
                        {
                            await this.communicator.SendToRoom(null, userData.Character.Location, string.Empty, $"{item.Name.FirstCharToUpper()} disintegrates.");
                        }
                    }
                }
            }

            userData.Character.Inventory.RemoveAll(i => i.RotTimer == 0);

            foreach (var item in userData.Character.Equipment)
            {
                if (item.RotTimer == -1)
                {
                    continue;
                }
                else
                {
                    item.RotTimer -= 1;

                    if (item.RotTimer == 0)
                    {
                        await this.communicator.SendToRoom(null, userData.Character.Location, string.Empty, $"{item.Name.FirstCharToUpper()} disintegrates.");
                    }
                }
            }

            userData.Character.Equipment.RemoveAll(i => i.RotTimer == 0);
        }

        private async Task ProcessMobiles(UserData userData)
        {
            await this.communicator.CheckMobCommunication(userData.Character, userData.Character.Location, "is standing here");
        }

        private async Task ProcessTime(UserData userData, int gameHour)
        {
            var room = this.communicator.ResolveRoom(userData.Character.Location);

            if (gameHour == 6)
            {
                await this.communicator.SendToPlayer(this.connectedUser.Connection, "The sun rises in the east.");
            }
            else if (gameHour == 19)
            {
                await this.communicator.SendToPlayer(this.connectedUser.Connection, "The sun sets in the west.");
            }
        }

        /// <summary>
        /// Processes messages about the weather each hour to the user.
        /// </summary>
        /// <param name="userData">The user.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        private async Task ProcessWeather(UserData userData, CancellationToken cancellationToken = default)
        {
            // TODO: Finish the weather.
            var room = this.communicator.ResolveRoom(userData.Character.Location);

            var weather = this.random.Next(1, 8);

            switch (room.Terrain)
            {
                case Core.Types.Terrain.Air:
                    break;
                case Core.Types.Terrain.Beach:
                    break;
                case Core.Types.Terrain.City:
                    break;
                case Core.Types.Terrain.Desert:
                    break;
                case Core.Types.Terrain.Ethereal:
                    {
                        switch (weather)
                        {
                            default:
                                break;
                            case 1:
                                await this.communicator.SendToPlayer(this.connectedUser.Connection, "The stars in space seem to swirl around.", cancellationToken);
                                await this.communicator.PlaySound(this.connectedUser.Character, Core.Types.AudioChannel.Weather, Sounds.SPACE, cancellationToken);
                                break;
                            case 2:
                                await this.communicator.SendToPlayer(this.connectedUser.Connection, "A comet flies by.", cancellationToken);
                                await this.communicator.PlaySound(this.connectedUser.Character, Core.Types.AudioChannel.Weather, Sounds.SPACE, cancellationToken);
                                break;
                            case 3:
                                await this.communicator.SendToPlayer(this.connectedUser.Connection, "Somewhere in the distance, a star goes supernova.", cancellationToken);
                                await this.communicator.PlaySound(this.connectedUser.Character, Core.Types.AudioChannel.Weather, Sounds.SPACE, cancellationToken);
                                break;
                            case 4:
                                await this.communicator.SendToPlayer(this.connectedUser.Connection, "The bleakness of vast space stretches all around you.", cancellationToken);
                                await this.communicator.PlaySound(this.connectedUser.Character, Core.Types.AudioChannel.Weather, Sounds.SPACE, cancellationToken);
                                break;
                            case 5:
                                await this.communicator.SendToPlayer(this.connectedUser.Connection, "A cloud of primordial dust floats past you.");
                                await this.communicator.PlaySound(this.connectedUser.Character, Core.Types.AudioChannel.Weather, Sounds.SPACE, cancellationToken);
                                break;
                        }

                        break;
                    }

                case Core.Types.Terrain.Forest:
                    break;
                case Core.Types.Terrain.Grasslands:
                    break;
                case Core.Types.Terrain.Hills:
                    break;
                case Core.Types.Terrain.Jungle:
                    break;
                case Core.Types.Terrain.Mountains:
                    break;
                case Core.Types.Terrain.Snow:
                    break;
                case Core.Types.Terrain.Swamp:
                    break;
            }
        }
    }
}
