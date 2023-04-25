// <copyright file="CacheService.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Data
{
    using System.Text.Json;
    using System.Threading.Tasks;
    using Legendary.Data.Contracts;
    using Microsoft.Extensions.Caching.Distributed;

    /// <summary>
    /// Concrete implementation of a cache services.
    /// </summary>
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheService"/> class.
        /// </summary>
        /// <param name="cache">The cache.</param>
        public CacheService(IDistributedCache cache)
        {
            this.cache = cache;
        }

        /// <inheritdoc/>
        public async Task<T?> GetFromCache<T>(string key)
            where T : class
        {
            var cachedResponse = await this.cache.GetStringAsync(key);
            return cachedResponse == null ? null : JsonSerializer.Deserialize<T>(cachedResponse);
        }

        /// <inheritdoc/>
        public async Task SetCache<T>(string key, T value, DistributedCacheEntryOptions options)
            where T : class
        {
            var response = JsonSerializer.Serialize(value);
            await this.cache.SetStringAsync(key, response, options);
        }

        /// <inheritdoc/>
        public async Task ClearCache(string key)
        {
            await this.cache.RemoveAsync(key);
        }
    }
}
