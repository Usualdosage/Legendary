// <copyright file="IDBConnection.cs" company="Legendary">
//  Copyright © 2021-2022 Legendary
//  All rights are reserved. Reproduction or transmission in whole or
//  in part, in any form or by any means, electronic, mechanical or
//  otherwise, is prohibited without the prior written consent of
//  the copyright owner.
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
        IMongoDatabase? Database { get; set; }

        /// <summary>
        /// Checks to see if a connection can be made to the specified database.
        /// </summary>
        /// <returns>True if able to connect, else false.</returns>
        bool TestConnection();
    }
}