// <copyright file="IWorld.cs" company="Legendary">
//  Copyright © 2021-2022 Legendary
//  All rights are reserved. Reproduction or transmission in whole or
//  in part, in any form or by any means, electronic, mechanical or
//  otherwise, is prohibited without the prior written consent of
//  the copyright owner.
// </copyright>

namespace Legendary.Core.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Legendary.Core.Models;
    using MongoDB.Driver;

    /// <summary>
    /// Represents the world, as defined by its sub objects.
    /// </summary>
    public interface IWorld : IDisposable
    {
        /// <summary>
        /// Gets a hashset of areas currently loaded into memory.
        /// </summary>
        HashSet<Area> Areas { get; }

        /// <summary>
        /// Gets a hashset of items currently loaded into memory.
        /// </summary>
        HashSet<Item> Items { get; }

        /// <summary>
        /// Gets a hashset of mobiles currently loaded into memory.
        /// </summary>
        HashSet<Mobile> Mobiles { get; }

        /// <summary>
        /// Calls the IMongoCollection Find extension method.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <param name="options">The options.</param>
        /// <returns>The existing area or null.</returns>
        Task<Area?> FindArea(
            Expression<Func<Area, bool>> filter,
            FindOptions? options = null);

        /// <summary>
        /// Retrieves all documents in the collection.
        /// </summary>
        /// <returns>List of areas.</returns>
        List<Area> GetAllAreas();

        /// <summary>
        /// Retrieves all documents in the collection.
        /// </summary>
        /// <returns>List of characters.</returns>
        List<Character> GetAllCharacters();

        /// <summary>
        /// Retrieves all documents in the collection.
        /// </summary>
        /// <returns>List of items.</returns>
        List<Item> GetAllItems();

        /// <summary>
        /// Retrieves all documents in the collection.
        /// </summary>
        /// <returns>List of mobiles.</returns>
        List<Mobile> GetAllMobiles();

        /// <summary>
        /// Finds a character by the filter.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <param name="options">The options.</param>
        /// <returns>The existing character or null.</returns>
        Task<Character?> FindCharacter(
            Expression<Func<Character, bool>> filter,
            FindOptions? options = null);

        /// <summary>
        /// Finds a item by the filter.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <param name="options">The options.</param>
        /// <returns>The existing character or null.</returns>
        Task<Item?> FindItem(
            Expression<Func<Item, bool>> filter,
            FindOptions? options = null);

        /// <summary>
        /// Inserts a single document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        void InsertOneArea(
            Area document,
            InsertOneOptions? options = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Inserts a single document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        void InsertOneCharacter(
            Character document,
            InsertOneOptions? options = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Inserts a single document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        void InsertOneItem(
            Item document,
            InsertOneOptions? options = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Replaces a single document.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <param name="replacement">The replacement.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result of the replacement.</returns>
        Task<ReplaceOneResult> ReplaceOneAreaAsync(
            Expression<Func<Area, bool>> filter,
            Area replacement,
            ReplaceOptions? options = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Replaces a single document.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <param name="replacement">The replacement.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result of the replacement.</returns>
        Task<ReplaceOneResult> ReplaceOneCharacterAsync(
            Expression<Func<Character, bool>> filter,
            Character replacement,
            ReplaceOptions? options = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Replaces a single document.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <param name="replacement">The replacement.</param>
        /// <param name="options">The options.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result of the replacement.</returns>
        Task<ReplaceOneResult> ReplaceOneItemAsync(
            Expression<Func<Item, bool>> filter,
            Item replacement,
            ReplaceOptions? options = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Saves all character changes to the database globally.
        /// </summary>
        /// <returns>Task.</returns>
        Task SaveAllCharacters();

        /// <summary>
        /// Reload the in-memory collections from the database.
        /// </summary>
        void Reload();

        /// <summary>
        /// Loads items and mobs based on resets.
        /// </summary>
        /// <returns>Task.</returns>
        Task Populate();

        /// <summary>
        /// Preloads all of the room images once.
        /// </summary>
        /// <returns>Task.</returns>
        Task LoadRoomImages();
    }
}



