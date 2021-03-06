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
    using System.Threading;
    using System.Threading.Tasks;
    using Legendary.Core.Contracts;
    using Legendary.Core.Models;
    using Legendary.Core.Types;
    using Legendary.Data.Contracts;
    using Legendary.Engine.Contracts;
    using Legendary.Engine.Models;
    using MongoDB.Driver;

    /// <summary>
    /// Represents an instance of a world.
    /// </summary>
    public class World : IWorld
    {
        private readonly IRandom random;
        private readonly IDataService dataService;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="World"/> class.
        /// </summary>
        /// <param name="dataService">The areas within the world.</param>
        /// <param name="random">The random generator.</param>
        /// <param name="logger">The logger.</param>
        public World(IDataService dataService, IRandom random, ILogger logger)
        {
            this.dataService = dataService;
            this.logger = logger;
            this.random = random;

            // Cache common lookups as hash sets for faster reads.
            this.Areas = new HashSet<Area>(this.dataService.Areas.Find(a => true).ToList());
            this.Items = new HashSet<Item>(this.dataService.Items.Find(a => true).ToList());
            this.Mobiles = new HashSet<Mobile>(this.dataService.Mobiles.Find(a => true).ToList());
        }

        /// <inheritdoc/>
        public HashSet<Area> Areas { get; private set; }

        /// <inheritdoc/>
        public HashSet<Item> Items { get; private set; }

        /// <inheritdoc/>
        public HashSet<Mobile> Mobiles { get; private set; }

        /// <inheritdoc/>
        public GameMetrics? GameMetrics { get; private set; } = null;

        /// <inheritdoc/>
        public async Task Populate()
        {
            foreach (var area in this.Areas)
            {
                foreach (var room in area.Rooms)
                {
                    // Populate items from resets
                    foreach (var reset in room.ItemResets)
                    {
                        var item = await this.FindItem(f => f.ItemId == reset);
                        if (item != null)
                        {
                            item.Location = new KeyValuePair<long, long>(area.AreaId, room.RoomId);
                            room.Items.Add(item);
                        }
                    }

                    // Populate mobs from resets
                    foreach (var reset in room.MobileResets)
                    {
                        var mobile = await this.FindMobile(f => f.CharacterId == reset);
                        if (mobile != null)
                        {
                            mobile.Location = new KeyValuePair<long, long>(area.AreaId, room.RoomId);
                            room.Mobiles.Add(mobile);
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
        public async Task ProcessWorldChanges(ICommunicator communicator, IRandom random, CancellationToken cancellationToken = default)
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
                        await communicator.SendToRoom(null, location, string.Empty, $"{item.ShortDescription} disintegrates.", cancellationToken);
                    }

                    // Maybe move any wandering mobiles.
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
                                            await communicator.SendToRoom(mobile, mobile.Location, string.Empty, $"{mobile.FirstName} leaves {dir}.", cancellationToken);

                                            // Set the mobile's new location.
                                            mobile.Location = new KeyValuePair<long, long>(exit.ToArea, exit.ToRoom);

                                            // Add the mobile to the new location.
                                            var nextRoom = communicator.ResolveRoom(mobile.Location);
                                            nextRoom.Mobiles.Add(mobile);

                                            await communicator.SendToRoom(mobile, mobile.Location, string.Empty, $"{mobile.FirstName} enters.", cancellationToken);
                                        }
                                    }
                                }
                            }
                        }
                    }
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
    }
}
