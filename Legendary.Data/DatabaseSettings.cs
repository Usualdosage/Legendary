// <copyright file="DatabaseSettings.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Data
{
    using Legendary.Data.Contracts;

    /// <summary>
    /// Database settings class.
    /// </summary>
    public class DatabaseSettings : IDatabaseSettings
    {
        /// <summary>
        /// Gets or sets conversation log collection name.
        /// </summary>
        public string? CollectionName { get; set; }

        /// <summary>
        /// Gets or sets connection string.
        /// </summary>
        public string? ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets database name.
        /// </summary>
        public string? DatabaseName { get; set; }
    }
}