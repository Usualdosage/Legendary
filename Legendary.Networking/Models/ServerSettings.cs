// <copyright file="ServerSettings.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Networking.Models
{
    using Legendary.Core.Contracts;

    /// <summary>
    /// Used to house server settings loaded from a config file.
    /// </summary>
    public class ServerSettings : IServerSettings
    {
        /// <inheritdoc/>
        public string? ApiUrl { get; set; }

        /// <inheritdoc/>
        public int? ApiPort { get; set; }

        /// <inheritdoc/>
        public string? ChatGPTAPIKey { get; set; }

        /// <inheritdoc/>
        public string? AzureDefaultConnectionString { get; set; }

        /// <inheritdoc/>
        public string? AzureStorageKey { get; set; }

        /// <inheritdoc/>
        public string? FromEmailAddress { get; set; }

        /// <inheritdoc/>
        public string? FromEmailPassword { get; set; }

        /// <inheritdoc/>
        public string? FromEmailName { get; set; }

        /// <inheritdoc/>
        public string? MongoConnectionString { get; set; }

        /// <inheritdoc/>
        public string? MongoDatabaseName { get; set; }
    }
}