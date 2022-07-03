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
    using Legendary.Core.Models;
    using Legendary.Data.Contracts;
    using MongoDB.Driver;

    /// <summary>
    /// Concrete implementation of an IDataService.
    /// </summary>
    public class DataService : IDataService
    {
        private readonly IDBConnection dbConnection;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataService"/> class.
        /// </summary>
        /// <param name="dbConnection">The database connection.</param>
        public DataService(IDBConnection dbConnection)
        {
            this.dbConnection = dbConnection;
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
                    return new World(areas, characters, items, mobiles);
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
        public async Task<ReplaceOneResult> SaveCharacter(Character character)
        {
            var characters = this.dbConnection.Database?.GetCollection<Character>("Characters");
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
    }
}