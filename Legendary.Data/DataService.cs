// <copyright file="DataService.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Data
{
    using System;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Legendary.Core.Contracts;
    using Legendary.Core.Extensions;
    using Legendary.Core.Models;
    using Legendary.Data.Contracts;
    using MongoDB.Bson.IO;
    using MongoDB.Driver;
    using Newtonsoft.Json;

    /// <summary>
    /// Concrete implementation of an IDataService.
    /// </summary>
    public class DataService : IDataService
    {
        private readonly IDBConnection dbConnection;
        private readonly IRandom random;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataService"/> class.
        /// </summary>
        /// <param name="dbConnection">The database connection.</param>
        /// <param name="random">The random number generator.</param>
        public DataService(IDBConnection dbConnection, IRandom random)
        {
            this.dbConnection = dbConnection;
            this.random = random;
        }

        /// <inheritdoc/>
        public bool TestConnection()
        {
            return this.dbConnection.TestConnection();
        }

        /// <inheritdoc/>
        public World LoadWorld()
        {
            if (this.TestConnection())
            {
                var characters = this.dbConnection.Database?.GetCollection<Character>("Characters");
                var areas = this.dbConnection.Database?.GetCollection<Area>("Areas");
                var items = this.dbConnection.Database?.GetCollection<Item>("Items");
                var mobiles = this.dbConnection.Database?.GetCollection<Mobile>("Mobiles");

                if (areas != null && characters != null && items != null && mobiles != null)
                {
                    return new World(areas, characters, items, mobiles, this.random);
                }
                else
                {
                    throw new Exception("Error loading world. Missing at least one collection.");
                }
            }
            else
            {
                throw new Exception("A connection to the database could not be established.");
            }
        }

        /// <inheritdoc/>
        public async Task<ReplaceOneResult?> SaveCharacter(Character character)
        {
            try
            {
                var characters = this.dbConnection.Database?.GetCollection<Character>("Characters");

                if (!character.IsNPC)
                {
                    FilterDefinition<Character> charToReplace = new ExpressionFilterDefinition<Character>(d => d.CharacterId == character.CharacterId);
                    if (characters != null)
                    {
                        return await characters.ReplaceOneAsync(charToReplace, character);
                    }
                    else
                    {
                        throw new Exception("No characters to replace!");
                    }
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Character?> FindCharacter(
            Expression<Func<Character, bool>> filter,
            FindOptions? options = null)
        {
            try
            {
                var characters = this.dbConnection.Database?.GetCollection<Character>("Characters");
                return await characters.Find(filter, options)
                    .FirstOrDefaultAsync();
            }
            catch (Exception exc)
            {
                throw new Exception($"Unable to connect to the database. {exc}");
            }
        }

        /// <inheritdoc/>
        public async Task<Character?> CreateCharacter(Character character)
        {
            var characters = this.dbConnection.Database?.GetCollection<Character>("Characters");

            if (characters != null)
            {
                try
                {
                    await characters.InsertOneAsync(character);
                    return character;
                }
                catch
                {
                    throw;
                }
            }
            else
            {
                throw new Exception("Unable to create character.");
            }
        }

        /// <inheritdoc/>
        public async Task<Character?> CreateMobile(Character mobile)
        {
            var mobiles = this.dbConnection.Database?.GetCollection<Character>("Mobiles");

            if (mobiles != null)
            {
                try
                {
                    await mobiles.InsertOneAsync(mobile);
                    return mobile;
                }
                catch
                {
                    throw;
                }
            }
            else
            {
                throw new Exception("Unable to create mobile.");
            }
        }

        /// <inheritdoc/>
        public async Task<Item?> CreateItem(Item item)
        {
            var items = this.dbConnection.Database?.GetCollection<Item>("Items");

            if (items != null)
            {
                try
                {
                    await items.InsertOneAsync(item);
                    return item;
                }
                catch
                {
                    throw;
                }
            }
            else
            {
                throw new Exception("Unable to create item.");
            }
        }
    }
}