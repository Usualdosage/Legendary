// <copyright file="World.cs" company="Legendary">
//  Copyright © 2021 Legendary
//  All rights are reserved. Reproduction or transmission in whole or
//  in part, in any form or by any means, electronic, mechanical or
//  otherwise, is prohibited without the prior written consent of
//  the copyright owner.
// </copyright>

namespace Legendary.Core.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Legendary.Core.Contracts;
    using MongoDB.Driver;

    /// <summary>
    /// Represents an instance of a world.
    /// </summary>
    public class World : IWorld
    {
        private readonly IMongoCollection<Area> areas;
        private readonly IMongoCollection<Character> characters;
        private readonly IMongoCollection<Item> items;
        private readonly IMongoCollection<Mobile> mobiles;
        private readonly IApiClient apiClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="World"/> class.
        /// </summary>
        /// <param name="areas">The areas within the world.</param>
        /// <param name="characters">The characters.</param>
        /// <param name="items">The items.</param>
        /// <param name="apiClient">The API client.</param>
        public World(IMongoCollection<Area> areas, IMongoCollection<Character> characters, IMongoCollection<Item> items, IMongoCollection<Mobile> mobiles, IApiClient apiClient)
        {
            this.areas = areas;
            this.characters = characters;
            this.items = items;
            this.mobiles = mobiles;
            this.apiClient = apiClient;

            var temp = areas.Find(a => a.AreaId == 1).ToList();

            this.Areas = new HashSet<Area>(this.GetAllAreas());
            this.Items = new HashSet<Item>(this.GetAllItems());
            this.Mobiles = new HashSet<Mobile>(this.GetAllMobiles());
        }

        /// <summary>
        /// Gets the time the world was created.
        /// </summary>
        public static DateTime StartupDateTime
        {
            get
            {
                return DateTime.UtcNow;
            }
        }

        /// <inheritdoc/>
        public HashSet<Area> Areas { get; private set; }

        /// <inheritdoc/>
        public HashSet<Item> Items { get; private set; }

        /// <inheritdoc/>
        public HashSet<Mobile> Mobiles { get; private set; }

        /// <inheritdoc/>
        public void Dispose()
        {
        }

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
                            room.Items.Add(item);
                        }
                    }

                    // Populate mobs from resets
                    foreach (var reset in room.MobileResets)
                    {
                        var mobile = await this.FindMobile(f => f.MobileId == reset);
                        if (mobile != null)
                        {
                            room.Mobiles.Add(mobile);
                        }
                    }
                }
            }
        }

        /// <inheritdoc/>
        public async Task<Area?> FindArea(
            Expression<Func<Area, bool>> filter,
            FindOptions? options = null)
        {
            return await this.areas.Find(filter, options)
                .FirstOrDefaultAsync();
        }

        /// <inheritdoc/>
        public async Task<Character?> FindCharacter(
            Expression<Func<Character, bool>> filter,
            FindOptions? options = null)
        {
            return await this.characters.Find(filter, options)
                .FirstOrDefaultAsync();
        }

        /// <inheritdoc/>
        public async Task<Item?> FindItem(
            Expression<Func<Item, bool>> filter,
            FindOptions? options = null)
        {
            return await this.items.Find(filter, options)
                .FirstOrDefaultAsync();
        }

        /// <inheritdoc/>
        public async Task<Mobile?> FindMobile(
            Expression<Func<Mobile, bool>> filter,
            FindOptions? options = null)
        {
            return await this.mobiles.Find(filter, options)
                .FirstOrDefaultAsync();
        }

        /// <inheritdoc/>
        public List<Area> GetAllAreas()
        {
            return this.areas.Find(a => true).ToList();
        }

        /// <inheritdoc/>
        public List<Character> GetAllCharacters()
        {
            return this.characters.Find(c => true).ToList();
        }

        /// <inheritdoc/>
        public List<Item> GetAllItems()
        {
            return this.items.Find(i => true).ToList();
        }

        /// <inheritdoc/>
        public List<Mobile> GetAllMobiles()
        {
            return this.mobiles.Find(i => true).ToList();
        }

        /// <inheritdoc/>
        public void InsertOneArea(
            Area document,
            InsertOneOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            this.areas.InsertOne(document, options, cancellationToken);
        }

        /// <inheritdoc/>
        public void InsertOneCharacter(
            Character document,
            InsertOneOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            this.characters.InsertOne(document, options, cancellationToken);
        }

        /// <inheritdoc/>
        public void InsertOneItem(
            Item document,
            InsertOneOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            this.items.InsertOne(document, options, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<ReplaceOneResult> ReplaceOneAreaAsync(
            Expression<Func<Area, bool>> filter,
            Area replacement,
            ReplaceOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            return await this.areas.ReplaceOneAsync(
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
            return await this.characters.ReplaceOneAsync(
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
            return await this.items.ReplaceOneAsync(
                filter,
                replacement,
                options,
                cancellationToken);
        }

        /// <inheritdoc/>
        public async Task SaveAllCharacters()
        {
            var allCharacters = this.GetAllCharacters();
            foreach (var character in allCharacters)
            {
                await this.ReplaceOneCharacterAsync(c => c.CharacterId == character.CharacterId, character);
            }
        }

        /// <summary>
        /// Reload the in-memory collections from the database.
        /// </summary>
        public void Reload()
        {
            this.Areas = new HashSet<Area>(this.areas.Find(l => true).ToList());
            this.Items = new HashSet<Item>(this.items.Find(l => true).ToList());
        }

        /// <inheritdoc/>
        public async Task LoadRoomImages()
        {
            foreach (var area in this.Areas)
            {
                foreach (var room in area.Rooms)
                {
                    var base64 = await this.apiClient.GetRawContent($"room?roomId={room.RoomId}");
                    room.Image = base64;
                }
            }
        }
    }
}



