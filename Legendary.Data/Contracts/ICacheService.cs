// <copyright file="ICacheService.cs" company="Legendary™">
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
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Caching.Distributed;

    /// <summary>
    /// Implementation contract for a caching service.
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Gets an item from the cache.
        /// </summary>
        /// <typeparam name="T">The type if item to get.</typeparam>
        /// <param name="key">The item key.</param>
        /// <returns>Task with type.</returns>
        Task<T?> GetFromCache<T>(string key)
            where T : class;

        /// <summary>
        /// Sets an item in the cache.
        /// </summary>
        /// <typeparam name="T">The type of item to cache.</typeparam>
        /// <param name="key">The item key.</param>
        /// <param name="value">The value.</param>
        /// <param name="options">Options.</param>
        /// <returns>Task.</returns>
        Task SetCache<T>(string key, T value, DistributedCacheEntryOptions options)
            where T : class;

        /// <summary>
        /// Removes an item from the cache.
        /// </summary>
        /// <param name="key">The item key.</param>
        /// <returns>Task.</returns>
        Task ClearCache(string key);
    }
}
