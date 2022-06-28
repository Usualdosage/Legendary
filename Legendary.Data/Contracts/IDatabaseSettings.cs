// <copyright file="IDatabaseSettings.cs" company="Legendary™">
//  Copyright ©2021-2022 Legendary and Matthew Martin (Crypticant).
//  Use, reuse, and/or modification of this software requires
//  adherence to the included license file at
//  https://github.com/Usualdosage/Legendary.
//  Registered work by https://www.thelegendarygame.com.
//  This header must remain on all derived works.
// </copyright>

namespace Legendary.Data.Contracts
{
    /// <summary>
    /// Database settings interface to support configurability and multi-tenant db use.
    /// </summary>
    public interface IDatabaseSettings
    {
        /// <summary>
        /// Gets or sets name for the collection.
        /// </summary>
        string? CollectionName { get; set; }

        /// <summary>
        /// Gets or sets database connection string.
        /// </summary>
        string? ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets database Name.
        /// </summary>
        string? DatabaseName { get; set; }
    }
}