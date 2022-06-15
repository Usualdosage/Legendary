// <copyright file="IDataService.cs" company="Legendary">
//  Copyright © 2021 Legendary
//  All rights are reserved. Reproduction or transmission in whole or
//  in part, in any form or by any means, electronic, mechanical or
//  otherwise, is prohibited without the prior written consent of
//  the copyright owner.
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
        public World? LoadWorld();

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
        /// Creates a character.
        /// </summary>
        /// <param name="firstName">First name.</param>
        /// <param name="lastName">Last name.</param>
        /// <param name="hashedPassword">Hashed password.</param>
        /// <returns></returns>
        Task<Character?> CreateCharacter(string firstName, string lastName, string hashedPassword);

        /// <summary>
        /// Tests that this instance can connect to Mongo.
        /// </summary>
        /// <returns>True if succeeded, false if not.</returns>
        bool TestConnection();
    }
}