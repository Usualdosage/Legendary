// <copyright file="IDataService.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Data.Contracts
{
    using System;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Legendary.Core.Models;
    using MongoDB.Driver;

    /// <summary>
    /// Implementation contract for interacting with a database.
    /// </summary>
    public interface IDataService
    {
        /// <summary>
        /// Gets the areas.
        /// </summary>
        public IMongoCollection<Area> Areas { get; }

        /// <summary>
        /// Gets the items.
        /// </summary>
        public IMongoCollection<Item> Items { get; }

        /// <summary>
        /// Gets the mobiles.
        /// </summary>
        public IMongoCollection<Mobile> Mobiles { get; }

        /// <summary>
        /// Gets the characters.
        /// </summary>
        public IMongoCollection<Character> Characters { get; }

        /// <summary>
        /// Gets the awards.
        /// </summary>
        public IMongoCollection<Award> Awards { get; }

        /// <summary>
        /// Finds a character using the given filter.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <param name="options">The find options.</param>
        /// <returns>Character.</returns>
        public Task<Character?> FindCharacter(
            Expression<Func<Character, bool>> filter,
            FindOptions? options = null);

        /// <summary>
        /// Finds a mobile using the given filter.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <param name="options">The find options.</param>
        /// <returns>Mobile.</returns>
        public Task<Character?> FindMobile(
            Expression<Func<Mobile, bool>> filter,
            FindOptions? options = null);

        /// <summary>
        /// Gets the GameMetrics object.
        /// </summary>
        /// <returns>Task.</returns>
        public Task<GameMetrics?> GetGameMetrics();

        /// <summary>
        /// Saves the character to the database.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>ReplaceOneResult.</returns>
        public Task<ReplaceOneResult?> SaveCharacter(Character character, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a character.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        Task<Character?> CreateCharacter(Character character, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates an item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        Task<Item?> CreateItem(Item item, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a mobile.
        /// </summary>
        /// <param name="mobile">The mobile.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        Task<Mobile?> CreateMobile(Mobile mobile, CancellationToken cancellationToken = default);

        /// <summary>
        /// Saves the game metrics.
        /// </summary>
        /// <param name="metrics">The metrics.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        Task<GameMetrics?> SaveGameMetrics(GameMetrics metrics, CancellationToken cancellationToken = default);
    }
}