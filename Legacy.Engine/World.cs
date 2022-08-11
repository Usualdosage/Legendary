// <copyright file="World.cs" company="Legendary™">
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
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Legendary.Core.Contracts;
    using Legendary.Core.Models;
    using Legendary.Core.Types;
    using Legendary.Data.Contracts;
    using Legendary.Engine.Contracts;
    using Legendary.Engine.Extensions;
    using MongoDB.Driver;

    /// <summary>
    /// Represents an instance of a world.
    /// </summary>
    public class World : IWorld
    {
        private readonly IRandom random;
        private readonly IDataService dataService;
        private readonly ILogger logger;
        private readonly ICommunicator communicator;

        /// <summary>
        /// Initializes a new instance of the <see cref="World"/> class.
        /// </summary>
        /// <param name="dataService">The areas within the world.</param>
        /// <param name="random">The random generator.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="communicator">The communicator.</param>
        public World(IDataService dataService, IRandom random, ILogger logger, ICommunicator communicator)
        {
            this.dataService = dataService;
            this.logger = logger;
            this.random = random;
            this.communicator = communicator;
        }

        /// <inheritdoc/>
        public HashSet<Area> Areas { get; private set; } = new HashSet<Area>();

        /// <inheritdoc/>
        public HashSet<Item> Items { get; private set; } = new HashSet<Item>();

        /// <inheritdoc/>
        public HashSet<Mobile> Mobiles { get; private set; } = new HashSet<Mobile>();

        /// <inheritdoc/>
        public GameMetrics? GameMetrics { get; internal set; } = null;

        /// <inheritdoc/>
        public async Task LoadWorld()
        {
            var areas = await this.dataService.Areas.Find(Builders<Area>.Filter.Empty).ToListAsync();
            var items = await this.dataService.Items.Find(Builders<Item>.Filter.Empty).ToListAsync();
            var mobiles = await this.dataService.Mobiles.Find(Builders<Mobile>.Filter.Empty).ToListAsync();

            // Cache common lookups as hash sets for faster reads.
            this.Areas = new HashSet<Area>(areas);
            this.Items = new HashSet<Item>(items);
            this.Mobiles = new HashSet<Mobile>(mobiles);
        }

        /// <inheritdoc/>
        public void Populate()
        {
            foreach (var area in this.Areas)
            {
                foreach (var room in area.Rooms)
                {
                    // Populate items from resets
                    foreach (var reset in room.ItemResets)
                    {
                        var item = this.Items.FirstOrDefault(i => i.ItemId == reset);

                        if (item != null)
                        {
                            var clone = item.DeepCopy();
                            clone.Location = new KeyValuePair<long, long>(area.AreaId, room.RoomId);

                            if (clone.ItemResets != null)
                            {
                                foreach (var itemReset in clone.ItemResets)
                                {
                                    var itemInItem = this.Items.FirstOrDefault(i => i.ItemId == itemReset);

                                    if (itemInItem != null)
                                    {
                                        var itemInItemClone = itemInItem.DeepCopy();

                                        if (clone.Contains == null)
                                        {
                                            clone.Contains = new List<IItem>();
                                        }

                                        clone.Contains.Add(itemInItemClone);
                                    }
                                }
                            }

                            room.Items.Add(clone);
                        }
                    }

                    // Populate mobs from resets
                    foreach (var reset in room.MobileResets)
                    {
                        var mobile = this.Mobiles.FirstOrDefault(m => m.CharacterId == reset);

                        if (mobile != null)
                        {
                            var clone = mobile.DeepCopy();
                            clone.Location = new KeyValuePair<long, long>(area.AreaId, room.RoomId);

                            // Add items to mobs.
                            foreach (var itemReset in clone.EquipmentResets)
                            {
                                var item = this.Items.FirstOrDefault(i => i.ItemId == itemReset.ItemId);

                                if (item != null)
                                {
                                    var itemClone = item.DeepCopy();
                                    clone.Equipment.Add(itemClone);
                                }
                            }

                            room.Mobiles.Add(clone);
                        }
                    }
                }
            }
        }

        /// <inheritdoc/>
        public async Task CleanupWorld(CancellationToken cancellationToken = default)
        {
            try
            {
                await Parallel.ForEachAsync(this.Areas, async (area, cancellationToken) =>
                {
                    await Parallel.ForEachAsync(area.Rooms, async (room, cancellationToken) =>
                    {
                        await Task.Run(
                            () =>
                            {
                                room.Mobiles.RemoveAll(m => m.Location.Value != room.RoomId);
                                room.Items.RemoveAll(i => i.RotTimer == 0);
                            }, cancellationToken);
                    });
                });
            }
            catch (Exception exc)
            {
                this.logger.Error(exc, null);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task UpdateGameMetrics(Exception? lastException, CancellationToken cancellationToken)
        {
            try
            {
                var metrics = await this.dataService.GetGameMetrics();

                if (metrics == null)
                {
                    metrics = new GameMetrics();
                }

                // Update hour, day, month, and year.
                metrics.CurrentHour++;

                if (metrics.CurrentHour == 24)
                {
                    metrics.CurrentHour = 0;
                    metrics.CurrentDay++;

                    if (metrics.CurrentDay >= 31)
                    {
                        metrics.CurrentDay = 1;
                        metrics.CurrentMonth++;

                        if (metrics.CurrentMonth >= 13)
                        {
                            metrics.CurrentMonth = 1;
                            metrics.CurrentYear++;
                        }
                    }
                }

                metrics.HostURL = "https://legendary-mud.azurewebsites.net";
                metrics.LastError = lastException;
                metrics.LastStartupDateTime ??= DateTime.UtcNow;
                metrics.MaxPlayers = this.dataService.Characters.CountDocuments(c => c.CharacterId > 0, cancellationToken: cancellationToken);
                metrics.TotalAreas = this.Areas.Count;
                metrics.TotalMobiles = this.Mobiles.Count;
                metrics.TotalItems = this.Items.Count;
                metrics.TotalRooms = this.Areas.Sum(a => a.Rooms.Count);

                // Update the local cached version.
                this.GameMetrics = metrics;

                // TODO
                // metrics.MostKills = this.dataService.Characters.Find(c => c.Metrics != null && c.Metrics.PlayerKills > 0).SortByDescending(s => s.Metrics.PlayerKills).FirstOrDefault(cancellationToken: cancellationToken).FirstName;
                await this.dataService.SaveGameMetrics(metrics, cancellationToken);
            }
            catch
            {
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task ProcessWorldChanges(CancellationToken cancellationToken = default)
        {
            foreach (var area in this.Areas)
            {
                foreach (var room in area.Rooms)
                {
                    // Decompose items.
                    var items = room.Items.Where(i => i.RotTimer == 0);

                    var location = new KeyValuePair<long, long>(area.AreaId, room.RoomId);

                    foreach (var item in items)
                    {
                        await this.communicator.SendToRoom(null, location, string.Empty, $"{item.ShortDescription} disintegrates.", cancellationToken);
                    }

                    // Apply affects to mobiles.
                    await this.ProcessMobileAffects(room);

                    // Move mobiles who wander.
                    await this.ProcessMobileWander(room, cancellationToken);
                }
            }

            // Tidy everything up after movement, decomposition, etc.
            await this.CleanupWorld(cancellationToken);
       }

        /// <inheritdoc/>
        public async Task<Area?> FindArea(
            Expression<Func<Area, bool>> filter,
            FindOptions? options = null)
        {
            return await this.dataService.Areas.Find(filter, options)
                .FirstOrDefaultAsync();
        }

        /// <inheritdoc/>
        public async Task<Character?> FindCharacter(
            Expression<Func<Character, bool>> filter,
            FindOptions? options = null)
        {
            return await this.dataService.Characters.Find(filter, options)
                .FirstOrDefaultAsync();
        }

        /// <inheritdoc/>
        public async Task<Item?> FindItem(
            Expression<Func<Item, bool>> filter,
            FindOptions? options = null)
        {
            return await this.dataService.Items.Find(filter, options)
                .FirstOrDefaultAsync();
        }

        /// <inheritdoc/>
        public async Task<Mobile?> FindMobile(
            Expression<Func<Mobile, bool>> filter,
            FindOptions? options = null)
        {
            return await this.dataService.Mobiles.Find(filter, options)
                .FirstOrDefaultAsync();
        }

        /// <inheritdoc/>
        public List<Area> GetAllAreas()
        {
            return this.dataService.Areas.Find(a => true).ToList();
        }

        /// <inheritdoc/>
        public List<Character> GetAllCharacters()
        {
            return this.dataService.Characters.Find(c => true).ToList();
        }

        /// <inheritdoc/>
        public List<Item> GetAllItems()
        {
            return this.dataService.Items.Find(i => true).ToList();
        }

        /// <inheritdoc/>
        public List<Mobile> GetAllMobiles()
        {
            return this.dataService.Mobiles.Find(i => true).ToList();
        }

        /// <inheritdoc/>
        public void InsertOneArea(
            Area document,
            InsertOneOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            this.dataService.Areas?.InsertOne(document, options, cancellationToken);
        }

        /// <inheritdoc/>
        public void InsertOneCharacter(
            Character document,
            InsertOneOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            this.dataService.Characters?.InsertOne(document, options, cancellationToken);
        }

        /// <inheritdoc/>
        public void InsertOneItem(
            Item document,
            InsertOneOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            this.dataService.Items?.InsertOne(document, options, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<ReplaceOneResult> ReplaceOneAreaAsync(
            Expression<Func<Area, bool>> filter,
            Area replacement,
            ReplaceOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            return await this.dataService.Areas.ReplaceOneAsync(
                filter,
                replacement,
                options,
                cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<ReplaceOneResult> ReplaceOneCharacterAsync(
            Expression<Func<Character, bool>> filter,
            Character replacement,
            ReplaceOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            return await this.dataService.Characters.ReplaceOneAsync(
                filter,
                replacement,
                options,
                cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<ReplaceOneResult> ReplaceOneItemAsync(
            Expression<Func<Item, bool>> filter,
            Item replacement,
            ReplaceOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            return await this.dataService.Items.ReplaceOneAsync(
                filter,
                replacement,
                options,
                cancellationToken);
        }

        private async Task ProcessMobileAffects(Room room)
        {
            // Process effects on mobiles, and (maybe) move them if they are wandering.
            foreach (var mobile in room.Mobiles)
            {
                // Check effects.
                if (mobile.AffectedBy.Count > 0)
                {
                    foreach (var effect in mobile.AffectedBy)
                    {
                        if (effect != null)
                        {
                            effect.Duration -= 1;

                            if (effect.Effector != null && effect.Action != null)
                            {
                                await effect.Action.OnTick(mobile, effect);
                            }
                        }
                    }

                    mobile.AffectedBy.RemoveAll(e => e.Duration < 0);
                }
            }
        }

        private async Task ProcessMobileWander(Room room, CancellationToken cancellationToken)
        {
            // Process effects on mobiles, and (maybe) move them if they are wandering.
            foreach (var mobile in room.Mobiles)
            {
                if (mobile.MobileFlags != null && mobile.MobileFlags.Contains(MobileFlags.Wander))
                {
                    if (!mobile.CharacterFlags.Contains(CharacterFlags.Fighting) && !mobile.CharacterFlags.Contains(CharacterFlags.Charmed))
                    {
                        if (mobile.MobileFlags.Any(a => a == MobileFlags.Wander))
                        {
                            // Mobiles have a 50% chance each tick to move around.
                            var move = this.random.Next(0, 100);

                            if (move <= 50)
                            {
                                var randomExitNumber = this.random.Next(0, room.Exits.Count);

                                var exit = room.Exits[randomExitNumber];

                                var newArea = await this.FindArea(a => a.AreaId == exit.ToArea);
                                var newRoom = newArea?.Rooms?.FirstOrDefault(r => r.RoomId == exit.ToRoom);

                                if (newArea != null && newRoom != null)
                                {
                                    string? dir = Enum.GetName(typeof(Direction), exit.Direction)?.ToLower();
                                    await this.communicator.SendToRoom(mobile, mobile.Location, string.Empty, $"{mobile.FirstName.FirstCharToUpper()} leaves {dir}.", cancellationToken);

                                    // Set the mobile's new location.
                                    mobile.Location = new KeyValuePair<long, long>(exit.ToArea, exit.ToRoom);

                                    // Add the mobile to the new location.
                                    var nextRoom = this.communicator.ResolveRoom(mobile.Location);

                                    if (nextRoom != null)
                                    {
                                        nextRoom.Mobiles.Add(mobile);
                                        await this.communicator.SendToRoom(mobile, mobile.Location, string.Empty, $"{mobile.FirstName.FirstCharToUpper()} enters.", cancellationToken);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
