// <copyright file="IDBConnection.cs" company="Legendary™">
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
    using MongoDB.Driver;

    /// <summary>
    /// Implementation contract for a database connection.
    /// </summary>
    public interface IDBConnection : IDisposable
    {
        /// <summary>
        /// Gets or sets the database.
        /// </summary>
        IMongoDatabase Database { get; set; }
    }
}