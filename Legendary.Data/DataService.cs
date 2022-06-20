// <copyright file="DataService.cs" company="Legendary">
//  Copyright © 2021 Legendary
//  All rights are reserved. Reproduction or transmission in whole or
//  in part, in any form or by any means, electronic, mechanical or
//  otherwise, is prohibited without the prior written consent of
//  the copyright owner.
// </copyright>

namespace Legendary.Data
{
    using System;
    using Legendary.Core.Contracts;
    using Legendary.Core.Models;
    using Legendary.Data.Contracts;
    using MongoDB.Driver;
    using System.Threading.Tasks;
    using System.Linq.Expressions;

    /// <summary>
    /// Concrete implementation of an IDataService.
    /// </summary>
    public class DataService : IDataService
    {
        private readonly IDBConnection dbConnection;
        private readonly IApiClient apiClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataService"/> class.
        /// </summary>
        /// <param name="dbConnection">The database connection.</param>
        /// <param name="apiClient">The API client.</param>
        public DataService(IDBConnection dbConnection, IApiClient apiClient)
        {
            this.apiClient = apiClient;
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
                    return new World(areas, characters, items, mobiles, this.apiClient);
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
        public async Task<Character?> CreateCharacter(string firstName, string lastName, string hashedPassword)
        {
            var characters = this.dbConnection.Database?.GetCollection<Character>("Characters");

            var character = new Character()
            {
                FirstName = firstName,
                LastName = lastName,
                Password = hashedPassword,
                Title = "The Tourist",
                Health = new Core.Types.MaxCurrent(30, 30),
                Mana = new Core.Types.MaxCurrent(30, 30),
                Movement = new Core.Types.MaxCurrent(30, 30),
                IsNPC = false,
                Level = 1,
                Location = Room.Default
            };

            if (characters != null)
            {
                await characters.InsertOneAsync(character);
                return character;
            }
            else
            {
                throw new Exception("Unable to create character.");
            }
        }

    }
}