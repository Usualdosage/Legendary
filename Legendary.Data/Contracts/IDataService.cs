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
    using System.Threading.Tasks;
    using Legendary.Core.Models;
    using MongoDB.Driver;

    /// <summary>
    /// Implementation contract for interacting with a database.
    /// </summary>
    public interface IDataService
    {
        /// <summary>
        /// Loads the world into memory.
        /// </summary>
        /// <returns>The current world.</returns>
        public World LoadWorld();

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
        /// Saves the character to the database.
        /// </summary>
        /// <param name="character">The chanracter.</param>
        /// <returns>ReplaceOneResult.</returns>
        public Task<ReplaceOneResult?> SaveCharacter(Character character);

        /// <summary>
        /// Creates a character.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <returns>Task.</returns>
        Task<Character?> CreateCharacter(Character character);

        /// <summary>
        /// Creates an item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>Task.</returns>
        Task<Item?> CreateItem(Item item);

        /// <summary>
        /// Creates a mobile.
        /// </summary>
        /// <param name="mobile">The mobile.</param>
        /// <returns>Task.</returns>
        Task<Character?> CreateMobile(Character mobile);

        /// <summary>
        /// Tests that this instance can connect to Mongo.
        /// </summary>
        /// <returns>True if succeeded, false if not.</returns>
        bool TestConnection();
    }
}