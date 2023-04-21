// <copyright file="IServerSettings.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Core.Contracts
{
    /// <summary>
    /// Implementation contract for server settings.
    /// </summary>
    public interface IServerSettings
    {
        /// <summary>
        /// Gets or sets the URL of the content API.
        /// </summary>
        string? ApiUrl { get; set; }

        /// <summary>
        /// Gets or sets the port of the content API.
        /// </summary>
        int? ApiPort { get; set; }

        /// <summary>
        /// Gets or sets the API key for Chat GPT.
        /// </summary>
        string? ChatGPTAPIKey { get; set; }

        /// <summary>
        /// Gets or sets the default connection string for Azure storage.
        /// </summary>
        string? AzureDefaultConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the key for Azure storage retrieval.
        /// </summary>
        string? AzureStorageKey { get; set; }

        /// <summary>
        /// Gets or sets the from email address.
        /// </summary>
        string? FromEmailAddress { get; set; }

        /// <summary>
        /// Gets or sets the from email password.
        /// </summary>
        string? FromEmailPassword { get; set; }

        /// <summary>
        /// Gets or sets the from email username.
        /// </summary>
        string? FromEmailName { get; set; }

        /// <summary>
        /// Gets or sets the Mongo connection string.
        /// </summary>
        string? MongoConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the Mongo database name.
        /// </summary>
        string? MongoDatabaseName { get; set; }

        /// <summary>
        /// Gets or sets the connection string to the Redis cache.
        /// </summary>
        string? RedisCacheConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the instancee of the Redis cache.
        /// </summary>
        string? RedisCacheInstance { get; set; }
    }
}