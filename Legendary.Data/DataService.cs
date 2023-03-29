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
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Legendary.Core.Contracts;
    using Legendary.Core.Models;
    using Legendary.Data.Contracts;
    using MongoDB.Driver;

    /// <summary>
    /// Concrete implementation of an IDataService.
    /// </summary>
    public class DataService : IDataService
    {
        private readonly IDBConnection dbConnection;

        private readonly ReplaceOptions replaceOptions = new () { IsUpsert = true };
        private readonly InsertOneOptions insertOptions = new () { Comment = "Insert options." };

        /// <summary>
        /// Initializes a new instance of the <see cref="DataService"/> class.
        /// </summary>
        /// <param name="dbConnection">The database connection.</param>
        public DataService(IDBConnection dbConnection)
        {
            this.dbConnection = dbConnection;
        }

        /// <summary>
        /// Gets the areas.
        /// </summary>
        public IMongoCollection<Area> Areas { get => this.dbConnection.Database.GetCollection<Area>("Areas"); }

        /// <summary>
        /// Gets the characters.
        /// </summary>
        public IMongoCollection<Character> Characters { get => this.dbConnection.Database.GetCollection<Character>("Characters"); }

        /// <summary>
        /// Gets the items.
        /// </summary>
        public IMongoCollection<Item> Items { get => this.dbConnection.Database.GetCollection<Item>("Items"); }

        /// <summary>
        /// Gets the mobiles.
        /// </summary>
        public IMongoCollection<Mobile> Mobiles { get => this.dbConnection.Database.GetCollection<Mobile>("Mobiles"); }

        /// <summary>
        /// Gets the awards.
        /// </summary>
        public IMongoCollection<Award> Awards { get => this.dbConnection.Database.GetCollection<Award>("Awards"); }

        /// <summary>
        /// Gets the game metrics.
        /// </summary>
        public IMongoCollection<GameMetrics> GameMetrics { get => this.dbConnection.Database.GetCollection<GameMetrics>("GameMetrics"); }

        /// <summary>
        /// Gets the messages.
        /// </summary>
        public IMongoCollection<Message> Messages { get => this.dbConnection.Database.GetCollection<Message>("Messages"); }

        /// <inheritdoc/>
        public async Task<ReplaceOneResult?> SaveCharacter(Character character, CancellationToken cancellationToken)
        {
            try
            {
                if (!character.IsNPC)
                {
                    FilterDefinition<Character> charToReplace = new ExpressionFilterDefinition<Character>(d => d.CharacterId == character.CharacterId);

                    if (this.Characters != null)
                    {
                        return await this.Characters.ReplaceOneAsync(charToReplace, character, this.replaceOptions, cancellationToken);
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
                throw new Exception($"Unable to load character. {exc}");
            }
        }

        /// <inheritdoc/>
        public async Task<Character?> FindMobile(
            Expression<Func<Mobile, bool>> filter,
            FindOptions? options = null)
        {
            try
            {
                return await this.Mobiles.Find(filter, options)
                    .FirstOrDefaultAsync();
            }
            catch (Exception exc)
            {
                throw new Exception($"Unable to load mobile. {exc}");
            }
        }

        /// <inheritdoc/>
        public async Task<GameMetrics?> GetGameMetrics()
        {
            try
            {
                return await this.GameMetrics.Find(f => f.Id == 1).FirstOrDefaultAsync();
            }
            catch (Exception exc)
            {
                throw new Exception($"Unable to load game metrics. {exc}");
            }
        }

        /// <inheritdoc/>
        public async Task<Character?> CreateCharacter(Character character, CancellationToken cancellationToken)
        {
            if (this.Characters != null)
            {
                try
                {
                    await this.Characters.InsertOneAsync(character, this.insertOptions, cancellationToken);
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
        public async Task<Mobile?> CreateMobile(Mobile mobile, CancellationToken cancellationToken)
        {
            if (this.Mobiles != null)
            {
                try
                {
                    await this.Mobiles.InsertOneAsync(mobile, this.insertOptions, cancellationToken);
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
        public async Task<Item?> CreateItem(Item item, CancellationToken cancellationToken)
        {
            if (this.Items != null)
            {
                try
                {
                    await this.Items.InsertOneAsync(item, this.insertOptions, cancellationToken);
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

        /// <inheritdoc/>
        public async Task<GameMetrics?> SaveGameMetrics(GameMetrics metrics, CancellationToken cancellationToken)
        {
            if (this.GameMetrics != null)
            {
                try
                {
                    FilterDefinition<GameMetrics> metricsToReplace = new ExpressionFilterDefinition<GameMetrics>(c => c.Id == 1);

                    await this.GameMetrics.ReplaceOneAsync(metricsToReplace, metrics, this.replaceOptions, cancellationToken);

                    return metrics;
                }
                catch
                {
                    throw;
                }
            }
            else
            {
                throw new Exception("Unable to update game metrics.");
            }
        }
    }
}