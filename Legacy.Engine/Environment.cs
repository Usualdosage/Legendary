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
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Legendary.Core;
    using Legendary.Core.Contracts;
    using Legendary.Core.Models;
    using Legendary.Engine.Contracts;
    using Legendary.Engine.Extensions;
    using Legendary.Engine.Helpers;
    using Legendary.Engine.Models;

    /// <summary>
    /// Represents the user environment.
    /// </summary>
    public class Environment : IEnvironment
    {
        private readonly ICommunicator communicator;
        private readonly IRandom random;
        private readonly IWorld world;
        private readonly ILogger logger;
        private int dayWeatherIndex = 0;

        private List<Weather> dayWeatherForward = new List<Weather>()
        {
            new Weather(0, "The sun shines in a nearly cloudless sky.", string.Empty, "It is sunny with a clear, blue sky overhead.", 0),
            new Weather(1, "Some small clouds form in the sky.", string.Empty, "There are some small clouds in the sky, but it is mostly sunny.", 0),
            new Weather(2, "Some thicker clouds begin to form in the sky.", string.Empty, "It is partly cloudy. The sun is partially obscured by the clouds.", 0),
            new Weather(3, "The sky becomes mostly cloudy.", string.Empty, "It is mostly cloudy with more clouds forming on the horizon.", 0),
            new Weather(4, "The cloud cover gets heavier.", string.Empty, "It is gloomy and overcast with some large clouds.", 0),
            new Weather(5, "You hear thunder rumble in the distance.", Sounds.THUNDER, "The sky is thick with clouds. It looks like it's about to storm.", 0),
            new Weather(6, "Dark, heavy clouds fill the sky.", string.Empty, "Heavy, dark clouds fill an ominous sky.", 0),
            new Weather(7, "The wind picks up.", Sounds.WIND, "It is cloudy, but very windy.", 0),
            new Weather(8, "It begins to {precipitate} lightly.", Sounds.LIGHTRAIN, "It is cloudy and raining lightly.", 0),
            new Weather(9, "Lighting flashes in the sky from the heavy clouds.", Sounds.LIGHTNINGBOLT, "It is storming, and you see lightning.", 0),
            new Weather(10, "The wind begins to howl.", Sounds.HEAVYWIND, "The wind is roaring, and the clouds streak across the sky.", 0),
            new Weather(11, "The {precipitate} picks up and starts coming down heavily.", Sounds.RAIN, "Rain falls from heavy clouds above you.", 0),
            new Weather(12, "Lightning flashes all around you while thunder erupts from the skies.", Sounds.HEAVYTHUNDER, "You are in the midsts of a heavy thunderstorm. Lightning is flashing everywhere and thunder rumbles in the distance.", 0),
            new Weather(13, "The {precipitate} is coming down in buckets all around you.", Sounds.HEAVYRAIN, "It's hard to look at the sky as the rain is coming down heavily. Black clouds fill the sky.", 0),
        };

        private List<Weather> dayWeatherBackward = new List<Weather>()
        {
            new Weather(0, "The small clouds dissipate.", string.Empty, "It is sunny with a clear, blue sky overhead.", 0),
            new Weather(1, "The thicker clouds get smaller as the wind blows them away.", string.Empty, "There are some small clouds in the sky, but it is mostly sunny.", 0),
            new Weather(2, "The sky begins to clear.", string.Empty, "It is partly cloudy. The sun is partially obscured by the clouds.", 0),
            new Weather(3, "The heavy cloud cover begins to break up.", string.Empty, "It is mostly cloudy with but appears to be breaking up.", 0),
            new Weather(4, "The thunder fades off in the distance.", Sounds.THUNDERFADE, "The sky is thick with clouds, but the heavy weather seems to be moving away.", 0),
            new Weather(5, "The dark, heavy clouds begin to break up.", string.Empty, "The heavy, dark clouds that filled the sky are breaking up.", 0),
            new Weather(6, "The wind dies down a bit.", Sounds.WINDFADE, "The wind has died down and the clouds are no longer being blown across the sky.", 0),
            new Weather(7, "The light {precipitate} stops.", string.Empty, "It is cloudy but the {precipitate} is slowing.", 0),
            new Weather(8, "The lightning seems to fade off in the distance.", Sounds.THUNDERFADE, "It is storming heavily, but the lightning appears to be easing slightly.", 0),
            new Weather(9, "The wind calms to a dull roar.", Sounds.WIND, "The wind is blowing heavily, moving clouds quickly across the sky.", 0),
            new Weather(10, "The {precipitate} slows to a medium fall.", Sounds.LIGHTRAIN, "There is a medium {precipiate} falling around you.", 0),
            new Weather(11, "The thunder and lighting around you seems to gradually fade out.", Sounds.THUNDERFADE, "A heavy thunderstorm crackles with lightning all around but, but seems to be slowing.", 0),
            new Weather(12, "The {precipitate} stops coming down so heavily.", Sounds.RAIN, "The rain is slowing, but heavy showers are still in the area.", 0),
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="Environment"/> class.
        /// </summary>
        /// <param name="communicator">The communicator.</param>
        /// <param name="random">The random number generator.</param>
        /// <param name="world">The world.</param>
        /// <param name="logger">The logger.</param>
        public Environment(ICommunicator communicator, IRandom random, IWorld world, ILogger logger)
        {
            this.communicator = communicator;
            this.random = random;
            this.world = world;
            this.logger = logger;

            this.dayWeatherIndex = this.random.Next(0, this.dayWeatherForward.Count - 1);
        }

        /// <summary>
        /// Gets or sets the current weather for an area.
        /// </summary>
        public static Dictionary<long, Weather> CurrentWeather { get; set; } = new Dictionary<long, Weather>();

        /// <summary>
        /// Populates the current weather randomly for every area.
        /// </summary>
        public void GenerateWeather()
        {
            var areas = this.world.Areas;

            foreach (var area in areas)
            {
                // Grab the first non-indoor room.
                var defaultRoom = area.Rooms.FirstOrDefault(r => !r.Flags.Contains(Core.Types.RoomFlags.Indoors));

                if (defaultRoom != null)
                {
                    var weatherMessage = this.GenerateRandomWeather(defaultRoom);

                    if (weatherMessage != null)
                    {
                        if (CurrentWeather.ContainsKey(defaultRoom.AreaId))
                        {
                            CurrentWeather[defaultRoom.AreaId] = weatherMessage;
                        }
                        else
                        {
                            CurrentWeather.Add(defaultRoom.AreaId, weatherMessage);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Processes all environment changes that are relevant to the game environment.
        /// </summary>
        /// <param name="gameTicks">The game ticks.</param>
        /// <param name="gameHour">The game hour.</param>
        public void ProcessEnvironmentChanges(int gameTicks, int gameHour)
        {
            if (Communicator.Users != null)
            {
                List<Task> tasks = new List<Task>();

                foreach (var user in Communicator.Users)
                {
                    tasks.Add(this.ProcessRecovery(user.Value));
                    tasks.Add(this.ProcessItemRot(user.Value));
                    tasks.Add(this.ProcessAffects(user.Value));
                }

                tasks.Add(this.ProcessTime(gameHour));
                tasks.Add(this.ProcessMobiles());
                tasks.Add(this.ProcessWeather());

                Task.WaitAll(tasks.ToArray());
            }
        }

        private async Task ProcessMobiles()
        {
            // TODO
        }

        /// <summary>
        /// Recovery is based on the player's vitals, assuming it would take 8 hours of uninterrupted sleep to fully recover.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>Task.</returns>
        private async Task ProcessRecovery(UserData user)
        {
            var standardHPRecover = user.Character.Health.Max / 48;
            var standardManaRecover = user.Character.Mana.Max / 48;
            var standardMoveRecover = user.Character.Movement.Max / 48;

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
            user.Character.Movement.Current += (int)moveRestore;

            var manaRestore = Math.Min(user.Character.Mana.Max - user.Character.Mana.Current, standardManaRecover);
            user.Character.Mana.Current += (int)manaRestore;

            var hitRestore = Math.Min(user.Character.Health.Max - user.Character.Health.Current, standardHPRecover);
            user.Character.Health.Current += (int)hitRestore;

            // TODO Cumulative effects, and add damage as these increase
            if (user.Character.Level < Constants.WIZLEVEL)
            {
                if (user.Character.Hunger.Current < user.Character.Hunger.Max * 4)
                {
                    user.Character.Hunger.Current += 1;
                }
                else
                {
                    // TODO Don't increment, and just apply damage
                    user.Character.Hunger.Current = user.Character.Hunger.Max * 4;
                }

                if (user.Character.Thirst.Current < user.Character.Thirst.Max * 3)
                {
                    user.Character.Thirst.Current += 1;
                }
                else
                {
                    // TODO Don't increment, and just apply damage
                    user.Character.Thirst.Current = user.Character.Thirst.Max * 3;
                }

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
                        if (effect.Name?.ToLower() == "ghost")
                        {
                            await this.communicator.SendToPlayer(user.Connection, $"You are no longer a ghost.");
                        }
                        else
                        {
                            await this.communicator.SendToPlayer(user.Connection, $"The {effect.Name} effect wears off.");
                        }
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
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        private async Task ProcessItemRot(UserData userData, CancellationToken cancellationToken = default)
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
                        await this.communicator.SendToRoom(userData.Character.Location, $"{item.Name.FirstCharToUpper()} disintegrates.", cancellationToken);
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
                        await this.communicator.SendToRoom(userData.Character.Location, $"{item.Name.FirstCharToUpper()} disintegrates.", cancellationToken);
                    }
                }
            }

            userData.Character.Equipment.RemoveAll(i => i.RotTimer == 0);
        }

        private async Task ProcessTime(int gameHour)
        {
            if (Communicator.Users != null)
            {
                foreach (var user in Communicator.Users)
                {
                    var room = this.communicator.ResolveRoom(user.Value.Character.Location);

                    if (room != null && !room.Flags.Contains(Core.Types.RoomFlags.Indoors) && !room.Flags.Contains(Core.Types.RoomFlags.Dark))
                    {
                        if (gameHour == 6)
                        {
                            await this.communicator.SendToPlayer(user.Value.Character, "The sun sets in the west.");
                        }
                        else if (gameHour == 19)
                        {
                            await this.communicator.SendToPlayer(user.Value.Character, "The sun sets in the west.");
                        }
                        else if (gameHour == 21)
                        {
                            await this.communicator.SendToPlayer(user.Value.Character, "The moon rises in east.");
                        }
                        else if (gameHour == 2)
                        {
                            await this.communicator.SendToPlayer(user.Value.Character, "The moon sets in the west.");
                        }
                    }
                }
            }
        }

        private Weather? GenerateRandomWeather(Room room)
        {
            // If they're indoors, don't show the weather.
            if (room == null || room.Flags.Contains(Core.Types.RoomFlags.Indoors))
            {
                return null;
            }
            else
            {
                // Roll a 3 sided die. 0, we increment the weather. 1, we decrement it. 2 - 5, no action.
                var chance = this.random.Next(0, 2);

                switch (chance)
                {
                    case 0:
                        this.dayWeatherIndex += 1;

                        if (this.dayWeatherIndex >= this.dayWeatherForward.Count - 1)
                        {
                            this.dayWeatherIndex = this.dayWeatherForward.Count - 1;
                        }

                        break;
                    case 1:
                        this.dayWeatherIndex -= 1;
                        if (this.dayWeatherIndex < 0)
                        {
                            this.dayWeatherIndex = 0;
                        }

                        break;
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                        break;
                }

                string precipitate = string.Empty;

                switch (room.Terrain)
                {
                    default:
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
                        precipitate = "stardust";
                        break;
                    case Core.Types.Terrain.Mountains:
                    case Core.Types.Terrain.Snow:
                        precipitate = "snow";
                        break;
                }

                Weather weatherMessage = this.dayWeatherForward[0];

                try
                {
                    if (chance == 0)
                    {
                        weatherMessage = this.dayWeatherForward[Math.Min(this.dayWeatherIndex, this.dayWeatherForward.Count - 1)];
                    }
                    else if (chance == 1)
                    {
                        weatherMessage = this.dayWeatherBackward[Math.Min(this.dayWeatherIndex, this.dayWeatherBackward.Count - 1)];
                    }
                }
                catch
                {
                    this.logger.Debug($"Didn't process weather change. Index out of range. Weather index was {this.dayWeatherIndex}.", this.communicator);
                }

                weatherMessage.Message = weatherMessage.Message.Replace("{precipitate}", precipitate);
                weatherMessage.Temp = this.GetRandomTemp(room);

                return weatherMessage;
            }
        }

        /// <summary>
        /// Processes messages about the weather each hour to the user.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        private async Task ProcessWeather(CancellationToken cancellationToken = default)
        {
            if (Communicator.Users != null)
            {
                // Give a 20% chance to display some weather info each tick.
                if (this.random.Next(0, 100) <= 20)
                {
                    var playersInAreaGroup = Communicator.Users.GroupBy(a => a.Value.Character.Location.Key).ToList();

                    foreach (var areaGrouping in playersInAreaGroup)
                    {
                        // Get all the players in room groups.
                        var playersInRoom = areaGrouping.GroupBy(g => g.Value.Character.Location.Value);

                        foreach (var roomGrouping in playersInRoom)
                        {
                            // Get the location of the first player. All the rest in here are in the same area and room.
                            var location = roomGrouping.First().Value.Character.Location;

                            var room = this.communicator.ResolveRoom(location);

                            if (room != null && !room.Flags.Contains(Core.Types.RoomFlags.Indoors) && !room.Flags.Contains(Core.Types.RoomFlags.Dark))
                            {
                                var weatherMessage = this.GenerateRandomWeather(room);

                                if (weatherMessage != null)
                                {
                                    await this.communicator.SendToRoom(location, weatherMessage.Message, cancellationToken);
                                    await this.communicator.PlaySoundToRoom(location, Core.Types.AudioChannel.Weather, weatherMessage.Sound, cancellationToken);

                                    if (CurrentWeather.ContainsKey(room.AreaId))
                                    {
                                        CurrentWeather[room.AreaId] = weatherMessage;
                                    }
                                    else
                                    {
                                        CurrentWeather.Add(room.AreaId, weatherMessage);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets a random temperature based on the room terrain and time of year.
        /// </summary>
        /// <param name="room">The room to base the temp off of.</param>
        /// <returns>int.</returns>
        private int GetRandomTemp(Room room)
        {
            // Average year round temperature in Mystra.
            int baseTemp = 60;

            // This is kind of ballpark, but we want to have cooler temps in the winter than in the summer, so we'll base it off the room and the season.
            var metrics = this.world.GameMetrics;

            if (metrics != null)
            {
                string season = DateTimeHelper.FormatSeason(metrics.CurrentMonth);

                switch (season)
                {
                    case "spring":
                        baseTemp += this.random.Next(3, 6);
                        break;
                    case "summer":
                        baseTemp += this.random.Next(8, 12);
                        break;
                    case "autumn":
                        baseTemp -= this.random.Next(3, 9);
                        break;
                    case "winter":
                        baseTemp -= this.random.Next(11, 16);
                        break;
                }
            }

            switch (room.Terrain)
            {
                case Core.Types.Terrain.Mountains:
                    baseTemp -= this.random.Next(8, 20);
                    break;
                default:
                case Core.Types.Terrain.City:
                    baseTemp += this.random.Next(1, 3);
                    break;
                case Core.Types.Terrain.Water:
                case Core.Types.Terrain.Air:
                    baseTemp -= this.random.Next(8, 20);
                    break;
                case Core.Types.Terrain.Beach:
                    baseTemp += this.random.Next(8, 20);
                    break;
                case Core.Types.Terrain.Desert:
                    baseTemp += this.random.Next(14, 29);
                    break;
                case Core.Types.Terrain.Ethereal:
                    baseTemp -= this.random.Next(50, 60);
                    break;
                case Core.Types.Terrain.Forest:
                    baseTemp += this.random.Next(3, 7);
                    break;
                case Core.Types.Terrain.Hills:
                case Core.Types.Terrain.Grasslands:
                    baseTemp -= this.random.Next(3, 7);
                    break;
                case Core.Types.Terrain.Jungle:
                    baseTemp += this.random.Next(15, 25);
                    break;
                case Core.Types.Terrain.Snow:
                    baseTemp -= this.random.Next(17, 31);
                    break;
                case Core.Types.Terrain.Swamp:
                    baseTemp += this.random.Next(3, 7);
                    break;
            }

            return baseTemp;
        }
    }
}
