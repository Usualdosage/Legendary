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

            this.TestConnection();
        }

        /// <summary>
        /// Gets the areas.
        /// </summary>
        public IMongoCollection<Area>? Areas { get => this.dbConnection.Database?.GetCollection<Area>("Areas"); }

        /// <summary>
        /// Gets the characters.
        /// </summary>
        public IMongoCollection<Character>? Characters { get => this.dbConnection.Database?.GetCollection<Character>("Characters"); }

        /// <summary>
        /// Gets the items.
        /// </summary>
        public IMongoCollection<Item>? Items { get => this.dbConnection.Database?.GetCollection<Item>("Items"); }

        /// <summary>
        /// Gets the mobiles.
        /// </summary>
        public IMongoCollection<Mobile>? Mobiles { get => this.dbConnection.Database?.GetCollection<Mobile>("Mobiles"); }

        /// <inheritdoc/>
        public bool TestConnection()
        {
            return this.dbConnection.TestConnection();
        }

        /// <inheritdoc/>
        public async Task<ReplaceOneResult?> SaveCharacter(Character character)
        {
            try
            {
                if (!character.IsNPC)
                {
                    FilterDefinition<Character> charToReplace = new ExpressionFilterDefinition<Character>(d => d.CharacterId == character.CharacterId);
                    if (this.Characters != null)
                    {
                        return await this.Characters.ReplaceOneAsync(charToReplace, character);
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
                return await this.Characters.Find(filter, options)
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
            if (this.Characters != null)
            {
                try
                {
                    await this.Characters.InsertOneAsync(character);
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
        public async Task<Mobile?> CreateMobile(Mobile mobile)
        {
            if (this.Mobiles != null)
            {
                try
                {
                    await this.Mobiles.InsertOneAsync(mobile);
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
            if (this.Items != null)
            {
                try
                {
                    await this.Items.InsertOneAsync(item);
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