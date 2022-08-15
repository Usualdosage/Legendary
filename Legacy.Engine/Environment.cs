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
    using Legendary.Engine.Models;

    /// <summary>
    /// Represents the user environment.
    /// </summary>
    public class Environment : IEnvironment
    {
        private readonly ICommunicator communicator;
        private readonly IRandom random;
        private readonly Combat combat;
        private readonly UserData connectedUser;
        private int dayWeatherIndex = 0;

        private List<Weather> dayWeatherForward = new List<Weather>()
        {
            new Weather(0, "The sun shines in a nearly cloudless sky.", string.Empty),
            new Weather(1, "Some small clouds form in the sky.", string.Empty),
            new Weather(2, "Some thicker clouds begin to form in the sky.", string.Empty),
            new Weather(3, "The sky becomes mostly cloudy.", string.Empty),
            new Weather(4, "The cloud cover gets heavier.", string.Empty),
            new Weather(5, "You hear thunder rumble in the distance.", Sounds.THUNDER),
            new Weather(6, "Dark, heavy clouds fill the sky.", string.Empty),
            new Weather(7, "The wind picks up.", Sounds.WIND),
            new Weather(8, "It begins to {precipitate} lightly.", Sounds.LIGHTRAIN),
            new Weather(9, "Lighting flashes in the sky from the heavy clouds.", Sounds.LIGHTNINGBOLT),
            new Weather(10, "The wind begins to howl.", Sounds.HEAVYWIND),
            new Weather(11, "The {precipitate} picks up and starts coming down heavily.", Sounds.RAIN),
            new Weather(12, "Lightning flashes all around you while thunder erupts from the skies.", Sounds.HEAVYTHUNDER),
            new Weather(13, "The {precipitate} is coming down in buckets all around you.", Sounds.HEAVYRAIN),
        };

        private List<Weather> dayWeatherBackward = new List<Weather>()
        {
            new Weather(0, "The small clouds dissipate.", string.Empty),
            new Weather(1, "The thicker clouds get smaller as the wind blows them away.", string.Empty),
            new Weather(2, "The sky begins to clear.", string.Empty),
            new Weather(3, "The heavy cloud cover begins to break up.", string.Empty),
            new Weather(4, "The thunder fades off in the distance.", Sounds.THUNDERFADE),
            new Weather(5, "The dark, heavy clouds begin to break up.", string.Empty),
            new Weather(6, "The wind dies down a bit.", Sounds.WINDFADE),
            new Weather(7, "The light {precipitate} stops.", string.Empty),
            new Weather(8, "The lightning seems to fade off in the distance.", Sounds.THUNDERFADE),
            new Weather(9, "The wind calms to a dull roar.", Sounds.WIND),
            new Weather(10, "The {precipitate} slows to a medium fall.", Sounds.LIGHTRAIN),
            new Weather(11, "The thunder and lighting around you seems to gradually fade out.", Sounds.THUNDERFADE),
            new Weather(12, "The {precipitate} stops coming down so heavily.", Sounds.RAIN),
        };

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

            this.dayWeatherIndex = this.random.Next(0, this.dayWeatherForward.Count - 1);
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

            // TODO Cumulative effects, and add damage as these increase.
            if (user.Character.Level < Constants.WIZLEVEL)
            {
                user.Character.Hunger.Current += 1;
                user.Character.Thirst.Current += 1;

                if (user.Character.Hunger.Current >= user.Character.Hunger.Max)
                {
                    await this.communicator.SendToPlayer(user.Connection, $"You are hungry.");
                }

                if (user.Character.Thirst.Current >= user.Character.Thirst.Max)
                {
                    await this.communicator.SendToPlayer(user.Connection, $"You are thirsty.");
                }
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

            if (room == null || room.Flags.Contains(Core.Types.RoomFlags.Indoors))
            {
                return;
            }

            if (gameHour == 6)
            {
                await this.communicator.SendToPlayer(this.connectedUser.Connection, "The sun rises in the east.");
            }
            else if (gameHour == 19)
            {
                await this.communicator.SendToPlayer(this.connectedUser.Connection, "The sun sets in the west.");
            }
            else if (gameHour == 21)
            {
                await this.communicator.SendToPlayer(this.connectedUser.Connection, "The moon rises in east.");
            }
            else if (gameHour == 2)
            {
                await this.communicator.SendToPlayer(this.connectedUser.Connection, "The moon sets in the west.");
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
            // Give a 30% chance to display some weather info each tick.
            if (this.random.Next(0, 100) <= 30)
            {
                var room = this.communicator.ResolveRoom(userData.Character.Location);

                if (room == null || room.Flags.Contains(Core.Types.RoomFlags.Indoors))
                {
                    return;
                }

                // Roll a 3 sided die. 0, we increment the weather. 1, we decrement it. 2, no action.
                var chance = this.random.Next(0, 2);

                switch (chance)
                {
                    case 0:
                        this.dayWeatherIndex += 1;
                        break;
                    case 1:
                        this.dayWeatherIndex -= 1;
                        break;
                    case 2:
                        break;
                }

                var weather = this.random.Next(1, 8);
                string precipitate = string.Empty;

                switch (room.Terrain)
                {
                    case Core.Types.Terrain.Forest:
                    case Core.Types.Terrain.Grasslands:
                    case Core.Types.Terrain.Hills:
                    case Core.Types.Terrain.Jungle:
                    case Core.Types.Terrain.Air:
                    case Core.Types.Terrain.Beach:
                    case Core.Types.Terrain.City:
                    case Core.Types.Terrain.Swamp:
                        precipitate = "rain";
                        break;
                    case Core.Types.Terrain.Desert:
                        precipitate = "virga";
                        break;
                    case Core.Types.Terrain.Ethereal:
                        {
                            precipitate = "stardust";
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

                    case Core.Types.Terrain.Mountains:
                    case Core.Types.Terrain.Snow:
                        precipitate = "snow";
                        break;
                }

                if (chance == 0)
                {
                    var weatherMessage = this.dayWeatherForward[Math.Min(this.dayWeatherIndex, this.dayWeatherForward.Count - 1)];
                    await this.communicator.SendToPlayer(this.connectedUser.Connection, weatherMessage.Message.Replace("{precipitate}", precipitate), cancellationToken);
                    await this.communicator.PlaySound(this.connectedUser.Character, Core.Types.AudioChannel.Weather, weatherMessage.Sound, cancellationToken);
                }
                else if (chance == 1)
                {
                    var weatherMessage = this.dayWeatherBackward[Math.Min(this.dayWeatherIndex, this.dayWeatherBackward.Count - 1)];
                    await this.communicator.SendToPlayer(this.connectedUser.Connection, weatherMessage.Message.Replace("{precipitate}", precipitate), cancellationToken);
                    await this.communicator.PlaySound(this.connectedUser.Character, Core.Types.AudioChannel.Weather, weatherMessage.Sound, cancellationToken);
                }
            }
        }
    }
}
